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
let ``Can bind ValidationResults`` () =
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }    

    let result : ValidationResult<string> =         
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
            | Success x -> Success {|Name = x |}
            | Failure e -> Failure e

        validate {
            let! result = result
            return! validator result
        }
    
    result3    
    |> ValidationResult.toResult
    |> Result.bind (fun r -> Ok(r |> should equal {|Name = expected.Name|}))
    
[<Fact>]
let ``Validation of record succeeds using computation expression`` () =        
    let expected : FakeValidationRecord = { Name = "John"; Age = 1 }    
    let result : ValidationResult<FakeValidationRecord> = 
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
    |> ValidationResult.toResult
    |> Result.bind (fun r -> Ok(r |> should equal expected))

[<Fact>]
let ``Validation of record with option succeeds`` () =        
    let expected : FakeValidationRecordWithOption = { Name = "John"; Age = None }
    let result : ValidationResult<FakeValidationRecordWithOption> = 
        let nameValidator = 
            Validators.Default.String.greaterThanLen 2 "Name" expected.Name

        let ageValidator = 
            Validators.optional 
                (Validators.Default.Int.greaterThan 0 <+> Validators.Default.Int.lessThan 100) 
                "Age" 
                expected.Age

        FakeValidationRecordWithOption.Create
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
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100

        FakeValidationRecord.Create
        <!> nameValidator "Name" name
        <*> Validators.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age
    
    result 
    |> ValidationResult.toResult
    |> Result.mapError (fun r -> 
        let rMap = ValidationErrors.toMap r
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

[<Fact>]
let ``Validation of record fails with computation expression`` () =           
    let name = "Jo"
    let age = 3
    let result : ValidationResult<FakeValidationRecord> =         
        let nameValidator =             
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100

        validate {
            let! name = nameValidator "Name" name
            and! age = Validators.Int.greaterThan 3 (sprintf "%s must be greater than 3") "Age" age
            return FakeValidationRecord.Create name age
        }
    
    result 
    |> ValidationResult.toResult
    |> Result.mapError (fun r -> 
        let rMap = ValidationErrors.toMap r
        (rMap.ContainsKey "Name", rMap.ContainsKey "Age") |> should equal (true, true)
        rMap.["Age"] |> should equal ["Age must be greater than 3"])

[<Fact>]
let ``Validation of record fails and can be flattened`` () =           
    let name = "Jo"    
    let result : ValidationResult<string> =         
        let nameValidator =             
            Validators.Default.String.greaterThanLen 2
            <+> Validators.Default.String.lessThanLen 100
        
        validate {
            let! name = nameValidator "Name" name
            return name
        }
    
    let flattened = ValidationResult.flatten result     

    flattened 
    |> should be instanceOfType<Result<string, string list>>

    flattened 
    |> Result.mapError(fun r -> r |> should haveLength 1)

[<Fact>]
let ``Validation of multiple record fails and can be sequenced`` () =    
    let names = [ "Jo"; "Jim"; "Lo"; "Bob" ]
    
    let nameValidator =             
        Validators.Default.String.greaterThanLen 2
        <+> Validators.Default.String.lessThanLen 100

    let validator name =           
        validate {
            let! name = nameValidator "Name" name
            return name
        }

    let result = 
        names 
        |> List.map validator

    let sequenced =
        result
        |> ValidationResult.sequence

    result 
    |> should be instanceOfType<ValidationResult<string> seq>

    sequenced
    |> should be instanceOfType<ValidationResult<string seq>>