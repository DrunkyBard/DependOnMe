open System.IO
open Microsoft.FSharp.Text.Lexing
open Lexer
open Parser
open DslAst
open TextUtilities
open Errors
open System


let readLexems lexbuf =
    let rec readLexemsInternal state = function
        | EOF -> EOF::state
        | any -> readLexemsInternal (any::state) (lex lexbuf)
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

let renderAst (DependencyTest.Test(name, boolFlags1, boolFlags2, regList, (startPos, endPos), testRange)) (source: string[]) = 
    printf "Test name: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" name startPos.Line startPos.Column endPos.Line endPos.Column

    let rec printBoolFlag1 = function
        | BoolFlag1.Flag(v, (startPos, endPos)) -> 
            printf "Bool flag 1: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" v startPos.Line startPos.Column endPos.Line endPos.Column
        | BoolFlag1.Error(missing, errPos, (startPos, endPos)) -> 
            printf "Bool flag 1. Missing part: %A. Error pos: {Line: %A, Column: %A}. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" missing errPos.Line errPos.Column startPos.Line startPos.Column endPos.Line endPos.Column
            
    let rec printBoolFlag2 = function
        | BoolFlag2.Flag(v, (startPos, endPos)) -> 
            printf "Bool flag 2: %A. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" v startPos.Line startPos.Column endPos.Line endPos.Column
        | BoolFlag2.Error(missing, errPos, (startPos, endPos)) -> 
            printf "Bool flag 2. Missing part: %A. Error pos: {Line: %A, Column: %A}. StartPos: {Line: %A, Column: %A}. EndPos: {Line: %A, Column: %A}\r\n" missing errPos.Line errPos.Column startPos.Line startPos.Column endPos.Line endPos.Column

    let printReg = function
        | Class(dep, imp, depPos, impPos) -> 
            printf "Class registration. From %A to %A. Dep pos: %A. Imp pos: %A\r\n" dep imp depPos impPos
        | Module(modName, termPos, modPos) -> 
            printf "Module registration: %A. Mod term pos: %A. Mod name pos: %A\r\n" modName termPos modPos
        | ClassError(missing, errPos, regPos) ->
            printf "Class error: %A. Error pos: %A. Reg pos: %A" missing errPos regPos
        | ModuleError(missing, errPos, regPos) -> 
            printf "Module error: %A. Error pos: %A. Reg pos: %A" missing errPos regPos
    
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
    let d2 = TextDistance.jaroDistance "MARTHA" "MARHTA"
    
    let d4 = TextDistance.jaroDistance "DIXON" "DICKSONX"
    
    let d6 = TextDistance.jaroDistance "DWAYNE" "DUANE"

    let dd1 = TextDistance.jaroWinklerDistance "DIXON" "DICKSONX"
    let dd2 = TextDistance.jaroWinklerDistance "MARTHA" "MARHTA"
    let dd3 = TextDistance.jaroWinklerDistance "DWAYNE" "DUANE"
    let errorLogger = ErrorLogger()
    let file = "TestDslFile.drt"
    let testContent = File.ReadAllText file
    let testContentArr = File.ReadAllLines file
    let lexbuf = LexBuffer<char>.FromString testContent
    setInitialPos lexbuf file
    //let lexems = readLexems lexbuf
    let ast = (lex, lexbuf) ||> start
    let logger = Parser.errorLogger
    logger.Diagnostics |> renderErrors
    renderAst ast testContentArr
    0
