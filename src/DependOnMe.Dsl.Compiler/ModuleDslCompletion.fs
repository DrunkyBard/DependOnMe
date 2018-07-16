module ModuleDslCompletion

open ModuleDslAst
open CommonDslAst
open Errors
open ModuleLexer
open ModuleParser
open Microsoft.FSharp.Text.Lexing
open TextUtilities
open Navigation
open SuggestionText

let checkSuggestion suggestion writtenText = 
    if System.String.IsNullOrWhiteSpace(writtenText) then true
    else
        let threshold = 0.70
        let distance = TextDistance.jaroWinklerDistance writtenText suggestion

        if distance >= threshold then true
        else false

let suggestRegistration pos = function
    | RegistrationTerm.Class(_, _, _, _, (_, endPos)) 
    | RegistrationTerm.Module(_, _, (_, endPos))            when less endPos pos     -> SuggestionText.allModuleBody
    | RegistrationTerm.ClassError(ArrowBetween(posRange))   when pos <=> posRange    -> SuggestionText.arrow
    | RegistrationTerm.ClassError(ArrowAfter(errPos))       when lessEq errPos pos   -> SuggestionText.arrow
    | RegistrationTerm.ClassError(OrphanArrow(startPos, _)) when lessEq pos startPos -> SuggestionText.depName
    | RegistrationTerm.ClassError(OrphanArrow(_, endPos))   when lessEq endPos pos   -> SuggestionText.implName
    | RegistrationTerm.ClassError(DepName(startPos, _))     when lessEq pos startPos -> SuggestionText.depName
    | RegistrationTerm.ClassError(DepName(_, endPos))       when lessEq endPos pos   -> SuggestionText.allModuleBody
    | RegistrationTerm.ClassError(ImplName(errPos))         when lessEq errPos pos   -> SuggestionText.implName
    | RegistrationTerm.ModuleError(Name(errPos))            when less errPos pos     -> SuggestionText.moduleName
    | _ -> SuggestionText.None

let suggestModuleDeclaration pos = function
    | Full(_, _, (_, endPos))            when less endPos pos   -> SuggestionText.allModuleBody
    | Full(_, _, (startPos, _))          
    | Partial(startPos, _)               when less pos startPos -> SuggestionText.moduleHeaderOrUsing
    | Partial(_, endPos)                 when less endPos pos   -> SuggestionText.moduleName
    | ModuleHeader.Error((startPos, _), _)      when less pos startPos  -> SuggestionText.moduleHeader
    | ModuleHeader.Error((_, endPos), errToken) when pos == endPos && checkSuggestion SuggestionText.moduleHeaderStr errToken -> SuggestionText.moduleHeader
    | _ -> SuggestionText.None

let rec suggestForErrorDeclaration pos = function
    | (errPos, term)::_ when pos <=> errPos -> List.where (fun s -> checkSuggestion s term) SuggestionText.allModuleBodyStr |> Suggestion.Many
    | _::t -> suggestForErrorDeclaration pos t
    | []   -> SuggestionText.None

let suggestUsing pos = function
    | Orphan(startPos, _) 
    | Fqn(_, (startPos, _)) 
    | Iqn(_, (startPos, _)) when less pos startPos -> SuggestionText.using
    | Fqn(_, (_, endPos)) 
    | Iqn(_, (_, endPos))   when less endPos pos   -> SuggestionText.moduleHeaderOrUsing
    | Orphan(_, endPos)     when less endPos pos   -> SuggestionText.ns
    | _ -> SuggestionText.None

let suggestBetween pos firstTerm secondTerm = 
    match firstTerm, secondTerm with
        | ModuleIndexTerm.UsingTerm(t), ModuleIndexTerm.UsingTerm(_)
        | ModuleIndexTerm.UsingTerm(t), ModuleIndexTerm.ModuleHeaderTerm(_) -> suggestUsing pos t
        | ModuleIndexTerm.RegistrationTerm(ClassError(_) as t),   _ 
        | ModuleIndexTerm.RegistrationTerm(ModuleError(_) as t),  _  -> suggestRegistration pos t
        | ModuleIndexTerm.ModuleHeaderTerm(Partial(_) as t),   _ 
        | ModuleIndexTerm.ModuleHeaderTerm(ModuleHeader.Error(_) as t), _ -> suggestModuleDeclaration pos t
        | ModuleIndexTerm.UsingTerm(Using.Iqn(_) as t), _
        | ModuleIndexTerm.UsingTerm(Using.Orphan(_) as t), _ -> suggestUsing pos t
        | _, ModuleIndexTerm.RegistrationTerm(ClassError(_) as t)
        | _, ModuleIndexTerm.RegistrationTerm(ModuleError(_) as t)   -> suggestRegistration pos t
        | _, ModuleIndexTerm.ModuleHeaderTerm(Partial(_) as t)
        | _, ModuleIndexTerm.ModuleHeaderTerm(ModuleHeader.Error(_) as t) -> suggestModuleDeclaration pos t
        | _, _ -> SuggestionText.allModuleBody

let suggestFrom fileName src pos = 
    ModuleParser.testIndex.Clear()
    let lexbuf = LexBuffer<char>.FromString src
    setInitialPos lexbuf fileName
    let errLogger = ErrorLogger()
    ModuleLexer.errorLogger <- errLogger
    ModuleParser.errorLogger <- errLogger
    let ast = (lexModule, lexbuf) ||> parseModule 
    let index = ModuleParser.testIndex
    
    let suggestion = 
        match index.Find(pos) with
            | CaretTermPosition.Inside(ModuleIndexTerm.RegistrationTerm(t)) -> suggestRegistration pos t
            | CaretTermPosition.Inside(ModuleIndexTerm.ModuleHeaderTerm(t)) -> suggestModuleDeclaration pos t
            | CaretTermPosition.Inside(ModuleIndexTerm.UsingTerm(t))        -> suggestUsing pos t
            | CaretTermPosition.Inside(ModuleIndexTerm.Error(_, errors))    -> suggestForErrorDeclaration pos errors
            | CaretTermPosition.Between(firstTerm, secondTerm)              -> suggestBetween pos firstTerm secondTerm

    match suggestion with 
        | One(s)  -> [| s |]
        | Many(s) -> s |> Array.ofList
        | None    -> Array.empty 
