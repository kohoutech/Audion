/* ----------------------------------------------------------------------------
Transonic Patch Library
Copyright (C) 1995-2019  George E Greaney

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

namespace Transonic.Patch
{
    public class PatchPanel
    {
        public enum CONNECTIONTYPE 
        {
            SOURCE,
            DEST,
            NEITHER
        }

        public String panelName;

        public PatchBox patchbox;
        public List<PatchWire> wires;
        public CONNECTIONTYPE connType;

        public Rectangle frame;
        public int frameWidth;
        public int frameHeight;

        public PatchPanel(PatchBox box, String _panelName)
        {
            patchbox = box;
            panelName = _panelName;

            updateFrame(patchbox.frame.Width, 20);      //default frame size

            wires = new List<PatchWire>();
            connType = CONNECTIONTYPE.NEITHER;
        }

        public void updateFrame(int width, int height)
        {
            frameWidth = width;
            frameHeight = height;
            frame = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        public virtual void setPos(int xOfs, int yOfs)
        {
            frame.Offset(xOfs, yOfs);
            foreach(PatchWire connector in wires)
            {
                if (connType == CONNECTIONTYPE.SOURCE)
                {
                    connector.SourceEndPos = this.ConnectionPoint;
                }
                if (connType == CONNECTIONTYPE.DEST)
                {
                    connector.DestEndPos = this.ConnectionPoint;
                }
            }
        }

        public virtual bool hitTest(Point p)
        {
            return (frame.Contains(p));
        }

//- connections ---------------------------------------------------------------

        public bool canConnectIn()
        {
            return (connType == CONNECTIONTYPE.DEST);
        }

        public bool canConnectOut()
        {
            return (connType == CONNECTIONTYPE.SOURCE);
        }

        //default connection point - dead center of the frame
        public virtual Point ConnectionPoint
        {
            get { return new Point(frame.Left + frame.Width / 2, frame.Top + frame.Height / 2); }
        }

        public virtual void connectLine(PatchWire line)
        {
            wires.Add(line);
        }

        public virtual void disconnectLine(PatchWire line)
        {
            wires.Remove(line);                
        }

        public bool isConnected()
        {
            return (wires.Count != 0);
        }

//- user input ----------------------------------------------------------------

        public virtual bool canTrackMouse()
        {
            return false;
        }

        public virtual void onMouseDown(Point pos)
        {
        }
        
        public virtual void onMouseMove(Point pos)
        {
        }
        
        public virtual void onMouseUp(Point pos)
        {
        }

        public virtual void onClick(Point pos)
        {
        }

        public virtual void onDoubleClick(Point pos)
        {
        }

        public virtual void onRightClick(Point pos)
        {
        }

//- painting ------------------------------------------------------------------

        public virtual void paint(Graphics g)
        {
            g.DrawRectangle(Pens.Black, frame);
        }
    }

}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
