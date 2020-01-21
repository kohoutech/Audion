/* ----------------------------------------------------------------------------
Tidepool : a C compiler
 
based on Fabrice Bellard's Tiny C compiler

Copyright (C) 2018-2020  George E Greaney

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
using System.IO;

namespace TidepoolD
{
    class Program
    {
        const string sourceName = "test.c";

        //test driver
        static void Main(string[] args)
        {
            Tidepool tp = new Tidepool();
            String sourcecode = File.ReadAllText(sourceName);
            tp.compileCode(sourcecode);
            tp.link.outputFile("test.o");
            Console.Out.WriteLine("done.");

            //Parser parser = new Parser();
            //parser.ParseCode("5+6-3+7");
        }
    }
}
