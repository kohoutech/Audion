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
using Transonic.MIDI.System;

namespace Transonic.MIDI.Engine
{
    public class MidiTimer
    {
        //timer callback delegate
        delegate void TimeProc(uint uID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

        //p/invoke imports
        [DllImport("winmm.dll", SetLastError = true)]
        static extern uint timeSetEvent(uint uDelay, uint uResolution, TimeProc lpTimeProc, UIntPtr dwUser, uint fuEvent);

        [DllImport("winmm.dll", SetLastError = true)]
        static extern MMRESULT timeKillEvent(uint uTimerID);

        //const vals from mmsystem.h
        const int TIME_ONESHOT = 0;
        const int TIME_PERIODIC = 1;
        const int TIME_CALLBACK_FUNCTION = 0;
        const int TIME_CALLBACK_EVENT_SET = 16;
        const int TIME_CALLBACK_EVENT_PULSE = 32;

//-----------------------------------------------------------------------------

        uint id = 0;                    //timer id
        TimeProc timerProc;             //timer callback func        

        public MidiTimer()
        {
            timerProc = timerCallback;          //set callback func
        }

        public void start(uint msec)
        {
            stop();         //stop prev timer

            id = timeSetEvent(msec, 0, timerProc, UIntPtr.Zero, (uint)(TIME_CALLBACK_FUNCTION | TIME_PERIODIC));
            if (id == 0)
                throw new Exception("timeSetEvent error");
            //Console.WriteLine("started timer " + id.ToString());
        }

        public void stop()
        {
            if (id != 0)
            {
                timeKillEvent(id);
                //Console.WriteLine("stopped timer " + id.ToString());
                id = 0;
            }
        }

// timer event & callback func ------------------------------------------------

        public event EventHandler Timer;

        protected virtual void OnTimer(EventArgs e)
        {
            if (Timer != null)
                Timer(this, e);
        }

        void timerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            OnTimer(new EventArgs());
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");
