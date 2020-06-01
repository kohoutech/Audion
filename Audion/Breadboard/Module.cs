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
using System.Text;

using Kohoutech.Patch;

using Audion.Fast;
using Audion.Tidepool;

namespace Audion.Breadboard
{
    public class Module : IPatchBox
    {
        public ModuleDef def;
        public String defname;
        public AudionPatch patch;
        public String name;
        public List<ModulePanel> panels;

        public int num;         //for loading and saving
        public int xpos;
        public int ypos;

        public AILModule ailmod;        //for building

        //cons for built-in modules, it's cons will add its own panels
        public Module(String _name)
        {
            patch = null;
            def = null;
            name = _name;
            panels = new List<ModulePanel>();
            num = 0;
            xpos = 0;
            ypos = 0;
            ailmod = null;
        }

        //cons for modules loaded from module definitions
        public Module(ModuleDef _def)
        {
            patch = null;
            def = _def;
            defname = def.name;
            name = def.name;

            //add module panels
            panels = new List<ModulePanel>();
            for (int i = 0; i < def.inputCount; i++)
            {
                string inname = "In" + i.ToString();
                AudioPanel inpanel = new AudioPanel(this, inname, i, JackPanel.DIRECTION.IN);
                addPanel(inpanel);
            }
            foreach (ModuleParameter parm in def.paramList)
            {
                ParamPanel parampanel = new ParamPanel(this, parm);
                addPanel(parampanel);
            }
            for (int i = 0; i < def.outputCount; i++)
            {
                string outname = "Out" + i.ToString();
                AudioPanel outpanel = new AudioPanel(this, outname, i, JackPanel.DIRECTION.OUT);
                addPanel(outpanel);
            }
        }

        public void addPanel(ModulePanel panel)
        {
            panel.num = panels.Count;       //num panel for saving / loading connection(s) from patch file
            panels.Add(panel);
        }

        //- view interface methods --------------------------------------------

        public string getTitle()
        {
            return name;
        }

        public int getWidth()
        {
            return AudionPatch.MODULEWIDTH;
        }

        public List<IPatchPanel> getPanels()
        {
            return panels.Cast<IPatchPanel>().ToList();
        }

        public void remove()
        {
            if (patch != null)
            {
                patch.removeModule(this);
            }
        }

        public void titleDoubleClick()
        {
        }

        public void setPos(int x, int y)
        {
            xpos = x;
            ypos = y;
        }
    }

    //- built-ins -------------------------------------------------------------

    public class AudioIn : Module
    {
        public AudioIn() : base("Audio In")
        {
            defname = "AudioIn";
            addPanel(new AudioPanel(this, "Left", 0, JackPanel.DIRECTION.OUT));
            addPanel(new AudioPanel(this, "Right", 1, JackPanel.DIRECTION.OUT));
        }
    }

    public class AudioOut : Module
    {
        public AudioOut() : base("Audio Out")
        {
            defname = "AudioOut";
            addPanel(new AudioPanel(this, "Left", 0, JackPanel.DIRECTION.IN));
            addPanel(new AudioPanel(this, "Right", 1, JackPanel.DIRECTION.IN));
        }
    }

    public class ControlModule : Module
    {
        public ControlModule(string name) : base(name)
        {
            defname = "control";
        }
    }
}
