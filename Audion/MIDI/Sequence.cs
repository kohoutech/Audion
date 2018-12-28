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
    public class Sequence
    {
        public const int DEFAULTDIVISION = 96;         //ticks / quarter note
        public int division;                           //ppq - ticks (pulses) / quarter note

        public int length;                  //total length in ticks
        public int measures;                //num of measures in longest track

        public List<Track> tracks;
        public TempoMap tempoMap;
        public MeterMap meterMap;
        public MarkerMap markerMap;

        public Sequence() : this(DEFAULTDIVISION) { }

        public Sequence(int _division)
        {
            division = _division;
            length = 0;
            measures = 0;

            tracks = new List<Track>();
            tempoMap = new TempoMap(this);
            meterMap = new MeterMap(this);
            markerMap = new MarkerMap(this);            
        }

        public Track addTrack()
        {
            Track track = new Track(this);
            tracks.Add(track);
            return track;
        }

        public void deleteTrack(Track track)
        {
            tracks.Remove(track);            
        }

        //public void dump()
        //{
        //    for (int i = 0; i < tracks.Count; i++)
        //    {
        //        Console.WriteLine("contents of track[{0}]", i);
        //        tracks[i].dump();
        //    }
        //}

        public void allNotesOff()
        {
            for (int trackNum = 1; trackNum < tracks.Count; trackNum++)
            {
                tracks[trackNum].allNotesOff();
            }
        }
    }

}
