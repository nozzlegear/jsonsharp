namespace JsonSharp

open System
open System.Text.Json

type ElementReader (element : JsonElement) =
    let tryGetSelf (fn : JsonElement -> 't) :'t option =
        match element.ValueKind with
        | JsonValueKind.Null
        | JsonValueKind.Undefined -> None
        | _ -> Some (fn element)
        
    let tryGetItem (index : int) =
        if index >= element.GetArrayLength() then
            None
        else
            let item = element[index]
            match item.ValueKind with
            | JsonValueKind.Null
            | JsonValueKind.Undefined -> None
            | _ -> Some item
            
    let getItem (index : int) =
        if index >= element.GetArrayLength() then
            raise (IndexOutOfRangeException())
        else
            element[index]
    
    let tryGetProperty (name : string) =
        match element.TryGetProperty name with
        | false, _ -> None
        | true, p when p.ValueKind = JsonValueKind.Null || p.ValueKind = JsonValueKind.Undefined -> None
        | true, property -> Some property
        
    let getProperty (name : string) =
        match element.TryGetProperty name with
        | false, _ -> raise (JsonException $"Tried to parse JSON property {name} but it was not found.")
        | true, property -> property
        
    let enumerateTarget (target : JsonElement) =
        target.EnumerateArray()

    static member private defaultDocumentOptions =
        let mutable options = JsonDocumentOptions()
        options.AllowTrailingCommas <- true
        options.CommentHandling <- JsonCommentHandling.Skip
        options

    static member parse (json : string, ?options : JsonDocumentOptions) =
        let options = defaultArg options ElementReader.defaultDocumentOptions
        use document = JsonDocument.Parse(json, options)
        document.RootElement.Clone()
        |> ElementReader
        
    static member parseAsync (jsonStream : System.IO.Stream, ?options : JsonDocumentOptions) = task {
        let options = defaultArg options ElementReader.defaultDocumentOptions
        use! document = JsonDocument.ParseAsync(jsonStream, options)
        return document.RootElement.Clone()
               |> ElementReader
    }
    
    member _.getArrayLength () =
        element.GetArrayLength()
        
    member _.getUnderlyingElement () =
        element.Clone()
        
    member _.currentValueKind =
        element.ValueKind
        
    member _.hasProperty (name : string) : bool =
        match element.TryGetProperty name with
        | true, _ -> true
        | false, _ -> false
        
    /// <summary>
    ///   Compares <paramref name="text" /> to the string value of this element.
    /// </summary>
    /// <param name="text">The text to compare against.</param>
    /// <exception cref="InvalidOperationException">
    ///   This value's <see cref="ValueKind"/> is not <see cref="JsonValueKind.String"/>.
    /// </exception>
    /// <remarks>
    ///   This method is functionally equal to doing an ordinal comparison of <paramref name="text" /> and
    ///   the result of calling <see cref="string" />, but avoids creating the string instance.
    /// </remarks>
    member _.valueEquals (text: string) : bool =
        element.ValueEquals text
        
    /// <summary>
    ///   Compares <paramref name="text" /> to the string value of the element at <paramref name="property" />.
    /// </summary>
    /// <param name="text">The text to compare against.</param>
    /// <param name="property">The property to lookup.</param>
    /// <exception cref="InvalidOperationException">
    ///   This value's <see cref="ValueKind"/> is not <see cref="JsonValueKind.String"/>.
    /// </exception>
    /// <remarks>
    ///   This method is functionally equal to doing an ordinal comparison of <paramref name="text" /> and
    ///   the result of calling <see cref="string" />, but avoids creating the string instance.
    /// </remarks>
    member self.valueEquals (property : string, text: string) : bool =
        let el = self.get property
        el.valueEquals(text)
        
    member _.getPropertyValueKind (index : int) : JsonValueKind option =
        try 
            let item = getItem index
            Some item.ValueKind
        with
        | :? IndexOutOfRangeException -> None 
        
    member _.getPropertyValueKind (name : string) : JsonValueKind option =
        match element.TryGetProperty name with
        | false, _ -> None
        | true, p -> Some p.ValueKind
        
    member _.get (index : int) : ElementReader =
        ElementReader (getItem index)
        
    member _.get (name : string) : ElementReader =
        ElementReader (getProperty name)
        
    member _.tryGet (index : int) : ElementReader option =
        tryGetItem index
        |> Option.map ElementReader
        
    member _.tryGet (name : string) : ElementReader option =
        tryGetProperty name
        |> Option.map ElementReader
        
    member _.int () : int =
        element.GetInt32()
        
    member _.int (index : int) : int =
        let property = getItem index
        property.GetInt32()
        
    member _.int (name : string) : int =
        let property = getProperty name
        property.GetInt32()
        
    member _.intOrNone () : int option =
        tryGetSelf (fun p -> p.GetInt32())
        
    member _.intOrNone (index : int) : int option =
        tryGetItem index
        |> Option.map (fun p -> p.GetInt32())
        
    member _.intOrNone (name : string) : int option =
        tryGetProperty name
        |> Option.map (fun p -> p.GetInt32())
        
    member _.int64 () : int64 =
        element.GetInt64()
        
    member _.int64 (index : int) : int64 =
        let property = getItem index
        property.GetInt64()
        
    member _.int64 (name : string) : int64 =
        let property = getProperty name
        property.GetInt64()
        
    member _.int64OrNone () : int64 option =
        tryGetSelf (fun p -> p.GetInt64())
        
    member _.int64OrNone (index : int) : int64 option =
        tryGetItem index
        |> Option.map (fun p -> p.GetInt64())
        
    member _.int64OrNone (name : string) : int64 option =
        tryGetProperty name
        |> Option.map (fun p -> p.GetInt64())
        
    member _.bool () : bool =
        element.GetBoolean()
        
    member _.bool (name : string) : bool =
        let property = getProperty name
        property.GetBoolean()
        
    member _.bool (index : int) : bool =
        let property = getItem index
        property.GetBoolean()
        
    member _.boolOrNone () : bool option =
        tryGetSelf (fun p -> p.GetBoolean())
        
    member _.boolOrNone (index : int) : bool option =
        tryGetItem index
        |> Option.map (fun p -> p.GetBoolean())
        
    member _.boolOrNone (name : string) : bool option =
        tryGetProperty name
        |> Option.map (fun p -> p.GetBoolean())
        
    member _.string () : string =
        element.GetString()
        
    member _.string (index : int) : string =
        let property = getItem index
        property.GetString()
        
    member _.string (name : string) : string =
        let property = getProperty name
        property.GetString()
        
    member _.stringOrNone () : string option =
        tryGetSelf (fun p -> p.GetString())
        |> Option.bind (fun p -> if isNull p then None else Some p)
        
    member _.stringOrNone (index : int) : string option =
        tryGetItem index
        |> Option.map (fun p -> p.GetString())
        |> Option.bind (fun p -> if isNull p then None else Some p)
        
    member _.stringOrNone (name : string) : string option =
        tryGetProperty name
        |> Option.map (fun p -> p.GetString())
        |> Option.bind (fun p -> if isNull p then None else Some p)

    member _.array (fn : ElementReader -> 't) : 't seq = 
        enumerateTarget element
        |> Seq.map (ElementReader >> fn)

    member _.array (index : int, fn : ElementReader -> 't) : 't seq =
        getItem index
        |> enumerateTarget
        |> Seq.map (ElementReader >> fn)
        
    member _.array (name : string, fn : ElementReader -> 't) : 't seq =
        getProperty name
        |> enumerateTarget
        |> Seq.map (ElementReader >> fn)

    member _.arrayOrNone (fn : ElementReader -> 't) : 't seq option =
        tryGetSelf (enumerateTarget >> Seq.map (ElementReader >> fn))

    member _.arrayOrNone (index : int, fn : ElementReader -> 't) : 't seq option =
        tryGetItem index
        |> Option.map (enumerateTarget >> Seq.map (ElementReader >> fn))
        
    member _.arrayOrNone (name : string, fn : ElementReader -> 't) : 't seq option =
        tryGetProperty name
        |> Option.map (enumerateTarget >> Seq.map (ElementReader >> fn))

    member self.object (fn : ElementReader -> 't) : 't =
        // TODO: is this method needed? Is it not the same as the entire ElementReader itself?
        fn self

    member _.object (index : int, fn : ElementReader -> 't) : 't = 
        getItem index
        |> ElementReader
        |> fn
        
    member _.object (name : string, fn : ElementReader -> 't) : 't =
        getProperty name
        |> ElementReader
        |> fn

    member _.objectOrNone (fn : ElementReader -> 't) : 't option =
        tryGetSelf (ElementReader >> fn)

    member _.objectOrNone (index : int, fn : ElementReader -> 't) : 't option =
        tryGetItem index
        |> Option.map (ElementReader >> fn)
        
    member _.objectOrNone (name : string, fn : ElementReader -> 't) : 't option =
        tryGetProperty name
        |> Option.map (ElementReader >> fn)
