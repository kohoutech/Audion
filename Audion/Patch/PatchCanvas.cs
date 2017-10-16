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
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;

namespace Transonic.Patch
{
    public class PatchCanvas : Control
    {
        public IPatchView patchwin;         //the window that holds this canvas
        List<PatchBox> boxList;             //the boxes on the canvas
        List<PatchLine> lineList;           //the connector lines on the canvas

        PatchBox selectedBox;           //currently selected box
        PatchLine selectedLine;         //currently select line

        Point newBoxOrg;                //point on canvas where first new box is placed
        Point newBoxOfs;                //offset of next new box is placed
        Point newBoxPos;                //point where next new box is placed

        bool dragging;
        Point dragOrg;
        Point dragOfs;

        bool connecting;
        Point connectLineStart;
        Point connectLineEnd;
        PatchPanel sourcePanel;
        PatchPanel targetPanel;

        bool tracking;
        PatchPanel trackingPanel;

        //cons
        public PatchCanvas(IPatchView _patchwin)
        {
            patchwin = _patchwin;
            boxList = new List<PatchBox>();
            lineList = new List<PatchLine>();

            newBoxOrg = new Point(50, 50);
            newBoxOfs = new Point(20, 20);
            newBoxPos = new Point(newBoxOrg.X, newBoxOrg.Y);

            this.BackColor = Color.FromArgb(0xFF, 0x75, 0x00);
            this.DoubleBuffered = true;

            selectedBox = null;             //selecting
            selectedLine = null;
            dragging = false;               //dragging
            connecting = false;             //connecting
            sourcePanel = null;
            targetPanel = null;
            tracking = false;
            trackingPanel = null;
        }

//- patch management ----------------------------------------------------------

        public void clearPatch()
        {
            //connections
            List<PatchLine> dellineList = new List<PatchLine>(lineList);
            foreach (PatchLine line in dellineList)
            {
                removePatchLine(line);
            }

            //boxes
            List<PatchBox> delboxList = new List<PatchBox>(boxList);
            foreach (PatchBox box in delboxList)
            {
                removePatchBox(box);
            }

            PatchBox.boxCount = 0;
            PatchPanel.panelCount = 0;
            newBoxPos = new Point(newBoxOrg.X, newBoxOrg.Y);            
        }

        public void loadPatch(String patchFileName)
        {
            clearPatch();       //start with a clean slate

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(patchFileName);

            foreach (XmlNode patchNode in xmlDoc.DocumentElement.ChildNodes)
            {
                if (patchNode.Name.Equals("boxes"))
                {
                    foreach (XmlNode boxNode in patchNode.ChildNodes)
                    {
                        loadPatchBox(boxNode);
                    }
                }
                if (patchNode.Name.Equals("connections"))
                {
                    foreach (XmlNode lineNode in patchNode.ChildNodes)
                    {
                        loadPatchLine(lineNode);
                    }
                }
            }

            //renumber boxes
            int boxNum = 0;
            foreach (PatchBox box in boxList)
            {
                box.boxNum = ++boxNum;
                box.RenumberPanels();
            }
            PatchBox.boxCount = boxNum;

            Invalidate();
        }

