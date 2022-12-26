namespace Validus.Tests

open Validus

module Result =
    let isOkValue x =
        function
        | Ok y -> y = x
        | Error _ -> false

    let containsErrorValue x =
        function
        | Ok _ -> false
        | Error e -> e |> ValidationErrors.toList |> List.contains x

    let vError x y =
        Error <| ValidationErrors.create x [y]
