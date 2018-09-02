namespace Compilation

open CompilationUnit
open System.Collections.Generic
open System
open ModuleDslAst
open TestDslAst
open Common
open System.Linq
open DslAst.Extension
open System.Diagnostics
open System.Numerics

type RefType = | ModuleRef | TestRef  

type internal RefItem = 
    {
        FilePath: string;
        Name: string;
        Type: RefType
    }

type internal RefRecord = 
    {
        Declaration:  RefItem;
        References:   Dictionary<RefItem, int>;
        Duplications: Dictionary<RefItem, int>;
    }

type internal Rec =
    static member FromModule name filePath = { FilePath = filePath; Name = name; Type = ModuleRef }
    static member FromTest name filePath   = { FilePath = filePath; Name = name; Type = TestRef }

type internal RefItemComparer private() =
    static member Instance = RefItemComparer() :> IEqualityComparer<RefItem>

    interface IEqualityComparer<RefItem> with
        member __.Equals(x, y)   = StringComparer.OrdinalIgnoreCase.Equals(x.FilePath, y.FilePath) && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name)
        member __.GetHashCode(x) = StringComparer.OrdinalIgnoreCase.GetHashCode(x.FilePath) ^^^ StringComparer.OrdinalIgnoreCase.GetHashCode(x.Name)

