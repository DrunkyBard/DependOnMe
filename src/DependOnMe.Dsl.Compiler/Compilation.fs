module Compilation

open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open TextUtilities

type PositionComparer() =
    let (|Earlier|Later|Intersection|Same|) (s1: Position, e1: Position, s2: Position, e2: Position) =
        if   s1.AbsoluteOffset = s2.AbsoluteOffset && e1.AbsoluteOffset = e2.AbsoluteOffset then Same
        elif e1.AbsoluteOffset < s2.AbsoluteOffset then Earlier
        elif e1.AbsoluteOffset > s2.AbsoluteOffset then Later
        else Intersection

    static member Instance = PositionComparer()

    interface IComparer<PosRange> with
        member __.Compare((startPos1, endPos1), (startPos2, endPos2)) =
            if startPos1.Line > endPos1.Line then failwith "StartPos1.Line is greater than EndPos1.Line"
            if startPos1.Line = endPos1.Line && startPos1.Column > endPos1.Column then failwith "StartPos1.Line and EndPos1.Line are equal, but StartPos1.Column is greater than EndPos1.Column"
            if startPos2.Line > endPos2.Line then failwith "StartPos2.Line is greater than EndPos2.Line"
            if startPos2.Line = endPos2.Line && startPos2.Column > endPos2.Column then failwith "StartPos2.Line and EndPos2.Line are equal, but StartPos2.Column is greater than EndPos2.Column"
            
            match (startPos1, endPos1, startPos2, endPos2) with 
                | Earlier -> -1
                | Same    -> 0
                | Later   -> 1
                | Intersection -> failwith "Ranges intersection."

type IntComparer() =
    static member Instance = PositionComparer()

    interface IComparer<int> with
        member __.Compare(x, y) =
            if x > y then 1
            elif x < y then -1
            else 0
            
