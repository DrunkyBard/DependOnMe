module ModuleTextColoring

open System
open ModuleLexer
open ModuleParser
open Microsoft.FSharp.Text.Lexing
open TextUtilities
open Errors
open TextColoring

let readLexems (lexbuf: LexBuffer<char>) (offset: int) =
    let colorWithOffset classifier = createColor classifier (lexbuf.StartPos.Column + offset) (lexbuf.LexemeLength)

    let rec readLexemsInternal acc = function
        | MODULEHEADER 
        | MODULE     
        | USING      -> ((colorWithOffset Classification.Keyword) :: acc, lexModule lexbuf) ||> readLexemsInternal
        | ARROW      
        | QUOT       -> ((colorWithOffset Classification.Sign)    :: acc, lexModule lexbuf) ||> readLexemsInternal
        | FQN(_)     
        | IQN(_)     
        | SNAME(_)   -> ((colorWithOffset Classification.Default) :: acc, lexModule lexbuf) ||> readLexemsInternal
        | EOF        -> acc

    let errLogger = ErrorLogger()
    ModuleLexer.errorLogger <- errLogger
    
    readLexemsInternal [] (lexModule lexbuf) |> List.rev

let colorLine line offset =
    let lexbuf = LexBuffer<char>.FromString line
    setInitialPos lexbuf String.Empty
    readLexems lexbuf offset |> Array.ofList
    