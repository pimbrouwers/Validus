module Validus.Validators

open System

open Validus

type EqualityValidator<'a when 'a : equality>() =
    /// Value is equal to provided value
    member _.equals
        (equalTo : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.equality equalTo
        Validator.create message rule field input

    /// Value is not equal to provided value
    member _.notEquals
        (notEqualTo : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.inequality notEqualTo
        Validator.create message rule field input

type ComparisonValidator<'a when 'a : comparison>() =
    inherit EqualityValidator<'a>()

    /// Value is inclusively between provided min and max
    member _.between
        (min : 'a)
        (max : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.between min max
        Validator.create message rule field input

    /// Value is greater than provided min
    member _.greaterThan
        (min : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.greaterThan min
        Validator.create message rule field input

    /// Value is greater than or equal to provided min
    member _.greaterThanOrEqualTo
        (min : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.greaterThanOrEqualTo min
        Validator.create message rule field input

    /// Value is less than provided max
    member _.lessThan
        (max : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.lessThan max
        Validator.create message rule field input

    /// Value is less than or equal to provided max
    member _.lessThanOrEqualTo
        (max : 'a)
        (message : ValidationMessage)
        (field : string)
        (input : 'a)
        : ValidationResult<'a> =
        let rule = ValidationRule.lessThanOrEqualTo max
        Validator.create message rule field input

type StringValidator() =
    inherit EqualityValidator<string>()

    /// Validate string is between length (inclusive)
    member _.betweenLen
        (min : int)
        (max : int)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.betweenLen min max
        Validator.create message rule field input

    /// Validate string is null or ""
    member _.empty
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        Validator.create message String.IsNullOrWhiteSpace field input

    /// Validate string length is equal to provided value
    member _.equalsLen
        (len : int)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.equalsLen len
        Validator.create message rule field input

    /// Validate string length is greater than provided value
    member _.greaterThanLen
        (min : int)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.greaterThanLen min
        Validator.create message rule field input

    /// Validate string length is greater than o requal to provided value
    member _.greaterThanOrEqualToLen
        (min : int)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.greaterThanOrEqualToLen min
        Validator.create message rule field input

    /// Validate string length is less than provided value
    member _.lessThanLen
        (max : int)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.lessThanLen max
        Validator.create message rule field input

    /// Validate string length is less than or equal to provided value
    member _.lessThanOrEqualToLen
        (max : int)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.lessThanOrEqualToLen max
        Validator.create message rule field input

    /// Validate string is not null or ""
    member _.notEmpty
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        Validator.create
            message
            (fun str -> not(String.IsNullOrWhiteSpace (str)))
            field
            input

    /// Validate string matches regular expression
    member _.pattern
        (pattern : string)
        (message : ValidationMessage)
        (field : string)
        (input : string)
        : ValidationResult<string> =
        let rule = ValidationRule.strPattern pattern
        Validator.create message rule field input

type GuidValidator() =
    inherit EqualityValidator<Guid> ()

    /// Validate string is null or ""
    member _.empty
        (message : ValidationMessage)
        (field : string)
        (input : Guid)
        : ValidationResult<Guid> =
        Validator.create message (fun guid -> Guid.Empty = guid) field input

    /// Validate string is not null or ""
    member _.notEmpty
        (message : ValidationMessage)
        (field : string)
        (input : Guid)
        : ValidationResult<Guid> =
        Validator.create message (fun guid -> Guid.Empty <> guid) field input

type SequenceValidator<'a, 'b when 'a : equality and 'b :> 'a seq>() =
    inherit EqualityValidator<'a seq> ()

    /// Validate sequence is between length (inclusive)
    member _.betweenLen
        (min : int)
        (max : int)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        let rule input =
            let length = Seq.length input
            length >= min && length <= max

        Validator.create message rule field input

    /// Validate sequence is empty
    member _.empty
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        Validator.create message Seq.isEmpty field input

    /// Validate sequence length is equal to provided value
    member _.equalsLen
        (len : int)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        let rule input =
            let length = Seq.length input
            length = len

        Validator.create message rule field input

    /// Validate sequence contains element matching predicate
    member _.exists
        (predicate : 'a -> bool)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        Validator.create message (Seq.exists predicate) field input

    /// Validate sequence length is greater than provided value
    member _.greaterThanLen
        (min : int)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        let rule input =
            let length = Seq.length input
            length > min

        Validator.create message rule field input

    /// Validate sequence length is greater than or equal to provided value
    member _.greaterThanOrEqualToLen
        (min : int)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        let rule input =
            let length = Seq.length input
            length >= min

        Validator.create message rule field input

    /// Validate sequence length is less than provided value
    member _.lessThanLen
        (max : int)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        let rule input =
            let length = Seq.length input
            length < max

        Validator.create message rule field input

    /// Validate sequence length is less than or equal to provided value
    member _.lessThanOrEqualToLen
        (max : int)
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        let rule input =
            let length = Seq.length input
            length <= max

        Validator.create message rule field input

    /// Validate sequence is not empty
    member _.notEmpty
        (message : ValidationMessage)
        (field : string)
        (input : 'b)
        : ValidationResult<'b> =
        Validator.create message (fun x -> not(Seq.isEmpty x)) field input

module Default =
    type DefaultEqualityValidator<'a when 'a
        : equality>(x : EqualityValidator<'a>) =
        /// Value is equal to provided value with the default error message
        member _.equals (equalTo: 'a) (field : string) (input : 'a) =
            let msg field = sprintf "'%s' must be equal to %A" field equalTo
            x.equals equalTo msg field input

        /// Value is not equal to provided value with the default
        /// error message
        member _.notEquals (notEqualTo : 'a) (field : string) (input : 'a) =
            let msg field = sprintf "'%s' must not equal %A" field notEqualTo
            x.notEquals notEqualTo msg field input

    type DefaultComparisonValidator<'a when 'a
        : comparison>(x : ComparisonValidator<'a>) =
        inherit DefaultEqualityValidator<'a>(x)

        /// Value is inclusively between provided min and max with the
        /// default error message
        member _.between (min : 'a) (max : 'a) (field : string) (input : 'a) =
            let msg field =
                sprintf "'%s' must be between %A and %A" field min max
            x.between min max msg field input

        /// Value is greater than provided min with the default error
        /// message
        member _.greaterThan (min : 'a) (field : string) (input : 'a) =
            let msg field =
                sprintf "'%s' must be greater than %A" field min
            x.greaterThan min msg field input

        /// Value is greater than or equal to provided min with the default
        /// error message
        member _.greaterThanOrEqualTo (min : 'a) (field : string) (input : 'a) =
            let msg field =
                sprintf "'%s' must be greater than or equal to %A" field min
            x.greaterThan min msg field input

        /// Value is less than provided max with the default error message
        member _.lessThan (max : 'a) (field : string) (input : 'a) =
            let msg field =
                sprintf "'%s' must be less than %A" field max
            x.lessThan max msg field input

        /// Value is less than or equal to provided max with the default
        /// error message
        member _.lessThanOrEqualTo (max : 'a) (field : string) (input : 'a) =
            let msg field =
                sprintf "'%s' must be less than or equal to %A" field max
            x.lessThan max msg field input

    type DefaultStringValidator(x : StringValidator) =
        inherit DefaultEqualityValidator<string>(x)

        /// Validate string is between length (inclusive) with the default
        /// error message
        member _.betweenLen (min : int) (max : int) (field : string) (input : string) =
            let msg field =
                sprintf
                    "'%s' must be between %i and %i characters"
                    field min max
            x.betweenLen min max msg field input

        /// Validate string is null or "" with the default error message
        member _.empty (field : string) (input : string) =
            let msg field = sprintf "'%s' must be empty" field
            x.empty msg field input

        /// Validate string length is equals to provided value with the
        /// default error message
        member _.equalsLen (len : int) (field : string) (input : string) =
            let msg field = sprintf "'%s' must be %i characters" field len
            x.equalsLen len msg field input

        /// Validate string length is greater than provided value with the
        /// default error message
        member _.greaterThanLen (min : int) (field : string) (input : string) =
            let msg field =
                sprintf "'%s' must be greater than %i characters" field min
            x.greaterThanLen min msg field input

        /// Validate string length is greater than or equal to provided
        /// value with the default error message
        member _.greaterThanOrEqualToLen (min : int) (field : string) (input : string) =
            let msg field =
                sprintf "'%s' must be greater than or equal to %i characters" field min
            x.greaterThanLen min msg field input

        /// Validate string length is less than provided value with the
        /// default error message
        member _.lessThanLen (max : int) (field : string) (input : string) =
            let msg field =
                sprintf "'%s' must be less than %i characters" field max
            x.lessThanLen max msg field input

        /// Validate string length is less than or equal to provided value
        /// with the default error message
        member _.lessThanOrEqualToLen (max : int) (field : string) (input : string) =
            let msg field =
                sprintf "'%s' must be less than or equal to %i characters" field max
            x.lessThanLen max msg field input

        /// Validate string is not null or "" with the default error message
        member _.notEmpty (field : string) (input : string) =
            let msg field = sprintf "'%s' must not be empty" field
            x.notEmpty msg field input

        /// Validate string matches regular expression with the default
        /// error message
        member _.pattern (pattern : string) (field : string) (input : string) =
            let msg field = sprintf "'%s' must match pattern %s" field pattern
            x.pattern pattern msg field input

    type DefaultGuidValidator(x : GuidValidator) =
        inherit DefaultEqualityValidator<Guid>(x)

        /// Validate System.Guid is null or "" with the default error
        /// message
        member _.empty (field : string) (input : Guid) =
            let msg field = sprintf "'%s' must be empty" field
            x.empty msg field input

        /// Validate System.Guid is not null or "" with the default error
        /// message
        member _.notEmpty (field : string) (input : Guid) =
            let msg field = sprintf "'%s' must not be empty" field
            x.notEmpty msg field input

    type DefaultSequenceValidator<'a, 'b when 'a : equality and 'b :> 'a seq>(x : SequenceValidator<'a, 'b>) =
        inherit DefaultEqualityValidator<'a seq>(x)

        /// Validate sequence is between length (inclusive) with the default
        /// error message
        member _.betweenLen (min : int) (max : int) (field : string) (input : 'b) =
            let msg field =
                sprintf
                    "'%s' must be between %i and %i items"
                    field min max
            x.betweenLen min max msg field input

        /// Validate sequence is empty with the default error message
        member _.empty (field : string) (input : 'b) =
            let msg field = sprintf "'%s' must be empty" field
            x.empty msg field input

        /// Validate sequence length is equal to than provided value with the
        /// default error message
        member _.equalsLen (len : int) (field : string) (input : 'b) =
            let msg field = sprintf "'%s' must be %i items" field len
            x.equalsLen len msg field input

        /// Validate sequence contains the provided value with the
        /// default error message
        member _.exists (predicate : 'a -> bool) (field : string) (input : 'b) =
            let msg field = sprintf "'%s' must contain the specified item" field
            x.exists predicate msg field input

        /// Validate sequence length is greater than provided value with the
        /// default error message
        member _.greaterThanLen (min : int) (field : string) (input : 'b) =
            let msg field =
                sprintf "'%s' must be greater than %i items" field min
            x.greaterThanLen min msg field input

        /// Validate sequence is greater than or equal to provided
        /// value with the default error message
        member _.greaterThanOrEqualToLen (min : int) (field : string) (input : 'b) =
            let msg field =
                sprintf "'%s' must be greater than or equal to %i items" field min
            x.greaterThanLen min msg field input

        /// Validate sequence length is less than provided value with the
        /// default error message
        member _.lessThanLen (max : int) (field : string) (input : 'b) =
            let msg field =
                sprintf "'%s' must be less than %i items" field max
            x.lessThanLen max msg field input

        /// Validate sequence is less than or equal to provided value
        /// with the default error message
        member _.lessThanOrEqualToLen (max : int) (field : string) (input : 'b) =
            let msg field =
                sprintf "'%s' must be less than or equal to %i items" field max
            x.lessThanLen max msg field input

        /// Validate sequence is not null or "" with the default error message
        member _.notEmpty (field : string) (input : 'b) =
            let msg field = sprintf "'%s' must not be empty" field
            x.notEmpty msg field input
