module Common

let (||||>) (x1, x2, x3, x4) f = f x1 x2 x3 x4

let id x = x

let thrd (_, _, x) = x

let frth (_, _, _, x) = x