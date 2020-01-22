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
        public Token tok;               //the current token
        public CValue tokc;             //the current const value
        public String tokcstr;                 //const string

        public bool at_line_start;

        public List<TokenSym> table_ident;
        public Dictionary<String, TokenSym> hash_ident;

        public int[] isidnum_table;

        public Dictionary<String, TokenType> tp_keywords;

        //cons
        public Preprocessor(Tidepool _tp)
        {
            tp = _tp;
            file = null;
            tok = null;
            tokc = null;
            tokcstr = "";

            at_line_start = true;

            table_ident = new List<TokenSym>();
            hash_ident = new Dictionary<string, TokenSym>();

            // init isid table */
            isidnum_table = new int[256];
            for (int i = 0; i < 128; i++)
            {
                set_idnum(i, is_space(i) ? IS_SPC : isid(i) ? IS_ID : isnum(i) ? IS_NUM : 0);
            }

            for (int i = 128; i < 256; i++)
            {
                set_idnum(i, IS_ID);
            }

            //init keyword dict
            tp_keywords = new Dictionary<string, TokenType>() { 
                { "auto", TokenType.AUTO },
                { "BREAK", TokenType.BREAK },
                { "case", TokenType.CASE},
                { "char", TokenType.CHAR},
                { "const", TokenType.CONST},
                { "continue", TokenType.CONTINUE},
                { "default", TokenType.DEFAULT},
                { "do", TokenType.DO},
                { "double", TokenType.DOUBLE},
                { "else", TokenType.ELSE},
                { "enum", TokenType.ENUM},
                { "extern", TokenType.EXTERN},
                { "float", TokenType.FLOAT},
                { "for", TokenType.FOR},
                { "goto", TokenType.GOTO},
                { "if", TokenType.IF},
                { "inline", TokenType.INLINE},
                { "int", TokenType.INT},
                { "long", TokenType.LONG},
                { "register", TokenType.REGISTER},
                { "restrict", TokenType.RESTRICT},
                { "return", TokenType.RETURN},
                { "short", TokenType.SHORT},
                { "signed", TokenType.SIGNED},
                { "sizeof", TokenType.SIZEOF},
                { "static", TokenType.STATIC},
                { "struct", TokenType.STRUCT},
                { "switch", TokenType.SWITCH},
                { "typedef", TokenType.TYPEDEF},
                { "union", TokenType.UNION},
                { "unsigned", TokenType.UNSIGNED},
                { "void", TokenType.VOID},
                { "volatile", TokenType.VOLATILE},
                { "while", TokenType.WHILE},
           
                    //preprocessing key words
                { "ifdef", TokenType.IFDEF},
                { "ifndef", TokenType.IFNDEF},
                { "elif", TokenType.ELIF},
                { "endif", TokenType.ENDIF},
                { "include", TokenType.INCLUDE},
                { "define", TokenType.DEFINE},
                { "undef", TokenType.UNDEF},
                { "line", TokenType.LINE},
                { "error", TokenType.ERROR},
                { "pragma", TokenType.PRAGMA}};
        }

        //* isidnum_table flags: */
        const int IS_SPC = 1;
        const int IS_ID = 2;
        const int IS_NUM = 4;

        // space excluding newline */
        public bool is_space(int ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\v' || ch == '\f' || ch == '\r';
        }

        public bool isid(int c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        public bool isnum(int c)
        {
            return c >= '0' && c <= '9';
        }

        public bool isoct(int c)
        {
            return c >= '0' && c <= '7';
        }

        public char toup(char c)
        {
            return (c >= 'a' && c <= 'z') ? (char)((int)c - (int)'a' + (int)'A') : c;
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
            ts.tok = table_ident.Count;             //set token num
            table_ident.Add(ts);
            hash_ident.Add(str, ts);
            return ts;
        }

        public TokenSym tok_alloc(String str)
        {
            if (hash_ident.ContainsKey(str)) {
                return hash_ident[str];
            }
            return tok_alloc_new(str);
        }

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
        
        //line comments
        public int parse_line_comment(int p)
        {
            char c;
            p++;
            while(true)
            {
                c = file.buffer[p];
                if (c == '\n' || c == '\0')
                {
                    break;
                }
                else
                {
                    p++;
                }
            }
            return p;
        }

        //block comments /* ... */
        public int parse_comment(int p)
        {
            char c;
            p++;
            bool end_of_comment = false;
            while (!end_of_comment)
            {
                while (true)        //scan for eoln or end of comment
                {
                    c = file.buffer[p];
                    if (c == '\n' || c == '*' || c == '\\')
                        break;
                    p++;
                }

                //we seen either eoln or possible end of comment
                if (c == '\n')
                {
                    file.line_num++;        //eoln - keep scanning
                    p++;
                }
                else if (c == '*')          //possible end of comment
                {
                    p++;
                    while (!end_of_comment)
                    {
                        c = file.buffer[p];
                        if (c == '*')               //if we see a row of asterisks, ie ****, treat the last one as the possible start of */
                        {
                            p++;
                        }
                        else if (c == '/')
                        {
                            end_of_comment = true;
                        }
                        else
                        {
                            break;          //wasn't the end of comment
                        }
                    }
                }
            }
            p++;
            return p;
        }

        public int set_idnum(int c, int val)
        {
            int prev = isidnum_table[c];
            isidnum_table[c] = val;
            return prev;
        }

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

        public void define_undef(Sym s)
        {
        }

        public Sym define_find(Token v)
        {
            return null;
        }

        //free_defines
        //label_find
        //label_push
        //label_pop
        //maybe_run_test
        //expr_preprocess

        public void parse_define()
        {
        }

        //search_cached_include
        //pragma_parse

        public void preprocess(bool is_bof)
        {
            	next_nomacro();
                switch (tok.type)
                {
                    case TokenType.DEFINE:
                        next_nomacro();             //get define ident
                        parse_define();
                        break;

                    case TokenType.UNDEF:
                        next_nomacro();             //get undefine ident
                        Sym s = define_find(tok);
                        if (s != null)              // undefine symbol by putting an invalid name */
                            define_undef(s);
                        break;

                    case TokenType.INCLUDE:
                        break;

                    case TokenType.IFNDEF:
                        break;

                    case TokenType.IF:
                        break;

                    case TokenType.IFDEF:
                        break;

                    case TokenType.ELSE:
                        break;

                    case TokenType.ELIF:
                        break;

                    case TokenType.ENDIF:
                        break;

                    case TokenType.PPNUM:
                        break;

                    case TokenType.LINE:
                        break;

                    case TokenType.ERROR:
                        break;

                    case TokenType.PRAGMA:
                        break;

                    default:
                        break;
                }

        }

        //parse_escape_string

        public void parse_string(String s)
        {
        }

        //bn_lshift
        //bn_zero

        public void parse_number(String s)
        {
            ulong bass = 10;
            ulong val = 0;
            int pos = 0;
            while (pos < s.Length)
            {
                char c = s[pos++];
                val = (val * bass) + (ulong)(c - '0');
            } 

            tok = new Token(TokenType.INTCONST);
            tokc = new CValue();
            tokc.i = val;
        }

        public void next_nomacro1()
        {
            int p = file.buf_ptr;
            char c = file.buffer[p];
            bool found;
            do
            {
                found = true;            //assume we'll find a token in this pass
                switch (c)
                {
                    case ' ':
                    case '\t':
                        p++;
                        c = file.buffer[p];
                        while ((isidnum_table[c] & IS_SPC) != 0)
                        {
                            p++;
                            c = file.buffer[p];
                        }
                        found = false;
                        break;

                    case '\f':
                    case '\v':
                    case '\r':
                        p++;
                        c = file.buffer[p];
                        found = false;
                        break;

                    case '\n':
                        file.line_num++;
                        at_line_start = true;
                        p++;
                        c = file.buffer[p];
                        found = false;
                        break;

                    case '#':
                        p++;
                        if (at_line_start)          //we have a preprocessor directive
                        {
                            file.buf_ptr = p;
                            preprocess(at_line_start);
                            p = file.buf_ptr;
                            c = file.buffer[p];
                            found = false;
                        }
                        else
                        {
                            c = file.buffer[p];
                            if (c == '#')
                            {
                                p++;
                                tok = new Token(TokenType.SHARPSHARP);
                            }
                            else
                            {
                                tok = new Token(TokenType.SHARP);
                            }
                        }
                        break;

                        //identifier
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
                        {
                            String tokidstr = "";
                            tokidstr += c;
                            p++;
                            c = file.buffer[p];
                            while ((isidnum_table[c] & (IS_ID | IS_NUM)) != 0)
                            {
                                tokidstr += c;
                                p++;
                                c = file.buffer[p];
                            }
                            if (tp_keywords.ContainsKey(tokidstr))
                            {
                                tok = new Token(tp_keywords[tokidstr]);
                            }
                            else
                            {
                                tok = new Token(TokenType.IDENT);
                                TokenSym ts = tok_alloc(tokidstr);       //find prev ident token sym or alloc a new one
                                tok.num = ts.tok;
                            }
                        }
                        break;

                        //number
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
                        {
                            char t = c;
                            tokcstr = "";
                            p++;
                            c = file.buffer[p];
                            while (true)
                            {
                                tokcstr += t;
                                if (!(((isidnum_table[c] & (IS_ID | IS_NUM)) != 0) ||
                                    (c == '.') ||
                                    ((c == '+' || c == '-') && (t == 'e' || t == 'E'))))
                                    break;
                                t = c;
                                p++;
                                c = file.buffer[p];
                            }
                        }
                        tok = new Token(TokenType.PPNUM);
                        break;

                    //comment or operator
                    case '/':
                        p++;
                        c = file.buffer[p];
                        if (c == '*')
                        {
                            p = parse_comment(p);       //block comment
                            c = ' ';                    // comments replaced by a blank */
                            found = false;
                        }
                        else if (c == '/')
                        {
                            p = parse_line_comment(p);
                            c = file.buffer[p];
                            found = false;
                        }
                        else if (c == '=')
                        {
                            p++;
                            tok = new Token(TokenType.SLASHEQU);
                        }
                        else
                        {
                            tok = new Token(TokenType.SLASH);
                        }
                        break;

                    case '(':
                        tok = new Token(TokenType.LPAREN);
                        p++;
                        break;
                    case ')':
                        tok = new Token(TokenType.RPAREN);
                        p++;
                        break;
                    case '{':
                        tok = new Token(TokenType.LBRACE);
                        p++;
                        break;
                    case '}':
                        tok = new Token(TokenType.RBRACE);
                        p++;
                        break;
                    case ';':
                        tok = new Token(TokenType.SEMICOLON);
                        p++;
                        break;

                    case '\0':
                        tok = new Token(TokenType.EOF);
                        break;

                    default:
                        tp.tpError("unrecognized character 0x{0}", ((byte)c).ToString("X02"));
                        break;
                }
            } while (!found);
            at_line_start = false;      //if we've found a token, then we aren't at start of line anymore
            file.buf_ptr = p;
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

            // convert preprocessor tokens into C tokens */
            if (tok.type == TokenType.PPNUM)
            {
                parse_number(tokcstr);
            }
            else if (tok.type == TokenType.PPSTR)
            {
                parse_string(tokcstr);
            }
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
        public int num;                 //sym tbl num if token is ident

        public Token(TokenType _type)
        {
            type = _type;
            num = 0;
        }

        //for debugging
        public void writeOut()
        {
            switch (type)
            {
                case TokenType.IDENT:
                    Console.Out.WriteLine("Identifier - {0}", num);
                    break;
                default:
                    Console.Out.WriteLine(type.ToString());
                    break;
            }
        }
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
        RBRACE,         //}
        DOT,            //.
        ARROW,          //->
        PLUSPLUS,
        MINUSMINUS,
        AMPERSAND,
        STAR,
        PLUS,
        MINUS,
        TILDE,
        EXCLAIM,        //!
        SLASH,
        PERCENT,
        LESSLESS,       //<<
        GTRGTR,         //>>
        LESSTHAN,
        GTRTHAN,
        LESSEQU,
        GTREQU,
        EQUEQU,
        NOTEQU,
        CARET,
        BAR,            //|
        AMPAMP,         //&&
        BARBAR,         //||
        QUESTION,       //?
        COLON,
        SEMICOLON,
        ELIPSIS,        //...
        EQUAL,
        STAREQU,
        SLASHEQU,
        PERCENTEQU,
        PLUSEQU,
        MINUSEQU,
        LESSLESSEQU,
        GTRGTREQU,
        AMPEQU,
        CARETEQU,
        BAREQU,
        COMMA,

        EOF,

        //preprocessing tokens - IF & ELSE already handled above
        //IF,
        IFDEF,
        IFNDEF,
        ELIF,
        //ELSE,
        ENDIF,
        INCLUDE,
        DEFINE,
        UNDEF,
        LINE,
        ERROR,
        PRAGMA,
        PPNUM,
        PPSTR,
        SHARP,
        SHARPSHARP
    }
}
