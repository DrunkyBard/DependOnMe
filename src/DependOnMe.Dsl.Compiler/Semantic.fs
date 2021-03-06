﻿module Semantic

open TextUtilities
open TestDslAst
open Errors
open System.Collections.Concurrent
open CompilationUnit
open System
open DslAst
open Compilation

let listDuplications errMsg (positions: PosRange list) = 
    let rec innerCheck acc = function
        | (startPos, endPos)::t -> 
            let err = (startPos, endPos, errMsg) |> ProdError |> CompilationError.Range
            innerCheck (err::acc) t
        | [] -> acc

    innerCheck [] (List.skip 1 positions)

let selectDuplicated regs errMsg keySelector posErrorSelector = 
    let mapToError ((startPos, endPos): PosRange) c = (startPos, endPos, errMsg c) |> ProdError |> CompilationError.Range
    regs
        |> List.groupBy keySelector
        |> List.where (fun (_, c) -> c.Length > 1)
        |> List.collect (fun (_, c) -> c.Tail)
        |> List.map (fun c -> (posErrorSelector c, c))
        |> List.map (fun (posRange, c) -> mapToError posRange c)

let checkDuplicates = function
    | DependencyTest.Test(Full(_, _, _), boolFlags1, boolFlags2, classRegs, moduleRegs, _) -> 
        let duplicatedClasses = selectDuplicated classRegs  (fun c -> ErrMsg.DuplicatedDependency c.Dependency)  (fun c -> c.Dependency) (fun c -> c.DependencyPosition)
        let duplicatedModules = selectDuplicated moduleRegs (fun c -> ErrMsg.DuplicatedModule c.Name) (fun c -> c.Name) (fun c -> c.NamePosition)
        let bool1Duplications = (ErrMsg.DuplicateBf1Declaration, boolFlags1 |> List.map (fun c -> c.WholePosition)) ||> listDuplications
        let bool2Duplications = (ErrMsg.DuplicateBf2Declaration, boolFlags2 |> List.map (fun c -> c.WholePosition)) ||> listDuplications

        duplicatedClasses
        |> List.append duplicatedModules 
        |> List.append bool1Duplications 
        |> List.append bool2Duplications
    | DependencyTest.Empty
    | DependencyTest.Test(_) -> []

let checkTestSemantic testUnit =
    Extension.OnlyValidTests(testUnit).ValidTests 
    |> Seq.collect (fun test -> test.RegisteredModules)
    |> Seq.choose  (fun regModule -> 
        let fromPos, toPos = fst regModule.ModuleTermPosition, snd regModule.NamePosition

        if RefTable.Instance.HasDuplicates regModule.Name then 
            ProdError(fromPos, toPos, ErrMsg.Ambiguous(regModule.Name)) |> Range |> Some
        elif RefTable.Instance.HasDefinition(regModule.Name) |> not then
            ProdError(fromPos, toPos, ErrMsg.ModuleIsNotDefined(regModule.Name)) |> Range |> Some
        else None)
    |> List.ofSeq

let checkModuleSemantic moduleUnit =
    Extension.OnlyValidModules(moduleUnit).ValidModules
    |> Seq.collect (fun m -> m.ModuleRegistrations)
    |> Seq.choose  (fun m ->
        let fromPos, toPos = fst m.ModuleTermPosition, snd m.NamePosition

        if RefTable.Instance.HasDuplicates m.Name then
            ProdError(fromPos, toPos, ErrMsg.DuplicatedModule(m.Name)) |> Range |> Some
        else None)
    |> List.ofSeq
