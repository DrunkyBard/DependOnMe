namespace Compilation

open CompilationUnit
open System.Collections.Generic
open System
open ModuleDslAst
open TestDslAst
open Common
open System.Linq

type internal RefItem = 
    {
        FilePath: string;
        Name: string;
    }

type internal RefRecord = 
    {
        Declaration:  RefItem;
        References:   HashSet<RefItem>;
        Duplications: HashSet<RefItem>;
    }

type internal Rec =
    static member From name (fileUnit: 'a FileCompilationUnit) = { FilePath = fileUnit.FilePath; Name = name; }

type internal RefItemComparer private() =
    static member Instance = RefItemComparer() :> IEqualityComparer<RefItem>

    interface IEqualityComparer<RefItem> with
        member __.Equals(x, y)   = StringComparer.OrdinalIgnoreCase.Equals(x.FilePath, y.FilePath) && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name)
        member __.GetHashCode(x) = StringComparer.OrdinalIgnoreCase.GetHashCode(x.FilePath) ^^^ StringComparer.OrdinalIgnoreCase.GetHashCode(x.Name)

type RefTable private() =
    let table = Dictionary<string, RefRecord>(StringComparer.OrdinalIgnoreCase)
    
    let newRecord declaration =
        {
            Declaration  = declaration;
            References   = HashSet<RefItem>(RefItemComparer.Instance);
            Duplications = HashSet<RefItem>(RefItemComparer.Instance);
        }

    let addRefInternal moduleName reference =
        match table.TryGetValue moduleName with
            | (false, _) -> failwithf "Module '%A' does not exist" moduleName
            | (true, prevRef) -> prevRef.References.AddOrUpdate(reference)
            
    let removeRefInternal moduleName reference =
        match table.TryGetValue moduleName with
            | (false, _) -> failwithf "Module '%A' does not exist" moduleName
            | (true, prevRef) -> prevRef.References.Remove(reference) |> ignore

    let removeDeclarationInternal moduleName (reference: RefRecord) =
        match reference.Duplications.Count with
            | count when count > 0 -> 
                let newDeclaration = reference.Duplications.First()
                reference.Duplications.Remove(newDeclaration) |> ignore
                table.[moduleName] <- { reference with Declaration = newDeclaration; }
            | _ ->
                table.Remove(moduleName) |> ignore

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

    member __.AddDeclaration(moduleName: string, declaration: FileCompilationUnit<ModuleDeclaration>) =
        match table.TryGetValue moduleName with
            | (false, _)      -> table.Add(moduleName, Rec.From moduleName declaration |> newRecord)
            | (true, prevRef) -> prevRef.Duplications.AddOrUpdate(Rec.From moduleName declaration)

    member __.RemoveDeclaration(moduleName: string) =
        match table.TryGetValue moduleName with
            | (false, _)      -> failwithf "Module '%A' does not exist" moduleName
            | (true, prevRef) -> removeDeclarationInternal moduleName prevRef

    member __.HasDefinition(moduleName) = table.ContainsKey(moduleName)

    member __.HasDuplicates(moduleName) = __.HasDefinition(moduleName) && table.[moduleName].Duplications.Count > 0
