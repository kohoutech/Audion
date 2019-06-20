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
    public class PatchBox
    {
        public PatchCanvas canvas;
        public List<PatchPanel> panels;
        public Dictionary<String, PatchPanel> panelNames;       //for looking up a panel by its name
        int nextPanelPos;

        bool isSelected;
        bool isTargeted;

        public Rectangle frame;
        public Rectangle titleBar;
        public String title;
        public Rectangle newPanelBar;

        //box constants
        readonly Pen NORMALBORDER  = Pens.Black;
        readonly Pen SELECTEDBORDER = new Pen(Color.Black, 3.0f);
        readonly Pen TARGETEDBORDER = new Pen(Color.Blue, 3.0f);
        readonly Brush BACKGROUNDCOLOR = new SolidBrush(Color.FromArgb(111, 177, 234));
        readonly Brush TITLECOLOR = Brushes.Black;
        readonly Font TITLEFONT = SystemFonts.DefaultFont;
        readonly int FRAMEWIDTH = 100;

        public PatchBox()
        {
            canvas = null;

            frame.Location = new Point(0,0);
            frame.Width = FRAMEWIDTH;

            title = "untitled";
            titleBar.Location = frame.Location;
            titleBar.Width = FRAMEWIDTH;
            titleBar.Height = 25;
            nextPanelPos = titleBar.Bottom;
            frame.Height = nextPanelPos;

            panels = new List<PatchPanel>();
            panelNames = new Dictionary<string, PatchPanel>();

            isSelected = false;
            isTargeted = false;
        }

//- box methods ---------------------------------------------------------------

        public bool hitTest(Point p)
        {
            return (frame.Contains(p));
        }

        //box can be dragged by clicking in its title bar
        public bool dragTest(Point p)
        {
            return (titleBar.Contains(p));
        }

        public void setSelected(bool _selected)
        {
            isSelected = _selected;
        }

        public void setTargeted(bool _targeted)
        {
            isTargeted = _targeted;
        }

        public Point getPos()
        {
            return frame.Location;
        }

        //move frame, title and all panels to new location
        public void setPos(Point pos)
        {
            Point here = getPos();
            int xofs = pos.X - here.X;
            int yofs = pos.Y - here.Y;
            frame.Offset(xofs, yofs);
            titleBar.Offset(xofs, yofs);
            foreach (PatchPanel panel in panels)
            {
                panel.setPos(xofs, yofs);                    
            }
        }

//- mouse methods -------------------------------------------------------------

        //called by canvas when user double clicks on title bar
        public virtual void onTitleDoubleClick()
        {
        }

//- panel methods -------------------------------------------------------------

        public PatchPanel panelHitTest(Point p)
        {
            PatchPanel result = null;
            foreach (PatchPanel panel in panels)
            {
                if (panel.hitTest(p))
                {
                    result = panel;
                    break;
                }
            }
            return result;
        }

        public void addPanel(PatchPanel panel)
        {
            panels.Add(panel);
            panelNames.Add(panel.panelName, panel);
            panel.setPos(frame.Left, frame.Top + nextPanelPos);
            nextPanelPos += panel.frame.Height;
            frame.Height = nextPanelPos;
        }

        public PatchPanel getPanel(string panelName)
        {
            PatchPanel result = null;
            if (panelNames.ContainsKey(panelName))
            {
                result = panelNames[panelName];
            }
            return result;
        }

        public void removePanel(PatchPanel panel)
        { 
        }

        //called by canvas to get all of a box's connections for deletion
        public List<PatchWire> getWireList()
        {
            List<PatchWire> wires = new List<PatchWire>();
            foreach (PatchPanel panel in panels)
            {
                if (panel.wires != null)
                {
                    wires.AddRange(panel.wires);
                }
            }
            return wires;
        }

//- painting ------------------------------------------------------------------

        public void paint(Graphics g) 
        {
            //background
            g.FillRectangle(BACKGROUNDCOLOR, frame);
            
            //title bar
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            g.DrawString(title, TITLEFONT, TITLECOLOR, titleBar, stringFormat);
            g.DrawLine(NORMALBORDER, titleBar.Left, titleBar.Bottom, titleBar.Right, titleBar.Bottom);

            //panels
            foreach (PatchPanel panel in panels)
            {
                panel.paint(g);
            }

            //frame
            g.DrawRectangle(isSelected ? SELECTEDBORDER : (isTargeted ? TARGETEDBORDER : NORMALBORDER), frame);
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
