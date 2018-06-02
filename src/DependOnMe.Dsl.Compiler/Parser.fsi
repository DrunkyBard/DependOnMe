// Signature file for parser generated by fsyacc
module Parser
type token = 
  | EOF
  | SNAME of (string)
  | FQN of (string)
  | DEPENDENCIES
  | MODULE
  | QUOT
  | FALSE
  | TRUE
  | BF2
  | BF1
  | EQ
  | ARROW
  | ERROR
  | TESTHEADER
type tokenId = 
    | TOKEN_EOF
    | TOKEN_SNAME
    | TOKEN_FQN
    | TOKEN_DEPENDENCIES
    | TOKEN_MODULE
    | TOKEN_QUOT
    | TOKEN_FALSE
    | TOKEN_TRUE
    | TOKEN_BF2
    | TOKEN_BF1
    | TOKEN_EQ
    | TOKEN_ARROW
    | TOKEN_ERROR
    | TOKEN_TESTHEADER
    | TOKEN_end_of_input
    | TOKEN_error
type nonTerminalId = 
    | NONTERM__startstart
    | NONTERM_start
    | NONTERM_testBody
    | NONTERM_testHeader
    | NONTERM_expressionSet
    | NONTERM_bodyExpression
    | NONTERM_registration
    | NONTERM_registrationSet
    | NONTERM_boolFlag1
    | NONTERM_boolFlag2
    | NONTERM_errorBoolFlag1
    | NONTERM_errorBoolFlag2
    | NONTERM_errorRegistration
/// This function maps tokens to integer indexes
val tagOfToken: token -> int

/// This function maps integer indexes to symbolic token ids
val tokenTagToTokenId: int -> tokenId

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
val prodIdxToNonTerminal: int -> nonTerminalId

/// This function gets the name of a token as a string
val token_to_string: token -> string
val start : (Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> token) -> Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> (DslAst.DependencyTest) 
