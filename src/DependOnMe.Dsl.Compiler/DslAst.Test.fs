module TestDslAst

open CommonDslAst
open Microsoft.FSharp.Text.Lexing
open TextUtilities

type BoolFlagMissingPart =
    | EqualBetween    of PosRange // Range between BF term and boolean value
    | EqualAfter      of Position // Interval after BF term
    | Value           of Position * Position // Interval after equal term X term start position
    | IncompleteValue of PosRange * string
    | BoolTerm        of Position // Interval before equal term
    | BoolFlagTerm    of Position // Interval before equal term

type BoolFlagType =
    | First
    | Second

type BoolFlag1Term = 
    | Flag  of bool * PosRange * Position * PosRange * PosRange // boolean value X bool flag term position X equal term position X boolean value position X whole term pos
    | Error of BoolFlagMissingPart                              // missing part X error position

type BoolFlag2Term = 
    | Flag  of bool * PosRange * Position * PosRange * PosRange // boolean value X bool flag term position X equal term position X boolean value position X whole term pos
    | Error of BoolFlagMissingPart                              // missing part X error position

type Declaration = 
    | Registration of RegistrationTerm list
    | BoolFlag1    of BoolFlag1Term
    | BoolFlag2    of BoolFlag2Term
    | Error        of ErrorTerm

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

type TestHeader = 
    | Full        of string * PosRange * PosRange // Test terminal range X test name range
    | Partial     of PosRange                     // Term terminal range
    | HeaderError of PosRange * string

type DependencyTest = 
    | Test of TestHeader * BoolFlag1 list * BoolFlag2 list * ClassRegistration list * ModuleRegistration list * PosRange // whole test pos range
    | Empty

// ---------------------

type IndexTerm =
    | RegistrationTerm of RegistrationTerm
    | BoolFlag1Term    of BoolFlag1Term
    | BoolFlag2Term    of BoolFlag2Term
    | TestHeaderTerm   of TestHeader
    | UsingTerm        of Using
    | Error            of ErrorTerm * ((PosRange * string) list)
    