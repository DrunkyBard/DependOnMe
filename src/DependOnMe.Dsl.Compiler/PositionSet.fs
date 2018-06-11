namespace Navigation

open System
open Positioning
open DslAst
open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing

type CaretTermPosition =
    | Inside  of IndexTerm
    | Between of IndexTerm * IndexTerm

type PositionSet() = 
    let positions = ResizeArray<PositionIndex>() //TODO: need uniqueness support
                                                 //TODO: get rid of sorting during every method call

    let (|Earlier|Later|Inside|) (pos: Position, termStart: Position, termEnd: Position) =
        if   pos.Line < termStart.Line then Earlier
        elif pos.Line = termStart.Line && pos.Column < termStart.Column then Earlier
        elif pos.Line > termEnd.Line then Later
        elif termEnd.Line = pos.Line && pos.Column > termEnd.Column then Later
        else Inside

    let (|InsideFirst|Between|InsideSecond|) (s1: Position, e1: Position, pos: Position, s2: Position, e2: Position) = 
        match pos, s1, e1 with
            | Earlier -> failwith "Precondition failed: position cannot be earlier then term start position"
            | Inside  -> InsideFirst
            | Later   -> 
                match pos, s2, e2 with
                    | Earlier -> Between
                    | Later -> failwith "Precondition failed: position cannot be later then term end position"
                    | Inside -> InsideSecond

    member __.Insert(idx: PositionIndex) = positions.Add(idx)

    member __.Positions with get() = positions

    member __.Clear() = positions.Clear()

    member __.Find(pos) = //TODO: handle empty index
        let sortedPositions = Array.sortWith (fun i j -> PosIndexComparer.Instance.Compare(i, j)) (positions.ToArray()) 

        let choose (idx1: PositionIndex) (idx2: PositionIndex) =
            let s1, e1 = idx1.Range
            let s2, e2 = idx2.Range

            match s1, e1, pos, s2, e2 with
                | InsideFirst  -> CaretTermPosition.Inside(idx1.Term)
                | InsideSecond -> CaretTermPosition.Inside(idx2.Term)
                | Between      -> CaretTermPosition.Between(idx1.Term, idx2.Term)

        let rec matchPosition min max =

            let mid = (min + max)/2

            match mid with
                | mid when mid = 0 -> CaretTermPosition.Inside(sortedPositions.[0].Term)
                | mid when mid = sortedPositions.Length - 1 -> CaretTermPosition.Inside(sortedPositions.[mid].Term)
                | mid -> 
                    let midStart, _ = sortedPositions.[mid].Range
                    let _, nextEnd = sortedPositions.[mid+1].Range

                    match pos, midStart, nextEnd with
                        | Earlier -> matchPosition min (mid-1)
                        | Later   -> matchPosition (mid+1) max
                        | Inside  -> choose sortedPositions.[mid] sortedPositions.[mid+1]

        matchPosition 0 (sortedPositions.Length - 1)

    member __.RemoveBetween(startPos: Position, endPos: Position) =
        let sortedPositions = Array.sortWith (fun i j -> PosIndexComparer.Instance.Compare(i, j)) (positions.ToArray()) |> ResizeArray

        let rec findPosition min max pos =
            if min < 0 || max = sortedPositions.Count then None
            else 
                let mid = (min + max) / 2
                let termStart, termEnd = sortedPositions.[mid].Range
                
                match pos, termStart, termEnd with
                    | Earlier -> findPosition min (mid - 1) pos
                    | Later   -> findPosition (mid + 1) max pos
                    | Inside  -> Some mid
        
        let find = findPosition 0 (sortedPositions.Count-1)
        let fromIdx, toIdx = find startPos, find endPos

        match fromIdx, toIdx with 
                | None, None       -> ()
                | Some(i), None    -> sortedPositions.RemoveRange(i, sortedPositions.Count - i)
                | None, Some(i)    -> sortedPositions.RemoveRange(0, i)
                | Some(i), Some(j) -> sortedPositions.RemoveRange(i, j-i)
        
        positions.Clear()
        positions.AddRange(sortedPositions)
            