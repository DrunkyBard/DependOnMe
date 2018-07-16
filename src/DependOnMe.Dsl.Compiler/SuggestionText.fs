module SuggestionText

type Suggestion = 
    | One  of string
    | Many of string list
    | None

let depNameStr = "\"DependencyName\""

let depName    = One depNameStr

let implName   = One "\"ImplementationName\""

let moduleNameStr = "\"ModuleName\""

let testHeaderStr = "DependencyTest"

let moduleHeaderStr = "DependencyModule"

let moduleHeader = One moduleHeaderStr

let testHeader = One testHeaderStr

let using      = One "using"

let ns         = One "namespace"

let arrow      = One "->"

let equal      = One "="

let boolFlag1  = "BoolFlag1"

let boolFlag2  = "BoolFlag2"

let boolValueStr = [ "True"; "False" ]

let boolValue  = Many boolValueStr

let boolTerm   = Many [boolFlag1; boolFlag2]

let testName   = Many [ "\"TEST_NAME\""; "TEST_NAME" ]

let moduleName   = Many [ "\"MODULE_NAME\""; "MODULE_NAME" ]

let allTestBodyStr = [boolFlag1; boolFlag2; "Module"; depNameStr] 

let allModuleBodyStr = ["Module"; depNameStr] 

let allTestBody      = Many allTestBodyStr

let allModuleBody    = Many allModuleBodyStr

let testHeaderOrUsing = Many [ testHeaderStr; "using" ]

let moduleHeaderOrUsing = Many [ moduleHeaderStr; "using" ]