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
using System.Text;

using Audion.Graph;
using Audion.UI;

namespace Audion
{
    public class Audion
    {
        public void initModuleMenu(AudionWindow window)
        {
            //modules
            window.addModuleToMenu("Oscillator");
            window.addModuleToMenu("Filter");
            window.addModuleToMenu("Env Gen");
            window.addModuleToMenu("VCA");
            window.addModuleToMenu("Audio Out");

            //controls
            window.addModuleToMenu("Knob");
            window.addModuleToMenu("List");
        }

        public Module addModuleToPatch(String modName)
        {
            Module mod = null;

            switch (modName)
            {
                case "Oscillator":
                    {
                        mod = new Module("Oscillator");
                        mod.addJack(new ModuleJack("Freq", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Shape", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
                        break;
                    }
                case "Filter":
                    {
                        mod = new Module("Filter");
                        mod.addJack(new ModuleJack("In", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Cutoff", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Resonance", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
                        break;
                    }
                case "Env Gen":
                    {
                        mod = new Module("Env Gen");
                        mod.addJack(new ModuleJack("Attack", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Decay", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Sustain", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Release", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
                        break;
                    }
                case "VCA":
                    {
                        mod = new Module("VCA");
                        mod.addJack(new ModuleJack("In", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Amount", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
                        break;
                    }
                case "Audio Out":
                    {
                        mod = new Module("Audio Out");
                        mod.addJack(new ModuleJack("Left", ModuleJack.DIRECTION.IN));
                        mod.addJack(new ModuleJack("Right", ModuleJack.DIRECTION.IN));
                        break;
                    }

                //controls
                case "Knob":
                    {
                        mod = new Module("Knob");
                        mod.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
                        break;
                    }
                case "List":
                    {
                        mod = new Module("List");
                        mod.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
                        break;
                    }
            }

            return mod;
        }

    }
}
