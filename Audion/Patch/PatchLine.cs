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
using System.Drawing.Drawing2D;
using System.Xml;

namespace Transonic.Patch
{
    public class PatchLine
    {
        public PatchCanvas canvas;

        public PatchPanel srcPanel;         //connections in view
        public PatchPanel destPanel;
        public Point srcEnd;                //line's endpoints on the panels
        public Point destEnd;

        public iPatchConnector connector;       //connector in the backing model

        Color lineColor;                    //for rendering line
        Color selectedColor;
        float lineWidth;
        GraphicsPath path;

        bool isSelected;                    
        
        //for reloading existing connections from a stored patch
        public PatchLine(PatchCanvas _canvas, PatchPanel srcPanel, PatchPanel destPanel)
        {
            canvas = _canvas;
            connectSourceJack(srcPanel);
            connectDestJack(destPanel);

            //default appearance
            lineColor = Color.Black;
            selectedColor = Color.Red;
            lineWidth = 2.0f;

            path = new GraphicsPath();
            updatePath();

            isSelected = false;
        }

//- parameters ----------------------------------------------------------------

        //these can be set by the user
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }
        
//- connections ---------------------------------------------------------------

        public void connectSourceJack(PatchPanel _srcPanel)
        {
            srcPanel = _srcPanel;
            srcEnd = srcPanel.ConnectionPoint;
            srcPanel.connectLine(this);                     //connect line & source panel in view     
        }

        public void connectDestJack(PatchPanel _destPanel)
        {
            destPanel = _destPanel;
            destEnd = destPanel.ConnectionPoint;
            destPanel.connectLine(this);                            //connect line & dest panel in view
            
            connector = srcPanel.makeConnection(destPanel);         //connect panels in model, get model connector
            if (connector != null)
            {
                connector.setLine(this);                                //and set this as connector's view
            }
        }

        public void disconnect()
        {
            if (srcPanel != null)
            {
                srcPanel.breakConnection(destPanel);                 //disconnect panels in model
                srcPanel.disconnectLine(this);
                srcPanel = null;
            }

            if (destPanel != null)
            {
                destPanel.disconnectLine(this);
                destPanel = null;                                           //disconnect line from both panels in view
            }
            path = null;
        }

        //these are called if the source / dest panels are moved and the line's path needs to be updated
        public Point SourceEndPos
        {
            set
            {
                srcEnd = value;
                updatePath();
            }
        }

        public Point DestEndPos
        {
            set
            {
                destEnd = value;
                updatePath();
            }
        }

        public void updatePath() 
        {
            path.Reset();
            path.AddLine(srcEnd, destEnd);
        }

//- displaying ----------------------------------------------------------------

        public bool hitTest(Point p)
        {
            Pen linePen = new Pen(lineColor, lineWidth);
            return path.IsOutlineVisible(p, linePen);
        }

        public bool Selected 
        {
            get { return isSelected; }              //don't know if get is needed?
            set { isSelected = value; }
        }

//- user input ----------------------------------------------------------------

        //pass user events back to the model's data connector

        public void onDoubleClick(Point pos)
        {
            if (connector != null)
            {
                connector.onDoubleClick(pos);
            }
        }

        public void onRightClick(Point pos)
        {
            if (connector != null)
            {
                connector.onRightClick(pos);
            }
        }

//- painting ------------------------------------------------------------------

        public void paint(Graphics g)
        {
            using (Pen linePen = new Pen(isSelected ? selectedColor : lineColor, lineWidth))
            {
                g.DrawPath(linePen, path);
            }
        }

//- persistance ---------------------------------------------------------------

        //get source & dest box nums from XML file and get the matching boxes from the canvas
        //then get the panel nums from XML and get the matching panels from the boxes
        //having the source & dest panels. create a new line between them
        //this will create a connection in the backing model, call loadFromXML() on it with XML node to set its properties
        public static PatchLine loadFromXML(PatchCanvas canvas, XmlNode lineNode)
        {
            PatchLine line = null;
            try
            {
                int srcBoxNum = Convert.ToInt32(lineNode.Attributes["sourcebox"].Value);
                int srcPanelNum = Convert.ToInt32(lineNode.Attributes["sourcepanel"].Value);
                int destBoxNum = Convert.ToInt32(lineNode.Attributes["destbox"].Value);
                int destPanelNum = Convert.ToInt32(lineNode.Attributes["destpanel"].Value);

                PatchBox sourceBox = canvas.findPatchBox(srcBoxNum);
                PatchBox destBox = canvas.findPatchBox(destBoxNum);
                if (sourceBox != null && destBox != null)
                {
                    PatchPanel sourcePanel = sourceBox.findPatchPanel(srcPanelNum);
                    PatchPanel destPanel = destBox.findPatchPanel(destPanelNum);

                    if (sourcePanel != null && destPanel != null)
                    {
                        line = new PatchLine(canvas, sourcePanel, destPanel);                        
                    }
                }

            }
            catch (Exception e)
            {
                throw new PatchLoadException();
            }

            return line;
        }

        public void saveToXML(XmlWriter xmlWriter)
        {
            //save patch line attributes
            xmlWriter.WriteStartElement("connection");
            xmlWriter.WriteAttributeString("sourcebox", srcPanel.patchbox.boxNum.ToString());
            xmlWriter.WriteAttributeString("sourcepanel", srcPanel.panelNum.ToString());
            xmlWriter.WriteAttributeString("destbox", destPanel.patchbox.boxNum.ToString());
            xmlWriter.WriteAttributeString("destpanel", destPanel.panelNum.ToString());

            xmlWriter.WriteEndElement();
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
