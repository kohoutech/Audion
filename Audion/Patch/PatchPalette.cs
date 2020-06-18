/* ----------------------------------------------------------------------------
Kohoutech Patch Library
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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kohoutech.Patch
{
    public class PatchPalette : Control
    {
        const int PALLETEWIDTH = 150;
        const int ITEMBOXHEIGHT = 25;
        public Color PALETTECOLOR = Color.Ivory;
        public Color DISABLEDCOLOR = Color.LightGray;
        public Color GROUPITEMCOLOR = Color.LightBlue;

        public PatchCanvas canvas;
        public Button btnOpen;
        private VScrollBar scrollbar;
        private ToolTip paletteToolTip;
        private Panel panelSpace;

        public int buttonWidth;
        public int itemWidth;
        bool isOpen;

        public List<PaletteGroup> groups;
        public Dictionary<string, PaletteGroup> groupNames;

        public Dictionary<string, PaletteItem> itemNames;

        public List<PaletteItem> ungrouped;

        public List<Label> itemBoxes;

        internal PatchPalette(PatchCanvas _canvas)
        {
            canvas = _canvas;
            this.Size = new Size(PALLETEWIDTH, 100);        //the width is constant, the height changes
            this.BackColor = PALETTECOLOR;
            this.TabStop = false;                           //keep palette from stealing focus from canvas

            //open/close button at top right corner
            btnOpen = new Button();
            btnOpen.FlatStyle = FlatStyle.System;
            btnOpen.Text = "<";
            btnOpen.TabStop = false;
            btnOpen.Click += new System.EventHandler(btnOpen_Click);
            this.Controls.Add(btnOpen);

            paletteToolTip = new ToolTip();
            paletteToolTip.SetToolTip(btnOpen, "open / close palette");
            
            //scrollbar on right
            scrollbar = new VScrollBar();
            scrollbar.Minimum = 0;
            scrollbar.Dock = DockStyle.Right;
            scrollbar.TabStop = false;
            scrollbar.ValueChanged += new EventHandler(scrollbar_ValueChanged);
            this.Controls.Add(scrollbar);

            //set sizes
            btnOpen.Size = new System.Drawing.Size(scrollbar.Width, scrollbar.Width);
            btnOpen.Location = new System.Drawing.Point(this.Width - scrollbar.Width, 0);
            buttonWidth = btnOpen.Width;
            itemWidth = this.Width - buttonWidth;
            isOpen = true;

            //set panel space on left of scroll bar
            panelSpace = new Panel();
            panelSpace.BackColor = PALETTECOLOR;
            panelSpace.Location = new Point(0, 0);
            panelSpace.Size = new Size(0, 0);
            panelSpace.TabStop = false;
            this.Controls.Add(panelSpace);


            groups = new List<PaletteGroup>();
            groupNames = new Dictionary<string, PaletteGroup>();
            ungrouped = new List<PaletteItem>();
            itemNames = new Dictionary<string, PaletteItem>();

            itemBoxes = new List<Label>();
            buildPalette();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            updateScrollBar();
        }

        //- item management ---------------------------------------------------

        private PaletteItem findItem(string itemName)
        {
            PaletteItem item = null;
            if (itemNames.ContainsKey(itemName))
            {
                item = itemNames[itemName];
            }
            return item;
        }

        internal void addGroup(string groupName)
        {
            PaletteGroup group = null;
            if (groupNames.ContainsKey(groupName))          //if already added
            {
                return;
            }
            group = new PaletteGroup(groupName);
            groups.Add(group);
            groupNames[groupName] = group;
            createPaletteEntry(group);
        }

        internal void addItem(string groupName, string itemName, string modelName)
        {
            PaletteItem item = new PaletteItem(itemName, modelName);

            if (groupNames.ContainsKey(groupName))
            {
                PaletteGroup group = groupNames[groupName];
                group.addItem(item);
            }
            else
            {
                ungrouped.Add(item);
            }

            createPaletteEntry(item);               //create a palette entry for this item
            buildPalette();                         //and (re)build palette from its items & groups
        }

        private void createPaletteEntry(PaletteItem item)
        {
            Label itemBox = new Label();
            item.itembox = itemBox;
            itemBox.Text = item.name;
            itemBox.TextAlign = ContentAlignment.MiddleCenter;
            itemBox.BorderStyle = BorderStyle.FixedSingle;
            if (!item.enabled)
            {
                itemBox.BackColor = DISABLEDCOLOR;
                itemBox.Enabled = false;
            }
            itemBox.Location = new Point(0, 0);
            itemBox.Size = new Size(itemWidth, ITEMBOXHEIGHT);      //box height will be set by group item when it's opened/closed
            itemBox.Tag = item;
            if (item is PaletteGroup)
            {
                itemBox.BackColor = GROUPITEMCOLOR;
                itemBox.Click += new EventHandler(groupBox_DoubleClick);
            }
            else
            {
                itemBox.DoubleClick += new EventHandler(itemBox_DoubleClick);
            }
        }

        private void buildPalette()
        {
            panelSpace.Controls.Clear();
            itemBoxes.Clear();

            foreach (PaletteGroup group in groups)
            {
                //handle group box first
                Label itemBox = group.itembox;
                panelSpace.Controls.Add(itemBox);
                itemBoxes.Add(itemBox);

                if (group.isOpen)
                {
                    foreach (PaletteItem groupItem in group.items)
                    {
                        Label groupItemBox = groupItem.itembox;
                        panelSpace.Controls.Add(groupItemBox);
                        itemBoxes.Add(groupItemBox);
                    }
                }
            }

            //add any items not ina group at the end of the palette
            foreach(PaletteItem item in ungrouped)
            {
                Label itemBox = item.itembox;
                panelSpace.Controls.Add(itemBox);
                itemBoxes.Add(itemBox);
            }

            layoutPaletteItems();
            updateScrollBar();
        }

        private void layoutPaletteItems()
        {
            int ypos = 0;
            foreach (Label itemBox in itemBoxes)
            {
                itemBox.Location = new Point(0, ypos);
                ypos += itemBox.Height;
            }
            panelSpace.Size = new Size(itemWidth, ypos);
        }

        //enable or disable palette item w/o rebuilding entire list
        internal void enableItem(String itemName, bool enabled)
        {
            PaletteItem item = findItem(itemName);
            item.enabled = enabled;

            if (item.itembox != null)
            {
                item.itembox.BackColor = item.enabled ? PALETTECOLOR : DISABLEDCOLOR;
                item.itembox.Enabled = item.enabled;
                item.itembox.Invalidate();
            }
        }

        //- event handlers ----------------------------------------------------

        private void scrollbar_ValueChanged(object sender, EventArgs e)
        {
            panelSpace.Location = new Point(0, -scrollbar.Value);
        }

        //compensate for the fact that if the scrollbar max = 50, the greatest value will be 50 - THUMBWIDTH (value determined experimentally)
        const int THUMBWIDTH = 9;

        //called when the canvas space or item list changes
        private void updateScrollBar()
        {
            if (scrollbar != null)
            {
                scrollbar.Maximum = ((this.Height) < panelSpace.Height) ? (panelSpace.Height - this.Height + THUMBWIDTH) : 0;
                if ((scrollbar.Maximum > THUMBWIDTH) && (scrollbar.Maximum - scrollbar.Value < THUMBWIDTH))
                {
                    scrollbar.Value = scrollbar.Maximum - THUMBWIDTH;
                }
            }
        }

        //when any palette item is double clicked
        private void itemBox_DoubleClick(object sender, EventArgs e)
        {
            PaletteItem item = (PaletteItem)((Label)sender).Tag;
            if (item.enabled)
            {
                canvas.handlePaletteItemDoubleClick(item.tag);
            }
        }

        //when any palette group is double clicked
        private void groupBox_DoubleClick(object sender, EventArgs e)
        {
            PaletteGroup group = (PaletteGroup)((Label)sender).Tag;
            group.isOpen = !group.isOpen;
            foreach (PaletteItem item in group.items)
            {
                int width = item.itembox.Width;
                item.itembox.Size = new Size(width, group.isOpen ? ITEMBOXHEIGHT : 0);
            }
            layoutPaletteItems();
            this.Invalidate();
        }

        //tells the canvas to open or close the palette
        internal void btnOpen_Click(object sender, EventArgs e)
        {
            isOpen = !isOpen;
            btnOpen.Text = isOpen ? "<" : ">";
            String tooltiptext = isOpen ? "close palette" : "open palette";
            paletteToolTip.SetToolTip(btnOpen, tooltiptext);
            scrollbar.Visible = isOpen;
            canvas.openPalette(isOpen);
        }
    }

    //-------------------------------------------------------------------------

    public class PaletteItem
    {
        public String name;                 //the name displayed on the palette
        public bool enabled;                //whether the item responds to dbl clicks 
        public Label itembox;               //its view in the palette wiudget
        public String tag;                  //value passed to the model when dbl clicked

        public PaletteItem(String _name, String _tag)
        {
            name = _name;
            enabled = true;
            itembox = null;
            tag = _tag;
        }
    }

    //-------------------------------------------------------------------------

    public class PaletteGroup : PaletteItem
    {
        public List<PaletteItem> items;
        public bool isOpen;

        public PaletteGroup(String name) : base(name, "")
        {
            items = new List<PaletteItem>();
            isOpen = true;
        }

        public void addItem(PaletteItem item)
        {
            items.Add(item);
        }
    }
}
