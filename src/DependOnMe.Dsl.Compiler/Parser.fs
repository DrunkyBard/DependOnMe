// Implementation file for parser generated by fsyacc
module Parser
#nowarn "64";; // turn off warnings that type variables used in production annotations are instantiated to concrete type
open Microsoft.FSharp.Text.Lexing
open Microsoft.FSharp.Text.Parsing.ParseHelpers
# 1 "Parser.fsy"

open System
open DslAst
open TextUtilities
open Common
open Compilation
open System.Collections.Generic
open Errors
open Positioning
open DataStructures
open Navigation

let errorLogger = ErrorLogger()

let index = RedBlackTree<PosRange, IndexTerm>(PositionComparer.Instance)
let testIndex = PositionSet()

let addIdx posRange term = index.Insert(posRange, term)
let addTestIdx posRange term = testIndex.Insert(PositionIndex(posRange, term))

let separate lexems = 
    let rec separateRec (boolFlags1: BoolFlag1 list) (boolFlags2: BoolFlag2 list) (registrations:  Registration list) = function
        | (BoolFlag1(BoolFlag1.Flag(_) as b))::t -> 
            (b::boolFlags1, boolFlags2, registrations, t)  ||||> separateRec 
        | (BoolFlag2(BoolFlag2.Flag(_) as b))::t -> 
            (boolFlags1, b::boolFlags2, registrations, t)  ||||> separateRec
        | Registration(r)::t           -> 
            (boolFlags1, boolFlags2, List.append registrations r, t) ||||> separateRec
        | any::t -> (boolFlags1, boolFlags2, registrations, t) ||||> separateRec
        | []     -> (boolFlags1 |> List.rev, boolFlags2 |> List.rev, registrations)

    separateRec [] [] [] lexems

