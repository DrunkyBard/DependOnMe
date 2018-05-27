// Implementation file for parser generated by fsyacc
module Parser
#nowarn "64";; // turn off warnings that type variables used in production annotations are instantiated to concrete type
open Microsoft.FSharp.Text.Lexing
open Microsoft.FSharp.Text.Parsing.ParseHelpers
# 1 "Parser.fsy"

open System
open DslAst
open TextUtilities

let (||||>) (x1, x2, x3, x4) f = f x1 x2 x3 x4

let (|Bf1Presense|Bf1Unique|) (boolFlag1Presence: BoolFlag1 option, lexeme) = 
    match lexeme with
        | BoolFlag1(b) when boolFlag1Presence.IsNone     -> Bf1Unique(b)
        | BoolFlag1(_) when not boolFlag1Presence.IsNone -> Bf1Presense
        | t -> failwithf "Incorrect node: %A" t

let (|Bf2Presense|Bf2Unique|) (boolFlag2Presence: BoolFlag2 option, lexeme) = 
    match lexeme with
        | BoolFlag2(b) when boolFlag2Presence.IsNone     -> Bf2Unique(b)
        | BoolFlag2(_) when not boolFlag2Presence.IsNone -> Bf2Presense
        | t -> failwithf "Incorrect node: %A" t

let testBoolFlag1 boolFlag1Presence lexeme = 
    match (boolFlag1Presence, lexeme) with
        | Bf1Presense  -> false
        | Bf1Unique(b) -> true
        
let testBoolFlag2 boolFlag2Presence lexeme = 
    match (boolFlag2Presence, lexeme) with
        | Bf2Presense  -> false
        | Bf2Unique(b) -> true

let testInnerDeclaration lexems = 
    let rec innerTest (boolFlag1Presence: BoolFlag1 option) (boolFlag2Presence: BoolFlag2 option) (registrations:  Registration list) = function
        | (BoolFlag1(v) as b)::t -> if testBoolFlag1 boolFlag1Presence b then 
                                        (Some v, boolFlag2Presence, registrations, t) ||||> innerTest 
                                    else failwith "Presense of Flag1 is detected"
        | (BoolFlag2(v) as b)::t -> if testBoolFlag2 boolFlag2Presence b then 
                                        (boolFlag1Presence, Some v, registrations, t) ||||> innerTest 
                                    else failwith "Presense of Flag2 is detected"
        | Registration(r)::t     -> (boolFlag1Presence, boolFlag2Presence, registrations @ r, t) ||||> innerTest
        | []                     -> (boolFlag1Presence, boolFlag2Presence, registrations)

    innerTest None None [] lexems

# 49 "Parser.fs"
// This type is the type of tokens accepted by the parser
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
    | TOKEN_TESTHEADER
    | TOKEN_end_of_input
    | TOKEN_error
// This type is used to give symbolic names to token indexes, useful for error messages
type nonTerminalId = 
    | NONTERM__startstart
    | NONTERM_start
    | NONTERM_testBody
    | NONTERM_expressionSet
    | NONTERM_bodyExpression
    | NONTERM_registration
    | NONTERM_registrationSet
    | NONTERM_boolFlag1
    | NONTERM_boolFlag2
    | NONTERM_errorBoolFlag1
    | NONTERM_errorBoolFlag2
    | NONTERM_errorRegistration

// This function maps tokens to integer indexes
let tagOfToken (t:token) = 
  match t with
  | EOF  -> 0 
  | SNAME _ -> 1 
  | FQN _ -> 2 
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
  | TESTHEADER  -> 13 

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
  | 13 -> TOKEN_TESTHEADER 
  | 16 -> TOKEN_end_of_input
  | 14 -> TOKEN_error
  | _ -> failwith "tokenTagToTokenId: bad token"

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
let prodIdxToNonTerminal (prodIdx:int) = 
  match prodIdx with
    | 0 -> NONTERM__startstart 
    | 1 -> NONTERM_start 
    | 2 -> NONTERM_testBody 
    | 3 -> NONTERM_expressionSet 
    | 4 -> NONTERM_expressionSet 
    | 5 -> NONTERM_bodyExpression 
    | 6 -> NONTERM_bodyExpression 
    | 7 -> NONTERM_bodyExpression 
    | 8 -> NONTERM_registration 
    | 9 -> NONTERM_registration 
    | 10 -> NONTERM_registration 
    | 11 -> NONTERM_registrationSet 
    | 12 -> NONTERM_registrationSet 
    | 13 -> NONTERM_boolFlag1 
    | 14 -> NONTERM_boolFlag1 
    | 15 -> NONTERM_boolFlag1 
    | 16 -> NONTERM_boolFlag2 
    | 17 -> NONTERM_boolFlag2 
    | 18 -> NONTERM_boolFlag2 
    | 19 -> NONTERM_errorBoolFlag1 
    | 20 -> NONTERM_errorBoolFlag1 
    | 21 -> NONTERM_errorBoolFlag1 
    | 22 -> NONTERM_errorBoolFlag1 
    | 23 -> NONTERM_errorBoolFlag2 
    | 24 -> NONTERM_errorBoolFlag2 
    | 25 -> NONTERM_errorBoolFlag2 
    | 26 -> NONTERM_errorBoolFlag2 
    | 27 -> NONTERM_errorRegistration 
    | 28 -> NONTERM_errorRegistration 
    | 29 -> NONTERM_errorRegistration 
    | _ -> failwith "prodIdxToNonTerminal: bad production index"

