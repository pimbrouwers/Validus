module Validus.StringValidator.Tests

open FsCheck.Xunit
open Validus

let private TestValidator = Check.String
let private testString = "validus"
let private empty = ""

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Success`` () =
    match TestValidator.betweenLen 0 100 "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Error`` () =
    match TestValidator.betweenLen 100 1000 "Test" testString with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =
    match TestValidator.empty "Test" empty with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Error`` () =
    match TestValidator.empty "Test" testString with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.equalsLen len) should produce Success`` () =
    match TestValidator.equalsLen 7 "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Success`` () =
    match TestValidator.greaterThanLen 0 "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Error`` () =
    match TestValidator.greaterThanLen 100 "Test" testString with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.greaterThanOrEqualToLen min) should produce Success`` () =
    match TestValidator.greaterThanOrEqualToLen 0 "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanOrEqualToLen min) should produce Error`` () =
    match TestValidator.greaterThanOrEqualToLen 100 "Test" testString with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Success`` () =
    match TestValidator.lessThanLen 100 "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Error`` () =
    match TestValidator.lessThanLen 0 "Test" testString with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.lessThanOrEqualToLen min) should produce Success`` () =
    match TestValidator.lessThanOrEqualToLen 100 "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.lessThanOrEqualToLen min) should produce Error`` () =
    match TestValidator.lessThanOrEqualToLen 0 "Test" testString with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =
    match TestValidator.notEmpty "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Error`` () =
    match TestValidator.notEmpty "Test" empty with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.pattern) [a-z] should produce Success`` () =
    match TestValidator.pattern "[a-z]" "Test" testString with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.pattern) [a-z] should produce Error`` () =
    match TestValidator.pattern "[a-z]" "Test" "123456789" with
    | Ok _ -> false
    | Error _ -> true
