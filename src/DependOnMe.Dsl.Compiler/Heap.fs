module DataStructures

open System.Collections.Generic

type Heap<'a when 'a: equality>(comparer: IComparer<'a>) =
    let internalList = ResizeArray()

    let parent = function
        | 0 -> internalList.[0]
        | i -> internalList.[(i-1)/2]

    let nodeOpt = function
        | j when j < internalList.Count -> (j, Some(internalList.[j]))
        | _ -> (-1, None)

    let leftChild i = nodeOpt (i*2+1)
    let rightChild i = nodeOpt (i*2+2)
    let current i = internalList.[i]

    let swap i j =
        let t = internalList.[i]
        internalList.[i] <- internalList.[j]
        internalList.[j] <- t
        
    let increaseKey i node =
        if comparer.Compare(node, internalList.[i]) = -1 then failwith ""

        let rec increaseKeyInternal j =
            if j = 0 || comparer.Compare(parent j, current j) = 1 then ()
            else 
                let parentIdx = j/2
                swap parentIdx j
                increaseKeyInternal parentIdx

        internalList.[i] <- node
        increaseKeyInternal i

    let mutable size = 0

    member __.Size with get() = internalList.Count
    
    member __.Root with get() = if __.Size = 0 then None else Some(internalList.[0])

    member __.Insert key =
        let size = __.Size
        internalList.Add key
        increaseKey size key