let _fsyacc_endOfInputTag = 16 
let _fsyacc_tagOfErrorTerminal = 14

// This function gets the name of a token as a string
let token_to_string (t:token) = 
  match t with 
  | EOF  -> "EOF" 
  | SNAME _ -> "SNAME" 
  | FQN _ -> "FQN" 
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
  | TESTHEADER  -> "TESTHEADER" 

// This function gets the data carried by a token as an object
let _fsyacc_dataOfToken (t:token) = 
  match t with 
  | EOF  -> (null : System.Object) 
  | SNAME _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | FQN _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
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
  | TESTHEADER  -> (null : System.Object) 
let _fsyacc_gotos = [| 0us; 65535us; 1us; 65535us; 0us; 1us; 1us; 65535us; 0us; 2us; 1us; 65535us; 5us; 6us; 2us; 65535us; 5us; 8us; 6us; 7us; 3us; 65535us; 5us; 18us; 6us; 18us; 11us; 19us; 2us; 65535us; 5us; 11us; 6us; 11us; 2us; 65535us; 5us; 9us; 6us; 9us; 2us; 65535us; 5us; 10us; 6us; 10us; 2us; 65535us; 5us; 24us; 6us; 24us; 2us; 65535us; 5us; 29us; 6us; 29us; 3us; 65535us; 5us; 17us; 6us; 17us; 11us; 17us; |]
let _fsyacc_sparseGotoTableRowOffsets = [|0us; 1us; 3us; 5us; 7us; 10us; 14us; 17us; 20us; 23us; 26us; 29us; |]
let _fsyacc_stateToProdIdxsTableElements = [| 1us; 0us; 1us; 0us; 1us; 1us; 1us; 1us; 1us; 2us; 1us; 2us; 2us; 2us; 3us; 1us; 3us; 1us; 4us; 1us; 5us; 1us; 6us; 2us; 7us; 12us; 3us; 8us; 27us; 28us; 2us; 8us; 28us; 1us; 8us; 2us; 9us; 29us; 1us; 9us; 1us; 10us; 1us; 11us; 1us; 12us; 6us; 13us; 14us; 19us; 20us; 21us; 22us; 3us; 13us; 14us; 20us; 1us; 13us; 1us; 14us; 1us; 15us; 6us; 16us; 17us; 23us; 24us; 25us; 26us; 3us; 16us; 17us; 24us; 1us; 16us; 1us; 17us; 1us; 18us; 1us; 21us; 1us; 22us; 1us; 25us; 1us; 26us; 1us; 27us; |]
let _fsyacc_stateToProdIdxsTableRowOffsets = [|0us; 2us; 4us; 6us; 8us; 10us; 12us; 15us; 17us; 19us; 21us; 23us; 26us; 30us; 33us; 35us; 38us; 40us; 42us; 44us; 46us; 53us; 57us; 59us; 61us; 63us; 70us; 74us; 76us; 78us; 80us; 82us; 84us; 86us; 88us; |]
let _fsyacc_action_rows = 35
let _fsyacc_actionTableElements = [|1us; 32768us; 13us; 4us; 0us; 49152us; 1us; 32768us; 0us; 3us; 0us; 16385us; 1us; 32768us; 1us; 5us; 4us; 32768us; 2us; 12us; 4us; 15us; 8us; 25us; 9us; 20us; 4us; 16386us; 2us; 12us; 4us; 15us; 8us; 25us; 9us; 20us; 0us; 16387us; 0us; 16388us; 0us; 16389us; 0us; 16390us; 2us; 16391us; 2us; 12us; 4us; 15us; 2us; 32768us; 2us; 34us; 11us; 13us; 1us; 16412us; 2us; 14us; 0us; 16392us; 1us; 16413us; 2us; 16us; 0us; 16393us; 0us; 16394us; 0us; 16395us; 0us; 16396us; 3us; 16403us; 6us; 31us; 7us; 30us; 10us; 21us; 2us; 16404us; 6us; 23us; 7us; 22us; 0us; 16397us; 0us; 16398us; 0us; 16399us; 3us; 16407us; 6us; 33us; 7us; 32us; 10us; 26us; 2us; 16408us; 6us; 28us; 7us; 27us; 0us; 16400us; 0us; 16401us; 0us; 16402us; 0us; 16405us; 0us; 16406us; 0us; 16409us; 0us; 16410us; 0us; 16411us; |]
let _fsyacc_actionTableRowOffsets = [|0us; 2us; 3us; 5us; 6us; 8us; 13us; 18us; 19us; 20us; 21us; 22us; 25us; 28us; 30us; 31us; 33us; 34us; 35us; 36us; 37us; 41us; 44us; 45us; 46us; 47us; 51us; 54us; 55us; 56us; 57us; 58us; 59us; 60us; 61us; |]
let _fsyacc_reductionSymbolCounts = [|1us; 2us; 3us; 2us; 1us; 1us; 1us; 1us; 3us; 2us; 1us; 1us; 2us; 3us; 3us; 1us; 3us; 3us; 1us; 1us; 2us; 2us; 2us; 1us; 2us; 2us; 2us; 2us; 2us; 1us; |]
let _fsyacc_productionToNonTerminalTable = [|0us; 1us; 2us; 3us; 3us; 4us; 4us; 4us; 5us; 5us; 5us; 6us; 6us; 7us; 7us; 7us; 8us; 8us; 8us; 9us; 9us; 9us; 9us; 10us; 10us; 10us; 10us; 11us; 11us; 11us; |]
let _fsyacc_immediateActions = [|65535us; 49152us; 65535us; 16385us; 65535us; 65535us; 65535us; 16387us; 16388us; 16389us; 16390us; 65535us; 65535us; 65535us; 16392us; 65535us; 16393us; 16394us; 16395us; 16396us; 65535us; 65535us; 16397us; 16398us; 16399us; 65535us; 65535us; 16400us; 16401us; 16402us; 16405us; 16406us; 16409us; 16410us; 16411us; |]
let _fsyacc_reductions ()  =    [| 
# 222 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : DslAst.DependencyTest)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
                      raise (Microsoft.FSharp.Text.Parsing.Accept(Microsoft.FSharp.Core.Operators.box _1))
                   )
                 : '_startstart));
