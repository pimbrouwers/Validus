module Validus

open System

// ------------------------------------------------
// Validation Errors
// ------------------------------------------------

/// A mapping of fields and errors
type ValidationErrors = private { ValidationErrors : Map<string, string list> } with
    member internal x.Value = x.ValidationErrors

let inline private validationErrors x = { ValidationErrors = x }

/// Functions for ValidationErrors type
module ValidationErrors =
    /// Create a new ValidationErrors instance from a field  and errors list
    let create (field : string) (errors : string list) : ValidationErrors =
        [ field, errors ] |> Map.ofList |> validationErrors

    /// Combine two ValidationErrors instances
    let merge (e1 : ValidationErrors) (e2 : ValidationErrors) : ValidationErrors =
        Map.fold
            (fun acc k v ->
                match Map.tryFind k acc with
                | Some v' -> Map.add k (v' @ v) acc
                | None    -> Map.add k v acc)
            e1.Value
            e2.Value
        |> validationErrors

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

// ------------------------------------------------
// Validation Results
// ------------------------------------------------

/// Functions for ValidationResult type
module ValidationResult =
    /// Unpack ValidationResult and feed into validation function
    let apply
        (resultFn : Result<'a -> 'b, ValidationErrors>)
        (result : Result<'a, ValidationErrors>)
        : Result<'b, ValidationErrors> =
        match resultFn, result with
        | Ok fn, Ok x  -> fn x |> Ok
        | Error e, Ok _   -> Error e
        | Ok _, Error e   -> Error e
        | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)

    /// Create a tuple form ValidationResult, if two ValidationResult objects
    /// are in Ok state, otherwise return failure
    let zip
        (r1 : Result<'a, ValidationErrors>)
        (r2 : Result<'b, ValidationErrors>)
        : Result<'a * 'b, ValidationErrors> =
        match r1, r2 with
        | Ok x1res, Ok x2res -> Ok (x1res, x2res)
        | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)
        | Error e, _         -> Error e
        | _, Error e         -> Error e


// ------------------------------------------------
// Validators
// ------------------------------------------------

/// Validation rules
module ValidationRule =
    let equality<'a when 'a : equality> (equalTo : 'a) : 'a -> bool =
        fun v -> v = equalTo

    let inequality<'a when 'a : equality> (notEqualTo : 'a) : 'a -> bool=
        fun v -> not(v = notEqualTo)

    let between<'a when 'a : comparison> (min : 'a) (max : 'a) : 'a -> bool =
        fun v -> v >= min && v <= max

    let greaterThan<'a when 'a : comparison> (min : 'a) : 'a -> bool =
        fun v -> v > min

    let lessThan<'a when 'a : comparison> (max : 'a) : 'a -> bool =
        fun v -> v < max

    let inline betweenLen (min : int) (max : int) (x : ^a) : bool =
        (between min max (^a : (member Length : int) x))

    let inline equalsLen (len : int) (x : ^a) : bool =
        (equality len (^a : (member Length : int) x))

    let inline greaterThanLen (min : int) (x : ^a) : bool =
        (greaterThan min (^a : (member Length : int) x))

    let inline lessThanLen (max : int) (x : ^a) : bool =
        (lessThan max (^a : (member Length : int) x))

    let strPattern (pattern : string) : string -> bool =
        fun v -> Text.RegularExpressions.Regex.IsMatch(v, pattern)

/// Functions for Validator type
module Validator =
    /// Combine two Validators
    let compose
        (v1 : string -> 'a -> Result<'a, ValidationErrors>)
        (v2 : string -> 'a -> Result<'a, ValidationErrors>)
        : string -> 'a -> Result<'a, ValidationErrors> =
        fun (field : string) (value : 'a) ->
            match v1 field value, v2 field value with
            | Ok a, Ok _   -> Ok a
            | Error e, Ok _   -> Error e
            | Ok _, Error e   -> Error e
            | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)

    /// Create a new Validator
    let create
        (message : string -> string)
        (rule : 'a -> bool)
        : string -> 'a -> Result<'a, ValidationErrors> =
        fun (field : string) (value : 'a) ->
            let error = ValidationErrors.create field [ message field ]
            if rule value then Ok value
            else error |> Error

/// Validation functions
module Validators =
    type EqualityValidator<'a when 'a : equality>() =
        /// Value is equal to provided value
        member _.equals
            (equalTo : 'a)
            (message : string -> string)
            (field : string)
            (input : 'a)
            : Result<'a, ValidationErrors> =
            let rule = ValidationRule.equality equalTo
            Validator.create message rule field input

        /// Value is not equal to provided value
        member _.notEquals
            (notEqualTo : 'a)
            (message : string -> string)
            (field : string)
            (input : 'a)
            : Result<'a, ValidationErrors> =
            let rule = ValidationRule.inequality notEqualTo
            Validator.create message rule field input

    type ComparisonValidator<'a when 'a : comparison>() =
        inherit EqualityValidator<'a>()

        /// Value is inclusively between provided min and max
        member _.between
            (min : 'a)
            (max : 'a)
            (message : string -> string)
            (field : string)
            (input : 'a)
            : Result<'a, ValidationErrors> =
            let rule = ValidationRule.between min max
            Validator.create message rule field input

        /// Value is greater than provided min
        member _.greaterThan
            (min : 'a)
            (message : string -> string)
            (field : string)
            (input : 'a)
            : Result<'a, ValidationErrors> =
            let rule = ValidationRule.greaterThan min
            Validator.create message rule field input

        /// Value is less than provided max
        member _.lessThan
            (max : 'a)
            (message : string -> string)
            (field : string)
            (input : 'a)
            : Result<'a, ValidationErrors> =
            let rule = ValidationRule.lessThan max
            Validator.create message rule field input

    type StringValidator() =
        inherit EqualityValidator<string>()

        /// Validate string is between length (inclusive)
        member _.betweenLen
            (min : int)
            (max : int)
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            let rule = ValidationRule.betweenLen min max
            Validator.create message rule field input

        /// Validate string is null or ""
        member _.empty
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            Validator.create message String.IsNullOrWhiteSpace field input

        /// Validate string length is equal to provided value
        member _.equalsLen
            (len : int)
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            let rule = ValidationRule.equalsLen len
            Validator.create message rule field input

        /// Validate string length is greater than provided value
        member _.greaterThanLen
            (min : int)
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            let rule = ValidationRule.greaterThanLen min
            Validator.create message rule field input

        /// Validate string length is less than provided value
        member _.lessThanLen
            (max : int)
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            let rule = ValidationRule.lessThanLen max
            Validator.create message rule field input

        /// Validate string is not null or ""
        member _.notEmpty
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            Validator.create
                message
                (fun str -> not(String.IsNullOrWhiteSpace (str)))
                field
                input

        /// Validate string matches regular expression
        member _.pattern
            (pattern : string)
            (message : string -> string)
            (field : string)
            (input : string)
            : Result<string, ValidationErrors> =
            let rule = ValidationRule.strPattern pattern
            Validator.create message rule field input

    type GuidValidator() =
        inherit EqualityValidator<Guid> ()

        /// Validate string is null or ""
        member _.empty
            (message : string -> string)
            (field : string)
            (input : Guid)
            : Result<Guid, ValidationErrors> =
            Validator.create message (fun guid -> Guid.Empty = guid) field input

        /// Validate string is not null or ""
        member _.notEmpty
            (message : string -> string)
            (field : string)
            (input : Guid)
            : Result<Guid, ValidationErrors> =
            Validator.create message (fun guid -> Guid.Empty <> guid) field input

    type ListValidator<'a when 'a : equality>() =
        inherit EqualityValidator<'a list> ()

        /// Validate list is between length (inclusive)
        member _.betweenLen
            (min : int)
            (max : int)
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<'a list, ValidationErrors> =
            let rule = ValidationRule.betweenLen min max
            Validator.create message rule field input

        /// Validate list is empty
        member _.empty
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<'a list, ValidationErrors> =
            Validator.create message List.isEmpty field input

        /// Validate list length is equal to provided value
        member _.equalsLen
            (len : int)
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<'a list, ValidationErrors> =
            let rule = ValidationRule.equalsLen len
            Validator.create message rule field input

        /// Validate list contains element matching predicate
        member _.exists
            (predicate : 'a -> bool)
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<('a) list, ValidationErrors> =
            Validator.create message (List.exists predicate) field input

        /// Validate list length is greater than provided value
        member _.greaterThanLen
            (min : int)
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<'a list, ValidationErrors> =
            let rule = ValidationRule.greaterThanLen min
            Validator.create message rule field input

        /// Validate list length is less than provided value
        member _.lessThanLen
            (max : int)
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<'a list, ValidationErrors> =
            let rule = ValidationRule.lessThanLen max
            Validator.create message rule field input

        /// Validate list is not empty
        member _.notEmpty
            (message : string -> string)
            (field : string)
            (input : ('a) list)
            : Result<'a list, ValidationErrors> =
            Validator.create message (fun x -> not(List.isEmpty x)) field input

    /// Execute validator if 'a is Some, otherwise return Ok 'a
    let optional
        (validator : string -> 'a -> Result<'b, ValidationErrors>)
        (field : string) (value : 'a option)
        : Result<'b option, ValidationErrors> =
        match value with
        | Some v -> validator field v |> Result.map (fun v -> Some v)
        | None   -> Ok None

    /// Execute validator if 'a is Some, otherwise return Failure
    let required
        (validator : string -> 'a -> Result<'b, ValidationErrors>)
        (message : string -> string)
        (field : string)
        (input : 'a option)
        : Result<'b, ValidationErrors> =
        match input with
        | Some x -> validator field x
        | None   -> Error (ValidationErrors.create field [ message field ])

    /// DateTime validators
    let DateTime = ComparisonValidator<DateTime>()

    /// DateTimeOffset validators
    let DateTimeOffset = ComparisonValidator<DateTimeOffset>()

    /// decimal validators
    let Decimal = ComparisonValidator<decimal>()

    /// float validators
    let Float = ComparisonValidator<float>()

    /// System.Guid validators
    let Guid = GuidValidator()

    /// int32 validators
    let Int = ComparisonValidator<int>()

    /// int16 validators
    let Int16 = ComparisonValidator<int16>()

    /// int64 validators
    let Int64 = ComparisonValidator<int64>()

    /// string validators
    let String = StringValidator()

    /// System.TimeSpan validators
    let TimeSpan = ComparisonValidator<TimeSpan>()

    /// List validators
    let List<'a when 'a : equality> = ListValidator<'a>()

    module Default =
        type DefaultEqualityValidator<'a when 'a
            : equality>(x : EqualityValidator<'a>) =
            /// Value is equal to provided value with the default error message
            member _.equals (equalTo: 'a) (field : string) (input : 'a) =
                let msg field = sprintf "%s must be equal to %A" field equalTo
                x.equals equalTo msg field input

            /// Value is not equal to provided value with the default
            /// error message
            member _.notEquals (notEqualTo : 'a) (field : string) (input : 'a) =
                let msg field = sprintf "%s must not equal %A" field notEqualTo
                x.notEquals notEqualTo msg field input

        type DefaultComparisonValidator<'a when 'a
            : comparison>(x : ComparisonValidator<'a>) =
            inherit DefaultEqualityValidator<'a>(x)

            /// Value is inclusively between provided min and max with the
            /// default error message
            member _.between (min : 'a) (max : 'a) (field : string) (input : 'a) =
                let msg field =
                    sprintf "%s must be between %A and %A" field min max
                x.between min max msg field input

            /// Value is greater than provided min with the default error
            /// message
            member _.greaterThan (min : 'a) (field : string) (input : 'a) =
                let msg field =
                    sprintf "%s must be greater than %A" field min
                x.greaterThan min msg field input

            /// Value is less than provided max with the default error message
            member _.lessThan (max : 'a) (field : string) (input : 'a) =
                let msg field =
                    sprintf "%s must be less than %A" field max
                x.lessThan max msg field input

        type DefaultStringValidator(x : StringValidator) =
            inherit DefaultEqualityValidator<string>(x)

            /// Validate string is between length (inclusive) with the default
            /// error message
            member _.betweenLen (min : int) (max : int) (field : string) (input : string) =
                let msg field =
                    sprintf
                        "%s must be between %i and %i characters"
                        field min max
                x.betweenLen min max msg field input

            /// Validate string is null or "" with the default error message
            member _.empty (field : string) (input : string) =
                let msg field = sprintf "%s must be empty" field
                x.empty msg field input

            /// Validate string length is greater than provided value with the
            /// default error message
            member _.equalsLen (len : int) (field : string) (input : string) =
                let msg field = sprintf "%s must be %i characters" field len
                x.equalsLen len msg field input

            /// Validate string length is greater than provided value with the
            /// default error message
            member _.greaterThanLen (min : int) (field : string) (input : string) =
                let msg field =
                    sprintf "%s must not execeed %i characters" field min
                x.greaterThanLen min msg field input

            /// Validate string length is less than provided value with the
            /// default error message
            member _.lessThanLen (max : int) (field : string) (input : string) =
                let msg field =
                    sprintf "%s must be at least %i characters" field max
                x.lessThanLen max msg field input

            /// Validate string is not null or "" with the default error message
            member _.notEmpty (field : string) (input : string) =
                let msg field = sprintf "%s must not be empty" field
                x.notEmpty msg field input

            /// Validate string matches regular expression with the default
            /// error message
            member _.pattern (pattern : string) (field : string) (input : string) =
                let msg field = sprintf "%s must match pattern %s" field pattern
                x.pattern pattern msg field input

        type DefaultGuidValidator(x : GuidValidator) =
            inherit DefaultEqualityValidator<Guid>(x)

            /// Validate System.Guid is null or "" with the default error
            /// message
            member _.empty (field : string) (input : Guid) =
                let msg field = sprintf "%s must be empty" field
                x.empty msg field input

            /// Validate System.Guid is not null or "" with the default error
            /// message
            member _.notEmpty (field : string) (input : Guid) =
                let msg field = sprintf "%s must not be empty" field
                x.notEmpty msg field input

        type DefaultListValidator<'a when 'a : equality>(x : ListValidator<'a>) =
            inherit DefaultEqualityValidator<'a list>(x)

            /// Validate string is between length (inclusive) with the default
            /// error message
            member _.betweenLen (min : int) (max : int) (field : string) (input : 'a list) =
                let msg field =
                    sprintf
                        "%s must be between %i and %i items in length"
                        field min max
                x.betweenLen min max msg field input

            /// Validate string is null or "" with the default error message
            member _.empty (field : string) (input : 'a list) =
                let msg field = sprintf "%s must be empty" field
                x.empty msg field input

            /// Validate string length is greater than provided value with the
            /// default error message
            member _.equalsLen (len : int) (field : string) (input : 'a list) =
                let msg field = sprintf "%s must be %i items in length" field len
                x.equalsLen len msg field input

            /// Validate string length is greater than provided value with the
            /// default error message
            member _.exists (predicate : 'a -> bool) (field : string) (input : 'a list) =
                let msg field = sprintf "%s must contain the specified item" field
                x.exists predicate msg field input

            /// Validate string length is greater than provided value with the
            /// default error message
            member _.greaterThanLen (min : int) (field : string) (input : 'a list) =
                let msg field =
                    sprintf "%s must not execeed %i items in length" field min
                x.greaterThanLen min msg field input

            /// Validate string length is less than provided value with the
            /// default error message
            member _.lessThanLen (max : int) (field : string) (input : 'a list) =
                let msg field =
                    sprintf "%s must be at least %i items in length" field max
                x.lessThanLen max msg field input

            /// Validate string is not null or "" with the default error message
            member _.notEmpty (field : string) (input : 'a list) =
                let msg field = sprintf "%s must not be empty" field
                x.notEmpty msg field input

        /// Execute validator if 'a is Some, otherwise return Failure with the
        /// default error message
        let required
            (validator : string -> 'a -> Result<'b, ValidationErrors>)
            (field : string)
            (value : 'a option) =
            let msg field = sprintf "%s is required" field
            required validator msg field value

        /// DateTime validators with the default error messages
        let DateTime = DefaultComparisonValidator<DateTime>(DateTime)

        /// DateTimeOffset validators with the default error messages
        let DateTimeOffset = DefaultComparisonValidator<DateTimeOffset>(DateTimeOffset)

        /// decimal validators with the default error messages
        let Decimal = DefaultComparisonValidator<decimal>(Decimal)

        /// float validators with the default error messages
        let Float = DefaultComparisonValidator<float>(Float)

        /// System.Guid validators with the default error message
        let Guid = DefaultGuidValidator(Guid)

        /// int32 validators with the default error messages
        let Int = DefaultComparisonValidator<int>(Int)

        /// int16 validators with the default error messages
        let Int16 = DefaultComparisonValidator<int16>(Int16)

        /// int64 validators with the default error messages
        let Int64 = DefaultComparisonValidator<int64>(Int64)

        /// string validators with the default error messages
        let String = DefaultStringValidator(String)

        /// System.TimeSpan validators with the default error messages
        let TimeSpan = DefaultComparisonValidator<TimeSpan>(TimeSpan)

        /// List validators
        let List<'a when 'a : equality> = DefaultListValidator<'a>(List)


// ------------------------------------------------
// Operators
// ------------------------------------------------

/// Custom operators for ValidationResult
module Operators =
    /// Alias for ValidationResult.apply
    let inline (<*>) f x = ValidationResult.apply f x

    /// Alias for Result.map
    let inline (<!>) f x = Result.map f x

    /// Alias for ValidationResult.bind
    let inline (>>=) x f = Result.bind f x

    /// Alias for Validator.compose
    let inline (<+>) v1 v2 = Validator.compose v1 v2


// ------------------------------------------------
// Builder
// ------------------------------------------------

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

/// Validate computation expression
let validate = ValidationResultBuilder()
