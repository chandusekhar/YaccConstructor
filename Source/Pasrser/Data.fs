﻿// Data.fs
//
// Copyright 2009 Semen Grigorev
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation.

#light

module Data 

open IL
open Production

let (get_next_ch:int->t<string,string>),input_length =       
    let lex_list = ref Test.test3                          
    let l = List.length !lex_list 
    let get i =  List.nth (!lex_list) (l-i)        
    let input_length () = l 
    get,input_length           

let goto_set:System.Collections.Generic.IDictionary<int,Set<Grammar.Item.t<Source.t>>> = IO.readValue "goto.dta"

let items:Set<Grammar.Item.t<Source.t>> = IO.readValue "items.dta"