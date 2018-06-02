module Errors

open Microsoft.FSharp.Text.Lexing
open TextUtilities

[<Struct>]
type ProdError(startPos: Position, endPos: Position, message: string) = 
    member __.StartPos = startPos
    
    member __.EndPos = endPos
    
    member __.Message = message

    member __.PosRange with get() = (startPos, endPos) |> PosRange
[<Struct>]
type TermError(pos: Position, message: string) =
    member __.Pos = pos

    member __.Message = message

type CompilationError = 
    | Range of ProdError
    | Point of TermError

type ErrorLogger() =
    let diagnostics = ResizeArray<CompilationError>()

    member __.Report error = diagnostics.Add error

    member __.Diagnostics with get() = diagnostics.ToArray() |> List.ofArray

    member __.Flush(out: CompilationError -> unit) =
        let rec flushRec = function
            | i when i = diagnostics.Count -> ()
            | i -> (diagnostics.[i]) |> out
        
        flushRec 0

let reportRange ((startPos, endPos): PosRange) msg (logger: ErrorLogger) = (startPos, endPos, msg) |> ProdError |> Range |> logger.Report

let reportPoint pos msg (logger: ErrorLogger) = (pos, msg) |> TermError |> Point |> logger.Report 