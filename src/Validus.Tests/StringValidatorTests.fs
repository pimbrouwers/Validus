module Validus.StringValidator.Tests

open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Validators.StringValidator()
let private testString = "validus"

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Success`` () =               
    match TestValidator.betweenLen 0 100 None "Test" testString with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Failure`` () =           
    match TestValidator.betweenLen 100 1000 None "Test" testString with
    | Success _ -> false
    | Failure _ -> true

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =           
    match TestValidator.empty None "Test" "" with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Failure`` () =           
    match TestValidator.empty None "Test" testString with
    | Success _ -> false
    | Failure _ -> true

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Success`` () =               
    match TestValidator.greaterThanLen 0 None "Test" testString with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Failure`` () =           
    match TestValidator.greaterThanLen 100 None "Test" testString with
    | Success _ -> false
    | Failure _ -> true

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Success`` () =               
    match TestValidator.lessThanLen 100 None "Test" testString with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Failure`` () =           
    match TestValidator.lessThanLen 0 None "Test" testString with
    | Success _ -> false
    | Failure _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =           
    match TestValidator.notEmpty None "Test" testString with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Failure`` () =           
    match TestValidator.notEmpty None "Test" "" with
    | Success _ -> false
    | Failure _ -> true

[<Property>]
let ``(TestValidator.pattern) [a-z] should produce Success`` () =           
    match TestValidator.pattern "[a-z]" None "Test" testString with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.pattern) [a-z] should produce Failure`` () =           
    match TestValidator.pattern "[a-z]" None "Test" "123456789" with
    | Success _ -> false
    | Failure _ -> true