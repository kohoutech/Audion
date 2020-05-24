/* ----------------------------------------------------------------------------
Audion : a audio plugin creator
Copyright (C) 2011-2020  George E Greaney

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

using Kohoutech.Patch;

namespace Audion.Breadboard
{
    public class PatchCord : IPatchWire
    {
        public AudionPatch patch;
        public JackPanel source;
        public JackPanel dest;

        public PatchCord(AudionPatch _patch, JackPanel _source, JackPanel _dest)
        {
            patch = _patch;
            source = _source;
            dest = _dest;
            source.connect(this);
        }

        public void disconnect()
        {
            source.disconnect(this);
            source = null;
            dest = null;
        }

        public void doubleClick()
        {            
        }

        public void rightClick()
        {            
        }
    }
}
