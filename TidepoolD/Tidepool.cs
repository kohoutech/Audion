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

        public bool verbose;        // if true, display some information during compilation */
        public bool nostdinc;       // if true, no standard headers are added */
        public bool nostdlib;       // if true, no standard libraries are added */
        public bool nocommon;       // if true, do not use common symbols for .bss data */
        public bool static_link;    // if true, static linking is performed */
        public bool rdynamic;       // if true, all symbols are exported */
        public bool symbolic;       // if true, resolve symbols in the current module first */
        public bool alacarte_link;  // if true, only link in referenced objects from archive */

        public OutputType output_type;
        public OutputFormat output_format;

        // C language options */
        public bool char_is_unsigned;
        public bool leading_underscore;
        public bool ms_extensions;	            // allow nested named struct w/o identifier behave like unnamed */
        public bool dollars_in_identifiers;	    // allows '$' char in identifiers */
        public bool ms_bitfields;               // if true, emulate MS algorithm for aligning bitfields */

        // warning switches */
        public bool warn_write_strings;
        public bool warn_unsupported;
        public bool warn_error;
        public bool warn_none;
        public bool warn_implicit_function_declaration;
        public bool warn_gcc_compat;

        // compile with debug symbol (and use them if error during execution) */
        public bool do_debug;

        public int nb_errors;

        // sections */
        public List<Section> sections;
        public List<Section> priv_sections;

        public Section got;                     // got & plt handling */
        public Section plt;

        public Section dynsymtab_section;       // temporary dynamic symbol sections (for dll loading) */
        public Section dynsym;                  // exported dynamic symbol section */
        public Section symtab;                  // copy of the global symtab_section variable */

        public Tidepool()
        {
            verbose = false;
            nostdinc = false;
            nostdlib = false;
            nocommon = true;
            static_link = false;
            rdynamic = false;
            symbolic = false;
            alacarte_link = true;

            warn_implicit_function_declaration = true;
            ms_extensions = true;

            //debug
            output_type = OutputType.TP_OUTPUT_OBJ;
            output_format = OutputFormat.TP_OUTPUT_FORMAT_ELF;

            sections = new List<Section>();
            priv_sections = new List<Section>();

            pp = new Preprocessor(this);
            gen = new Generator(this);
            link = new Linker(this);

            //wire up components
            gen.pp = pp;
            gen.link = link;
        }

        //asm_instr
        //asm_global_instr

        //normalize_slashes
        //tcc_set_lib_path_w32
        //tcc_add_systemdir
        //DllMain
        //pstrcpy
        //pstrcat
        //pstrncpy
        //tcc_basename
        //tcc_fileextension
        //tcc_free
        //tcc_malloc
        //tcc_mallocz
        //tcc_realloc
        //tcc_strdup
        //tcc_memcheck
        //malloc_check
        //tcc_malloc_debug
        //tcc_free_debug
        //tcc_mallocz_debug
        //tcc_realloc_debug
        //tcc_strdup_debug
        //tcc_memcheck
        //dynarray_add
        //dynarray_reset
        //tcc_split_path

        //strcat_vprintf
        //strcat_printf
        //error1
        //tcc_set_error_func
        //tcc_error_noabort

        public void tpError(String fmt, params object[] args)
        {
            Console.Out.WriteLine(string.Format(fmt, args));
        }

        //tcc_warning

        public void tpOpenBuf(string filename, int initlen)
        {
            int buflen = (initlen != 0) ? initlen : BufferedFile.IO_BUF_SIZE;

            BufferedFile bf = new BufferedFile();
            bf.buffer = new char[buflen];
            //bf.filename = String.Copy(filename);
            //debug
            bf.filename = "debug/test2.c";
            pp.file = bf;
        }

        public void tpClose()
        {
            //throw new NotImplementedException();
        }

        //tcc_open

        public int tpCompile()                      //tcc_compile
        {
            return gen.tpgen_compile();
        }

        public int compileCode(String str)          //tcc_compile_string
        {
            str = str + '\0';                               //add null to end of string to mark end for scanner
            tpOpenBuf("<string>", str.Length);
            str.ToCharArray().CopyTo(pp.file.buffer, 0);
            int result = tpCompile();
            tpClose();
            return result;
        }

        //tcc_define_symbol
        //tcc_undefine_symbol
        //tcc_cleanup
        //tcc_new
        //tcc_delete
        //tcc_set_output_type
        //tcc_add_include_path
        //tcc_add_sysinclude_path
        //tcc_add_file_internal
        //tcc_add_file
        //tcc_add_library_path
        //tcc_add_library_internal
        //tcc_add_dll
        //tcc_add_crt
        //tcc_add_library
        //tcc_add_library_err
        //tcc_add_pragma_libs
        //tcc_add_symbol
        //tcc_set_lib_path
        //no_flag
        //set_flag
        //strstart
        //link_option
        //skip_linker_arg
        //copy_linker_arg
        //tcc_set_linker
        //parse_option_D
        //args_parser_add_file
        //args_parser_make_argv
        //args_parser_listfile
        //tcc_parse_args
        //tcc_set_options
        //tcc_print_stats
    }
}
