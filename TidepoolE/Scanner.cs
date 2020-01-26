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
    class Scanner
    {
        TidePool tp;

        public string user_input;
        public List<Token> tokens;
        public int curToken;

        public Scanner(TidePool _tp)
        {
            tp = _tp;

            user_input = "";
        }

        public void error(string format, params object[] args)
        {
            Console.Out.WriteLine(string.Format(format, args));
            System.Environment.Exit(1);
        }

        public void verror_at()
        {
        }

        public void error_at(int loc, string fmt)
        {
        }

        public void error_tok(Token tok, string fmt, params object[] args)
        {
        }

        public void warn_tok()
        {
        }

        //- token handling ------------------------------------------

        //get the current token
        public Token token()
        {
            return tokens[curToken];
        }

        public int mark()
        {
            return curToken;
        }

        //rewind the pos in the token list
        public void reset(int pos)
        {
            curToken = pos;
        }

        // Consumes the current token if it matches `op`.
        public Token consume(string op)
        {
            if (token().kind != TokenKind.TK_RESERVED || op.Length != token().str.Length || (token().str.Equals(op)))
                return null;
            Token t = token();
            curToken++;
            return t;
        }

        // Returns true if the current token matches a given string.
        public Token peek(string s)
        {
            if (token().kind != TokenKind.TK_RESERVED || s.Length != token().str.Length || token().str.Equals(s))
                return null;
            return token();
        }

        // Consumes the current token if it is an identifier.
        public Token consume_ident()
        {
            if (token().kind != TokenKind.TK_IDENT)
                return null;
            Token t = token();
            curToken++;
            return t;
        }

        // Ensure that the current token is a given string
        public void expect(string s)
        {
            if (peek(s) == null)
                error_tok(token(), "expected \"%s\"", s);
            curToken++;
        }

        public string expect_ident()
        {
            return "";
        }

        public bool at_eof()
        {
            return (curToken < tokens.Count) && (tokens[curToken].kind == TokenKind.TK_EOF);
        }

        public Token new_token(TokenKind kind, List<Token> tokens, int start, int len)
        {
            Token tok = new Token();
            tok.kind = kind;
            tok.str = (len > 0) ? user_input.Substring(start, len) : "";
            tokens.Add(tok);
            return tok;
        }

        //- string handling -----------------------------------------

        public bool startswith(int p, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (user_input[p++] != str[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int strstr(int p, string str)
        {
            return user_input.IndexOf(str, p);
        }

        public bool is_space(char ch)
        {
            return (ch == ' ' | ch == '\n' | ch == '\t' | ch == '\v' | ch == '\f' | ch == '\r');
        }

        public bool is_alpha(char ch)
        {
            return (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ch == '_');
        }

        public bool is_alnum(char ch)
        {
            return (is_alpha(ch) || ('0' <= ch && ch <= '9'));
        }

        public bool isdigit(char ch)
        {
            return ('0' <= ch && ch <= '9');
        }

        public bool ishexdigit(char ch)
        {
            return ('0' <= ch && ch <= '9') || ('a' <= ch && ch <= 'f') || ('A' <= ch && ch <= 'F');
        }

        public bool is_punct(char ch)
        {
            return (!((ch == ' ') || ('0' <= ch && ch <= '9') || ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z')));
        }

        string[] kw = new string[]{"return", "if", "else", "while", "for", "int", "char", "sizeof", "struct", "typedef", "short",
                       "long", "void", "_Bool", "enum", "static", "break", "continue", "goto", "switch", "case", "default",
                       "extern", "_Alignof", "do", "signed"};

        string[] ops = new string[]{"<<=", ">>=", "...", "==", "!=", "<=", ">=", "->", "++", "--", "<<", ">>", "+=", "-=", "*=",
                        "/=", "&&", "||", "&=", "|=", "^="};

        public string starts_with_reserved(int p)
        {
            //keywords
            for (int i = 0; i < kw.Length; i++)
            {
                int len = kw[i].Length;
                if (startswith(p, kw[i]) && !is_alnum(user_input[p + len]))
                {
                    return kw[i];
                }
            }

            // Multi-letter punctuator
            for (int i = 0; i < ops.Length; i++)
            {
                if (startswith(p, ops[i]))
                {
                    return ops[i];
                }
            }

            return null;
        }

        public char get_escape_char(char ch)
        {
            switch (ch)
            {
                case 'a': return '\a';
                case 'b': return '\b';
                case 't': return '\t';
                case 'n': return '\n';
                case 'v': return '\v';
                case 'f': return '\f';
                case 'r': return '\r';
                case 'e': return '\u001b';
                case '0': return '\0';
                default: return ch;
            }
        }

        public Token read_string_literal(List<Token> cur, int start)
        {
            return null;
        }

        public Token read_char_literal(List<Token> cur, int start)
        {
            return null;
        }

        public Token read_int_literal(List<Token> cur, int p)
        {
            int start = p;

            //determine bass
            int bass;
            if (user_input[p] == '0' && (user_input[p + 1] == 'x' | user_input[p + 1] == 'X') && is_alnum(user_input[p + 2]))
            {
                p += 2;
                bass = 16;
            }
            else if (user_input[p] == '0' && (user_input[p + 1] == 'b' | user_input[p + 1] == 'B') && is_alnum(user_input[p + 2]))
            {
                p += 2;
                bass = 2;
            }
            else if (user_input[p] == '0')
            {
                bass = 8;
            }
            else
            {
                bass = 10;
            }

            //get num str
            int q = p;
            string numstr = "";
            while ((bass == 16 && ishexdigit(user_input[p])) ||
                   (bass == 10 && isdigit(user_input[p])) ||
                   (bass == 8 && ('0' <= user_input[p] && user_input[p] <= '9')) ||
                   (bass == 2 && ('0' <= user_input[p] && user_input[p] <= '1')))
            {
                numstr += user_input[p];
                p++;
            }

            //convert num str to val
            long val = Convert.ToInt64(numstr, bass);
            tpType ty = Analyzer.int_type;

            // Read L or LL prefix or infer a type.
            if (startswith(p, "LL") || startswith(p, "ll"))
            {
                p += 2;
                ty = Analyzer.long_type;
            }
            else if (user_input[p] == 'L' || user_input[p] == 'l')
            {
                p++;
                ty = Analyzer.long_type;
            }
            else if (val != (int)val)
            {
                ty = Analyzer.long_type;
            }

            if (is_alnum(user_input[p]))
                error_at(p, "invalid digit");

            //tokenize
            Token tok = new_token(TokenKind.TK_NUM, cur, start, p - start);
            tok.val = val;
            tok.ty = ty;
            return tok;
        }

        public void tokenize()
        {
            tokens = new List<Token>();
            Token cur = null;
            int p = 0;

            while (user_input[p] != '\0')
            {
                //skip whitespace
                if (is_space(user_input[p]))
                {
                    p++;
                    continue;
                }

                //skip line comments
                if (startswith(p, "//"))
                {
                    p += 2;
                    while (user_input[p] != '\n')
                    {
                        p++;
                    }
                    continue;
                }

                //skip block comments
                if (startswith(p, "/*"))
                {
                    int q = strstr(p + 2, "*/");
                    if (q == -1)
                    {
                        error_at(p, "unclosed block comment");
                    }
                    p = q + 2;
                    continue;
                }

                //string literal
                if (user_input[p] == '"')
                {
                    cur = read_string_literal(tokens, p);
                    p += cur.str.Length;
                    continue;
                }

                //char literal
                if (user_input[p] == '\'')
                {
                    cur = read_char_literal(tokens, p);
                    p += cur.str.Length;
                    continue;
                }

                //keywords 
                string kw = starts_with_reserved(p);
                if (kw != null)
                {
                    int len = kw.Length;
                    cur = new_token(TokenKind.TK_RESERVED, tokens, p, len);
                    p += len;
                    continue;
                }

                //identifier
                if (is_alpha(user_input[p]))
                {
                    int q = p;
                    while (is_alnum(user_input[p]))
                    {
                        p++;
                    }
                    cur = new_token(TokenKind.TK_IDENT, tokens, q, p - q);
                    continue;
                }

                // Single-letter punctuators
                if (is_punct(user_input[p]))
                {
                    cur = new_token(TokenKind.TK_RESERVED, tokens, p++, 1);
                    continue;
                }

                // Integer literal
                if (isdigit(user_input[p]))
                {
                    cur = read_int_literal(tokens, p);
                    p += cur.str.Length;
                    continue;
                }

                error_at(p, "invalid token");
            }

            new_token(TokenKind.TK_EOF, tokens, p, 0);
        }
    }

    public enum TokenKind
    {
        TK_RESERVED,        // Keywords or punctuators
        TK_IDENT,           // Identifiers
        TK_STR,             // String literals
        TK_NUM,             // Integer literals
        TK_EOF,             // End-of-file markers
    }

    public class Token
    {
        public TokenKind kind;
        public long val;
        public tpType ty;
        public string str;
        public string contents;
    }
}
