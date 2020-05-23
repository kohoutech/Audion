﻿/* ----------------------------------------------------------------------------
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Kohoutech.MIDI.System;
using Kohoutech.Patch;
using Kohoutech.Widget;

using Audion.Breadboard;
using Audion.UI;

namespace Audion
{
    public partial class AudionWindow : Form
    {
        const String settingsFilename = "audion.cfg";
        public Settings settings;

        const int CANVASCOLOR = 0x00e66e1e;
        public Audion audion;                       //controller
        public PatchCanvas canvas;                  //view

        public AudionWindow()
        {
            InitializeComponent();

            canvas = new PatchCanvas();
            canvas.setCanvasColor(Color.FromArgb(0xe6,0x6e,0x1e));
            canvas.Location = new Point(this.ClientRectangle.Left, this.audionToolStrip.Bottom);
            canvas.Size = new Size(this.ClientRectangle.Width, this.AudionStatus.Top - this.audionToolStrip.Bottom);
            this.Controls.Add(canvas);

            settings = Settings.loadSettings(settingsFilename);

            audion = new Audion(this);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (canvas != null)
            {
                canvas.Size = new Size(this.ClientRectangle.Width, this.AudionStatus.Top - this.audionToolStrip.Bottom);
            }
        }

        //- file menu -----------------------------------------------------------------

        private void newPluginFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openPluginFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void savePluginFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void savePluginAsFileMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitFileMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //- module menu ---------------------------------------------------------------

        public void manageModuleMenuItem_Click(object sender, EventArgs e)
        {

        }

        //handler for all module menu items
        //private void modSelectMenuItem_Click(object sender, EventArgs e)
        //{
        //    ToolStripItem item = (ToolStripItem)sender;
        //    String modName = (String)item.Tag;                  //get module name from menu item


        //    Module mod = audion.addModuleToPatch(modName);      //add module to graph
        //    PatchBox box;
        //    switch (modName)
        //    {
        //        case "Knob" :
        //            box = new KnobControl(mod);
        //            break;
        //        case "List":
        //            box = new ListSelectControl(mod);
        //            break;
        //        default:
        //            box = new ModuleBox(mod);                 //create new module box from unit
        //            break;
        //    }
        //    canvas.addPatchBox(box);                            //and add it to canvas
        //}

        //- plugin menu -----------------------------------------------------------------

        private void runPluginMenuItem_Click(object sender, EventArgs e)
        {

        }
        
        //- help menu -----------------------------------------------------------------

        private void aboutHelpMenuItem_Click(object sender, EventArgs e)
        {
            String msg = "Audion\nversion 1.0.0\n" +
                "\xA9 Transonic Software 2011-2020\n" +
                "http://transonic.kohoutech.com";
            MessageBox.Show(msg, "About");
        }


    }
}
