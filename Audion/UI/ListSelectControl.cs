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
using System.Windows.Forms;
using System.Text;
using System.Drawing;

using Transonic.Patch;
using Audion.Graph;

namespace Audion.UI
{
    public class ListSelectControl : PatchBox
    {
        Module module;
        ListPanel panel;

        public ListSelectControl(Module _module)
            : base()
        {
            module = _module;
            title = "List";
            addPanel(new ModulePanel(this, module.jacks[0]), false);
            List<String> items = new List<string>() { "SINE", "TRIANGLE", "SAW", "SQUARE" };
            panel = new ListPanel(this, items);
            addPanel(panel, false);
        }
    }

    public class ListPanel : PatchPanel
    {
        const int ITEMMARGIN = 10;
        List<String> items;
        int curitem;
        Rectangle itemRect;

        public ListPanel(PatchBox box, List<String> _items)
            : base(box)
        {
            panelType = "ListPanel";
            connType = CONNECTIONTYPE.NEITHER;
            updateFrame(40, frameWidth);
            itemRect = new Rectangle(frame.Left + ITEMMARGIN, frame.Top + ITEMMARGIN,
                frame.Width - (ITEMMARGIN * 2), frame.Height - (ITEMMARGIN * 2));
            items = _items;
            curitem = 0;
        }

        public override void setPos(int xOfs, int yOfs)
        {
            base.setPos(xOfs, yOfs);
            itemRect.Offset(xOfs, yOfs);
        }

        public override void onRightClick(Point pos)
        {
            ContextMenuStrip cm = new ContextMenuStrip();
            cm.ItemClicked += new ToolStripItemClickedEventHandler(ItemsMenuClicked);
            for (int i = 0; i < items.Count; i++)
            {
                ToolStripButton item = new ToolStripButton(items[i]);
                item.Checked = (i == curitem);
                item.Tag = i;
                cm.Items.Add(item);
            }

            cm.Show(patchbox.canvas, pos);
        }

        void ItemsMenuClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripButton item = (ToolStripButton)e.ClickedItem;
            curitem = (Int32)item.Tag;
            patchbox.canvas.Invalidate();            
        }


        public override void paint(Graphics g)
        {
            base.paint(g);
            g.FillRectangle(Brushes.White, itemRect);
            RectangleF rect = new RectangleF(itemRect.Left, itemRect.Top, itemRect.Width, itemRect.Height);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString(items[curitem], SystemFonts.DefaultFont, Brushes.Black, rect, format);
        }
    }
}
