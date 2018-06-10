open System.IO
open Microsoft.FSharp.Text.Lexing
open Lexer
open Parser
open DslAst
open TextUtilities
open Errors
open System
open Compilation
open System.Collections.Generic
open System.Collections.Generic
open Common
open DataStructures
open Positioning

let readLexems lexbuf =
    let rec readLexemsInternal state = function
        | EOF -> EOF::state
        | any -> readLexemsInternal (any::state) (lex lexbuf)
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

let renderAst  = function
    | DependencyTest.Empty -> 
        printf "Test file is empty"
    | DependencyTest.Test(name, boolFlags1, boolFlags2, regList, (startPos, endPos)) ->
        printf "Test name: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" name startPos.Line startPos.Column endPos.Line endPos.Column

        let rec printBoolFlag1 = function
            | BoolFlag1.Flag(v, (startPos, endPos), _, _) -> 
                printf "Bool flag 1: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" v startPos.Line startPos.Column endPos.Line endPos.Column
            | BoolFlag1.Error(missing) -> 
                printf "Bool flag 1. Missing part: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" missing startPos.Line startPos.Column endPos.Line endPos.Column
                
        let rec printBoolFlag2 = function
            | BoolFlag2.Flag(v, (startPos, endPos), _, _) -> 
                printf "Bool flag 2: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" v startPos.Line startPos.Column endPos.Line endPos.Column
            | BoolFlag2.Error(missing) -> 
                printf "Bool flag 2. Missing part: %A.  StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" missing startPos.Line startPos.Column endPos.Line endPos.Column

        let printReg = function
            | Class(dep, imp, depPos, _, impPos) -> 
                printf "Class registration. From %A to %A. Dep pos: %A. Imp pos: %A\r\n" dep imp depPos impPos
            | Module(modName, termPos, modPos) -> 
                printf "Module registration: %A. Mod term pos: %A. Mod name pos: %A\r\n" modName termPos modPos
            | ClassError(missing) ->
                printf "Class error: %A" missing 
            | ModuleError(missing) -> 
                printf "Module error: %A" missing
        
        (printBoolFlag1, boolFlags1) ||> List.iter
        (printBoolFlag2, boolFlags2) ||> List.iter
        (printReg, regList) ||> List.iter

let renderErrors diagnostics = 
    let rec renderRec = function
        | Range(e)::t -> 
            printfn "(%A, %A)-(%A, %A): %A\r\n" e.StartPos.Line e.StartPos.Column e.EndPos.Line e.EndPos.Column e.Message
            renderRec t
        | Point(e)::t -> 
            printfn "(%A, %A): %A\r\n" e.Pos.Line e.Pos.Column e.Message
            renderRec t
        |[] -> ()
    
    printfn "ERRORS:\r\n"
    renderRec diagnostics

[<EntryPoint>]
let main argv = 
    let tree = RedBlackTree<int, int>(Comparer.Default)
    tree.Insert(1, 1)
    tree.Insert(2, 1)
    tree.Insert(-1, 1)
    tree.Insert(2, 1)
    tree.Insert(6, 1)
    tree.Insert(54, 1)
    tree.Insert(32, 1)
    tree.Insert(-23, 1)
    tree.Insert(44, 1)
    tree.Print()
    //heap.Insert 6
    //heap.Insert 9
    //heap.Insert 11
    //heap.Insert 5
    //heap.Insert 2
    //heap.Insert 4
    //heap.Insert 5
    //heap.Insert 7
    //heap.Insert 2
    
    let d2 = TextDistance.jaroDistance "MARTHA" "MARHTA"
    
    let d4 = TextDistance.jaroDistance "DIXON" "DICKSONX"
    
    let d6 = TextDistance.jaroDistance "DWAYNE" "DUANE"
    
    let dd1 = TextDistance.jaroWinklerDistance "DIXON" "DICKSONX"
    let dd2 = TextDistance.jaroWinklerDistance "MARTHA" "MARHTA"
    let dd3 = TextDistance.jaroWinklerDistance "DWAYNE" "DUANE"
    let errorLogger = ErrorLogger()
    let file = "TestDslFile.drt"
    let testContent = File.ReadAllText file
    let lexbuf = LexBuffer<char>.FromString testContent
    setInitialPos lexbuf file
    //let lexems = readLexems lexbuf
    let (DependencyTest.Test(name, boolFlags1, boolFlags2, regs, wholeRange) as l) = (lex, lexbuf) ||> start
    let logger = Parser.errorLogger
    let idx = Parser.index
    let testIdx = Parser.testIndex
    let findPos: Position = { pos_bol = 0;
                        pos_fname = file;
                        pos_cnum = 20;
                        pos_lnum = 1; }
    
    //let p = idx.TryFindTermPosition(findPos)
    let fromPos, toPos = fst testIdx.Positions.[1].Range, fst testIdx.Positions.[3].Range
    //testIdx.RemoveBetween(fromPos, toPos)
    let q = testIdx.Find(findPos)
    let suggestion = DslCompletion.suggestFrom file "" findPos
    
    let logger = Parser.errorLogger
    logger.Diagnostics |> renderErrors
    0
