module Validus.ValidationErrors.Tests

open Xunit
open Validus
open FsUnit.Xunit

[<Fact>]
let ``ValidationErrors.empty produces empty Map<string, string list>`` () =
    ValidationErrors.empty |> ValidationErrors.toMap |> should equal Map.empty<string, string list>

[<Fact>]
let ``ValidationErrors.create produce Map<string, string list> from field and errors`` () =
    let expected = [ "fakeField1", [ "fake error message 1" ] ] |> Map.ofList
    let error = ValidationErrors.create "fakeField1" [ "fake error message 1" ]
    error |> ValidationErrors.toMap |> should equal expected    

[<Fact>]
let ``ValidationErrors.merge produces Map<string, string list> from two source`` () =
    let expected = [ "fakeField1", [ "fake error message 1" ]; "fakeField2", [ "fake error message 2" ] ] |> Map.ofList
    let error = ValidationErrors.create "fakeField1" [ "fake error message 1" ]
    let error2 = ValidationErrors.create "fakeField2" [ "fake error message 2" ]

    ValidationErrors.merge error error2
    |> ValidationErrors.toMap 
    |> should equal expected

[<Fact>]
let ``ValidationErrors.merge produces Map<string, string list> from two sources with same key`` () =
    let expected = [ "fakeField1", ["fake error message 1"; "fake error message 2" ] ] |> Map.ofList
    let error = ValidationErrors.create "fakeField1" [ "fake error message 1" ]
    let error2 = ValidationErrors.create "fakeField1" [ "fake error message 2" ]

    ValidationErrors.merge error error2
    |> ValidationErrors.toMap 
    |> should equal expected