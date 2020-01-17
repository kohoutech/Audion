﻿/* ----------------------------------------------------------------------------
Tidepool : a C compiler
 
Copyright (C) 2020  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolD
{
    class Parser
    {
        List<string> asmcode;

        public void ParseCode(String sourcecode) {

            int pos = 0;
            asmcode = new List<string>();
            asmcode.Add(".intel_syntax noprefix\n");
            asmcode.Add(".global main\n");
            asmcode.Add("main:\n");
            asmcode.Add("  mov rax, %ld\n", (sourcecode[pos] - '0'));

            while (*p)
            {
                if (sourcecode[pos] == '+')
                {
                    pos++;
                    asmcode.Add("  add rax, {0}\n", (sourcecode[pos] - '0'));
                    continue;
                }

                if (sourcecode[pos] == '-')
                {
                    pos++;
                    asmcode.Add("  sub rax, {0}\n", (sourcecode[pos] - '0'));
                    continue;
                }

                Console.Out.WriteLine("unexpected character: {0}\n", sourcecode[pos]);
            }

            asmcode.Add("  ret\n");            
        }
    }
}