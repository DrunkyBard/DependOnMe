module DslAst

open Microsoft.FSharp.Text.Lexing
type PosRange = (Position * Position)

type BoolFlag =
    | Flag1 of bool * PosRange
    | Flag2 of bool * PosRange

type Registration = 
    | Class of string * string * PosRange * PosRange // dependency X implementation X dependency position X implementation position
    | Module of string * PosRange * PosRange // module name X MODULE terminal position X module name position

type Declaration = 
    | Registration of Registration list
    | BoolFlag of BoolFlag

type DependencyTest = Test of string * BoolFlag * BoolFlag * Registration list * PosRange