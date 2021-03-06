﻿module DslAst.Extension

open ModuleDslAst
open TestDslAst
open CompilationUnit
open CommonDslAst
open TextUtilities
open Errors

type ValidModuleRegistration =
    {
        Name: string;
        ClassRegistrations: ClassRegistration[];
        ModuleRegistrations: ModuleRegistration[];
        Position: PosRange;
    }

type ValidTestRegistration =
    {
        Name: string;
        BoolFlag1Values: BoolFlag1 list;
        BoolFlag2Values: BoolFlag2 list;
        RegisteredModules: ModuleRegistration[];
        ClassRegistrations: ClassRegistration[];
        Position: PosRange;
    }

type ValidModulesContainer = 
    {
        ValidModules: ValidModuleRegistration[];
        Errors: PosRange[];
    }
    
type ValidTestsContainer = 
    {
        ValidTests: ValidTestRegistration[];
        Errors: PosRange[];
    }


let OnlyValidModules (cUnit: ModuleCompilationUnit) = 
    let chooseValidModules (declaration: ModuleDeclaration) =
        match declaration with
            | ModuleDeclaration.Module(ModuleHeader.Full(name, _, _), classRegs, moduleRegs, posRange) -> 
                { Name = name; ClassRegistrations = Array.ofList classRegs; ModuleRegistrations = Array.ofList moduleRegs; Position = posRange; } 
                |> Some
            | _ -> None

    let validModules = List.choose chooseValidModules (cUnit.Declarations) |> Array.ofList

    let choosePosRange = function
        | CompilationError.Range(x) -> (x.StartPos, x.EndPos) |> Some
        | CompilationError.Point(_) -> None

    let errors = cUnit.Errors |> List.choose choosePosRange |> Array.ofList

    { ValidModules = validModules; Errors = errors; }
    
let OnlyValidTests (cUnit: TestCompilationUnit) = 
    let chooseValidTests (declaration: DependencyTest) =
        match declaration with
            | DependencyTest.Test(TestHeader.Full(name, _, _), bf1, bf2, classRegs, moduleRegs, posRange) -> 
                { 
                    Name               = name; 
                    BoolFlag1Values    = bf1;
                    BoolFlag2Values    = bf2;
                    Position           = posRange; 
                    RegisteredModules  = moduleRegs |> Array.ofList; 
                    ClassRegistrations = classRegs |> Array.ofList;
                } |> Some
            | _ -> None

    let validTests = List.choose chooseValidTests (cUnit.Declarations) |> Array.ofList

    let choosePosRange = function
        | CompilationError.Range(x) -> (x.StartPos, x.EndPos) |> Some
        | CompilationError.Point(_) -> None

    let errors = cUnit.Errors |> List.choose choosePosRange |> Array.ofList

    { ValidTests = validTests; Errors = errors; }

let TryGetName = function
    | ModuleDeclaration.Module(ModuleHeader.Full(name, _, _), _, _, _) -> (true, name)
    | _ -> (false, null)
