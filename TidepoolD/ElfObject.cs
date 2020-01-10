﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolD
{
    public class ElfObject
    {
    }

    //-------------------------------------------------------------------------

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
