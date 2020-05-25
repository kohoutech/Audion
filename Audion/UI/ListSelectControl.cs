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
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Drawing;

using Kohoutech.Patch;
using Audion.Breadboard;

namespace Audion.UI
{
    public class ListSelectControl : Module, IPatchBox
    {
        public List<String> items;
        public int curitem;

        public ListSelectControl() : base("List")
        {
            JackPanel jack = new ListJackPanel(this);
            panels.Add(jack);
            ListPanel list = new ListPanel(this);
            panels.Add(list);

            items = null;
            curitem = 0;
        }

        //public void remove()
        //{            
        //}
    }

    //-------------------------------------------------------------------------

    public class ListJackPanel : JackPanel
    {

        public ListJackPanel(Module _module) : base(_module, "Out", JackPanel.DIRECTION.OUT)
        {
        }

        public override void connect(PatchCord cord)
        {
            base.connect(cord);
            ModuleParameter dparm = ((ParamPanel)cord.dest).parm;
            if (dparm != null && dparm is ListParameter)
            {
                ListParameter listparm = (ListParameter)dparm;
                ((ListSelectControl)module).items = listparm.paramList;
                ((ListSelectControl)module).curitem = 0;
            }
        }

        public override void disconnect(PatchCord cord)
        {
            base.disconnect(cord);
            ((ListSelectControl)module).items = null;
            ((ListSelectControl)module).curitem = 0;
        }
    }

    //-------------------------------------------------------------------------

    public class ListPanel : ModulePanel, IPatchPanel
    {
        const int ITEMMARGIN = 10;

        public ListPanel(ListSelectControl module) : base(module, "List")
        {
        }

        //display list values in a popup menu over the list panel
        public void click(Point pos)
        {
            if (((ListSelectControl)module).items != null)
            {
                ContextMenuStrip cm = new ContextMenuStrip();
                cm.ItemClicked += new ToolStripItemClickedEventHandler(ItemsMenuClicked);
                for (int i = 0; i < ((ListSelectControl)module).items.Count; i++)
                {
                    ToolStripButton item = new ToolStripButton(((ListSelectControl)module).items[i]);
                    item.Checked = (i == ((ListSelectControl)module).curitem);
                    item.Tag = i;
                    cm.Items.Add(item);
                }

                cm.Show(module.patch.canvas, pos);
            }
        }

        void ItemsMenuClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripButton item = (ToolStripButton)e.ClickedItem;
            ((ListSelectControl)module).curitem = (Int32)item.Tag;
            module.patch.canvas.Invalidate();            
        }


        public void paint(Graphics g, Rectangle frame)
        {
            //Rectangle itemRect = new Rectangle(frame.Left + ITEMMARGIN, frame.Top, frame.Width - (ITEMMARGIN * 2), frame.Height);
            //g.FillRectangle(Brushes.White, itemRect);

            //RectangleF rect = new RectangleF(itemRect.Left, itemRect.Top, itemRect.Width, itemRect.Height);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            if (((ListSelectControl)module).items != null)
            {
                g.DrawString(((ListSelectControl)module).items[((ListSelectControl)module).curitem], SystemFonts.DefaultFont, Brushes.Black, frame, format);
            }
            else
            {
                g.DrawString("---", SystemFonts.DefaultFont, Brushes.Black, frame, format);
            }
        }
    }
}
