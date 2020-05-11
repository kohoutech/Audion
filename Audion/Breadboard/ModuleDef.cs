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

namespace Audion.Breadboard
{
    public class ModuleDef
    {
        public String name;
        public List<ModuleParameter> paramList;

        public static ModuleDef loadModuleDef(String filename)
        {
            return null;
        }

        public ModuleDef(String _name)
        {
            name = _name;
            paramList = new List<ModuleParameter>();
        }

        public void addParameter(ModuleParameter param)
        {
            paramList.Add(param);
        }
    }

    //-------------------------------------------------------------------------

    public class ModuleParameter
    {
        public enum DIRECTION
        {
            IN,
            OUT
        }

        public string name;
        public DIRECTION dir;

        public ModuleParameter(String _name, DIRECTION _dir)
        {
            name = _name;
            dir = _dir;
        }
    }

    //-------------------------------------------------------------------------

    public class ListParameter : ModuleParameter
    {
        public List<String> paramList;

        public ListParameter(String _name, DIRECTION _dir, List<String> _paramList) : base(_name, _dir)
        {
            paramList = _paramList;
        }
    }
}