# 231 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'testBody)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 65 "Parser.fsy"
                                          _1 
                   )
# 65 "Parser.fsy"
                 : DslAst.DependencyTest));
# 242 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'expressionSet)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 69 "Parser.fsy"
                              
                                 let boolFlag1, boolFlag2, registrations = testInnerDeclaration _3
                                 Test(_2, boolFlag1.Value, boolFlag2.Value, registrations, posRangeExt parseState 1 2, posRangeExt parseState 1 3)
                             
                   )
# 69 "Parser.fsy"
                 : 'testBody));
# 257 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expressionSet)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'bodyExpression)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 75 "Parser.fsy"
                                                          _2::_1 |> List.rev 
                   )
# 75 "Parser.fsy"
                 : 'expressionSet));
# 269 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'bodyExpression)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 76 "Parser.fsy"
                                            [_1] 
                   )
# 76 "Parser.fsy"
                 : 'expressionSet));
# 280 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolFlag1)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 79 "Parser.fsy"
                                             BoolFlag1(_1) 
                   )
# 79 "Parser.fsy"
                 : 'bodyExpression));
# 291 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolFlag2)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 80 "Parser.fsy"
                                             BoolFlag2(_1) 
                   )
# 80 "Parser.fsy"
                 : 'bodyExpression));
# 302 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'registrationSet)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 81 "Parser.fsy"
                                             Registration(_1 |> List.rev) 
                   )
# 81 "Parser.fsy"
                 : 'bodyExpression));
# 313 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 84 "Parser.fsy"
                                               Class(_1, _3, posRange parseState 1, posRange parseState 3) 
                   )
# 84 "Parser.fsy"
                 : 'registration));
# 325 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 85 "Parser.fsy"
                                               Module(_2, posRange parseState 1, posRange parseState 2) 
                   )
# 85 "Parser.fsy"
                 : 'registration));
# 336 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'errorRegistration)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 86 "Parser.fsy"
                                            _1 
                   )
