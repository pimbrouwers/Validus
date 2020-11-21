module Validus.Tests

open Xunit
open Validus
open Validus.Operators
open FsUnit.Xunit

type FakeValidationRecord = { Name : string; Age : int }
type FakeValidationRecordWithOption = { Name : string; Age : int option }

[<Fact>]
let ``ValidationError.merges produces Map<string, string list> from two source`` () =
    let expected = [ "fakeField1", [ "fake error message 1" ]; "fakeField2", [ "fake error message 2" ] ] |> Map.ofList
    let error = ValidationErrors.create "fakeField1" [ "fake error message 1" ]
    let error2 = ValidationErrors.create "fakeField2" [ "fake error message 2" ]

    ValidationErrors.merge error error2
    |> should equal expected

[<Fact>]
let ``ValidationError.merges produces Map<string, string list> from two sources with same key`` () =
    let expected = [ "fakeField1", ["fake error message 1"; "fake error message 2" ] ] |> Map.ofList
    let error = ValidationErrors.create "fakeField1" [ "fake error message 1" ]
    let error2 = ValidationErrors.create "fakeField1" [ "fake error message 2" ]

    ValidationErrors.merge error error2
    |> should equal expected

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

        fun name age -> { 
            Name = name
            Age = age
        }
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
    |> ValidationResult.bind (fun r -> Success(r |> should equal expected))

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
        <*> Validators.Int.greaterThan 3 None "Age" age
    
    result 
    |> ValidationResult.toResult
    |> Result.mapError (fun r -> (r.ContainsKey "Name", r.ContainsKey "Age") |> should equal (true, true))
