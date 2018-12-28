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
    public class MeterMap
    {
        public Sequence seq;
        public List<Meter> meters;
        public int count;

        public MeterMap(Sequence _seq)
        {
            seq = _seq;
            meters = new List<Meter>();
            Meter meter = new Meter(0, 4, 4, 0);            //def meter = 4/4, key of C
            meters.Add(meter);
            count = 1;
        }

        public void addMeter(Meter meter)
        {
            //insert the meter into the meter list
            int pos = 0;
            if (meter.tick > meters[meters.Count - 1].tick)         //if meter goes at end of list
            {
                meters.Add(meter);
                pos = meters.Count - 1;
            }
            else
            {
                while ((pos < meters.Count) && (meter.tick > meters[pos].tick))
                {
                    pos++;
                }
                if (meter.tick == meters[pos].tick)     //if we already have a meter change at this tick, replace it
                {
                    meters[pos] = meter;
                }
                else
                {
                    meters.Insert(pos, meter);          //else insert new meter change into list at this pos
                }
            }
            count = meters.Count;
            calcMeterMap(pos);
        }

        public void calcMeterMap(int pos)           //calc time of each tempo change from this tempo to tempo list end
        {
            for (int i = pos; i < meters.Count; i++)
            {
                if (i == 0)
                {
                    meters[i].measure = 0;
                }
                else
                {
                    Meter prev = meters[i - 1];
                    int delta = meters[i].tick - prev.tick;                         //amount of ticks from prev meter to this one
                    double quarts = (((float)delta) / seq.division);
                    double deltameas = ((quarts * prev.denom / 4.0) / prev.numer);
                    meters[i].measure = prev.measure + (int)deltameas;            
                }
            }
        }

        //find nearest meter change before this tick
        public Meter findMeter(int tick, out int meterPos)
        {
            meterPos = 0;
            while ((meterPos < meters.Count) && (meters[meterPos].tick <= tick))
            {
                meterPos++;
            }
            meterPos--;                     //we passed it, or at end of list; back up one tempo
            return meters[meterPos];
        }

        public void tickToBeat(int tick, out int measure, out decimal beat)
        {
            int meterPos;
            Meter meter = findMeter(tick, out meterPos);

            int measureticks = (int)(seq.division * ((float)meter.numer * 4 / meter.denom));

            int ticks = tick - meter.tick;
            measure = meter.measure + (ticks / measureticks);
            ticks = ticks % measureticks;
            beat = ((decimal)ticks / seq.division);
        }
    }

//-----------------------------------------------------------------------------

    public class Meter
    {
        public int tick;        //tick which the meter change occurs at
        public int numer;
        public int denom;
        public int keysig;
        public int measure;

        public Meter(int _tick, int _numer, int _denom, int _keysig)
        {
            tick = _tick;
            numer = _numer;
            denom = _denom;
            keysig = _keysig;
            measure = 0;
        }
    }
}
