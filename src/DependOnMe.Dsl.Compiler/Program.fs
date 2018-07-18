open System.IO
open Lexer
open Parser
open Compilation

let readLexems lexbuf =
    let rec readLexemsInternal state = function
        | EOF -> EOF::state
        | any -> readLexemsInternal (any::state) (lex lexbuf)
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

[<EntryPoint>]
let main argv = 
    //let file = "TestDslFile.drt"
    //let testContent = File.ReadAllText file
    //let lexbuf = LexBuffer<char>.FromString testContent
    //setInitialPos lexbuf file
    //let errLogger = ErrorLogger()
    //Lexer.errorLogger  <- errLogger
    //Parser.errorLogger <- errLogger
    //let lexems = readLexems lexbuf

    let fPath = @"c:\users\satta\documents\TestDslSolution\ClassLibrary1\TestDslFile.drt"
    let src = File.ReadAllText(fPath)
    let un = Compiler.Instance.CompileTestOnFly(src, fPath)

    //let errLogger = ErrorLogger()
    //Lexer.errorLogger  <- errLogger
    //Parser.errorLogger <- errLogger
    //let cUnit = (lex, lexbuf) ||> Parser.parseDrt
    //let ooo = Semantic.checkDuplicates cUnit.Declarations.Head
    
    //let logger = Parser.errorLogger
    //let testIdx = Parser.testIndex
    //let findPos: Position = 
    //    { 
    //        pos_bol = 0;
    //        pos_fname = file;
    //        pos_cnum = 1;
    //        pos_lnum = 1; 
    //    }
    
    //let q = testIdx.Find(findPos)
    //let suggestion = TestDslCompletion.suggestFrom file testContent findPos
    //let logger = Parser.errorLogger
    0
