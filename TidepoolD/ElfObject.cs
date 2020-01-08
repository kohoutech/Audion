using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TidepoolD
{
    class ElfObject
    {
        public static int SHT_PROGBITS = 1;		// Program data */
        public static int SHT_NOBITS = 8;		// Program space with no data (bss) */

        public static int SHF_WRITE = (1 << 0);         // Writable */
        public static int SHF_ALLOC = (1 << 1);	        // Occupies memory during execution */
        public static int SHF_EXECINSTR = (1 << 2);	    // Executable */
    }
}
