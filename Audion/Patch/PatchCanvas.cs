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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using Origami.ENAML;

namespace Transonic.Patch
{
    public class PatchCanvas : Control
    {
        public IPatchModel patchModel;         //the canvas' backing model
        public PatchPalette palette;

        List<PatchBox> boxList;             //the boxes on the canvas
        List<PatchWire> wireList;           //the wires on the canvas
        List<Object> zList;                 //z-order for painting boxes & wires

        PatchBox selectedBox;           //currently selected box
        PatchWire selectedWire;         //currently select wire

        Point newBoxOrg;                //point on canvas where first new box is placed
        Point newBoxOfs;                //offset of next new box is placed
        Point newBoxPos;                //point where next new box is placed

        bool dragging;
        Point dragOrg;
        Point dragOfs;

        bool connecting;
        Point connectWireStart;
        Point connectWireEnd;
        PatchPanel sourcePanel;
        PatchPanel targetPanel;

        bool tracking;
        PatchPanel trackingPanel;

        //cons
        public PatchCanvas(IPatchModel _patchwin)
        {
            patchModel = _patchwin;

            palette = new PatchPalette(this);
            palette.Location = new Point(this.ClientRectangle.Left, this.ClientRectangle.Top);
            palette.Size = new Size(palette.Width, this.ClientRectangle.Height);
            this.Controls.Add(palette);

            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            boxList = new List<PatchBox>();
            wireList = new List<PatchWire>();
            zList = new List<object>();

            newBoxOrg = new Point(palette.Width + 50, 50);
            newBoxOfs = new Point(20, 20);
            newBoxPos = new Point(newBoxOrg.X, newBoxOrg.Y);

            //init canvas state
            selectedBox = null;             //selecting
            selectedWire = null;
            dragging = false;               //dragging
            connecting = false;             //connecting
            sourcePanel = null;
            targetPanel = null;
            tracking = false;
            trackingPanel = null;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (palette != null)
            {
                palette.Size = new Size(palette.Width, this.ClientRectangle.Height);
            }
        }

        //- palette management ----------------------------------------------------------

        public void openPalette(bool isOpen)
        {
            if (isOpen)
            {
                palette.Location = new Point(this.ClientRectangle.Left, this.ClientRectangle.Top);
            }
            else
            {
                palette.Location = new Point(this.ClientRectangle.Left - palette.Width + palette.buttonWidth, this.ClientRectangle.Top);
            }
            this.Invalidate();          //redraw palette border
        }

        public void setPaletteColor(Color color)
        {
            palette.BackColor = color;
            palette.Invalidate();
        }

        public void setPaletteItems(List<PaletteItem> items)
        {
            palette.setItems(items);
        }

        public void enablePaletteItem(PaletteItem item)
        {
            item.enabled = true;
            palette.enableItem(item);
        }

        public void disablePaletteItem(PaletteItem item)
        {
            item.enabled = false;
            palette.enableItem(item);
        }

        public void handlePaletteItemDoubleClick(PaletteItem item)
        {
            PatchBox newBox = patchModel.getPatchBox(item);
            addPatchBox(newBox);
        }

        //- patch management ----------------------------------------------------------

        public void clearPatch()
        {
            //connections
            List<PatchWire> delWireList = new List<PatchWire>(wireList);
            foreach (PatchWire wire in delWireList)
            {
                removePatchWire(wire, false);
            }

            //boxes
            List<PatchBox> delboxList = new List<PatchBox>(boxList);
            foreach (PatchBox box in delboxList)
            {
                removePatchBox(box, false);
            }

            patchModel.patchHasBeenCleared();

            newBoxPos = new Point(newBoxOrg.X, newBoxOrg.Y);
        }

        //- box methods ---------------------------------------------------------------

        //add a patch box at default location on canvas
        public void addPatchBox(PatchBox box)
        {
            addPatchBox(box, newBoxPos.X, newBoxPos.Y);
            newBoxPos.Offset(newBoxOfs);
            if (!this.ClientRectangle.Contains(newBoxOrg))
            {
                newBoxPos = new Point(newBoxOrg.X, newBoxOrg.Y);      //if we've gone outside the canvas, reset to original new box pos
            }
        }

        public void addPatchBox(PatchBox box, int xpos, int ypos)
        {
            box.canvas = this;
            box.setPos(new Point(xpos, ypos));
            boxList.Add(box);
            zList.Add(box);
            patchModel.patchHasChanged();
            Invalidate();
        }

