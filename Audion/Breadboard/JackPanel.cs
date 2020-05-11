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
    public class JackPanel : ModulePanel, IPatchPanel
    {
        public ModuleParameter parm;
        public ModuleParameter.DIRECTION dir;
        public List<PatchCord> outs;

        public JackPanel(Module _module, ModuleParameter _parm) : base(_module, _parm.name)
        {
            parm = _parm;
            dir = parm.dir;
            outs = new List<PatchCord>();
        }

        public virtual void connect(PatchCord cord)
        {
            outs.Add(cord);
        }

        public virtual void disconnect(PatchCord cord)
        {
            outs.Remove(cord);
        }

        public PatchPanel.CONNECTIONTYPE getConnType()
        {
            return (dir == ModuleParameter.DIRECTION.IN) ? PatchPanel.CONNECTIONTYPE.DEST : PatchPanel.CONNECTIONTYPE.SOURCE;
        }

        public Point connectionPoint()
        {
            int h = height / 2;
            int w = (dir == ModuleParameter.DIRECTION.IN) ? 0 : width;
            return new Point(w, h);
        }

        public bool canTrackMouse()
        {
            return (dir == ModuleParameter.DIRECTION.OUT);
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

        public void click(Point pos)
        {           
        }

        public void rightClick(Point pos)
        {           
        }

        public void doubleClick(Point pos)
        {            
        }

        public void paint(Graphics g, Rectangle frame)
        {
            //jack name & pos
            Font nameFont = SystemFonts.DefaultFont;
            SizeF nameSize = g.MeasureString(name, nameFont);
            int centerY = frame.Top + (height / 2);
            int nameY = centerY - ((int)nameSize.Height / 2);

            if (dir == ModuleParameter.DIRECTION.IN)
            {
                g.DrawLine(Pens.Black, frame.Left, centerY, frame.Left + 5, centerY);
                g.DrawString(name, nameFont, Brushes.Black, frame.Left + 7, nameY);
            }
            else 
            {
                g.DrawLine(Pens.Black, frame.Right - 5, centerY, frame.Right, centerY);
                g.DrawString(name, nameFont, Brushes.Black, frame.Right - nameSize.Width - 7, nameY);
            }
        }
    }
}
