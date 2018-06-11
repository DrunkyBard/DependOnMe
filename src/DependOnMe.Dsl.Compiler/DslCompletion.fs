module DslCompletion

open DslAst
open TextDistance
open Lexer
open Parser
open Microsoft.FSharp.Text.Lexing
open System.IO
open TextUtilities
open DataStructures
open Navigation
open Positioning
open SuggestionText

let suggestBoolFlag pos term = 
    match term with
        | BoolFlagMissingPart.EqualBetween(startPos, endPos) when less startPos pos && less pos endPos -> SuggestionText.equal
        | BoolFlagMissingPart.Value(errPos)          when less errPos pos     -> SuggestionText.boolValue
        | BoolFlagMissingPart.BoolTerm(errPos)       when less pos errPos     -> SuggestionText.boolTerm
        | _ -> SuggestionText.allBody

let suggestBoolFlag1 pos = function
    | BoolFlag1.Flag(_, _, _, (_, endPos)) when less endPos pos -> SuggestionText.allBody 
    | BoolFlag1.Error(err) -> suggestBoolFlag pos err
    | _ -> SuggestionText.None

let suggestBoolFlag2 pos = function
    | BoolFlag2.Flag(_, _, _, (_, endPos)) when less endPos pos -> SuggestionText.allBody 
    | BoolFlag2.Error(err) -> suggestBoolFlag pos err
    | _ -> SuggestionText.None

let suggestRegistration pos = function
    | Class(_, _, _, _, (_, endPos)) 
    | Module(_, _, (_, endPos))    when less endPos pos     -> SuggestionText.allBody
    | ClassError(Arrow(posRange))  when pos <=> posRange -> SuggestionText.arrow
    | ClassError(DepName(errPos))  when lessEq pos errPos    -> SuggestionText.depName
    | ClassError(ImplName(errPos)) when lessEq errPos pos    -> SuggestionText.implName
    | ModuleError(Name(errPos))    when less errPos pos     -> SuggestionText.moduleName
    | _ -> SuggestionText.None

let suggestTestDeclaration pos = function
    | Full(_, _, (_, endPos))            when less endPos pos   -> SuggestionText.allBody
    | Partial(_, endPos)                 when less endPos pos   -> SuggestionText.testName
    | TestDeclaration.Error(startPos, _) when less pos startPos -> SuggestionText.testHeader
    | _ -> SuggestionText.None

let suggest pos = function
    | RegistrationTerm(term)    -> suggestRegistration pos term
    | BoolFlag1Term(term)       -> suggestBoolFlag1 pos term
    | BoolFlag2Term(term)       -> suggestBoolFlag2 pos term
    | TestDeclarationTerm(term) -> suggestTestDeclaration pos term

let suggestBetween pos firstTerm secondTerm = 
    match firstTerm, secondTerm with
        | RegistrationTerm(ClassError(_) as t),   _ 
        | RegistrationTerm(ModuleError(_) as t),  _  -> suggestRegistration pos t
        | BoolFlag1Term(BoolFlag1.Error(_) as t), _  -> suggestBoolFlag1 pos t
        | BoolFlag2Term(BoolFlag2.Error(_) as t), _  -> suggestBoolFlag2 pos t
        | TestDeclarationTerm(Partial(_) as t),   _ 
        | TestDeclarationTerm(TestDeclaration.Error(_) as t), _ -> suggestTestDeclaration pos t
        | _, RegistrationTerm(ClassError(_) as t)
        | _, RegistrationTerm(ModuleError(_) as t)   -> suggestRegistration pos t
        | _, BoolFlag1Term(BoolFlag1.Error(_) as t)  -> suggestBoolFlag1 pos t
        | _, BoolFlag2Term(BoolFlag2.Error(_) as t)  -> suggestBoolFlag2 pos t
        | _, TestDeclarationTerm(Partial(_) as t)
        | _, TestDeclarationTerm(TestDeclaration.Error(_) as t) -> suggestTestDeclaration pos t
        | _, _ -> SuggestionText.allBody

let checkSuggestion suggestion writtenText = 
    if System.String.IsNullOrWhiteSpace(writtenText) then true
    else
        let threshold = 0.70
        let distance = TextDistance.jaroWinklerDistance writtenText suggestion

        if distance >= threshold then true
        else false

let suggestFrom fileName writtenText pos = 
    Parser.index.Clear()
    Parser.testIndex.Clear()
    let testContent = File.ReadAllText fileName
    let lexbuf = LexBuffer<char>.FromString testContent
    setInitialPos lexbuf fileName
    let ast = (lex, lexbuf) ||> start 
    let index = Parser.testIndex
    
    let suggestion = match index.Find(pos) with
        | CaretTermPosition.Inside(RegistrationTerm(t))    -> suggestRegistration pos t
        | CaretTermPosition.Inside(BoolFlag1Term(t))       -> suggestBoolFlag1 pos t
        | CaretTermPosition.Inside(BoolFlag2Term(t))       -> suggestBoolFlag2 pos t
        | CaretTermPosition.Inside(TestDeclarationTerm(t)) -> suggestTestDeclaration pos t
        | CaretTermPosition.Between(firstTerm, secondTerm) -> suggestBetween pos firstTerm secondTerm

    match suggestion with 
        | One(s)  -> if checkSuggestion s writtenText then [| s |] else List.empty |> Array.ofList
        | Many(s) -> List.where (fun str -> checkSuggestion str writtenText) s |> Array.ofList
        | None    -> Array.empty 

    

    //match index.Find(pos) with
    //    | None -> None
    //    | Some(term)  -> 
    //        let threshold = 0.70
    //        let file = "TestDslFile.drt"
    //        let testContent = File.ReadAllText file
    //        let lexbuf = LexBuffer<char>.FromString testContent
    //        setInitialPos lexbuf file
    //        //let lexems = readLexems lexbuf
    //        let ast = (lex, lexbuf) ||> start 
    //        let logger = Parser.errorLogger
    //        Some 1