# 40 "Parser.fs"
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
    | NONTERM_testHeader
    | NONTERM_expressionSet
    | NONTERM_bodyExpression
    | NONTERM_registration
    | NONTERM_registrationSet
    | NONTERM_boolValue
    | NONTERM_boolFlag1
    | NONTERM_boolFlag2
    | NONTERM_errorBoolFlag1
    | NONTERM_errorBoolFlag2
    | NONTERM_errorRegistration
    | NONTERM_recover
    | NONTERM_any

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
    | 2 -> NONTERM_start 
    | 3 -> NONTERM_testBody 
    | 4 -> NONTERM_testHeader 
    | 5 -> NONTERM_testHeader 
    | 6 -> NONTERM_testHeader 
    | 7 -> NONTERM_expressionSet 
    | 8 -> NONTERM_expressionSet 
    | 9 -> NONTERM_bodyExpression 
    | 10 -> NONTERM_bodyExpression 
    | 11 -> NONTERM_bodyExpression 
    | 12 -> NONTERM_bodyExpression 
    | 13 -> NONTERM_registration 
    | 14 -> NONTERM_registration 
    | 15 -> NONTERM_registration 
    | 16 -> NONTERM_registrationSet 
    | 17 -> NONTERM_registrationSet 
    | 18 -> NONTERM_boolValue 
    | 19 -> NONTERM_boolValue 
    | 20 -> NONTERM_boolFlag1 
    | 21 -> NONTERM_boolFlag1 
    | 22 -> NONTERM_boolFlag2 
    | 23 -> NONTERM_boolFlag2 
    | 24 -> NONTERM_errorBoolFlag1 
    | 25 -> NONTERM_errorBoolFlag1 
    | 26 -> NONTERM_errorBoolFlag1 
    | 27 -> NONTERM_errorBoolFlag1 
    | 28 -> NONTERM_errorBoolFlag1 
    | 29 -> NONTERM_errorBoolFlag2 
    | 30 -> NONTERM_errorBoolFlag2 
    | 31 -> NONTERM_errorBoolFlag2 
    | 32 -> NONTERM_errorRegistration 
    | 33 -> NONTERM_errorRegistration 
    | 34 -> NONTERM_errorRegistration 
    | 35 -> NONTERM_recover 
    | 36 -> NONTERM_any 
    | 37 -> NONTERM_any 
    | 38 -> NONTERM_any 
    | 39 -> NONTERM_any 
    | 40 -> NONTERM_any 
    | 41 -> NONTERM_any 
    | 42 -> NONTERM_any 
    | 43 -> NONTERM_any 
    | 44 -> NONTERM_any 
    | 45 -> NONTERM_any 
    | 46 -> NONTERM_any 
    | 47 -> NONTERM_any 
    | 48 -> NONTERM_any 
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
let _fsyacc_gotos = [| 0us; 65535us; 1us; 65535us; 0us; 1us; 1us; 65535us; 0us; 2us; 1us; 65535us; 0us; 5us; 1us; 65535us; 5us; 6us; 2us; 65535us; 5us; 11us; 6us; 10us; 3us; 65535us; 5us; 22us; 6us; 22us; 14us; 23us; 2us; 65535us; 5us; 14us; 6us; 14us; 7us; 65535us; 5us; 37us; 6us; 37us; 26us; 34us; 27us; 28us; 30us; 38us; 31us; 32us; 35us; 36us; 2us; 65535us; 5us; 12us; 6us; 12us; 2us; 65535us; 5us; 13us; 6us; 13us; 2us; 65535us; 5us; 29us; 6us; 29us; 2us; 65535us; 5us; 33us; 6us; 33us; 3us; 65535us; 5us; 21us; 6us; 21us; 14us; 21us; 2us; 65535us; 5us; 15us; 6us; 15us; 1us; 65535us; 0us; 9us; |]
let _fsyacc_sparseGotoTableRowOffsets = [|0us; 1us; 3us; 5us; 7us; 9us; 12us; 16us; 19us; 27us; 30us; 33us; 36us; 39us; 43us; 46us; |]
let _fsyacc_stateToProdIdxsTableElements = [| 1us; 0us; 1us; 0us; 1us; 1us; 1us; 1us; 1us; 2us; 1us; 3us; 2us; 3us; 7us; 3us; 4us; 5us; 36us; 1us; 4us; 1us; 6us; 1us; 7us; 1us; 8us; 1us; 9us; 1us; 10us; 2us; 11us; 17us; 1us; 12us; 3us; 13us; 32us; 33us; 2us; 13us; 33us; 1us; 13us; 2us; 14us; 34us; 1us; 14us; 1us; 15us; 1us; 16us; 1us; 17us; 1us; 18us; 1us; 19us; 4us; 20us; 24us; 25us; 26us; 2us; 20us; 25us; 1us; 20us; 1us; 21us; 4us; 22us; 29us; 30us; 31us; 2us; 22us; 30us; 1us; 22us; 1us; 23us; 1us; 26us; 1us; 27us; 1us; 27us; 1us; 28us; 1us; 31us; 1us; 32us; 1us; 35us; 1us; 37us; 1us; 38us; 1us; 39us; 1us; 40us; 1us; 41us; 1us; 42us; 1us; 43us; 1us; 44us; 1us; 45us; 1us; 46us; 1us; 47us; 1us; 48us; |]
let _fsyacc_stateToProdIdxsTableRowOffsets = [|0us; 2us; 4us; 6us; 8us; 10us; 12us; 15us; 19us; 21us; 23us; 25us; 27us; 29us; 31us; 34us; 36us; 40us; 43us; 45us; 48us; 50us; 52us; 54us; 56us; 58us; 60us; 65us; 68us; 70us; 72us; 77us; 80us; 82us; 84us; 86us; 88us; 90us; 92us; 94us; 96us; 98us; 100us; 102us; 104us; 106us; 108us; 110us; 112us; 114us; 116us; 118us; 120us; |]
let _fsyacc_action_rows = 53
let _fsyacc_actionTableElements = [|14us; 32768us; 0us; 4us; 1us; 52us; 2us; 51us; 3us; 50us; 4us; 49us; 5us; 48us; 6us; 47us; 7us; 46us; 8us; 45us; 9us; 44us; 10us; 43us; 11us; 42us; 12us; 41us; 13us; 7us; 0us; 49152us; 1us; 32768us; 0us; 3us; 0us; 16385us; 0us; 16386us; 8us; 32768us; 2us; 16us; 4us; 19us; 6us; 25us; 7us; 24us; 8us; 30us; 9us; 26us; 10us; 35us; 14us; 40us; 8us; 16387us; 2us; 16us; 4us; 19us; 6us; 25us; 7us; 24us; 8us; 30us; 9us; 26us; 10us; 35us; 14us; 40us; 1us; 16389us; 1us; 8us; 0us; 16388us; 0us; 16390us; 0us; 16391us; 0us; 16392us; 0us; 16393us; 0us; 16394us; 2us; 16395us; 2us; 16us; 4us; 19us; 0us; 16396us; 2us; 32768us; 2us; 39us; 11us; 17us; 1us; 16417us; 2us; 18us; 0us; 16397us; 1us; 16418us; 2us; 20us; 0us; 16398us; 0us; 16399us; 0us; 16400us; 0us; 16401us; 0us; 16402us; 0us; 16403us; 3us; 16408us; 6us; 25us; 7us; 24us; 10us; 27us; 2us; 16409us; 6us; 25us; 7us; 24us; 0us; 16404us; 0us; 16405us; 3us; 16413us; 6us; 25us; 7us; 24us; 10us; 31us; 2us; 16414us; 6us; 25us; 7us; 24us; 0us; 16406us; 0us; 16407us; 0us; 16410us; 2us; 32768us; 6us; 25us; 7us; 24us; 0us; 16411us; 0us; 16412us; 0us; 16415us; 0us; 16416us; 0us; 16419us; 0us; 16421us; 0us; 16422us; 0us; 16423us; 0us; 16424us; 0us; 16425us; 0us; 16426us; 0us; 16427us; 0us; 16428us; 0us; 16429us; 0us; 16430us; 0us; 16431us; 0us; 16432us; |]
let _fsyacc_actionTableRowOffsets = [|0us; 15us; 16us; 18us; 19us; 20us; 29us; 38us; 40us; 41us; 42us; 43us; 44us; 45us; 46us; 49us; 50us; 53us; 55us; 56us; 58us; 59us; 60us; 61us; 62us; 63us; 64us; 68us; 71us; 72us; 73us; 77us; 80us; 81us; 82us; 83us; 86us; 87us; 88us; 89us; 90us; 91us; 92us; 93us; 94us; 95us; 96us; 97us; 98us; 99us; 100us; 101us; 102us; |]
let _fsyacc_reductionSymbolCounts = [|1us; 2us; 1us; 2us; 2us; 1us; 1us; 2us; 1us; 1us; 1us; 1us; 1us; 3us; 2us; 1us; 1us; 2us; 1us; 1us; 3us; 1us; 3us; 1us; 1us; 2us; 2us; 2us; 1us; 1us; 2us; 2us; 2us; 2us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; |]
let _fsyacc_productionToNonTerminalTable = [|0us; 1us; 1us; 2us; 3us; 3us; 3us; 4us; 4us; 5us; 5us; 5us; 5us; 6us; 6us; 6us; 7us; 7us; 8us; 8us; 9us; 9us; 10us; 10us; 11us; 11us; 11us; 11us; 11us; 12us; 12us; 12us; 13us; 13us; 13us; 14us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; 15us; |]
let _fsyacc_immediateActions = [|65535us; 49152us; 65535us; 16385us; 16386us; 65535us; 65535us; 65535us; 16388us; 16390us; 16391us; 16392us; 16393us; 16394us; 65535us; 16396us; 65535us; 65535us; 16397us; 65535us; 16398us; 16399us; 16400us; 16401us; 16402us; 16403us; 65535us; 65535us; 16404us; 16405us; 65535us; 65535us; 16406us; 16407us; 16410us; 65535us; 16411us; 16412us; 16415us; 16416us; 16419us; 16421us; 16422us; 16423us; 16424us; 16425us; 16426us; 16427us; 16428us; 16429us; 16430us; 16431us; 16432us; |]
let _fsyacc_reductions ()  =    [| 
# 236 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : DslAst.DependencyTest)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
                      raise (Microsoft.FSharp.Text.Parsing.Accept(Microsoft.FSharp.Core.Operators.box _1))
                   )
                 : '_startstart));