# 86 "Parser.fsy"
                 : 'registration));
# 347 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'registration)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 89 "Parser.fsy"
                                                          [_1] 
                   )
# 89 "Parser.fsy"
                 : 'registrationSet));
# 358 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'registrationSet)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'registration)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 90 "Parser.fsy"
                                                          _2::_1 
                   )
# 90 "Parser.fsy"
                 : 'registrationSet));
# 370 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 93 "Parser.fsy"
                                            BoolFlag1.Flag(true,  posRangeExt parseState 1 3)  
                   )
# 93 "Parser.fsy"
                 : 'boolFlag1));
# 380 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 94 "Parser.fsy"
                                            BoolFlag1.Flag(false, posRangeExt parseState 1 3) 
                   )
# 94 "Parser.fsy"
                 : 'boolFlag1));
# 390 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'errorBoolFlag1)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 95 "Parser.fsy"
                                            _1 
                   )
# 95 "Parser.fsy"
                 : 'boolFlag1));
# 401 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 98 "Parser.fsy"
                                            BoolFlag2.Flag(true,  posRangeExt parseState 1 3) 
                   )
# 98 "Parser.fsy"
                 : 'boolFlag2));
# 411 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 99 "Parser.fsy"
                                            BoolFlag2.Flag(false, posRangeExt parseState 1 3) 
                   )
# 99 "Parser.fsy"
                 : 'boolFlag2));
# 421 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'errorBoolFlag2)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 100 "Parser.fsy"
                                            _1 
                   )
# 100 "Parser.fsy"
                 : 'boolFlag2));
# 432 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 103 "Parser.fsy"
                                       BoolFlag1.Error(BoolFlagMissingPart.Equal,  endPos parseState 1, posRangeExt parseState 1 1) 
                   )
# 103 "Parser.fsy"
                 : 'errorBoolFlag1));
# 442 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 104 "Parser.fsy"
                                       BoolFlag1.Error(BoolFlagMissingPart.Value,  endPos parseState 2, posRangeExt parseState 1 2) 
                   )
# 104 "Parser.fsy"
                 : 'errorBoolFlag1));
# 452 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 105 "Parser.fsy"
                                       BoolFlag1.Error(BoolFlagMissingPart.Equal,  endPos parseState 1, posRangeExt parseState 1 2) 
                   )
# 105 "Parser.fsy"
                 : 'errorBoolFlag1));
# 462 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 106 "Parser.fsy"
                                       BoolFlag1.Error(BoolFlagMissingPart.Equal,  endPos parseState 1, posRangeExt parseState 1 2) 
                   )
# 106 "Parser.fsy"
                 : 'errorBoolFlag1));
# 472 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 109 "Parser.fsy"
                                       BoolFlag2.Error(BoolFlagMissingPart.Equal,  endPos parseState 1, posRangeExt parseState 1 1) 
                   )
# 109 "Parser.fsy"
                 : 'errorBoolFlag2));
# 482 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 110 "Parser.fsy"
                                       BoolFlag2.Error(BoolFlagMissingPart.Value,  endPos parseState 1, posRangeExt parseState 1 2) 
                   )
# 110 "Parser.fsy"
                 : 'errorBoolFlag2));
# 492 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 111 "Parser.fsy"
                                    BoolFlag2.Error(BoolFlagMissingPart.Equal,  endPos parseState 1, posRangeExt parseState 1 2) 
                   )
# 111 "Parser.fsy"
                 : 'errorBoolFlag2));
# 502 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 112 "Parser.fsy"
                                       BoolFlag2.Error(BoolFlagMissingPart.Equal,  endPos parseState 1, posRangeExt parseState 1 2) 
                   )
# 112 "Parser.fsy"
                 : 'errorBoolFlag2));
# 512 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 115 "Parser.fsy"
                                    ClassError(ClassRegMissingPart.Arrow, endPos parseState 1, posRangeExt parseState 1 2) 
                   )
# 115 "Parser.fsy"
                 : 'errorRegistration));
# 524 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 116 "Parser.fsy"
                                    ClassError(ClassRegMissingPart.Name, endPos parseState 2, posRangeExt parseState 1 2) 
                   )
# 116 "Parser.fsy"
                 : 'errorRegistration));
# 535 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 117 "Parser.fsy"
                                    ModuleError(ModuleRegMissingPart.Name, endPos parseState 1, posRangeExt parseState 1 1) 
                   )
# 117 "Parser.fsy"
                 : 'errorRegistration));
|]
# 546 "Parser.fs"
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
