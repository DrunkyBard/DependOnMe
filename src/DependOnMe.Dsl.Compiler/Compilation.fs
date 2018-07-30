namespace Compilation

open Microsoft.FSharp.Text.Lexing
open TextUtilities
open TestDslAst
open ModuleDslAst
open Errors
open System.IO
open CompilationUnit
open DslAst.Extension

type Compiler private() =
    static let inst = Compiler()

    let addModuleToTable (fileUnit: FileCompilationUnit<ModuleDeclaration>) = 
        match fileUnit.CompilationUnit with
            | ModuleDeclaration.Module(ModuleHeader.Full(name, _, _), classRegs, moduleRegs, pos) -> 
                let validRegistration = 
                    {
                        ValidModuleRegistration.Name = name;
                        ClassRegistrations = Array.ofList classRegs;
                        ModuleRegistrations = Array.ofList moduleRegs;
                        Position = pos;
                    }
                RefTable.Instance.AddDeclaration({ FilePath = fileUnit.FilePath; CompilationUnit = validRegistration; })
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
        let cUnit = { cUnit with Errors = (cUnit.Errors, Semantic.checkModuleSemantic cUnit) ||> List.append  }
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
        let cUnit = { cUnit with Errors = (cUnit.Errors, Semantic.checkTestSemantic cUnit) ||> List.append  }
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

    static member Instance = inst

    member __.CompileTest(testPaths: string[]) = testPaths |> List.ofArray |> compileTests |> Array.ofList
    
    member __.CompileModule(modulePaths: string[]) = modulePaths |> List.ofArray |> compileModules |> Array.ofList

    member __.CompileAll(testPaths: string[], modulePaths: string[]) =
        let testUnits   = __.CompileTest(testPaths)
        let moduleUnits = __.CompileModule(modulePaths)
        (testUnits, moduleUnits)

    member __.CompileTestOnFly(src: string, filePath: string) = compileTest src filePath

    member __.CompileModuleOnFly(src: string, filePath: string) = compileModule src filePath
