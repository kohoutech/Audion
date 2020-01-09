/* ----------------------------------------------------------------------------
Tidepool : a C compiler
 
based on Fabrice Bellard's Tiny C compiler

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
    public class Tidepool
    {
        public Preprocessor pp;
        public Generator gen;
        public Linker link;

        String outname;

        public Section[] sections;
        public int nb_sections;

        public Tidepool()
        {
            pp = new Preprocessor(this);
            gen = new Generator(this);
            link = new Linker(this);

            //wire up components
            gen.pp = pp;
            gen.link = link;

            //debug
            nb_sections = 7;
            sections = new Section[nb_sections];

            sections[0] = null;

            sections[1] = new Section(this, "sec1", 0, 0);
            sections[1].sh_name = 1;
            sections[1].sh_num = 1;
            sections[1].sh_type = 1;
            sections[1].sh_flags = 6;
            sections[1].sh_addr = 0;
            sections[1].sh_offset = 0x34;
            sections[1].sh_size = 0x11;
            sections[1].link = null;
            sections[1].sh_info = 0;
            sections[1].sh_addralign = 4;
            sections[1].sh_entsize = 0;
            sections[1].data = new byte[] { 0x55, 0x89, 0xe5, 0x81, 0xec, 0, 0, 0, 0, 0x90, 0xb8, 0x45, 0, 0, 0, 0xc9, 0xc3 };

            sections[2] = new Section(this, "sec2", 0, 0);
            sections[2].sh_name = 7;
            sections[2].sh_num = 2;
            sections[2].sh_type = 1;
            sections[2].sh_flags = 3;
            sections[2].sh_addr = 0;
            sections[2].sh_offset = 0x48;
            sections[2].sh_size = 0;
            sections[2].link = null;
            sections[2].sh_info = 0;
            sections[2].sh_addralign = 4;
            sections[2].sh_entsize = 0;
            sections[2].data = null;

            sections[3] = new Section(this, "sec3", 0, 0);
            sections[3].sh_name = 0xd;
            sections[3].sh_num = 3;
            sections[3].sh_type = 8;
            sections[3].sh_flags = 3;
            sections[3].sh_addr = 0;
            sections[3].sh_offset = 0x48;
            sections[3].sh_size = 0;
            sections[3].link = null;
            sections[3].sh_info = 0;
            sections[3].sh_addralign = 4;
            sections[3].sh_entsize = 0;
            sections[3].data = null;

            sections[4] = new Section(this, "sec4", 0, 0);
            sections[4].sh_name = 0x12;
            sections[4].sh_num = 4;
            sections[4].sh_type = 2;
            sections[4].sh_flags = 0;
            sections[4].sh_addr = 0;
            sections[4].sh_offset = 0x48;
            sections[4].sh_size = 0x30;
            sections[4].link = null;
            sections[4].sh_info = 2;
            sections[4].sh_addralign = 4;
            sections[4].sh_entsize = 0x10;
            sections[4].data = new byte[] { 
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                1,0,0,0,0,0,0,0,0,0,0,0,4,0,0xf1,0xff,
                0xf,0,0,0,0,0,0,0,0x11,0,0,0,0x12,0,1,0};

            sections[5] = new Section(this, "sec5", 0, 0);
            sections[5].sh_name = 0x1a;
            sections[5].sh_num = 5;
            sections[5].sh_type = 3;
            sections[5].sh_flags = 0;
            sections[5].sh_addr = 0;
            sections[5].sh_offset = 0x78;
            sections[5].sh_size = 0x14;
            sections[5].link = null;
            sections[5].sh_info = 0;
            sections[5].sh_addralign = 1;
            sections[5].sh_entsize = 0;
            sections[5].data = new byte[] { 0,
                0x64,0x65,0x62,0x75,0x67,0x2f,0x74,0x65,0x73,0x74,0x32,0x2e,0x63,0,
                0x6d,0x61,0x69,0x6e,0};

            sections[4].link = sections[5];

            sections[6] = new Section(this, "sec6", 0, 0);
            sections[6].sh_name = 0x22;
            sections[6].sh_num = 6;
            sections[6].sh_type = 3;
            sections[6].sh_flags = 0;
            sections[6].sh_addr = 0;
            sections[6].sh_offset = 0x8c;
            sections[6].sh_size = 0x2c;
            sections[6].link = null;
            sections[6].sh_info = 0;
            sections[6].sh_addralign = 1;
            sections[6].sh_entsize = 0;
            sections[6].data = new byte[] { 0,
                0x2e,0x74,0x65,0x78,0x74,0,
                0x2e,0x64,0x61,0x74,0x61,0,
                0x2e,0x62,0x73,0x73,0,
                0x2e,0x73,0x79,0x6d,0x74,0x61,0x62,0,
                0x2e,0x73,0x74,0x72,0x74,0x61,0x62,0,
                0x2e,0x73,0x68,0x73,0x74,0x72,0x74,0x61,0x62,0};
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

        public void outputFile(String outname)
        {
            link.elf_output_file(outname);
        }
    }
}
