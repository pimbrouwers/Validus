module Validus.ValidationResult.Tests

open System
open System.Net.Mail
open Xunit
open FsUnit.Xunit
open Validus
open Validus.Operators

type FakeValidationRecord =
    { Name : string; Age : int }

    static member Create name age =
        { Name = name; Age = age }

type FakeValidationRecordWithOption =
    { Name : string; Age : int option }

    static member Create name age =
        { Name = name; Age = age }

type FakeValidationRecordWithValueOption =
    { Name : string; Age : int voption }

    static member Create name age =
        { Name = name; Age = age }

[<Fact>]
let ``String.empty should produce Success for null`` () =
    match Check.String.empty "Test" null with
    | Ok _ -> true
    | Error _ -> false

[<Fact>]
let ``String.notEmpty should produce Failure for null`` () =
    match Check.String.notEmpty "Test" null with
    | Ok _ -> false
    | Error _ -> true

[<Fact>]
let ``String.notEmpty composed should produce Failure for null`` () =
    let notStartsWithWhiteSpace =
        let rule (x : string) = x <> "foo"
        let msg = sprintf "'%s' should not start with a space"
        Validator.create msg rule

    let validator =
        GroupValidator(Check.String.notEmpty)
            .And(notStartsWithWhiteSpace)
            .Build()

    match validator "Test" null with
    | Ok _ -> false
    | Error _ -> true

[<Fact>]
let ``Can bind ValidationResults`` () =
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }

    let result : Result<string, ValidationErrors> =
        let validator =
            GroupValidator(Check.String.greaterThanLen 2)
                .And(Check.String.lessThanLen 100)
                .Build()

        validate {
            return! validator "Name" expected.Name
        }

    let result3 =
        let rule name = if System.String.Equals(name, expected.Name) then true else false
        let message = fun field -> sprintf "%s must equal %s" field expected.Name
        let validator name =
            match Validator.create message rule "Name" name with
            | Ok x -> Ok {|Name = x |}
            | Error e -> Error e

        validate {
            let! result = result
            return! validator result
        }

    result3
    |> Result.bind (fun r -> Ok(r |> should equal {|Name = expected.Name|}))

[<Fact>]
let ``Validation of record succeeds using computation expression`` () =
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }
    let result : Result<FakeValidationRecord, ValidationErrors> =
        let nameValidator =
            GroupValidator(Check.String.greaterThanLen 2)
                .And(Check.String.lessThanLen 100)
                .And(Check.String.equals expected.Name)
                .Build()

        validate {
            let! name = nameValidator "Name" expected.Name
            and! age = Check.Int.greaterThan 0 "Age" 1
            return FakeValidationRecord.Create name age
        }

    result
    |> Result.bind (fun r -> Ok(r |> should equal expected))

[<Fact>]
let ``Validation of record with option succeeds`` () =
    let expected : FakeValidationRecordWithOption = { Name = "John"; Age = None }
    let result : Result<FakeValidationRecordWithOption, ValidationErrors> =
        let nameValidator =
            Check.String.greaterThanLen 2 "Name" expected.Name

        let ageValidator =
            let validator =
                GroupValidator(Check.Int.greaterThan 0)
                    .And(Check.Int.lessThan 100)
                    .Build()

            Check.optional
                validator
                "Age"
                expected.Age

        validate {
            let! name = nameValidator
            and! age = ageValidator

            let result = FakeValidationRecordWithOption.Create name age

            return result
        }

    result
    |> Result.bind (fun r -> Ok (r |> should equal expected))

[<Fact>]
let ``Validation of record with voption succeeds`` () =
    let expected : FakeValidationRecordWithValueOption = { Name = "John"; Age = ValueNone }
    let result : Result<FakeValidationRecordWithValueOption, ValidationErrors> =
        let nameValidator =
            Check.String.greaterThanLen 2 "Name" expected.Name

        let ageValidator =
            let validator =
                GroupValidator(Check.Int.greaterThan 0)
                    .And(Check.Int.lessThan 100)
                    .Build()

            Check.voptional
                validator
                "Age"
                expected.Age

        validate {
            let! name = nameValidator
            and! age = ageValidator

            let result = FakeValidationRecordWithValueOption.Create name age

            return result
        }

    result
    |> Result.bind (fun r -> Ok (r |> should equal expected))

[<Fact>]
let ``Validation of record fails`` () =
    let name = "Jo"
    let age = 3
    let result : Result<FakeValidationRecord, ValidationErrors> =
        let nameValidator =
            GroupValidator(Check.String.greaterThanLen 2)
                .And(Check.String.lessThanLen 100)
                .Build()

        validate {
            let! name = nameValidator "Name" name
            and! age = Check.WithMessage.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age

            let result = FakeValidationRecord.Create name age

            return result
        }

    result
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

[<Fact>]
let ``Validation of record fails with computation expression`` () =
    let name = "Jo"
    let age = 3
    let result : Result<FakeValidationRecord, ValidationErrors> =
        let nameValidator =
            GroupValidator(Check.String.greaterThanLen 2)
                .And(Check.String.lessThanLen 100)
                .Build()

        validate {
            let! name = nameValidator "Name" name
            and! age = Check.WithMessage.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age
            return FakeValidationRecord.Create name age
        }

    result
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

[<Fact>]
let ``GroupValidator works with both And() and Then()`` () =

    let emailPatternValidator =
        let msg = sprintf "Please provide a valid %s"
        Check.WithMessage.String.pattern "[^@]+@[^\.]+\..+" msg

    let notEqualsValidator =
        let msg = sprintf "%s must not equal fake@test.com"
        let rule (x : string) =
            x <> "fake" &&
            x <> "fake@test" &&
            x <> "fake@test.com"

        Validator.create msg rule

    let emailValidator =
        GroupValidator(Check.String.betweenLen 8 512)
            .And(emailPatternValidator)
            .Then(notEqualsValidator)
            .Build()

    // too short failure
    "fake"
    |> emailValidator "Login"
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Login" |> should equal true
        rMap.["Login"] |> should equal [
            "'Login' must be between 8 and 512 characters"
            "Please provide a valid Login" ])
    |> ignore

    // pattern failure
    "fake@test"
    |> emailValidator "Login"
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Login" |> should equal true
        rMap.["Login"] |> should equal [ "Please provide a valid Login" ])
    |> ignore

    // notEquals failure
    "fake@test.com"
    |> emailValidator "Login"
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Login" |> should equal true
        rMap.["Login"].Length |> should equal 1
        rMap.["Login"] |> should equal ["Login must not equal fake@test.com"] )
    |> ignore