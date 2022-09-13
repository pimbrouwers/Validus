namespace Validus

[<RequireQualifiedAccess>]
module Check =
    open System
    open Validators
    open Validators.Default

    module WithMessage =
        /// Execute validator if 'a is Some, otherwise return Failure
        let required
            (validator : Validator<'a, 'b>)
            (message : ValidationMessage)
            (field : string)
            (input : 'a option)
            : ValidationResult<'b> =
            match input with
            | Some x -> validator field x
            | None   -> Error (ValidationErrors.create field [ message field ])

        /// Execute validator if 'a is Some, otherwise return Failure
        let vrequired
            (validator : Validator<'a, 'b>)
            (message : ValidationMessage)
            (field : string)
            (value : 'a voption)
            : ValidationResult<'b> =
            match value with
            | ValueSome v -> validator field v
            | ValueNone   -> Error (ValidationErrors.create field [ message field ])

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

        /// Sequence validators
        let Seq<'a when 'a : equality> = SequenceValidator<'a>()

    /// Execute validator if 'a is Some, otherwise return Ok 'a
    let optional
        (validator : Validator<'a, 'b>)
        (field : string)
        (value : 'a option)
        : ValidationResult<'b option> =
        match value with
        | Some v -> validator field v |> Result.map (fun v -> Some v)
        | None   -> Ok None

    /// Execute validator if 'a is ValueSome, otherwise return Ok 'a
    let voptional
        (validator : Validator<'a, 'b>)
        (field : string)
        (value : 'a voption)
        : ValidationResult<'b voption> =
        match value with
        | ValueSome v -> validator field v |> Result.map (fun v -> ValueSome v)
        | ValueNone   -> Ok ValueNone

    /// Execute validator if 'a is Some, otherwise return Failure with the
    /// default error message
    let required
        (validator : Validator<'a, 'b>)
        (field : string)
        (value : 'a option)
        : ValidationResult<'b> =
        let msg field = sprintf "'%s' is required" field
        WithMessage.required validator msg field value

    /// Execute validator if 'a is Some, otherwise return Failure with the
    /// default error message
    let vrequired
        (validator : Validator<'a, 'b>)
        (field : string)
        (value : 'a ValueOption)
        : ValidationResult<'b> =
        let msg field = sprintf "'%s' is required" field
        WithMessage.vrequired validator msg field value


    /// DateTime validators with the default error messages
    let DateTime = DefaultComparisonValidator<DateTime>(WithMessage.DateTime)

    /// DateTimeOffset validators with the default error messages
    let DateTimeOffset = DefaultComparisonValidator<DateTimeOffset>(WithMessage.DateTimeOffset)

    /// decimal validators with the default error messages
    let Decimal = DefaultComparisonValidator<decimal>(WithMessage.Decimal)

    /// float validators with the default error messages
    let Float = DefaultComparisonValidator<float>(WithMessage.Float)

    /// System.Guid validators with the default error message
    let Guid = DefaultGuidValidator(WithMessage.Guid)

    /// int32 validators with the default error messages
    let Int = DefaultComparisonValidator<int>(WithMessage.Int)

    /// int16 validators with the default error messages
    let Int16 = DefaultComparisonValidator<int16>(WithMessage.Int16)

    /// int64 validators with the default error messages
    let Int64 = DefaultComparisonValidator<int64>(WithMessage.Int64)

    /// string validators with the default error messages
    let String = DefaultStringValidator(WithMessage.String)

    /// System.TimeSpan validators with the default error messages
    let TimeSpan = DefaultComparisonValidator<TimeSpan>(WithMessage.TimeSpan)

    /// List validators
    let List<'a when 'a : equality> = Validators.Default.DefaultListValidator<'a>(WithMessage.List)

    /// Sequence validators
    let Seq<'a when 'a : equality> = Validators.Default.DefaultSequenceValidator<'a>(WithMessage.Seq)
