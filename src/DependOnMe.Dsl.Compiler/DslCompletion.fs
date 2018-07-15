module DslCompletion

open TestDslAst
open CommonDslAst
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
    | BoolFlag1Term.Flag(_, _, _, (_, endPos), _) when less endPos pos -> SuggestionText.allBody 
    | BoolFlag1Term.Error(err) -> suggestBoolFlag pos err
    | _ -> SuggestionText.None

let suggestBoolFlag2 pos = function
    | BoolFlag2Term.Flag(_, _, _, (_, endPos), _) when less endPos pos -> SuggestionText.allBody 
    | BoolFlag2Term.Error(err) -> suggestBoolFlag pos err
    | _ -> SuggestionText.None

let suggestRegistration pos = function
    | RegistrationTerm.Class(_, _, _, _, (_, endPos)) 
    | RegistrationTerm.Module(_, _, (_, endPos))            when less endPos pos     -> SuggestionText.allBody
    | RegistrationTerm.ClassError(ArrowBetween(posRange))   when pos <=> posRange    -> SuggestionText.arrow
    | RegistrationTerm.ClassError(ArrowAfter(errPos))       when lessEq errPos pos   -> SuggestionText.arrow
    | RegistrationTerm.ClassError(OrphanArrow(startPos, _)) when lessEq pos startPos -> SuggestionText.depName
    | RegistrationTerm.ClassError(OrphanArrow(_, endPos))   when lessEq endPos pos   -> SuggestionText.implName
    | RegistrationTerm.ClassError(DepName(startPos, _))     when lessEq pos startPos -> SuggestionText.depName
    | RegistrationTerm.ClassError(DepName(_, endPos))       when lessEq endPos pos   -> SuggestionText.allBody
    | RegistrationTerm.ClassError(ImplName(errPos))         when lessEq errPos pos   -> SuggestionText.implName
    | RegistrationTerm.ModuleError(Name(errPos))            when less errPos pos     -> SuggestionText.moduleName
    | _ -> SuggestionText.None

let suggestTestDeclaration pos = function
    | Full(_, _, (_, endPos))            when less endPos pos   -> SuggestionText.allBody
    | Full(_, _, (startPos, _))          
    | Partial(startPos, _)               when less pos startPos -> SuggestionText.headerOrUsing
    | Partial(_, endPos)                 when less endPos pos   -> SuggestionText.testName
    | HeaderError((startPos, _), _)      when less pos startPos  -> SuggestionText.testHeader
    | HeaderError((_, endPos), errToken) when pos == endPos && checkSuggestion SuggestionText.testHeaderStr errToken -> SuggestionText.testHeader
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
        | IndexTerm.RegistrationTerm(ClassError(_) as t),   _ 
        | IndexTerm.RegistrationTerm(ModuleError(_) as t),  _  -> suggestRegistration pos t
        | IndexTerm.BoolFlag1Term(BoolFlag1Term.Error(_) as t), _  -> suggestBoolFlag1 pos t
        | IndexTerm.BoolFlag2Term(BoolFlag2Term.Error(_) as t), _  -> suggestBoolFlag2 pos t
        | IndexTerm.TestHeaderTerm(Partial(_) as t),   _ 
        | IndexTerm.TestHeaderTerm(HeaderError(_) as t), _ -> suggestTestDeclaration pos t
        | IndexTerm.UsingTerm(Using.Iqn(_) as t), _
        | IndexTerm.UsingTerm(Using.Orphan(_) as t), _ -> suggestUsing pos t
        | _, IndexTerm.RegistrationTerm(ClassError(_) as t)
        | _, IndexTerm.RegistrationTerm(ModuleError(_) as t)   -> suggestRegistration pos t
        | _, IndexTerm.BoolFlag1Term(BoolFlag1Term.Error(_) as t)  -> suggestBoolFlag1 pos t
        | _, IndexTerm.BoolFlag2Term(BoolFlag2Term.Error(_) as t)  -> suggestBoolFlag2 pos t
        | _, IndexTerm.TestHeaderTerm(Partial(_) as t)
        | _, IndexTerm.TestHeaderTerm(HeaderError(_) as t) -> suggestTestDeclaration pos t
        | _, _ -> SuggestionText.allBody

let suggestFrom fileName src pos = 
    Parser.testIndex.Clear()
    //let testContent = File.ReadAllText fileName
    let lexbuf = LexBuffer<char>.FromString src
    setInitialPos lexbuf fileName
    let errLogger = ErrorLogger()
    Lexer.errorLogger <- errLogger
    Parser.errorLogger <- errLogger
    let ast = (lex, lexbuf) ||> parseDrt 
    let index = Parser.testIndex
    
    let suggestion = 
        match index.Find(pos) with
            | CaretTermPosition.Inside(IndexTerm.RegistrationTerm(t)) -> suggestRegistration pos t
            | CaretTermPosition.Inside(IndexTerm.BoolFlag1Term(t))    -> suggestBoolFlag1 pos t
            | CaretTermPosition.Inside(IndexTerm.BoolFlag2Term(t))    -> suggestBoolFlag2 pos t
            | CaretTermPosition.Inside(IndexTerm.TestHeaderTerm(t))   -> suggestTestDeclaration pos t
            | CaretTermPosition.Inside(IndexTerm.UsingTerm(t))        -> suggestUsing pos t
            | CaretTermPosition.Inside(IndexTerm.Error(_, errors))    -> suggestForErrorDeclaration pos errors
            | CaretTermPosition.Between(firstTerm, secondTerm)        -> suggestBetween pos firstTerm secondTerm

    match suggestion with 
        | One(s)  -> [| s |]
        | Many(s) -> s |> Array.ofList
        | None    -> Array.empty 
        
        
    //match suggestion with 
    //    | One(s)  -> if checkSuggestion s writtenText then [| s |] else List.empty |> Array.ofList
    //    | Many(s) -> List.where (fun str -> checkSuggestion str writtenText) s |> Array.ofList
    //    | None    -> Array.empty 
