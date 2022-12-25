namespace Validus

open System
open System.Collections.Generic

/// A validation message for a field
type ValidationMessage = string -> string

/// Given a value, return true/false to indicate validity
type ValidationRule<'a> = 'a -> bool

/// A mapping of fields and errors
type ValidationErrors = private { ValidationErrors : Map<string, string list> } with
    member internal x.Value = x.ValidationErrors

/// The ValidationResult type represents a choice between success and failure
type ValidationResult<'a> = Result<'a, ValidationErrors>

/// Given a field name and value, 'a, produces a ValidationResult<'a>
type Validator<'a, 'b> = string -> 'a -> ValidationResult<'b>

/// Validation rules
module ValidationRule =
    let inline equality<'a when 'a : equality> (equalTo : 'a) : ValidationRule<'a> =
        fun v -> v = equalTo

    let inline inequality<'a when 'a : equality> (notEqualTo : 'a) : ValidationRule<'a> =
        fun v -> not(v = notEqualTo)

    let inline between<'a when 'a : comparison> (min : 'a) (max : 'a) : ValidationRule<'a> =
        fun v -> v >= min && v <= max

    let inline greaterThan<'a when 'a : comparison> (min : 'a) : ValidationRule<'a> =
        fun v -> v > min

    let inline greaterThanOrEqualTo<'a when 'a : comparison> (min : 'a) : ValidationRule<'a> =
        fun v -> v >= min

    let inline lessThan<'a when 'a : comparison> (max : 'a) : ValidationRule<'a> =
        fun v -> v < max

    let inline lessThanOrEqualTo<'a when 'a : comparison> (max : 'a) : ValidationRule<'a> =
        fun v -> v <= max

    let inline betweenLen (min : int) (max : int) (x : ^a) : bool =
        (between min max (^a : (member Length : int) x))

    let inline equalsLen (len : int) (x : ^a) : bool =
        (equality len (^a : (member Length : int) x))

    let inline greaterThanLen (min : int) (x : ^a) : bool =
        (greaterThan min (^a : (member Length : int) x))

    let inline greaterThanOrEqualToLen (min : int) (x : ^a) : bool =
        (greaterThanOrEqualTo min (^a : (member Length : int) x))

    let inline lessThanLen (max : int) (x : ^a) : bool =
        (lessThan max (^a : (member Length : int) x))

    let inline lessThanOrEqualToLen (max : int) (x : ^a) : bool =
        (lessThan max (^a : (member Length : int) x))

    let inline strPattern (pattern : string) : ValidationRule<string> =
        fun v -> if isNull v then false else Text.RegularExpressions.Regex.IsMatch(v, pattern)

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

/// Functions for Validator type
module Validator =
    /// Create a new Validator
    let create
        (message : ValidationMessage)
        (rule : 'a -> bool)
        : Validator<'a, 'a> =
        fun (field : string) (value : 'a) ->
            let error = ValidationErrors.create field [ message field ]
            if rule value then Ok value
            else error |> Error

type ValidatorGroup<'a>(startValidator : Validator<'a, 'a>) =
    member _.Build() = startValidator

    member _.And(andValidator : Validator<'a, 'a>) =
        ValidatorGroup(fun f v ->
            match startValidator f v, andValidator f v with
            | Ok a, Ok _   -> Ok a
            | Error e, Ok _   -> Error e
            | Ok _, Error e   -> Error e
            | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2))

    member _.Then(nextValidator : Validator<'a, 'a>) =
        ValidatorGroup(fun f v ->
            Result.bind (nextValidator f) (startValidator f v))
