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

using Transonic.MIDI;
using Transonic.MIDI.System;

namespace Transonic.MIDI.Engine
{
    public class Transport
    {
        IMidiView window;           //for updating the UI        

        Sequence seq;
        TempoMap tempoMap;
        MeterMap meterMap;
        int division;
        int trackCount;

        MidiTimer timer;
        long startTime;         //time we started playing to calc num of ticks that have elasped since then (in 0.1 microsecs)
        long startOffset;
        long tickLen;          //length of one tick in fractions of 0.1 microsec (100 nanosec)
        long tickTime;         //cumulative tick time
        public int tickNum;    //cur tick number

        int[] trackPos;        //pos of the next event in each track
        int tempoPos;          //pos of next tempo event
        Tempo curTempo;
        int meterPos;
        Meter curMeter;

        bool isPlaying;
        float playbackSpeed;

        public Transport(IMidiView _window)
        {
            window = _window;            
            seq = null;

            timer = new MidiTimer();
            timer.Timer += new EventHandler(OnPulse);
            isPlaying = false;
            playbackSpeed = 1.0f;            
        }

        public void setSequence(Sequence _seq)
        {
            seq = _seq;
            tempoMap = seq.tempoMap;
            meterMap = seq.meterMap;
            division = seq.division;
            trackCount = seq.tracks.Count;
            trackPos = new int[trackCount];
            rewind();
        }

        //tempo is len of quarter note in microsecs, division is number of ticks / quarter note
        public void setTempo(Tempo tempo)
        {
            curTempo = tempo;
            tickLen = (long)((curTempo.rate / (division * playbackSpeed)) * 10.0f);     //len of each tick in 0.1 usecs (or 100 nanosecs)
            window.tempoChange(curTempo.rate);
        }

        //tempo is len of quarter note in microsecs, division is number of ticks / quarter note
        public void setMeter(Meter meter)
        {
            curMeter = meter;
            window.meterChange(curMeter.numer, curMeter.denom, curMeter.keysig);
        }


//- operation methods ---------------------------------------------------------

        public void init()
        {            
        }

        public void shutdown()
        {
            if (isPlaying)
            {
                stop();
            }
        }

        public void play()
        {
            if (!isPlaying)
            {
                //set start time so elasped time is the same as when we stopped playing, if first time, ofs = 0
                startTime = DateTime.Now.Ticks - startOffset;       

                timer.start(1);               //timer interval = 1 msec
                isPlaying = true;
            }
        }

        public void record()
        {
        }

        public void stop()
        {
            if (isPlaying)
            {
                long now = DateTime.Now.Ticks;          //start offset is the elapsed time from start to stopping
                startOffset = (now - startTime);        //so timer will be correct when we restart this up again
                timer.stop();
                seq.allNotesOff();
                isPlaying = false;
            }
        }

        public void rewind()
        {
            if (!isPlaying)
            {
                //set initial tempo
                tempoPos = 0;
                setTempo(tempoMap.tempos[tempoPos++]);

                meterPos = 0;
                setMeter(meterMap.meters[meterPos++]);

                tickNum = 0;
                tickTime = tickLen;                    //time of first tick (not 0 - that would be no ticks)

                for (int i = 1; i < trackPos.Length; i++)
                {
                    trackPos[i] = 0;
                }
                startOffset = 0;                        //at the beginning of seq
            }
        }

        public void sequenceDone()
        {
            timer.stop();
            seq.allNotesOff();
            isPlaying = false;
            window.sequenceDone();
        }

        public void setPlaybackSpeed(bool on)
        {
            playbackSpeed = on ? 0.5f : 1.0f;
            setTempo(curTempo);
        }

        public int getCurrentPos()
        {
            return tickNum;
        }

