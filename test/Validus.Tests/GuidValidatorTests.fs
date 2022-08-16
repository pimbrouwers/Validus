module Validus.GuidValidator.Tests

open System
open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Validators.Default.Guid
let private testGuid = Guid.NewGuid ()

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =
    match TestValidator.empty "Test" Guid.Empty with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Failure`` () =
    match TestValidator.empty "Test" testGuid with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =
    match TestValidator.notEmpty "Test" testGuid with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Failure`` () =
    match TestValidator.notEmpty "Test" Guid.Empty with
    | Ok _ -> false
    | Error _ -> true