# 245 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'testBody)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 56 "Parser.fsy"
                                          _1 
                   )
# 56 "Parser.fsy"
                 : DslAst.DependencyTest));
# 256 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 57 "Parser.fsy"
                                 DependencyTest.Empty 
                   )
# 57 "Parser.fsy"
                 : DslAst.DependencyTest));
# 266 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'testHeader)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'expressionSet)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 61 "Parser.fsy"
                              
                                 (posRange parseState 1, _1 |> TestDeclarationTerm) ||> addIdx
                                 (posRange parseState 1, _1 |> TestDeclarationTerm) ||> addTestIdx
                                 let boolFlags1, boolFlags2, registrations = separate _2
                                 Test(_1, boolFlags1, boolFlags2, registrations, posRangeExt parseState 1 2)
                             
                   )
# 61 "Parser.fsy"
                 : 'testBody));
# 283 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 69 "Parser.fsy"
                                              TestDeclaration.Full(_2, posRange parseState 1, posRange parseState 2) 
                   )
# 69 "Parser.fsy"
                 : 'testHeader));
# 294 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 71 "Parser.fsy"
                               
                                 (endPos parseState 1, ErrMsg.TestNameIsNotDefined, errorLogger) |||> reportPoint
                                 TestDeclaration.Partial(posRange parseState 1)
                             
                   )
