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

using Kohoutech.ENAML;
using Kohoutech.Patch;

using Audion.UI;

namespace Audion.Breadboard
{
    public class AudionPatch : IPatchModel
    {
        public const int MODULEWIDTH = 100;
        public const int PANELHEIGHT = 20;

        public Audion audion;
        public List<Module> modules;
        public List<PatchCord> cords;

        public PatchCanvas canvas;

        public void loadPatch(Audion _audion, String filename)
        {
            
        }

        public int savePatch(String filename)
        {
            EnamlData data = new EnamlData();

            //global
            data.setStringValue("Audion.version", audion.settings.version);

            //modules
            for(int i = 0; i < modules.Count; i++)
            {
                Module mod = modules[i];
                mod.num = i;
                string modpath = "Modules.module" + (i + 1).ToString("D3");
                data.setStringValue(modpath + ".name", mod.name);
                data.setStringValue(modpath + ".def", mod.defname);
                data.setIntValue(modpath + ".xpos", mod.xpos);
                data.setIntValue(modpath + ".ypos", mod.ypos);
            }

            //cords
            for (int i = 0; i < cords.Count; i++)
            {
                PatchCord cord = cords[i];
                string modpath = "Cords.cord" + (i + 1).ToString("D3");
                data.setIntValue(modpath + ".sourcemod", cord.source.module.num);
                data.setIntValue(modpath + ".sourcepanel", cord.source.num);
                data.setIntValue(modpath + ".destmod", cord.dest.module.num);
                data.setIntValue(modpath + ".destpanel", cord.dest.num);
            }

            data.saveToFile(filename);

            return 0;
        }

        public AudionPatch(Audion _audion)
        {
            audion = _audion;
            modules = new List<Module>();
            cords = new List<PatchCord>();
            canvas = null;
        }

        internal void removeModule(Module module)
        {
            modules.Remove(module);
            audion.patchHasChanged(false);
        }

        internal void removeCord(PatchCord cord)
        {
            cords.Remove(cord);
            audion.patchHasChanged(false);
        }

        //- canvas interface methods --------------------------------------------

        //create patch module from module name, add it to patch & pass it back to canvas
        //so that canvas can graphically display the module in the patch
        public IPatchBox getPatchBox(string modName)
        {
            Module module = null;
            switch (modName)
            {
                //built-ins
                case "AudioIn":
                    module = new AudioIn();
                    break;

                case "AudioOut":
                    module = new AudioOut();
                    break;

                //controls
                case "Button":
                    module = new ButtonControl();
                    break;

                case "Knob":
                    module = new KnobControl();
                    break;

                case "List":
                    module = new ListSelectControl();
                    break;

                case "Keyboard":
                    module = new KeyboardControl();
                    break;

                default:
                    ModuleDef def = audion.getModuleDef(modName);
                    module = new Module(def);
                    break;
            }
            modules.Add(module);
            module.patch = this;
            audion.patchHasChanged(false);
            return module;
        }

        //create a patch cord and connect it to the source and dest jack panels
        //then return it to the canvas so the panels will be connected on the canvas too
        public IPatchWire getPatchWire(IPatchPanel source, IPatchPanel dest)
        {
            PatchCord cord = null;
            if (source is JackPanel && dest is JackPanel)
            {
                cord = new PatchCord(this, (JackPanel)source, (JackPanel)dest);
                cords.Add(cord);
            }
            audion.patchHasChanged(false);
            return cord;
        }

        public void layoutHasChanged()
        {
            audion.patchHasChanged(false);
        }
    }
}
