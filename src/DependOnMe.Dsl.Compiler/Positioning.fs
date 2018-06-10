module Positioning

open Microsoft.FSharp.Text.Lexing
open System.Runtime.CompilerServices
open DataStructures
open DslAst
open TextUtilities
open System.Collections.Generic

let (|Earlier|Later|Intersection|Same|Inside|) (s1: Position, e1: Position, s2: Position, e2: Position) =
        if   s1.AbsoluteOffset = s2.AbsoluteOffset && e1.AbsoluteOffset = e2.AbsoluteOffset then Same
        elif s2.AbsoluteOffset < s1.AbsoluteOffset && e1.AbsoluteOffset < e2.AbsoluteOffset then Inside
        elif e1.AbsoluteOffset < s2.AbsoluteOffset then Earlier
        elif e1.AbsoluteOffset > s2.AbsoluteOffset then Later
        else Intersection

type PosRangeComparer() =

    static member Instance = PosRangeComparer()

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
                | Intersection -> failwith "Ranges intersection"
                | Inside -> failwith "Range lies inside"

type PositionComparer() =

    static member Instance = PositionComparer() :> IComparer<Position>

    interface IComparer<Position> with
        member __.Compare(pos1, pos2) =
            match (pos1, pos1, pos2, pos2) with 
                | Earlier -> -1
                | Same    -> 0
                | Later   -> 1
                | Intersection -> failwith "Ranges intersection"
                | Inside -> failwith "Range lies inside"
                
type PosIndexComparer() =

    static member Instance = PosIndexComparer() :> IComparer<PositionIndex>

    interface IComparer<PositionIndex> with
        member __.Compare(idx1, idx2) =
            let startPos1, endPos1 = idx1.Range
            let startPos2, endPos2 = idx2.Range

            if startPos1.Line > endPos1.Line then failwith "StartPos1.Line is greater than EndPos1.Line"
            if startPos1.Line = endPos1.Line && startPos1.Column > endPos1.Column then failwith "StartPos1.Line and EndPos1.Line are equal, but StartPos1.Column is greater than EndPos1.Column"
            if startPos2.Line > endPos2.Line then failwith "StartPos2.Line is greater than EndPos2.Line"
            if startPos2.Line = endPos2.Line && startPos2.Column > endPos2.Column then failwith "StartPos2.Line and EndPos2.Line are equal, but StartPos2.Column is greater than EndPos2.Column"
            
            match (startPos1, endPos1, startPos2, endPos2) with 
                | Earlier -> -1
                | Same    -> 0
                | Later   -> 1
                | Intersection -> failwith "Ranges intersection"
                | Inside -> failwith "Range lies inside"

[<Extension>]
type RbTreeExtensions =

    [<Extension>]
    static member TryFindTermPosition(tree: RedBlackTree<PosRange, IndexTerm>, (startPos: Position, endPos: Position): PosRange) = 
        let rec findInternal node =
            match node with
            | E -> None
            | T(_, l, r, ((nodeStartPos, nodeEndPos), term)) -> 
                match (startPos, endPos, nodeStartPos, nodeEndPos) with 
                    | Earlier       -> findInternal l
                    | Later         -> findInternal r
                    | Same | Inside -> Some term
                    | Intersection  -> failwith "Position intersection"

        findInternal tree.Root

    [<Extension>]
    static member TryFindTermPosition(tree: RedBlackTree<PosRange, IndexTerm>, position: Position) = tree.TryFindTermPosition((position, position))

