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

using Transonic.MIDI.System;

namespace Transonic.MIDI
{
    public class Track : SystemUnit
    {
        public Sequence seq;
        public List<Event> events;

        public int length;                  //total length in ticks
        public int measures;                //num of measures in track
        public int hiNote;
        public int loNote;

        //track i/o
        public InputDevice inDev;
        public int inputChannel;
        public OutputDevice outDev;
        public int outputChannel;

        public bool muted;
        public bool soloing;
        public bool recording;

        public int keyOfs;
        public int velOfs;
        public int timeOfs;

        public int bankNum;
        public int patchNum;
        public String patchname;
        public String defPatchname;
        public int volume;
        public int pan;

        public Track(Sequence _seq)
            : base("track")
        {
            seq = _seq;
            events = new List<Event>();
            length = 0;
            measures = 0;
            hiNote = 0;
            loNote = 127;

            muted = false;
            recording = false;

            inDev = null;
            inputChannel = 1;
            outDev = null;
            outputChannel = 1;

            keyOfs = 0;
            velOfs = 0;
            timeOfs = 0;

            bankNum = 0;
            patchNum = 0;
            patchname = null;
            defPatchname = defPatchname = MidiSystem.GMNames[0];
            volume = 127;
            pan = 64;
        }

//- track settings -----------------------------------------------------------------

        public void setName(String _name)
        {
            name = _name;
        }

        public void setMuted(bool on)
        {
            muted = on;
            if (muted)
            {
                allNotesOff();
            }
        }

        public void setSoloing(bool on)
        {
            soloing = on;
        }

        public void setRecording(bool on)
        {
            recording = on;
        }

        public void setPatchNum(int patch)
        {
            patchNum = patch;            
            defPatchname = (outputChannel != 9) ? MidiSystem.GMNames[patch] : "GM Drumkit";
        }

        public void setPatchname(String name)
        {
            patchname = name;
        }

        public void setVolume(int vol)
        {
            volume = vol;
        }

        public void setPan(int _pan)
        {
            pan = _pan;
        }

//- track input -----------------------------------------------------------------

        public void setInputDevice(InputDevice _inDev)
        {
            inDev = _inDev;
            inDev.open();
        }

        public void setInputChannel(int channel) 
        {
            inputChannel = channel;
        }

//- track output -----------------------------------------------------------------

        public void setOutputDevice(OutputDevice _outDev) 
        {
            outDev = _outDev;
            outDev.open();
        }

        public void setOutputChannel(int channel)
        {
            outputChannel = channel;
            defPatchname = (outputChannel != 9) ? MidiSystem.GMNames[patchNum] : "GM Drumkit";
        }

        public void sendMessage(Message msg)
        {
            if (!muted)
            {
                byte[] bytes = msg.getDataBytes();
                if (msg is ChannelMessage)
                {
                    bytes[0] |= (byte)outputChannel;
                }
                outDev.sendMessage(bytes);
            }
        }

        public void allNotesOff()
        {
            if (outDev != null)
            {
                outDev.allNotesOff();
            }
        }

//- event handling ------------------------------------------------------------

        //add new event to track positioned by tick
        public void addEvent(Event evt, int tick)
        {
            evt.setTick(tick);
            events.Add(evt);

            if ((evt is MessageEvent) && (((MessageEvent)evt).msg is NoteOnMessage))
            {
                NoteOnMessage noteOn = (NoteOnMessage)((MessageEvent)evt).msg;
                hiNote = (noteOn.noteNumber > hiNote) ? noteOn.noteNumber : hiNote;
                loNote = (noteOn.noteNumber < loNote) ? noteOn.noteNumber : loNote;
            }

            if (tick > length)
            {
                length = tick;
                if (length > seq.length)
                {
                    seq.length = length;
                }
            }

            seq.meterMap.tickToBeat(tick, out evt.measure, out evt.beat);
            if ((evt.measure + 1) > measures)
            {
                measures = evt.measure + 1;
                if (measures > seq.measures)
                {
                    seq.measures = measures;
                }
            }
        }

        //add new event to track positioned by measure and beat
        public void addEvent(Event evt, int measure, decimal beat)
        {
        }

        public Event findEvent(int measure, decimal beat)
        {
            int eventNum = 0;
            while ((eventNum < events.Count) && (events[eventNum].measure < measure))
                eventNum++;
            while ((eventNum < events.Count) && (events[eventNum].measure == measure) && (events[eventNum].beat < beat))
                eventNum++;
            return (eventNum < events.Count) ? events[eventNum] : null;
        }

        public List<Event> findMeasureEvents(int measure)
        {
            List<Event> result = new List<Event>();
            int eventNum = 0;
            while ((eventNum < events.Count) && (events[eventNum].measure < measure))
                eventNum++;
            while ((eventNum < events.Count) && (events[eventNum].measure == measure))
            {
                result.Add(events[eventNum++]);
            }
            return result;
        }

//- track saving -------------------------------------------------------------

        public void saveTrack(MidiOutStream stream)
        {
            //List<byte> data = new List<byte>();

            //uint curtime = 0;
            //foreach(Event evt in events) {
            //    uint delta = evt.time - curtime;
            //    curtime = evt.time;
            //    List<byte> vardelta = stream.getVarLenQuantity(delta);
            //    data.AddRange(vardelta);
            //    byte[] msgbytes = evt.msg.getDataBytes();
            //    data.AddRange(msgbytes);
            //}

            ////track header
            //int size = data.Count;
            //stream.putString("MTrk");
            //stream.putFour(size);
            //stream.putData(data.ToArray());
        }

        //public void dump()
        //{
        //    for (int i = 0; i < events.Count; i++)
        //    {
        //        events[i].dump();
        //    }
        //}
    }
}
