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

    let addModuleToTable = function
        | ModuleDeclaration.Module(ModuleHeader.Full(name, _, _), _, _, _) -> CompilationUnitTable.Instance.AddModule(name)
        | _ -> ()
        
    let addTestToTable = function
        | DependencyTest.Test(TestHeader.Full(name, _, _), _, _, _, _, _) -> CompilationUnitTable.Instance.AddTest(name)
        | _ -> ()

    let compileTests paths =
        let rec compileRec acc = function
            | file::t ->
                let testContent = File.ReadAllText file
                let lexbuf = LexBuffer<char>.FromString testContent
                setInitialPos lexbuf file
                let errLogger = ErrorLogger()
                Lexer.errorLogger  <- errLogger
                Parser.errorLogger <- errLogger
                let cUnit = (Lexer.lex, lexbuf) ||> Parser.parseDrt
                let cUnit = { FilePath = file; CompilationUnit = cUnit }
                List.iter addTestToTable (cUnit.CompilationUnit.Declarations)

                compileRec (cUnit::acc) t
            | [] -> acc
        
        compileRec [] paths

    let compileModule src file = 
        let lexbuf = LexBuffer<char>.FromString src
        setInitialPos lexbuf file
        let errLogger = ErrorLogger()
        ModuleLexer.errorLogger  <- errLogger
        ModuleParser.errorLogger <- errLogger
        let cUnit = (ModuleLexer.lexModule, lexbuf) ||> ModuleParser.parseModule
        List.iter addModuleToTable cUnit.Declarations
        cUnit
        
    let compileModules paths =
        let rec compileRec acc = function
            | file::t ->
                let moduleContent = File.ReadAllText file
                let cUnit = compileModule moduleContent file
                let cUnit = { FilePath = file; CompilationUnit = cUnit }
                compileRec (cUnit::acc) t
            | [] -> acc
        
        compileRec [] paths

    member __.CompileTest(testPaths: string list) = compileTests testPaths
    
    member __.CompileModule(modulePaths: string list) = compileModules modulePaths

    member __.CompileAll(testPaths: string list, modulePaths: string list) =
        let testUnits   = __.CompileTest(testPaths)
        let moduleUnits = __.CompileModule(modulePaths)
        (testUnits, moduleUnits)

    member __.CompileModule(src: string) = compileModule src System.String.Empty




