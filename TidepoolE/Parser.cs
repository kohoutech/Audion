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

        // All local variable instances created during parsing are accumulated to this list.
        List<Var> locals;

        // Likewise, global variables are accumulated to this list.
        List<Var> globals;

        // C has two block scopes; one is for variables/typedefs and the other is for struct/union/enum tags.
        List<VarScope> var_scope;
        List<TagScope> tag_scope;
        int scope_depth;

        Node current_switch;

        public Parser(TidePool _tp)
        {
            tp = _tp;
            locals = new List<Var>();
            globals = new List<Var>();
            var_scope = new List<VarScope>();
            tag_scope = new List<TagScope>();
            scope_depth = 0;
        }

        public Scope enter_scope()
        {
            Scope sc = new Scope();
            sc.var_scope = var_scope.Count;
            sc.tag_scope = tag_scope.Count;
            scope_depth++;
            return sc;
        }

        public void leave_scope(Scope sc)
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

        public VarScope push_scope(string name)
        {
            VarScope sc = new VarScope();
            sc.name = name;
            sc.depth = scope_depth;
            var_scope.Add(sc);
            return sc;
        }

        public Var new_var(string name, tpType ty, bool is_local)
        {
            Var var = new Var();
            var.name = name;
            var.ty = ty;
            var.is_local = is_local;
            return var;
        }

        public Var new_lvar()
        {
            return null;
        }

        public Var new_gvar(string name, tpType ty, bool is_static, bool emit)
        {
            Var var = new_var(name, ty, false);
            var.is_static = is_static;
            push_scope(name).var = var;

            if (emit)
            {
                globals.Add(var);
            }

            return var;
        }

        public tpType find_typedef(Token tok)
        {
            return null;
        }

        public string new_label()
        {
            return null;
        }

        public bool is_function()
        {
            int tokpos = tp.scan.mark();
            bool isfunc = false;

            StorageClass sclass = StorageClass.NONE;
            tpType ty = basetype(ref sclass);

            if (tp.scan.consume(";") == null)
            {
                string name = null;
                declarator(ty, ref name);
                isfunc = (name != null) && (tp.scan.consume("(") != null);
            }

            tp.scan.reset(tokpos);

            return isfunc;
        }

        // program = (global-var | function)*
        public Program program()
        {
            List<Function> funcs = new List<Function>();
            globals = new List<Var>();
            tp.scan.reset(0);

            while (!tp.scan.at_eof())
            {
                if (is_function())
                {
                    Function fn = function();
                    if (fn == null)
                        continue;
                    funcs.Add(fn);
                }

                global_var();
            }

            Program prog = new Program();
            prog.globals = globals;
            prog.fns = funcs;
            return prog;
        }

        public tpType basetype(ref StorageClass sclass)
        {
            if (!is_typename())
                tp.scan.error_tok(tp.scan.token(), "typename expected");

            tpType ty = Analyzer.int_type;
            BaseType counter = 0;
            bool seenLong = false;

            if (sclass != StorageClass.NOTALLOWED)
            {
                sclass = StorageClass.NONE;
            }

            while (is_typename())
            {
                Token tok = tp.scan.token();

                // Handle storage class specifiers.
                if (tp.scan.peek("typedef") != null || tp.scan.peek("static") != null || tp.scan.peek("extern") != null)
                {
                    if (sclass == StorageClass.NOTALLOWED)
                        tp.scan.error_tok(tok, "storage class specifier is not allowed");

                    if (tp.scan.consume("typedef") != null)
                        sclass |= StorageClass.TYPEDEF;
                    else if (tp.scan.consume("static") != null)
                        sclass |= StorageClass.STATIC;
                    else if (tp.scan.consume("extern") != null)
                        sclass |= StorageClass.EXTERN;

                    if ((sclass & (sclass - 1)) != 0)
                        tp.scan.error_tok(tok, "typedef, static and extern may not be used together");

                    continue;
                }

                  // Handle user-defined types.
                  if (tp.scan.peek("void") == null && tp.scan.peek("_Bool") == null && tp.scan.peek("char") == null &&
                      tp.scan.peek("short") == null && tp.scan.peek("int") == null && tp.scan.peek("long") == null &&
                      tp.scan.peek("signed") == null) {
                    if (counter != 0)
                      break;

                    if (tp.scan.peek("struct") != null) {
                      ty = struct_decl();
                    } else if (tp.scan.peek("enum") != null) {
                      ty = enum_specifier();
                    } else {
                      ty = find_typedef(tp.scan.token());
                      if (ty == null)
                          tp.scan.error("unknown type name", tp.scan.token().str);
                      tp.scan.nextToken();
                    }

                    counter |= BaseType.OTHER;
                    continue;
                  }

                  // Handle built-in types.
                  if (tp.scan.consume("void") != null)
                      counter |= BaseType.VOID;
                  else if (tp.scan.consume("_Bool") != null)
                      counter |= BaseType.BOOL;
                  else if (tp.scan.consume("char") != null)
                      counter |= BaseType.CHAR;
                  else if (tp.scan.consume("short") != null)
                      counter |= BaseType.SHORT;
                  else if (tp.scan.consume("int") != null)
                      counter |= BaseType.INT;
                  else if (tp.scan.consume("long") != null)
                  {
                      if (seenLong)
                      {
                          counter |= BaseType.LLONG;
                      }
                      else
                      {
                          counter |= BaseType.LONG;
                          seenLong = true;
                      }
                  }
                  else if (tp.scan.consume("signed") != null)
                      counter |= BaseType.SIGNED;

                  switch (counter)
                  {
                      case BaseType.VOID:
                          ty = Analyzer.void_type;
                          break;
                      case BaseType.BOOL:
                          ty = Analyzer.bool_type;
                          break;
                      case BaseType.CHAR:
                      case BaseType.SIGNED | BaseType.CHAR:
                          ty = Analyzer.char_type;
                          break;
                      case BaseType.SHORT:
                      case BaseType.SHORT | BaseType.INT:
                      case BaseType.SIGNED | BaseType.SHORT:
                      case BaseType.SIGNED | BaseType.SHORT | BaseType.INT:
                          ty = Analyzer.short_type;
                          break;
                      case BaseType.INT:
                      case BaseType.SIGNED:
                      case BaseType.SIGNED | BaseType.INT:
                          ty = Analyzer.int_type;
                          break;
                      case BaseType.LONG:
                      case BaseType.LONG | BaseType.INT:
                      case BaseType.LLONG:
                      case BaseType.LLONG | BaseType.INT:
                      case BaseType.SIGNED | BaseType.LONG:
                      case BaseType.SIGNED | BaseType.LONG | BaseType.INT:
                      case BaseType.SIGNED | BaseType.LLONG:
                      case BaseType.SIGNED | BaseType.LLONG | BaseType.INT:
                          ty = Analyzer.long_type;
                          break;
                      default:
                          tp.scan.error_tok(tok, "invalid type");
                          break;
                  }
            }

            return ty;
        }

        public tpType declarator(tpType ty, ref string name)
        {
            while (tp.scan.consume("*") != null)
                ty = tp.ana.pointer_to(ty);

            if (tp.scan.consume("(") != null)
            {
                tpType placeholder = new tpType();
                tpType new_ty = declarator(placeholder, ref name);
                tp.scan.expect(")");
                placeholder.copy(type_suffix(ty));
                return new_ty;
            }

            name = tp.scan.expect_ident();
            return type_suffix(ty);
        }

        public tpType abstract_declarator()
        {
            return null;
        }

        public tpType type_suffix(tpType ty)
        {
            return ty;
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

        public Var read_func_param()
        {
            return null;
        }

        public void read_func_params(Function fn)
        {
            if (tp.scan.consume(")") != null)       //empty param list
                return;

            int tokpos = tp.scan.mark();
            if ((tp.scan.consume("void") != null) && (tp.scan.consume(")") != null))    //(void) param list
                return;

            tp.scan.reset(tokpos);                      //have param list, reset token to start of list
            fn.parameters.Add(read_func_param());       //first param in list

            while (tp.scan.consume(")") == null)
            {
                tp.scan.expect(",");

                if (tp.scan.consume("...") != null)
                {
                    fn.has_varargs = true;
                    tp.scan.expect(")");
                    return;
                }

                fn.parameters.Add(read_func_param());
            }
        }

        public Function function()
        {
            locals = null;

            StorageClass sclass = StorageClass.NONE;
            tpType ty = basetype(ref sclass);
            string name = null;
            ty = declarator(ty, ref name);

            // Add a function type to the scope
            new_gvar(name, tp.ana.func_type(ty), false, false);

            // Construct a function object
            Function fn = new Function();
            fn.name = name;
            fn.is_static = (sclass == StorageClass.STATIC);
            tp.scan.expect("(");

            Scope sc = enter_scope();
            read_func_params(fn);

            if (tp.scan.consume(";") != null)
            {
                leave_scope(sc);
                return null;
            }

            // Read function body
            List<Node> body = new List<Node>();
            tp.scan.expect("{");
            while (tp.scan.consume("}") == null)
            {
                body.Add(stmt());                
            }
            leave_scope(sc);

            fn.node = body;
            fn.locals = locals;
            return fn;
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
            return (tp.scan.peek("void") != null || tp.scan.peek("_Bool") != null || tp.scan.peek("char") != null ||
                    tp.scan.peek("short") != null || tp.scan.peek("int") != null || tp.scan.peek("long") != null ||
                    tp.scan.peek("enum") != null || tp.scan.peek("struct") != null || tp.scan.peek("typedef") != null ||
                    tp.scan.peek("static") != null || tp.scan.peek("extern") != null || tp.scan.peek("signed") != null ||
                    find_typedef(tp.scan.token()) != null);
        }

        public Node stmt()
        {
            Node node = stmt2();
            tp.ana.add_type(node);
            return node;
        }

        public Node stmt2()
        {
            Token tok;

            if ((tok = tp.scan.consume("return")) != null)
            {
            //    if (consume(";"))
            //        return new_node(ND_RETURN, tok);

            //    Node* node = new_unary(ND_RETURN, expr(), tok);
            //    expect(";");
            //    return node;
            }

            if ((tok = tp.scan.consume("if")) != null)
            {
            //    Node* node = new_node(ND_IF, tok);
            //    expect("(");
            //    node->cond = expr();
            //    expect(")");
            //    node->then = stmt();
            //    if (consume("else"))
            //        node->els = stmt();
            //    return node;
            }

            if ((tok = tp.scan.consume("switch")) != null)
            {
            //    Node* node = new_node(ND_SWITCH, tok);
            //    expect("(");
            //    node->cond = expr();
            //    expect(")");

            //    Node* sw = current_switch;
            //    current_switch = node;
            //    node->then = stmt();
            //    current_switch = sw;
            //    return node;
            }

            if ((tok = tp.scan.consume("case")) != null)
            {
            //    if (!current_switch)
            //        error_tok(tok, "stray case");
            //    int val = const_expr();
            //    expect(":");

            //    Node* node = new_unary(ND_CASE, stmt(), tok);
            //    node->val = val;
            //    node->case_next = current_switch->case_next;
            //    current_switch->case_next = node;
            //    return node;
            }

            if ((tok = tp.scan.consume("default")) != null)
            {
            //    if (!current_switch)
            //        error_tok(tok, "stray default");
            //    expect(":");

            //    Node* node = new_unary(ND_CASE, stmt(), tok);
            //    current_switch->default_case = node;
            //    return node;
            }

            if ((tok = tp.scan.consume("while")) != null)
            {
            //    Node* node = new_node(ND_WHILE, tok);
            //    expect("(");
            //    node->cond = expr();
            //    expect(")");
            //    node->then = stmt();
            //    return node;
            }

            if ((tok = tp.scan.consume("for")) != null)
            {
            //    Node* node = new_node(ND_FOR, tok);
            //    expect("(");
            //    Scope* sc = enter_scope();

            //    if (!consume(";"))
            //    {
            //        if (is_typename())
            //        {
            //            node->init = declaration();
            //        }
            //        else
            //        {
            //            node->init = read_expr_stmt();
            //            expect(";");
            //        }
            //    }
            //    if (!consume(";"))
            //    {
            //        node->cond = expr();
            //        expect(";");
            //    }
            //    if (!consume(")"))
            //    {
            //        node->inc = read_expr_stmt();
            //        expect(")");
            //    }
            //    node->then = stmt();

            //    leave_scope(sc);
            //    return node;
            }

            if ((tok = tp.scan.consume("do")) != null)
            {
            //    Node* node = new_node(ND_DO, tok);
            //    node->then = stmt();
            //    expect("while");
            //    expect("(");
            //    node->cond = expr();
            //    expect(")");
            //    expect(";");
            //    return node;
            }

            if ((tok = tp.scan.consume("{")) != null)
            {
            //    Node head = { };
            //    Node* cur = &head;

            //    Scope* sc = enter_scope();
            //    while (!consume("}"))
            //    {
            //        cur->next = stmt();
            //        cur = cur->next;
            //    }
            //    leave_scope(sc);

            //    Node* node = new_node(ND_BLOCK, tok);
            //    node->body = head.next;
            //    return node;
            }

            if ((tok = tp.scan.consume("break")) != null)
            {
            //    expect(";");
            //    return new_node(ND_BREAK, tok);
            }

            if ((tok = tp.scan.consume("continue")) != null)
            {
            //    expect(";");
            //    return new_node(ND_CONTINUE, tok);
            }

            if ((tok = tp.scan.consume("goto")) != null)
            {
            //    Node* node = new_node(ND_GOTO, tok);
            //    node->label_name = expect_ident();
            //    expect(";");
            //    return node;
            }

            if ((tok = tp.scan.consume(";")) != null)
                return new_node(ND_NULL, tok);

            if ((tok = tp.scan.consume_ident()) != null)
            {
            //    if (consume(":"))
            //    {
            //        Node* node = new_unary(ND_LABEL, stmt(), tok);
            //        node->label_name = strndup(tok->str, tok->len);
            //        return node;
            //    }
            //    token = tok;
            }

            if (is_typename())
                return declaration();

            Node node = read_expr_stmt();
            tp.scan.expect(";");
            return node;
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
        public string name;        // Variable name
        public tpType ty;          // Type
        public bool is_local;  // local or global

        // Local variable
        public int offset;    // Offset from RBP

        // Global variable
        public bool is_static;
        public Initializer initializer;
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

        public List<Node> node;
        public List<Var> locals;
        public int stack_size;

        public Function()
        {
            name = "";
            parameters = new List<Var>();
            is_static = false;
            has_varargs = false;

            node = null;
            locals = new List<Var>();
            stack_size = 0;
        }
    }

    public class Program
    {
        public List<Var> globals;
        public List<Function> fns;
    }

    // Scope for local variables, global variables, typedefs or enum constants
    public class VarScope
    {
        public VarScope next;
        public string name;
        public int depth;

        public Var var;
        public tpType type_def;
        public tpType enum_ty;
        public int enum_val;
    }

    public class TagScope
    {
    }

    public class Scope
    {
        public int var_scope;
        public int tag_scope;
    }

    [Flags]
    public enum BaseType
    {
        VOID = 1 << 0,
        BOOL = 1 << 2,
        CHAR = 1 << 4,
        SHORT = 1 << 6,
        INT = 1 << 8,
        LONG = 1 << 10,
        LLONG = 1 << 11,
        OTHER = 1 << 12,
        SIGNED = 1 << 13,
    };

    [Flags]
    public enum StorageClass
    {
        NONE = 0,
        TYPEDEF = 1 << 0,
        STATIC = 1 << 1,
        EXTERN = 1 << 2,
        NOTALLOWED = 1 << 4,
    }
}
