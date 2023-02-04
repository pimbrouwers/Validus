module Validus.GuidValidator.Tests

open System
open FsCheck.Xunit
open Validus

let private TestValidator = Check.Guid
let private testGuid = Guid.NewGuid ()

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =
    match TestValidator.empty "Test" Guid.Empty with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Error`` () =
    match TestValidator.empty "Test" testGuid with
    | Ok _ -> false
    | Error _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =
    match TestValidator.notEmpty "Test" testGuid with
    | Ok _ -> true
    | Error _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Error`` () =
    match TestValidator.notEmpty "Test" Guid.Empty with
    | Ok _ -> false
    | Error _ -> true