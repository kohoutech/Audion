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
    public class TempoMap
    {
        public const int DEFAULTTEMPO = 500000;         //microsec / quarter note = 120 bpm

        public Sequence seq;
        public List<Tempo> tempos;
        public int count;

        public TempoMap(Sequence _seq)
        {
            seq = _seq;
            tempos = new List<Tempo>();
            Tempo tempo = new Tempo(0, DEFAULTTEMPO);
            tempos.Add(tempo);
            count = 1;
        }

        public void addTempo(Tempo tempo)
        {
            //insert the tempo into the tempo list
            int pos = 0;
            if (tempo.tick > tempos[tempos.Count - 1].tick)         //if tempo goes at end of list
            {
                tempos.Add(tempo);
                pos = tempos.Count - 1;
            }
            else
            {
                while ((pos < tempos.Count) && (tempo.tick > tempos[pos].tick))
                {
                    pos++;
                }
                if (tempo.tick == tempos[pos].tick)     //if we already have a temp change at this tick, replace it
                {
                    tempos[pos] = tempo;
                }
                else
                {
                    tempos.Insert(pos, tempo);          //else insert new tempo change into list at this pos
                }
            }
            count = tempos.Count;
            calcTempoMap(pos);
        }

        public void calcTempoMap(int pos)           //calc time of each tempo change from this tempo to tempo list end
        {
            for (int i = pos; i < tempos.Count; i++)
            {
                if (i == 0)
                {
                    tempos[i].time = 0;
                }
                else
                {
                    Tempo prev = tempos[i - 1];
                    int delta = tempos[i].tick - prev.tick;                         //amount of ticks from prev tempo to this one
                    double deltatime = (((double)delta) / seq.division) * prev.rate;
                    tempos[i].time = prev.time + (int)deltatime;                    //calc time in microsec of this tempo event
                }
            }
        }

        //find nearest tempo change before this tick
        public Tempo findTempo(int tick, out int tempoPos)
        {
            tempoPos = 0;
            while ((tempoPos < tempos.Count) && (tempos[tempoPos].tick <= tick))
            {
                tempoPos++;
            }
            tempoPos--;                     //we passed it, or at end of list; back up one tempo
            return tempos[tempoPos];
        }
    }

//-----------------------------------------------------------------------------

    //maps a tempo message or a time signature message to a elapsed time, so if move the cur pos
    //in a sequence, we can calculate what time that is; needs to be recalculated any time tempo or time sig change
    public class Tempo
    {
        public int tick;        //tick which the tempo change occurs at
        public int rate;        //num of microsec / tick
        public long time;       //cumulative time from sequence start to this tempo change, in microsec

        public Tempo(int _tick, int _rate)
        {
            tick = _tick;
            rate = _rate;
            time = 0;
        }
    }
}
