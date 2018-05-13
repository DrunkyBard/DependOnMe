open System.IO
open Microsoft.FSharp.Text.Lexing
open Lexer
open Parser
open DslAst
open TextUtilities

let readLexems lexbuf =
    let rec readLexemsInternal state = function
        | EOF -> EOF::state
        | any -> readLexemsInternal (any::state) (lex lexbuf)
    
    readLexemsInternal [] (lex lexbuf) |> List.rev

[<EntryPoint>]
let main argv =  
    let d2 = TextDistance.jaroDistance "MARTHA" "MARHTA"
    
    let d4 = TextDistance.jaroDistance "DIXON" "DICKSONX"
    
    let d6 = TextDistance.jaroDistance "DWAYNE" "DUANE"

    let dd1 = TextDistance.jaroWinklerDistance "DIXON" "DICKSONX"
    let dd2 = TextDistance.jaroWinklerDistance "MARTHA" "MARHTA"
    let dd3 = TextDistance.jaroWinklerDistance "DWAYNE" "DUANE"

    let file = "TestDslFile.drt"
    let testContent = File.ReadAllText file
    let lexbuf = LexBuffer<char>.FromString testContent
    setInitialPos lexbuf file
    //let lexems = readLexems lexbuf
    let ast = (lex, lexbuf) ||> start
    printfn "%A" argv
    0
