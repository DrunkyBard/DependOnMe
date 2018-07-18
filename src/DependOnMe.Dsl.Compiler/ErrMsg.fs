module ErrMsg

let EqMissing = "Equals is missing"
let ArrowMissing = "Arrow symbol is missing"
let BoolMissing = "Boolean value is missing"
let FqnMissing = "Fully qualified type name is missing"
let DuplicateBf1Declaration = "BoolFlag1 is already defined"
let DuplicateBf2Declaration = "BoolFlag2 is already defined"
let TestNameIsNotDefined = "Test name is not defined"
let UnexpectedToken token = sprintf "Unexpected token: %s" token
let InvalidModuleName token = sprintf "Invalid module name: %s. Module name cannot contains dots" token
let IncompleteName token = sprintf "Incomplete fully qualified name: %s" token
let MissingNamespace = "Using token was declared, but namespace is not"
let TestHeaderExpected = "TestHeader token expected"
let BoolFlagTokenExpected = "BoolFlag1 or BoolFlag2 token expected"
let OrphanArrow = "Unexpected '->' token"
let DuplicatedDependency name = sprintf "Duplicated dependency registration: %s" name
let DuplicatedModule name = sprintf "Duplicated module registration: %s" name
let AmbigousModule name = sprintf "Ambigous module '%s': module defined in many places" name
let ModuleIsNotDefined name = sprintf "Module '%s' is not defined" name