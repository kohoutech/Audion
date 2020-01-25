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
        public List<Token> token;

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

        public void error_tok()
        {
        }

        public void warn_tok()
        {
        }

        public Token consume()
        {
            return null;
        }

        public Token peek()
        {
            return null;
        }

        public Token consume_ident()
        {
            return null;
        }

        public void expect()
        {
        }

        public string expect_ident()
        {
            return "";
        }

        public bool at_eof()
        {
            return false;
        }

        public Token new_token()
        {
            return null;
        }

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

        public bool is_alpha()
        {
            return false;
        }

        public bool is_alnum()
        {
            return false;
        }

        public string starts_with_reserved(int p)
        {
            return "";
        }

        public char get_escape_char()
        {
            return '\0';
        }

        public Token read_string_literal(List<Token> cur, int start)
        {
            return null;
        }

        public Token read_char_literal(List<Token> cur, int start)
        {
            return null;
        }

        public Token read_int_literal()
        {
            return null;
        }

        public List<Token> tokenize()
        {
            List<Token> tokens = new List<Token>();
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
                    continue;
                }

            }


            return tokens;
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
