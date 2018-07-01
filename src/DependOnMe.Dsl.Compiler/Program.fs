open System.IO
open Microsoft.FSharp.Text.Lexing
open Lexer
open Parser
open TestDslAst
open CommonDslAst
open TextUtilities
open Errors
open System
open CompilationUnit
open System.Collections.Generic
open System.Collections.Generic
open Microsoft.FSharp.Text.Parsing
open Common
open DataStructures
open Positioning

let readLexems lexbuf =
    let rec readLexemsInternal state = function
        | EOF -> EOF::state
        | any -> readLexemsInternal (any::state) (lex lexbuf)
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

[<EntryPoint>]
let main argv = 
    let file = "TestDslFile.drt"
    let testContent = File.ReadAllText file
    let lexbuf = LexBuffer<char>.FromString testContent
    setInitialPos lexbuf file
    //let errLogger = ErrorLogger()
    //Lexer.errorLogger  <- errLogger
    //Parser.errorLogger <- errLogger
    //let lexems = readLexems lexbuf

    let errLogger = ErrorLogger()
    Lexer.errorLogger  <- errLogger
    Parser.errorLogger <- errLogger
    let cUnit = (lex, lexbuf) ||> Parser.parseDrt
    let ooo = Semantic.checkDuplicates cUnit.Declarations.Head
    
    let logger = Parser.errorLogger
    let testIdx = Parser.testIndex
    let findPos: Position = 
        { 
            pos_bol = 0;
            pos_fname = file;
            pos_cnum = 1;
            pos_lnum = 1; 
        }
    
    let q = testIdx.Find(findPos)
    let suggestion = DslCompletion.suggestFrom file testContent findPos
    let logger = Parser.errorLogger
    0
