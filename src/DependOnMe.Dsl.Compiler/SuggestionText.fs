module SuggestionText

type Suggestion = 
    | One  of string
    | Many of string list
    | None

let depNameStr = "\"DependencyName\""

let depName = One depNameStr

let implName = One "\"ImplementationName\""

let moduleName = One "\"ModuleName\""

let testHeader = One "TestHeader"

let boolValue = Many ["TRUE"; "FALSE"]

let boolTerm = Many ["BoolFlag1"; "BoolFlag2"]

let arrow = One "->"

let equal = One "="

let testName = Many [ "\"TEST_NAME\""; "TEST_NAME" ]

let allBody = Many [
                    "BoolFlag1";
                    "BoolFlag2";
                    "Module";
                    depNameStr
                   ]