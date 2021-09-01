module Validus.ComparisonValidator.Tests

open FsCheck
open FsCheck.Xunit
open Validus

let private TestValidator = Validators.Default.DefaultComparisonValidator(Validators.ComparisonValidator<int>())

[<Property>]
let ``(TestValidator.between min max) should produce Success`` (NonZeroInt min) =           
    let max = min + 100000
    let v = min + 50000
    match TestValidator.between min max "Test" v with
    | Ok _ -> true
    | _ -> false

[<Property>]
let ``(TestValidator.between min max) should produce Failure`` (NonZeroInt min) =           
    let max = min + 100000
    let v = min + 150000
    match TestValidator.between min max "Test" v with
    | Ok _ -> false
    | _ -> true

[<Property>]
let ``(TestValidator.graterThan min) should produce Success`` (NonZeroInt min) =           
    let v = min + 50000
    match TestValidator.greaterThan min "Test" v with
    | Ok _ -> true
    | _ -> false   
    
[<Property>]
let ``(TestValidator.graterThan min) should produce Failure`` (NonZeroInt min) =           
    let v = min - 50000
    match TestValidator.greaterThan min "Test" v with
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
let ``(TestValidator.lessThan max) should produce Failure`` (NonZeroInt min) =           
    let max = min + 100000
    let v = min + 150000
    match TestValidator.lessThan max "Test" v with
    | Ok _ -> false
    | _ -> true