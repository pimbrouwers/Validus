module Validus.SequenceValidator.Tests

open FsCheck.Xunit
open Validus
open Xunit

let private TestValidator = Check.Seq
let private testSequence = seq { 1..10 }
let private empty: int seq = seq []

[<Fact>]
let ``SequenceValidator preserves input type on sucess`` () =
    match Check.Array.notEmpty "Test" [|1|], Check.List.notEmpty "Test" [1], Check.Seq.notEmpty "Test" (seq {1}) with
    | Ok (x : int array), Ok (y : int list), Ok (z : int seq) -> true
    | _ -> false

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Success`` () =
    match TestValidator.betweenLen 0 100 "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.betweenLen min max) should produce Failure`` () =
    match TestValidator.betweenLen 100 1000 "Test" testSequence with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =
    match TestValidator.empty "Test" empty with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Failure`` () =
    match TestValidator.empty "Test" testSequence with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.equalsLen len) should produce Success`` () =
    match TestValidator.equalsLen 10 "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Success`` () =
    match TestValidator.greaterThanLen 0 "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanLen min) should produce Failure`` () =
    match TestValidator.greaterThanLen 100 "Test" testSequence with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.greaterThanOrEqualToLen min) should produce Success`` () =
    match TestValidator.greaterThanOrEqualToLen 0 "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.greaterThanOrEqualToLen min) should produce Failure`` () =
    match TestValidator.greaterThanOrEqualToLen 100 "Test" testSequence with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Success`` () =
    match TestValidator.lessThanLen 100 "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.lessThanLen min) should produce Failure`` () =
    match TestValidator.lessThanLen 0 "Test" testSequence with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.lessThanOrEqualToLen min) should produce Success`` () =
    match TestValidator.lessThanOrEqualToLen 100 "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.lessThanOrEqualToLen min) should produce Failure`` () =
    match TestValidator.lessThanOrEqualToLen 0 "Test" testSequence with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =
    match TestValidator.notEmpty "Test" testSequence with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Failure`` () =
    match TestValidator.notEmpty "Test" empty with
    | Ok _ -> false
    | Error _ -> true