        public void savePatch(String patchFileName)
        {
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            XmlWriter xmlWriter = XmlWriter.Create(patchFileName, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("patchworkerpatch");
            xmlWriter.WriteAttributeString("version", "1.1.0");

            xmlWriter.WriteStartElement("boxes");
            foreach (PatchBox box in boxList)
            {
                box.saveToXML(xmlWriter);
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("connections");
            foreach (PatchLine line in lineList)
            {
                line.saveToXML(xmlWriter);
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }
        
//- box methods ---------------------------------------------------------------

        public void addPatchBox(PatchBox box)
        {
            box.canvas = this;
            box.setPos(newBoxPos);
            newBoxPos.Offset(newBoxOfs);
            if (!this.ClientRectangle.Contains(newBoxOrg))
            {
                newBoxPos = new Point(newBoxOrg.X, newBoxOrg.Y);      //if we've gone outside the canvas, reset to original new box pos
            }
            boxList.Add(box);
            Invalidate();
        }

        public void loadPatchBox(XmlNode boxNode)
        {
            PatchBox box = PatchBox.loadFromXML(boxNode);
            if (box != null)
            {
                box.canvas = this;
                boxList.Add(box);
            }
        }

        public void removePatchBox(PatchBox box)
        {
            List<PatchLine> connections = box.getConnectionList();
            foreach (PatchLine line in connections)
            {
                removePatchLine(line);                  //remove all connections first
            }
            boxList.Remove(box);                        //and remove box from canvas            
            box.delete();
            Invalidate();            
        }

        public void deselectCurrentSelection()
        {
            //deselect current selection, if there is one
            if (selectedBox != null)
            {
                selectedBox.setSelected(false);
                selectedBox = null;
            }
            if (selectedLine != null)
            {
                selectedLine.Selected = false;
                selectedLine = null;
            }
        }

        public void selectPatchBox(PatchBox box)
        {
            deselectCurrentSelection();
            boxList.Remove(box);                //remove the box from its place in z-order
            boxList.Add(box);                   //and add to end of list, making it topmost
            selectedBox = box;                  //mark box as selected for future operations
            selectedBox.setSelected(true);      //and let it know it
            Invalidate();
        }

        public PatchBox findPatchBox(int boxNum)
        {
            PatchBox result = null;
            foreach (PatchBox box in boxList)
            {
                if (box.boxNum == boxNum)
                {
                    result = box;
                    break;
                }
            }
            return result;
        }

//- line methods ---------------------------------------------------------------

        public void addPatchLine(PatchPanel source, PatchPanel dest)
        {
            PatchLine line = new PatchLine(this, source, dest);         //create new line & connect it to source & dest panels
            lineList.Add(line);                                         //add to canvas
        }

        public void loadPatchLine(XmlNode lineNode)
        {
            PatchLine line = PatchLine.loadFromXML(this, lineNode);
            if (line != null)
            {
                lineList.Add(line);
            }
        }

        //patch line will be connected to source jack, but may or may not be connected to dest jack
        public void removePatchLine(PatchLine line)
        {
            line.disconnect();
            lineList.Remove(line);
            Invalidate();
        }

        public void selectPatchLine(PatchLine line)
        {
            deselectCurrentSelection();
            selectedLine = line;                 //mark line as selected for future operations
            selectedLine.Selected = true;        //and let it know it
            Invalidate();
        }

//- mouse handling ------------------------------------------------------------

        //dragging & connecting are handled while mouse button is pressed and end when it it let up
        //all other ops are handled with mouse clicks
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            bool handled = false;

            //check lines first - go in reverse z-order so we check topmost first
            for (int i = lineList.Count - 1; i >= 0; i--)
            {
                PatchLine line = lineList[i];
                if (line.hitTest(e.Location))
                {
                    selectPatchLine(line);
                    handled = true;
                    break;
                }
            }

            //check boxes - go in reverse z-order so we check topmost first
            if (!handled)
            {
                for (int i = boxList.Count - 1; i >= 0; i--)
                {
                    PatchBox box = boxList[i];
                    if (box.hitTest(e.Location))
                    {
                        selectPatchBox(box);
                        handled = true;
                        break;
                    }
                }

                if (handled)        //we clicked somewhere inside a patchbox (and selected it), check if dragging or connecting
                {
                    if (selectedBox.dragTest(e.Location))           //if we clicked on title panel
                    {
                        startDrag(e.Location);
                    }
                    else
                    {
                        PatchPanel panel = selectedBox.panelHitTest(e.Location);
                        if (panel != null)
                        {
                            if (panel.canConnectOut())                 //if we clicked on out jack panel
                            {
                                startConnection(panel, e.Location);
                            }
                            else if (panel.canTrackMouse())             //if we clicked on a panel that tracks mouse input
                            {
                                tracking = true;
                                trackingPanel = panel;
                                trackingPanel.onMouseDown(e.Location);
                                Invalidate();
                            }
                        }
                    }
                }
            }

            if (!handled)            //we clicked on a blank area of the canvas - deselect current selection if there is one
            {
                deselectCurrentSelection();
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging)
            {
                drag(e.Location);
            }

            if (connecting)
            {
                moveConnection(e.Location);
            }

            if (tracking)
            {
                trackingPanel.onMouseMove(e.Location);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (dragging)
            {
                endDrag(e.Location);
            }

            if (connecting)
            {
                finishConnection(e.Location);
            }

            if (tracking)
            {
                trackingPanel.onMouseUp(e.Location);
                trackingPanel = null;
                tracking = false;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (selectedBox != null && !dragging && !tracking)
            {
                PatchPanel panel = selectedBox.panelHitTest(e.Location);
                if (panel != null)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        panel.onRightClick(e.Location);
                    }
                    else
                    {
                        panel.onClick(e.Location);
                    }
                }
            }
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (selectedLine != null)
            {
                selectedLine.onDoubleClick(e.Location);
            } 
            else if (selectedBox != null)
            {
                if (dragging)       //if we're dragging, then we've double clicked in the title bar
                {
                    selectedBox.onTitleDoubleClick();
                }
                else
                {
                    PatchPanel panel = selectedBox.panelHitTest(e.Location);
                    if (panel != null)
                    {
                        panel.onDoubleClick(e.Location);
                    }
                }
            }
            Invalidate();
        }


//- keyboard handling ---------------------------------------------------------

        //delete key removes currently selected box & any connections to other boxes from canvas
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (selectedBox != null)
                {
                    removePatchBox(selectedBox);
                    selectedBox = null;
                }

                if (selectedLine != null)
                {
                    removePatchLine(selectedLine);
                    selectedLine = null;
                }
            }
        }

//- dragging ------------------------------------------------------------------

