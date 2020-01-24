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
    class Parser
    {
        public TidePool tp;

        List<Var> locals;
        List<Var> globals;
        VarScope varscope;
        TagScope tagscope;
        int scope_depth;
        Node current_switch;

        public Parser(TidePool _tp)
        {
            tp = _tp;            
        }

        public Scope enter_scope()
        {
            return null;
        }

        public void leave_scope()
        {
        }

        public VarScope find_var()
        {
            return null;
        }

        public TagScope find_tag()
        {
            return null;
        }

        public Node new_node()
        {
            return null;
        }

        public Node new_binary()
        {
            return null;
        }

        public Node new_unary()
        {
            return null;
        }

        public Node new_num()
        {
            return null;
        }

        public Node new_var_node()
        {
            return null;
        }

        public VarScope push_scope()
        {
            return null;
        }

        public Var new_var()
        {
            return null;
        }

        public Var new_lvar()
        {
            return null;
        }

        public Var new_gvar()
        {
            return null;
        }

        public tpType find_typedef()
        {
            return null;
        }

        public string new_label()
        {
            return null;
        }

        public bool is_function()
        {
            return false;
        }

        public Program program()
        {            
            List<Function> funcs = new List<Function>();
            globals = new List<Var>();

            //while (!tp.scan.at_eof())
            //{
            //}

            return null;
        }

        public tpType basetype()
        {
            return null;
        }

        public tpType declarator()
        {
            return null;
        }

        public tpType abstract_declarator()
        {
            return null;
        }
        
        public tpType type_suffix()
        {
            return null;
        }

        public tpType type_name()
        {
            return null;
        }

        public void push_tag_scope()
        {
        }

        public tpType struct_decl()
        {
            return null;
        }

        public bool consume_end()
        {
            return false;
        }

        public bool peek_end()
        {
            return false;
        }

        public void expect_end()
        {
        }

        public tpType enum_specifier()
        {
            return null;
        }

        public Member struct_member()
        {
            return null;
        }

        public List<Var> read_func_param()
        {
            return null;
        }

        public void read_func_params()
        {
        }

        public Function function()
        {
            return null;
        }

        public Initializer new_init_val()
        {
            return null;
        }

        public Initializer new_init_label()
        {
            return null;
        }

        public Initializer new_init_zero()
        {
            return null;
        }

        public Initializer gvar_init_string()
        {
            return null;
        }

        public Initializer emit_struct_padding()
        {
            return null;
        }

        public void skip_excess_elements2()
        {
        }

        public void skip_excess_elements()
        {
        }

        public Initializer gvar_initializer2()
        {
            return null;
        }

        public Initializer gvar_initializer()
        {
            return null;
        }

        public void global_var()
        {
        }

        public Node new_desg_node2()
        {
            return null;
        }

        public Node new_desg_node()
        {
            return null;
        }

        public Node lvar_init_zero()
        {
            return null;
        }

        public Node lvar_initializer2()
        {
            return null;
        }

        public Node lvar_initializer()
        {
            return null;
        }

        public Node declaration()
        {
            return null;
        }

        public Node read_expr_stmt()
        {
            return null;
        }

        public bool is_typename()
        {
            return false;
        }

        public Node stmt()
        {
            return null;
        }

        public Node stmt2()
        {
            return null;
        }

        public Node expr()
        {
            return null;
        }

        public long eval()
        {
            return 0;
        }

        public long eval2()
        {
            return 0;
        }

        public long const_expr()
        {
            return 0;
        }

        public Node assign()
        {
            return null;
        }

        public Node conditional()
        {
            return null;
        }

        public Node logor()
        {
            return null;
        }

        public Node logand()
        {
            return null;
        }

        public Node bitor()
        {
            return null;
        }

        public Node bitxor()
        {
            return null;
        }

        public Node bitand()
        {
            return null;
        }

        public Node equality()
        {
            return null;
        }

        public Node relational()
        {
            return null;
        }

        public Node shift()
        {
            return null;
        }

        public Node new_add()
        {
            return null;
        }

        public Node new_sub()
        {
            return null;
        }

        public Node add()
        {
            return null;
        }

        public Node mul()
        {
            return null;
        }

        public Node cast()
        {
            return null;
        }

        public Node unary()
        {
            return null;
        }

        public Member find_member()
        {
            return null;
        }

        public Node struct_ref()
        {
            return null;
        }

        public Node postfix()
        {
            return null;
        }

        public Node compound_literal()
        {
            return null;
        }

        public Node stmt_expr()
        {
            return null;
        }

        public Node func_args()
        {
            return null;
        }

        public Node primary()
        {
            return null;
        }        
    }

    //---------------------------------------------------------------

    public class Node
    {
    }

    //---------------------------------------------------------------

    public class Var 
    {
    }

    public class Initializer
    {
    }

    public class Function
    {
        public String name;
        public List<Var> parameters;
        public bool is_static;
        public bool has_varargs;

        public Node node;
        public List<Var> locals;
        public int stack_size;
    }

    public class Program
    {
        public List<Var> globals;
        public List<Function> fns;
    }

    public class VarScope
    {
    }

    public class TagScope
    {
    }

    public class Scope
    {
    }
}
