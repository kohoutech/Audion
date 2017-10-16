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

namespace Transonic.Patch
{
    public class PatchBox
    {
        public String boxType;
        public static int boxCount = 0;
        public int boxNum;

        public PatchCanvas canvas;
        public List<PatchPanel> panels;
        int panelNum;
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
        readonly int NEWPANELBARHEIGHT = 0;

        public PatchBox()
        {
            boxType = "PatchBox";
            boxNum = ++boxCount;
            canvas = null;

            frame.Location = new Point(0,0);
            frame.Width = FRAMEWIDTH;

            title = "Box " + boxNum.ToString();
            titleBar.Location = frame.Location;
            titleBar.Width = FRAMEWIDTH;
            titleBar.Height = 25;
            nextPanelPos = titleBar.Bottom;
            frame.Height = nextPanelPos + NEWPANELBARHEIGHT;           //space at bottom of box for new panel bar

            panels = new List<PatchPanel>();
            panelNum = 0;

            isSelected = false;
            isTargeted = false;
        }

//- box methods ---------------------------------------------------------------

        //called by canvas when removing box - all of it's connections should have been deleted first
        public virtual void delete()
        {            
        }

        public bool hitTest(Point p)
        {
            return (frame.Contains(p));
        }

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

        public void addPanel(PatchPanel panel, bool hasNumber)
        {
            panels.Add(panel);
            if (!hasNumber)
            {
                panel.panelNum = ++panelNum;
            }
            panel.setPos(frame.Left, frame.Top + nextPanelPos);
            nextPanelPos += panel.frame.Height;
            frame.Height = nextPanelPos + NEWPANELBARHEIGHT;
        }

        public void removePanel(PatchPanel panel)
        { 
        }

        public PatchPanel findPatchPanel(int panelNum)
        {
            PatchPanel result = null;
            foreach (PatchPanel panel in panels)
            {
                if (panel.panelNum == panelNum)
                {
                    result = panel;
                    break;
                }
            }
            return result;
        }

        //called by canvas to get all of a box's connections for deletion
        public List<PatchLine> getConnectionList()
        {
            List<PatchLine> connections = new List<PatchLine>();
            foreach (PatchPanel panel in panels)
            {
                if (panel.connectors != null)
                {
                    connections.AddRange(panel.connectors);
                }
            }
            return connections;
        }

        public void RenumberPanels()
        {
            int panelcnt = 0;
            foreach (PatchPanel panel in panels)
            {
                panel.panelNum = ++panelcnt;
            }
            panelNum = panelcnt;
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

//- persistance ---------------------------------------------------------------

        static Dictionary<String, PatchBoxLoader> boxTypeList = new Dictionary<String, PatchBoxLoader>();

        public static void registerBoxType(String panelName, PatchBoxLoader loader)
        {
            boxTypeList.Add(panelName, loader);
        }

        //loading
        public static PatchBox loadFromXML(XmlNode boxNode)
        {
            PatchBox box = null;
            String boxName = boxNode.Attributes["boxtype"].Value;
            PatchBoxLoader loader = boxTypeList[boxName];
            if (loader != null)
            {
                box = loader.loadFromXML(boxNode);
                if (box != null)
                {
                    foreach (XmlNode panelNode in boxNode.ChildNodes)
                    {
                        PatchPanel panel = PatchPanel.loadFromXML(box, panelNode);
                        box.addPanel(panel, true);
                    }
                }
            }
            return box;
        }

        public virtual void loadAttributesFromXML(XmlNode boxNode)
        {
            title = boxNode.Attributes["title"].Value;
            boxNum = Convert.ToInt32(boxNode.Attributes["number"].Value);
            int xpos = Convert.ToInt32(boxNode.Attributes["x-pos"].Value);
            int ypos = Convert.ToInt32(boxNode.Attributes["y-pos"].Value);
            setPos(new Point(xpos, ypos));
        }

        //saving
        public void saveToXML(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("box");
            saveAttributesToXML(xmlWriter);
            foreach (PatchPanel panel in panels)
            {
                panel.saveToXML(xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        public virtual void saveAttributesToXML(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("boxtype", boxType);
            xmlWriter.WriteAttributeString("number", boxNum.ToString());
            xmlWriter.WriteAttributeString("title", title);
            xmlWriter.WriteAttributeString("x-pos", frame.X.ToString());
            xmlWriter.WriteAttributeString("y-pos", frame.Y.ToString());
        }
    }

//- box loader class --------------------------------------------------------

    public class PatchBoxLoader
    {
        public virtual PatchBox loadFromXML(XmlNode boxNode)
        {
            return new PatchBox();
        }
    }

}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
