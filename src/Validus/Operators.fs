namespace Validus

module Operators =
    // TODO touch up by Abel
    let map (f: Validator<'a, 'b>) (g: 'b -> 'c): Validator<'a, 'c> =
        fun a b -> Result.map g (f a b)

    let bind (f: Validator<'a, 'b>) g: Validator<'a, 'b> =
        fun a b -> Result.bind g (f a b)

    let compose (v1: Validator<'a, 'a>) (v2: Validator<'a, 'a>): Validator<'a, 'a> =
        fun a b ->
            match v1 a b, v2 a b with
            | Ok a, Ok _   -> Ok a
            | Error e, Ok _   -> Error e
            | Ok _, Error e   -> Error e
            | Error e1, Error e2 -> Error (ValidationErrors.merge e1 e2)

    let kleisli (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>): Validator<'a, 'c> =
        fun x y -> Result.bind (v2 x) (v1 x y)

    let pickLeft (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>): Validator<'a, 'b> =
        fun x y ->
            match v1 x y with
            | Ok v ->
                match v2 x v with
                | Ok _ -> Ok v
                | Error e -> Error e
            | Error e -> Error e

    // pickRight behaves the same as Kleisli for validators
    let pickRight (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>): Validator<'a, 'c> =
        fun x y ->
            match v1 x y with
            | Ok v ->
                match v2 x v with
                | Ok w -> Ok w
                | Error e -> Error e
            | Error e -> Error e

    // pickRight behaves the same as Kleisli for validators
    let pickBoth (v1: Validator<'a, 'b>) (v2: Validator<'b, 'c>): Validator<'a, 'b * 'c> =
        fun x y ->
            match v1 x y with
            | Ok v ->
                match v2 x v with
                | Ok w -> Ok(v, w)
                | Error e -> Error e
            | Error e -> Error e

    let (<!>) f g = map f g
    let (>>=) f g = bind f g
    let (<>=>) v1 v2 = compose v1 v2
    let (>=>) v1 v2 = kleisli v1 v2
    let (.>>) v1 v2 = pickLeft v1 v2
    let (>>.) v1 v2 = pickLeft v1 v2
    let (.>>.) v1 v2 = pickBoth v1 v2