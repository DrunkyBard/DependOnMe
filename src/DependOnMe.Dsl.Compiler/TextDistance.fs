module TextDistance

open System

let sameRadius (s1: string) (s2: string) = Math.Max(s1.Length, s2.Length) / 2 - 1

let isInRange i j rad = abs(i-j) <= rad

let commonChars (s1: string) (s2: string) (radius: int) =
    let rec charContains (c: char) (cIdx: int) (s: string) (sIdx: int) (toIdx: int) (rad: int) = 
        if isInRange cIdx sIdx rad && sIdx <= toIdx then
            if s.[sIdx] = c then true
            else charContains c cIdx s (sIdx + 1) toIdx rad
        else false
    
    let common = ResizeArray()

    for i = 0 to s1.Length - 1 do
        let c = s1.[i]
        let fromIdx = Math.Max(0, i-radius)
        let toIdx = Math.Min(s2.Length-1, i+radius)

        if charContains c i s2 fromIdx toIdx radius then common.Add(c)
        else ()
        
    common

let transpositions c1 c2 = 
    let rec transpositionsInternal (c1: ResizeArray<char>) (c2: ResizeArray<char>) toIdx idx acc = 
        if idx > toIdx then (acc + abs(c1.Count - c2.Count)) / 2
        elif c1.[idx] = c2.[idx] then transpositionsInternal c1 c2 toIdx (idx + 1) acc
        else transpositionsInternal c1 c2 toIdx (idx + 1) (acc+1)
    
    transpositionsInternal c1 c2 (Math.Min(c1.Count-1, c2.Count-1)) 0 0


let jaroDistance (s1: string) (s2: string) =
    let radius = sameRadius s1 s2
    
    let common1 = commonChars s1 s2 radius
    let common2 = commonChars s2 s1 radius
    let m1 = float common1.Count
    let m2 = float common2.Count
    let s1L = float s1.Length
    let s2L = float s2.Length
    let mL = Math.Max(m1, m2)
    let t = float(transpositions common1 common2)

    (m1/s1L + m2/s2L + (mL - t)/mL) / 3.0
    
let jaroWinklerDistance (s1: string) (s2: string) = 
    let rec computeCommonPrefix idx toIdx l = 
        if l = idx && idx <= toIdx && s1.[idx] = s2.[idx] then computeCommonPrefix (idx+1) toIdx (l+1)
        else l

    let toIdx = Math.Min(Math.Min(s1.Length, s2.Length), 4)
    let l = computeCommonPrefix 0 toIdx 0
    let jaroDistance = jaroDistance s1 s2
    let p = 0.1
    let l = float(l)
    
    jaroDistance + (l * p * (1.0 - jaroDistance))