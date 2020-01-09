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
using System.IO;

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

        public void section_realloc(Section sec, long new_size)
        {
            int size = sec.data_allocated;
            if (size == 0)
                size = 1;
            while (size < new_size)
                size = size * 2;

            byte[] data = new byte[size];
            if (sec.data != null)
            {
                Array.Copy(sec.data, data, sec.data_allocated);
            }
            sec.data = data;
            sec.data_allocated = size;
        }

        public void tp_output_elf(FileStream f, int phnum, ElfPhdr phdr, int file_offset, int[] sec_order)
        {
            int size;
            int file_type;

            int shnum = tp.nb_sections;

            file_offset = (file_offset + 3) & -4;       /* align to 4 */

            /* fill header */
            ElfEhdr ehdr = new ElfEhdr();
            ehdr.e_ident[0] = ElfEhdr.ELFMAG0;
            ehdr.e_ident[1] = ElfEhdr.ELFMAG1;
            ehdr.e_ident[2] = ElfEhdr.ELFMAG2;
            ehdr.e_ident[3] = ElfEhdr.ELFMAG3;
            ehdr.e_ident[4] = ElfEhdr.ELFCLASS32;
            ehdr.e_ident[5] = ElfEhdr.ELFDATA2LSB;
            ehdr.e_ident[6] = ElfEhdr.EV_CURRENT;

            ehdr.e_type = ElfEhdr.ET_REL;
            ehdr.e_machine = ElfEhdr.EM_386;
            ehdr.e_version = ElfEhdr.EV_CURRENT;
            ehdr.e_shoff = file_offset;
            ehdr.e_ehsize = ElfEhdr.ElfEhdrSize;
            ehdr.e_shentsize = ElfShdr.ElfShdrSize;
            ehdr.e_shnum = shnum;
            ehdr.e_shstrndx = shnum - 1;

            ehdr.writeOut(f);
            int offset = ElfEhdr.ElfEhdrSize + phnum * ElfPhdr.ElfPhdrSize;

            for (int i = 1; i < tp.nb_sections; i++)
            {
                Section s = tp.sections[sec_order[i]];
                if (s.sh_type != ElfObject.SHT_NOBITS)
                {
                    while (offset < s.sh_offset)
                    {
                        f.WriteByte(0);
                        offset++;
                    }
                    size = s.sh_size;
                    if (size > 0)
                        f.Write(s.data, 0, size);
                    offset += size;
                }
            }

            while (offset < ehdr.e_shoff)
            {
                f.WriteByte(0);
                offset++;
            }

            for (int i = 0; i < tp.nb_sections; i++)
            {
                ElfShdr sh = new ElfShdr();

                Section s = tp.sections[i];
                if (s != null)
                {
                    sh.sh_name = s.sh_name;
                    sh.sh_type = s.sh_type;
                    sh.sh_flags = s.sh_flags;
                    sh.sh_entsize = s.sh_entsize;
                    sh.sh_info = s.sh_info;
                    if (s.link != null)
                        sh.sh_link = s.link.sh_num;
                    sh.sh_addralign = s.sh_addralign;
                    sh.sh_addr = s.sh_addr;
                    sh.sh_offset = s.sh_offset;
                    sh.sh_size = s.sh_size;
                }
                sh.writeOut(f);

            }
        }


        public int tp_write_elf_file(String filename, int phnum, ElfPhdr phdr, int file_offset, int[] sec_order)
        {
            FileStream f = File.OpenWrite(filename);
            tp_output_elf(f, phnum, phdr, file_offset, sec_order);
            f.Close();
            return 0;
        }

        public int elf_output_file(string outname)
        {
            int i;
            int ret;
            int phnum = 0;
            int shnum = 0;
            int file_type;
            int file_offset = 0;
            int[] sec_order = { 0, 1, 2, 3, 4, 5, 6 };
            ElfPhdr phdr = null;

            file_offset = 0xb8;

            // Create the ELF file with name 'outname' */
            ret = tp_write_elf_file(outname, phnum, phdr, file_offset, sec_order);
            return ret;
        }
    }

    //-------------------------------------------------------------------------

    public class Section
    {
        public int data_offset;         // current data offset */
        public byte[] data;             // section data */
        public int data_allocated;      // used for realloc() handling */

        public int sh_name;             // elf section name (only used during output) */
        public int sh_num;              // elf section number */
        public int sh_type;             // elf section type */
        public int sh_flags;            // elf section flags */
        public int sh_info;             // elf section info */
        public int sh_addralign;        // elf section alignment */
        public int sh_entsize;          // elf entry size */

        public int sh_size;             // section size (only used during output) */
        public int sh_addr;             //* address at which the section is relocated */
        public int sh_offset;           // file offset */

        public int nb_hashed_syms;      /* used to resize the hash table */
        public Section link;            /* link to another section */
        public Section reloc;           /* corresponding section for relocation, if any */
        public Section hash;            /* hash table for symbols */
        public Section prev;            /* previous section on section stack */

        public String name;

        public Section(Tidepool tp, string _name, int _sh_type, int _sh_flags)
        {
            name = _name;
            sh_type = _sh_type;
            sh_flags = _sh_flags;

            data_offset = 0;
            data = null;
            data_allocated = 0;

            sh_name = 0;            // elf section name (only used during output) */
            sh_num = 0;             // elf section number */
            sh_info = 0;            // elf section info */
            sh_addralign = 0;       // elf section alignment */
            sh_entsize = 0;         // elf entry size */

            sh_size = 0;
            sh_addr = 0;
            sh_offset = 0;

            nb_hashed_syms = 0;
            link = null;            // link to another section */
            reloc = null;           // corresponding section for relocation, if any */
            hash = null;            // hash table for symbols */
            prev = null;            // previous section on section stack */

        }
    }
}
