module DslAst

open Microsoft.FSharp.Text.Lexing
open TextUtilities
open System.Collections.Generic

type BoolFlagMissingPart =
    | EqualBetween  of PosRange // Range between BF term and boolean value
    | EqualAfter    of Position // Interval after BF term
    | Value         of Position // Interval after equal term
    | BoolTerm      of Position // Interval before equal term
    | BoolFlagTerm  of Position // Interval before equal term

type ClassRegMissingPart =
    | Arrow    of PosRange // Range between dependency and implenentation names
    | DepName  of Position // Interval before arrow term
    | ImplName of Position // Interval after arrow term

type ErrorPosion =
    | Range          of PosRange
    | IntervalAfter  of Position
    | IntervalBefore of Position

type ModuleRegMissingPart = Name of Position // Interval after MODULE term

type BoolFlagType =
    | First
    | Second

type BoolFlag1 = 
    | Flag  of bool * PosRange * Position * PosRange // boolean value X bool flag term position X equal term position X boolean value position
    | Error of BoolFlagMissingPart                   // missing part X error position

type BoolFlag2 = 
    | Flag  of bool * PosRange * Position * PosRange // boolean value X bool flag term position X equal term position X boolean value position
    | Error of BoolFlagMissingPart                   // missing part X error position

type Registration = 
    | Class       of string * string * PosRange * PosRange * PosRange // dependency X implementation X dependency position X arrow term position X implementation position
    | Module      of string * PosRange * PosRange                     // module name X MODULE terminal position X module name position
    | ClassError  of ClassRegMissingPart  
    | ModuleError of ModuleRegMissingPart

type Declaration = 
    | Registration of Registration list
    | BoolFlag1    of BoolFlag1
    | BoolFlag2    of BoolFlag2
    | Error

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
    | Error   of PosRange

type DependencyTest = 
    | Test of TestDeclaration * BoolFlag1 list * BoolFlag2 list * Registration list * PosRange // whole test pos range
    | Empty

// ---------------------

type IndexTerm =
    | RegistrationTerm    of Registration
    | BoolFlag1Term       of BoolFlag1
    | BoolFlag2Term       of BoolFlag2
    | TestDeclarationTerm of TestDeclaration

type PositionIndex(posRange: PosRange, term: IndexTerm) =

    member __.Range = posRange

    member __.Term = term

