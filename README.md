# nyasharp~

Nyasharp is an esoteric programming language consisting mainly of emoticons similar to uwuspeak for tokens and keywords.

# Installation
### Pre-built binaries
You can install the latest nyasharp cli binary from github actions. Pick the binary for your system (builds for Linux & Windows). Then place that in your path.

Then you can run the shell interface with `nyasharp` or run a file with `nyasharp file.nya`.

### Building
You can build the CLI app with a few simple commands. Make sure you have `git` and `dotnet` version 7.0 installed.
```
git clone https://github.com/AI-nsley69/nyasharp
cd nyasharp/src/nyasharp.cli
dotnet publish -C Release -p:PublishSingleFile=true --self-contained true
```
This will produce a single file that is self contained.

# Coding in Nyasharp
### Introduction
Nyasharp has a syntax very similar to most languages you're already familiar with. However, the majority of the characters have been replaced. If you wish to try nyasharp without installing it, you can visit the [online interpreter](https://nyasharp.trainsley69.me).

## Hello World
```
// This is a comment!
pwint "Hello World!";
pwint uwuify("Hello World!");
```
Pwint is a built-in function for logging stuff to console, hence why we do not use parentheses for it.
In the example we see two different pwint statements, one with the `uwuify()` function on the string and one without it. Nyasharp actually provides a native function to uwuify your strings and doing so before printing is highly recommend to keep the spirit of the language

## Explanations
Before we get started with writing a fibonacci program, we'll have to go over certain aspects. Like defining a variable, function, accepting input etc.

### Variables
Declaring a new variable is done with `>.<`, it expects an identifier (name) along with an initializer (assignment of value). Example of how to assign a boolean, number and string below:
```
// create a new variable called a with the value of the number 3
>.< a o/ 3;
// create a new variable called with the value of a boolean, twue = true, fawse = false
>.< b o/ twue;
// create a new variable called c with the value of the string "Hello World!"
>.< c o/ "Hello World!"
```
You can then reference them in the same way you would with another language.

### Arithmetic
In nyasharp, arithmetic is done as the same in other languages but with different tokens for the operator.
```
>.< a o/ 8;
>.< b o/ 4;
// Print a + b
pwint a +.+ b; // Prints 12
// Print a - b
pwint a -.- b; // Prints 4
// Print a * b
pwint a +.* b; // Prints 32
// Print a / b
pwint a -.* b; // Prints 2
```

### Comparison
Comparison is very similar to different languages but like with arithmetic we do it with different tokens.
```
>.< a o/ 8;
>.< b o/ 4;
// Equal to
pwint a \o/ b; // Prints false
// Not equal to
pwint a _o_ b; // Prints true
// Less than
pwint a /o/ b; // Prints false
// Less than or equal to
pwint a _o/ b; // Prints false
// Greater than
pwint a \o\ b; // Prints true
// Greater than or equal to
pwint a \o_ b; // Prints true
```

### Code blocks
Code blocks require an opening (`:>`) and a closing (`:>`).

### Functions
Functions are declared with `:D` and expect an identifier (name) along with arguments. You can leave arguments empty, but you cannot as of right now have optional arguments. If we wanted a function that returned an uwufied version of a string we can do the following:
```
:D transform(str) :>
  c: uwuify(str);
<:
```
`c:` is used to return the value from a function. We can then call this function like we would in other languages

### If, While and For statements
They're similar to most other languages, except we here have also replaced the keywords:
```
>.< a o/ 10;
>.< b o/ 0;
// if statement with else statement
^u^ (a \o\ b) pwint uwuify("10 is bigger than 0"); ^e^ pwint uwuify("Or not..");
// For loop
^o^ (>.< i o/ 0; i /o/ 10; i o/ i +.+ 1) :>
  pwint i;
<:

// While loop
^w^ (b /o/ a) :>
  b o/ b +.+ 1;
  pwint b;
<:
```