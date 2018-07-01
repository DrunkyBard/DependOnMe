namespace CompilationUnit

open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open TextUtilities
open TestDslAst
open ModuleDslAst
open Errors

type Using = string

type TestCompilationUnit =
    {
        Usings: Using list;
        Declarations: DependencyTest list;
        Errors: CompilationError list
    }

type ModuleCompilationUnit =
    {
        Usings: Using list;
        Declarations: ModuleDeclaration list;
        Errors: CompilationError list
    }

type 'a FileCompilationUnit = 
    {
        FilePath: string;
        CompilationUnit: 'a;
    }
