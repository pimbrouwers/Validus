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

```f#
open Validus 

// Untrusted input
type PersonInput = 
      {
            FirstName : string
            LastName  : string
            Email     : string
            Age       : int
      }

// Internal domain model for name's
type Name = { First : string; Last : string }

// Internal person record, which has been validated
type VerifiedPerson = 
  {
        Name  : Name
        Email : string
        Age   : int
  }

let validatePerson input = 
    // Shared validator for first & last name
    let nameValidator = 
        Validators.String.betweenLen 3 64 None 

    // Composing multiple validators to form complex validation rules    
    let emailValidator = 
        let invalidEmailMessage = "Please provide a valid email address"
        Validators.String.betweenLen 8 512 None 
        <+> Validators.String.pattern "[^@]+@[^\.]+\..+" (Some invalidEmailMessage) // Overriding default error message

    // Construct VerifiedPerson if all validators return Success
    fun first last email age -> {
        Name  = { First = first; Last = last }
        Email = email
        Age   = age
    }   
    <!> nameValidator "First name" input.FirstName // <!> is alias for ValidationResult.map
    <*> nameValidator "Last name" input.LastName   // <*> is an alis for ValidationResult.apply
    <*> emailValidator "Email address" input.Email
    <*> Validators.Int.between 1 100 None "Age" input.Age
```

## Built-in Validators

_Coming soon_

## Custom Validators

_Coming soon_