module Validus.EqaulityValidator.Tests

open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Check.Int

[<Property>]
let ``(TestValidator.equals n) should produce Success`` (NonZeroInt n) =
    match TestValidator.equals n "Test" n with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.equals n) should produce Error`` (NonZeroInt n) =
    match TestValidator.equals n "Test" 0 with
    | Error _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.notEquals n) should produce Success`` (NonZeroInt n) =
    match TestValidator.notEquals n "Test" 0 with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.notEquals n) should produce Error`` (NonZeroInt n) =
    match TestValidator.notEquals n "Test" n with
    | Error _ -> true
    | _ -> false
