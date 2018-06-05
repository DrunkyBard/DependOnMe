module SuggestionText

type Suggestion = 
    | One  of string
    | Many of string list

let allBody = Many([
                    "BoolFlag1";
                    "BoolFlag2";
                    "Module";
                    "\"DEPENDENCY_NAME\"";
                  ])

let testHeader = One("TestHeader")

let boolValue = Many(["TRUE"; "FALSE"])

let boolTerm = Many(["BoolFlag1"; "BoolFlag2"])

let arrow = One("->")

let equal = One("=")

let testName = Many([ "\"TEST_NAME\""; "TEST_NAME" ])

