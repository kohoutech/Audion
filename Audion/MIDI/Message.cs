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

//J Glatt's Midi page: http://midi.teragonaudio.com/tech/midispec.htm

namespace Transonic.MIDI
{
    public class Message
    {

//- static methods ------------------------------------------------------------

        public static Message getMessage(byte[] data)
        {
            Message msg = null;

            int status = data[0];
            if (status < 0xF0)          //midi channel message
            {
                int msgtype = status / 16;
                int channel = status % 16;

                int b1 = data[1];
                int b2 = 0;
                if ((msgtype != 0xC) && (msgtype != 0xD))
                {
                    b2 = data[2];
                }

                msg = Message.getChannelMessage(msgtype, channel, b1, b2);
            }
            else if (status == 0xF0)                        //sys ex msg
            {
                List<byte> bytes = new List<byte>(data);
                msg = new SysExMessage(bytes);
            }
            else
            {                                   //status msg
                int b1 = 0;
                int b2 = 0;
                int datalen = SystemMessage.SysMsgLen[status - 0xF0] - 1;
                if (datalen > 0)
                {
                    b1 = data[1];
                }
                if (datalen > 1)
                {
                    b2 = data[2];
                    b1 = ((b1 % 128) * 128) + (b2 % 128);
                }
                msg = new SystemMessage(status, b1);
            }

            return msg;
        }

        public static Message getChannelMessage(int msgtype, int channel, int b1, int b2)
        {
            Message msg = null;

            switch (msgtype)
            {
                case 0x8:
                    msg = new NoteOffMessage(channel, b1, b2);
                    break;
                case 0x9:
                    msg = new NoteOnMessage(channel, b1, b2);
                    break;
                case 0xa:
                    msg = new AftertouchMessage(channel, b1, b2);
                    break;
                case 0xb:
                    msg = new ControllerMessage(channel, b1, b2);
                    break;
                case 0xc:
                    msg = new PatchChangeMessage(channel, b1);
                    break;
                case 0xd:
                    msg = new ChannelPressureMessage(channel, b1);
                    break;
                case 0xe:
                    int wheelamt = ((b1 % 128) * 128) + (b2 % 128);
                    msg = new PitchWheelMessage(channel, wheelamt);
                    break;
                default:
                    break;
            }
            //convert noteon msg w/ vel = 0 to noteoff msg
            if (msg is NoteOnMessage)
            {
                NoteOnMessage noteOn = (NoteOnMessage)msg;
                if (noteOn.velocity == 0)
                {
                    NoteOffMessage noteOff = new NoteOffMessage(noteOn.channel, noteOn.noteNumber, 0);
                    msg = noteOff;
                }
            }
            return msg;
        }

//- base class ----------------------------------------------------------------

        public Message()
        {
        }

        //for splitting a midi msg - handles subclass fields too
        public Message copy()
        {
            return (Message)this.MemberwiseClone();
        }

        //for sending a msg to an output device
        virtual public byte[] getDataBytes() 
        {
            return null;
        }
    }

//- subclasses ----------------------------------------------------------------

//-----------------------------------------------------------------------------
//  CHANNEL MESSAGES
//-----------------------------------------------------------------------------

    //channel message base class
    public class ChannelMessage : Message
    {
        public int channel;

        public ChannelMessage(int _channel) : base()
        {
            channel = _channel;
        }
    }

    public class NoteOnMessage : ChannelMessage     //0x90
    {
        public int noteNumber;
        public int velocity;

        public NoteOnMessage(int channel, int note, int vel)
            : base(channel)
        {
            noteNumber = note;
            velocity = vel;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0x90 + channel);
            bytes[1] = (byte)noteNumber;
            bytes[2] = (byte)velocity;
            return bytes;
        }

