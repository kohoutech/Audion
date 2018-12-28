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

// p/invoke calls and structs used with WINMM.DLL library taken from http://www.pinvoke.net

namespace Transonic.MIDI.System
{
    public class MidiSystem
    {       
        //p/invoke imports
        [DllImport("winmm.dll", SetLastError = true)]
        static extern uint midiInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        static extern MMRESULT midiInGetDevCaps(int uDeviceID, ref MIDIINCAPS caps, int cbMidiInCaps);

        [DllImport("winmm.dll", SetLastError = true)]
        static extern uint midiOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        static extern MMRESULT midiOutGetDevCaps(int uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, int cbMidiOutCaps);

//-----------------------------------------------------------------------------

        public List<InputDevice> inputDevices;
        public List<OutputDevice> outputDevices;

        public MidiSystem()
        {
            //input devices
            int deviceID;
            uint incount = midiInGetNumDevs();
            inputDevices = new List<InputDevice>();
            MIDIINCAPS inCaps = new MIDIINCAPS();
            for (deviceID = 0; deviceID < incount; deviceID++)
            {
                MMRESULT result = midiInGetDevCaps(deviceID, ref inCaps, Marshal.SizeOf(inCaps));

                //if we get an error, just skip the device
                if (result == MMRESULT.MMSYSERR_NOERROR)
                {
                    InputDevice indev = new InputDevice(deviceID, inCaps.szPname);
                    inputDevices.Add(indev);
                }
            }

            //output devices
            uint outcount = midiOutGetNumDevs();
            outputDevices = new List<OutputDevice>((int)outcount);
            MIDIOUTCAPS outCaps = new MIDIOUTCAPS();
            for (deviceID = 0; deviceID < outcount; deviceID++)
            {
                MMRESULT result = midiOutGetDevCaps(deviceID, ref outCaps, Marshal.SizeOf(outCaps));                 

                //if we get an error, just skip the device
                if (result == MMRESULT.MMSYSERR_NOERROR)
                {
                    OutputDevice outdev = new OutputDevice(deviceID, outCaps.szPname);
                    outputDevices.Add(outdev);
                }
            }
        }

        public void shutdown()
        {
            foreach (InputDevice indev in inputDevices)
            {
                indev.stop();
                indev.close();
            }
            foreach (OutputDevice outdev in outputDevices)
            {
                outdev.close();
            }
        }

        public List<String> getInDevNameList()
        {
            List<String> nameList = new List<String>();
            foreach (InputDevice indev in inputDevices) 
            {
                nameList.Add(indev.devName);
            }
            if (nameList.Count == 0) nameList.Add("none");
            return nameList;
        }

        public InputDevice findInputDevice(String inName)
        {
            InputDevice result = null;
            foreach (InputDevice indev in inputDevices)
            {
                if (indev.devName.Equals(inName)) {
                    result = indev;
                    break;
                }
            }
            return result;
        }

        public List<String> getOutDevNameList()
        {
            List<String> nameList = new List<String>();
            foreach (OutputDevice outdev in outputDevices)
            {
                nameList.Add(outdev.devName);
            }
            if (nameList.Count == 0) nameList.Add("none");
            return nameList;
        }

        public OutputDevice findOutputDevice(String outName)
        {
            OutputDevice result = null;
            foreach (OutputDevice outdev in outputDevices)
            {
                if (outdev.devName.Equals(outName)) {
                    result = outdev;
                    break;
                }
            }
            return result;
        }

//- general midi --------------------------------------------------------------

        //general MIDI list: https://en.wikipedia.org/wiki/General_MIDI

