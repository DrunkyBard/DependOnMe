module DslAst.Extension

open ModuleDslAst
open CompilationUnit
open CommonDslAst

type ValidModuleRegistration =
    {
        Name: string;
        ClassRegistrations: ClassRegistration[];
        ModuleRegistrations: ModuleRegistration[];
    }


let OnlyValidModules (cUnit: ModuleCompilationUnit) = 
    let chooseValidModules (declaration: ModuleDeclaration) =
        match declaration with
            | ModuleDeclaration.Module(ModuleHeader.Full(name, _, _), classRegs, moduleRegs, _) -> 
                { Name = name; ClassRegistrations = Array.ofList classRegs; ModuleRegistrations = Array.ofList moduleRegs; } |> Some
            | ModuleDeclaration.Empty
            | _ -> None

    List.choose chooseValidModules (cUnit.Declarations) |> Array.ofList
