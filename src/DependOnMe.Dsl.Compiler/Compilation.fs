namespace Compilation

open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open TextUtilities
open TestDslAst
open ModuleDslAst
open Errors
open System.Threading.Tasks
open System.Collections.Concurrent
open System.IO
open CompilationUnit
open Semantic
open CompilationTable

type Compiler() =

    let addModuleToTable (fileUnit: FileCompilationUnit<ModuleDeclaration>) = 
        match fileUnit.CompilationUnit with
            | ModuleDeclaration.Module(ModuleHeader.Full(name, _, _), _, _, _) -> RefTable.Instance.AddDeclaration(name, fileUnit)
            | _ -> ()
        
    let addTestToTable (fileUnit: FileCompilationUnit<DependencyTest>) = 
        match fileUnit.CompilationUnit with
            | DependencyTest.Test(TestHeader.Full(_, _, _), _, _, _, _, _) -> RefTable.Instance.AddRef(fileUnit)
            | _ -> ()

    let compileModule src file = 
        let lexbuf = LexBuffer<char>.FromString src
        setInitialPos lexbuf file
        let errLogger = ErrorLogger()
        ModuleLexer.errorLogger  <- errLogger
        ModuleParser.errorLogger <- errLogger
        let cUnit = (ModuleLexer.lexModule, lexbuf) ||> ModuleParser.parseModule
        cUnit.Declarations 
            |> List.map (fun x -> { FilePath = file; CompilationUnit = x; })
            |> List.iter addModuleToTable
        cUnit
        
    let compileTest testContent file = 
        let lexbuf = LexBuffer<char>.FromString testContent
        setInitialPos lexbuf file
        let errLogger = ErrorLogger()
        Lexer.errorLogger  <- errLogger
        Parser.errorLogger <- errLogger
        let cUnit = (Lexer.lex, lexbuf) ||> Parser.parseDrt
        cUnit.Declarations 
            |> List.map (fun x -> { FilePath = file; CompilationUnit = x; })
            |> List.iter addTestToTable
        cUnit

    let compileTests paths =
        let rec compileRec acc = function
            | file::t ->
                let testContent = File.ReadAllText file
                let cUnit = compileTest testContent file
                let cUnit = { FilePath = file; CompilationUnit = cUnit }

                compileRec (cUnit::acc) t
            | [] -> acc
        
        compileRec [] paths
        
    let compileModules paths =
        let rec compileRec acc = function
            | file::t ->
                let moduleContent = File.ReadAllText file
                let cUnit = compileModule moduleContent file
                let cUnit = { FilePath = file; CompilationUnit = cUnit }

                compileRec (cUnit::acc) t
            | [] -> acc
        
        compileRec [] paths

    member __.CompileTest(testPaths: string[]) = testPaths |> List.ofArray |> compileTests |> Array.ofList
    
    member __.CompileModule(modulePaths: string[]) = modulePaths |> List.ofArray |> compileModules |> Array.ofList

    member __.CompileAll(testPaths: string[], modulePaths: string[]) =
        let testUnits   = __.CompileTest(testPaths)
        let moduleUnits = __.CompileModule(modulePaths)
        (testUnits, moduleUnits)

    member __.CompileTestOnFly(src: string, filePath: string) = compileTest src filePath

    member __.CompileModuleOnFly(src: string, filePath: string) = compileModule src filePath

    member __.CompileTest(src: string) = __.CompileTestOnFly(src, System.String.Empty)
    
    member __.CompileModule(src: string) = __.CompileModuleOnFly(src, System.String.Empty)
