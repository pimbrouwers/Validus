module Validus.Validator.Tests

open System

open Xunit
open FsUnit.Xunit

open Validus
open Validus.Tests

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
        ValidatorGroup(Check.String.notEmpty)
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
            ValidatorGroup(Check.String.greaterThanLen 2)
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
            ValidatorGroup(Check.String.greaterThanLen 2)
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
                ValidatorGroup(Check.Int.greaterThan 0)
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
                ValidatorGroup(Check.Int.greaterThan 0)
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
            ValidatorGroup(Check.String.greaterThanLen 2)
                .And(Check.String.lessThanLen 100)
                .Build()

        validate {
            let! name = nameValidator "Name" name
            and! age = Check.WithMessage.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age

            let result = FakeValidationRecord.Create name age

            return result
        }

    result
    |> ValidationResult.mapError (fun rMap ->
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

[<Fact>]
let ``Validation of record fails with computation expression`` () =
    let name = "Jo"
    let age = 3
    let result : Result<FakeValidationRecord, ValidationErrors> =
        let nameValidator =
            ValidatorGroup(Check.String.greaterThanLen 2)
                .And(Check.String.lessThanLen 100)
                .Build()

        validate {
            let! name = nameValidator "Name" name
            and! age = Check.WithMessage.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age
            return FakeValidationRecord.Create name age
        }

    result
    |> ValidationResult.mapError (fun rMap ->
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

[<Fact>]
let ``ValidatorGroup works with both And() and Then()`` () =

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
        ValidatorGroup(Check.String.betweenLen 8 512)
            .And(emailPatternValidator)
            .Then(notEqualsValidator)
            .Build()

    // too short failure
    "fake"
    |> emailValidator "Login"
    |> ValidationResult.mapError (fun rMap ->
        rMap.ContainsKey "Login" |> should equal true
        rMap.["Login"] |> should equal [
            "'Login' must be between 8 and 512 characters"
            "Please provide a valid Login" ])
    |> ignore

    // pattern failure
    "fake@test"
    |> emailValidator "Login"
    |> ValidationResult.mapError (fun rMap ->
        rMap.ContainsKey "Login" |> should equal true
        rMap.["Login"] |> should equal [ "Please provide a valid Login" ])
    |> ignore

    // notEquals failure
    "fake@test.com"
    |> emailValidator "Login"
    |> ValidationResult.mapError (fun rMap ->
        rMap.ContainsKey "Login" |> should equal true
        rMap.["Login"].Length |> should equal 1
        rMap.["Login"] |> should equal ["Login must not equal fake@test.com"] )
    |> ignore

[<Fact>]
let ``Validators.Default use correct built-in messages`` () =
    Check.Int.equals -1 "equals" 1 |> Result.containsErrorValue (ValidationMessages.equals "equals" -1) |> ignore
    Check.Int.notEquals -1 "notEquals" -1 |> Result.containsErrorValue (ValidationMessages.notEquals "notEquals" -1) |> ignore
    Check.Int.between -1 0 "between" 1 |> Result.containsErrorValue (ValidationMessages.between "between" -1 0) |> ignore
    Check.Int.greaterThan -1 "greaterThan" -2 |> Result.containsErrorValue (ValidationMessages.greaterThan "greaterThan" -1) |> ignore
    Check.Int.greaterThanOrEqualTo -1 "greaterThanOrEqualTo" -2 |> Result.containsErrorValue (ValidationMessages.greaterThanOrEqualTo "greaterThanOrEqualTo" -1) |> ignore
    Check.Int.lessThan -1 "lessThan" 1 |> Result.containsErrorValue (ValidationMessages.lessThan "lessThan" -1) |> ignore
    Check.Int.lessThanOrEqualTo -1 "lessThanOrEqualTo" 1 |> Result.containsErrorValue (ValidationMessages.lessThanOrEqualTo "lessThanOrEqualTo" -1) |> ignore

    Check.String.betweenLen 0 1 "betweenLen" "betweenLen" |> Result.containsErrorValue (ValidationMessages.strBetweenLen "betweenLen" 0 1) |> ignore
    Check.String.empty "empty" "empty" |> Result.containsErrorValue (ValidationMessages.strEmpty "empty") |> ignore
    Check.String.equalsLen 0 "equalsLen" "equalsLen" |> Result.containsErrorValue (ValidationMessages.strEqualsLen "equalsLen" 0) |> ignore
    Check.String.greaterThanLen 99 "greaterThanLen" "greaterThanLen" |> Result.containsErrorValue (ValidationMessages.strGreaterThanLen "greaterThanLen" 99) |> ignore
    Check.String.greaterThanOrEqualToLen 99 "greaterThanOrEqualToLen" "greaterThanOrEqualToLen" |> Result.containsErrorValue (ValidationMessages.strGreaterThanOrEqualToLen "greaterThanOrEqualToLen" 99) |> ignore
    Check.String.lessThanLen 0 "lessThanLen" "lessThanLen" |> Result.containsErrorValue (ValidationMessages.strLessThanLen "lessThanLen" 0) |> ignore
    Check.String.lessThanOrEqualToLen 0 "lessThanOrEqualToLen" "lessThanOrEqualToLen" |> Result.containsErrorValue (ValidationMessages.strLessThanOrEqualToLen "lessThanOrEqualToLen" 0) |> ignore
    Check.String.notEmpty "notEmpty" "" |> Result.containsErrorValue (ValidationMessages.strNotEmpty "notEmpty") |> ignore
    Check.String.pattern "@[0-9]+" "pattern" "pattern" |> Result.containsErrorValue (ValidationMessages.strPattern "pattern" @"[0-9]+") |> ignore

    Check.Guid.empty "empty" (Guid.NewGuid ()) |> Result.containsErrorValue (ValidationMessages.guidEmpty "empty") |> ignore
    Check.Guid.notEmpty "notEmpty" Guid.Empty |> Result.containsErrorValue (ValidationMessages.guidNotEmpty "notEmpty") |> ignore

    Check.Option.isNone "isNone" (Some ()) |> Result.containsErrorValue (ValidationMessages.guidEmpty "isNone") |> ignore
    Check.Option.isSome "isSome" None |> Result.containsErrorValue (ValidationMessages.guidNotEmpty "isSome") |> ignore

    Check.VOption.isNone "isNone" (ValueSome ()) |> Result.containsErrorValue (ValidationMessages.guidEmpty "isNone") |> ignore
    Check.VOption.isSome "isSome" ValueNone |> Result.containsErrorValue (ValidationMessages.guidNotEmpty "isSome") |> ignore

    Check.Seq.betweenLen 99 100 "betweenLen" [] |> Result.containsErrorValue (ValidationMessages.seqBetweenLen "betweenLen" 99 100) |> ignore
    Check.Seq.empty "empty" [1] |> Result.containsErrorValue (ValidationMessages.seqEmpty "empty") |> ignore
    Check.Seq.equalsLen 99 "equalsLen" [] |> Result.containsErrorValue (ValidationMessages.seqEqualsLen "equalsLen" 9) |> ignore
    Check.Seq.exists (fun (n : int) -> n = 2) "exists" [1] |> Result.containsErrorValue (ValidationMessages.seqExists "exists") |> ignore
    Check.Seq.greaterThanLen 99 "greaterThanLen" [] |> Result.containsErrorValue (ValidationMessages.seqGreaterThanLen "greaterThanLen" 99) |> ignore
    Check.Seq.greaterThanOrEqualToLen 99 "greaterThanOrEqualToLen" [] |> Result.containsErrorValue (ValidationMessages.seqGreaterThanOrEqualToLen "greaterThanOrEqualToLen" 99) |> ignore
    Check.Seq.lessThanLen -1 "lessThanLen" [] |> Result.containsErrorValue (ValidationMessages.seqLessThanLen "lessThanLen" -1) |> ignore
    Check.Seq.lessThanOrEqualToLen -1 "lessThanOrEqualToLen" [] |> Result.containsErrorValue (ValidationMessages.seqLessThanOrEqualToLen "lessThanOrEqualToLen" -1) |> ignore
    Check.Seq.notEmpty "notEmpty" [1] |> Result.containsErrorValue (ValidationMessages.seqNotEmpty "notEmpty") |> ignore