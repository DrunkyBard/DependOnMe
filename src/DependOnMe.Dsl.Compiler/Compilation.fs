namespace Compilation

open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open TextUtilities
open DslAst

type Using = string

type CompilationUnit =
    {
        Usings: Using list;
        Declarations: DependencyTest list;
    }


