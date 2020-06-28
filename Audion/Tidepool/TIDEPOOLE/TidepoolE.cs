using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolModelE
{
    class TidepoolE
    {
        int SYM_STRUCT = 0x40000000;        // struct/union/enum symbol space 
        int SYM_FIELD = 0x20000000;         // struct/union field symbol space 
        int SYM_FIRST_ANOM = 0x10000000;    // first anonymous sym 

        int VT_CONST = 0x30;

        int VT_BTYPE = 0x000f;          // mask for basic type 
        int VT_VOID = 0;                // void type 
        int VT_BYTE = 1;                // signed byte type 
        int VT_SHORT = 2;               // short type 
        int VT_INT = 3;                 // integer type 
        int VT_LLONG = 4;               // 64 bit integer 
        int VT_PTR = 5;                 // pointer 
        int VT_FUNC = 6;                // function type 
        int VT_STRUCT = 7;              // struct/union definition 
        int VT_FLOAT = 8;               // IEEE float 
        int VT_DOUBLE = 9;              // IEEE double 
        int VT_LDOUBLE = 10;            // IEEE long double 

        int VT_UNSIGNED = 0x0010;       // unsigned type 
        int VT_DEFSIGN = 0x0020;        // explicitly signed or unsigned 
        int VT_ARRAY = 0x0040;          // array type (also has VT_PTR) 
        int VT_BITFIELD = 0x0080;       // bitfield modifier 
        int VT_CONSTANT = 0x0100;       // const modifier 
        int VT_VOLATILE = 0x0200;       // volatile modifier 
        int VT_LONG = 0x0800;           // long type (also has VT_INT rsp. VT_LLONG) 

        int VT_EXTERN = 0x00001000;     // extern definition
        int VT_STATIC = 0x00002000;     // static variable 
        int VT_TYPEDEF = 0x00004000;    // typedef definition 
        int VT_INLINE = 0x00008000;     // inline definition 

        int TYPE_ABSTRACT = 1;          // type without variable 
        int TYPE_DIRECT = 2;            // type with variable 

        int VT_STORAGE = 0x0000F000;    //VT_EXTERN | VT_STATIC | VT_TYPEDEF | VT_INLINE
        //int VT_TYPE = (~(VT_STORAGE | VT_STRUCT_MASK));


        Scanner scanner;

        public List<Sym> global_stack;
        public List<Sym> local_stack;

        public TidepoolE()
        {
            scanner = null;

            global_stack = new List<Sym>();
            local_stack = null;
        }

        public void compile(byte[] source)

        {
            scanner = new Scanner(source);
            scanner.next();
            decl(VT_CONST);

            //while (scanner.token != (int)TokenType.tEOF)
            //{
            //    scanner.printToken();
            //    scanner.next();
            //}
            Console.WriteLine("done.");
        }

        public Sym sym_push2(List<Sym> ps, int v, int t, int c)
        {
            Sym s = new Sym();
            s.v = v;
            s.type.t = t;
            s.c = c;

            /* add in stack */
            ps.Add(s);

            return s;
        }

        public Sym sym_push(int v, CType type, int r, int c)
        {
            Sym s;
            List<Sym> ps;

            if (local_stack != null)
                ps = local_stack;
            else
                ps = global_stack;
            s = sym_push2(ps, v, type.t, c);

            return s;
        }

        public void mk_pointer(CType type)
        {
            Sym s;
            s = sym_push(SYM_FIELD, type, 0, -1);
            type.t = VT_PTR | (type.t & VT_STORAGE);
            type.reff = s;
        }


        public int parse_btype(out CType type, out AttributeDef ad)
        {
            type = new CType();
            ad = new AttributeDef();
            int type_found = 0;
            int typespec_found = 0;
            int t = VT_INT;
            int bt = -1;
            int st = -1;
            int u = 0;

            bool done = false;
            while (!done)
            {
                switch (scanner.token)
                {
                    case (int)TokenType.tCHAR:
                        break;

                    case (int)TokenType.tVOID:
                        scanner.next();
                        t = (t & ~(VT_BTYPE | VT_LONG)) | VT_VOID;
                        typespec_found = 1;
                        break;

                    case (int)TokenType.tSHORT:
                        break;

                    case (int)TokenType.tINT:
                        scanner.next();
                        bt = VT_INT;
                        typespec_found = 1;
                        break;

                    case (int)TokenType.tLONG:
                        break;

                    case (int)TokenType.tFLOAT:
                        break;

                    case (int)TokenType.tDOUBLE:
                        break;

                    case (int)TokenType.tENUM:
                        break;

                    case (int)TokenType.tSTRUCT:
                        break;

                    case (int)TokenType.tUNION:
                        break;

                    case (int)TokenType.tCONST:
                        break;

                    case (int)TokenType.tVOLATILE:
                        break;

                    case (int)TokenType.tSIGNED:
                        break;

                    case (int)TokenType.tUNSIGNED:
                        t |= VT_DEFSIGN | VT_UNSIGNED;
                        scanner.next();
                        typespec_found = 1;
                        break;

                    case (int)TokenType.tREGISTER:
                    case (int)TokenType.tAUTO:
                    case (int)TokenType.tRESTRICT:
                        scanner.next();
                        break;

                    case (int)TokenType.tEXTERN:
                        t |= VT_EXTERN;
                        scanner.next();
                        break;

                    case (int)TokenType.tSTATIC:
                        t |= VT_STATIC;
                        scanner.next();
                        break;

                    case (int)TokenType.tTYPEDEF:
                        t |= VT_TYPEDEF;
                        scanner.next();
                        break;

                    case (int)TokenType.tINLINE:
                        t |= VT_INLINE;
                        scanner.next();
                        break;

                    default:
                        if (typespec_found != 0)
                            done = true;

                        break;
                }
                type_found = 1;
            }

            type.t = t;
            return type_found;
        }

        public CType type_decl(CType type, AttributeDef ad, ref int v, int td)
        {
            int qualifiers = 0;
            bool start = true;
            while (scanner.token == (int)TokenType.tSTAR)
            {
                if (start)
                {
                    qualifiers = 0;
                }
                scanner.next();
                switch(scanner.token)
                {
                    case (int)TokenType.tCONST:
                        qualifiers |= VT_CONSTANT;
                        start = false;
                        break;

                    case (int)TokenType.tVOLATILE:
                        qualifiers |= VT_VOLATILE;
                        start = false;
                        break;

                    case (int)TokenType.tRESTRICT:                        
                        start = false;
                        break;

                    default:
                        break;
                }
                mk_pointer(type);
                type.t |= qualifiers;

            }

            if (scanner.token == (int)TokenType.tLPAREN)
            {

            }
            else if ((scanner.token >= (int)TokenType.tIDENT) && ((td & TYPE_DIRECT) != 0))
            {
                v = scanner.token;
                scanner.next();
            }
            else
            {
                v = 0;
            }
            return null;
        }

        public int decl0(int l, int is_for_loop_init, Sym func_sym)
        {
            int v = 0;
            CType type;
            CType btype;
            Sym sym;
            AttributeDef ad;

            while (true)
            {
                if (parse_btype(out btype, out ad) != 0)
                {
                    if (is_for_loop_init != 0)
                        return 0;
                }

                if (scanner.token == (int)TokenType.tSEMICLN)
                {

                }

                while (true)
                {
                    type = new CType(btype);
                    type_decl(type, ad, ref v, TYPE_DIRECT);

                    if (scanner.token == (int)TokenType.tLBRACE)
                    {
                        break;
                    }
                    else
                    {
                        if ((type.t & VT_TYPEDEF) != 0)
                        {
                            sym = sym_push(v, type, 0, 0);
                        }
                    }

                    if (scanner.token != (int)TokenType.tCOMMA)
                    {
                        if (is_for_loop_init != 0)
                            return 1;
                        scanner.skip((int)TokenType.tSEMICLN);
                        break;
                    }
                    scanner.next();
                }
            }
        }


        public void decl(int l)
        {
            decl0(l, 0, null);
        }

    }

    public class Sym
    {
        public int v;
        public CType type;
        public int c;

        public Sym()
        {
            v = 0;
            type = new CType();
            c = 0;
        }
    }

    public class CType
    {
        public int t;
        public Sym reff;


        public CType()
        {
            t = 0;
            reff = null;
        }

        public CType(CType that)
        {
            t = that.t;
            reff = that.reff;
        }
    }

    public class AttributeDef
    {

    }

}
