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

namespace Transonic.MIDI
{
    //base event class
    public class Event : IComparable<Event>
    {
        public int tick;
        public int measure;
        public decimal beat;

        public Event()
        {
            tick = 0;
            measure = 0;
            beat = 0;
        }

        public void setTick(int _tick) 
        {
            tick = _tick;
        }

        public void setBeat(int _measure, decimal _beat)
        {
            measure = _measure;
            beat = _beat;
        }

        public int CompareTo(Event other)
        {
            return this.tick.CompareTo(other.tick);
        }
    }

//-----------------------------------------------------------------------------
//  MESSAGE EVENTS
//-----------------------------------------------------------------------------

    public class MessageEvent : Event
    {
        public Message msg;

        public MessageEvent(Message _msg)
            : base()
        {
            msg = _msg;         //midi message
        }
    }

//-----------------------------------------------------------------------------
//  META EVENTS
//-----------------------------------------------------------------------------

    //J Glatt's Midi file page describing defined meta events: http://midi.teragonaudio.com/tech/midifile.htm

    //meta event base class
    public class MetaEvent : Event
    {
        public MetaEvent()
            : base()
        {            
        }
    }

    public class SequenceNumberEvent : MetaEvent    //0xff 0x00
    {
        int val;

        public SequenceNumberEvent(int _val)
            : base()
        {
            val = _val;
        }
    }

    //text events
    public class TextEvent : MetaEvent      //0xff 0x01
    {
        String text;

        public TextEvent(String _text)
            : base()
        {
            text = _text;
        }
    }

    public class CopyrightEvent : MetaEvent     //0xff 0x02
    {
        String copyright;

        public CopyrightEvent(String _copy)
            : base()
        {
            copyright = _copy;
        }
    }

    public class TrackNameEvent : MetaEvent     //0xff 0x03
    {
        public String trackName;

        public TrackNameEvent(String name)
            : base()
        {
            trackName = name;
        }
    }

    public class InstrumentEvent : MetaEvent    //0xff 0x04
    {
        public String instrumentName;

        public InstrumentEvent(String name)
            : base()
        {
            instrumentName = name;
        }
    }

    public class LyricEvent : MetaEvent     //0xff 0x05
    {
        public String lyric;

        public LyricEvent(String _lyric)
            : base()
        {
            lyric = _lyric;
        }
    }

    public class MarkerEvent : MetaEvent        //0xff 0x06
    {
        public String marker;

        public MarkerEvent(String _marker)
            : base()
        {
            marker = _marker;
        }
    }

    public class CuePointEvent : MetaEvent      //0xff 0x07
    {
        public String cuePoint;

        public CuePointEvent(String cue)
            : base()
        {
            cuePoint = cue;
        }
    }

    public class PatchNameEvent : MetaEvent        //0xff 0x08
    {
        public String patchName;

        public PatchNameEvent(String name)
            : base()
        {
            patchName = name;
        }
    }

    public class DeviceNameEvent : MetaEvent        //0xff 0x09
    {
        public String deviceName;

        public DeviceNameEvent(String name)
            : base()
        {
            deviceName = name;
        }
    }

    //obsolete
    public class MidiChannelEvent : MetaEvent       //0xff 0x20
    {
        int channelNum;

        public MidiChannelEvent(int cc)
            : base()
        {
            channelNum = cc;
        }
    }

    //obsolete
    public class MidiPortEvent : MetaEvent          //0xff 0x21
    {
        int portNum;

        public MidiPortEvent(int pp)
            : base()
        {
            portNum = pp;
        }
    }

    //end of track
    public class EndofTrackEvent : MetaEvent        //0xff 0x2f
    {

        public EndofTrackEvent()
            : base()
        {
            //length should be 0
        }

        public override string ToString()
        {
            return "End of Track";
        }
    }

    //timing events
    public class TempoEvent : MetaEvent             //0xff 0x51
    {
        public int tempo;        

        public TempoEvent(int _tempo)
            : base()
        {
            tempo = _tempo;            
        }

        public override string ToString()
        {
            return "Tempo = " + tempo;
        }
    }

    public class SMPTEOffsetEvent : MetaEvent       //0xff 0x54
    {
        int frameRate, hour, min, sec, frame, frame100;

        public SMPTEOffsetEvent(int rr, int hh, int mn, int se, int fr, int ff)
            : base()
        {
            frameRate = rr;
            hour = hh;
            min = mn;
            sec = se;
            frame = fr;
            frame100 = ff;
        }
    }

    public class TimeSignatureEvent : MetaEvent         //0xff 0x58
    {
        public int numer;
        public int denom;
        public int clicks;
        public int clocksPerQuarter;

        public TimeSignatureEvent(int nn, int dd, int cc, int bb)
            : base()
        {
            numer = nn;
            denom = dd;
            clicks = cc;
            clocksPerQuarter = bb;
        }

        public override string ToString()
        {
            return "Time Signature = " + numer + "/" + denom + " clicks = " + clicks + " clocks/quarter = " + clocksPerQuarter;
        }
    }

    public class KeySignatureEvent : MetaEvent          //0xff 0x59
    {
        public int keySig;
        public bool minor;

        public KeySignatureEvent(int sf, int mi)
            : base()
        {
            keySig = sf;
            minor = (mi == 1);
        }
    }

    public class ProprietaryEvent : MetaEvent          //0xff 0x7f
    {
        List<byte> data;

        public ProprietaryEvent(List<byte> _data)
            : base()
        {
            data = _data;            
        }
    }
}
