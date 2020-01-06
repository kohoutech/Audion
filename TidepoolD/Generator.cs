using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolD
{
    class Generator
    {
        public Tidepool tp;
        public Preprocessor pp;

        public int tpgen_compile()
        {
            return 0;
        }

        public void gexpr() 
        {
        }

        public void block()
        {
            if (pp.tok.type == TokenType.IF)
            {
            }
            else if (tp.pp.tok.type == TokenType.WHILE)
            {
            }
            else if (tp.pp.tok.type == TokenType.LBRACE)
            {
            }
            else if (tp.pp.tok.type == TokenType.RETURN)
            {
            }
            else if (tp.pp.tok.type == TokenType.BREAK)
            {
            }
            else if (tp.pp.tok.type == TokenType.CONTINUE)
            {
            }
            else if (tp.pp.tok.type == TokenType.FOR)
            {
            }
            else if (tp.pp.tok.type == TokenType.DO)
            {
            }
            else if (tp.pp.tok.type == TokenType.SWITCH)
            {
            }
            if (tp.pp.tok.type == TokenType.CASE)
            {
            }
            else if (tp.pp.tok.type == TokenType.DEFAULT)
            {
            }
            else if (tp.pp.tok.type == TokenType.GOTO)
            {
            }
        }
    }
}
