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
using System.Linq;
using System.Text;

using Kohoutech.Patch;

using Audion.Breadboard;
using Audion.UI;

namespace Audion
{
    public class Audion
    {
        public AudionWindow window;
        public PatchCanvas canvas;
        public AudionPatch patch;
        public Settings settings;

        public Dictionary<string, ModuleDef> moduleDefs;

        public Audion(AudionWindow _window)
        {
            window = _window;
            settings = window.settings;

            patch = new AudionPatch(this);
            canvas = new PatchCanvas(patch);
            patch.canvas = canvas;              //so the model can notify the canvas when it has changed

            loadModuleDefinitions();            
        }

        //- module management -------------------------------------------------

        public void loadModuleDefinitions()
        {
            moduleDefs = new Dictionary<string, ModuleDef>();

            //groups
            canvas.addPaletteGroup("modules");

            foreach (String moduleFilename in window.settings.moduleFiles)
            {
                string filename = window.settings.modPath + "\\" + moduleFilename;
                ModuleDef def = ModuleDef.loadModuleDef(filename);
                moduleDefs.Add(def.name, def);
                canvas.addPaletteItem("modules", def.name, def.name);
            }

            //built in modules
            canvas.addPaletteItem("modules", "Audio In", "AudioIn");
            canvas.addPaletteItem("modules", "Audio Out", "AudioOut");

            //controls - don't have defintions
            canvas.addPaletteGroup("controls");
            canvas.addPaletteItem("controls", "Button", "Button");
            canvas.addPaletteItem("controls", "Knob", "Knob");
            canvas.addPaletteItem("controls", "List", "List");
            canvas.addPaletteItem("controls", "Keyboard", "Keyboard");
        }

        internal ModuleDef getModuleDef(string modName)
        {
            ModuleDef def = null;
            if (moduleDefs.ContainsKey(modName))
            {
                def = moduleDefs[modName];
            }
            return def;
        }

        //- patch management -------------------------------------------------

        public void newPatch()
        {
            canvas.clearPatch();
        }

        public int loadPatch(string filename)
        {
            canvas.clearPatch();
            string filepath = settings.patchFolder + "\\" + filename;
            int result = patch.loadPatch(filepath);
            return result;
        }

        public int savePatch(string filename)
        {
            string filepath = settings.patchFolder + "\\" + filename;
            int result = patch.savePatch(filepath);
            return result;
        }

        //called when the patch has changed, if update is true, the change originated 
        //with the patch and the canvas needs to been redrawn to reflect it, otherwise
        //the change came from the canvas, and it should already up to date
        internal void patchHasChanged(bool update)
        {
            if (update)
            {
                canvas.redraw();
            }
            window.patchHasChanged();       //prevent user from closing patch w/o saving changes
        }
    }
}

////hard coding them for now
//ModuleDef def = new ModuleDef("Oscillator");
//def.addParameter(new ModuleParameter("Freq", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ListParameter("Shape", ModuleParameter.DIRECTION.IN, new List<string>() { "SINE", "TRIANGLE", "SAW", "SQUARE" }));
//            def.addParameter(new ModuleParameter("Out", ModuleParameter.DIRECTION.OUT));
//            moduleDefs.Add("Oscillator", def);
//            canvas.addPaletteItem("modules", def.name, "Oscillator");


//            def = new ModuleDef("Env Gen");
//def.addParameter(new ModuleParameter("Attack", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ModuleParameter("Decay", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ModuleParameter("Sustain", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ModuleParameter("Release", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ModuleParameter("Out", ModuleParameter.DIRECTION.OUT));
//            moduleDefs.Add("EnvGen", def);
//            canvas.addPaletteItem("modules", def.name, "EnvGen");

//            def = new ModuleDef("VCA");
//def.addParameter(new ModuleParameter("In", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ModuleParameter("Amount", ModuleParameter.DIRECTION.IN));
//            def.addParameter(new ModuleParameter("Out", ModuleParameter.DIRECTION.OUT));
//            moduleDefs.Add("VCA", def);
//            canvas.addPaletteItem("modules", def.name, "VCA");
