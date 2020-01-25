/* ----------------------------------------------------------------------------
Tidepool(Model E) : a C compiler
 
 based on the 9cc C compiler
 
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
using System.IO;

namespace TidepoolE
{
    class TidePool
    {
        public Scanner scan;
        public Parser parser;
        public Analyzer ana;
        public Generator gen;

        static void Main(string[] args)
        {
            string filename = "test.c";
            TidePool tp = new TidePool();
            tp.compile(filename);
        }

        public TidePool()
        {
            scan = new Scanner(this);
            parser = new Parser(this);
            ana = new Analyzer(this);
            gen = new Generator(this);
        }

        public string read_file(string filename)
        {
            string buf = "";
            try
            {
                buf = File.ReadAllText(filename);
            } catch(Exception e)
            {
                scan.error("cannot open {0}: {1}", filename, e.Message);
            }
            if (buf.Length == 0 || buf[buf.Length - 1] != '\n')
                buf = buf + '\n';
            buf = buf + '\0';
            return buf;
        }

        public void compile(string filename)
        {
            scan.user_input = read_file(filename);
            scan.token = scan.tokenize();
            Program prog = parser.program();
            
            gen.generate(prog);
        }
    }
}
