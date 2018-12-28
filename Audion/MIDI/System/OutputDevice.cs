/* ----------------------------------------------------------------------------
Transonic MIDI Library
Copyright (C) 1995-2018  George E Greaney

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

using Transonic.MIDI;

// p/invoke calls and structs used with WINMM.DLL library taken from http://www.pinvoke.net

namespace Transonic.MIDI.System
{
    public class OutputDevice
    {
        [DllImport("winmm.dll")]
        static extern MMRESULT midiOutOpen(out IntPtr lphMidiOut, int uDeviceID, IntPtr dwCallback, IntPtr dwInstance, int dwFlags);

        [DllImport("winmm.dll")]
        static extern MMRESULT midiOutClose(IntPtr hMidiOut);

        [DllImport("winmm.dll")]
        static extern MMRESULT midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

        [DllImport("winmm.dll")]
        static extern MMRESULT midiOutLongMsg(IntPtr hMidiOut, ref MIDIHDR lpMidiOutHdr, int cbMidiOutHdr);

        [DllImport("winmm.dll")]
        static extern MMRESULT midiOutPrepareHeader(IntPtr hMidiOut, ref MIDIHDR lpMidiOutHdr, int cbMidiOutHdr);

        [DllImport("winmm.dll")]
        static extern MMRESULT midiOutUnprepareHeader(IntPtr hMidiOut, ref MIDIHDR lpMidiOutHdr, int cbMidiOutHdr);

//winmm midi out funcs not implemented yet
//midiOutCacheDrumPatches
//midiOutCachePatches
//midiOutGetErrorTextA
//midiOutGetErrorTextW

//midiOutGetID
//midiOutGetVolume
//midiOutSetVolume
//midiOutReset

//-----------------------------------------------------------------------------

        public int devID;
        public String devName;
        public IntPtr devHandle;

        private bool opened;

        const int CALLBACK_NULL = 0x0;

        public OutputDevice(int _id, string _name)
        {
            devID = _id;
            devName = _name;
            opened = false;
        }


// midi funcs -----------------------------------------------------------------

        public void open()
        {
            if (!opened)
            {
                MMRESULT result = midiOutOpen(out devHandle, devID, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);
                if (result != MMRESULT.MMSYSERR_NOERROR)
                {
                    throw new MidiSystemException("couldn't open output device " + devName);
                }
                opened = true;
                //Console.WriteLine("opened device " + devName + " result = " + result);
            }
        }

        public void close()
        {
            if (opened)
            {
                MMRESULT result = midiOutClose(devHandle);
                if (result != MMRESULT.MMSYSERR_NOERROR)
                {
                    throw new MidiSystemException("couldn't close output device " + devName);
                }
                opened = false;
                //Console.WriteLine("closed device " + devName + " result = " + result);
            }
        }

        //only send channel and sysex messages
        public void sendMessage(byte[] msg)
        {
            if (msg.Length > 3)
            {
                //we don't handle sending sys ex msgs yet
            }
            else 
            {
                //short midi message
                uint outMsg = msg[0];
                if (msg.Length > 1) outMsg += (uint)(msg[1] << 8);
                if (msg.Length > 2) outMsg += (uint)(msg[2] << 16);
                midiOutShortMsg(devHandle, outMsg);
                //Console.WriteLine("OUTPUT DEVICE: sent short msg to name = " + devName + " : " + data[0].ToString("X2") + "." + 
                //data[1].ToString("X2") + "." + data[2].ToString("X2"));                  
            }
        }

        //send an "all Notes Off" control msg to each channel
        public void allNotesOff()
        {
            uint outMsg = (0x7b << 8) + 0xb0;
            for (int channel = 0; channel < 16; channel++)
            {
                midiOutShortMsg(devHandle, outMsg);
                outMsg++;
            }
        }
    }
}
