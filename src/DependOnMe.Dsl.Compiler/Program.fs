open System.IO
open Microsoft.FSharp.Text.Lexing
open Lexer
open Parser
open DslAst

let readLexems lexbuf =
    let rec readLexemsInternal state = function
        | EOF -> EOF::state
        | any -> readLexemsInternal (any::state) (lex lexbuf)
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

[<EntryPoint>]
let main argv =     
    let testContent = File.ReadAllText "TestDslFile.drt"
    let lexbuf = LexBuffer<char>.FromString testContent
    //let lexems = readLexems lexbuf
    let ast = (lex, lexbuf) ||> start
    printfn "%A" argv
    0
