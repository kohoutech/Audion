using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolModelE
{
    class TidepoolE
    {
        Scanner scanner;

        public TidepoolE()
        {
            scanner = null;
        }

        public void compile(byte[] source)
        
        {
            scanner = new Scanner(source);
            scanner.next();
            while (scanner.token != (int)TokenType.tEOF)
            {
                scanner.printToken();
                scanner.next();
            }
            Console.WriteLine("done.");
        }
    }
}
