/* ----------------------------------------------------------------------------
Transonic VST Library
Copyright (C) 2005-2019  George E Greaney

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

//copied from the Transonic LibTransWave library

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Transonic.VST
{
    public class WaveDevices
    {
        //p/invoke imports
        [DllImport("winmm.dll", SetLastError = true)]
        static extern uint waveInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        static extern MMRESULT waveInGetDevCaps(int uDeviceID, ref WAVEINCAPS pwic, int cbwic);

        [DllImport("winmm.dll", SetLastError = true)]
        static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        static extern MMRESULT waveOutGetDevCaps(int hwo, ref WAVEOUTCAPS pwoc, int cbwoc);

//-----------------------------------------------------------------------------

        public List<String> inputDeviceNames;
        public List<String> outputDeviceNames;

        public WaveDevices()
        {
            //input devices
            int deviceID;
            uint incount = waveInGetNumDevs();
            inputDeviceNames = new List<String>();
            inputDeviceNames.Add("no input");
            WAVEINCAPS inCaps = new WAVEINCAPS();
            for (deviceID = 0; deviceID < incount; deviceID++)
            {
                MMRESULT result = waveInGetDevCaps(deviceID, ref inCaps, Marshal.SizeOf(inCaps));
                String inName = inCaps.szPname;
                inputDeviceNames.Add(inName);
            }

            //output devices
            uint outcount = waveOutGetNumDevs();
            outputDeviceNames = new List<String>();
            outputDeviceNames.Add("no output");
            WAVEOUTCAPS outCaps = new WAVEOUTCAPS();
            for (deviceID = 0; deviceID < outcount; deviceID++)
            {
                MMRESULT result = waveOutGetDevCaps(deviceID, ref outCaps, Marshal.SizeOf(outCaps));
                String outName = outCaps.szPname;
                outputDeviceNames.Add(outName);
            }
        }

        public List<String> getInDevNameList()
        {
            return inputDeviceNames;
        }

        public List<String> getOutDevNameList()
        {
            return outputDeviceNames;
        }
    }

//-----------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEINCAPS
    {
        public ushort wMid;             //manfacturer id
        public ushort wPid;             //product id
        public uint vDriverVersion;     //device driver ver num, high byte = major ver, lo byte = minor ver
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;          //product name
        public int dwFormats;
        public short wChannels;
        public short wReserved;
        public uint dwSupport;          //must be 0
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEOUTCAPS
    {
        public ushort wMid;             //manfacturer id
        public ushort wPid;             //product id
        public uint vDriverVersion;     //device driver ver num, high byte = major ver, lo byte = minor ver
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;          //product name
        public int dwFormats;
        public short wChannels;
        public short wReserved;
        public uint dwSupport;          //optional functionality supported by the device
    }

    public enum MMRESULT : uint
    {
        MMSYSERR_NOERROR = 0,
        MMSYSERR_ERROR = 1,
        MMSYSERR_BADDEVICEID = 2,
        MMSYSERR_NOTENABLED = 3,
        MMSYSERR_ALLOCATED = 4,
        MMSYSERR_INVALHANDLE = 5,
        MMSYSERR_NODRIVER = 6,
        MMSYSERR_NOMEM = 7,
        MMSYSERR_NOTSUPPORTED = 8,
        MMSYSERR_BADERRNUM = 9,
        MMSYSERR_INVALFLAG = 10,
        MMSYSERR_INVALPARAM = 11,
        MMSYSERR_HANDLEBUSY = 12,
        MMSYSERR_INVALIDALIAS = 13,
        MMSYSERR_BADDB = 14,
        MMSYSERR_KEYNOTFOUND = 15,
        MMSYSERR_READERROR = 16,
        MMSYSERR_WRITEERROR = 17,
        MMSYSERR_DELETEERROR = 18,
        MMSYSERR_VALNOTFOUND = 19,
        MMSYSERR_NODRIVERCB = 20,
        WAVERR_BADFORMAT = 32,
        WAVERR_STILLPLAYING = 33,
        WAVERR_UNPREPARED = 34
    }
}

