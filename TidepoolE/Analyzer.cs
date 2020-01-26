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

        public static tpType void_type  = new tpType();   //&(Type){ TY_VOID, 1, 1 };
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

        public void new_type()
        {
        }

        public void pointer_to()
        {
        }

        public void array_of()
        {
        }

        public void func_type()
        {
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

    public class tpType
    {
    }

    public class Member
    {
    }

}
