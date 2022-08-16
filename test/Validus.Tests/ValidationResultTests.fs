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
let ``Can bind ValidationResults`` () =
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }

    let result : Result<string, ValidationErrors> =
        let validator =
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100


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
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100
            <+> Validators.Default.String.equals expected.Name

        validate {
            let! name = nameValidator "Name" expected.Name
            and! age = Validators.Default.Int.greaterThan 0 "Age" 1
            return FakeValidationRecord.Create name age
        }

    result
    |> Result.bind (fun r -> Ok(r |> should equal expected))

[<Fact>]
let ``Validation of record with option succeeds`` () =
    let expected : FakeValidationRecordWithOption = { Name = "John"; Age = None }
    let result : Result<FakeValidationRecordWithOption, ValidationErrors> =
        let nameValidator =
            Validators.Default.String.greaterThanLen 2 "Name" expected.Name

        let ageValidator =
            let validator =
                Validators.Default.Int.greaterThan 0 <+> Validators.Default.Int.lessThan 100

            Validators.optional
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
            Validators.Default.String.greaterThanLen 2 "Name" expected.Name

        let ageValidator =
            let validator =
                Validators.Default.Int.greaterThan 0 <+> Validators.Default.Int.lessThan 100

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
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100

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
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100

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
        |> Validators.Default.String.betweenLen 2 16 field
        |> Result.map (fun v -> { Str16 = v.Trim() })

[<Fact>]
let ``Validation supports transformation at the point of marking as optional`` () =
    let name = Some "pim"
    let result = Validators.optional Str16.Of "First Name" name

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
    let result = Validators.Default.required Str16.Of "First Name" name

    result
    |> Result.bind (fun r -> Ok (r |> should equal { Str16 = "pim" }))

[<Fact>]
let ``Validation supports transformation at the point of marking as vrequired`` () =
    let name = ValueSome "pim"
    let result = Validators.Default.vrequired Str16.Of "First Name" name

    result
    |> Result.bind (fun r -> Ok (r |> should equal { Str16 = "pim" }))