# 71 "Parser.fsy"
                 : 'testHeader));
# 307 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'any)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 76 "Parser.fsy"
                               
                                 let errPos, errToken = _1
                                 (errPos, ErrMsg.TestHeaderExpected, errorLogger) |||> reportRange
                                 TestDeclaration.Error(errPos)
                             
                   )
# 76 "Parser.fsy"
                 : 'testHeader));
# 322 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expressionSet)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'bodyExpression)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 83 "Parser.fsy"
                                                          _2::_1 |> List.rev 
                   )
# 83 "Parser.fsy"
                 : 'expressionSet));
# 334 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'bodyExpression)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 84 "Parser.fsy"
                                            [_1] 
                   )
# 84 "Parser.fsy"
                 : 'expressionSet));
# 345 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolFlag1)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 88 "Parser.fsy"
                                       
                                         (posRange parseState 1, _1 |> BoolFlag1Term) ||> addIdx
                                         (posRange parseState 1, _1 |> BoolFlag1Term) ||> addTestIdx
                                         BoolFlag1(_1)
                                     
                   )
# 88 "Parser.fsy"
                 : 'bodyExpression));
# 360 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolFlag2)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 94 "Parser.fsy"
                                       
                                         (posRange parseState 1, _1 |> BoolFlag2Term) ||> addIdx
                                         (posRange parseState 1, _1 |> BoolFlag2Term) ||> addTestIdx
                                         BoolFlag2(_1)
                                     
                   )
# 94 "Parser.fsy"
                 : 'bodyExpression));
# 375 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'registrationSet)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 99 "Parser.fsy"
                                             Registration(_1 |> List.rev) 
                   )
# 99 "Parser.fsy"
                 : 'bodyExpression));
# 386 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'recover)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 100 "Parser.fsy"
                                             Declaration.Error 
                   )
# 100 "Parser.fsy"
                 : 'bodyExpression));
# 397 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 103 "Parser.fsy"
                                               Class(_1, _3, posRange parseState 1, posRange parseState 2, posRange parseState 3) 
                   )
