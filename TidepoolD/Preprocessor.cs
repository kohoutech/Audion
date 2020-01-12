/* ----------------------------------------------------------------------------
Tidepool : a C compiler
 
based on Fabrice Bellard's Tiny C compiler

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

namespace TidepoolD
{
    public class Preprocessor
    {
        public Tidepool tp;
        public BufferedFile file;
        public Token tok;
        public CValue tokc;

        //debugging
        int idx;
        TokenType[] tokList = { TokenType.INT, TokenType.IDENT, TokenType.LPAREN, TokenType.RPAREN, TokenType.LBRACE,
                                  TokenType.RETURN, TokenType.INTCONST, TokenType.SEMICOLON,
                                  TokenType.RBRACE, TokenType.EOF};

        //cons
        public Preprocessor(Tidepool _tp)
        {
            tp = _tp;
            tok = new Token();
            tokc = new CValue();
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

        public String get_tok_str(Token tok)
        {
            return "token";
        }

        public void next()
        {
            //set up for debugging
            tok.type = tokList[idx];
            if (idx == 1)
            {
                tok.num = 100;
            }
            if (idx == 6)
            {
                tokc.str = "69";
                tokc.i = 69;
            }
            idx++;
        }
    }

    //-------------------------------------------------------------------------

    public class BufferedFile
    {
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
        public String str;
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
