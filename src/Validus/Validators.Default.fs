namespace Validus.Validators.Default

open System

open Validus
open Validus.Validators

type DefaultEqualityValidator<'a when 'a
    : equality>(x : EqualityValidator<'a>) =
    /// Value is equal to provided value with the default error message
    member _.equals (equalTo: 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.equals field equalTo
        x.equals equalTo msg field input

    /// Value is not equal to provided value with the default
    /// error message
    member _.notEquals (notEqualTo : 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.notEquals field notEqualTo
        x.notEquals notEqualTo msg field input

type DefaultComparisonValidator<'a when 'a
    : comparison>(x : ComparisonValidator<'a>) =
    inherit DefaultEqualityValidator<'a>(x)

    /// Value is inclusively between provided min and max with the
    /// default error message
    member _.between (min : 'a) (max : 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.between field min max
        x.between min max msg field input

    /// Value is greater than provided min with the default error
    /// message
    member _.greaterThan (min : 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.greaterThan field min
        x.greaterThan min msg field input

    /// Value is greater than or equal to provided min with the default
    /// error message
    member _.greaterThanOrEqualTo (min : 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.greaterThanOrEqualTo field min
        x.greaterThanOrEqualTo min msg field input

    /// Value is less than provided max with the default error message
    member _.lessThan (max : 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.lessThan field max
        x.lessThan max msg field input

    /// Value is less than or equal to provided max with the default
    /// error message
    member _.lessThanOrEqualTo (max : 'a) (field : string) (input : 'a) =
        let msg field = ValidationMessages.lessThanOrEqualTo field max
        x.lessThanOrEqualTo max msg field input

type DefaultStringValidator(x : StringValidator) =
    inherit DefaultEqualityValidator<string>(x)

    /// Validate string is between length (inclusive) with the default
    /// error message
    member _.betweenLen (min : int) (max : int) (field : string) (input : string) =
        let msg field = ValidationMessages.strBetweenLen field min max
        x.betweenLen min max msg field input

    /// Validate string is null or "" with the default error message
    member _.empty (field : string) (input : string) =
        let msg field = ValidationMessages.strEmpty field
        x.empty msg field input

    /// Validate string length is equals to provided value with the
    /// default error message
    member _.equalsLen (len : int) (field : string) (input : string) =
        let msg field = ValidationMessages.strEqualsLen field len
        x.equalsLen len msg field input

    /// Validate string length is greater than provided value with the
    /// default error message
    member _.greaterThanLen (min : int) (field : string) (input : string) =
        let msg field = ValidationMessages.strGreaterThanLen field min
        x.greaterThanLen min msg field input

    /// Validate string length is greater than or equal to provided
    /// value with the default error message
    member _.greaterThanOrEqualToLen (min : int) (field : string) (input : string) =
        let msg field = ValidationMessages.strGreaterThanOrEqualToLen field min
        x.greaterThanOrEqualToLen min msg field input

    /// Validate string length is less than provided value with the
    /// default error message
    member _.lessThanLen (max : int) (field : string) (input : string) =
        let msg field = ValidationMessages.strLessThanLen field max
        x.lessThanLen max msg field input

    /// Validate string length is less than or equal to provided value
    /// with the default error message
    member _.lessThanOrEqualToLen (max : int) (field : string) (input : string) =
        let msg field = ValidationMessages.strLessThanOrEqualToLen field max
        x.lessThanOrEqualToLen max msg field input

    /// Validate string is not null or "" with the default error message
    member _.notEmpty (field : string) (input : string) =
        let msg field = ValidationMessages.strNotEmpty field
        x.notEmpty msg field input

    /// Validate string matches regular expression with the default
    /// error message
    member _.pattern (pattern : string) (field : string) (input : string) =
        let msg field = ValidationMessages.strPattern field pattern
        x.pattern pattern msg field input

type DefaultGuidValidator(x : GuidValidator) =
    inherit DefaultEqualityValidator<Guid>(x)

    /// Validate System.Guid is null or "" with the default error
    /// message
    member _.empty (field : string) (input : Guid) =
        let msg field = ValidationMessages.guidEmpty field
        x.empty msg field input

    /// Validate System.Guid is not null or "" with the default error
    /// message
    member _.notEmpty (field : string) (input : Guid) =
        let msg field = ValidationMessages.guidNotEmpty field
        x.notEmpty msg field input

type DefaultOptionValidator(x : OptionValidator) =
    /// Validate 'a option is None with default error
    /// message
    member _.isNone (field : string) (input : 'a option) =
        let msg field = ValidationMessages.optionIsNone field
        x.isNone msg field input

    /// Validate 'a option is Some with default error
    /// message
    member _.isSome (field : string) (input : 'a option) =
        let msg field = ValidationMessages.optionIsSome field
        x.isSome msg field input

type DefaultValueOptionValidator(x : ValueOptionValidator) =
    /// Validate 'a voption is None with default error
    /// message
    member _.isNone (field : string) (input : 'a voption) =
        let msg field = ValidationMessages.optionIsNone field
        x.isNone msg field input

    /// Validate 'a voption is Some with default error
    /// message
    member _.isSome (field : string) (input : 'a voption) =
        let msg field = ValidationMessages.optionIsSome field
        x.isSome msg field input

type DefaultSequenceValidator<'a, 'b when 'a : equality and 'b :> 'a seq>(x : SequenceValidator<'a, 'b>) =
    inherit DefaultEqualityValidator<'a seq>(x)

    /// Validate sequence is between length (inclusive) with the default
    /// error message
    member _.betweenLen (min : int) (max : int) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqBetweenLen field min max
        x.betweenLen min max msg field input

    /// Validate sequence is empty with the default error message
    member _.empty (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqEmpty field
        x.empty msg field input

    /// Validate sequence length is equal to than provided value with the
    /// default error message
    member _.equalsLen (len : int) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqEqualsLen field len
        x.equalsLen len msg field input

    /// Validate sequence contains the provided value with the
    /// default error message
    member _.exists (predicate : 'a -> bool) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqExists field
        x.exists predicate msg field input

    /// Validate sequence length is greater than provided value with the
    /// default error message
    member _.greaterThanLen (min : int) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqGreaterThanLen field min
        x.greaterThanLen min msg field input

    /// Validate sequence is greater than or equal to provided
    /// value with the default error message
    member _.greaterThanOrEqualToLen (min : int) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqGreaterThanOrEqualToLen field min
        x.greaterThanOrEqualToLen min msg field input

    /// Validate sequence length is less than provided value with the
    /// default error message
    member _.lessThanLen (max : int) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqLessThanLen field max
        x.lessThanLen max msg field input

    /// Validate sequence is less than or equal to provided value
    /// with the default error message
    member _.lessThanOrEqualToLen (max : int) (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqLessThanOrEqualToLen field max
        x.lessThanOrEqualToLen max msg field input

    /// Validate sequence is not null or "" with the default error message
    member _.notEmpty (field : string) (input : 'b) =
        let msg field = ValidationMessages.seqNotEmpty field
        x.notEmpty msg field input
