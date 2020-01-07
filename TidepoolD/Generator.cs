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

        int VT_CONST = 0x0030;

        int VT_INT = 3;

        uint nocode_wanted;      // no code generation wanted 

        //cons
        public Generator(Tidepool _tp)
        {
            tp = _tp;
        }

        //---------------------------------------------------------------------

        public int tpgen_compile()
        {
            pp.next();
            decl(VT_CONST);

            return 0;
        }

        public Sym sym_find(Token tok)
        {
            return null;
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

        public bool parse_btype(CType type, AttributeDef ad)
        {
            int t = VT_INT;
            int u;
            int basic_type;
            bool type_found = false;
            bool typespec_found = false;
            Sym s;
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

        public void gexpr()
        {
            pp.next();
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
            
            block(0, 0, 0);
            
            nocode_wanted = 0;
            
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
                if (!parse_btype(btype, ad))
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

    public class Sym
    {
    }

    public class CType
    {
        public int t;
        public Sym reff;
    }

    public class AttributeDef
    {
    }
}
