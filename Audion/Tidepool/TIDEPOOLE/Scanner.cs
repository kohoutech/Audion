using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolModelE
{
    class Scanner
    {
        public String[] keywordSpelling = { "EOF", "int const", "float const", "string const", "char const",
            "AUTO", "BREAK", "CASE", "CONST", "CONTINUE", "DEFAULT", "DO", "ELSE", "ENUM", "EXTERN", "FOR",
            "GOTO", "IF", "INLINE", "RESTRICT", "REGISTER", "RETURN",  "SIZEOF", "STATIC", "STRUCT", "SWITCH", "TYPEDEF", "UNION", "WHILE", "VOLATILE",
            "VOID", "CHAR", "INT", "FLOAT", "DOUBLE", "SHORT", "LONG", "SIGNED", "UNSIGNED",
            "[", "]", "(", ")", "{", "}", ".", "->", "++", "--", "&", "*", "+", "-", "~", "!",
            "/", "%", "<<", ">>", "<", ">", "<=", ">=", "==", "!=", "^", "|", "&&", "||",
            "?", ":", ";", "...", "=", "*=", "/=", "%=", "+=", "-=", "<<=", ">>=", "&=", "^=", "|=", ",",  
            "error"};

        public byte[] source;
        public int srcpos;

        public int token;
        public int tokenIdNum;

        public Dictionary<string, TokenType> keywordTbl;
        public Dictionary<string, TokenSym> identTbl;
        public Dictionary<int, TokenSym> identSpellTbl;
        public Dictionary<char, char> escapeTbl;

        public long intval;
        public double floatval;
        public char chval;
        public string strval;

        public Scanner(byte[] _source)
        {
            List<byte> sourcebytes = new List<byte>(_source);
            sourcebytes.Add(0x0);
            source = sourcebytes.ToArray();
            srcpos = 0;

            keywordTbl = new Dictionary<string, TokenType>();

            keywordTbl.Add("auto", TokenType.tAUTO);
            keywordTbl.Add("break", TokenType.tBREAK);
            keywordTbl.Add("case", TokenType.tCASE);
            keywordTbl.Add("const", TokenType.tCONST);
            keywordTbl.Add("continue", TokenType.tCONTINUE);
            keywordTbl.Add("default", TokenType.tDEFAULT);
            keywordTbl.Add("do", TokenType.tDO);
            keywordTbl.Add("else", TokenType.tELSE);
            keywordTbl.Add("enum", TokenType.tENUM);
            keywordTbl.Add("extern", TokenType.tEXTERN);
            keywordTbl.Add("for", TokenType.tFOR);
            keywordTbl.Add("goto", TokenType.tGOTO);
            keywordTbl.Add("if", TokenType.tIF);
            keywordTbl.Add("inline", TokenType.tINLINE);
            keywordTbl.Add("restrict", TokenType.tRESTRICT);
            keywordTbl.Add("register", TokenType.tREGISTER);
            keywordTbl.Add("return", TokenType.tRETURN);
            keywordTbl.Add("sizeof", TokenType.tSIZEOF);
            keywordTbl.Add("static", TokenType.tSTATIC);
            keywordTbl.Add("struct", TokenType.tSTRUCT);
            keywordTbl.Add("switch", TokenType.tSWITCH);
            keywordTbl.Add("typedef", TokenType.tTYPEDEF);
            keywordTbl.Add("union", TokenType.tUNION);
            keywordTbl.Add("while", TokenType.tWHILE);
            keywordTbl.Add("volatile", TokenType.tVOLATILE);

            keywordTbl.Add("void", TokenType.tVOID);
            keywordTbl.Add("char", TokenType.tCHAR);
            keywordTbl.Add("int", TokenType.tINT);
            keywordTbl.Add("float", TokenType.tFLOAT);
            keywordTbl.Add("double", TokenType.tDOUBLE);
            keywordTbl.Add("short", TokenType.tSHORT);
            keywordTbl.Add("long", TokenType.tLONG);
            keywordTbl.Add("signed", TokenType.tSIGNED);
            keywordTbl.Add("unsigned", TokenType.tUNSIGNED);

            identTbl = new Dictionary<string, TokenSym>();
            identSpellTbl = new Dictionary<int, TokenSym>();
            tokenIdNum = (int)TokenType.tERROR + 1;

            escapeTbl = new Dictionary<char, char>();
            escapeTbl.Add('a', '\a');
            escapeTbl.Add('b', '\b');
            escapeTbl.Add('f', '\f');
            escapeTbl.Add('n', '\n');
            escapeTbl.Add('r', '\r');
            escapeTbl.Add('t', '\t');
            escapeTbl.Add('v', '\v');
            escapeTbl.Add('\\', '\\');
            escapeTbl.Add('\'', '\'');
            escapeTbl.Add('\"', '\"');            
        }

        public void printToken()
        {
            if (token == (int)TokenType.tINTCONST)
            {
                Console.WriteLine("int const ({0})", intval);
            }
            else if (token == (int)TokenType.tFLOATCONST)
            {
                Console.WriteLine("float const ({0})", floatval);
            }
            else if (token == (int)TokenType.tSTRINGCONST)
            {
                Console.WriteLine("string const \"{0}\"", strval);
            }
            else if (token == (int)TokenType.tCHARCONST)
            {
                Console.WriteLine("char const \'{0}\'", chval);
            }
            else if (token <= (int)TokenType.tERROR)
            {
                Console.WriteLine(keywordSpelling[token]);
            }
            else
            {
                TokenSym ts = identSpellTbl[token];
                Console.WriteLine("IDENT ({0})", ts.tokstr);
            }
        }

        public bool isIdentChar(char c)
        {
            return ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c == '_'));
        }

        public bool isDigit(char c, int bass)
        {
            return (bass == 16) ? ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')) :
                   (bass == 8) ?   (c >= '0' && c <= '7') : 
                                   (c >= '0' && c <= '9');
        }

        public int parseNumber(int p, char c)
        {
            string numstr = "" + c;
            int bass = 10;
            if (c == '0')
            {
                c = (char)source[++p];
                if (c == 'x' || c == 'X')
                {
                    numstr = "0x";
                    bass = 16;
                }
                else
                {
                    p--;
                    numstr = "0";
                    bass = 8;
                }
            }
            c = (char)source[++p];
            while (isDigit(c, bass))
            {
                numstr = numstr + c;
                c = (char)source[++p];
            }
            if (c != '.')
            {
                intval = Convert.ToInt32(numstr, bass);         //it's an int const
                token = (int)TokenType.tINTCONST;
            }
            else
            {
                numstr = numstr + ".";
                c = (char)source[++p];
                while (c >= '0' && c <= '9')        //get decimal part
                {
                    numstr = numstr + c;
                    c = (char)source[++p];
                }
                if (c == 'E' || c == 'e')            //get exponent part
                {
                    numstr = numstr + "E";
                    c = (char)source[++p];
                    if (c == '+' | c == '-')
                    {
                        numstr = numstr + c;
                    }
                    while (c >= '0' && c <= '9')
                    {
                        numstr = numstr + c;
                        c = (char)source[++p];
                    }
                }
                floatval = Convert.ToDouble(numstr);
                token = (int)TokenType.tFLOATCONST;
            }
            return p;
        }

        public int parseStringConst(int p)
        {
            string s = "";    
            char c = (char)source[++p];
            while (c != '\"')
            {
                if (c == '\\')
                {
                    c = (char)source[++p];
                    c = escapeTbl[c];
                }
                s = s + c;
                c = (char)source[++p];
            }
            p++;
            strval = s;
            token = (int)TokenType.tSTRINGCONST;
            return p;
        }

        public int parseCharConst(int p)
        {
            string s = "";
            char c = (char)source[++p];
            while (c != '\'')
            {
                if (c == '\\')
                {
                    c = (char)source[++p];
                    c = escapeTbl[c];
                }
                s = s + c;
                c = (char)source[++p];
            }
            p++;
            chval = s[0];
            token = (int)TokenType.tCHARCONST;
            return p;
        }

        public void next()
        {
            int p = srcpos;
            int p1 = 0;
            uint h = 1;
            bool done = false;
            while (!done)
            {
                char c = (char)source[p];
                switch (c)
                {

                    //end of file
                    case '\0':
                        token = (int)TokenType.tEOF;
                        break;

                    //whitespace
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        p++;
                        continue;

                    //identifiers
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
                    case 'L':
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
                        string idstr = "" + c;
                        c = (char)source[++p];
                        while (isIdentChar(c))
                        {
                            idstr = idstr + c;
                            c = (char)source[++p];
                        }
                        if (keywordTbl.ContainsKey(idstr))
                        {
                            token = (int)keywordTbl[idstr];
                        }
                        else if (identTbl.ContainsKey(idstr))
                        {
                            token = identTbl[idstr].toknum;
                        }
                        else
                        {
                            TokenSym ts = new TokenSym(idstr, tokenIdNum++);
                            token = ts.toknum;
                            identSpellTbl[token] = ts;
                        }
                        break;

                    //numbers
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
                        p = parseNumber(p, c);
                        break;

                    case '\'':
                        p = parseCharConst(p);
                        break;

                    case '\"':
                        p = parseStringConst(p);
                        break;

                    //punctuation
                    case '[':
                        p++;
                        token = (int)TokenType.tLBRACKET;
                        break;

                    case ']':
                        p++;
                        token = (int)TokenType.tRBRACKET;
                        break;

                    case '(':
                        p++;
                        token = (int)TokenType.tLPAREN;
                        break;

                    case ')':
                        p++;
                        token = (int)TokenType.tRPAREN;
                        break;

                    case '{':
                        p++;
                        token = (int)TokenType.tLBRACE;
                        break;

                    case '}':
                        p++;
                        token = (int)TokenType.tRBRACE;
                        break;

                    case '~':
                        p++;
                        token = (int)TokenType.tTILDE;
                        break;

                    case '?':
                        p++;
                        token = (int)TokenType.tQUESTION;
                        break;

                    case ':':
                        p++;
                        token = (int)TokenType.tCOLON;
                        break;

                    case ';':
                        p++;
                        token = (int)TokenType.tSEMICLN;
                        break;

                    case ',':
                        p++;
                        token = (int)TokenType.tCOMMA;
                        break;

                    case '.':
                        c = (char)source[++p];
                        if (c == '.')
                        {
                            token = (int)TokenType.tELIPSIS;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tPERIOD;
                        }
                        p++;
                        break;

                    case '!':
                        c = (char)source[++p];
                        if (c == '=')
                        {
                            token = (int)TokenType.tNOTEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tEXCLAIM;
                        }
                        p++;
                        break;

                    case '=':
                        c = (char)source[++p];
                        if (c == '=')
                        {
                            token = (int)TokenType.tEQUEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tEQUAL;
                        }
                        p++;
                        break;


                    case '*':
                        c = (char)source[++p];
                        if (c == '=')
                        {
                            token = (int)TokenType.tMULTEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tSTAR;
                        }
                        p++;
                        break;

                    case '/':
                        c = (char)source[++p];
                        if (c == '=')
                        {
                            token = (int)TokenType.tDIVEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tSLASH;
                        }
                        p++;
                        break;

                    case '%':
                        c = (char)source[++p];
                        if (c == '=')
                        {
                            token = (int)TokenType.tMODEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tPERCENT;
                        }
                        p++;
                        break;

                    case '^':
                        c = (char)source[++p];
                        if (c == '=')
                        {
                            token = (int)TokenType.tXOREQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tCARET;
                        }
                        p++;
                        break;

                    case '+':
                        c = (char)source[++p];
                        if (c == '+')
                        {
                            token = (int)TokenType.tPLUSPLUS;
                        }
                        else if (c == '=')
                        {
                            token = (int)TokenType.tPLUSEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tPLUS;
                        }
                        p++;
                        break;

                    case '-':
                        c = (char)source[++p];
                        if (c == '-')
                        {
                            token = (int)TokenType.tMINUSMINUS;
                        }
                        else if (c == '=')
                        {
                            token = (int)TokenType.tMINUSEQU;
                        }
                        else if (c == '>')
                        {
                            token = (int)TokenType.tARROW;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tMINUS;
                        }
                        p++;
                        break;

                    case '<':
                        c = (char)source[++p];
                        if (c == '<')
                        {
                            char c1 = (char)source[++p];
                            if (c1 == '=')
                            {
                                token = (int)TokenType.tLESSLESSEQU;
                            }
                            else
                            {
                                p--;
                                token = (int)TokenType.tLESSLESS;
                            }
                        }
                        else if (c == '=')
                        {
                            token = (int)TokenType.tLESSEQU;
                        }
                        else 
                        {
                            p--;
                            token = (int)TokenType.tLESSTHN;
                        }
                        p++;
                        break;

                    case '>':
                        c = (char)source[++p];
                        if (c == '>')
                        {
                            char c1 = (char)source[++p];
                            if (c1 == '=')
                            {
                                token = (int)TokenType.tGTRGTREQU;
                            }
                            else
                            {
                                p--;
                                token = (int)TokenType.tGTRGTR;
                            }
                        }
                        else if (c == '=')
                        {
                            token = (int)TokenType.tGTREQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tGTRTHN;
                        }
                        p++;
                        break;

                    case '&':
                        c = (char)source[++p];
                        if (c == '&')
                        {
                            token = (int)TokenType.tAMPAMP;
                        }
                        else if (c == '=')
                        {
                            token = (int)TokenType.tAMPEQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tAMPERSAND;
                        }
                        p++;
                        break;

                    case '|':
                        c = (char)source[++p];
                        if (c == '|')
                        {
                            token = (int)TokenType.tBARBAR;
                        }
                        else if (c == '=')
                        {
                            token = (int)TokenType.tBAREQU;
                        }
                        else
                        {
                            p--;
                            token = (int)TokenType.tBAR;
                        }
                        p++;
                        break;

                    //everthing else
                    default:
                        p++;
                        token = (int)TokenType.tERROR;
                        break;

                }
                srcpos = p;
                done = true;
            }

        }
    }

    public enum TokenType
    {
        tEOF,
        tINTCONST,
        tFLOATCONST,
        tSTRINGCONST,
        tCHARCONST,

        tAUTO,
        tBREAK,
        tCASE,
        tCONST,
        tCONTINUE,
        tDEFAULT,
        tDO,
        tELSE,
        tENUM,
        tEXTERN,
        tFOR,
        tGOTO,
        tIF,
        tINLINE,
        tRESTRICT,
        tREGISTER,
        tRETURN,
        tSIZEOF,
        tSTATIC,
        tSTRUCT,
        tSWITCH,
        tTYPEDEF,
        tUNION,
        tVOLATILE,
        tWHILE,

        tVOID,
        tCHAR,
        tINT,
        tFLOAT,
        tDOUBLE,
        tSHORT,
        tLONG,
        tSIGNED,
        tUNSIGNED,

        tLBRACKET,      //[
        tRBRACKET,      //]
        tLPAREN,
        tRPAREN,
        tLBRACE,        //{
        tRBRACE,        //}
        tPERIOD,
        tARROW,         //->

        tPLUSPLUS,
        tMINUSMINUS,
        tAMPERSAND,
        tSTAR,
        tPLUS,
        tMINUS,
        tTILDE,
        tEXCLAIM,

        tSLASH,
        tPERCENT,
        tLESSLESS,
        tGTRGTR,
        tLESSTHN,
        tGTRTHN,
        tLESSEQU,
        tGTREQU,
        tEQUEQU,
        tNOTEQU,
        tCARET,
        tBAR,
        tAMPAMP,
        tBARBAR,        //not the elephant

        tQUESTION,
        tCOLON,
        tSEMICLN,
        tELIPSIS,

        tEQUAL,
        tMULTEQU,
        tDIVEQU,
        tMODEQU,
        tPLUSEQU,
        tMINUSEQU,
        tLESSLESSEQU,
        tGTRGTREQU,

        tXOREQU,
        tAMPEQU,
        tBAREQU,
        tCOMMA,

        tERROR
    }

    public class TokenSym
    {
        public int toknum;
        public String tokstr;

        public TokenSym(String str, int num)
        {
            tokstr = str;
            toknum = num;
        }
    }

    public class CValue
    {

    }
}
