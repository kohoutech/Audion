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

        public static AudionPatch loadPatch(Audion _audion, String filename)
        {
            return null;
        }

        public AudionPatch(Audion _audion)
        {
            audion = _audion;
            modules = new List<Module>();
            cords = new List<PatchCord>();
            canvas = null;
        }

        internal void remove(Module module)
        {
            modules.Remove(module);
        }

        //- view interface methods --------------------------------------------

        public void setCanvas(PatchCanvas _canvas)
        {
            canvas = _canvas;
        }

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
            }
            return cord;
        }
    }
}
