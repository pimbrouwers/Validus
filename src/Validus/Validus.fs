module Validus

open System

/// A mapping of fields and errors
type ValidationErrors = Map<string, string list>

/// The ValidationResult type represents a choice between success and failure
type ValidationResult<'a> = Success of 'a | Failure of ValidationErrors

/// Given a field name and value, 'a, produces a ValidationResult<'a>
type Validator<'a> = string -> 'a -> ValidationResult<'a>

/// Functions for ValidationErrors type
module ValidationErrors =
    /// Empty ValidationErrors, alias for Map.empty<string, string list>
    let empty : ValidationErrors = Map.empty<string, string list>

    /// Create a new ValidationErrors instance from a field  and errors list
    let create (field : string) (errors : string list) : ValidationErrors =   
        [ field, errors ] |> Map.ofList

    /// Combine two ValidationErrors instances
    let merge (e1 : ValidationErrors) (e2 : ValidationErrors) = 
        Map.fold 
            (fun acc k v -> 
                match Map.tryFind k acc with
                | Some v' -> Map.add k (v' @ v) acc
                | None    -> Map.add k v acc)
            e1
            e2

/// Functions for ValidationResult type
module ValidationResult = 
    /// Unpack ValidationResult and feed into validation function
    let apply (resultFn : ValidationResult<'a -> 'b>) (result : ValidationResult<'a>) : ValidationResult<'b> =
        match resultFn, result with
        | Success fn, Success x  -> fn x |> Success
        | Failure e, Success _   -> Failure e
        | Success _, Failure e   -> Failure e
        | Failure e1, Failure e2 -> Failure (ValidationErrors.merge e1 e2)  

    /// Unpack ValidationResult and apply inner value to function
    let bind (fn : 'a -> ValidationResult<'b>) (result : ValidationResult<'a>) : ValidationResult<'b> =
        match result with 
        | Success x -> fn x
        | Failure e -> Failure e

    /// Combine two ValidationResult
    let compose (a : ValidationResult<'a>) (b : ValidationResult<'a>) =
        match a, b with
        | Success a', Success _  -> Success a'
        | Failure e, Success _   -> Failure e
        | Success _, Failure e   -> Failure e
        | Failure e1, Failure e2 -> Failure (ValidationErrors.merge e1 e2)  

    /// Create a ValidationResult<'a> based on condition, yield
    /// error message if condition evaluates false
    let create condition value error : ValidationResult<'a> =
        if condition then Success value
        else error |> Failure

    /// Unpack ValidationResult, evaluate function if Success or return if Failure
    let map (fn : 'a -> 'b) (result : ValidationResult<'a>) : ValidationResult<'b> =
        match result with 
        | Success x -> fn x |> Success
        | Failure e -> Failure e

    /// Transform ValidationResult<'a> to Result<'a, ValidationErrors>
    let toResult (result : ValidationResult<'a>) : Result<'a, ValidationErrors> =
        match result with 
        | Success r -> Ok r
        | Failure e -> Error e

/// Functions for Validator type
module Validator =     
    /// Combine two Validators
    let compose (a : Validator<'a>) (b : Validator<'a>) =
        fun (field : string) (value : 'a) ->
            ValidationResult.compose
                (a field value)
                (b field value)                  

    /// Create a new Validator
    let create (predicate : 'a -> bool) (message : string) : Validator<'a> = 
        fun (field : string) (value : 'a) ->
            let error = ValidationErrors.create field [message]
            ValidationResult.create (predicate value) value error
            
/// Validation functions for primitive types
module Validators =
    let private messageOrDefault (message : string option) (defaultMessage : unit -> string) =        
        message |> Option.defaultValue (defaultMessage ())        
        
    type EqualityValidator<'a when 'a : equality>() =                 
        member _.equals (equalTo : 'a) (message : string option) : Validator<'a> =
            let defaultMessage () = sprintf "Value must be equal to %A" equalTo
            Validator.create (fun v -> v = equalTo) (messageOrDefault message defaultMessage)

        member _.notEquals (notEqualTo : 'a) (message : string option) : Validator<'a> =            
            let defaultMessage () = sprintf "Value must not equal %A"  notEqualTo
            Validator.create (fun v -> v <> notEqualTo) (messageOrDefault message defaultMessage)    

    type ComparisonValidator<'a when 'a : comparison>() = 
        inherit EqualityValidator<'a>()

        member _.between (min : 'a) (max : 'a) (message : string option) : Validator<'a> =            
            let defaultMessage () = sprintf "Value must be between %A and %A" min max
            Validator.create (fun v -> v >= min && v <= max) (messageOrDefault message defaultMessage)
                
        member _.greaterThan (min : 'a) (message : string option) : Validator<'a> =            
            let defaultMessage () = sprintf "Value must be greater than or equal to %A" min
            Validator.create (fun v -> v > min) (messageOrDefault message defaultMessage)

        member _.lessThan (max : 'a) (message : string option) : Validator<'a> =            
            let defaultMessage () = sprintf "Value must be less than or equal to %A" min
            Validator.create (fun v -> v < max) (messageOrDefault message defaultMessage)

    type StringValidator() =
        inherit EqualityValidator<string>() 

        member _.betweenLen (min : int) (max : int) (message : string option) : Validator<string> =
            let defaultMessage () = sprintf "Value must be between %i and %i characters" min max
            Validator.create (fun v -> v.Length >= min && v.Length <= max) (messageOrDefault message defaultMessage)

        member _.empty (message : string option) : Validator<string> =
            let defaultMessage () = sprintf "Value must be empty"                 
            Validator.create (fun v -> String.IsNullOrWhiteSpace(v)) (messageOrDefault message defaultMessage)

        member _.greaterThanLen (max : int) (message : string option) : Validator<string> =
            let defaultMessage () = sprintf "Value must not execeed %i characters" max
            Validator.create (fun v -> v.Length < max) (messageOrDefault message defaultMessage)

        member _.lessThanLen (min : int) (message : string option) : Validator<string> =
            let defaultMessage () = sprintf "Value must be at least %i characters" min
            Validator.create (fun v -> v.Length > min) (messageOrDefault message defaultMessage)

        member _.notEmpty (message : string option) : Validator<string> =
            let defaultMessage () = sprintf "Value must not be empty"                 
            Validator.create (fun v -> not(String.IsNullOrWhiteSpace(v))) (messageOrDefault message defaultMessage)

        member _.pattern (pattern : string) (message : string option) : Validator<string> =
            let defaultMessage () = sprintf "Value must match pattern %s" pattern
            Validator.create (fun v -> Text.RegularExpressions.Regex.IsMatch(v, pattern)) (messageOrDefault message defaultMessage)

    /// Execute validator if 'a is Some, otherwise return Success 'a
    let optional (validator : Validator<'a>) (field : string) (value : 'a option): ValidationResult<'a option> =  
        match value with
        | Some v -> validator field v |> ValidationResult.map (fun v -> Some v)
        | None   -> Success value

    /// Execute validate if 'a is Some, otherwise return Failure 
    let required (validator : Validator<'a>) (message : string option) (field : string) (value : 'a option) : ValidationResult<'a> =  
        let defaultMessage () = "Value is required" 
        match value with
        | Some v -> validator field v
        | None   -> Failure (ValidationErrors.create field [(messageOrDefault message defaultMessage)])
             
    let DateTime       = ComparisonValidator<DateTime>()
    let DateTimeOffset = ComparisonValidator<DateTimeOffset>()
    let Decimal        = ComparisonValidator<decimal>()
    let Float          = ComparisonValidator<float>()
    let Int            = ComparisonValidator<int>()    
    let Int16          = ComparisonValidator<int16>()
    let Int64          = ComparisonValidator<int64>()
    let String         = StringValidator()
    let TimeSpan       = ComparisonValidator<TimeSpan>()

/// Custom operators for ValidationResult
module Operators =
    /// Alias for ValidationResult.apply
    let (<*>) = ValidationResult.apply

    /// Alias for ValidationResult.map
    let (<!>) = ValidationResult.map

    /// Alias for Validator.compose
    let (<+>) = Validator.compose