        public override string ToString()
        {
            return "Note On (" + channel + ") note = " + noteNumber + ", velocity = " + velocity;
        }
    }

    public class NoteOffMessage : ChannelMessage   //0x80
    {
        public int noteNumber;
        public int velocity;

        public NoteOffMessage(int channel, int note, int vel)
            : base(channel)
        {
            noteNumber = note;
            velocity = vel;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0x80 + channel);
            bytes[1] = (byte)noteNumber;
            bytes[2] = (byte)velocity;
            return bytes;
        }

        public override string ToString()
        {
            return "Note Off (" + channel + ") note = " + noteNumber;
        }
    }

    public class AftertouchMessage : ChannelMessage     //0xA0
    {
        public int noteNumber;
        public int pressure;

        public AftertouchMessage(int channel, int note, int press)
            : base(channel)
        {
            noteNumber = note;
            pressure = press;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0xa0 + channel);
            bytes[1] = (byte)noteNumber;
            bytes[2] = (byte)pressure;
            return bytes;
        }
    }

    public class ControllerMessage : ChannelMessage     //0xB0
    {
        public int ctrlNumber;
        public int ctrlValue;

        public ControllerMessage(int channel, int num, int val)
            : base(channel)
        {
            ctrlNumber = num;
            ctrlValue = val;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0xb0 + channel);
            bytes[1] = (byte)ctrlNumber;
            bytes[2] = (byte)ctrlValue;
            return bytes;
        }

        public override string ToString()
        {
            return "Controller (" + channel + ") number = " + ctrlNumber + ", value = " + ctrlValue;
        }
    }

    public class PatchChangeMessage : ChannelMessage       //0xC0
    {
        public int patchNumber;

        public PatchChangeMessage(int channel, int num)
            : base(channel)
        {
            patchNumber = num;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(0xc0 + channel);
            bytes[1] = (byte)patchNumber;
            return bytes;
        }

        public override string ToString()
        {
            return "Patch Change (" + channel + ") number = " + patchNumber;
        }
    }

    public class ChannelPressureMessage : ChannelMessage       //0xD0
    {
        public int pressure;

        public ChannelPressureMessage(int channel, int press)
            : base(channel)
        {
            pressure = press;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(0xd0 + channel);
            bytes[1] = (byte)pressure;
            return bytes;
        }
    }

    public class PitchWheelMessage : ChannelMessage     //0xE0
    {
        public int wheel;

        public PitchWheelMessage(int channel, int _wheel)
            : base(channel)
        {
            wheel = _wheel;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0xe0 + channel);
            bytes[1] = (byte)(wheel / 128);
            bytes[2] = (byte)(wheel % 128);
            return bytes;
        }
    }

//-----------------------------------------------------------------------------
//  SYSTEM MESSAGES
//-----------------------------------------------------------------------------

    public class SysExMessage : Message
    {
        public List<byte> sysExData;

        public SysExMessage(List<byte> _data)
            : base()
        {
            sysExData = _data;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = sysExData.ToArray();
            return bytes;
        }
    }

    public enum SYSTEMMESSAGE { 
        QUARTERFRAME = 0Xf1,        //f1
        SONGPOSITION,               //f2
        SONGSELECT,                 //f3
        UNDEFINED1,                 //f4
        UNDEFINED2,                 //f5
        TUNEREQUEST,                //f6
        SYSEXEND,                   //f7
        MIDICLOCK,                  //f8
        MIDITICK,                   //f9
        MIDISTART,                  //fa
        MIDICONTINUE,               //fb
        MIDISTOP,                   //fc
        UNDEFINED3,                 //fd
        ACTIVESENSE = 0xfe          //fe
    }; 

    public class SystemMessage : Message
    {
        public static int[] SysMsgLen = {1, 2, 3, 2, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1};      //f4, f5, fd are undefined

        SYSTEMMESSAGE msgtype;
        int value;

        public SystemMessage(int status, int val)
            : base()
        {
            msgtype = (SYSTEMMESSAGE)status;
            value = 0;
            switch (msgtype)
            {
                case SYSTEMMESSAGE.QUARTERFRAME :
                case SYSTEMMESSAGE.SONGSELECT :
                case SYSTEMMESSAGE.SONGPOSITION:
                    value = val;
                    break;
                default :
                    break;
            }        
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[SysMsgLen[(byte)msgtype - 0xF0]];
            bytes[0] = (byte)msgtype;
            switch (msgtype)
            {
                case SYSTEMMESSAGE.QUARTERFRAME:
                case SYSTEMMESSAGE.SONGSELECT:
                    bytes[1] = (byte)value;
                    break;
                case SYSTEMMESSAGE.SONGPOSITION:
                    bytes[1] = (byte)(value / 128);
                    bytes[2] = (byte)(value % 128);
                    break;
                default:
                    break;
            }
            return bytes;
        }
    }

    //to preserve any escape data in a midi file
    public class EscapeMessage : Message
    {
        List<byte> escData;

        public EscapeMessage(List<byte> _data)
            : base()
        {
            escData = _data;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = escData.ToArray();
            return bytes;
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");
