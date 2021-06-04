# Validus

[![NuGet Version](https://img.shields.io/nuget/v/Validus.svg)](https://www.nuget.org/packages/Validus)
[![Build Status](https://travis-ci.org/pimbrouwers/Validus.svg?branch=main)](https://travis-ci.org/pimbrouwers/Validus)

Validus is a composable validation library for F#, with built-in validators for most primitive types and easily extended through custom validators.

## Key Features

- Composable validation.
- [Built-in](#built-in-validators) for most primitive types.
- Easily extended through [custom-validators](#custom-validators).
- Infix [operators](#operators) to provide clean syntax.

## Quick Start

A common example of receiving input from an untrusted source `PersonInput` (ex: user form submission), applying validation and producing a result based on success/failure.

```f#
open Validus
open Validus.Operators

type PersonInput = 
    { 
        FirstName : string
        LastName  : string
        Email     : string
        Age       : int option
        StartDate : DateTime option
    }

type Name = 
    { 
        First : string
        Last : string 
    }

type Person = 
    { 
        Name      : Name
        Email     : string
        Age       : int option 
        StartDate : DateTime
    }

    static member Create first last email age startDate =
        { 
            Name      = { First = first; Last = last }
            Email     = email
            Age       = age 
            StartDate = startDate
        }   

let validatePersonInput (input : PersonInput) = 
    // Shared validator for first & last name
    let nameValidator = 
        Validators.Default.String.betweenLen 3 64

    // Composing multiple validators to form complex validation rules,
    // overriding default error message (Note: "Validators.String" as 
    // opposed to "Validators.Default.String")
    let emailValidator = 
        Validators.Default.String.betweenLen 8 512
        <+> Validators.String.pattern "[^@]+@[^\.]+\..+" (Some (sprintf "Please provide a valid %s")) 

    // Defining a validator for an value option
    let ageValidator = 
        Validators.optional (Validators.Default.Int.between 1 100)

    // Defining a validator for an required value option
    let dateValidator = 
        Validators.Default.required (Validators.Default.DateTime.greaterThan DateTime.Now)

    // Construct Person if all validators return Success
    validate {
      let! first = nameValidator "First name" input.FirstName
      and! last = nameValidator "Last name" input.LastName
      and! email = emailValidator "Email address" input.Email
      and! age = ageValidator "Age" input.Age
      and! startDate = dateValidator "Start Date" input.StartDate

      return Person.Create first last email age startDate
    }

//
// Execution
//
let input : PersonInput = 
    { 
        FirstName = "John"
        LastName  = "Doe"
        Email     = "john.doe@url.com"
        Age       = Some 63
        StartDate = Some (new DateTime(2058, 1, 1))
    }

match validatePerson input with 
| Success p -> printfn "%A" p
| Failure e -> 
    e 
    |> ValidationErrors.toList
    |> Seq.iter (printfn "%s") 
```

## Built-in Validators

All of the built-in validators reside in the `Validators` module and follow a similar definition.

```fsharp
// Produce a validation message based on a field name
type ValidationMessage = string -> string

// Produce a validation result based on a field name and result
type Validator<'a> = string -> 'a -> ValidationResult<'a>

// Given 'a value, and optional validtion message produce 
// a ready to use validator for 'a
'a -> ValidationMessage option -> Validator<'a>
```

> Note: Validators pre-populated with the default error messages reside within the `Validators.Default` module.

## [`equals`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L99)

Applies to: `string, int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

```fsharp
// Define a validator which checks if a string equals
// "foo" displaying the standard error message.
let equalsFoo = 
  Validators.String.equals "foo" None "field"

equalsFoo "bar" // ValidationResult<string>
```

## [`notEquals`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L103)

Applies to: `string, int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

```fsharp
// Define a validator which checks if a string is not 
// equal to "foo" displaying the standard error message.
let notEqualsFoo = 
  Validators.String.equals "foo" None "field"

notEqualsFoo "bar" // ValidationResult<string>
```

## [`between`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L110) (inclusive)

Applies to: `int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

```fsharp
// Define a validator which checks if an int is between
// 1 and 100 (inclusive) displaying the standard error message.
let between1and100 = 
  Validators.Int.between 1 100 None "field"

between1and100 12 // ValidationResult<int>
```

## [`greaterThan`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L114)

Applies to: `int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

```fsharp
// Define a validator which checks if an int is greater than
// 100 displaying the standard error message.
let greaterThan100 = 
  Validators.Int.greaterThan 100 None "field"

greaterThan100 12 // ValidationResult<int>
```

## [`lessThan`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L118)

Applies to: `int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

```fsharp
// Define a validator which checks if an int is less than
// 100 displaying the standard error message.
let lessThan100 = 
  Validators.Int.lessThan 100 None "field"

lessThan100 12 // ValidationResult<int>
```

### String specific validators

## [`betweenLen`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L126)

Applies to: `string`

```fsharp
// Define a validator which checks if a string is between
// 1 and 100 chars displaying the standard error message.
let between1and100Chars = 
  Validators.String.betweenLen 1 100 None "field"

between1and100Chars "validus" // ValidationResult<string>
```

## [`greaterThanLen`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L136)

Applies to: `string`

```fsharp
// Define a validator which checks if a string is greater tha
// 100 chars displaying the standard error message.
let greaterThan100Chars = 
  Validators.String.greaterThanLen 100 None "field"

greaterThan100Chars "validus" // ValidationResult<string>
```

## [`lessThanLen`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L141)

Applies to: `string`

```fsharp
// Define a validator which checks if a string is less tha
// 100 chars displaying the standard error message.
let lessThan100Chars = 
  Validators.String.lessThanLen 100 None "field"

lessThan100Chars "validus" // ValidationResult<string>
```

## [`empty`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L131)

Applies to: `string`

```fsharp
// Define a validator which checks if a string is empty
// displaying the standard error message.
let stringIsEmpty = 
  Validators.String.empty None "field"

stringIsEmpty "validus" // ValidationResult<string>
```

## [`notEmpty`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L146)

Applies to: `string`

```fsharp
// Define a validator which checks if a string is not empty
// displaying the standard error message.
let stringIsNotEmpty = 
  Validators.String.notEmpty None "field"

stringIsNotEmpty "validus" // ValidationResult<string>
```

## [`pattern`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L151) (Regular Expressions)

Applies to: `string`

```fsharp
// Define a validator which checks if a string matches the 
// provided regex displaying the standard error message.
let stringIsChars = 
  Validators.String.pattern "[a-z]" None "field"

stringIsChars "validus" // ValidationResult<string>
```

## Custom Validators

Custom validators can be created by combining built-in validators together using `Validator.compose`, or the `<+>` infix operator, as well as creating bespoke validator's using `Validator.create`.

```f#
// Combining built-in validators
let emailValidator = 
    Validators.Default.String.betweenLen 8 512
    <+> Validators.Default.String.pattern "[^@]+@[^\.]+\..+"

let email = "fake@test.com"
let emailResult = emailValidator "Login email" email 

// Creating a custom validator 
let fooValidator =
    let fooRule : ValidationRule<string> = fun v -> v = "foo"
    let fooMessage = sprintf "%s must be a string that matches 'foo'"
    Validator.create fooMessage fooRule

"bar"
|> fooValidator "Test string" 
// Outputs: [ "Test string", [ "Test string must be a string that matches 'foo'" ] ]
```

## Find a bug?

There's an [issue](https://github.com/pimbrouwers/Validus/issues) for that.

## License

Built with ♥ by [Pim Brouwers](https://github.com/pimbrouwers) in Toronto, ON. Licensed under [Apache License 2.0](https://github.com/pimbrouwers/Validus/blob/master/LICENSE).
