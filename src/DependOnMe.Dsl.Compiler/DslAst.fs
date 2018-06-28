module DslAst

open Microsoft.FSharp.Text.Lexing
open TextUtilities
open System.Collections.Generic

type BoolFlagMissingPart =
    | EqualBetween    of PosRange // Range between BF term and boolean value
    | EqualAfter      of Position // Interval after BF term
    | Value           of Position * Position // Interval after equal term X term start position
    | IncompleteValue of PosRange * string
    | BoolTerm        of Position // Interval before equal term
    | BoolFlagTerm    of Position // Interval before equal term

type ClassRegMissingPart =
    | ArrowBetween  of PosRange // Range between dependency and implenentation names
    | ArrowAfter    of Position // Range after FQN
    | OrphanArrow   of PosRange
    | DepName       of PosRange // StartPos: Interval before arrow term, EndPos: term end position
    | ImplName      of Position // Interval after arrow term

type ErrorPosition =
    | Range          of PosRange
    | IntervalAfter  of Position
    | IntervalBefore of Position

type ModuleRegMissingPart = Name of Position // Interval after MODULE term

type BoolFlagType =
    | First
    | Second

type ErrorTerm = Error of PosRange

type BoolFlag1Term = 
    | Flag  of bool * PosRange * Position * PosRange * PosRange // boolean value X bool flag term position X equal term position X boolean value position X whole term pos
    | Error of BoolFlagMissingPart                              // missing part X error position

type BoolFlag2Term = 
    | Flag  of bool * PosRange * Position * PosRange * PosRange // boolean value X bool flag term position X equal term position X boolean value position X whole term pos
    | Error of BoolFlagMissingPart                              // missing part X error position

type RegistrationTerm = 
    | Class       of string * string * PosRange * PosRange * PosRange // dependency X implementation X dependency position X arrow term position X implementation position
    | Module      of string * PosRange * PosRange                     // module name X MODULE terminal position X module name position
    | ClassError  of ClassRegMissingPart  
    | ModuleError of ModuleRegMissingPart

type Declaration = 
    | Registration of RegistrationTerm list
    | BoolFlag1    of BoolFlag1Term
    | BoolFlag2    of BoolFlag2Term
    | Error        of ErrorTerm

//type DependencyTest = 
//    {
//        Name: string;
//        BoolFlags1: BoolFlag1 list;
//        BoolFlags2: BoolFlag2 list;
//        Registrations: Registration list;
//        DeclarationRange: PosRange;
//        TestRange: PosRange;
//    }

type BoolFlag1 = 
    {
        Value: bool;
        BoolFlagPosition: PosRange;
        EqualTermPosition: Position;
        ValuePosition: PosRange;
        WholePosition: PosRange;
    }
    
type BoolFlag2 = 
    {
        Value: bool;
        BoolFlagPosition: PosRange;
        EqualTermPosition: Position;
        ValuePosition: PosRange;
        WholePosition: PosRange;
    }

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

type TestDeclaration = 
    | Full    of string * PosRange * PosRange // Test terminal range X test name range
    | Partial of PosRange // Term terminal range
    | Error   of PosRange * string

type DependencyTest = 
    | Test of TestDeclaration * BoolFlag1 list * BoolFlag2 list * ClassRegistration list * ModuleRegistration list * PosRange // whole test pos range
    | Empty

// ---------------------

type IndexTerm =
    | RegistrationTerm    of RegistrationTerm
    | BoolFlag1Term       of BoolFlag1Term
    | BoolFlag2Term       of BoolFlag2Term
    | TestDeclarationTerm of TestDeclaration
    | UsingTerm           of Using
    | Error               of ErrorTerm * ((PosRange * string) list)

type PositionIndex(posRange: PosRange, term: IndexTerm) =

    member __.Range = posRange

    member __.Term = term

