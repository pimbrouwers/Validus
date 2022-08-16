module Validus.ValidatorOperations.Tests

open Xunit
open Validus
open Validus.Operators
open FsUnit.Xunit
open FsToolkit.ErrorHandling

let notStartsWithWhiteSpace fieldName (s: string) =
    if s.StartsWith ' '
    then Error <| ValidationErrors.create fieldName [ $"%s{fieldName} can't start with whitespace" ]
    else Ok <| s

let notEndsWithWhiteSpace fieldName (s: string) =
    if s.EndsWith ' '
    then Error <| ValidationErrors.create fieldName [ $"%s{fieldName} can't end with whitespace" ]
    else Ok <| s

let compositionValidator = notStartsWithWhiteSpace <+> notEndsWithWhiteSpace

let chainValidator = Validators.Default.String.notEmpty >=> compositionValidator

[<Fact>]
let ``Can compose validators`` () =

    compositionValidator "Name" " text"
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"] |> should equal ["Name can't start with whitespace"])
    |> ignore

    compositionValidator "Name" "text "
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"] |> should equal ["Name can't end with whitespace"])
    |> ignore

    compositionValidator "Name" " text "
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"] |> should equal
            ["Name can't start with whitespace"
             "Name can't end with whitespace"])
    |> ignore

[<Fact>]
let ``Can chain validators`` () =

    chainValidator "Name" Unchecked.defaultof<string>
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"] |> should equal ["Name must not be empty"])
    |> ignore

    chainValidator "Name" " text"
    |> Result.mapError (fun r ->
        let rMap = ValidationErrors.toMap r
        rMap.ContainsKey "Name" |> should equal true
        rMap.["Name"] |> should equal ["Name can't start with whitespace"])
    |> ignore
