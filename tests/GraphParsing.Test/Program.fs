﻿module GraphParsingTests

open System.IO
open QuickGraph
open NUnit.Framework
open YC.GraphParsing.Tests.RDFPerfomance
open Util
open System.Collections.Generic
open GraphParsing

let graphParsingTestPath = "..\..\..\GraphParsing.Test"

let createEmptyMatrix = ProbabilityMatrix.empty

let getInnerValue (matrix: ProbabilityMatrix.T) = matrix.InnerValue

let toArray (matrix: ProbabilityMatrix.T) (isTranspose: bool) = matrix.GetSubArray id isTranspose matrix.WholeMatrix

let innerSum f1 f2 = f1 + f2

let innerMult f1 f2 = f1 * f2

let innerZero = 0.0

let innerOne = 1.0

let graphParsingPrint (matrix: ProbabilityMatrix.T) =
    let rowLength = matrix.Nrow
    let colLength = matrix.Ncol
    for i in [ 0..rowLength - 1 ] do
        for j in [ 0..colLength - 1 ] do
            let cell = Cell.create i j
            printf "%.8f  " <| Probability.unwrap matrix.[cell]
        printfn ""
    printfn ""

[<TestFixture>]
type ``Graph parsing tests``() =  
    member this._01_SimpleRecognizerTest () =
        let graph = new AdjacencyGraph<int, TaggedEdge<int, int<AbstractAnalysis.Common.token>>>()
        graph.AddVertex(0) |> ignore
        graph.AddVertex(1) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(0, 1, 2*1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(1, 0, 2*1<AbstractAnalysis.Common.token>)) |> ignore
        let A = NonTerminal "A"
        let B = NonTerminal "B"
        let S = NonTerminal "S"
        let nonterminals = [| A; B; S |]
        let rawHeadsToProbs = List.map (fun (nt, prob) -> nt, Probability.create prob)
        let crl = new Dictionary<NonTerminal * NonTerminal, (NonTerminal * Probability.T) list>()
        [ (A, B), [ S, 1.0 ]
          (A, A), [ B, 1.0 ] ]
        |> List.map (fun (nts, heads) -> nts, rawHeadsToProbs heads)
        |> Seq.iter crl.Add
        let srl = new Dictionary< int<AbstractAnalysis.Common.token>, (NonTerminal * Probability.T) list>()
        [ 2*1<AbstractAnalysis.Common.token>, [ A, 1.0 ] ]
        |> List.map (fun (c, heads) -> c, rawHeadsToProbs heads)
        |> Seq.iter srl.Add
        let erl: NonTerminal list = []
        let rules = new RulesHolder(crl, srl, erl)
        let (recognizeMatrix, vertexToInt, multCount) =
            GraphParsing.recognizeGraph<ProbabilityMatrix.T, float> <| graph <| GraphParsing.naiveSquareMatrix<ProbabilityMatrix.T, float> <| rules <| nonterminals <| S <| createEmptyMatrix <| 
                getInnerValue <| toArray <|innerSum <| innerMult <| innerZero <| innerOne
        printfn "Multiplacation count: %d" multCount
        graphParsingPrint recognizeMatrix

    member this._02_SimpleRecognizerTest2 () =
        let graph = new AdjacencyGraph<int, TaggedEdge<int, int<AbstractAnalysis.Common.token>>>()
        graph.AddVertex(0) |> ignore
        graph.AddVertex(1) |> ignore
        graph.AddVertex(2) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(0, 1, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(1, 2, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(2, 0, 1<AbstractAnalysis.Common.token>)) |> ignore
        let grammarPath = System.IO.Path.Combine(graphParsingTestPath, "SimpleGrammar_cnf.yrd")
        let fe = new Yard.Frontends.YardFrontend.YardFrontend()
        let loadIL = fe.ParseGrammar grammarPath
        let tokenizer str =
            match str with
                | "A" -> 1<AbstractAnalysis.Common.token>
                | _ -> -1<AbstractAnalysis.Common.token>

        let (parsingMatrix, _, multCount) = graphParse<ProbabilityMatrix.T, float> <| graph <| naiveSquareMatrix<ProbabilityMatrix.T, float> <| loadIL
                                          <| tokenizer <| createEmptyMatrix <| getInnerValue <| toArray <| innerSum <| innerMult <| innerZero <| innerOne
        printfn "Multiplacation count: %d" multCount
        graphParsingPrint parsingMatrix

    member this._03_SimpleLoopTest3 () =
        let graph = new AdjacencyGraph<int, TaggedEdge<int, int<AbstractAnalysis.Common.token>>>()
        graph.AddVertex(0) |> ignore
        graph.AddVertex(1) |> ignore
        graph.AddVertex(2) |> ignore
        graph.AddVertex(3) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(0, 0, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(0, 1, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(1, 2, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(1, 3, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(2, 3, 1<AbstractAnalysis.Common.token>)) |> ignore
        graph.AddEdge(new TaggedEdge<int, int<AbstractAnalysis.Common.token>>(3, 2, 1<AbstractAnalysis.Common.token>)) |> ignore
        let grammarPath = System.IO.Path.Combine(graphParsingTestPath, "SimpleGrammar_cnf.yrd")
        let fe = new Yard.Frontends.YardFrontend.YardFrontend()
        let loadIL = fe.ParseGrammar grammarPath
        let tokenizer str =
            match str with
                | "A" -> 1<AbstractAnalysis.Common.token>
                | _ -> -1<AbstractAnalysis.Common.token>

        let (parsingMatrix, _, multCount) = graphParse<ProbabilityMatrix.T, float> <| graph <| naiveSquareMatrix<ProbabilityMatrix.T, float> <| loadIL
                                          <| tokenizer <| createEmptyMatrix <| getInnerValue <| toArray <| innerSum <| innerMult <| innerZero <| innerOne
        printfn "Multiplacation count: %d" multCount
        graphParsingPrint parsingMatrix

[<EntryPoint>]
let f x =
    System.Runtime.GCSettings.LatencyMode <- System.Runtime.GCLatencyMode.LowLatency
    let t = new ``Graph parsing tests``()
//    t._01_SimpleRecognizerTest ()
//    t._02_SimpleRecognizerTest2 ()
//    t._03_SimpleLoopTest3 ()
//    YC.GraphParsing.Tests.RDFPerfomance.performTests ()
    0
