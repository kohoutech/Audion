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
using Audion.Breadboard;

namespace Audion.UI
{
    public class KnobControl : PatchBox
    {
        Module module;
        KnobPanel panel;

        public KnobControl(Module _module)
            : base()
        {
            module = _module;
            title = "Knob";
            addPanel(new ModulePanel(this, module.jacks[0]), false);
            panel = new KnobPanel(this);
            this.addPanel(panel, false);
        }
    }

    public class KnobPanel : PatchPanel {

        const int KNOBSIZE = 30;
        Rectangle knobRect;
        PointF centerPt;
        double val;
        int orgY;

        public KnobPanel(PatchBox box)
            : base(box)
        {
            panelType = "KnobPanel";
            connType = CONNECTIONTYPE.NEITHER;
            updateFrame(KNOBSIZE + 10 + 10, frameWidth);
            knobRect = new Rectangle(frame.Left + ((frame.Width - KNOBSIZE) / 2), frame.Top + 10, KNOBSIZE, KNOBSIZE);
            centerPt = new PointF(knobRect.Left + knobRect.Height / 2, knobRect.Top + knobRect.Width / 2);
            val = 0.0;
        }

        public override void setPos(int xOfs, int yOfs)
        {
            base.setPos(xOfs, yOfs);
            knobRect.Offset(xOfs, yOfs);
            centerPt = new PointF(knobRect.Left + knobRect.Height / 2, knobRect.Top + knobRect.Width / 2);
        }

        public override bool canTrackMouse()
        {
            return true;
        }

        public override void onMouseDown(Point pos)
        {
            orgY = pos.Y;   
        }

        public override void onMouseMove(Point pos)
        {
            double delta = (pos.Y - orgY) / 100.0;
            val -= delta;
            val = (val > 1.0) ? 1.0 : (val < 0.0) ? 0.0 : val;
            orgY = pos.Y;              
        }

        public override void onMouseUp(Point pos)
        {
            
        }

        public override void paint(Graphics g)
        {
            base.paint(g);

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
