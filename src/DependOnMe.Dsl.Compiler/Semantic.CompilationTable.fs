namespace CompilationTable

open TextUtilities
open TestDslAst
open Errors
open System.Collections.Concurrent
open CompilationUnit
open System

type CompilationUnitTable private() =
    let testUnits   = ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    let moduleUnits = ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    
    let add (globalUnits: ConcurrentDictionary<string, int>) name =
        globalUnits.AddOrUpdate(name, (fun _ -> 1), (fun _ count -> count + 1)) |> ignore

    let hasDuplicates (globalUnits: ConcurrentDictionary<string, int>) name =
        match globalUnits.TryGetValue(name) with
            | (true, count) when count > 0 -> count > 1
            | (false, _)    -> false
            | (true, count) -> failwithf "Broken invariant: reference count should be greater than zero. Current count: %A" count

    let tryRemove key (dict: ConcurrentDictionary<string, int>) =
        let rec removeRec curVal =
            if dict.TryUpdate(key, curVal - 1, curVal) then
                if curVal - 1 = 0 then dict.TryRemove(key) |> ignore
            else 
                match dict.TryGetValue(key) with
                    | (true, value) -> removeRec value
                    | (false, _)    -> failwithf "Broken invariant: trying to remove key: %A" key
        
        match dict.TryGetValue(key) with
                    | (true, value) -> removeRec value
                    | (false, _)    -> failwithf "Broken invariant: trying to remove key: %A" key

    static let inst = CompilationUnitTable()

    static member Instance = inst

    member __.AddTest(testName: string)           = add testUnits testName
    
    member __.AddModule(moduleName: string)       = add moduleUnits moduleName

    member __.RemoveTest(testName: string)        = tryRemove testName testUnits
                    
    member __.RemoveModule(moduleName: string)    = tryRemove moduleName moduleUnits

    member __.IsTestDefined(testName: string)     = hasDuplicates testUnits testName
            
    member __.IsModuleDefined(moduleName: string) = hasDuplicates moduleUnits moduleName