module Validus.GuidValidator.Tests

open System
open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Validators.Default.DefaultGuidValidator(Validators.GuidValidator())
let private testGuid = Guid.NewGuid ()

[<Property>]
let ``(TestValidator.empty) should produce Success`` () =           
    match TestValidator.empty "Test" Guid.Empty with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.empty) should produce Failure`` () =           
    match TestValidator.empty "Test" testGuid with
    | Success _ -> false
    | Failure _ -> true

[<Property>]
let ``(TestValidator.notEmpty) should produce Success`` () =           
    match TestValidator.notEmpty "Test" testGuid with
    | Success _ -> true
    | Failure _ -> false

[<Property>]
let ``(TestValidator.notEmpty) should produce Failure`` () =           
    match TestValidator.notEmpty "Test" Guid.Empty with
    | Success _ -> false
    | Failure _ -> true