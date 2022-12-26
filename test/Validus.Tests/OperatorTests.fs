module Validus.Operators.Tests

open System
open System.Globalization

open Xunit

open Validus
open Validus.Operators
open Validus.Tests

type AgeGroup =
    | Adult of int
    | Child
    | Senior

module Int =
    /// Minimalistic TryParse function for testing with bind
    /// that allows decimal point in integers, but truncates the result to an int.
    let tryParseFromDecimal lbl (x: string) =
        let x, y = Decimal.TryParse(x, NumberStyles.Integer ||| NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
        if x then Ok(int y)
        else Error <| ValidationErrors.create lbl ["Not a number"]

[<Fact>]
let ``Test choice and reverse kleisli operator`` () =
    let ageValidator =
        Check.Int.between 0 17 *| Child
        <|> Check.Int.greaterThan 65 *| Senior
        <=< Check.Int.between 0 120
        <=< Check.String.pattern @"\d+" *|* Int32.Parse

    ageValidator "Age" "12"  |> Result.isOkValue Child |> ignore
    ageValidator "Age" "18"  |> Result.isOkValue (Adult 18) |> ignore
    ageValidator "Age" "66"  |> Result.isOkValue Senior |> ignore
    ageValidator "Age" "-1"  |> Result.containsErrorValue "'Age' must be between 0 and 120" |> ignore
    ageValidator "Age" "foo" |> Result.containsErrorValue "'Age' must match pattern \d+" |> ignore
    ageValidator "Age" "200" |> Result.containsErrorValue "'Age' must be between 0 and 120" |> ignore

[<Fact>]
let ``Test choice and forward kleisli operator`` () =
    let ageValidator =
        Check.String.pattern @"\d+" *|* Int32.Parse
        >=> Check.Int.between 0 120
        >=> (Check.Int.between 0 17  *| Child
        <|> Check.Int.greaterThan 65 *| Senior
        <|> Check.Int.between 18 65  *|* Adult)


    ageValidator "Age" "12"  |> Result.isOkValue Child |> ignore
    ageValidator "Age" "18"  |> Result.isOkValue (Adult 18) |> ignore
    ageValidator "Age" "66"  |> Result.isOkValue Senior |> ignore
    ageValidator "Age" "-1"  |> Result.containsErrorValue "'Age' must be between 0 and 120" |> ignore
    ageValidator "Age" "foo" |> Result.containsErrorValue "'Age' must match pattern \d+" |> ignore
    ageValidator "Age" "200" |> Result.containsErrorValue "'Age' must be between 0 and 120" |> ignore

[<Fact>]
let ``Test bind operator`` () =
    let decimalByteValidator =
        Check.String.pattern @"[\d\.]+"
        >>= Int.tryParseFromDecimal "Number"
        >=> Check.Int.between 0 255

    decimalByteValidator "Byte" "33" |> Result.isOkValue 33 |> ignore
    decimalByteValidator "Byte" "33.99" |> Result.isOkValue 33 |> ignore
    decimalByteValidator "Byte" "33.99." |> Result.containsErrorValue "Not a number" |> ignore

let tryRange255 lbl = function x when x < 255 -> Ok x     | _ -> Result.vError lbl "Wrong range"
let tryFizz lbl     = function x when x % 3 = 0 -> Ok x   | _ -> Result.vError lbl "Not a Fizz"
let tryBuzz lbl     = function x when x % 5 = 0 -> Ok x   | _ -> Result.vError lbl "Not a Buzz"
let tryFizzBuzz lbl = function x when x % 15 = 0 -> Ok x  | _ -> Result.vError lbl "Not a FizzBuzz"

[<Fact>]
let ``Test multiple bind operators fizzbuzz`` () =
    let fizzBuzzValidator =
        Check.String.pattern @"[\d\.]+"
        >>= Int.tryParseFromDecimal "Number"
        >>= tryRange255 "Byte"
        >>= tryFizz "Fizz"
        >>= tryBuzz "Buzz"

    fizzBuzzValidator "FizzBuzz" "0"     |> Result.isOkValue 0 |> ignore
    fizzBuzzValidator "FizzBuzz" "165"   |> Result.isOkValue 165 |> ignore
    fizzBuzzValidator "FizzBuzz" "30.99" |> Result.isOkValue 30 |> ignore
    fizzBuzzValidator "FizzBuzz" "256"   |> Result.containsErrorValue "Wrong range" |> ignore
    fizzBuzzValidator "FizzBuzz" "9"     |> Result.containsErrorValue "Not a Buzz" |> ignore
    fizzBuzzValidator "FizzBuzz" "20"    |> Result.containsErrorValue "Not a Fizz" |> ignore

type FizzBuzz = Fizz of int | Buzz of int | FizzBuzz of int

[<Fact>]
let ``Test multiple bind and pickLeft & Right operators fizzbuzz`` () =
    // only ever returns Buzz or error.
    let fizzBuzzValidator =
        Check.String.pattern @"[\d\.]+"
        >>= Int.tryParseFromDecimal "Number"
        >>= tryRange255 "Byte"
        .>> tryFizz *|* Fizz  // a log statement or other side effect makes more sense here, as we ignore the resutl
        >>. tryBuzz *|* Buzz

    fizzBuzzValidator "Byte" "0" |> Result.isOkValue (Buzz 0) |> ignore
    fizzBuzzValidator "Byte" "3" |> Result.containsErrorValue "Not a Buzz" |> ignore
    fizzBuzzValidator "Byte" "15" |> Result.isOkValue (Buzz 15) |> ignore

[<Fact>]
let ``Test combination of bind, kleisli and choice operators fizzbuzz`` () =
    let fizzBuzzValidator =
        Check.String.pattern @"[\d\.]+"
        >>= Int.tryParseFromDecimal "Number"
        >>= tryRange255 "Byte"
        >=> (tryFizzBuzz *|* FizzBuzz
        <|> tryFizz      *|* Fizz  // a log statement or other side effect makes more sense here, as we ignore the resutl
        <|> tryBuzz      *|* Buzz)

    fizzBuzzValidator "FizzBuzz" "0"   |> Result.isOkValue (FizzBuzz 0) |> ignore
    fizzBuzzValidator "FizzBuzz" "150" |> Result.isOkValue (FizzBuzz 150) |> ignore
    fizzBuzzValidator "FizzBuzz" "21"  |> Result.isOkValue (Fizz 21) |> ignore
    fizzBuzzValidator "FizzBuzz" "50"  |> Result.isOkValue (Buzz 50) |> ignore
    fizzBuzzValidator "FizzBuzz" "254" |> Result.containsErrorValue "Not a Buzz" |> ignore
    fizzBuzzValidator "FizzBuzz" "254" |> Result.containsErrorValue "Not a Fizz" |> ignore
    fizzBuzzValidator "FizzBuzz" "254" |> Result.containsErrorValue "Not a FizzBuzz" |> ignore
    fizzBuzzValidator "FizzBuzz" "500" |> Result.containsErrorValue "Wrong range" |> ignore
    fizzBuzzValidator "FizzBuzz" "Foo" |> Result.containsErrorValue "'FizzBuzz' must match pattern [\d\.]+" |> ignore

[<Fact>]
let ``Use pickleft to parse as decimal, do some logging, but keep the input string`` () =
    let mutable x = 0
    let log _ _ = x <- x + 1; Ok()   // fake logger for testing
    let percentageValidator =
        Check.String.pattern @"[\d\.]+"
        .>> log
        .>> (Int.tryParseFromDecimal >=> Check.Int.between 0 100)
        .>> log
        >>= (sprintf "%s%%" >> Ok)

    percentageValidator "Percentage" "10" |> Result.isOkValue "10%" |> ignore
    ignore <| percentageValidator "Percentage" "100"; x = 2  |> ignore  // 'test' creates new closure
    percentageValidator "Percentage" "10.10.10" |> Result.containsErrorValue "Not a number" |> ignore

[<Fact>]
let ``Use pickBoth and pickLeft in combination, sad path`` () =
    let mutable x = 0
    let log _ _ = Ok() // fake logger
    let storeDbFail _ _ = Result.vError "Db" "Connection lost"   // fake side effect with result
    let parseAndStore =
        Check.String.pattern @"[\d\.]+"
        .>> Int.tryParseFromDecimal
        .>>. storeDbFail
        >| 42 // we'll never get here
        .>> log

    parseAndStore "Data" "10" |> Result.containsErrorValue "Connection lost" |> ignore
    parseAndStore "Data" "3.3.3" |> Result.containsErrorValue "Not a number" |> ignore

[<Fact>]
let ``Use pickBoth and pickLeft in combination, happy path`` () =
    let mutable x = 0
    let log _ _ = Ok() // fake logger
    let callService _ _ = Ok "HTTP: 200 Ok"   // fake side effect with result
    let parseAndStore =
        Check.String.pattern @"[\d\.]+"
        .>> Int.tryParseFromDecimal
        .>>. callService
        .>> log
        >>| fun (x, dbresult) -> $"Result: {dbresult}, data: {x}"

    parseAndStore "Data" "10" |> Result.isOkValue "Result: HTTP: 200 Ok, data: 10" |> ignore
    parseAndStore "Data" "3.3.3" |> Result.containsErrorValue "Not a number" |> ignore

[<Fact>]
let ``Show composability of all low precedence operators`` () =
    let mutable x = 0
    let log _ _ = x <- x + 1; Ok() // fake logger
    let callService _ _ = Ok "HTTP: 200 Ok"   // fake side effect with result

    // The following doesn't make sense, but is here to ensure composibility with operators
    // and to prevent that, in the event of updates, those operators stop working together
    let someSillyCombination =
        fun y -> if true then Ok y else Result.vError "Oops" "Oops"
        <<= Check.Int.lessThan 100
        >>| string
        >=> Check.String.pattern @"[\d\.]+"
        .>> Int.tryParseFromDecimal
        .>>. callService
        >>| fun (a, b) -> a, Adult 99
        >>| (snd >> string)
        >=> Check.String.betweenLen 0 10
        >| 42
        >>. log
        >| Guid.Empty
        >>= fun y -> if y = Guid.Empty then Ok 42 else Result.vError "Oops" "Very oops"
        >=> ((fun y -> Ok (Random().Next(0, 10))) <<= Check.Int.lessThan 100)
        >=> Check.Int.lessThan 11

    someSillyCombination "Data" 200 |> Result.containsErrorValue "'Data' must be less than 100" |> ignore
    let _ = someSillyCombination "Data" 90 |> function Ok x when x < 11 -> true | _ -> false
    ignore <| someSillyCombination "Data" 10; x = 1 |> ignore
