﻿//  CodeGenerator.fs contains functions for action code generation
//
//  Copyright 2009,2010 Semen Grigorev <rsdpisuy@gmail.com>
//
//  This file is part of YaccConctructor.
//
//  YaccConstructor is free software:you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace  Yard.Generators.RACCGenerator

open Yard.Core.IL
open Yard.Core.IL.Definition
open Yard.Core.IL.Production
open Yard.Core.IL.Rule

type CodeGenerator(outPath: string) = 
    class        

        let ruleToAction = ref []

        let textWriter = TextWriter outPath                                
        let write str = textWriter.Write(str)
         
        let generatePreheader grammarName =
            write "//this file was generated by RACC"
            write ("//source grammar:" + grammarName )
            write ("//date:" + System.DateTime.Now.ToString())
            write ""
            write "module RACC.Actions"
            write ""
            write "open Yard.Generators.RACCGenerator"            

        let notMatched expectedType = 
            "| x -> \"Unexpected type of node\\nType \" + x.ToString() + \" is not expected in this position\\n" + expectedType + " was expected.\" |> failwith"

        let generateHeader header = 
            if (Option.isSome header)
            then write (Source.toString header.Value)            
                                      
        let generateFooter footer =
            if (Option.isSome footer)
            then write (Source.toString footer.Value)

        let rec generateBody indentSize body =
            let lAltFName = "yardLAltAction"
            let rAltFName = "yardRAltAction"
            let elemFName = "yardElemAction"
            let clsFName = "yardClsAction"
            let indentString l = String.replicate l "    "
                
            match body with 
            | PSeq(elems,expr) -> 
                let num = Enumerator()
                let namesPair = 
                    List.map 
                        (fun elem -> 
                            if Option.isSome elem.binding
                            then  Source.toString elem.binding.Value, "x" + num.Next().ToString()
                            else "_","_")
                        elems
                    
                let namesMap = dict namesPair
                                
                let checkersMap =
                    List.map 
                        (fun elem -> 
                            if Option.isSome elem.checker
                            then  Source.toString elem.binding.Value, "if not (" + Source.toString elem.checker.Value + ") then raise Constants.CheckerFalse\n"
                            else "_","")
                        elems
                    |> dict

                let genElem elem =
                    if Option.isSome elem.binding
                    then 
                          let eFun = generateBody (indentSize + 3) elem.rule
                          indentString (indentSize + 1) + "let (" + Source.toString elem.binding.Value + ") =\n"
                        + indentString (indentSize + 2) + "let " + elemFName + " expr = \n" + eFun + "\n"
                        + indentString (indentSize + 2) + elemFName + "(" + namesMap.[Source.toString elem.binding.Value] + ")"
                    else ""
                    + try 
                        "\n" + indentString (indentSize + 1) + checkersMap.[Source.toString elem.binding.Value]
                      with _ -> ""

                        
                                           
                indentString indentSize + "match expr with\n"
                + indentString indentSize + "| RESeq [" + (List.unzip namesPair |> snd |> String.concat "; ") + "] -> \n"
                + (List.map genElem elems |> String.concat "\n")
                + "\n"
                + indentString (indentSize + 1)
                + if expr.IsSome
                  then "(" + Source.toString expr.Value + ")\n"
                  else "()\n"
                + indentString indentSize + notMatched "RESeq" + "\n"
                 
            | PAlt(alt1,alt2)  -> 
                 let lFun = generateBody (indentSize + 2) alt1
                 let rFun = generateBody (indentSize + 2) alt2
                 indentString indentSize + "match expr with\n"
               + indentString indentSize + "| REAlt(Some(x), None) -> \n" 
               + indentString (indentSize + 1) + "let " + lAltFName + " expr = \n" + lFun + "\n"
               + indentString (indentSize + 1) + lAltFName + " x \n"
               + indentString indentSize + "| REAlt(None, Some(x)) -> \n"
               + indentString (indentSize + 1) + "let " + rAltFName + " expr = \n" + rFun + "\n"
               + indentString (indentSize + 1) + rAltFName + " x \n"
               + indentString indentSize + notMatched "REAlt" + "\n"
            
            | PRef(x,arg) ->
                 indentString indentSize + "match expr with\n" 
               + indentString indentSize + "| RELeaf " + Source.toString x + " -> (" + Source.toString x + " :?> _ )" 
               + (if arg.IsSome then Source.toString arg.Value else "") + " \n"
               + indentString indentSize + notMatched "RELeaf" + "\n"

            | PToken (x) ->
                 indentString indentSize + "match expr with\n" 
               + indentString indentSize + "| RELeaf " + "t" + Source.toString x + " -> " + "t" + Source.toString x + " :?> 'a\n"
               + indentString indentSize + notMatched "RELeaf" + "\n"
               
            | PMany(expr) ->
                 indentString indentSize + "match expr with\n"
               + indentString indentSize + "| REClosure(lst) -> \n" 
               + indentString (indentSize + 1) + "let " + clsFName + " expr = \n" + (generateBody (indentSize + 2) expr) + "\n"
               + indentString (indentSize + 1) + "List.map " + clsFName + " lst \n"
               + indentString indentSize + notMatched "REClosure" + "\n"
               
            | _ -> "NotSupported"
                    
        let enum = Enumerator()

        let generateRules rules =            
            let genRule rule =
                let actName =  rule.name + enum.Next().ToString()
                let args = 
                    rule.args
                    |> List.map Source.toString
                    |> String.concat ") ("
                    |> fun x -> if x.Trim() = "" then "" else "(" + x + ")"
                ruleToAction := (rule.name, actName) :: !ruleToAction
                "let " + actName + " expr = \n    let inner " + args + " = \n" + generateBody 2 rule.body + "    box (inner)"
            List.map genRule rules

        let genearte grammar= 
            generatePreheader grammar.info.fileName
            generateHeader grammar.head
            generateRules grammar.grammar |> String.concat "\n" |> write
            List.map 
                (fun r2a -> "(\"" + fst r2a + "\"," + snd r2a + ")")
                !ruleToAction
            |> String.concat "; "
            |> fun x -> "\nlet ruleToAction = dict [|" + x + "|]\n"
            |> write             
            generateFooter grammar.foot
            textWriter.CloseOutStream()
                
        member self.Gemerate grammar = genearte grammar
                
    end