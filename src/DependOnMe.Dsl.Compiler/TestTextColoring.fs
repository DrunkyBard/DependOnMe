﻿module TestTextColoring

open System
open Lexer
open Parser
open Microsoft.FSharp.Text.Lexing
open TextUtilities
open Errors
open TextColoring

let readLexems (lexbuf: LexBuffer<char>) (offset: int) =
    let colorWithOffset classifier = createColor classifier (lexbuf.StartPos.Column + offset) (lexbuf.LexemeLength)

    let rec readLexemsInternal acc = function
        | TESTHEADER 
        | BF1        
        | BF2        
        | TRUE       
        | FALSE      
        | MODULE     
        | USING      -> ((colorWithOffset Classification.Keyword) :: acc, lex lexbuf) ||> readLexemsInternal
        | ARROW      
        | EQ         
        | QUOT       -> ((colorWithOffset Classification.Sign)    :: acc, lex lexbuf) ||> readLexemsInternal
        | FQN(_)     
        | IQN(_)     
        | SNAME(_)   -> ((colorWithOffset Classification.Default) :: acc, lex lexbuf) ||> readLexemsInternal
        | EOF        -> acc

    let errLogger = ErrorLogger()
    Lexer.errorLogger <- errLogger
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

let colorLine line offset =
    let lexbuf = LexBuffer<char>.FromString line
    setInitialPos lexbuf String.Empty
    readLexems lexbuf offset |> Array.ofList
    