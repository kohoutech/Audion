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
    public class Preprocessor
    {
        public Tidepool tp;

        public BufferedFile file;
        public Token tok;
        public CValue tokc;

        public List<TokenSym> table_ident;
        public Dictionary<String, TokenSym> hash_ident;

        //debugging
        int idx;
        TokenType[] tokList = { TokenType.INT, TokenType.IDENT, TokenType.LPAREN, TokenType.RPAREN, TokenType.LBRACE,
                                  TokenType.RETURN, TokenType.INTCONST, TokenType.SEMICOLON,
                                  TokenType.RBRACE, TokenType.EOF};

        //cons
        public Preprocessor(Tidepool _tp)
        {
            tp = _tp;
            file = null;
            tok = new Token();
            tokc = new CValue();

            table_ident = new List<TokenSym>();
            hash_ident = new Dictionary<string, TokenSym>();

            idx = 0;
        }

        //---------------------------------------------------------------------

        public void skip(TokenType c)
        {
            if (tok.type != c)
                tp.tpError("'%c' expected (got \"%s\")", c, get_tok_str(tok));
            next();
        }

        public void expect(String msg)
        {
            tp.tpError("%s expected", msg);
        }

        //tal_new
        //tal_delete
        //tal_free_impl
        //tal_realloc_impl

        //cstr_realloc
        //cstr_ccat
        //cstr_cat
        //cstr_wccat
        //cstr_new
        //cstr_free
        //cstr_reset
        //add_char


        public TokenSym tok_alloc_new(String str)
        {
            TokenSym ts = new TokenSym(str);
            ts.tok = table_ident.Count;
            table_ident.Add(ts);
            hash_ident.Add(str, ts);
            return ts;
        }

        //  tok_alloc

        public String get_tok_str(Token tok)
        {
            String s = null;
            if (tok.type == TokenType.IDENT)
            {
                s = table_ident[tok.num].str;
            }
            return s;
        }

        //handle_eob
        //inp
        //handle_stray_noerror
        //handle_stray
        //handle_stray1
        //minp
        //parse_line_comment
        //parse_comment
        //set_idnum
        //skip_spaces
        //check_space
        //parse_pp_string
        //preprocess_skip
        //tok_size
        //tok_str_new
        //tok_str_alloc
        //tok_str_dup
        //tok_str_free_str
        //tok_str_free
        //tok_str_realloc
        //tok_str_add
        //begin_macro
        //end_macro
        //tok_str_add2
        //tok_str_add_tok
        //TOK_GET
        //macro_is_equal
        //define_push
        //define_undef
        //define_find
        //free_defines
        //label_find
        //label_push
        //label_pop
        //maybe_run_test
        //expr_preprocess
        //parse_define
        //search_cached_include
        //pragma_parse
        //preprocess
        //parse_escape_string
        //parse_string
        //bn_lshift
        //bn_zero
        //parse_number
        
        public void next_nomacro1()
        {
            int p = file.buf_ptr;
            char c = file.buffer[p];
            switch (c)
            {
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    break;
                case '(':
                    tok.type = TokenType.LPAREN;
                    p++;
                    break;
                case ')':
                    tok.type = TokenType.RPAREN;
                    p++;
                    break;
                case '{':
                    tok.type = TokenType.LBRACE;
                    p++;
                    break;
                case '}':
                    tok.type = TokenType.RBRACE;
                    p++;
                    break;
                case ';':
                    tok.type = TokenType.SEMICOLON;
                    p++;
                    break;
                default:
                    tp.tpError("unrecognized character {0}", c);
                    break;

            }
        }

        public void next_nomacro_spc()
        {
            next_nomacro1();
        }

        public void next_nomacro()
        {
            //do
            //{
                next_nomacro_spc();
            //} while (tok < 256 && (isidnum_table[tok - CH_EOF] & IS_SPC));
        }

        //macro_arg_subst
        //paste_tokens
        //macro_twosharps
        //next_argstream
        //macro_subst_tok
        //macro_subst

        public void next()
        {
            next_nomacro();

            ////set up for debugging
            //tok.type = tokList[idx];
            //if (idx == 1)               //'main'
            //{
            //    TokenSym ts = tok_alloc_new("main");
            //    tok.num = ts.tok;
            //}
            //if (idx == 6)               //69
            //{
            //    tokc.str = "69";
            //    tokc.i = 69;
            //}
            //idx++;
        }

        //unget_tok
        //preprocess_start
        //preprocess_end
        //tccpp_new
        //tccpp_delete
        //tok_print
        //pp_line
        //define_print
        //pp_debug_defines
        //pp_debug_builtins
        //pp_need_space
        //pp_check_he0xE
        //tcc_preprocess

    }

    //-------------------------------------------------------------------------

    public class BufferedFile
    {
        public const int IO_BUF_SIZE = 8192;

        public int buf_ptr;
        public int buf_end;

        public int line_num;

        public String filename;
        public char[] buffer;
    }

    public class TokenString
    {
    }

    public class Token
    {
        public TokenType type;
        public int num;
    }

    public class TokenSym
    {
        public Sym sym_define;              /* direct pointer to define */
        public Sym sym_label;               /* direct pointer to label */
        public Sym sym_struct;              /* direct pointer to structure */
        public Sym sym_identifier;          /* direct pointer to identifier */

        public int tok;                     /* token number */
        public String str;

        public TokenSym(String _str)
        {
            str = _str;
            tok = 0;
            sym_define = null;
            sym_label = null;
            sym_struct = null;
            sym_identifier = null;
        }
    }

    public class CType              // type definition */
    {
        public ValueType t;
        public Sym reff;
    }

    public class CValue
    {
        public float f;
        public double d;
        public ulong i;
        public String str;
    }

    public enum TokenType
    {
        IDENT,
        INTCONST,

        //keywords
        AUTO,
        BREAK,
        CASE,
        CHAR,
        CONST,
        CONTINUE,
        DEFAULT,
        DO,
        DOUBLE,
        ELSE,
        ENUM,
        EXTERN,
        FLOAT,
        FOR,
        GOTO,
        IF,
        INLINE,
        INT,
        LONG,
        REGISTER,
        RESTRICT,
        RETURN,
        SHORT,
        SIGNED,
        SIZEOF,
        STATIC,
        STRUCT,
        SWITCH,
        TYPEDEF,
        UNION,
        UNSIGNED,
        VOID,
        VOLATILE,
        WHILE,

        //punctuation
        LBRACKET,       //[
        RBRACKET,       //]
        LPAREN,
        RPAREN,
        LBRACE,         //{
        RBRACE,          //}
        DOT,
        ARROW,
        PLUSPLUS,
        MINUSMINUS,
        AMPERSAND,
        STAR,
        PLUS,
        MINUS,
        TIDLE,
        EXCLAIM,
        SLASH,
        PERCENT,
        LESSLESS,
        GTRGTR,
        LESSTHAN,
        GTRTHAN,
        LESSEQU,
        GTREQU,
        EQUEQU,
        NOTEQU,
        CARET,
        BAR,
        AMPAMP,
        BARBAR,
        QUESTION,
        COLON,
        SEMICOLON,
        ELIPSIS,
        COMMA,

        EOF,
        LAST_TOKEN
    }
}
