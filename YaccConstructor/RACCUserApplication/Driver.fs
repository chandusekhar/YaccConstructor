﻿// Driver.fs
//
// Copyright 2009-2010 Semen Grigorev
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation.

module m1

open Microsoft.FSharp.Text.Lexing
open Yard.Generators._RACCGenerator
open Yard.Generators._RACCGenerator.Tables
module Lexer = UserLexer

let run_common path = 
    let content = System.IO.File.ReadAllText(path)    
    let reader = new System.IO.StringReader(content) in
    LexBuffer<_>.FromTextReader reader

let run path =
    let buf = run_common path 
    let l = UserLexer.Lexer()
    let tables =
        {
            gotoSet = gotoSet
            automataDict = autumataDict
            items = items
        }
    let driver = Yard.Generators._RACCGenerator.CoreDriver(tables)
    let forest = driver.Parse l buf
    forest
do run @"W:\Users\gsv2\Diploma\trunk\YaccConstructor\RACCUserApplication\test"
System.Console.ReadLine();

