/* ----------------------------------------------------------------------------
Transonic Patch Library
Copyright (C) 1995-2020  George E Greaney

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
        public Point connectionPoint;

        public Rectangle frame;
        public int frameWidth;
        public int frameHeight;

        public iPatchPanel model;

        public PatchPanel(PatchBox box, iPatchPanel _model)
        {
            patchbox = box;
            model = _model;
            panelName = model.getName();

            updateFrame(patchbox.frame.Width, model.getHeight());

            wires = new List<PatchWire>();
            connType = model.getConnType();
            connectionPoint = model.connectionPoint();
        }

        public void updateFrame(int width, int height)
        {
            frameWidth = width;
            frameHeight = height;
            frame = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        public void setPos(int xOfs, int yOfs)
        {
            frame.Offset(xOfs, yOfs);
            foreach(PatchWire connector in wires)
            {
                if (connType == CONNECTIONTYPE.SOURCE)
                {
                    connector.SourceEndPos = this.ConnectionPoint();
                }
                if (connType == CONNECTIONTYPE.DEST)
                {
                    connector.DestEndPos = this.ConnectionPoint();
                }
            }
        }

        public bool hitTest(Point p)
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
        public Point ConnectionPoint()
        {
            return connectionPoint;
        }

        public void connectLine(PatchWire line)
        {
            wires.Add(line);
        }

        public void disconnectLine(PatchWire line)
        {
            wires.Remove(line);                
        }

        public bool isConnected()
        {
            return (wires.Count != 0);
        }

//- user input ----------------------------------------------------------------

        public bool canTrackMouse()
        {
            return model.canTrackMouse();
        }

        public void onMouseDown(Point pos)
        {
            model.mouseDown(pos);
        }
        
        public void onMouseMove(Point pos)
        {
            model.mouseMove(pos);
        }
        
        public void onMouseUp(Point pos)
        {
            model.mouseUp(pos);
        }

        public void onClick(Point pos)
        {
            model.click(pos);
        }

        public void onDoubleClick(Point pos)
        {
            model.doubleClick(pos);
        }

        public void onRightClick(Point pos)
        {
            model.rightClick(pos);
        }

//- painting ------------------------------------------------------------------

        public void paint(Graphics g)
        {
            g.DrawRectangle(Pens.Black, frame);
            model.paint(g);
        }
    }

    //- model interface -------------------------------------------------------

    public interface iPatchPanel
    {
        //get panel's name from model
        string getName();

        //get panel height from model
        int getHeight();

        //get penel type (source, dest, neither) from model
        PatchPanel.CONNECTIONTYPE getConnType();

        //get pos of connection on panel from model
        Point connectionPoint();

        //can the panel track the mosue movements?
        bool canTrackMouse();

        //handle mouse down events
        void mouseDown(Point pos);

        //handle mouse move events
        void mouseMove(Point pos);

        //handle mouse up events
        void mouseUp(Point pos);

        //handle mouse single left click events
        void click(Point pos);

        //handle mouse right click events
        void rightClick(Point pos);

        //handle mouse double click events
        void doubleClick(Point pos);

        //paint the panel's display inside it's frame
        void paint(Graphics g);
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