        public void removePatchBox(PatchBox box, bool notify)
        {
            List<PatchWire> wires = box.getWireList();
            foreach (PatchWire wire in wires)
            {
                removePatchWire(wire, false);           //remove all connections first
            }
            patchModel.removePatchBox(box);               //delete box's model
            boxList.Remove(box);                        //and remove box from canvas            
            zList.Remove(box);
            if (notify)
            {
                patchModel.patchHasChanged();
            }
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
            if (selectedWire != null)
            {
                selectedWire.Selected = false;
                selectedWire = null;
            }
        }

        public void selectPatchBox(PatchBox box)
        {
            deselectCurrentSelection();
            List<PatchWire> wires = box.getWireList();      //first make all box's wires top of z-order
            foreach (PatchWire wire in wires)
            {
                zList.Remove(wire);
                zList.Add(wire);
            }
            zList.Remove(box);                  //remove the box from its place in z-order
            zList.Add(box);                     //and add to end of list, making it topmost
            selectedBox = box;                  //mark box as selected for future operations
            selectedBox.setSelected(true);      //and let it know it is
            Invalidate();
        }

        public  List<PatchBox> getBoxList()
        {
            return boxList;
        }

        //- wire methods ---------------------------------------------------------------

        public void addPatchWire(PatchWire wire)
        {
            wire.canvas = this;
            wireList.Add(wire);                                      //add to canvas
            zList.Add(wire);
            patchModel.patchHasChanged();
            Invalidate();
        }

        //patch wire will be connected to source jack, but may or may not be connected to dest jack
        public void removePatchWire(PatchWire wire, bool notify)
        {
            patchModel.removePatchWire(wire);             //delete wire's model
            wire.disconnect();                          //and disconnect wire from source & dest jacks
            wireList.Remove(wire);
            zList.Remove(wire);
            if (notify)
            {
                patchModel.patchHasChanged();
            }
            Invalidate();
        }

        //selecting a wire does NOT change its z-order pos
        public void selectPatchWire(PatchWire wire)
        {
            deselectCurrentSelection();
            selectedWire = wire;                 //mark wire as selected for future operations
            selectedWire.Selected = true;        //and let it know it is
            Invalidate();
        }

        public List<PatchWire> getWireList()
        {
            return wireList;
        }

        //- mouse handling ------------------------------------------------------------

        //dragging & connecting are handled while mouse button is pressed and end when it it let up
        //all other ops are handled with mouse clicks
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            bool handled = false;

            //go in reverse z-order so we check topmost first
            for (int i = zList.Count - 1; i >= 0; i--)
            {
                object obj = zList[i];
                if (obj is PatchWire)
                {
                    PatchWire wire = (PatchWire)obj;
                    if (wire.hitTest(e.Location))
                    {
                        selectPatchWire(wire);
                        handled = true;
                        break;
                    }
                }

                if (!handled && obj is PatchBox)
                {
                    PatchBox box = (PatchBox)obj;
                    if (box.hitTest(e.Location))
                    {
                        selectPatchBox(box);

                        //we clicked somewhere inside a patchbox (and selected it), check if dragging or connecting                    
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
                        handled = true;
                        break;
                    }
                }
            }

            //we clicked on a blank area of the canvas - deselect current selection if there is one
            if (!handled)            
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
            if (selectedWire != null)
            {
                selectedWire.onDoubleClick(e.Location);
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
                    removePatchBox(selectedBox, true);
                    selectedBox = null;
                }

