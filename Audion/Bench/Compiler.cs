/* ----------------------------------------------------------------------------
Audion : a audio plugin creator
Copyright (C) 2011-2018  George E Greaney

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
using System.Runtime.InteropServices;


namespace Audion.Bench
{
    class Compiler
    {
        //communication with tidepool.dll
        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tp_new();

        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void tp_delete(IntPtr tidepool);

        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void tp_set_options(IntPtr tidepool, string str);

        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int tp_set_output_type(IntPtr tidepool, int output_type);

        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int tp_add_file(IntPtr tidepool, string filename);

        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int tp_add_library(IntPtr tidepool, string libraryname);

        [DllImport("Tidepool.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int tp_output_file(IntPtr tidepool, string filename);

        //---------------------------------------------------------------------

        IntPtr tidepool;

        public Compiler()
        {
            tidepool = tp_new();
        }

        public void compileIt(string srcname, string dllname)
        {
            int result;
            
            tp_set_output_type(tidepool, 3);

            tp_set_options(tidepool, "-shared");

            result = tp_add_file(tidepool, "ART.obj");
            result = tp_add_file(tidepool, srcname);

            result = tp_add_library(tidepool, "kernel32");
            result = tp_add_library(tidepool, "user32");
            result = tp_add_library(tidepool, "gdi32");

            result = tp_output_file(tidepool, dllname);
        }

        public void shutDown()
        {
            tp_delete(tidepool);
        }
    }
}
