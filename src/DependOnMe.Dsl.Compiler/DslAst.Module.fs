module ModuleDslAst

open TextUtilities
open CommonDslAst

type ModuleBody = 
    | Registration of RegistrationTerm list
    | Error        of ErrorTerm

type ModuleHeader = 
    | Full of string * PosRange * PosRange // module terminal range X module name range
    | Partial of PosRange                  // Term terminal range
    | Error of PosRange * string

type ModuleDeclaration = 
    | Module of ModuleHeader * ClassRegistration list * ModuleRegistration list * PosRange // whole module pos range
    | Empty

// ---------------------
    
type ModuleIndexTerm =
    | RegistrationTerm of RegistrationTerm
    | ModuleHeaderTerm of ModuleHeader
    | UsingTerm        of Using
    | Error            of ErrorTerm * ((PosRange * string) list)
