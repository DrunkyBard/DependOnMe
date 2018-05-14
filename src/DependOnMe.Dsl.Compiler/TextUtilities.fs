module TextUtilities

open Microsoft.FSharp.Text.Parsing
open Microsoft.FSharp.Text.Lexing
open System

let setInitialPos (lexbuf: LexBuffer<char>) filename =
     lexbuf.EndPos <- { pos_bol = 0;
                        pos_fname=filename;
                        pos_cnum=1;
                        pos_lnum=1 }

let newLine (lexbuf: LexBuffer<char>) = 
    let nextLine = {lexbuf.EndPos.NextLine with pos_cnum = lexbuf.EndPos.NextLine.pos_cnum + 1}
    lexbuf.StartPos <- nextLine 
    lexbuf.EndPos <- nextLine
    ()

let posRange (parseState: IParseState) productionNumber = parseState.InputRange productionNumber

let startPos (parseState: IParseState) productionNumber = parseState.InputStartPosition productionNumber

let endPos (parseState: IParseState) productionNumber = parseState.InputEndPosition productionNumber

let posRangeExt (parseState: IParseState) productionNumber1 productionNumber2 = (startPos parseState productionNumber1, endPos parseState productionNumber2)