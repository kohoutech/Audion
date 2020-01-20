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

//
namespace TidepoolD
{
    public class RJDObject
    {
    }

    //-------------------------------------------------------------------------

    // The ELF file header.  This appears at the start of every ELF file.  */
    public class ElfEhdr
    {
        int EI_NIDENT = 16;

        public static int ElfEhdrSize = 0x34;

        public static byte ELFMAG0 = 0x7f;		    /* Magic number byte 0 */
        public static byte ELFMAG1 = (byte)'E';		/* Magic number byte 1 */
        public static byte ELFMAG2 = (byte)'L';		/* Magic number byte 2 */
        public static byte ELFMAG3 = (byte)'F';		/* Magic number byte 3 */
        public static byte ELFCLASS32 = 1;		    /* 32-bit objects */
        public static byte ELFDATA2LSB = 1; 		/* 2's complement, little endian */
        public static byte EV_CURRENT = 1;		    /* Current version */

        public static int ET_REL = 1;		        /* Relocatable file */

        public static int EM_386 = 3;		        /* Intel 80386 */

        public byte[] e_ident;	        /* Magic number and other info */

        public int e_type;			    /* Object file type */
        public int e_machine;		    /* Architecture */

        public int e_version;		    /* Object file version */
        public int e_entry;		        /* Entry point virtual address */
        public int e_phoff;		        /* Program header table file offset */
        public int e_shoff;		        /* Section header table file offset */
        public int e_flags;		        /* Processor-specific flags */

        public int e_ehsize;		    /* ELF header size in bytes */
        public int e_phentsize;		    /* Program header table entry size */
        public int e_phnum;		        /* Program header table entry count */
        public int e_shentsize;		    /* Section header table entry size */
        public int e_shnum;		        /* Section header table entry count */
        public int e_shstrndx;		    /* Section header string table index */

        public ElfEhdr()
        {
            e_ident = new byte[EI_NIDENT];
            e_type = 0;
            e_machine = 0;

            e_version = 0;
            e_entry = 0;
            e_phoff = 0;
            e_shoff = 0;
            e_flags = 0;

            e_ehsize = 0;
            e_phentsize = 0;
            e_phnum = 0;
            e_shentsize = 0;
            e_shnum = 0;
            e_shstrndx = 0;
        }

        public void writeOut(System.IO.FileStream f)
        {
            byte[] data = new byte[ElfEhdrSize];

            Array.Copy(e_ident, data, EI_NIDENT);

            Array.Copy(BitConverter.GetBytes((short)e_type), 0, data, 0x10, 2);
            Array.Copy(BitConverter.GetBytes((short)e_machine), 0, data, 0x12, 2);

            Array.Copy(BitConverter.GetBytes(e_version), 0, data, 0x14, 4);
            Array.Copy(BitConverter.GetBytes(e_entry), 0, data, 0x18, 4);
            Array.Copy(BitConverter.GetBytes(e_phoff), 0, data, 0x1c, 4);
            Array.Copy(BitConverter.GetBytes(e_shoff), 0, data, 0x20, 4);
            Array.Copy(BitConverter.GetBytes(e_flags), 0, data, 0x24, 4);

            Array.Copy(BitConverter.GetBytes((short)e_ehsize), 0, data, 0x28, 2);
            Array.Copy(BitConverter.GetBytes((short)e_phentsize), 0, data, 0x2a, 2);
            Array.Copy(BitConverter.GetBytes((short)e_phnum), 0, data, 0x2c, 2);
            Array.Copy(BitConverter.GetBytes((short)e_shentsize), 0, data, 0x2e, 2);
            Array.Copy(BitConverter.GetBytes((short)e_shnum), 0, data, 0x30, 2);
            Array.Copy(BitConverter.GetBytes((short)e_shstrndx), 0, data, 0x32, 2);

            f.Write(data, 0, ElfEhdrSize);
        }
    }

    //-------------------------------------------------------------------------

    // Section header.  */
    public class ElfShdr        
    {
        public static int ElfShdrSize = 0x28;

        public int sh_name;		        /* Section name (string tbl index) */
        public SectionType sh_type;		/* Section type */
        public SectionFlags sh_flags;   /* Section flags */
        public int sh_addr;		        /* Section virtual addr at execution */
        public int sh_offset;		    /* Section file offset */
        public int sh_size;		        /* Section size in bytes */
        public int sh_link;		        /* Link to another section */
        public int sh_info;		        /* Additional section information */
        public int sh_addralign;		/* Section alignment */
        public int sh_entsize;		    /* Entry size if section holds table */

