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


namespace TidepoolD
{
    public class Tidepool
    {
        public Preprocessor pp;
        public Generator gen;
        public Linker link;

        public String outname;

        /* sections */
        public List<Section> sections;
        public List<Section> priv_sections;

        public Section got;                     // got & plt handling */
        public Section plt;

        public Section dynsymtab_section;       // temporary dynamic symbol sections (for dll loading) */
        public Section dynsym;                  // exported dynamic symbol section */
        public Section symtab;                  // copy of the global symtab_section variable */


        public Tidepool()
        {
            sections = new List<Section>();
            priv_sections = new List<Section>();

            pp = new Preprocessor(this);
            gen = new Generator(this);
            link = new Linker(this);

            //wire up components
            gen.pp = pp;
            gen.link = link;

        }

        public void tpError(String fmt, params object[] args)
        {
            Console.Out.WriteLine(string.Format(fmt, args));
        }

        public void tpOpenBuf(string p, int p_2)
        {
            //throw new NotImplementedException();
        }

        public void tpClose()
        {
            //throw new NotImplementedException();
        }

        public int tpCompile()
        {
            return gen.tpgen_compile();
        }

        public int compileCode(String str)
        {
            tpOpenBuf("<string>", str.Length);
            //str.ToCharArray().CopyTo(pp.file.buffer, 0);
            int result = tpCompile();
            tpClose();
            return result;
        }

        public void outputFile(String outname)          //tcc_output_file
        {
            link.elf_output_file(outname);
        }
    }
}
