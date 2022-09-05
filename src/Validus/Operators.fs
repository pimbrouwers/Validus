namespace Validus

module Operators =
    let map (f: Validator<'a, 'b>) (g: 'b -> 'c) : Validator<'a, 'c> = fun a b -> Result.map g (f a b)

    let bindResult (f: Validator<'a, 'b>) (g: 'b -> Result<'c, _>) : Validator<'a, 'c> =
        fun a b -> Result.bind g (f a b)

    let compose (v1: Validator<'a, 'a>) (v2: Validator<'a, 'a>) : Validator<'a, 'a> =
        fun a b ->
            match v1 a b, v2 a b with
            | Ok a, Ok _ -> Ok a
            | Error e, Ok _ -> Error e
            | Ok _, Error e -> Error e
            | Error e1, Error e2 -> Error(ValidationErrors.merge e1 e2)

    let kleisli (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>) : Validator<'a, 'c> =
        fun x y -> Result.bind (v2 x) (v1 x y)

    let pickLeft (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>) : Validator<'a, 'b> =
        fun x y ->
            match v1 x y with
            | Ok v ->
                match v2 x v with
                | Ok _ -> Ok v
                | Error e -> Error e
            | Error e -> Error e

    // pickRight behaves the same as Kleisli for validators
    let pickRight (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>) : Validator<'a, 'c> =
        fun x y ->
            match v1 x y with
            | Ok v ->
                match v2 x v with
                | Ok w -> Ok w
                | Error e -> Error e
            | Error e -> Error e

    // pickRight behaves the same as Kleisli for validators
    let pickBoth (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>) : Validator<'a, 'b * 'c> =
        fun x y ->
            match v1 x y with
            | Ok v ->
                match v2 x v with
                | Ok w -> Ok(v, w)
                | Error e -> Error e
            | Error e -> Error e

    let choice (v1: Validator<'a, 'b>) (v2: Validator<'a, 'b>) : Validator<'a, 'b> =
        fun x y ->
            match v1 x y with
            | Ok v -> Ok v
            | Error e1 ->
                match v2 x y with
                | Ok v -> Ok v
                | Error e2 -> ValidationErrors.merge e1 e2 |> Error


    /// Map the Ok result of a validator, high precence, for use with choice (<|>).
    let ( *|* ) f g = map f g

    /// Set the Ok result of a validator to a fixed value, high precedence, for use with choice (<|>).
    let ( *| ) f x = map f (fun _ -> x)

    /// Map the Ok result of a validator, low precence, for use in chained validation
    let (>>|) f g = map f g

    /// Set the Ok result of a validator to a fixed value, low precedence, for use in chained validation
    let (>|) f x = map f (fun _ -> x)

    /// Bind the Ok result of a validator with a one-argument function that returns a Result
    let (>>=) f g = bindResult f g

    /// Reverse-bind the Ok result of a validator with a one-argument function that returns a Result
    let (<<=) f g = bindResult g f

    /// Set the Ok result of a validator to a fixed Result value
    let (>>%) f x = bindResult f (fun _ -> x)

    /// Compose two validators of equal types
    let (<+>) v1 v2 = compose v1 v2

    /// Introduce choice: if the rh-side validates Ok, pick that result, otherwise, continue with the next validator
    let (<|>) v1 v2 = choice v1 v2

    /// Kleisli-bind two validators. Other than Compose (<+>), this can change the result type.
    let (>=>) v1 v2 = kleisli v1 v2

    /// Reverse kleisli-bind two validators (rh-side is evaluated first). Other than Compose (<+>), this can change the result type.
    let (<=<) v1 v2 = kleisli v2 v1

    /// Compose two validators, but keep the result of the lh-side. Ignore the result of the rh-side, unless it returns an Error.
    let (.>>) v1 v2 = pickLeft v1 v2

    /// Compose two validators, but keep the result of the rh-side. Ignore the result of the lh-side, unless it returns an Error.
    let (>>.) v1 v2 = pickRight v1 v2

    /// Compose two validators, and keep the result of both sides as a tuple.
    let (.>>.) v1 v2 = pickBoth v1 v2
