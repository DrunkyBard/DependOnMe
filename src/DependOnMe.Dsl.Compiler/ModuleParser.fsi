// Signature file for parser generated by fsyacc
module ModuleParser
type token = 
  | EOF
  | USING
  | SNAME of (string)
  | IQN of (string)
  | FQN of (string)
  | MODULE
  | QUOT
  | ARROW
  | MODULEHEADER
type tokenId = 
    | TOKEN_EOF
    | TOKEN_USING
    | TOKEN_SNAME
    | TOKEN_IQN
    | TOKEN_FQN
    | TOKEN_MODULE
    | TOKEN_QUOT
    | TOKEN_ARROW
    | TOKEN_MODULEHEADER
    | TOKEN_end_of_input
    | TOKEN_error
type nonTerminalId = 
    | NONTERM__startparseModule
    | NONTERM_parseModule
    | NONTERM_moduleHeader
    | NONTERM_moduleCompilationUnit
    | NONTERM_modules
    | NONTERM_moduleBody
    | NONTERM_moduleBodyExpression
    | NONTERM_moduleExpressionSet
    | NONTERM_usings
    | NONTERM_using
    | NONTERM_registrationSet
    | NONTERM_registration
    | NONTERM_name
    | NONTERM_errorRegistration
    | NONTERM_errorExprBody
    | NONTERM_errToken
    | NONTERM_recover
    | NONTERM_any
/// This function maps tokens to integer indexes
val tagOfToken: token -> int

/// This function maps integer indexes to symbolic token ids
val tokenTagToTokenId: int -> tokenId

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
val prodIdxToNonTerminal: int -> nonTerminalId

/// This function gets the name of a token as a string
val token_to_string: token -> string
val parseModule : (Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> token) -> Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> (ModuleCompilationUnit) 
