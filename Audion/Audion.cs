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
            Module osc = new Module("Oscillator");
            osc.addJack(new ModuleJack("Freq", ModuleJack.DIRECTION.IN));
            osc.addJack(new ModuleJack("Shape", ModuleJack.DIRECTION.IN));
            osc.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
            window.addModuleToMenu(osc);

            Module filt = new Module("Filter");
            filt.addJack(new ModuleJack("In", ModuleJack.DIRECTION.IN));
            filt.addJack(new ModuleJack("Cutoff", ModuleJack.DIRECTION.IN));
            filt.addJack(new ModuleJack("Resonance", ModuleJack.DIRECTION.IN));
            filt.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
            window.addModuleToMenu(filt);

            Module adsr = new Module("Env Gen");
            adsr.addJack(new ModuleJack("Attack", ModuleJack.DIRECTION.IN));
            adsr.addJack(new ModuleJack("Decay", ModuleJack.DIRECTION.IN));
            adsr.addJack(new ModuleJack("Sustain", ModuleJack.DIRECTION.IN));
            adsr.addJack(new ModuleJack("Release", ModuleJack.DIRECTION.IN));
            adsr.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
            window.addModuleToMenu(adsr);

            Module vca = new Module("VCA");
            vca.addJack(new ModuleJack("In", ModuleJack.DIRECTION.IN));
            vca.addJack(new ModuleJack("Amount", ModuleJack.DIRECTION.IN));
            vca.addJack(new ModuleJack("Out", ModuleJack.DIRECTION.OUT));
            window.addModuleToMenu(vca);
        }

        public void addModuleToPatch(Module module)
        {
        }
    }
}
