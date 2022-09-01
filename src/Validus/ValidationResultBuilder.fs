namespace Validus

open System

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
