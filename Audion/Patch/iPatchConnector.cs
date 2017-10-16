/* ----------------------------------------------------------------------------
Transonic Patch Library
Copyright (C) 1995-2017  George E Greaney

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
using System.Drawing;
using System.Xml;

//the model's data connector implements this interface so that the user events
//on the patch line get passed back to the connector; subclassing the patch line
//would mean the subclass would have to inherit from both this lib AND the model
//which is why we use this interface instead

namespace Transonic.Patch
{
    public interface iPatchConnector
    {
        void setLine(PatchLine line);       //link the patch line view to the model's connector

        //single clicks select the patch line, so they aren't passed to the model's connector

        void onDoubleClick(Point pos);

        void onRightClick(Point pos);
    }
}
