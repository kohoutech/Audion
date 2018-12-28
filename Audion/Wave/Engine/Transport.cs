/* ----------------------------------------------------------------------------
LibTransWave : a library for playing, editing and storing audio wave data
Copyright (C) 2005-2017  George E Greaney

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

namespace Transonic.Wave.Engine
{
    public class Transport
    {
        //communication with waverly.dll

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportPlay();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportStop();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportPause();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportRewind();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportFastForward(int speed);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportRecord();

        //[DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void TransportSetVolume(float volume);

        //[DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void TransportSetBalance(float balance);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportSetLeftLevel(float level);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportSetRightLevel(float level);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportGetLeftLevel();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportGetRightLevel();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int TransportGetCurrentPos();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int TransportSetCurrentPos(int pos);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportSetWaveOut(int deviceIdx);

//- transport methods ---------------------------------------------------------

        public void play()
        {
            TransportPlay();
        }

        public void pause()
        {
            TransportPause();
        }

        public void stop()
        {
            TransportStop();
        }

        public void rewind()
        {
            TransportRewind();
        }

        public void fastForward(int speed)
        {
            //TransportFastForward(speed);      //doesn't work!
        }

        public void record()
        {
            TransportRecord();
        }

        public void setLeftLevel(float level)
        {
            TransportSetLeftLevel(level);
        }

        public void setRightLevel(float level)
        {
            TransportSetRightLevel(level);
        }

        public float getLeftLevel()
        {
            return TransportGetLeftLevel();
        }

        public float getRightLevel()
        {
            return TransportGetRightLevel();
        }

        public int getCurrentPos()
        {
            return TransportGetCurrentPos();    //in samples
        }

        public void setCurrentPos(int pos)
        {
            TransportSetCurrentPos(pos);		//in samples
        }

        public void setOutDevice(int devIdx)
        {
            TransportSetWaveOut(devIdx);
        }        
    }
}
