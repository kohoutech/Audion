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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Transonic.MIDI.System;
using Transonic.Patch;

using Audion.Graph;
using Audion.UI;

namespace Audion
{
    public partial class AudionWindow : Form, IPatchView
    {
        public Audion audion;
        public PatchCanvas canvas;

        public AudionWindow()
        {
            audion = new Audion();

            InitializeComponent();

            canvas = new PatchCanvas(this);
            canvas.Dock = DockStyle.Fill;
            this.Controls.Add(canvas);

            audion.initModuleMenu(this);
        }

//- file menu -----------------------------------------------------------------

        private void newFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveAsFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitFileMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

//- module menu ---------------------------------------------------------------

        public void addModuleToMenu(Module module)
        {
            ToolStripItem inputItem = new ToolStripMenuItem(module.name);
            inputItem.Click += new EventHandler(modSelectMenuItem_Click);
            inputItem.Tag = module;
            modulesMenuItem.DropDownItems.Insert(modulesMenuItem.DropDownItems.Count, inputItem);
        }

        //handler for all module menu items
        private void modSelectMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            Module mod = (Module)item.Tag;                  //get module obj from menu item

            audion.addModuleToPatch(mod);                   //add module to graph
            ModuleBox box = new ModuleBox(mod);             //create new module box from unit
            canvas.addPatchBox(box);                        //and add it to canvas
        }


//- help menu -----------------------------------------------------------------

        private void aboutHelpMenuItem_Click(object sender, EventArgs e)
        {
            String msg = "Audion\nversion 0.1.0\n" +
                "\xA9 Transonic Software 2011-2017\n" +
                "http://transonic.kohoutech.com";
            MessageBox.Show(msg, "About");
        }
    }
}
