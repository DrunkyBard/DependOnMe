module Extensions

open System.Runtime.CompilerServices
open System.Collections.Generic

[<Extension>]
type DictExtensions =

    [<Extension>]
    static member Insert<'k, 'v>(d: SortedDictionary<'k, 'v>, key, value) = d.[key] <- value