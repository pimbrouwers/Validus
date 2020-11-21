module Validus.EqaulityValidator.Tests

open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Validators.EqualityValidator<int>()

[<Property>]
let ``(TestValidator.equals n) should produce Success`` (NonZeroInt n) =           
    match TestValidator.equals n None "Test" n with
    | Success _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.equals n) should produce Failure`` (NonZeroInt n) =           
    match TestValidator.equals n None "Test" 0 with
    | Failure _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.notEquals n) should produce Success`` (NonZeroInt n) =           
    match TestValidator.notEquals n None "Test" 0 with
    | Success _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.notEquals n) should produce Failure`` (NonZeroInt n) =           
    match TestValidator.notEquals n None "Test" n with
    | Failure _ -> true
    | _ -> false
    