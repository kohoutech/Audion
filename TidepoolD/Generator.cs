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
    public class Generator
    {
        public Tidepool tp;
        public Preprocessor pp;
        public i386Generator i386;
        public Linker link;

        const int VSTACK_SIZE = 256;

        const int SYM_STRUCT = 0x40000000;         // struct/union/enum symbol space */
        const int SYM_FIELD = 0x20000000;          // struct/union field symbol space */
        const int SYM_FIRST_ANOM = 0x10000000;     // first anonymous sym */

        const int TYPE_ABSTRACT = 1;        // type without variable */
        const int TYPE_DIRECT = 2;          // type with variable */

        public int ind;

        public List<Sym> global_stack;
        public List<Sym> local_stack;
        public List<Sym> define_stack;
        public List<Sym> global_label_stack;
        public List<Sym> local_label_stack;
        public int local_scope;
        public int in_sizeof;
        public int section_sym;

        public SValue[] __vstack;
        public int vtop;

        public uint nocode_wanted;      // no code generation wanted 

        public CType func_vt;           // current function return type (used by return instruction) */

        public int last_line_num;
        public int last_ind;
        public int func_ind;            // debug last line number and pc */
        public String funcname;

        //cons
        public Generator(Tidepool _tp)
        {
            tp = _tp;
            i386 = new i386Generator(this);

            global_stack = new List<Sym>();
            local_stack = null;
            __vstack = new SValue[VSTACK_SIZE + 1];
        }

        //---------------------------------------------------------------------

        public int tpgen_compile()      //tccgen_compile
        {
            link.cur_text_section = null;
            pp.next();
            decl(ValueType.VT_CONST);

            return 0;
        }

        //- symbol management -------------------------------------------------

        public Sym sym_push2(List<Sym> ps, int v, ValueType t, int c)
        {
            Sym s;

            s = new Sym();
            s.v = v;
            s.type.t = t;
            s.c = c;

            /* add in stack */
            ps.Add(s);
            return s;
        }


        public Sym sym_find(int v)
        {
            return null;
        }

        public Sym sym_push(int v, CType type, int r, int c)
        {
            List<Sym> ps = global_stack;
            if (local_stack != null)
            {
                ps = local_stack;
            }
            Sym s = sym_push2(ps, v, type.t, c);
            return s;
        }

        public Sym global_identifier_push(int v, ValueType t, int c)
        {
            Sym s = sym_push2(global_stack, v, t, c);
            return s;
        }

        //---------------------------------------------------------------------

        public void vsetc(CType type, int r, CValue vc)
        {

            if (vtop >= (VSTACK_SIZE - 1))
                tp.tpError("memory full (vstack)");

            SValue sv = new SValue();
            sv.type = type;
            sv.r = r;
            sv.r2 = ValueType.VT_CONST;
            sv.c = vc;
            sv.sym = null;
            vtop++;
            __vstack[vtop] = sv;
        }

        public void vpop()
        {
            vtop--;
        }

        public Sym external_global_sym(int v, CType type, ValueType r)
        {
            Sym s;

            s = sym_find(v);
            if (s != null)
            {
                s = global_identifier_push(v, (type.t | ValueType.VT_EXTERN), 0);
                s.type.reff = type.reff;
                s.r = (int)(r | ValueType.VT_CONST | ValueType.VT_SYM);
            }
            return s;
        }

        public void patch_storage()
        {
            //throw new NotImplementedException();
        }

        public int get_reg(int rc)
        {
            return 0;
        }

        public int gv(int rc)
        {
            int r = get_reg(rc); ;
            i386.load(r, __vstack[vtop]);
            return r;
        }

        public bool parseBaseType(CType type, ref AttributeDef ad)      //parse_btype
        {
            ValueType t = ValueType.VT_INT;
            ValueType u;
            ValueType basic_type;
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
                        u = ValueType.VT_INT;
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
                        s = sym_find(pp.tok.num);
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

        public int post_type(CType type, AttributeDef ad, ValueType storage, int td)
        {
            int l = 0;
            int arg_size = 0;
            Sym s;
            Sym first = null;

            if (pp.tok.type == TokenType.LPAREN)
            {
                pp.next();
                if (pp.tok.type == TokenType.RPAREN)
                    l = 0;

                pp.skip(TokenType.RPAREN);
                type.t &= ~(ValueType.VT_CONSTANT);

                ad.f.func_args = arg_size;
                ad.f.func_type = l;
                s = sym_push(SYM_FIELD, type, 0, 0);
                s.a = ad.a;
                s.f = ad.f;
                s.next = first;
                type.t = ValueType.VT_FUNC;
                type.reff = s;

            }
            return l;
        }

        public CType type_decl(CType type, AttributeDef ad, ref int v, int td)
        {
            ValueType storage = type.t & ValueType.VT_STORAGE;
            type.t &= ~(ValueType.VT_STORAGE);
            CType post = type;
            CType ret = type;

            while (pp.tok.type == TokenType.STAR)
            {
            }

            if (pp.tok.type == TokenType.LPAREN)
            {
            }
            else if ((pp.tok.type == TokenType.IDENT) && ((td & TYPE_DIRECT) != 0))
            {
                /* type identifier */
                v = pp.tok.num;
                pp.next();
            }
            post_type(post, ad, storage, 0);
            return ret;
        }

        //---------------------------------------

        public void unary()
        {
            ValueType t;
            CType type = new CType();

            switch (pp.tok.type)
            {
                case TokenType.INTCONST:
                    t = ValueType.VT_INT;
                    type.t = t;
                    vsetc(type, (int)ValueType.VT_CONST, pp.tokc);
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

        //---------------------------------------

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
                    gfunc_return(func_vt);
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
            i386.gfunc_prolog(sym.type);
            local_scope = 0;

            block(0, 0, 0);
            nocode_wanted = 0;

            i386.gfunc_epilog();
            link.cur_text_section.data_offset = ind;


            nocode_wanted = 0x80000000;
        }

        public int decl0(ValueType l, int is_for_loop_init, Sym func_sym)
        {
            int v = 0;
            int has_init;
            int r;
            CType type;
            CType btype = new CType();
            Sym sym;
            AttributeDef ad = null;

            while (true)
            {
                if (!parseBaseType(btype, ref ad))
                {
                    break;
                }

                if (pp.tok.type == TokenType.SEMICOLON)
                {
                }

                while (true)
                {
                    type = btype;
                    type_decl(type, ad, ref v, TYPE_DIRECT);

                    if (pp.tok.type == TokenType.LBRACE)        //function body
                    {
                        sym = external_global_sym(v, type, 0);
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

        public void decl(ValueType l)
        {
            decl0(l, 0, null);
        }
    }

    //-------------------------------------------------------------------------

    public class SValue
    {
        public CType type;      // type */
        public int r;           // register + flags */
        public ValueType r2;    // second register, used for 'long long' type. If not used, set to VT_CONST */
        public CValue c;        // constant, if VT_CONST */
        public Sym sym;         // symbol, if (VT_SYM | VT_CONST), or if result of unary() for an identifier. */
    }

    public class SymAttr
    {
    }

    public class FuncAttr
    {
        public int func_call;          // calling convention (0..5), see below */
        public int func_type;          // FUNC_OLD/NEW/ELLIPSIS */
        public int func_args;          // PE __stdcall args */
    }

    public class AttributeDef
    {
        public SymAttr a;
        public FuncAttr f;
        public Section section;

        public AttributeDef()
        {
            a = new SymAttr();
            f = new FuncAttr();
            section = null;
        }
    }

    public class Sym
    {
        public int v;               // symbol token */
        public int r;               // associated register or VT_CONST/VT_LOCAL and LVAL type */
        public SymAttr a;           // symbol attributes */
        public int c;               // associated number or Elf symbol index */
        public FuncAttr f;          // function attributes */
        public CType type;          // associated type */
        public Sym next;            // next related symbol (for fields and anoms) */
        int asm_label;              // associated asm label */

        public Sym prev;            // prev symbol in stack */
        public Sym prev_tok;        // previous symbol for this token */

        public Sym()
        {
            v = 0;
            r = 0;
            a = new SymAttr();
            c = 0;
            f = new FuncAttr();
            type = new CType();
            next = null;
            prev = null;
            prev_tok = null;
        }
    }

    [Flags]
    public enum ValueType
    {
        VT_VALMASK = 0x003f,            // mask for value location, register or: */
        VT_CONST = 0x0030,              // constant in vc (must be first non register value) */
        VT_LLOCAL = 0x0031,             // lvalue, offset on stack */
        VT_LOCAL = 0x0032,              // offset on stack */
        VT_CMP = 0x0033,                // the value is stored in processor flags (in vc) */
        VT_JMP = 0x0034,                // value is the consequence of jmp true (even) */
        VT_JMPI = 0x0035,               // value is the consequence of jmp false (odd) */
        VT_LVAL = 0x0100,               // var is an lvalue */
        VT_SYM = 0x0200,                // a symbol value is added */
        VT_MUSTCAST = 0x0400,           // value must be casted to be correct (used for char/short stored in integer registers) */
        VT_MUSTBOUND = 0x0800,          // bound checking must be done before dereferencing value */
        VT_BOUNDED = 0x8000,            // value is bounded. The address of the bounding function call point is in vc */
        VT_LVAL_BYTE = 0x1000,          // lvalue is a byte */
        VT_LVAL_SHORT = 0x2000,         // lvalue is a short */
        VT_LVAL_UNSIGNED = 0x4000,      // lvalue is unsigned */

        VT_LVAL_TYPE = (VT_LVAL_BYTE | VT_LVAL_SHORT | VT_LVAL_UNSIGNED),

        // types */
        VT_BTYPE = 0x000f,              // mask for basic type */
        VT_VOID = 0,                    // void type */
        VT_BYTE = 1,                    // signed byte type */
        VT_SHORT = 2,                   // short type */
        VT_INT = 3,                     // integer type */
        VT_LLONG = 4,                   // 64 bit integer */
        VT_PTR = 5,                     // pointer */
        VT_FUNC = 6,                    // function type */
        VT_STRUCT = 7,                  // struct/union definition */
        VT_FLOAT = 8,                   // IEEE float */
        VT_DOUBLE = 9,                  // IEEE double */
        VT_LDOUBLE = 10,                // IEEE long double */
        VT_BOOL = 11,                   // ISOC99 boolean type */
        VT_QLONG = 13,                  // 128-bit integer. Only used for x86-64 ABI */
        VT_QFLOAT = 14,                 // 128-bit float. Only used for x86-64 ABI */

        VT_UNSIGNED = 0x0010,           // unsigned type */
        VT_DEFSIGN = 0x0020,            // explicitly signed or unsigned */
        VT_ARRAY = 0x0040,              // array type (also has VT_PTR) */
        VT_BITFIELD = 0x0080,           // bitfield modifier */
        VT_CONSTANT = 0x0100,           // const modifier */
        VT_VOLATILE = 0x0200,           // volatile modifier */
        VT_VLA = 0x0400,                // VLA type (also has VT_PTR and VT_ARRAY) */
        VT_LONG = 0x0800,                // long type (also has VT_INT rsp. VT_LLONG) */

        VT_EXTERN = 0x00001000,  // extern definition */
        VT_STATIC = 0x00002000,  // static variable */
        VT_TYPEDEF = 0x00004000,  // typedef definition */
        VT_INLINE = 0x00008000,  // inline definition */

        VT_STRUCT_MASK = (((1 << (6 + 6)) - 1) << 20 | VT_BITFIELD),


        VT_STORAGE = (VT_EXTERN | VT_STATIC | VT_TYPEDEF | VT_INLINE),
        VT_TYPE = (~(VT_STORAGE | VT_STRUCT_MASK))

    }

    //-------------------------------------------------------------------------

    public class i386Generator
    {
        public Generator gen;

        const int FUNC_PROLOG_SIZE = 10;
        public static int RC_EAX = 0x0004;
        public static int RC_IRET = RC_EAX; /* function return: integer register */

        public int func_sub_sp_offset;
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
                gen.link.cur_text_section.realloc(ind1);
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
            int fr = sv.r;
            ValueType ft = sv.type.t & ~(ValueType.VT_DEFSIGN);
            int fc = (int)sv.c.i;

            ft &= ~(ValueType.VT_VOLATILE | ValueType.VT_CONSTANT);

            int v = fr & (int)ValueType.VT_VALMASK;
            if ((fr & (int)ValueType.VT_LVAL) != 0)
            {
            }
            else
            {
                if (v == (int)ValueType.VT_CONST)
                {
                    o((uint)(0xb8 + r));            // mov $xx, r */
                    gen_addr32(fr, sv.sym, fc);
                }
            }
        }

        public void gfunc_prolog(CType func_type)
        {
            gen.ind += FUNC_PROLOG_SIZE;
            func_sub_sp_offset = gen.ind;

            func_ret_sub = 0;
        }

        public void gfunc_epilog()
        {
            uint v = 0;
            int saved_ind;

            //exit code
            o(0xc9);          // leave */
            if (func_ret_sub == 0)
            {
                o(0xc3);      // ret */
            }

            //entry code
            saved_ind = gen.ind;
            gen.ind = func_sub_sp_offset - FUNC_PROLOG_SIZE;

            o(0xe58955);                    /* push %ebp, mov %esp, %ebp */
            o(0xec81);                      /* sub esp, stacksize */
            gen_le32(v);

            gen.ind = saved_ind;
        }
    }
}
