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

namespace Audion
{
    public class Settings
    {
        public string version;
        public string modPath;
        public List<string> moduleFiles;

        public static Settings loadSettings(String filename)
        {
            Settings settings = new Settings();

            EnamlData data = EnamlData.loadFromFile(filename);

            //global
            settings.version = data.getStringValue("Audion.version", "");
            settings.modPath = data.getStringValue("Audion.modulefolder", "");

            //modules
            settings.moduleFiles = new List<string>();
            List<String> modules = data.getPathKeys("Modules");
            foreach(String module in modules)
            {
                string modfilename = data.getStringValue("Modules." + module + ".filename","");
                settings.moduleFiles.Add(modfilename);
            }

            return settings;
        }

        public void saveSettings(String filename)
        {

        }
    }
}