# 103 "Parser.fsy"
                 : 'registration));
# 409 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 104 "Parser.fsy"
                                               Module(_2, posRange parseState 1, posRange parseState 2) 
                   )
# 104 "Parser.fsy"
                 : 'registration));
# 420 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'errorRegistration)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 105 "Parser.fsy"
                                            _1 
                   )
# 105 "Parser.fsy"
                 : 'registration));
# 431 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'registration)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 108 "Parser.fsy"
                                         
                                            (posRange parseState 1, _1 |> RegistrationTerm) ||> addIdx
                                            (posRange parseState 1, _1 |> RegistrationTerm) ||> addTestIdx
                                            [_1]
                                        
                   )
# 108 "Parser.fsy"
                 : 'registrationSet));
# 446 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'registrationSet)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'registration)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 114 "Parser.fsy"
                                          
                                            (posRange parseState 2, _2 |> RegistrationTerm) ||> addIdx
                                            (posRange parseState 2, _2 |> RegistrationTerm) ||> addTestIdx
                                            _2::_1 
                                        
                   )
# 114 "Parser.fsy"
                 : 'registrationSet));
# 462 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 121 "Parser.fsy"
                                   true 
                   )
# 121 "Parser.fsy"
                 : 'boolValue));
# 472 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 122 "Parser.fsy"
                                   false 
                   )
# 122 "Parser.fsy"
                 : 'boolValue));
# 482 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolValue)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 125 "Parser.fsy"
                                              BoolFlag1.Flag(_3,  posRange parseState 1, startPos parseState 2, posRange parseState 3) 
                   )
# 125 "Parser.fsy"
                 : 'boolFlag1));
# 493 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'errorBoolFlag1)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 126 "Parser.fsy"
                                              _1 
                   )
# 126 "Parser.fsy"
                 : 'boolFlag1));
# 504 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolValue)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 129 "Parser.fsy"
                                              BoolFlag2.Flag(_3,  posRange parseState 1, startPos parseState 2, posRange parseState 3) 
                   )
# 129 "Parser.fsy"
                 : 'boolFlag2));
# 515 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'errorBoolFlag2)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 130 "Parser.fsy"
                                              _1 
                   )
# 130 "Parser.fsy"
                 : 'boolFlag2));
# 526 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 133 "Parser.fsy"
                                            
                                              (endPos parseState 1, ErrMsg.EqMissing, errorLogger) |||> reportPoint
                                              BoolFlag1.Error(BoolFlagMissingPart.Equal, endPos parseState 1) 
                                          
                   )
# 133 "Parser.fsy"
                 : 'errorBoolFlag1));
# 539 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 137 "Parser.fsy"
                                            
                                              (endPos parseState 2, ErrMsg.BoolMissing, errorLogger) |||> reportPoint
                                              BoolFlag1.Error(BoolFlagMissingPart.Value, endPos parseState 2) 
                                          
                   )
# 137 "Parser.fsy"
                 : 'errorBoolFlag1));
# 552 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolValue)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 141 "Parser.fsy"
                                            
                                             (endPos parseState 1, ErrMsg.EqMissing, errorLogger) |||> reportPoint
                                             BoolFlag1.Error(BoolFlagMissingPart.Equal, endPos parseState 1) 
                                          
                   )
# 141 "Parser.fsy"
                 : 'errorBoolFlag1));
# 566 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolValue)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 145 "Parser.fsy"
                                           
                                             (startPos parseState 1, ErrMsg.BoolFlagTokenExpected, errorLogger) |||> reportPoint
                                             BoolFlag1.Error(BoolFlagMissingPart.BoolFlagTerm, startPos parseState 1) 
                                          
                   )
# 145 "Parser.fsy"
                 : 'errorBoolFlag1));
