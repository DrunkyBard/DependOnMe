module SuggestionText

type Suggestion = 
    | One  of string
    | Many of string list
    | None

let depNameStr = "\"DependencyName\""

let depName    = One depNameStr

let implName   = One "\"ImplementationName\""

let moduleName = One "\"ModuleName\""

let testHeaderStr = "DependencyTest"

let testHeader = One testHeaderStr

let using      = One "using"

let arrow      = One "->"

let equal      = One "="

let boolFlag1  = "BoolFlag1"

let boolFlag2  = "BoolFlag2"

let boolValueStr = [ "True"; "False" ]

let boolValue  = Many boolValueStr

let boolTerm   = Many [boolFlag1; boolFlag2]

let testName   = Many [ "\"TEST_NAME\""; "TEST_NAME" ]

let allBodyStr = [boolFlag1; boolFlag2; "Module"; depNameStr] 

let allBody    = Many allBodyStr