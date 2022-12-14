module ElementReaderTests

open System
open System.Text.Json
open JsonSharp
open Xunit

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

[<Fact>]
let ``Should parse json objects without throwing an exception`` () =
    let json = """
        {"hello": "world"}
"""
    let reader = ElementReader.parse json
    Assert.Equal(JsonValueKind.Object, reader.currentValueKind)
    
[<Fact>]
let ``Should parse json`` () =
    let json = """
        [true, false, "hello", "world"]
"""
    let reader = ElementReader.parse json
    Assert.Equal(JsonValueKind.Array, reader.currentValueKind)
    
[<Fact>]
let ``Should parse a value from an object`` () =
    let json = """
        {"hello": "world"}
"""
    let reader = ElementReader.parse json
    Assert.Equal("world", reader.string "hello")
    
[<Fact>]
let ``Should compare the string value of a json property`` () =
    let json = """
        {"hello": "world"}
"""
    let reader = ElementReader.parse json
    let reader = reader.get "hello"
    Assert.True(reader.valueEquals "world")
    
[<Fact>]
let ``Should compare the string value of a json property with property lookup`` () =
    let json = """
        {"hello": "world"}
"""
    let reader = ElementReader.parse json
    Assert.True(reader.valueEquals("hello", "world"))
    
[<Fact>]
let ``Should throw InvalidOperationException when parsing null values to non-option types`` () =
    let json = """
        {"hello": null}
"""
    let reader = ElementReader.parse json
    Assert.Throws<InvalidOperationException>(fun _ -> reader.int "hello" |> ignore)
    
[<Fact>]
let ``Should parse a null value to None`` () =
    let json = """
        {"hello": null}
"""
    let reader = ElementReader.parse json
    Assert.Equal(None, reader.intOrNone "hello")
    
[<Fact>]
let ``Should parse a null string value to None`` () =
    let json = """
        {"hello": null}
"""
    let reader = ElementReader.parse json
    Assert.Equal(None, reader.stringOrNone "hello")
    
[<Fact>]
let ``Should parse an empty string value to Some("")`` () =
    let json = """
        {"hello": ""}
"""
    let reader = ElementReader.parse json
    Assert.Equal(Some "", reader.stringOrNone "hello")
    
[<Fact>]
let ``Should map objects to records`` () =
    let json = """
        {"hello": "world", "foo": [1,2,3,4,5], "bar": {"thing": null}}
"""
    let reader = ElementReader.parse json
    let data =
        {| hello = reader.string "hello"
           foo = reader.array ("foo", fun read -> read.int ())
           bar = reader.object ("bar", fun read ->
                {| thing = read.stringOrNone "thing" |})
        |}
    
    Assert.Equal("world", data.hello)
    Assert.Equal(None, data.bar.thing)
    Assert.Equal([1;2;3;4;5], data.foo)
    
[<Fact>]
let ``Should parse nested objects`` () =
    let json = """
        {"foo":{"bar":{"baz":1000}}}
"""
    let g (n : string) (e : ElementReader) =
        e.get n
    let reader =
        ElementReader.parse json
        |> g "foo"
        |> g "bar"
    Assert.Equal(1000, reader.int "baz")
    
[<Fact>]
let ``Should parse a value from an array`` () =
    let json = """
        [true, false, "hello", "world"]
"""
    let reader = ElementReader.parse json
    Assert.Equal(true, reader.bool 0)
    Assert.Equal(false, reader.bool 1)
    Assert.Equal("hello", reader.string 2)
    Assert.Equal("world", reader.string 3)
    
[<Fact>]
let ``Should parse an object from an array`` () =
    let json = """
        [{"hello": "world", "foo": {"bar": null}}]
"""
    let reader = ElementReader.parse json
    let data = reader.object (0, fun read -> {| hello = read.string "hello"; bar = read.object ("foo", fun read -> read.stringOrNone "bar") |})
    Assert.Equal("world", data.hello)
    Assert.Equal(None, data.bar)

[<Fact>]
let ``Should throw an IndexOutOfRangeException for arrays`` () =
    let json = "[]"
    let reader = ElementReader.parse json
    Assert.Throws<IndexOutOfRangeException>(fun _ -> reader.bool 0 |> ignore)

[<Fact>]
let ``Should parse an array of multiple value types into a union type`` () =
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
    let expected = 
        [ Dog ({ Name = "Mars"; Loyal = true })
          Dog ({ Name = "Juno"; Loyal = true })
          Dog ({ Name = "Hog" ; Loyal = true })
          Cat ({ Name = "Puma"; Lives = 9    })]
    Assert.Equal(expected, animals)
