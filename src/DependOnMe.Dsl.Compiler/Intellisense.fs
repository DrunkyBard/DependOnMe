module Intellisense

open DslAst

type Suggestion = Suggestion of string * bool // suggestion X should be printed

//let transition astNode = 
//    match astNode with
//        | Test(_, _, _, _) -> None
//        | 