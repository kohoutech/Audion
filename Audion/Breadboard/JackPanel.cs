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

using Kohoutech.Patch;

namespace Audion.Breadboard
{
    //a module panel that has a connection jack
    public class JackPanel : ModulePanel
    {
        public enum DIRECTION
        {
            IN,
            OUT
        }

        public string tag;
        public DIRECTION dir;
        public List<PatchCord> outs;

        public JackPanel(Module _module, String name, JackPanel.DIRECTION _dir, string _tag) : base(_module, name)
        {
            tag = _tag;
            dir = _dir;
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

        //jack panel can either be a DEST or a SOURCE (but never a NEITHER)
        public override PatchPanel.CONNECTIONTYPE getConnType()
        {
            return (dir == DIRECTION.IN) ? PatchPanel.CONNECTIONTYPE.DEST : PatchPanel.CONNECTIONTYPE.SOURCE;
        }

        public override Point connectionPoint()
        {
            int h = height / 2;
            int w = (dir == DIRECTION.IN) ? 0 : width;
            return new Point(w, h);
        }

        //only allow connections between panels with matching types!
        public override bool canConnectIn(IPatchPanel source)
        {
            return ((JackPanel)source).tag.Equals(tag);
        }

        public override bool canTrackMouse()
        {
            return (dir == DIRECTION.OUT);
        }

        public override void paint(Graphics g, Rectangle frame)
        {
            //jack name & pos
            Font nameFont = SystemFonts.DefaultFont;
            SizeF nameSize = g.MeasureString(name, nameFont);
            int centerY = frame.Top + (height / 2);
            int nameY = centerY - ((int)nameSize.Height / 2);

            if (dir == DIRECTION.IN)
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

    //-------------------------------------------------------------------------

    public class AudioPanel : JackPanel
    {
        public AudioPanel(Module _module, String name, DIRECTION dir) : base(_module, name, dir, "Audio")
        {            
        }
    }

    //-------------------------------------------------------------------------

    //a jack panel for controlling a module's parameters
    //all param panels have in jacks
    public class ParamPanel : JackPanel
    {
        public ModuleParameter parm;

        public ParamPanel(Module _module, ModuleParameter _parm) : base(_module, _parm.name, DIRECTION.IN, _parm.getParamType())
        {
            parm = _parm;
        }
    }
}
