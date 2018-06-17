module DslCompletion

open DslAst
open Errors
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

let checkSuggestion suggestion writtenText = 
    if System.String.IsNullOrWhiteSpace(writtenText) then true
    else
        let threshold = 0.70
        let distance = TextDistance.jaroWinklerDistance writtenText suggestion

        if distance >= threshold then true
        else false

let suggestBoolFlag pos term = 
    match term with
        | BoolFlagMissingPart.EqualBetween(startPos, endPos)      when less startPos pos && less pos endPos -> SuggestionText.equal
        | BoolFlagMissingPart.EqualAfter(errPos)                  when lessEq errPos pos          -> SuggestionText.equal
        | BoolFlagMissingPart.Value(errPos, _)                    when lessEq errPos pos          -> SuggestionText.boolValue
        | BoolFlagMissingPart.Value(errPos, startPos)             when pos <=> (startPos, errPos) -> SuggestionText.None
        | BoolFlagMissingPart.BoolTerm(errPos)                    when less pos errPos            -> SuggestionText.boolTerm
        | BoolFlagMissingPart.IncompleteValue(posRange, errToken) when pos <=> posRange           -> List.where (fun s -> checkSuggestion s errToken) SuggestionText.boolValueStr |> Suggestion.Many
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
    | Module(_, _, (_, endPos))            when less endPos pos     -> SuggestionText.allBody
    | ClassError(ArrowBetween(posRange))   when pos <=> posRange    -> SuggestionText.arrow
    | ClassError(ArrowAfter(errPos))       when lessEq errPos pos   -> SuggestionText.arrow
    | ClassError(OrphanArrow(startPos, _)) when lessEq pos startPos -> SuggestionText.depName
    | ClassError(OrphanArrow(_, endPos))   when lessEq endPos pos   -> SuggestionText.implName
    | ClassError(DepName(startPos, _))     when lessEq pos startPos -> SuggestionText.depName
    | ClassError(DepName(_, endPos))       when lessEq endPos pos   -> SuggestionText.allBody
    | ClassError(ImplName(errPos))         when lessEq errPos pos   -> SuggestionText.implName
    | ModuleError(Name(errPos))            when less errPos pos     -> SuggestionText.moduleName
    | _ -> SuggestionText.None

let suggestTestDeclaration pos = function
    | Full(_, _, (_, endPos))            when less endPos pos   -> SuggestionText.allBody
    | Partial(_, endPos)                 when less endPos pos   -> SuggestionText.testName
    | TestDeclaration.Error((startPos, _), _) when less pos startPos  -> SuggestionText.testHeader
    | TestDeclaration.Error((_, endPos), errToken) when pos == endPos && checkSuggestion SuggestionText.testHeaderStr errToken -> SuggestionText.testHeader
    | _ -> SuggestionText.None

let rec suggestForErrorDeclaration pos = function
    | (errPos, term)::_ when pos <=> errPos -> List.where (fun s -> checkSuggestion s term) SuggestionText.allBodyStr |> Suggestion.Many
    | _::t -> suggestForErrorDeclaration pos t
    | []   -> SuggestionText.None

let suggestUsing pos = function
    | Orphan(startPos, _) 
    | Fqn(_, (startPos, _)) 
    | Iqn(_, (startPos, _)) when less pos startPos -> SuggestionText.using
    | Fqn(_, (_, endPos)) 
    | Iqn(_, (_, endPos))   when less endPos pos   -> SuggestionText.testHeader
    | _ -> SuggestionText.None

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

let suggestFrom fileName src pos = 
    Parser.index.Clear()
    Parser.testIndex.Clear()
    //let testContent = File.ReadAllText fileName
    let lexbuf = LexBuffer<char>.FromString src
    setInitialPos lexbuf fileName
    let errLogger = ErrorLogger()
    Lexer.errorLogger <- errLogger
    Parser.errorLogger <- errLogger
    let ast = (lex, lexbuf) ||> start 
    let index = Parser.testIndex
    
    let suggestion = 
        match index.Find(pos) with
            | CaretTermPosition.Inside(RegistrationTerm(t))    -> suggestRegistration pos t
            | CaretTermPosition.Inside(BoolFlag1Term(t))       -> suggestBoolFlag1 pos t
            | CaretTermPosition.Inside(BoolFlag2Term(t))       -> suggestBoolFlag2 pos t
            | CaretTermPosition.Inside(TestDeclarationTerm(t)) -> suggestTestDeclaration pos t
            | CaretTermPosition.Inside(UsingTerm(t))           -> suggestUsing pos t
            | CaretTermPosition.Inside(Error(_, errors))       -> suggestForErrorDeclaration pos errors
            | CaretTermPosition.Between(firstTerm, secondTerm) -> suggestBetween pos firstTerm secondTerm

    match suggestion with 
        | One(s)  -> [| s |]
        | Many(s) -> s |> Array.ofList
        | None    -> Array.empty 
        
        
    //match suggestion with 
    //    | One(s)  -> if checkSuggestion s writtenText then [| s |] else List.empty |> Array.ofList
    //    | Many(s) -> List.where (fun str -> checkSuggestion str writtenText) s |> Array.ofList
    //    | None    -> Array.empty 
