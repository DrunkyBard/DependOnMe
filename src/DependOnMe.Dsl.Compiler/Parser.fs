// Implementation file for parser generated by fsyacc
module Parser
#nowarn "64";; // turn off warnings that type variables used in production annotations are instantiated to concrete type
open Microsoft.FSharp.Text.Lexing
open Microsoft.FSharp.Text.Parsing.ParseHelpers
# 1 "Parser.fsy"

open System
open DslAst

# 11 "Parser.fs"
// This type is the type of tokens accepted by the parser
type token = 
  | EOF
  | SNAME
  | FQN
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
  | SPACE
// This type is used to give symbolic names to token indexes, useful for error messages
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
    | TOKEN_SPACE
    | TOKEN_end_of_input
    | TOKEN_error
// This type is used to give symbolic names to token indexes, useful for error messages
type nonTerminalId = 
    | NONTERM__startstart
    | NONTERM_start

// This function maps tokens to integer indexes
let tagOfToken (t:token) = 
  match t with
  | EOF  -> 0 
  | SNAME  -> 1 
  | FQN  -> 2 
  | DEPENDENCIES  -> 3 
  | MODULE  -> 4 
  | QUOT  -> 5 
  | FALSE  -> 6 
  | TRUE  -> 7 
  | BF2  -> 8 
  | BF1  -> 9 
  | EQ  -> 10 
  | ARROW  -> 11 
  | ERROR  -> 12 
  | SPACE  -> 13 

// This function maps integer indexes to symbolic token ids
let tokenTagToTokenId (tokenIdx:int) = 
  match tokenIdx with
  | 0 -> TOKEN_EOF 
  | 1 -> TOKEN_SNAME 
  | 2 -> TOKEN_FQN 
  | 3 -> TOKEN_DEPENDENCIES 
  | 4 -> TOKEN_MODULE 
  | 5 -> TOKEN_QUOT 
  | 6 -> TOKEN_FALSE 
  | 7 -> TOKEN_TRUE 
  | 8 -> TOKEN_BF2 
  | 9 -> TOKEN_BF1 
  | 10 -> TOKEN_EQ 
  | 11 -> TOKEN_ARROW 
  | 12 -> TOKEN_ERROR 
  | 13 -> TOKEN_SPACE 
  | 16 -> TOKEN_end_of_input
  | 14 -> TOKEN_error
  | _ -> failwith "tokenTagToTokenId: bad token"

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
let prodIdxToNonTerminal (prodIdx:int) = 
  match prodIdx with
    | 0 -> NONTERM__startstart 
    | 1 -> NONTERM_start 
    | _ -> failwith "prodIdxToNonTerminal: bad production index"

let _fsyacc_endOfInputTag = 16 
let _fsyacc_tagOfErrorTerminal = 14

// This function gets the name of a token as a string
let token_to_string (t:token) = 
  match t with 
  | EOF  -> "EOF" 
  | SNAME  -> "SNAME" 
  | FQN  -> "FQN" 
  | DEPENDENCIES  -> "DEPENDENCIES" 
  | MODULE  -> "MODULE" 
  | QUOT  -> "QUOT" 
  | FALSE  -> "FALSE" 
  | TRUE  -> "TRUE" 
  | BF2  -> "BF2" 
  | BF1  -> "BF1" 
  | EQ  -> "EQ" 
  | ARROW  -> "ARROW" 
  | ERROR  -> "ERROR" 
  | SPACE  -> "SPACE" 

// This function gets the data carried by a token as an object
let _fsyacc_dataOfToken (t:token) = 
  match t with 
  | EOF  -> (null : System.Object) 
  | SNAME  -> (null : System.Object) 
  | FQN  -> (null : System.Object) 
  | DEPENDENCIES  -> (null : System.Object) 
  | MODULE  -> (null : System.Object) 
  | QUOT  -> (null : System.Object) 
  | FALSE  -> (null : System.Object) 
  | TRUE  -> (null : System.Object) 
  | BF2  -> (null : System.Object) 
  | BF1  -> (null : System.Object) 
  | EQ  -> (null : System.Object) 
  | ARROW  -> (null : System.Object) 
  | ERROR  -> (null : System.Object) 
  | SPACE  -> (null : System.Object) 
let _fsyacc_gotos = [| 0us; 65535us; 1us; 65535us; 0us; 1us; |]
let _fsyacc_sparseGotoTableRowOffsets = [|0us; 1us; |]
let _fsyacc_stateToProdIdxsTableElements = [| 1us; 0us; 1us; 0us; 1us; 1us; |]
let _fsyacc_stateToProdIdxsTableRowOffsets = [|0us; 2us; 4us; |]
let _fsyacc_action_rows = 3
let _fsyacc_actionTableElements = [|1us; 32768us; 0us; 2us; 0us; 49152us; 0us; 16385us; |]
let _fsyacc_actionTableRowOffsets = [|0us; 2us; 3us; |]
let _fsyacc_reductionSymbolCounts = [|1us; 1us; |]
let _fsyacc_productionToNonTerminalTable = [|0us; 1us; |]
let _fsyacc_immediateActions = [|65535us; 49152us; 16385us; |]
let _fsyacc_reductions ()  =    [| 
# 146 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : DslAst.DependencyTest)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
                      raise (Microsoft.FSharp.Text.Parsing.Accept(Microsoft.FSharp.Core.Operators.box _1))
                   )
                 : '_startstart));
# 155 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 27 "Parser.fsy"
                                 Test("", Flag1(false), Flag2(true), []) 
                   )
# 27 "Parser.fsy"
                 : DslAst.DependencyTest));
|]
# 166 "Parser.fs"
let tables () : Microsoft.FSharp.Text.Parsing.Tables<_> = 
  { reductions= _fsyacc_reductions ();
    endOfInputTag = _fsyacc_endOfInputTag;
    tagOfToken = tagOfToken;
    dataOfToken = _fsyacc_dataOfToken; 
    actionTableElements = _fsyacc_actionTableElements;
    actionTableRowOffsets = _fsyacc_actionTableRowOffsets;
    stateToProdIdxsTableElements = _fsyacc_stateToProdIdxsTableElements;
    stateToProdIdxsTableRowOffsets = _fsyacc_stateToProdIdxsTableRowOffsets;
    reductionSymbolCounts = _fsyacc_reductionSymbolCounts;
    immediateActions = _fsyacc_immediateActions;
    gotos = _fsyacc_gotos;
    sparseGotoTableRowOffsets = _fsyacc_sparseGotoTableRowOffsets;
    tagOfErrorTerminal = _fsyacc_tagOfErrorTerminal;
    parseError = (fun (ctxt:Microsoft.FSharp.Text.Parsing.ParseErrorContext<_>) -> 
                              match parse_error_rich with 
                              | Some f -> f ctxt
                              | None -> parse_error ctxt.Message);
    numTerminals = 17;
    productionToNonTerminalTable = _fsyacc_productionToNonTerminalTable  }
let engine lexer lexbuf startState = (tables ()).Interpret(lexer, lexbuf, startState)
let start lexer lexbuf : DslAst.DependencyTest =
    Microsoft.FSharp.Core.Operators.unbox ((tables ()).Interpret(lexer, lexbuf, 0))
