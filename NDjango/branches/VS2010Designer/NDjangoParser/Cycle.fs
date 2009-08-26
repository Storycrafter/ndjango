﻿(****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Parser.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************)


namespace NDjango.Tags

open System

open NDjango.OutputHandling
open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Variables
open NDjango.Expressions

module internal Cycle =
    /// Cycles among the given strings each time this tag is encountered.
    /// 
    /// Within a loop, cycles among the given strings each time through
    /// the loop::
    /// 
    ///     {% for o in some_list %}
    ///         <tr class="{% cycle 'row1' 'row2' %}">
    ///             ...
    ///         </tr>
    ///     {% endfor %}
    /// 
    /// Outside of a loop, give the values a unique name the first time you call
    /// it, then use that name each sucessive time through::
    /// 
    ///     <tr class="{% cycle 'row1' 'row2' 'row3' as rowcolors %}">...</tr>
    ///     <tr class="{% cycle rowcolors %}">...</tr>
    ///     <tr class="{% cycle rowcolors %}">...</tr>
    /// 
    /// You can use any number of values, separated by spaces. Commas can also
    /// be used to separate values; if a comma is used, the cycle values are
    /// interpreted as literal strings.
        
    type CycleController(values : Variable list, origValues : Variable list) =

        member this.Values = values
        member this.OrigValues = origValues
        member this.Value = List.hd values
        
    /// Cycles among the given strings each time this tag is encountered.
    type TagNode(provider, token, name: string, values: Variable list) =
        inherit NDjango.ParserNodes.TagNode(provider, token)
        
        let createController (controller: CycleController option) =
            match controller with
                | None -> new CycleController(values, values)
                | Some c -> 
                    match List.tl c.Values with
                        | head::tail -> new CycleController(List.tl c.Values, c.OrigValues)
                        | [] -> new CycleController (c.OrigValues, c.OrigValues)

        override this.walk manager walker = 
            let oldc = 
                match walker.context.tryfind ("$cycle" + name) with 
                | Some v -> Some (v :?> CycleController)
                | None -> 
                    match values with
                    | [] -> raise (RenderingError(sprintf "Named cycle '%s' does not exist" name))
                    | _ -> None
            let newc =
                match oldc with
                    | None -> new CycleController(values, values)
                    | Some c -> 
                        match List.tl c.Values with
                            | head::tail -> new CycleController(List.tl c.Values, c.OrigValues)
                            | [] -> new CycleController (c.OrigValues, c.OrigValues)

            let buffer = newc.Value.Resolve(walker.context) |> fst |> string
            {walker with 
                buffer = buffer;
                context = (walker.context.add ("$cycle" + name, (newc :> obj))).add (name, (buffer :> obj)) 
                }

    /// Note that the original django implementation returned the same instance of the 
    /// CycleNode for each instance of a given named cycle tag. This implementation
    /// Relies on the CycleNode instances to communicate with each other through 
    /// the context object available at render time to synchronize their activities
    type Tag() = 

        let checkForOldSyntax (value:TextToken) = 
            if (String.IsNullOrEmpty value.RawText) then false
            else match value.RawText.[0] with
                    | '"' -> false
                    | '\'' -> false
                    | _ when value.RawText.Contains(",") -> true
                    | _ -> false
                            
                
        interface NDjango.Interfaces.ITag with
            member this.Perform token context tokens =
            
                let oldstyle_re 
                    = new System.Text.RegularExpressions
                        .Regex("[^,]")

                let normalize (values: TextToken list) =
                    if List.exists checkForOldSyntax values then
                        let start = values.Head.Location.Offset
                        let end_location = values.[values.Length-1].Location
                        let t1 = token.CreateToken(start - token.Location.Offset, end_location.Offset + end_location.Length - start)
                        tokenize_for_token t1.Location oldstyle_re t1.Value |>
                        List.map (fun t -> t.WithValue("'" + t.RawText + "'"))
                    else
                        values

                let name, values =
                    match List.rev token.Args with
                    | name::MatchToken("as")::values1 ->
                        name.RawText, values1 |> List.rev |> normalize
                    | _ ->
                        match token.Args |> normalize with
                        | [] -> raise (SyntaxError ("'cycle' tag requires at least one argument"))
                        | name::[] -> name.RawText, []
                        | _ as values -> "$Anonymous$Cycle", values
//                        let values1 = token.Args |> normalize
//                        if values1.Length = 1 then (values1.[0].RawText, [])
//                        else "$Anonymous$Cycle", values1
                        
                let values = List.map (fun v -> new Variable(context, v)) values
                ((new TagNode(context, token, name, values) :> NDjango.Interfaces.INodeImpl), tokens)

