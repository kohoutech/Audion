/* ----------------------------------------------------------------------------
Tidepool(Model E) : a C compiler
 
 based on the 9cc C compiler
 
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

namespace TidepoolE
{
    class Analyzer
    {
        TidePool tp;

        public static tpType void_type = new tpType();   //&(Type){ TY_VOID, 1, 1 };
        public static tpType bool_type = new tpType();   //&(Type){ TY_BOOL, 1, 1 };
        public static tpType char_type = new tpType();   //&(Type){ TY_CHAR, 1, 1 };
        public static tpType short_type = new tpType();   //&(Type){ TY_SHORT, 2, 2 };
        public static tpType int_type = new tpType();   //&(Type){ TY_INT, 4, 4 };
        public static tpType long_type = new tpType();   //&(Type){ TY_LONG, 8, 8 };

        public Analyzer(TidePool _tp)
        {
            tp = _tp;
        }

        public void is_integer()
        {
        }

        public void align_to()
        {
        }

        public tpType new_type(TypeKind kind, int size, int align)
        {
            tpType ty = new tpType();
            ty.kind = kind;
            ty.size = size;
            ty.align = align;
            return ty;
        }

        public tpType pointer_to(tpType bass)
        {
            tpType ty = new_type(TypeKind.TY_PTR, 8, 8);
            ty.bass = bass;
            return ty;
        }

        public void array_of()
        {
        }

        public tpType func_type(tpType return_ty)
        {
            tpType ty = new_type(TypeKind.TY_FUNC, 1, 1);
            ty.return_ty = return_ty;
            return ty;
        }

        public void enum_type()
        {
        }

        public void struct_type()
        {
        }

        public void add_type()
        {
        }
    }

    public enum TypeKind
    {
        TY_VOID,
        TY_BOOL,
        TY_CHAR,
        TY_SHORT,
        TY_INT,
        TY_LONG,
        TY_ENUM,
        TY_PTR,
        TY_ARRAY,
        TY_STRUCT,
        TY_FUNC,
    }

    public class tpType
    {
        public TypeKind kind;
        public int size;                // sizeof() value
        public int align;               // alignment
        public bool is_incomplete;      // incomplete type

        public tpType bass;             // pointer or array
        public int array_len;           // array
        public List<Member> members;    // struct
        public tpType return_ty;        // function

        public void copy(tpType that)
        {
            this.kind = that.kind;
            this.size = that.size;
            this.align = that.align;
            this.is_incomplete = that.is_incomplete;

            this.bass = that.bass;
            this.array_len = that.array_len;
            this.members = that.members;
            this.return_ty = that.return_ty;
        }
    }

    public class Member
    {
    }

}
