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

[<Fact>]
let ``ValidationResult.create produces Ok result`` () =    
    ValidationResult.create true () ValidationErrors.empty    
    |> ValidationResult.map (fun result -> result |> should equal ())

[<Fact>]
let ``ValidationResult.create produces Error result`` () =    
    let expected = ValidationErrors.create "fakeField" [ "fake error message" ]
    
    match ValidationResult.create false () expected with
    | Success _ -> ()
    | Failure e -> e |> should equal expected
    
[<Fact>]
let ``Validation of record succeeds`` () =        
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }    
    let result : ValidationResult<FakeValidationRecord> = 
        let nameValidator =             
            Validators.String.greaterThanLen 2 None
            <+> Validators.String.lessThanLen 100 None
            <+> Validators.String.equals expected.Name None

        FakeValidationRecord.Create
        <!> nameValidator "Name" expected.Name       
        <*> Validators.Int.greaterThan 0 None "Age" 1
    
    result 
    |> ValidationResult.toResult
    |> Result.bind (fun r -> Ok(r |> should equal expected))

[<Fact>]
let ``Validation of record with option succeeds`` () =        
    let expected : FakeValidationRecordWithOption = { Name = "John"; Age = None }
    let result : ValidationResult<FakeValidationRecordWithOption> = 
        let nameValidator = 
            Validators.String.greaterThanLen 2 None "Name" expected.Name

        let ageValidator = 
            Validators.optional 
                (Validators.Int.greaterThan 0 None <+> Validators.Int.lessThan 100 None) 
                "Age" 
                expected.Age

        fun name age -> { 
            Name = name
            Age = age
        }
        <!> nameValidator
        <*> ageValidator
    
    result 
    |> ValidationResult.toResult
    |> Result.bind (fun r -> Ok (r |> should equal expected))

[<Fact>]
let ``Validation of record fails`` () =       
    let name = "Jo"
    let age = 3
    let result : ValidationResult<FakeValidationRecord> =         
        let nameValidator =             
            Validators.String.greaterThanLen 2 None
            <+> Validators.String.lessThanLen 100 None

        fun name age -> { 
            Name = name
            Age = age
        }
        <!> nameValidator "Name" name
        <*> Validators.Int.greaterThan 3 (Some (sprintf "%s must be greater than 3")) "Age" age
    
    result 
    |> ValidationResult.toResult
    |> Result.mapError (fun r -> 
        let rMap = ValidationErrors.toMap r
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])