        public void writeOut(System.IO.FileStream f)
        {
            byte[] data = new byte[ElfShdrSize];

            Array.Copy(BitConverter.GetBytes(sh_name), 0, data, 0x00, 4);
            Array.Copy(BitConverter.GetBytes((int)sh_type), 0, data, 0x04, 4);
            Array.Copy(BitConverter.GetBytes((int)sh_flags), 0, data, 0x08, 4);
            Array.Copy(BitConverter.GetBytes(sh_addr), 0, data, 0x0c, 4);
            Array.Copy(BitConverter.GetBytes(sh_offset), 0, data, 0x10, 4);
            Array.Copy(BitConverter.GetBytes(sh_size), 0, data, 0x14, 4);
            Array.Copy(BitConverter.GetBytes(sh_link), 0, data, 0x18, 4);
            Array.Copy(BitConverter.GetBytes(sh_info), 0, data, 0x1c, 4);
            Array.Copy(BitConverter.GetBytes(sh_addralign), 0, data, 0x20, 4);
            Array.Copy(BitConverter.GetBytes(sh_entsize), 0, data, 0x24, 4);

            f.Write(data, 0, ElfShdrSize);
        }
    }

    //-------------------------------------------------------------------------

    // Symbol table entry.  */
    public class ElfSym     
    {
        public static int ElfSymSize = 0x10;

        public int st_name;		    // Symbol name (string tbl index) */
        public int st_value;		// Symbol value */
        public int st_size;		    // Symbol size */
        public int st_info;		    // Symbol type and binding */
        public int st_other;		// Symbol visibility */
        public int st_shndx;		// Section index */

        public static SymbolBind ST_BIND(int info)
        {
            return (SymbolBind) (info >> 4);
        }

        public static int ST_INFO(SymbolBind bind, SymbolType type)	
        {
            return (((int)bind << 4) + ((int)type & 0xf));
        }

        public ElfSym(int _st_name, int _st_value, int _st_size, int _st_info, int _st_other, int _st_shndx)
        {
            st_name = _st_name;
            st_value = _st_value;
            st_size = _st_size;
            st_info = _st_info;
            st_other = _st_other;
            st_shndx = _st_shndx;
        }

        public static ElfSym readData(byte[] data, int offset)
        {
            int _st_name = BitConverter.ToInt32(data, offset);
            int _st_value = BitConverter.ToInt32(data, offset + 0x4);
            int _st_size = BitConverter.ToInt32(data, offset + 0x8);
            int _st_info = data[offset + 0xc];
            int _st_other = data[offset + 0xd];
            int _st_shndx = BitConverter.ToInt16(data, offset + 0xe);

            ElfSym sym = new ElfSym(_st_name, _st_value, _st_size, _st_info, _st_other, _st_shndx);
            return sym;
        }

        public void writeData(byte[] data, int offset)
        {
            Array.Copy(BitConverter.GetBytes(st_name), 0, data, offset, 4);
            Array.Copy(BitConverter.GetBytes(st_value), 0, data, offset+0x4, 4);
            Array.Copy(BitConverter.GetBytes(st_size), 0, data, offset+0x8, 4);
            data[offset + 0x0c] = (byte)st_info;
            data[offset + 0x0d] = (byte)st_other;
            Array.Copy(BitConverter.GetBytes((short)st_shndx), 0, data, offset+0xe, 2);
        }

    }

    // Legal values for ST_BIND subfield of st_info (symbol binding).  */
    public enum SymbolBind
    {
        STB_LOCAL = 0,		    // Local symbol */
        STB_GLOBAL = 1,		    // Global symbol */
        STB_WEAK = 2,		    // Weak symbol */
        STB_NUM = 3,		    // Number of defined types.  */
        STB_LOOS = 10,		    // Start of OS-specific */
        STB_GNU_UNIQUE = 10,	// Unique symbol.  */
        STB_HIOS = 12,		    // End of OS-specific */
        STB_LOPROC = 13,		// Start of processor-specific */
        STB_HIPROC = 15		    // End of processor-specific */
    }

    // Legal values for ST_TYPE subfield of st_info (symbol type).  */
    public enum SymbolType
    {
        STT_NOTYPE = 0,		    // Symbol type is unspecified */
        STT_OBJECT = 1,		    // Symbol is a data object */
        STT_FUNC = 2,		    // Symbol is a code object */
        STT_SECTION = 3,		// Symbol associated with a section */
        STT_FILE = 4,		    // Symbol's name is file name */
        STT_COMMON = 5,		    // Symbol is a common data object */
        STT_TLS = 6,		    // Symbol is thread-local data object*/
        STT_NUM = 7,		    // Number of defined types.  */
        STT_LOOS = 10,		    // Start of OS-specific */
        STT_GNU_IFUNC = 10,		// Symbol is indirect code object */
        STT_HIOS = 12,		    // End of OS-specific */
        STT_LOPROC = 13,		// Start of processor-specific */
        STT_HIPROC = 15,		// End of processor-specific */
    }

    //-------------------------------------------------------------------------

    // Program segment header.  */
    public class ElfPhdr
    {
        public static int ElfPhdrSize = 0x20;

        public int p_type;			/* Segment type */
        public int p_offset;		/* Segment file offset */
        public int p_vaddr;		    /* Segment virtual address */
        public int p_paddr;		    /* Segment physical address */
        public int p_filesz;		/* Segment size in file */
        public int p_memsz;		    /* Segment size in memory */
        public int p_flags;		    /* Segment flags */
        public int p_align;		    /* Segment alignment */
    }
}
