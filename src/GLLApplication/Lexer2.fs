﻿module Lexer2

open Microsoft.FSharp.Text
open Yard.Generators.GLL.Parser
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Text
open Microsoft.FSharp.Reflection
    


//let tokens(str : string) = 
//    let toLexerTag (name:string) =
//            match name with
//            | "A" -> GLL.SimpleAmb.A (2)
//            | "C" -> GLL.SimpleAmb.C (2)
//            | "B" -> GLL.SimpleAmb.B (2)
//            | x -> failwithf "Unexpected token %s" x
//
//    str.Split([|' '|])
//    |> Array.map toLexerTag

    

let tokens<'lexType>(path) = 
    let toLexerTag = 
        let targetUCIs = 
            FSharpType.GetUnionCases(typeof<'lexType>) 
            |> Array.map (fun uci -> (uci.Name,  FSharpValue.PreComputeUnionConstructor(uci)) ) 
            |> dict

//        printfn "%A" targetUCIs
        let curI = ref 0
        fun (name:string) ->
//            printf "%s " name
            let caseCtor = targetUCIs.[name]
            incr curI
            (caseCtor [|!curI|]) :?> 'lexType

    System.IO.File.ReadAllText(path)
        .Split([|' '|])
    |> Array.filter ((<>) "")
    |> Array.map toLexerTag
    |> (fun x -> printf "\n"; x)
