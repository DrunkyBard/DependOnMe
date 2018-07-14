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

type internal RefItem = 
    {
        FilePath: string;
        Name: string;
    }

type internal RefRecord = 
    {
        Declaration:  RefItem;
        References:   Dictionary<RefItem, int>;
        Duplications: Dictionary<RefItem, int>;
    }

type internal Rec =
    static member From name (fileUnit: 'a FileCompilationUnit) = { FilePath = fileUnit.FilePath; Name = name; }

type internal RefItemComparer private() =
    static member Instance = RefItemComparer() :> IEqualityComparer<RefItem>

    interface IEqualityComparer<RefItem> with
        member __.Equals(x, y)   = StringComparer.OrdinalIgnoreCase.Equals(x.FilePath, y.FilePath) && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name)
        member __.GetHashCode(x) = StringComparer.OrdinalIgnoreCase.GetHashCode(x.FilePath) ^^^ StringComparer.OrdinalIgnoreCase.GetHashCode(x.Name)

type RefTable private() =    
    let table       = Dictionary<string, RefRecord>(StringComparer.OrdinalIgnoreCase)                // key - module name
    let invertTable = Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)          // key - file path
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
                    | (true, refs) -> refs.AddOrUpdate(reference, 1, fun _ v -> v + 1)
                    | (false, _)   -> 
                        let orphans = Dictionary<RefItem, int>(RefItemComparer.Instance)
                        orphans.Add(reference, 1)
                        orphanRefs.Add(moduleName, orphans)
            | (true, prevRef) -> prevRef.References.AddOrUpdate(reference, 1, fun _ v -> v + 1)
            
    let removeRefInternal moduleName reference =
        match table.TryGetValue moduleName with
            | (false, _) -> 
                match orphanRefs.TryGetValue(moduleName) with
                    | (true, refs) -> refs.Remove(reference) |> ignore
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
        let remItem = Rec.From unitModuleName fUnit

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
                if reference.References.Count > 0 then orphanRefs.Add(refModuleName, reference.References)
                
                let success = table.Remove(refModuleName)
                Debug.Assert(success)
                
                true
        else false
        
    static let inst = RefTable()

    static member Instance = inst

    member __.AddRef(reference: FileCompilationUnit<DependencyTest>) = 
        match reference.CompilationUnit with
            | DependencyTest.Empty -> ()
            | Test(TestHeader.Full(name, _, _), _, _, _, _, _) -> (name, (name, reference) ||> Rec.From) ||> addRefInternal
            | _ -> failwith "Expected only empty or full test declaration as reference"
     
    member __.RemoveRef(reference: FileCompilationUnit<DependencyTest>) =
        match reference.CompilationUnit with
            | DependencyTest.Empty -> ()
            | Test(TestHeader.Full(name, _, _), _, _, _, _, _) -> (name, (name, reference) ||> Rec.From) ||> removeRefInternal
            | _ -> failwith "Expected only empty or full test declaration as reference"

    member __.AddDeclaration(declaration: FileCompilationUnit<ValidModuleRegistration>) =
        let moduleName = declaration.CompilationUnit.Name
        let recItem    = Rec.From moduleName declaration
        let fPath      = declaration.FilePath

        if invertTable.ContainsKey(declaration.FilePath) then
            let hashSet = invertTable.[fPath]
            hashSet.Add(moduleName) |> ignore
        else
            let hashSet = HashSet<string>(StringComparer.OrdinalIgnoreCase)
            hashSet.Add(moduleName) |> ignore
            invertTable.Add(fPath, hashSet) |> ignore

        match table.TryGetValue moduleName with
            | (false, _)      -> 
                match orphanRefs.TryGetValue(moduleName) with
                    | (true, refs) -> table.Add(moduleName, (recItem, refs) ||> newRecordRefs)
                    | (false, _)   -> table.Add(moduleName, recItem |> newRecord)
                orphanRefs.Remove(moduleName) |> ignore
            | (true, prevRef) -> prevRef.Duplications.AddOrUpdate(recItem, 1, fun _ v -> v + 1)

    member __.RemoveDeclaration(moduleName: string) =
        match table.TryGetValue moduleName with
            | (false, _)      -> failwithf "Module '%A' does not exist" moduleName
            | (true, prevRef) -> removeDeclarationInternal moduleName prevRef
            
    member __.TryRemoveDeclaration(declaration: FileCompilationUnit<string>) =
        let moduleName = declaration.CompilationUnit
        let filePath   = declaration.FilePath

        if invertTable.ContainsKey(filePath) then
            let declaredModules = invertTable.[filePath]

            if declaredModules.Remove(moduleName) then
                if declaredModules.Count = 0 then invertTable.Remove(filePath) |> ignore 
                else ()
                
                table.[moduleName] |> removeFileDeclarationInternal declaration //TODO: удалить по имени файла также
            else false
        else false

        //match table.TryGetValue moduleName with
        //    | (false, _)      -> ()
        //    | (true, prevRef) ->  prevRef

    member __.TryRemoveDeclarations(filePath: string) =
        match invertTable.TryGetValue(filePath) with
            | (true, declarations) -> 
                declarations.ToArray() 
                |> Array.iter (fun decl -> { FilePath = filePath; CompilationUnit = decl; } |> __.TryRemoveDeclaration |> ignore )
            | (false, _) -> ()

    member __.HasDefinition(moduleName) = table.ContainsKey(moduleName)

    member __.HasDuplicates(moduleName) = __.HasDefinition(moduleName) && table.[moduleName].Duplications.Count > 0

    member __.HasWithoutDuplicates(moduleName) = __.HasDefinition(moduleName) && table.[moduleName].Duplications.Count = 0

