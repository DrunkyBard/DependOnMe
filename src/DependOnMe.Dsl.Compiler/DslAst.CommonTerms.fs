module CommonDslAst

open Microsoft.FSharp.Text.Lexing
open TextUtilities

type ErrorPosition =
    | Range          of PosRange
    | IntervalAfter  of Position
    | IntervalBefore of Position

type ModuleRegMissingPart = Name of Position // Interval after MODULE term

type ErrorTerm = Error of PosRange

type ClassRegMissingPart =
    | ArrowBetween  of PosRange // Range between dependency and implenentation names
    | ArrowAfter    of Position // Range after FQN
    | OrphanArrow   of PosRange
    | DepName       of PosRange // StartPos: Interval before arrow term, EndPos: term end position
    | ImplName      of Position // Interval after arrow term

type RegistrationTerm = 
    | Class       of string * string * PosRange * PosRange * PosRange // dependency X implementation X dependency position X arrow term position X implementation position
    | Module      of string * PosRange * PosRange                     // module name X MODULE terminal position X module name position
    | ClassError  of ClassRegMissingPart  
    | ModuleError of ModuleRegMissingPart

type ClassRegistration =
    {
        Dependency: string;
        Implementation: string;
        DependencyPosition: PosRange;
        ArrowTermPosition: PosRange;
        ImplementationPosition: PosRange;
    }

type ModuleRegistration = 
    {
        Name: string;
        ModuleTermPosition: PosRange;
        NamePosition: PosRange;
    }

type Using = 
    | Fqn    of string * PosRange
    | Iqn    of string * PosRange
    | Orphan of PosRange

type 'a PositionIndex(posRange: PosRange, term: 'a) =

    member __.Range = posRange

    member __.Term = term