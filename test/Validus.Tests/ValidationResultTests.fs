module Validus.ValidationResult.Tests

open Xunit
open Validus
open Validus.Operators
open FsUnit.Xunit

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

    let validator = Check.String.notEmpty <+> notStartsWithWhiteSpace

    match validator "Test" null with
    | Ok _ -> false
    | Error _ -> true

[<Fact>]
let ``Can chain Validator's together`` () =
    let validator =
        Check.String.notEmpty
        >=> Check.String.greaterThanLen 7

    let result = validator "Name" ""

    result
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"].Length |> should equal 1)

[<Fact>]
let ``Can compose Validator's together`` () =
    let validator =
        Check.String.notEmpty
        <+> Check.String.greaterThanLen 7

    let result = validator "Name" ""

    result
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"].Length |> should equal 2)

[<Fact>]
let ``Can chain & compose Validator's together`` () =
    let validator =
        Check.String.notEmpty
        >=> (Check.String.pattern "^\S" // does not start with space
        <+> Check.String.pattern "\S$") // does not end with space

    let result = validator "Name" null

    result
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"].Length |> should equal 1)
    |> ignore

    let result2 = validator "Name" " Validus "

    result2
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"].Length |> should equal 2)
    |> ignore

[<Fact>]
let ``Can bind ValidationResults`` () =
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }

    let result : Result<string, ValidationErrors> =
        let validator =
            Check.String.greaterThanLen 2
            <+> Check.String.lessThanLen 100


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
            Check.String.greaterThanLen 2
            <+> Check.String.lessThanLen 100
            <+> Check.String.equals expected.Name

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
                Check.Int.greaterThan 0 <+> Check.Int.lessThan 100

            Check.option
                validator
                "Age"
                expected.Age

        FakeValidationRecordWithOption.Create
        <!> nameValidator
        <*> ageValidator

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
                Check.Int.greaterThan 0 <+> Check.Int.lessThan 100

            Validators.voptional
                validator
                "Age"
                expected.Age

        FakeValidationRecordWithValueOption.Create
        <!> nameValidator
        <*> ageValidator

    result
    |> Result.bind (fun r -> Ok (r |> should equal expected))

[<Fact>]
let ``Validation of record fails`` () =
    let name = "Jo"
    let age = 3
    let result : Result<FakeValidationRecord, ValidationErrors> =
        let nameValidator =
            Check.String.greaterThanLen 2
            <+> Check.String.lessThanLen 100

        FakeValidationRecord.Create
        <!> nameValidator "Name" name
        <*> Validators.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age

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
            Check.String.greaterThanLen 2
            <+> Check.String.lessThanLen 100

        validate {
            let! name = nameValidator "Name" name
            and! age = Validators.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age
            return FakeValidationRecord.Create name age
        }

    result
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

type Str16 =
    private { Str16 : string }

    override x.ToString () = x.Str16

    static member Of field input =
        input
        |> Check.String.betweenLen 2 16 field
        |> Result.map (fun v -> { Str16 = v.Trim() })

[<Fact>]
let ``Validation supports transformation at the point of marking as optional`` () =
    let name = Some "pim"
    let result = Check.option Str16.Of "First Name" name

    result
    |> Result.bind (fun r -> Ok (r |> should equal (Some { Str16 = "pim" })))

[<Fact>]
let ``Validation supports transformation at the point of marking as voptional`` () =
    let name = ValueSome "pim"
    let result = Validators.voptional Str16.Of "First Name" name

    result
    |> Result.bind (fun r -> Ok (r |> should equal (ValueSome { Str16 = "pim" })))

[<Fact>]
let ``Validation supports transformation at the point of marking as required`` () =
    let name = Some "pim"
    let result = Check.required Str16.Of "First Name" name

    result
    |> Result.bind (fun r -> Ok (r |> should equal { Str16 = "pim" }))

[<Fact>]
let ``Validation supports transformation at the point of marking as vrequired`` () =
    let name = ValueSome "pim"
    let result = Check.vrequired Str16.Of "First Name" name

    result
    |> Result.bind (fun r -> Ok (r |> should equal { Str16 = "pim" }))
