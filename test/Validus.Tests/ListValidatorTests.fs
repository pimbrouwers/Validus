module Validus.ListValidator.Tests

open FsCheck.Xunit
open Validus

let private TestValidator = Check.List
let private testList = [1..10]
let private empty = []

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Success`` () =
    match TestValidator.betweenLen 0 100 "Test" testList with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Failure`` () =
    match TestValidator.betweenLen 100 1000 "Test" testList with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =
    match TestValidator.empty "Test" empty with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Failure`` () =
    match TestValidator.empty "Test" testList with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.equalsLen len) should produce Success`` () =
    match TestValidator.equalsLen 10 "Test" testList with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Success`` () =
    match TestValidator.greaterThanLen 0 "Test" testList with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Failure`` () =
    match TestValidator.greaterThanLen 100 "Test" testList with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Success`` () =
    match TestValidator.lessThanLen 100 "Test" testList with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Failure`` () =
    match TestValidator.lessThanLen 0 "Test" testList with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =
    match TestValidator.notEmpty "Test" testList with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Failure`` () =
    match TestValidator.notEmpty "Test" empty with
    | Ok _ -> false
    | Error _ -> true