        //set cur pos in sequence to a specific tick, only if stoppped
        public void setCurrentPos(int _ticknum)
        {
            if (!isPlaying)
            {
                tickNum = _ticknum;
                Tempo tempo = tempoMap.findTempo(tickNum, out tempoPos);
                setTempo(tempo);

                int tickOfs = _ticknum - curTempo.tick;                //num of ticks from prev tempo msg to now
                tickTime = (curTempo.time * 10L) + (tickOfs * tickLen);      //prev tempo's time (in usec/10) + time of ticks to now

                startOffset = tickTime;                             //the elapsed time since seq start
                startTime = DateTime.Now.Ticks - startOffset;       //start time is when we would have started playing up to now

                Meter meter = meterMap.findMeter(tickNum, out meterPos);
                setMeter(meter);

                //set cur pos in each track
                //find and send most recent patch change message for each track
                for (int trackNum = 0; trackNum < trackCount; trackNum++)
                {
                    Track track = seq.tracks[trackNum];
                    List<Event> events = seq.tracks[trackNum].events;
                    trackPos[trackNum] = 0;
                    PatchChangeMessage patchmsg = null;
                    for (int i = 0; i < events.Count; i++)
                    {
                        Event evt = events[i];
                        if (evt is MessageEvent && (((MessageEvent)evt).msg is PatchChangeMessage))
                        {
                            patchmsg = (PatchChangeMessage)((MessageEvent)evt).msg;
                        }
                        if (events[i].tick >= _ticknum)
                            break;
                        trackPos[trackNum]++;
                    }
                    if (patchmsg != null)
                    {
                        track.sendMessage(patchmsg);
                    }
                }
            }
        }

        public int getCurrentTime()
        {
            return (int)(tickTime / 10000L);            //ret tick time in msec
        }

        public void getCurrentBeat(out int measure, out int beat, out int ticks)
        {
            int beatticks = (division * 4) / curMeter.denom;
            int measureticks = (beatticks * curMeter.numer);

            ticks = tickNum - curMeter.tick;
            measure = curMeter.measure + (ticks / measureticks);
            ticks = ticks % measureticks;
            beat = (ticks / beatticks) + 1;
            ticks = ticks % beatticks;
        }

//- timer method --------------------------------------------------------------

        //when transport is playing, this will be called approx every millisec
        //one or more ticks may have happened during this time, so we get the current time in 0.1 MICROSECs
        //and calc the running time since start up & compare with the time of the next tick
        //if we've passed it, then inc tick number and calc time of the following tick
        //and then handle all the messages on every track for the tick that we just passed
        //keep doing this until we're caught up to the current tick number (ie its ahead of where we are now)
        //since 1 ms < threshold of simultaneity (~2 - 5 ms), all events should sound like there are happening at the same time
        private void OnPulse(object sender, EventArgs e)
        {
            long now = DateTime.Now.Ticks;          //one system's clock tick = 0.1 microsec
            long runningTime = (now - startTime);

            while (runningTime > tickTime)          //we've passed one or more ticks
            {
                //handle tempo msgs
                if ((tempoPos < tempoMap.count) && (tickNum >= tempoMap.tempos[tempoPos].tick))
                {
                    Tempo tempo = tempoMap.tempos[tempoPos];
                    setTempo(tempo);
                    tempoPos++;           
                }

                //handle meter msgs
                if ((meterPos < meterMap.count) && (tickNum >= meterMap.meters[meterPos].tick))
                {
                    Meter meter = meterMap.meters[meterPos];
                    setMeter(meter);
                    meterPos++;
                }

                //handle track events for each track
                //alldone will be true when we've reached the end of every track
                for (int trackNum = 0; trackNum < trackCount; trackNum++)
                {
                    Track track = seq.tracks[trackNum];
                    if (track.outDev != null && !track.muted)       //check if track is outputting
                    {
                        List<Event> events = track.events;
                        bool done = (trackPos[trackNum] >= events.Count);

                        while (!done && (tickNum >= events[trackPos[trackNum]].tick))
                        {
                            Event evt = events[trackPos[trackNum]];             //get next event for this track

                            if (!(evt is MetaEvent))
                            {
                                Message msg = ((MessageEvent)evt).msg;      //get event's midi message
                                track.sendMessage(msg);                     //send it out                            
                                window.handleMessage(trackNum, msg);        //and update the UI 
                            }

                            trackPos[trackNum]++;
                            done = (trackPos[trackNum] >= events.Count);
                        }                                                
                    }
                }

                tickNum++;                          //update tick number
                tickTime = tickTime + tickLen;      //and get time of next tick

                if (tickNum > seq.length)
                {
                    sequenceDone();
                }
            }
        }
    }

//- UI interface --------------------------------------------------------------

    //for communication with the UI
    public interface IMidiView
    {
        //for passing note on & off msgs to the keyboard bar
        void handleMessage(int track, Transonic.MIDI.Message message);

        void sequenceDone();

        void tempoChange(int rate);
        void meterChange(int numer, int denom, int key);
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");