# 580 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolValue)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 149 "Parser.fsy"
                                           
                                             (startPos parseState 1, ErrMsg.BoolFlagTokenExpected, errorLogger) |||> reportPoint
                                             BoolFlag1.Error(BoolFlagMissingPart.BoolFlagTerm, startPos parseState 1) 
                                          
                   )
# 149 "Parser.fsy"
                 : 'errorBoolFlag1));
# 594 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 155 "Parser.fsy"
                                            
                                              (endPos parseState 1, ErrMsg.EqMissing, errorLogger) |||> reportPoint
                                              BoolFlag2.Error(BoolFlagMissingPart.Equal, endPos parseState 1) 
                                          
                   )
# 155 "Parser.fsy"
                 : 'errorBoolFlag2));
# 607 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 159 "Parser.fsy"
                                            
                                              (endPos parseState 2, ErrMsg.BoolMissing, errorLogger) |||> reportPoint
                                              BoolFlag2.Error(BoolFlagMissingPart.Value, endPos parseState 1) 
                                          
                   )
# 159 "Parser.fsy"
                 : 'errorBoolFlag2));
# 620 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'boolValue)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 163 "Parser.fsy"
                                         
                                             (endPos parseState 1, ErrMsg.EqMissing, errorLogger) |||> reportPoint
                                             BoolFlag2.Error(BoolFlagMissingPart.Equal, endPos parseState 1) 
                                          
                   )
# 163 "Parser.fsy"
                 : 'errorBoolFlag2));
# 634 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 169 "Parser.fsy"
                                    
                                         (endPos parseState 1, ErrMsg.ArrowMissing, errorLogger) |||> reportPoint
                                         ClassError(ClassRegMissingPart.Arrow, endPos parseState 1) 
                                     
                   )
# 169 "Parser.fsy"
                 : 'errorRegistration));
# 649 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 173 "Parser.fsy"
                                    
                                         (endPos parseState 2, ErrMsg.FqnMissing, errorLogger) |||> reportPoint
                                         ClassError(ClassRegMissingPart.Name, endPos parseState 2) 
                                     
                   )
# 173 "Parser.fsy"
                 : 'errorRegistration));
# 663 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 177 "Parser.fsy"
                                    
                                         (endPos parseState 1, ErrMsg.FqnMissing, errorLogger) |||> reportPoint
                                         ModuleError(ModuleRegMissingPart.Name, endPos parseState 1) 
                                     
                   )
# 177 "Parser.fsy"
                 : 'errorRegistration));
# 676 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 184 "Parser.fsy"
                              
                                 let lexbuf   = parseState.ParserLocalStore.["LexBuffer"] :?> LexBuffer<char>
                                 let posRange = (lexbuf.StartPos, lexbuf.EndPos)
                                 let errToken = new string(lexbuf.Lexeme)
                                 (posRange, ErrMsg.UnexpectedToken errToken, errorLogger) |||>reportRange
                             
                   )
# 184 "Parser.fsy"
                 : 'recover));
# 691 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 192 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 192 "Parser.fsy"
                 : 'any));
# 701 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 193 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 193 "Parser.fsy"
                 : 'any));
# 711 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 194 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 194 "Parser.fsy"
                 : 'any));
# 721 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 195 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 195 "Parser.fsy"
                 : 'any));
# 731 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 196 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 196 "Parser.fsy"
                 : 'any));
# 741 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 197 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 197 "Parser.fsy"
                 : 'any));
# 751 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 198 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 198 "Parser.fsy"
                 : 'any));
# 761 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 199 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 199 "Parser.fsy"
                 : 'any));
# 771 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 200 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 200 "Parser.fsy"
                 : 'any));
# 781 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 201 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 201 "Parser.fsy"
                 : 'any));
# 791 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 202 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 202 "Parser.fsy"
                 : 'any));
# 801 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 203 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 203 "Parser.fsy"
                 : 'any));
# 812 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 204 "Parser.fsy"
                                          posRangeAndToken parseState 
                   )
# 204 "Parser.fsy"
                 : 'any));
|]
# 824 "Parser.fs"
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
