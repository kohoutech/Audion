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
    public class Linker
    {
        public Tidepool tp;
        public Section text_section;
        public Section data_section;
        public Section bss_section;     // predefined sections */
        public Section common_section;
        public Section cur_text_section;    // current section where function code is generated */

        public Linker(Tidepool _tp)     //tccelf_new
        {
            tp = _tp;

            text_section = new Section(tp, ".text", ElfObject.SHT_PROGBITS, ElfObject.SHF_ALLOC | ElfObject.SHF_EXECINSTR);
            data_section = new Section(tp, ".data", ElfObject.SHT_PROGBITS, ElfObject.SHF_ALLOC | ElfObject.SHF_WRITE);
            bss_section = new Section(tp, ".bss", ElfObject.SHT_NOBITS, ElfObject.SHF_ALLOC | ElfObject.SHF_WRITE);

        }

        public void elf_output_file(string outname)
        {
        }

        public void section_realloc(Section sec, long new_size)
        {
        }
    }

    public class Section
    {
        public byte[] data;
        public long data_allocated;
        public String name;
        private Tidepool tp;
        private string p;
        private int p_2;
        private int p_3;

        public Section(Tidepool tp, string p, int p_2, int p_3)
        {
            // TODO: Complete member initialization
            this.tp = tp;
            this.p = p;
            this.p_2 = p_2;
            this.p_3 = p_3;
        }
    }
}
