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
    public class Generator
    {
        public Tidepool tp;
        public Preprocessor pp;
        public i386Generator i386;
        public Linker link;

        int VT_CONST = 0x0030;

        int VT_INT = 3;

        public int ind;

        public int local_scope;

        public SValue[] __vstack;
        public int vtop;

        public uint nocode_wanted;      // no code generation wanted 

        public int last_line_num;
        public int last_ind;
        public int func_ind;            // debug last line number and pc */
        public String funcname;

        //cons
        public Generator(Tidepool _tp)
        {
            tp = _tp;
            i386 = new i386Generator(this);
            __vstack = new SValue[100];

        }

        //---------------------------------------------------------------------

        public int tpgen_compile()      //tccgen_compile
        {
            link.cur_text_section = null;
            pp.next();
            decl(VT_CONST);

            return 0;
        }

        public Sym sym_find(Token tok)
        {
            return null;
        }

        public void vsetc(CType type, int r, CValue vc)
        {
        }

        public void vpop()
        {
            //throw new NotImplementedException();
        }

        public Sym external_global_sym()
        {
            return null;
        }

        public void patch_storage()
        {
            //throw new NotImplementedException();
        }

        public int gv(int rc)
        {
            int r = 0;
            i386.load(r, __vstack[vtop]);
            return r;
        }

        public bool parse_btype(CType type, ref AttributeDef ad)
        {
            int t = VT_INT;
            int u;
            int basic_type;
            bool type_found = false;
            bool typespec_found = false;
            Sym s;

            ad = new AttributeDef();
            bool done = false;

            while (!done)
            {
                switch (pp.tok.type)
                {
                    case TokenType.INT:
                        u = VT_INT;
                        pp.next();
                        basic_type = u;
                        typespec_found = true;
                        break;

                    default:
                        if (typespec_found)
                        {
                            done = true;
                            break;
                        }
                        s = sym_find(pp.tok);
                        if (s == null)
                        {
                            done = true;
                            break;
                        }
                        break;
                }
                if (!done)
                {
                    type_found = true;
                }
            }
            type.t = t;
            return type_found;
        }

        public int post_type()
        {
            int l = 0;

            if (pp.tok.type == TokenType.LPAREN)
            {
                pp.next();
                if (pp.tok.type == TokenType.RPAREN)
                    l = 0;
                pp.skip(TokenType.RPAREN);
            }
            return l;
        }

        public CType type_decl()
        {
            CType ret = null;
            pp.next();
            post_type();
            return ret;
        }

        public void unary()
        {
            int t;
            CType type = new CType();

            switch (pp.tok.type)
            {
                case TokenType.INTCONST:
                    t = VT_INT;
                    type.t = t;
                    vsetc(type, VT_CONST, pp.tokc);
                    pp.next();
                    break;

                default:
                    break;
            }
        }

        public void expr_prod()
        {
            unary();
        }

        public void expr_sum()
        {
            expr_prod();
        }

        public void expr_shift()
        {
            expr_sum();
        }

        public void expr_cmp()
        {
            expr_shift();
        }

        public void expr_cmpeq()
        {
            expr_cmp();
        }

        public void expr_and()
        {
            expr_cmpeq();
        }

        public void expr_xor()
        {
            expr_and();
        }

        public void expr_or()
        {
            expr_xor();
        }

        public void expr_land()
        {
            expr_or();
        }

        public void expr_lor()
        {
            expr_land();
        }

        public void expr_cond()
        {
            expr_lor();
        }

        public void expr_eq()
        {
            expr_cond();
        }

        public void gexpr()
        {
            while (true)
            {
                expr_eq();
                if (pp.tok.type != TokenType.COMMA)
                    break;
                vpop();
                pp.next();
            }
        }

        public void gfunc_return(CType func_type)
        {
            gv(i386Generator.RC_IRET);
            vtop--;
        }

        public void block(int bsym, int csym, int is_expr)
        {
            if (pp.tok.type == TokenType.IF)
            {
                pp.next();
                pp.skip(TokenType.LPAREN);
                gexpr();
                pp.skip(TokenType.RPAREN);

            }
            else if (tp.pp.tok.type == TokenType.WHILE)
            {
            }
            else if (tp.pp.tok.type == TokenType.LBRACE)
            {
                pp.next();
                while (pp.tok.type != TokenType.RBRACE)
                {
                    if (pp.tok.type != TokenType.RBRACE)
                    {
                        if (is_expr != 0)
                            vpop();
                        block(bsym, csym, is_expr);
                    }
                }
                pp.next();
            }
            else if (tp.pp.tok.type == TokenType.RETURN)
            {
                pp.next();
                if (pp.tok.type != TokenType.SEMICOLON)
                {
                    gexpr();
                    gfunc_return(null);
                }
                pp.skip(TokenType.SEMICOLON);
            }
            else if (tp.pp.tok.type == TokenType.BREAK)
            {
            }
            else if (tp.pp.tok.type == TokenType.CONTINUE)
            {
            }
            else if (tp.pp.tok.type == TokenType.FOR)
            {
            }
            else if (tp.pp.tok.type == TokenType.DO)
            {
            }
            else if (tp.pp.tok.type == TokenType.SWITCH)
            {
            }
            if (tp.pp.tok.type == TokenType.CASE)
            {
            }
            else if (tp.pp.tok.type == TokenType.DEFAULT)
            {
            }
            else if (tp.pp.tok.type == TokenType.GOTO)
            {
            }
        }

        public void gen_function(Sym sym)
        {
            nocode_wanted = 0;
            ind = link.cur_text_section.data_offset;

            local_scope = 1;                // for function parameters */
            //i386.gfunc_prolog(sym.type);
            local_scope = 0;

            block(0, 0, 0);
            nocode_wanted = 0;

            i386.gfunc_epilog();
            link.cur_text_section.data_offset = ind;


            nocode_wanted = 0x80000000;
        }

        public int decl0(int l, int is_for_loop_init, Sym func_sym)
        {
            int v;
            int has_init;
            int r;
            CType type;
            CType btype = new CType();
            Sym sym;
            AttributeDef ad = null;

            while (true)
            {
                if (!parse_btype(btype, ref ad))
                {
                    break;
                }

                if (pp.tok.type == TokenType.SEMICOLON)
                {
                }

                while (true)
                {
                    type = btype;
                    type_decl();

                    if (pp.tok.type == TokenType.LBRACE)
                    {
                        sym = external_global_sym();
                        patch_storage();
                        // compute text section */
                        link.cur_text_section = ad.section;
                        if (link.cur_text_section == null)
                            link.cur_text_section = link.text_section;
                        gen_function(sym);
                        break;
                    }
                }
            }
            return 0;
        }

        public void decl(int l)
        {
            decl0(l, 0, null);
        }
    }

    public class SValue
    {
    }

    public class Sym
    {
        public CType type;         // associated type */
    }

    public class CType
    {
        public int t;
        public Sym reff;
    }

    public class AttributeDef
    {
        public Section section;

        public AttributeDef()
        {
            section = null;
        }
    }

    public class i386Generator
    {
        public Generator gen;

        public static int RC_EAX = 0x0004;
        public static int RC_IRET = RC_EAX; /* function return: integer register */

        public int func_ret_sub;


        public i386Generator(Generator _gen)
        {
            gen = _gen;
        }

        public void g(uint c)
        {
            if (gen.nocode_wanted != 0)
                return;
            int ind1 = gen.ind + 1;
            if (ind1 > gen.link.cur_text_section.data_allocated)
            {
                gen.link.section_realloc(gen.link.cur_text_section, ind1);
            }
            gen.link.cur_text_section.data[gen.ind] = (byte)c;
            gen.ind = ind1;
        }

        public void o(uint c)
        {
            while (c != 0)
            {
                g(c);
                c = c >> 8;
            }
        }

        public void gen_le32(uint c)
        {
            g(c);
            g(c >> 8);
            g(c >> 16);
            g(c >> 24);
        }

        public void gen_addr32(int r, Sym sym, int c)
        {
            //if (r & VT_SYM)
            //    greloc(cur_text_section, sym, ind, R_386_32);
            gen_le32((uint)c);
        }

        public void load(int r, SValue sv)
        {
            o((uint)(0xb8 + r));    // mov $xx, r */
        }

        public void gfunc_prolog(CType func_type)
        {
            func_ret_sub = 0;
        }

        public void gfunc_epilog()
        {
            o(0xc9);          // leave */
            if (func_ret_sub == 0)
            {
                o(0xc3);      // ret */
            }


        }

    }

}
