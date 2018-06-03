#nowarn "62"
namespace DataStructures

open System.Collections.Generic
open Common

type color = Red | Black    

type ('a, 'b) tree =
    | E
    | T of color * ('a, 'b) tree * ('a, 'b) tree * ('a * 'b) // color X left X right X key*value
    
type RedBlackTree<'a, 'b>(comparer: IComparer<'a>) =
    let mutable root: tree<'a, 'b> = E

    let rec tryGetValue key = function
        | E -> None
        | T(_, l, r, (nodeKey, value)) ->
            match comparer.Compare(key, nodeKey) with
                | -1 -> tryGetValue key l
                | 0  -> Some value
                | 1  -> tryGetValue key r
                | _  -> failwith "Broken IComparer contract"

    let balance = function                              
        | Black, T(Red, T(Red, a, b, x), c, y), d, z    
        | Black, T(Red, a, T(Red, b, c, y), x), d, z            
        | Black, a, T(Red, T(Red, b, c, y), d, z), x            
        | Black, a, T(Red, b, T(Red, c, d, z), y), x            
            -> T(Red, T(Black, a, b, x), T(Black, c, d, z), y)
        | c, l, r, x -> T(c, l, r, x)

    member __.Root with get() = root

    member __.TryGetValue key = tryGetValue key root
    
    member __.Insert (key, value) = 
        let rec ins cont = function
            | E -> T(Red, E, E, (key, value)) |> cont
            | T(c, l, r, kv) ->
                match comparer.Compare(key, fst kv) with
                    | -1 -> ((fun n -> balance(c, n, r, kv)) >> cont, l) ||> ins
                    | 0  -> cont(T(c, l, r, (key, value)))
                    | 1  -> ((fun n -> balance(c, l, n, kv)) >> cont, r) ||> ins
                    | _  -> failwith "Broken IComparer contract"

        match ins id root with
            | E -> failwith "Should never return empty from an insert"
            | T(_, l, r, kv) -> root <- T(Black, l, r, kv)

    member __.Print() =
        let rec print (spaces : int) = function
            | E -> ()
            | T(c, l, r, kv) ->
                print (spaces + 4) r
                printfn "%s %A" (String.replicate spaces " ") kv
                print (spaces + 4) l
        
        print 0 root
    
