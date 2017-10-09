/* ----------------------------------------------------------------------------
Audion : a audio plugin creator
Copyright (C) 2011-2017  George E Greaney

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

using Transonic.Patch;
using Audion.Graph;

namespace Audion.UI
{
    public class ModulePanel : PatchPanel
    {
        String name;

        public ModulePanel(PatchBox _box, ModuleJack modParam)
            : base(_box)
        {
            panelType = "ModulePanel";
            name = modParam.name;
            connType = (modParam.dir == ModuleJack.DIRECTION.OUT) ? CONNECTIONTYPE.SOURCE : CONNECTIONTYPE.DEST;
        }

        public override Point ConnectionPoint
        {
            get
            {
                Point p = base.ConnectionPoint;
                if (connType == CONNECTIONTYPE.SOURCE)
                {
                    p = new Point(frame.Right, frame.Top + frame.Height / 2);
                }
                if (connType == CONNECTIONTYPE.DEST)
                {
                    p = new Point(frame.Left, frame.Top + frame.Height / 2);
                }
                return p;
            }
        }

        public override void paint(Graphics g)
        {
            base.paint(g);

            //jack name
            Font nameFont = SystemFonts.DefaultFont;
            SizeF nameSize = g.MeasureString(name, nameFont);
            int centerY = frame.Top + (frameHeight / 2);
            int nameY = centerY - ((int)nameSize.Height / 2);
            if (connType == CONNECTIONTYPE.SOURCE)
            {
                g.DrawLine(Pens.Black, frame.Right - 5, centerY, frame.Right, centerY);
                g.DrawString(name, nameFont, Brushes.Black, frame.Right - nameSize.Width - 7, nameY);                
            }
            if (connType == CONNECTIONTYPE.DEST)
            {
                g.DrawLine(Pens.Black, frame.Left, centerY, frame.Left + 5, centerY);
                g.DrawString(name, nameFont, Brushes.Black, frame.Left + 7, nameY);
            }            
        }
    }
}
