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
using System.Drawing;

using Kohoutech.Patch;
using Audion.Breadboard;

namespace Audion.UI
{
    public class KnobControl : Module, IPatchBox
    {
        public KnobControl() : base("Knob")
        {
            defname = "Knob";
            JackPanel jack = new JackPanel(this, "Out", JackPanel.DIRECTION.OUT, "Range");
            addPanel(jack);
            KnobPanel knob = new KnobPanel(this);
            addPanel(knob);
        }
    }

    //-------------------------------------------------------------------------

    public class KnobPanel : ModulePanel, IPatchPanel
    {

        const int KNOBSIZE = 30;
        double val;
        int orgY;

        public KnobPanel(KnobControl module) : base(module, "Knob")
        {
            height = AudionPatch.PANELHEIGHT * 2;       //2U height
            val = 0.0;
            orgY = 0;
        }

        public bool canTrackMouse()
        {
            return true;
        }

        public void mouseDown(Point pos)
        {
            orgY = pos.Y;   
        }

        public void mouseMove(Point pos)
        {
            double delta = (pos.Y - orgY) / 100.0;
            val -= delta;
            val = (val > 1.0) ? 1.0 : (val < 0.0) ? 0.0 : val;
            orgY = pos.Y;              
        }

        public void mouseUp(Point pos)
        {            
        }

        public void paint(Graphics g, Rectangle frame)
        {
            Rectangle knobRect = new Rectangle(frame.Left + ((frame.Width - KNOBSIZE) / 2), frame.Top + ((frame.Height - KNOBSIZE) / 2), KNOBSIZE, KNOBSIZE);
            PointF centerPt = new PointF(knobRect.Left + knobRect.Height / 2, knobRect.Top + knobRect.Width / 2);

            Pen knobPen = new Pen(Color.Black, 3.0f);
            g.DrawEllipse(knobPen, knobRect);

            double angle = (225 - (val * 270)) * (Math.PI / 180);
            float ptx = (float)(Math.Cos(angle) * KNOBSIZE / 2);
            float pty = (float)(Math.Sin(angle) * KNOBSIZE / 2);
            PointF edgePt = new PointF(centerPt.X + ptx, centerPt.Y - pty);
            g.DrawLine(knobPen, edgePt, centerPt);
        }

    }
}
