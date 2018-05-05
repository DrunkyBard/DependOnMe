module DslAst

type BoolFlag =
    | Flag1 of bool
    | Flag2 of bool

type Registration = 
    | Class of string * string
    | Module of string

type DependencyTest = Test of string * BoolFlag * BoolFlag * Registration list