        public static List<String> GMNames = new List<String>(){
        
            "Acoustic Grand Piano",
            "Bright Acoustic Piano",
            "Electric Grand Piano",
            "Honky-tonk Piano",
            "Electric Piano 1",
            "Electric Piano 2",
            "Harpsichord",
            "Clavinet",
            "Celesta",
            "Glockenspiel",
            "Music Box",
            "Vibraphone",
            "Marimba",
            "Xylophone",
            "Tubular Bells",
            "Dulcimer",
            "Drawbar Organ",
            "Percussive Organ",
            "Rock Organ",
            "Church Organ",
            "Reed Organ",
            "Accordion",
            "Harmonica",
            "Tango Accordion",
            "Acoustic Guitar (nylon)",
            "Acoustic Guitar (steel)",
            "Electric Guitar (jazz)",
            "Electric Guitar (clean)",
            "Electric Guitar (muted)",
            "Overdriven Guitar",
            "Distortion Guitar",
            "Guitar Harmonics",
            "Acoustic Bass",
            "Electric Bass (finger)",
            "Electric Bass (pick)",
            "Fretless Bass",
            "Slap Bass 1",
            "Slap Bass 2",
            "Synth Bass 1",
            "Synth Bass 2",
            "Violin",
            "Viola",
            "Cello",
            "Contrabass",
            "Tremolo Strings",
            "Pizzicato Strings",
            "Orchestral Harp",
            "Timpani",
            "String Ensemble 1",
            "String Ensemble 2",
            "Synth Strings 1",
            "Synth Strings 2",
            "Choir Aahs",
            "Voice Oohs",
            "Synth Choir",
            "Orchestra Hit",
            "Trumpet",
            "Trombone",
            "Tuba",
            "Muted Trumpet",
            "French Horn",
            "Brass Section",
            "Synth Brass 1",
            "Synth Brass 2",
            "Soprano Sax",
            "Alto Sax",
            "Tenor Sax",
            "Baritone Sax",
            "Oboe",
            "English Horn",
            "Bassoon",
            "Clarinet",
            "Piccolo",
            "Flute",
            "Recorder",
            "Pan Flute",
            "Blown bottle",
            "Shakuhachi",
            "Whistle",
            "Ocarina",
            "Lead 1 (square)",
            "Lead 2 (sawtooth)",
            "Lead 3 (calliope)",
            "Lead 4 (chiff)",
            "Lead 5 (charang)",
            "Lead 6 (voice)",
            "Lead 7 (fifths)",
            "Lead 8 (bass + lead)",
            "Pad 1 (new age)",
            "Pad 2 (warm)",
            "Pad 3 (polysynth)",
            "Pad 4 (choir)",
            "Pad 5 (bowed)",
            "Pad 6 (metallic)",
            "Pad 7 (halo)",
            "Pad 8 (sweep)",
            "FX 1 (rain)",
            "FX 2 (soundtrack)",
            "FX 3 (crystal)",
            "FX 4 (atmosphere)",
            "FX 5 (brightness)",
            "FX 6 (goblins)",
            "FX 7 (echoes)",
            "FX 8 (sci-fi)",
            "Sitar",
            "Banjo",
            "Shamisen",
            "Koto",
            "Kalimba",
            "Bagpipe",
            "Fiddle",
            "Shanai",
            "Tinkle Bell",
            "Agogo",
            "Steel Drums",
            "Woodblock",
            "Taiko Drum",
            "Melodic Tom",
            "Synth Drum",
            "Reverse Cymbal",
            "Guitar Fret Noise",
            "Breath Noise",
            "Seashore",
            "Bird Tweet",
            "Telephone Ring",
            "Helicopter",
            "Applause",
            "Gunshot"
        };
    }

//- sys exception -------------------------------------------------------------

    public class MidiSystemException : Exception
    {
        public MidiSystemException(String errorMsg)
            : base(errorMsg)
        {
        }
    }

//-----------------------------------------------------------------------------

    //const vals from mmsystem.h
    public enum MMRESULT : uint
    {
        MMSYSERR_NOERROR = 0,
        TIMERR_NOERROR = 0,
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
        WAVERR_UNPREPARED = 34,
        TIMERR_NOCANDO = 97
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIDIINCAPS
    {
        public ushort wMid;             //manfacturer id
        public ushort wPid;             //product id
        public uint vDriverVersion;     //device driver ver num, high byte = major ver, lo byte = minor ver
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;          //product name
        public uint dwSupport;          //must be 0
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIDIOUTCAPS
    {
        public ushort wMid;             //manfacturer id
        public ushort wPid;             //product id
        public uint vDriverVersion;     //device driver ver num, high byte = major ver, lo byte = minor ver
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;          //product name
        public ushort wTechnology;      //type of midi output device
        public ushort wVoices;          //num voices if internal synth, 0 if device is a port
        public ushort wNotes;           //max num of notes synth can play, 0 if device is a port
        public ushort wChannelMask;     //internal synth's channel, -1 if device is a port
        public uint dwSupport;          //optional functionality supported by the device
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIDIHDR
    {
    public IntPtr data;
    public uint bufferLength;
    public uint bytesRecorded;
    public IntPtr user;
    public uint flags;
    public IntPtr next;
    public IntPtr reserved;
    public uint offset;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public IntPtr[] reservedArray;
    }
}

//  Console.WriteLine(" there's no sun in the shadow of the wizard");
