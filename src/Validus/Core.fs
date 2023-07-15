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

/// Validation messages
module ValidationMessages =
    let equals field equalTo = sprintf "'%s' must be equal to %A" field equalTo
    let notEquals field notEqualTo = sprintf "'%s' must not equal %A" field notEqualTo
    let between field min max = sprintf "'%s' must be between %A and %A" field min max
    let greaterThan field min = sprintf "'%s' must be greater than %A" field min
    let greaterThanOrEqualTo field min = sprintf "'%s' must be greater than or equal to %A" field min
    let lessThan field max = sprintf "'%s' must be less than %A" field max
    let lessThanOrEqualTo field max = sprintf "'%s' must be less than or equal to %A" field max

    let strBetweenLen field min max = sprintf "'%s' must be between %i and %i characters" field min max
    let strEmpty field = sprintf "'%s' must be empty" field
    let strEqualsLen field len = sprintf "'%s' must be %i characters" field len
    let strGreaterThanLen field min = sprintf "'%s' must be greater than %i characters" field min
    let strGreaterThanOrEqualToLen field min = sprintf "'%s' must be greater than or equal to %i characters" field min
    let strLessThanLen field max = sprintf "'%s' must be less than %i characters" field max
    let strLessThanOrEqualToLen field max = sprintf "'%s' must be less than or equal to %i characters" field max
    let strNotEmpty field = sprintf "'%s' must not be empty" field
    let strPattern field pattern = sprintf "'%s' must match pattern %s" field pattern

    let guidEmpty field = sprintf "'%s' must be empty" field
    let guidNotEmpty field = sprintf "'%s' must not be empty" field

    let optionIsNone field = sprintf "'%s' must not have a value" field
    let optionIsSome field = sprintf "'%s' must have a value" field

    let seqBetweenLen field min max = sprintf "'%s' must be between %i and %i items" field min max
    let seqEmpty field = sprintf "'%s' must be empty" field
    let seqEqualsLen field len = sprintf "'%s' must be %i items" field len
    let seqExists field = sprintf "'%s' must contain the specified item" field
    let seqGreaterThanLen field min = sprintf "'%s' must be greater than %i items" field min
    let seqGreaterThanOrEqualToLen field min = sprintf "'%s' must be greater than or equal to %i items" field min
    let seqLessThanLen field max = sprintf "'%s' must be less than %i items" field max
    let seqLessThanOrEqualToLen field max = sprintf "'%s' must be less than or equal to %i items" field max
    let seqNotEmpty field = sprintf "'%s' must not be empty" field

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
    /// are in Ok state, otherwise return Error
    let zip
        (r1 : ValidationResult<'a>)
        (r2 : ValidationResult<'b>)
        : ValidationResult<'a * 'b> =
        match r1, r2 with
        | Ok x1res, Ok x2res -> Ok (x1res, x2res)
        | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)
        | Error e, _         -> Error e
        | _, Error e         -> Error e

    /// Apply a function to the result Error value which has been converted to
    /// a Map<string, string list>
    let mapError
        (resultFn : Map<string, string list> -> 'b)
        (result : ValidationResult<'a>)
        : Result<'a, 'b> =
        Result.mapError (ValidationErrors.toMap >> resultFn) result

    /// Apply a function to the result Error value which has been converted to
    /// a string list
    let mapErrorList
        (resultFn : string list -> 'b)
        (result : ValidationResult<'a>)
        : Result<'a, 'b> =
        Result.mapError (ValidationErrors.toList >> resultFn) result

    /// Apply validation function to each item in the list
    let traverse
        (fn : 'a -> ValidationResult<'b>)
        (lst : 'a list)
        : ValidationResult<'b list> =
        let folder item acc =
            match fn item, acc with
            | Ok i, Ok a -> Ok (i :: a)
            | Ok i, Error e -> Error e
            | Error e, Ok _ -> Error e
            | Error e, Error eAcc -> Error (ValidationErrors.merge eAcc e)

        let seed = Ok []

        List.foldBack folder lst seed

    /// Convert a ValidationResult<'a> seq to ValidationResult<'a seq>
    let sequence
        (lst : ValidationResult<'a> list)
        : ValidationResult<'a list> =
        traverse id lst


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

    let success : Validator<'a, 'a> = fun field x -> Ok x
    let fail msg : Validator<'a, 'a> = fun field x -> Error (ValidationErrors.create field [ msg field ])

type ValidatorGroup<'a>(startValidator : Validator<'a, 'a>) =
    member _.Build() = startValidator

    member _.And(andValidator : Validator<'a, 'a>)  =
        ValidatorGroup(fun f v ->
            match startValidator f v, andValidator f v with
            | Ok a, Ok _   -> Ok a
            | Error e, Ok _   -> Error e
            | Ok _, Error e   -> Error e
            | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2))

    member _.Then(nextValidator : Validator<'a, 'a>) =
        ValidatorGroup(fun f v ->
            Result.bind (nextValidator f) (startValidator f v))

/// Functionality to support the validate { } expression
module ValidationResultBuilder =

    /// Computation expression for ValidationResult<_>.
    type ValidationResultBuilder() =
        member _.Return (value) = Ok value

        member _.ReturnFrom (result) = result

        member _.Delay(fn) = fn

        member _.Run(fn) = fn ()

        member _.Bind (result, binder) = Result.bind binder result

        member x.Zero () = x.Return ()

        member x.TryWith (result, exceptionHandler) =
            try x.ReturnFrom (result)
            with ex -> exceptionHandler ex

        member x.TryFinally (result, fn) =
            try x.ReturnFrom (result)
            finally fn ()

        member x.Using (disposable : #IDisposable, fn) =
            x.TryFinally(fn disposable, fun _ ->
                match disposable with
                | null -> ()
                | disposable -> disposable.Dispose())

        member x.While (guard,  fn) =
            if not (guard())
                then x.Zero ()
            else
                do fn () |> ignore
                x.While(guard, fn)

        member x.For (items : seq<_>, fn) =
            x.Using(items.GetEnumerator(), fun enum ->
                x.While(enum.MoveNext,
                    x.Delay (fun () -> fn enum.Current)))

        member x.Combine (result, fn) =
            x.Bind(result, fun () -> fn ())

        member _.MergeSources (r1, r2) =
            ValidationResult.zip r1 r2

        member _.BindReturn (result, mapping) =
            Result.map mapping result

[<AutoOpen>]
module ValidationResultExpression =
    open ValidationResultBuilder

    /// Applicative computation expression for Validators
    let validate = ValidationResultBuilder()