type RefTable private() =    
    let table       = Dictionary<string, RefRecord>(StringComparer.OrdinalIgnoreCase)                // key - module name
    let invertTable = Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase)  // key - file path
    let orphanRefs  = Dictionary<string, Dictionary<RefItem, int>>(StringComparer.OrdinalIgnoreCase) // key - module name

    let TryDecrease (dict: Dictionary<RefItem, int>) key =
        match dict.TryGetValue(key) with
            | (true, count) when count = 0 -> dict.Remove(key)
            | (true, count) -> dict.[key] <- count - 1; true
            | _ -> false
    
    let newRecord declaration =
        {
            Declaration  = declaration;
            References   = Dictionary<RefItem, int>(RefItemComparer.Instance);
            Duplications = Dictionary<RefItem, int>(RefItemComparer.Instance);
        }
        
    let newRecordRefs declaration refs =
        {
            Declaration  = declaration;
            References   = refs;
            Duplications = Dictionary<RefItem, int>(RefItemComparer.Instance);
        }

    let addRefInternal moduleName reference =
        match table.TryGetValue moduleName with
            | (false, _) -> 
                match orphanRefs.TryGetValue(moduleName) with
                    | (true, refs) -> 
                        refs.AddOrUpdate(reference, 1, fun _ v -> v + 1)
                    | (false, _)   -> 
                        let orphans = Dictionary<RefItem, int>(RefItemComparer.Instance)
                        orphans.Add(reference, 1)
                        orphanRefs.Add(moduleName, orphans)
            | (true, prevRef) -> prevRef.References.AddOrUpdate(reference, 1, fun _ v -> v + 1)
            
    let removeRefInternal moduleName reference =
        match table.TryGetValue moduleName with
            | (false, _) -> 
                match orphanRefs.TryGetValue(moduleName) with
                    | (true, refs) -> 
                        refs.Remove(reference) |> ignore

                        if refs.Count = 0 then orphanRefs.Remove(moduleName) |> ignore
                    | (false, _)   -> ()
            | (true, prevRef) ->
                prevRef.References.Remove(reference) |> ignore

    let removeDeclarationInternal moduleName (reference: RefRecord) =
        match reference.Duplications.Count with
            | count when count > 0 -> 
                let newDeclaration = reference.Duplications.First().Key
                let success = TryDecrease reference.Duplications newDeclaration
                Debug.Assert(success)
                reference.Duplications.Remove(newDeclaration) |> ignore
                table.[moduleName] <- { reference with Declaration = newDeclaration; }
            | _ ->
                orphanRefs.Add(moduleName, reference.References)
                table.Remove(moduleName) |> ignore
                
    let removeFileDeclarationInternal (fUnit: string FileCompilationUnit) (reference: RefRecord) =
        let refFilePath    = reference.Declaration.FilePath
        let refModuleName  = reference.Declaration.Name
        let unitFilePath   = fUnit.FilePath
        let unitModuleName = fUnit.CompilationUnit
        let remItem = Rec.FromModule unitModuleName fUnit.FilePath

        reference.Duplications.Remove(remItem) |> ignore

        if refFilePath.Equals(unitFilePath, StringComparison.OrdinalIgnoreCase) && refModuleName.Equals(unitModuleName, StringComparison.OrdinalIgnoreCase) then
            match reference.Duplications.Count with
            | count when count > 0 -> 
                let newDeclaration = reference.Duplications.First().Key
                let success = TryDecrease reference.Duplications newDeclaration
                Debug.Assert(success)
                table.[refModuleName] <- { reference with Declaration = newDeclaration; }

                true
            | _ ->
                if reference.References.Count > 0 then 
                    let withoutCurrentFileRefs = reference.References.Where(fun x -> x.Key.FilePath.Equals(refFilePath, StringComparison.OrdinalIgnoreCase) |> not)
                    let refDict = withoutCurrentFileRefs.ToDictionary((fun x -> x.Key), (fun x -> x.Value), RefItemComparer.Instance)
                    orphanRefs.Add(refModuleName, refDict)
                
                let success = table.Remove(refModuleName)
                Debug.Assert(success)
                
                true
        else false
        
    static let inst = RefTable()

    static member Instance = inst

    member __.AddRef(reference: FileCompilationUnit<DependencyTest>) = 
        match reference.CompilationUnit with
            | DependencyTest.Empty -> ()
            | Test(TestHeader.Full(name, _, _), _, _, _, moduleRegs, _) -> 
                let ref = (name, reference.FilePath) ||> Rec.FromTest
                moduleRegs |> Seq.iter(fun x -> addRefInternal x.Name ref)
            | _ -> failwith "Expected only empty or full test declaration as reference"
     
    //member __.RemoveRef(reference: FileCompilationUnit<DependencyTest>) =
    //    match reference.CompilationUnit with
    //        | DependencyTest.Empty -> ()
    //        | Test(TestHeader.Full(name, _, _), _, _, _, _, _) -> (name, (name, reference.FilePath) ||> Rec.FromTest) ||> removeRefInternal
    //        | _ -> failwith "Expected only empty or full test declaration as reference"

    member __.GetAllModulesFrom(file: string) = 
        match invertTable.TryGetValue(file) with
            | (true, declarations) -> declarations.Keys.ToList().AsReadOnly()
            | (false, _) -> Seq.empty.ToList().AsReadOnly()

    member __.AddDeclaration(declaration: FileCompilationUnit<ValidModuleRegistration>) =
        let moduleName = declaration.CompilationUnit.Name
        let recItem    = Rec.FromModule moduleName declaration.FilePath
        let fPath      = declaration.FilePath

        if invertTable.ContainsKey(declaration.FilePath) then
            let moduleDict = invertTable.[fPath]
            moduleDict.AddOrUpdate(moduleName, 1, fun _ count -> count + 1) |> ignore
        else
            let moduleDict = Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            moduleDict.Add(moduleName, 1) |> ignore
            invertTable.Add(fPath, moduleDict) |> ignore

        match table.TryGetValue moduleName with
            | (false, _)      -> 
                match orphanRefs.TryGetValue(moduleName) with
                    | (true, refs) -> 
                        let moduleOrphans = 
                            refs
                                .Select(fun (ref: KeyValuePair<RefItem, int>) -> 
                                    match ref.Key.Type with
                                        | TestRef -> Some ref
                                        | ModuleRef when table.ContainsKey(ref.Key.Name) -> Some ref
                                        | ModuleRef -> None)
                                .Where(fun s -> s.IsSome)
                                .ToDictionary((fun s -> s.Value.Key), (fun s -> s.Value.Value), RefItemComparer.Instance)
                                
                        table.Add(moduleName, (recItem, moduleOrphans) ||> newRecordRefs)
                    | (false, _)   -> table.Add(moduleName, recItem |> newRecord)
                orphanRefs.Remove(moduleName) |> ignore
            | (true, prevRef) -> prevRef.Duplications.AddOrUpdate(recItem, 1, fun _ v -> v + 1)

        declaration.CompilationUnit.ModuleRegistrations
            |> Array.iter (fun mReg -> addRefInternal mReg.Name recItem)

    member __.RemoveDeclaration(moduleName: string) =
        match table.TryGetValue moduleName with
            | (false, _)      -> failwithf "Module '%A' does not exist" moduleName
            | (true, prevRef) -> removeDeclarationInternal moduleName prevRef
            
    member __.TryRemoveDeclaration(declaration: FileCompilationUnit<string>) =
        let moduleName = declaration.CompilationUnit
        let filePath   = declaration.FilePath

        orphanRefs.Keys
        |> Seq.iter(fun modName -> 
            let refs = orphanRefs.[modName]
            refs.Keys 
                    |> Seq.where (fun x -> x.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)) 
                    |> Array.ofSeq
                    |> Array.iter (fun rem -> refs.Remove(rem) |> ignore)

            if refs.Count = 0 then orphanRefs.Remove(moduleName) |> ignore)

        if invertTable.ContainsKey(filePath) then
            let declaredModules = invertTable.[filePath]

            match declaredModules.TryGetValue(moduleName) with
                | (true, _) -> 
                    declaredModules.Remove(moduleName) |> ignore
                    if declaredModules.Count = 0 then invertTable.Remove(filePath) |> ignore 
                    table.[moduleName] |> removeFileDeclarationInternal declaration
                | (false, _)    -> false
        else false

    member __.TryRemoveTestRefs(filePath: string) =
        let testsToRemove = 
            table.Values 
            |> Seq.collect(fun x -> x.References) 
            |> Seq.where(fun x -> x.Key.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)) 
            |> Seq.map(fun x -> x.Key)
            |> Array.ofSeq

        if testsToRemove.Length > 0 then
            let orphModules  = orphanRefs |> Seq.map (fun x -> x.Key) |> Array.ofSeq
            let tableModules = table |> Seq.map (fun x -> x.Key)  |> Array.ofSeq
            testsToRemove |> Seq.iter(fun testRef -> orphModules  |> Seq.iter(fun orph -> removeRefInternal orph testRef))
            testsToRemove |> Seq.iter(fun testRef -> tableModules |> Seq.iter(fun tRecord -> removeRefInternal tRecord testRef))
        else 
            let moduleToRefs = 
                orphanRefs
                |> Seq.where(fun kvp -> kvp.Value.Any(fun p -> p.Key.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                |> Seq.map(fun kvp -> (kvp.Key, kvp.Value |> Seq.where(fun kvp1 -> kvp1.Key.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))))
                |> Seq.map(fun (moduleName, v) -> (moduleName, v |> Seq.map(fun q -> q.Key) |> Array.ofSeq))
                |> Array.ofSeq
            
            moduleToRefs |> Seq.iter(fun (moduleName, refs) -> refs |> Array.iter(fun ref -> removeRefInternal moduleName ref))

    member __.TryRemoveDeclarations(filePath: string) =
        match invertTable.TryGetValue(filePath) with
            | (true, declarations) -> 
                declarations.ToArray() 
                |> Array.iter (fun decl -> { FilePath = filePath; CompilationUnit = decl.Key; } |> __.TryRemoveDeclaration |> ignore )
            | (false, _) -> ()

    member __.HasDefinition(moduleName) = table.ContainsKey(moduleName)

    member __.HasDuplicates(moduleName) = __.HasDefinition(moduleName) && table.[moduleName].Duplications.Count > 0

    member __.HasWithoutDuplicates(moduleName) = __.HasDefinition(moduleName) && table.[moduleName].Duplications.Count = 0

    member __.Clean() =
        table.Clear()
        orphanRefs.Clear()
        invertTable.Clear()

