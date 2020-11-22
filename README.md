# Validus

[![NuGet Version](https://img.shields.io/nuget/v/Validus.svg)](https://www.nuget.org/packages/Validus)
[![Build Status](https://travis-ci.org/pimbrouwers/Validus.svg?branch=master)](https://travis-ci.org/pimbrouwers/Validus)

Validus is a composable validation library for F#, with built-in validators for most primitive types and easily extended through custom validators.

## Key Features

- Composable validation
- Built-in [built-in-validators](#validators) for most primitive types
- Easily extended through [custom-validators](#custom-validators)
- Infix [Operators](#operators) to provide clean syntax

## Quick Start

A common example of receiving input from an untrusted source (ex: user form submission), applying validation and producing a result based on success/failure.

```f#
open Validus
open Validus.Operators

// Untrusted input
type PersonInput = 
      {
            FirstName : string
            LastName  : string
            Email     : string
            Age       : int
      }

// Internal domain model for names
type Name = { First : string; Last : string }

// Internal person record, which has been validated
type Person = 
    {
        Name  : Name
        Email : string
        Age   : int
    }
    static member Create first last email age =
        {
            Name  = { First = first; Last = last }
            Email = email
            Age   = age
        }   

// PersonInput -> ValidationResult<Person>
let validatePersonInput input = 
    // Shared validator for first & last name
    let nameValidator = 
        Validators.String.betweenLen 3 64 None 

    // Composing multiple validators to form complex validation rules    
    let emailValidator = 
        let invalidEmailMessage = "Please provide a valid email address"
        Validators.String.betweenLen 8 512 None 
        <+> Validators.String.pattern "[^@]+@[^\.]+\..+" (Some invalidEmailMessage) // Overriding default error message

    // Construct Person if all validators return Success
    Person.Create
    <!> nameValidator "First name" input.FirstName // <!> is alias for ValidationResult.map
    <*> nameValidator "Last name" input.LastName   // <*> is an alis for ValidationResult.apply
    <*> emailValidator "Email address" input.Email
    <*> Validators.Int.between 1 100 None "Age" input.Age
```

## Built-in Validators

## [`equals`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L99)

Applies to: `string, int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

## [`notEquals`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L103)

Applies to: `string, int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

## [`between`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L110) (inclusive)

Applies to: `int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

## [`greaterThan`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L114)

Applies to: `int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

## [`lessThan`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L118)

Applies to: `int16, int, int64, decimal, float, DateTime, DateTimeOffset, TimeSpan`

## [`betweenLen`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L126)

Applies to: `string`

## [`greaterThanLen`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L136)

Applies to: `string`

## [`lessThanLen`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L141)

Applies to: `string`

## [`empty`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L131)

Applies to: `string`

## [`notEmpty`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L146)

Applies to: `string`

## [`pattern`](https://github.com/pimbrouwers/Validus/blob/cb168960b788ea50914c661fcbba3cf096ec4f3a/src/Validus/Validus.fs#L151) (Regular Expressions)

Applies to: `string`

## Custom Validators

Custom validators can be created by combining built-in validators together using `Validator.compose`, or the `<+>` infix operator, as well as creating bespoke validator's using `Validator.create`.

```f#
// Combining built-in validators
let emailValidator = 
    Validators.String.betweenLen 8 512 None
    <+> Validators.String.pattern "[^@]+@[^\.]+\..+" None

let email = "fake@test.com"
let emailResult = emailValidator "Login email" email 

// Creating a custom validator 
let fooValidator =
    let fooRule : ValidationRule<string> = fun v -> v = "foo"
    let fooMessage = "You must provide a string that matches 'foo'"
    Validator.create fooRule fooMessage

let testString = "bar"
let fooRule = fooValidator "Test string" testString
```

## Find a bug?

There's an [issue](https://github.com/pimbrouwers/Validus/issues) for that.

## License

Built with ♥ by [Pim Brouwers](https://github.com/pimbrouwers) in Toronto, ON. Licensed under [Apache License 2.0](https://github.com/pimbrouwers/Validus/blob/master/LICENSE).