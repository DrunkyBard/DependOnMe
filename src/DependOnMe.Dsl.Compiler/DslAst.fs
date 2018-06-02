module DslAst

open Microsoft.FSharp.Text.Lexing
open TextUtilities

type BoolFlagMissingPart =
    | Equal
    | Value

type ClassRegMissingPart =
    | Arrow
    | Name

type ModuleRegMissingPart = Name

type BoolFlagType =
    | First
    | Second

type BoolFlag1 = 
    | Flag  of bool * PosRange * Position * PosRange     // boolean value X bool flag term position X equal term position X boolean value position
    | Error of BoolFlagMissingPart * Position // missing part X error position

type BoolFlag2 = 
    | Flag  of bool * PosRange * Position * PosRange     // boolean value X bool flag term position X equal term position X boolean value position
    | Error of BoolFlagMissingPart * Position // missing part X error position

type Registration = 
    | Class       of string * string * PosRange * PosRange * PosRange // dependency X implementation X dependency position X arrow term position X implementation position
    | Module      of string * PosRange * PosRange                     // module name X MODULE terminal position X module name position
    | ClassError  of ClassRegMissingPart * Position                   // missing part X error position
    | ModuleError of ModuleRegMissingPart * Position 

type Declaration = 
    | Registration of Registration list
    | BoolFlag1    of BoolFlag1
    | BoolFlag2    of BoolFlag2

//type DependencyTest = 
//    {
//        Name: string;
//        BoolFlags1: BoolFlag1 list;
//        BoolFlags2: BoolFlag2 list;
//        Registrations: Registration list;
//        DeclarationRange: PosRange;
//        TestRange: PosRange;
//    }

type TestDeclaration = 
    | Full    of string * PosRange * PosRange // Test terminal range X test name range
    | Partial of PosRange // Term terminal range

type DependencyTest = Test of TestDeclaration * BoolFlag1 list * BoolFlag2 list * Registration list * PosRange // whole test pos range

// ---------------------

type IndexValue =
    | DeclarationValue of Declaration * PosRange
    | BoolFlag1Value   of BoolFlag1 * PosRange
    | BoolFlag2Value   of BoolFlag2 * PosRange
    | TestDeclaration  of TestDeclaration * PosRange

type PositionIndex(posRange: PosRange, term: IndexValue) =

    member __.Range = posRange

    member __.Term = term


