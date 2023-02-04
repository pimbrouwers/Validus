module Validus.ComparisonValidator.Tests

open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Check.Int

[<Property>]
let ``(TestValidator.between min max) should produce Success`` (NonZeroInt min) =
    let max = min + 100000
    let v = min + 50000
    match TestValidator.between min max "Test" v with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.between min max) should produce Error`` (NonZeroInt min) =
    let max = min + 100000
    let v = min + 150000
    match TestValidator.between min max "Test" v with
    | Ok _ -> false
    | _ -> true

[<Property>]
let ``(TestValidator.greaterThan min) should produce Success`` (NonZeroInt min) =
    let v = min + 50000
    match TestValidator.greaterThan min "Test" v with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.greaterThan min) should produce Error`` (NonZeroInt min) =
    let v = min - 50000
    match TestValidator.greaterThan min "Test" v with
    | Ok _ -> false
    | _ -> true

[<Property>]
let ``(TestValidator.greaterThanOrEqualTo min) should produce Success`` (NonNegativeInt min) =
    let v = min + 50000
    match TestValidator.greaterThanOrEqualTo min "Test" v with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.greaterThanOrEqualTo min) should produce Error`` (NonNegativeInt min) =
    let v = min - 50000
    match TestValidator.greaterThanOrEqualTo min "Test" v with
    | Ok _ -> false
    | _ -> true

[<Property>]
let ``(TestValidator.lessThan max) should produce Success`` (NonZeroInt min) =
    let max = min + 100000
    let v = min + 50000
    match TestValidator.lessThan max "Test" v with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.lessThan max) should produce Error`` (NonZeroInt min) =
    let max = min + 100000
    let v = min + 150000
    match TestValidator.lessThan max "Test" v with
    | Ok _ -> false
    | _ -> true

[<Property>]
let ``(TestValidator.lessThanOrEqualTo max) should produce Success`` (NonNegativeInt min) =
    let max = min + 100000
    let v = min + 50000
    match TestValidator.lessThanOrEqualTo max "Test" v with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.lessThanOrEqualTo max) should produce Error`` (NonNegativeInt min) =
    let max = min + 100000
    let v = min + 150000
    match TestValidator.lessThanOrEqualTo max "Test" v with
    | Ok _ -> false
    | _ -> true
