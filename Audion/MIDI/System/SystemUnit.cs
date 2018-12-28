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

namespace Transonic.MIDI.System
{
    public abstract class SystemUnit
    {
        public String name;
        public InputDevice inputDev;                    //connect from input device
        public List<OutputDevice> outputDevList;        //connections to output devices

        public SystemUnit(String _name)
        {
            name = _name;
            inputDev = null;
            outputDevList = new List<OutputDevice>();
        }

        //for connection to input devices
        public virtual void receiveMessage(byte[] msg)
        {
        }

        //for connection to output devices
        public virtual void sendMessage(byte[] msg)
        {
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");
