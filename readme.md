# JsonSharp

[![Build/Test Pipeline](https://github.com/nozzlegear/jsonsharp/actions/workflows/ci.yml/badge.svg)](https://github.com/nozzlegear/jsonsharp/actions/workflows/ci.yml)

JsonSharp is a small wrapper around the System.Text.Json `JsonElement` class. The goal is to provide a functional approach for parsing and reading individual JSON elements into classes, records or union types. 

```fs
let json = """
    {
        "foo": "hello",
        "bar": "world",
        "baz": true
    }
"""
let reader = JsonSharp.ElementReader.parse json

// Read just the "foo" or just the "bar" properties 
let foo = reader.string "foo"
let bar = reader.string "bar"

// Or read the json into an object
let data = reader.object (fun read ->
    {
        foo = read.string "foo"
        bar = read.string "bar"
        baz = read.bool "baz"
    })
```

With JsonSharp, it's possible to make decisions about what types you'll deserialize the JSON into while you're deserializing it. For example, imagine you have an API that returns objects that differ slightly based on a `type` property. With JsonSharp, you can read that `type` property while deserializing and decide which union type or class needs to be used: 

```fs
type Dog = 
    { 
        Name : string
        Loyal : bool
    }

type Cat = 
    {
        Name : string
        Lives : int
    }

type Animal = 
    | Dog of Dog
    | Cat of Cat

let json = """
    [
        {"type":"dog","name":"Mars","loyal":true},
        {"type":"dog","name":"Juno","loyal":true},
        {"type":"dog","name":"Hog","loyal":true},
        {"type":"cat","name":"Puma","lives":9}
    ]
"""
let reader = JsonSharp.ElementReader.parse json

// Parse the json array into an array of Animal union types, deciding which animal type based on the "type" property
let animals = reader.array (fun read ->
    if read.valueEquals("type", "dog") then
        { Name = read.string "name"; Loyal = read.bool "loyal" }
        |> Dog
    else
        { Name = read.string "name"; Lives = read.int "lives" }
        |> Cat
)
```

You can, of course, do this without JsonSharp, but it typically requires an intermediate class which holds _all_ of the available properties between both Dog and Cat, plus the `type` property. Then you'd iterate over all of those intermediate class instances, look at the `type` and pare them down from there. 

JsonSharp lets you skip that step and make the decision during the parse.
