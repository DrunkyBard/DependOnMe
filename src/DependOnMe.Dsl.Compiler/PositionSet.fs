namespace Navigation

open System
open Positioning
open DslAst
open System.Collections.Generic
open TextUtilities
open Microsoft.FSharp.Text.Lexing

type PositionSet() = 
    let positions = ResizeArray<PositionIndex>() //TODO: need uniqueness support

    let s = SortedSet<PosRange>()

    let (|Earlier|Later|Intersection|Same|Inside|) (s1: Position, e1: Position, s2: Position, e2: Position) =
        if   s1.AbsoluteOffset = s2.AbsoluteOffset && e1.AbsoluteOffset = e2.AbsoluteOffset then Same
        elif s2.AbsoluteOffset < s1.AbsoluteOffset && e1.AbsoluteOffset < e2.AbsoluteOffset then Inside
        elif e1.AbsoluteOffset < s2.AbsoluteOffset then Earlier
        elif e1.AbsoluteOffset > s2.AbsoluteOffset then Later
        else Intersection

    let (|PrevSame|NextSame|BothSame|NewLines|) (prevEnd: Position, midStart: Position, midEnd: Position, nextStart: Position) = 
        if   prevEnd.Line = midStart.Line && midEnd.Line <> nextStart.Line then PrevSame
        elif prevEnd.Line <> midStart.Line && midEnd.Line = nextStart.Line then NextSame
        elif prevEnd.Line = midStart.Line && midEnd.Line = nextStart.Line then BothSame
        else NewLines
        
    let (|SameLine|DifferentLines|) (pos1: Position, pos2: Position) = 
        if pos1.Line = pos2.Line then SameLine
        else DifferentLines

    member __.Insert(idx: PositionIndex) = positions.Add(idx)

    member __.Find((startPos, endPos): PosRange) = 
        let sortedPositions = Array.sortWith (fun i j -> PosIndexComparer.Instance.Compare(i, j)) (positions.ToArray())
        
        let matchPosition mid =
            match mid with                
                | mid when mid = 0 && sortedPositions.Length = 0 -> 
                    let srcStart, srcEnd = sortedPositions.[mid].Range
                    ({srcStart with pos_cnum = Int32.MinValue}, {srcEnd with pos_cnum = Int32.MinValue})
                | mid when mid = 0 -> 
                    let midStart, midEnd = sortedPositions.[mid].Range
                    let nextStart, _ = sortedPositions.[mid+1].Range

                    match midEnd, nextStart with
                        | SameLine       -> ({midStart with pos_cnum = Int32.MinValue}, {nextStart with pos_cnum = nextStart.Column - 1})
                        | DifferentLines -> ({midStart with pos_cnum = Int32.MinValue}, {midEnd with pos_cnum = Int32.MaxValue})
                | mid when mid = sortedPositions.Length - 1 -> 
                    let midStart, midEnd = sortedPositions.[mid].Range
                    let _, prevEnd = sortedPositions.[mid-1].Range
                    
                    match midStart, prevEnd with
                        | SameLine       -> ({prevEnd with pos_cnum = prevEnd.Column + 1}, {midEnd with pos_cnum = Int32.MaxValue})
                        | DifferentLines -> ({midStart with pos_cnum = Int32.MinValue}, {midEnd with pos_cnum = Int32.MaxValue})
                | mid -> 
                    let midStart, midEnd = sortedPositions.[mid].Range
                    let _, prevEnd = sortedPositions.[mid-1].Range
                    let nextStart, _ = sortedPositions.[mid+1].Range

                    match prevEnd, midStart, midEnd, nextStart with
                        | PrevSame -> (midStart, {midEnd with pos_cnum = Int32.MaxValue})
                        | NextSame -> ({midStart with pos_cnum = Int32.MinValue}, midEnd)
                        | BothSame -> (midStart, {nextStart with pos_cnum = nextStart.Column - 1})
                        | NewLines -> ({midStart with pos_cnum = Int32.MinValue}, {midEnd with pos_cnum = Int32.MaxValue})
        
        let rec findInternal min max =
            let mid = (min+max)/2
            let newStart, newEnd = matchPosition mid

            match startPos, endPos, newStart, newEnd with
                | Earlier       -> findInternal min mid
                | Later         -> findInternal mid max
                | Intersection  -> failwith "INTERSECTION"
                | Same | Inside -> sortedPositions.[mid].Term

        findInternal 0 (sortedPositions.Length - 1)
