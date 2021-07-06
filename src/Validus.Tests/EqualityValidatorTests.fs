module Validus.EqaulityValidator.Tests

open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Validators.Default.DefaultEqualityValidator(Validators.EqualityValidator<int>())

[<Property>]
let ``(TestValidator.equals n) should produce Success`` (NonZeroInt n) =           
    match TestValidator.equals n "Test" n with
    | Success _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.equals n) should produce Failure`` (NonZeroInt n) =           
    match TestValidator.equals n "Test" 0 with
    | Failure _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.notEquals n) should produce Success`` (NonZeroInt n) =           
    match TestValidator.notEquals n "Test" 0 with
    | Success _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.notEquals n) should produce Failure`` (NonZeroInt n) =           
    match TestValidator.notEquals n "Test" n with
    | Failure _ -> true
    | _ -> false
    