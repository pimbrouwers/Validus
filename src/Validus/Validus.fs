module Validus

open System

/// A mapping of fields and errors
type ValidationErrors = private ValidationErrors of Map<string, string list>

/// The ValidationResult type represents a choice between success and failure
type ValidationResult<'a> = Success of 'a | Failure of ValidationErrors

/// A validation message for a field
type ValidationMessage = string -> string

/// Given a value, return true/false to indicate validity
type ValidationRule<'a> = 'a -> bool

/// Given a field name and value, 'a, produces a ValidationResult<'a>
type Validator<'a> = string -> 'a -> ValidationResult<'a>

/// Functions for ValidationErrors type
module ValidationErrors =
    /// Empty ValidationErrors, alias for Map.empty<string, string list>
    let empty : ValidationErrors = Map.empty<string, string list> |> ValidationErrors

    /// Create a new ValidationErrors instance from a field  and errors list
    let create (field : string) (errors : string list) : ValidationErrors =   
        [ field, errors ] |> Map.ofList |> ValidationErrors

    /// Combine two ValidationErrors instances
    let merge (e1 : ValidationErrors) (e2 : ValidationErrors) : ValidationErrors = 
        let (ValidationErrors e1') = e1
        let (ValidationErrors e2') = e2
        Map.fold 
            (fun acc k v -> 
                match Map.tryFind k acc with
                | Some v' -> Map.add k (v' @ v) acc
                | None    -> Map.add k v acc)
            e1'
            e2'
        |> ValidationErrors

    /// Unwrap ValidationErrors into a standard Map<string, string list>
    let toMap (e : ValidationErrors) : Map<string, string list> =
        let (ValidationErrors e') = e
        e'

    /// Unwrap ValidationErrors and collection individual errors into
    /// string list, excluding keys
    let toList (e : ValidationErrors) : string list =
        e 
        |> toMap
        |> Seq.collect (fun kvp -> kvp.Value)
        |> List.ofSeq

/// Functions for ValidationResult type
module ValidationResult = 
    /// Convert regular value 'a into ValidationResult<'a>
    let retn (v : 'a) = Success v

    /// Unpack ValidationResult and feed into validation function
    let apply (resultFn : ValidationResult<'a -> 'b>) (result : ValidationResult<'a>) : ValidationResult<'b> =
        match resultFn, result with
        | Success fn, Success x  -> fn x |> Success
        | Failure e, Success _   -> Failure e
        | Success _, Failure e   -> Failure e
        | Failure e1, Failure e2 -> Failure (ValidationErrors.merge e1 e2)  

    /// Create a ValidationResult<'a> based on condition, yield
    /// error message if condition evaluates false
    let create (condition : bool) (value : 'a) (error : ValidationErrors) : ValidationResult<'a> =
        if condition then Success value
        else error |> Failure

    /// Unpack ValidationResult, evaluate function if Success or return if Failure
    let map (fn : 'a -> 'b) (result : ValidationResult<'a>) : ValidationResult<'b> =
        apply (retn fn) result

    /// Transform ValidationResult<'a> to Result<'a, ValidationErrors>
    let toResult (result : ValidationResult<'a>) : Result<'a, ValidationErrors> =
        match result with 
        | Success r -> Ok r
        | Failure e -> Error e

/// Functions for Validator type
module Validator =     
    /// Combine two Validators
    let compose (a : Validator<'a>) (b : Validator<'a>) : Validator<'a> =
        fun (field : string) (value : 'a) ->            
            match a field value, b field value with
            | Success a', Success _  -> Success a'
            | Failure e, Success _   -> Failure e
            | Success _, Failure e   -> Failure e
            | Failure e1, Failure e2 -> Failure (ValidationErrors.merge e1 e2)                           

    /// Create a new Validator
    let create (message : ValidationMessage) (rule : ValidationRule<'a>) : Validator<'a> = 
        fun (field : string) (value : 'a) ->
            let error = ValidationErrors.create field [ message field ]
            ValidationResult.create (rule value) value error
        
module ValidationRule =
    let equality<'a when 'a : equality> (equalTo : 'a) : ValidationRule<'a> = 
        fun v -> v = equalTo
    
    let inequality<'a when 'a : equality> (notEqualTo : 'a) : ValidationRule<'a>= 
        fun v -> not(v = notEqualTo)

    let between<'a when 'a : comparison> (min : 'a) (max : 'a) : ValidationRule<'a> = 
        fun v -> v >= min && v <= max            

    let greaterThan<'a when 'a : comparison> (min : 'a) : ValidationRule<'a> = 
        fun v -> v > min

    let lessThan<'a when 'a : comparison> (max : 'a) : ValidationRule<'a> = 
        fun v -> v < max

    let betweenLen (min : int) (max : int) : ValidationRule<string> =
        fun str -> str.Length |> between min max

    let greaterThanLen (min : int) : ValidationRule<string> =
        fun str -> str.Length |> greaterThan min

    let lessThanLen (max : int) : ValidationRule<string> =
        fun str -> str.Length |> lessThan max

    let empty : ValidationRule<string> =
        fun str -> String.IsNullOrWhiteSpace(str)

    let notEmpty : ValidationRule<string> =
        fun str -> not(empty str)

    let pattern (pattern : string) : ValidationRule<string> =
        fun v -> Text.RegularExpressions.Regex.IsMatch(v, pattern)

/// Validation functions for primitive types
module Validators = 

    type EqualityValidator<'a when 'a : equality>() =                                 
        member _.equals (equalTo : 'a) (message : ValidationMessage option): Validator<'a> =            
            let defaultMessage = fun field -> sprintf "%s must be equal to %A" field equalTo
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.equality equalTo
            Validator.create msg rule

        member _.notEquals (notEqualTo : 'a) (message : ValidationMessage option): Validator<'a> =            
            let defaultMessage = fun field -> sprintf "%s must not equal %A" field notEqualTo
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.inequality notEqualTo
            Validator.create msg rule    

    type ComparisonValidator<'a when 'a : comparison>() = 
        inherit EqualityValidator<'a>()

        member _.between (min : 'a) (max : 'a) (message : ValidationMessage option): Validator<'a> =            
            let defaultMessage = fun field -> sprintf "%s must be between %A and %A" field min max
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.between min max
            Validator.create msg rule
                
        /// Detemine if a value is greater than the provided minimum        
        member _.greaterThan (min : 'a) (message : ValidationMessage option): Validator<'a> =            
            let defaultMessage = fun field -> sprintf "%s must be greater than or equal to %A" field min
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.greaterThan min
            Validator.create msg rule

        member _.lessThan (max : 'a) (message : ValidationMessage option): Validator<'a> =            
            let defaultMessage = fun field -> sprintf "%s must be less than or equal to %A" field min
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.lessThan max
            Validator.create msg rule

    type StringValidator() =
        inherit EqualityValidator<string>() 

        /// Validate string is between length (inclusive)
        member _.betweenLen (min : int) (max : int) (message : ValidationMessage option): Validator<string> =
            let defaultMessage = fun field -> sprintf "%s must be between %i and %i characters" field min max            
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.betweenLen min max
            Validator.create msg rule

        /// Validate string is null or ""
        member _.empty (message : ValidationMessage option): Validator<string> =
            let defaultMessage = fun field -> sprintf "%s must be empty" field
            let msg = message |> Option.defaultValue defaultMessage
            Validator.create msg ValidationRule.empty

        /// Validate string length is greater than provided value
        member _.greaterThanLen (min : int) (message : ValidationMessage option): Validator<string> =
            let defaultMessage = fun field -> sprintf "%s must not execeed %i characters" field min
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.greaterThanLen min
            Validator.create msg rule
        
        /// Validate string length is less than provided value
        member _.lessThanLen (max : int) (message : ValidationMessage option): Validator<string> =
            let defaultMessage = fun field -> sprintf "%s must be at least %i characters" field max
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.lessThanLen max
            Validator.create msg rule

        /// Validate string is not null or ""
        member _.notEmpty (message : ValidationMessage option): Validator<string> =
            let defaultMessage = fun field -> sprintf "%s must not be empty" field
            let msg = message |> Option.defaultValue defaultMessage
            Validator.create msg ValidationRule.notEmpty

        /// Validate string matches regular expression
        member _.pattern (pattern : string) (message : ValidationMessage option): Validator<string> =
            let defaultMessage = fun field -> sprintf "%s must match pattern %s" field pattern
            let msg = message |> Option.defaultValue defaultMessage
            let rule = ValidationRule.pattern pattern
            Validator.create msg rule

    /// Execute validator if 'a is Some, otherwise return Success 'a
    let optional (validator : Validator<'a>) (field : string) (value : 'a option): ValidationResult<'a option> =  
        match value with
        | Some v -> validator field v |> ValidationResult.map (fun v -> Some v)
        | None   -> Success value

    /// Execute validate if 'a is Some, otherwise return Failure 
    let required (validator : Validator<'a>) (message : ValidationMessage option) (field : string) (value : 'a option) : ValidationResult<'a> =  
        let defaultMessage = fun field -> sprintf "%s is required" field
        match value with
        | Some v -> validator field v
        | None   -> Failure (ValidationErrors.create field [ (message |> Option.defaultValue defaultMessage) field ])           
             
    /// System.DateTime validators
    let DateTime = ComparisonValidator<DateTime>()
    
    /// System.DateTimeOffset validators
    let DateTimeOffset = ComparisonValidator<DateTimeOffset>()

    /// Microsoft.FSharp.Core.decimal validators
    let Decimal = ComparisonValidator<decimal>()

    /// Microsoft.FSharp.Core.float  validators
    let Float = ComparisonValidator<float>()

    /// Microsoft.FSharp.Core.int32 validators
    let Int = ComparisonValidator<int>()    

    /// Microsoft.FSharp.Core.16 validators
    let Int16 = ComparisonValidator<int16>()

    /// Microsoft.FSharp.Core.int64 validators
    let Int64 = ComparisonValidator<int64>()

    /// Microsoft.FSharp.Core.string validators
    let String = StringValidator()

    /// System.TimeSpan validators
    let TimeSpan = ComparisonValidator<TimeSpan>()

/// Custom operators for ValidationResult
module Operators =
    /// Alias for ValidationResult.apply
    let (<*>) = ValidationResult.apply

    /// Alias for ValidationResult.map
    let (<!>) = ValidationResult.map

    /// Alias for Validator.compose
    let (<+>) = Validator.compose