        //track diff between pos when mouse button was pressed and where it is now, and move box by the same offset
        public void startDrag(Point p)
        {
            dragging = true;
            dragOrg = selectedBox.getPos();
            dragOfs = p;
        }

        public void drag(Point p)
        {
            int newX = p.X - dragOfs.X;
            int newY = p.Y - dragOfs.Y;
            selectedBox.setPos(new Point(dragOrg.X + newX, dragOrg.Y + newY));
            Invalidate();
        }

        public void endDrag(Point p)
        {
            dragging = false;
        }

//- connecting ------------------------------------------------------------------

        //for now, connections start from selected box's output jack
        //if it doesn't have output jack, it would fail jack hit test & not get here
        public void startConnection(PatchPanel panel, Point p)
        {
            connecting = true;
            sourcePanel = panel;
            connectLineStart = sourcePanel.ConnectionPoint;
            connectLineEnd = p;
            targetPanel = null;
            Invalidate();
        }

        public void moveConnection(Point p)
        {
            connectLineEnd = p;
            
            //check if currently over a possible target box
            bool handled = false;
            for (int i = boxList.Count - 1; i >= 0; i--)
            {
                PatchBox box = boxList[i];
                if (box.hitTest(p))
                {
                    if (box != selectedBox)         //check selected box in case another box is under it, but don't connect to itself
                    {
                        PatchPanel panel = box.panelHitTest(p);
                        if (panel != null && panel.canConnectIn() && !panel.isConnected())
                        {
                            if (targetPanel != null)
                            {
                                targetPanel.patchbox.setTargeted(false);    //deselect current target, if there is one
                            }
                            targetPanel = panel;                            //mark panel as current target, if we drop connection on it
                            targetPanel.patchbox.setTargeted(true);         //and let the panel's box know it
                            handled = true;
                        }
                    }
                    break;
                }
            }

            //if we aren't currently over any targets, unset prev target, if one
            if ((!handled) && (targetPanel != null))
            {
                targetPanel.patchbox.setTargeted(false);
                targetPanel = null;
            }

            Invalidate();
        }

        public void finishConnection(Point p)
        {
            if (targetPanel != null)                              //drop connection on target box we are currently over
            {
                addPatchLine(sourcePanel, targetPanel);
                targetPanel.patchbox.setTargeted(false);
            }

            targetPanel = null;
            sourcePanel = null;
            connecting = false;

            Invalidate();
        }
        
//- painting ------------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            //z-order is front to back - the last one in list is topmost
            for (int i = 0; i < boxList.Count; i++)
            {
                boxList[i].paint(g);
            }

            for (int i = 0; i < lineList.Count; i++)
            {
                lineList[i].paint(g);
            }
            if (connecting)
            {
                g.DrawLine(Pens.Red, connectLineStart, connectLineEnd);
            }
        }
    }

    public class PatchLoadException : Exception
    {
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
