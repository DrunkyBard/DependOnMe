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
    | Flag  of bool * PosRange
    | Error of BoolFlagMissingPart * Position * PosRange // missing part X error position X non-terminal range

type BoolFlag2 = 
    | Flag  of bool * PosRange
    | Error of BoolFlagMissingPart * Position * PosRange // missing part X error position X non-terminal range

type Registration = 
    | Class       of string * string * PosRange * PosRange // dependency X implementation X dependency position X implementation position
    | Module      of string * PosRange * PosRange // module name X MODULE terminal position X module name position
    | ClassError  of ClassRegMissingPart * Position * PosRange // missing part X error position X non-terminal range
    | ModuleError of ModuleRegMissingPart * Position * PosRange

type Declaration = 
    | Registration of Registration list
    | BoolFlag1    of BoolFlag1
    | BoolFlag2    of BoolFlag2

type DependencyTest = Test of string * BoolFlag1 * BoolFlag2 * Registration list * PosRange * PosRange // test declaration non-terminal range X whole test pos range

