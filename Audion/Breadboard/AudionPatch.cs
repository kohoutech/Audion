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

using Audion.Fast;
using Audion.UI;
using Audion.Tidepool;

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

        //global plugin settings
        public String effectName;
        public String productName;
        public String vendorName;
        public String pluginID;
        public int pluginVersion;

        public List<PatchParameter> paramList;

        //cons
        public AudionPatch(Audion _audion)
        {
            audion = _audion;
            modules = new List<Module>();
            cords = new List<PatchCord>();
            canvas = null;

            paramList = new List<PatchParameter>();
            clearSettings();
        }

        //- loading / saving to file ------------------------------------------

        public int loadPatch(String filename)
        {
            EnamlData data = EnamlData.loadFromFile(filename);

            String version = data.getStringValue("Audion.version", "");

            //globals
            effectName = data.getStringValue("Plugin.effect", "");
            productName = data.getStringValue("Plugin.product", "");
            vendorName = data.getStringValue("Plugin.vendor", "");
            pluginID = data.getStringValue("Plugin.plugid", "");
            pluginVersion = data.getIntValue("Plugin.plugversion", 0);

            //modules
            List<String> modulenames = data.getPathKeys("Modules");
            foreach (String modulename in modulenames)
            {
                string modpath = "Modules." + modulename;
                string modname = data.getStringValue(modpath + ".name", "");
                string moddef = data.getStringValue(modpath + ".def", "");
                int modxpos = data.getIntValue(modpath + ".xpos", 0);
                int modypos = data.getIntValue(modpath + ".ypos", 0);

                IPatchBox mod = getPatchBox(moddef);
                ((Module)mod).name = modname;
                canvas.addPatchBox(mod, modxpos, modypos);
            }

            //cords
            List<String> cordnames = data.getPathKeys("Cords");
            foreach (String cordname in cordnames)
            {
                string cordpath = "Cords." + cordname;
                int srcmodnum = data.getIntValue(cordpath + ".srcmod", 0);
                int srcpanelnum = data.getIntValue(cordpath + ".srcpanel", 0);
                int destmodnum = data.getIntValue(cordpath + ".destmod", 0);
                int destpanelnum = data.getIntValue(cordpath + ".destpanel", 0);

                Module srcmod = modules[srcmodnum];
                ModulePanel srcpanel = srcmod.panels[srcpanelnum];
                Module destmod = modules[destmodnum];
                ModulePanel destpanel = destmod.panels[destpanelnum];

                IPatchWire cord = getPatchWire(srcpanel, destpanel);
                canvas.addPatchWire(cord, srcpanel.panel, destpanel.panel);
            }

            return 0;
        }

        public int savePatch(String filename)
        {
            EnamlData data = new EnamlData();

            data.setStringValue("Audion.version", audion.settings.version);

            //globals
            data.setStringValue("Plugin.effect", effectName);
            data.setStringValue("Plugin.product", productName);
            data.setStringValue("Plugin.vendor", vendorName);
            data.setStringValue("Plugin.plugid", pluginID);
            data.setIntValue("Plugin.plugversion", pluginVersion);

            //modules
            for (int i = 0; i < modules.Count; i++)
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
                string cordpath = "Cords.cord" + (i + 1).ToString("D3");
                data.setIntValue(cordpath + ".srcmod", cord.source.module.num);
                data.setIntValue(cordpath + ".srcpanel", cord.source.num);
                data.setIntValue(cordpath + ".destmod", cord.dest.module.num);
                data.setIntValue(cordpath + ".destpanel", cord.dest.num);
            }

            data.saveToFile(filename);

            return 0;
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
            addParams(module);
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

        //- patch management --------------------------------------------------

        public void clearSettings()
        {
            paramList.Clear();

            //for debugging
            effectName = "Test1";
            productName = "Test1";
            vendorName = "Audion";
            pluginID = "test";
            pluginVersion = 158;
        }

        //has the user given the plugin settings values yet?
        // false if we still need to set them
        public bool checkSettings()
        {
            return ((effectName.Length != 0) && (productName.Length != 0) && (vendorName.Length != 0) &&
                (pluginID.Length != 0) && (pluginVersion != 0));
        }

        internal void addParams(Module module)
        {
            if (module.def != null)
            {
                int pcount = paramList.Count;
                int mcount = 0;
                foreach (ModuleParameter mparm in module.def.paramList)
                {
                    PatchParameter pparm = new PatchParameter(mparm.name, mcount++, module);
                    pparm.num = pcount++;
                    paramList.Add(pparm);
                }
            }
        }

        internal void removeParams(Module module)
        {
            if (module.def != null)
            {
                List<PatchParameter> tempList = new List<PatchParameter>();
                foreach(PatchParameter pparm in paramList)
                {
                    if (pparm.module != module)
                    {
                        tempList.Add(pparm);
                    }
                }
                paramList = tempList;

                //renumber the remaining ones
                int pcount = 0;
                foreach (PatchParameter pparm in paramList)
                {
                    pparm.num = pcount++;
                }
            }
        }

        internal void removeModule(Module module)
        {
            removeParams(module);
            modules.Remove(module);
            audion.patchHasChanged(false);
        }

        internal void removeCord(PatchCord cord)
        {
            cords.Remove(cord);
            audion.patchHasChanged(false);
        }

        //- plugin generation ------------------------------------------------

        internal AILObject generateAIL()
        {
            AILObject obj = new AILObject();

            obj.effectName = effectName;
            obj.productName = productName;
            obj.vendorName = vendorName;
            obj.pluginID = pluginID;
            obj.pluginVersion = pluginVersion;

            //build module list
            List<AILModule> amods = new List<AILModule>();
            int i = 0;
            foreach(Module pmod in modules)
            {
                pmod.num = i;
                if (!(pmod is ControlModule))           //don't include controls
                {
                    AILModule amod = new AILModule(pmod.name, pmod.def);
                    pmod.ailmod = amod;
                    amods.Add(amod);
                }
            }

            i = 0;
            foreach (PatchCord cord in cords)
            {
                if (cord.source.module is AudioOut)         //only link modules by audio patch cords
                {
                    AILModule srcmod = cord.source.module.ailmod;
                    int srcnum = ((AudioPanel)cord.source).panelnum;
                    AILModule destmod = cord.dest.module.ailmod;
                    int destnum = ((AudioPanel)cord.source).panelnum;

                    AILPatch patch = new AILPatch(srcmod, destmod);
                    patch.num = i;
                    srcmod.outs[srcnum] = patch;
                    destmod.ins[destnum] = patch;
                }
            }                

            return obj;
        }
    }

    //-------------------------------------------------------------------------

    public class PatchParameter
    {
        public int num;
        public string name;
        public int modnum;
        public Module module;

        public PatchParameter(String _name, int _modnum, Module _module)
        {
            num = 0;
            name = _name;
            modnum = _modnum;
            module = _module;
        }
    }
}
