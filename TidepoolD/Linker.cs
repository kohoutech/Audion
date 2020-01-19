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
    public class Linker
    {
        public Tidepool tp;
        public Section text_section;        // predefined sections */
        public Section data_section;
        public Section bss_section;
        public Section common_section;
        public Section cur_text_section;    // current section where function code is generated */

        public Section symtab_section;      // symbol sections */
        public Section stab_section;        // debug sections */
        public Section stabstr_section;

        public Linker(Tidepool _tp)     //tccelf_new
        {
            tp = _tp;

            /* no section zero */
            Section dummy = new Section(tp, "null", SectionType.SHT_NULL, (SectionFlags)0);

            /* create standard sections */
            text_section = new Section(tp, ".text", SectionType.SHT_PROGBITS, SectionFlags.SHF_ALLOC | SectionFlags.SHF_EXECINSTR);
            data_section = new Section(tp, ".data", SectionType.SHT_PROGBITS, SectionFlags.SHF_ALLOC | SectionFlags.SHF_WRITE);
            bss_section = new Section(tp, ".bss", SectionType.SHT_NOBITS, SectionFlags.SHF_ALLOC | SectionFlags.SHF_WRITE);
            common_section = new Section(tp, ".common", SectionType.SHT_NOBITS, SectionFlags.SHF_PRIVATE);
            common_section.sh_num = (int)SectionNum.SHN_COMMON;

            /* symbols are always generated for linking stage */
            symtab_section = Section.new_symtab(this, ".symtab", SectionType.SHT_SYMTAB, 0, ".strtab", ".hashtab", SectionFlags.SHF_PRIVATE);
            tp.symtab = symtab_section;

            /* private symbol table for dynamic symbols */
            tp.dynsymtab_section = Section.new_symtab(this, ".dynsymtab", SectionType.SHT_SYMTAB, SectionFlags.SHF_PRIVATE | SectionFlags.SHF_DYNSYM,
                ".dynstrtab", ".dynhashtab", SectionFlags.SHF_PRIVATE);

            //get_sym_attr(s, 0, 1);
        }

        //tccelf_bounds_new
        //tccelf_stab_new
        //free_section
        //tccelf_delete
        //tccelf_begin_file
        //tccelf_end_file

        public int put_elf_str(Section s, String sym)
        {
            int len = sym.Length + 1;
            int offset = s.data_offset;
            int ptr = s.ptr_add(len);
            foreach (Char c in sym)
            {
                s.data[ptr++] = (byte)c;
            }
            s.data[ptr] = 0;
            return offset;
        }

        //elf_hash
        //rebuild_hash

        public int put_elf_sym(Section s, int value, int size, int info, int other, int shndx, String name)
        {
            int sym_index = s.ptr_add(ElfSym.ElfSymSize);
            int name_offset = 0;
            if (name != null && name.Length > 0)
            {
                name_offset = put_elf_str(s.link, name);
            }
            ElfSym sym = new ElfSym(name_offset, value, size, info, other, shndx);
            sym.writeData(s.data, sym_index);
            return sym_index;
        }

        //find_elf_sym
        //get_elf_sym_addr
        //tcc_get_symbol
        //tcc_get_symbol_err
        //set_elf_sym

        //put_elf_reloca
        //put_elf_reloc
        //squeeze_multi_relocs
        //put_stabs
        //put_stabs_r
        //put_stabn
        //put_stabd
        //get_sym_attr

        public void sort_syms(Tidepool tp, Section s)
        {
            int nb_syms = s.data_offset / ElfSym.ElfSymSize;
            ElfSym[] new_syms = new ElfSym[nb_syms];
            int[] old_to_new_syms = new int[nb_syms];

            int offset = 0;
            for (int i = 0; i < nb_syms; i++)
            {
                new_syms[i] = ElfSym.readData(s.data, offset);
                offset += ElfSym.ElfSymSize;
            }
        }

        //relocate_syms
        //relocate_section
        //relocate_rel
        //prepare_dynamic_rel
        //build_got
        //put_got_entry
        //build_got_entries
        //put_dt
        //add_init_array_defines
        //tcc_add_support
        //tcc_add_bcheck
        //tcc_add_runtime
        //tcc_add_linker_symbols
        //resolve_common_syms
        //tcc_output_binary
        //fill_got_entry
        //fill_got
        //fill_local_got_entries
        //bind_exe_dynsyms
        //bind_libs_dynsyms
        //export_global_syms

        public int alloc_sec_names(OutputType file_type, Section strsec)
        {
            int textrel = 0;
            for (int i = 1; i < tp.sections.Count; i++)
            {
                Section s = tp.sections[i];
                s.sh_size = s.data_offset;
                if (s.sh_size > 0 || ((s.sh_flags & SectionFlags.SHF_ALLOC) != 0))
                {
                    s.sh_name = put_elf_str(strsec, s.name);
                }
            }
            strsec.sh_size = strsec.data_offset;
            return textrel;
        }

        public int layout_sections(ElfPhdr[] phdr, int phnum, Section interp, Section strsec, dyn_inf dyninf, int[] sec_order)
        {
            OutputType file_type = tp.output_type;
            int sh_order_index = 1;
            int file_offset = 0;
            if (tp.output_format == OutputFormat.TP_OUTPUT_FORMAT_ELF)
            {
                file_offset = ElfEhdr.ElfEhdrSize + (phnum * ElfPhdr.ElfPhdrSize);
            }

            for (int i = 1; i < tp.sections.Count; i++)
            {
                Section s = tp.sections[i];
                if (phnum > 0 && ((s.sh_flags & SectionFlags.SHF_ALLOC) != 0))
                    continue;
                sec_order[sh_order_index++] = i;

                file_offset = (file_offset + s.sh_addralign - 1) & ~(s.sh_addralign - 1);
                s.sh_offset = file_offset;
                if (s.sh_type != SectionType.SHT_NOBITS)
                    file_offset += s.sh_size;
            }

            return file_offset;

            //return 0xb8;
        }

        //fill_unloadable_phdr
        //fill_dynamic
        //final_sections_reloc

        public void tp_output_elf(FileStream f, int phnum, ElfPhdr[] phdr, int file_offset, int[] sec_order)
        {
            int size;
            int file_type;

            int shnum = tp.sections.Count;

            file_offset = (file_offset + 3) & -4;       /* align to 4 */

            // fill ELF header & write it out
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

            sort_syms(tp, symtab_section);

            //write out section data
            for (int i = 1; i < tp.sections.Count; i++)
            {
                Section s = tp.sections[sec_order[i]];
                if (s.sh_type != SectionType.SHT_NOBITS)
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

            //pad out last section
            while (offset < ehdr.e_shoff)
            {
                f.WriteByte(0);
                offset++;
            }

            //write out section tbl
            for (int i = 0; i < tp.sections.Count; i++)
            {
                ElfShdr sh = new ElfShdr();

                Section s = tp.sections[i];
                if (s != null && s.sh_type != SectionType.SHT_NULL)
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

        public int tp_write_elf_file(String filename, int phnum, ElfPhdr[] phdr, int file_offset, int[] sec_order)
        {
            FileStream f = File.OpenWrite(filename);
            tp_output_elf(f, phnum, phdr, file_offset, sec_order);
            f.Close();
            return 0;
        }

        //  tidy_section_headers

        public int elf_output_file(string outname)
        {
            int i;
            int ret;
            int phnum = 0;
            int shnum = 0;
            OutputType file_type = tp.output_type;
            int file_offset = 0;
            int[] sec_order = null;
            dyn_inf dyninf = null;
            ElfPhdr[] phdr = null;
            Section interp = null;

            // we add a section for symbols */
            Section strsec = new Section(tp, ".shstrtab", SectionType.SHT_STRTAB, 0);
            put_elf_str(strsec, "");

            // Allocate strings for section names */
            int textrel = alloc_sec_names(file_type, strsec);

            // compute number of program headers */
            if (file_type == OutputType.TP_OUTPUT_OBJ)
                phnum = 0;
            else if (file_type == OutputType.TP_OUTPUT_DLL)
                phnum = 3;
            else if (tp.static_link)
                phnum = 2;
            else
                phnum = 5;

            /* allocate program segment headers */
            phdr = new ElfPhdr[phnum];

            /* compute number of sections */
            shnum = tp.sections.Count;

            /* this array is used to reorder sections in the output file */
            sec_order = new int[shnum];
            sec_order[0] = 0;

            file_offset = layout_sections(phdr, phnum, interp, strsec, dyninf, sec_order);

            // Create the ELF file with name 'outname' */
            ret = tp_write_elf_file(outname, phnum, phdr, file_offset, sec_order);
            return ret;
        }

        public void outputFile(String outname)          //tcc_output_file
        {
            elf_output_file(outname);
        }

        //load_data
        //tcc_object_type
        //tcc_load_object_file
        //get_be32
        //get_be64
        //tcc_load_alacarte
        //tcc_load_archive
        //tcc_load_dll
        //ld_next
        //ld_add_file
        //new_undef_syms
        //ld_add_file_list
        //tcc_load_ldscript
    }

    public enum OutputType
    {
        TP_OUTPUT_MEMORY = 1,           // output will be run in memory (default) */
        TP_OUTPUT_EXE = 2,              // executable file */
        TP_OUTPUT_DLL = 3,              // dynamic library */
        TP_OUTPUT_OBJ = 4,              // object file */
        TP_OUTPUT_PREPROCESS = 5        // only preprocess (used internally) */
    }

    public enum OutputFormat
    {
        TP_OUTPUT_FORMAT_ELF = 0,       // default output format: ELF */
        TP_OUTPUT_FORMAT_BINARY = 1,    // binary image output */
        TP_OUTPUT_FORMAT_COFF = 2       // COFF */
    }

    //- section classes -------------------------------------------------------

    public enum SectionNum
    {
        SHN_UNDEF = 0,		            /* Undefined section */
        SHN_LORESERVE = 0xff00,	        /* Start of reserved indices */
        SHN_LOPROC = 0xff00,		    /* Start of processor-specific */
        SHN_BEFORE = 0xff00,		    /* Order section before all others (Solaris).  */
        SHN_AFTER = 0xff01,	            /* Order section after all others (Solaris).  */
        SHN_HIPROC = 0xff1f,	        /* End of processor-specific */
        SHN_LOOS = 0xff20,		        /* Start of OS-specific */
        SHN_HIOS = 0xff3f,		        /* End of OS-specific */
        SHN_ABS = 0xfff1,		        /* Associated symbol is absolute */
        SHN_COMMON = 0xfff2,		    /* Associated symbol is common */
        SHN_XINDEX = 0xffff,		    /* Index is in extra table.  */
        SHN_HIRESERVE = 0xffff		    /* End of reserved indices */
    }

    public enum SectionType : long
    {
        SHT_NULL = 0,		            /* Section header table entry unused */
        SHT_PROGBITS = 1,		        /* Program data */
        SHT_SYMTAB = 2,		            /* Symbol table */
        SHT_STRTAB = 3,		            /* String table */
        SHT_RELA = 4,		            /* Relocation entries with addends */
        SHT_HASH = 5,		            /* Symbol hash table */
        SHT_DYNAMIC = 6,		        /* Dynamic linking information */
        SHT_NOTE = 7,		            /* Notes */
        SHT_NOBITS = 8,		            /* Program space with no data (bss) */
        SHT_REL = 9,		            /* Relocation entries, no addends */
        SHT_SHLIB = 10,		            /* Reserved */
        SHT_DYNSYM = 11,		        /* Dynamic linker symbol table */
        SHT_INIT_ARRAY = 14,		    /* Array of constructors */
        SHT_FINI_ARRAY = 15,		    /* Array of destructors */
        SHT_PREINIT_ARRAY = 16,		    /* Array of pre-constructors */
        SHT_GROUP = 17,		            /* Section group */
        SHT_SYMTAB_SHNDX = 18,		    /* Extended section indices */
        SHT_NUM = 19,		            /* Number of defined types.  */
        SHT_LOOS = 0x60000000,	        /* Start OS-specific.  */
        SHT_GNU_ATTRIBUTES = 0x6ffffff5,	/* Object attributes.  */
        SHT_GNU_HASH = 0x6ffffff6,	    /* GNU-style hash table.  */
        SHT_GNU_LIBLIST = 0x6ffffff7,     /* Prelink library list */
        SHT_CHECKSUM = 0x6ffffff8,	    /* Checksum for DSO content.  */
        SHT_LOSUNW = 0x6ffffffa,	        /* Sun-specific low bound.  */
        SHT_SUNW_move = 0x6ffffffa,
        SHT_SUNW_COMDAT = 0x6ffffffb,
        SHT_SUNW_syminfo = 0x6ffffffc,
        SHT_GNU_verdef = 0x6ffffffd,	    /* Version definition section.  */
        SHT_GNU_verneed = 0x6ffffffe,	    /* Version needs section.  */
        SHT_GNU_versym = 0x6fffffff,	    /* Version symbol table.  */
        SHT_HISUNW = 0x6fffffff,	        /* Sun-specific high bound.  */
        SHT_HIOS = 0x6fffffff,	            /* End OS-specific type */
        SHT_LOPROC = 0x70000000,	        /* Start of processor-specific */
        SHT_HIPROC = 0x7fffffff,	        /* End of processor-specific */
        SHT_LOUSER = 0x80000000,	        /* Start of application-specific */
        SHT_HIUSER = 0x8fffffff	            /* End of application-specific */
    }

    [Flags]
    public enum SectionFlags : long
    {
        SHF_WRITE = (1 << 0),	            /* Writable */
        SHF_ALLOC = (1 << 1),	            /* Occupies memory during execution */
        SHF_EXECINSTR = (1 << 2),	        /* Executable */
        SHF_MERGE = (1 << 4),               /* Might be merged */
        SHF_STRINGS = (1 << 5),             /* Contains nul-terminated strings */
        SHF_INFO_LINK = (1 << 6),           /* `sh_info' contains SHT index */
        SHF_LINK_ORDER = (1 << 7),	        /* Preserve order after combining */
        SHF_OS_NONCONFORMING = (1 << 8),    /* Non-standard OS specific handling required */
        SHF_GROUP = (1 << 9),               /* Section is member of a group.  */
        SHF_TLS = (1 << 10),                /* Section hold thread-local data.  */
        SHF_COMPRESSED = (1 << 11),         /* Section with compressed data. */
        SHF_MASKOS = 0x0ff00000,            /* OS-specific.  */
        SHF_MASKPROC = 0xf0000000,          /* Processor-specific */
        SHF_ORDERED = (1 << 30),            /* Special ordering requirement (Solaris).  */
        SHF_EXCLUDE = (1 << 31),            /* Section is excluded unless referenced or allocated (Solaris).*/

        /* special flag to indicate that the section should not be linked to the other ones */
        SHF_PRIVATE = 0x80000000,
        /* section is dynsymtab_section */
        SHF_DYNSYM = 0x40000000,
    }

    public class Section
    {
        public int data_offset;         // current data offset */
        public byte[] data;             // section data */
        public int data_allocated;      // used for realloc() handling */

        public int sh_name;             // elf section name (only used during output) */
        public int sh_num;              // elf section number */
        public SectionType sh_type;     // elf section type */
        public SectionFlags sh_flags;   // elf section flags */
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

        //cons
        public Section(Tidepool tp, string _name, SectionType _sh_type, SectionFlags _sh_flags)
        {
            name = _name;
            sh_type = _sh_type;
            sh_flags = _sh_flags;

            //set alignment
            switch (sh_type)
            {
                case SectionType.SHT_NULL:      //unused section header
                    sh_addralign = 0;
                    break;
                case SectionType.SHT_HASH:
                case SectionType.SHT_REL:
                case SectionType.SHT_RELA:
                case SectionType.SHT_DYNSYM:
                case SectionType.SHT_SYMTAB:
                case SectionType.SHT_DYNAMIC:
                    sh_addralign = 4;
                    break;
                case SectionType.SHT_STRTAB:
                    sh_addralign = 1;
                    break;
                default:
                    sh_addralign = 4;       //PTR_SIZE - gcc/pcc default alignment */
                    break;
            }

            //clear rest of fields
            data_offset = 0;
            data = null;
            data_allocated = 0;

            sh_name = 0;            // elf section name (only used during output) */
            sh_num = 0;             // elf section number */
            sh_info = 0;            // elf section info */
            sh_entsize = 0;         // elf entry size */

            sh_size = 0;
            sh_addr = 0;
            sh_offset = 0;

            nb_hashed_syms = 0;
            link = null;            // link to another section */
            reloc = null;           // corresponding section for relocation, if any */
            hash = null;            // hash table for symbols */
            prev = null;            // previous section on section stack */

            //add the section to tidepool public/private list
            if ((sh_flags & SectionFlags.SHF_PRIVATE) != 0)
            {
                tp.priv_sections.Add(this);
            }
            else
            {
                sh_num = tp.sections.Count;
                tp.sections.Add(this);
            }
        }

        public static Section new_symtab(Linker link, String symtab_name, SectionType sh_type, SectionFlags sh_flags,
                                        String strtab_name, String hash_name, SectionFlags hash_sh_flags)
        {
            Section symtab;
            Section strtab;
            Section hash;
            int ptr;
            int nb_buckets;
            Tidepool tp = link.tp;

            symtab = new Section(tp, symtab_name, sh_type, sh_flags);
            symtab.sh_entsize = ElfSym.ElfSymSize;

            strtab = new Section(tp, strtab_name, SectionType.SHT_STRTAB, sh_flags);
            link.put_elf_str(strtab, "");
            symtab.link = strtab;
            link.put_elf_sym(symtab, 0, 0, 0, 0, 0, null);

            nb_buckets = 1;

            hash = new Section(tp, hash_name, SectionType.SHT_HASH, hash_sh_flags);
            hash.sh_entsize = 4;    //sizeof(int);
            symtab.hash = hash;
            hash.link = symtab;

            ptr = hash.ptr_add((2 + nb_buckets + 1) * 4);
            Array.Copy(BitConverter.GetBytes(nb_buckets), 0, hash.data, ptr, 4);
            Array.Copy(BitConverter.GetBytes(1), 0, hash.data, ptr + 4, 4);
            for (int i = 0; i < nb_buckets + 1; i++)
            {
                Array.Copy(BitConverter.GetBytes(0), 0, hash.data, ptr + ((2 + i) * 4), 4);
            }

            return symtab;
        }

        public void realloc(long new_size)
        {
            int size = this.data_allocated;
            if (size == 0)
                size = 1;
            while (size < new_size)
                size = size * 2;

            byte[] data = new byte[size];
            if (this.data != null)
            {
                Array.Copy(this.data, data, this.data_allocated);
            }
            this.data = data;
            this.data_allocated = size;
        }

        public int add(int size, int align)
        {
            int offset = (data_offset + align - 1) & -align;
            int offset1 = offset + size;
            if (sh_type != SectionType.SHT_NOBITS && offset1 > data_allocated)
                realloc(offset1);
            data_offset = offset1;
            if (align > sh_addralign)
                sh_addralign = align;
            return offset;
        }

        public int ptr_add(int size)
        {
            return add(size, 1);
        }

        public void reserve(int size)
        {
            if (size > data_allocated)
                realloc(size);

            if (size > data_offset)
                data_offset = size;
        }

        //find_section
    }

    // Info to be copied in dynamic section */
    public class dyn_inf
    {
        public Section dynamic;
        public Section dynstr;
        public int data_offset;
        public int rel_addr;
        public int rel_size;
    }
}