                if (selectedWire != null)
                {
                    removePatchWire(selectedWire, true);
                    selectedWire = null;
                }
            }
        }

        //- dragging ------------------------------------------------------------------

        //track diff between pos when mouse button was pressed and where it is now, and move box by the same offset
        private void startDrag(Point p)
        {
            dragging = true;
            dragOrg = selectedBox.getPos();
            dragOfs = p;
        }

        private void drag(Point p)
        {
            int newX = p.X - dragOfs.X;
            int newY = p.Y - dragOfs.Y;
            selectedBox.setPos(new Point(dragOrg.X + newX, dragOrg.Y + newY));
            Invalidate();
        }

        private void endDrag(Point p)
        {
            dragging = false;
        }

        //- connecting ------------------------------------------------------------------

        //for now, connections start from selected box's output jack
        //if it doesn't have output jack, it would fail jack hit test & not get here
        private void startConnection(PatchPanel panel, Point p)
        {
            connecting = true;
            sourcePanel = panel;
            connectWireStart = sourcePanel.ConnectionPoint;
            connectWireEnd = p;
            targetPanel = null;
            Invalidate();
        }

        private void moveConnection(Point p)
        {
            connectWireEnd = p;

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

        private void finishConnection(Point p)
        {
            if (targetPanel != null)                              //drop connection on target box we are currently over
            {
                targetPanel.patchbox.setTargeted(false);
                PatchWire newWire = patchModel.getPatchWire(sourcePanel, targetPanel);    //create new wire & connect it to source & dest panels
                addPatchWire(newWire);
            }

            targetPanel = null;
            sourcePanel = null;
            connecting = false;
        }

        //- painting ------------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            //palette border
            g.DrawLine(Pens.Black, palette.Right, palette.Top, palette.Right, palette.Bottom);

            //z-order is front to back - the last one in list is topmost
            foreach (Object obj in zList)
            {
                if (obj is PatchBox)
                {
                    ((PatchBox)obj).paint(g);
                }
                if (obj is PatchWire)
                {
                    ((PatchWire)obj).paint(g);
                }
            }

            //temporary connecting line
            if (connecting)
            {
                g.DrawLine(Pens.Red, connectWireStart, connectWireEnd);
            }
        }

        //- persistance ---------------------------------------------------------------

        public void loadPatch(string patchFilename)
        {
            clearPatch();       //start with a clean slate

            EnamlData data = EnamlData.loadFromFile(patchFilename);

            patchModel.loadPatchData(data);       //load model specific data from the patch file

            //temporary dict for mapping a name to its patch box after it's been added to the canvas
            Dictionary<String, PatchBox> boxDict = new Dictionary<string, PatchBox>();

            List<String> boxList = data.getPathKeys("boxes");
            foreach (String boxName in boxList)
            {
                String boxPath = "boxes." + boxName;
                PatchBox newBox = patchModel.loadPatchBox(data, boxPath);
                int xpos = data.getIntValue(boxPath + ".x-pos", 0);
                int ypos = data.getIntValue(boxPath + ".y-pos", 0);
                addPatchBox(newBox, xpos, ypos);
                boxDict.Add(newBox.title, newBox);                
            }

            List<String> wireList = data.getPathKeys("wires");
            foreach (String wireName in wireList)
            {
                String wirePath = "wires." + wireName;
                String srcBoxName = data.getStringValue(wirePath + ".source-box", "");
                String srcPanelName = data.getStringValue(wirePath + ".source-panel", "");
                PatchBox srcBox = boxDict[srcBoxName];
                PatchPanel srcPanel = srcBox.getPanel(srcPanelName);

                String destBoxName = data.getStringValue(wirePath + ".dest-box", "");
                String destPanelName = data.getStringValue(wirePath + ".dest-panel", "");
                PatchBox destBox = boxDict[destBoxName];
                PatchPanel destPanel = destBox.getPanel(destPanelName);

                //create new wire & connect it to source & dest panels
                PatchWire newWire = patchModel.loadPatchWire(data, wirePath, srcPanel, destPanel);    
                addPatchWire(newWire);
            }
        }

        public void savePatch(string patchFilename)
        {
            EnamlData data = new EnamlData();

            patchModel.savePatchData(data);       //store model specific data from the patch file

            int count = 1;
            foreach (PatchBox box in boxList)
            {
                String boxPath = "boxes.box-" + count.ToString().PadLeft(3, '0');
                patchModel.savePatchBox(data, boxPath, box);
                data.setIntValue(boxPath + ".x-pos", box.getPos().X);
                data.setIntValue(boxPath + ".y-pos", box.getPos().Y);
                count++;
            }

            count = 1;
            foreach (PatchWire wire in wireList)
            {
                String wirePath = "wires.wire-" + count.ToString().PadLeft(3, '0');
                patchModel.savePatchWire(data, wirePath, wire);
                data.setStringValue(wirePath + ".source-box", wire.srcPanel.patchbox.title);
                data.setStringValue(wirePath + ".source-panel", wire.srcPanel.panelName);
                data.setStringValue(wirePath + ".dest-box", wire.destPanel.patchbox.title);
                data.setStringValue(wirePath + ".dest-panel", wire.destPanel.panelName);
                count++;
            }

            data.saveToFile(patchFilename);
        }
    }

    public class PatchLoadException : Exception
    {
    }

    public class PatchSaveException : Exception
    {
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
