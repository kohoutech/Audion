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
using System.Drawing;
using System.Linq;
using System.Text;

using Transonic.Patch;

namespace Audion.Breadboard
{
    //base class for all audion module panels
    public class ModulePanel : IPatchPanel
    {
        public Module module;
        public String name;
        public int width;
        public int height;

        public ModulePanel(Module _module, String _name)
        {
            module = _module;
            name = _name;
            width = AudionPatch.MODULEWIDTH;
            height = AudionPatch.PANELHEIGHT;
        }

        public string getName()
        {
            return name;
        }

        public int getHeight()
        {
            return height;
        }

        public PatchPanel.CONNECTIONTYPE getConnType()
        {
            return PatchPanel.CONNECTIONTYPE.NEITHER;
        }

        //dummy value - since connection type is set to NEITHER, 
        //the patch canvas won't allow the user connect to it
        public Point connectionPoint()
        {
            return new Point(0, 0);         
        }

        //response to mouse clicks by default, instead of down / move / up events
        public bool canTrackMouse()
        {
            return false;
        }

        public void click(Point pos)
        {            
        }

        public void rightClick(Point pos)
        {            
        }
        
        public void doubleClick(Point pos)
        {            
        }

        public void mouseDown(Point pos)
        {            
        }

        public void mouseMove(Point pos)
        {            
        }

        public void mouseUp(Point pos)
        {            
        }

        public void paint(Graphics g, Rectangle frame)
        {
            
        }
    }
}
