namespace Validus

open System
open System.Collections.Generic

/// A mapping of fields and errors
type ValidationErrors = private { ValidationErrors : Map<string, string list> } with
    member internal x.Value = x.ValidationErrors

/// The ValidationResult type represents a choice between success and failure
type ValidationResult<'a> = Result<'a, ValidationErrors>

// ------------
// Validation Errors
// ------------

/// Functions for ValidationErrors type
module ValidationErrors =
    let inline private validationErrors x = { ValidationErrors = x }

    /// Create a new ValidationErrors instance from a field  and errors list
    let create (field : string) (errors : string list) : ValidationErrors =
        [ field, errors ] |> Map.ofList |> validationErrors

    /// Combine a list of ValidationErrors
    let collect (errors : ValidationErrors list) =
        let dict = Dictionary<string, string list>()
        for e in errors do
            for x in e.Value do
                if dict.ContainsKey(x.Key) then dict.[x.Key] <- List.concat [ dict.[x.Key]; x.Value ]
                else dict.Add(x.Key, x.Value)

        (dict :> seq<_>)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
        |> validationErrors

    /// Combine two ValidationErrors instances
    let merge (e1 : ValidationErrors) (e2 : ValidationErrors) : ValidationErrors =
        collect [ e1; e2 ]

    /// Unwrap ValidationErrors into a standard Map<string, string list>
    let toMap (e : ValidationErrors) : Map<string, string list> =
        e.Value

    /// Unwrap ValidationErrors and collection individual errors into
    /// string list, excluding keys
    let toList (e : ValidationErrors) : string list =
        e
        |> toMap
        |> Seq.collect (fun kvp -> kvp.Value)
        |> List.ofSeq

// ------------
// Validation Results
// ------------

/// Functions for ValidationResult type
module ValidationResult =
    /// Unpack ValidationResult and feed into validation function
    let apply
        (resultFn : ValidationResult<'a -> 'b>)
        (result : ValidationResult<'a>)
        : ValidationResult<'b> =
        match resultFn, result with
        | Ok fn, Ok x  -> fn x |> Ok
        | Error e, Ok _   -> Error e
        | Ok _, Error e   -> Error e
        | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)

    /// Create a tuple form ValidationResult, if two ValidationResult objects
    /// are in Ok state, otherwise return failure
    let zip
        (r1 : ValidationResult<'a>)
        (r2 : ValidationResult<'b>)
        : ValidationResult<'a * 'b> =
        match r1, r2 with
        | Ok x1res, Ok x2res -> Ok (x1res, x2res)
        | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)
        | Error e, _         -> Error e
        | _, Error e         -> Error e