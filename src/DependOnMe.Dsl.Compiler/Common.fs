﻿module Common

open System.Collections.Generic

let (||||>) (x1, x2, x3, x4) f = f x1 x2 x3 x4

let (|||||>) (x1, x2, x3, x4, x5) f = f x1 x2 x3 x4 x5

let id x = x

let thrd (_, _, x) = x

let frth (_, _, _, x) = x

type 'a HashSet with
    member this.AddOrUpdate(item: 'a) = 
        this.Remove(item) |> ignore
        this.Add(item)    |> ignore

type ('a, 'b) Dictionary with
    member dict.AddOrUpdate(key: 'a, value: 'b, update: 'a -> 'b -> 'b) =
        match dict.TryGetValue(key) with
            | (true, v)  -> dict.[key] <- update key v
            | (false, _) -> dict.Add(key, value)
    
    member dict.AddOrUpdateFunc(key: 'a, value: 'a -> 'b, update: 'a -> 'b -> 'b) =
        match dict.TryGetValue(key) with
            | (true, v)  -> dict.[key] <- update key v
            | (false, _) -> dict.Add(key, value(key))