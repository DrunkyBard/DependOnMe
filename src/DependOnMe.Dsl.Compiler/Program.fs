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
    let file = "TestDslFile.drt"
    let testContent = File.ReadAllText file
    let lexbuf = LexBuffer<char>.FromString testContent
    TextUtilities.setInitialPos lexbuf file
    //let lexems = readLexems lexbuf
    let ast = (lex, lexbuf) ||> start
    printfn "%A" argv
    0
