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

        public bool winposset;
        public int audWndX;
        public int audWndY;
        public int audWndWidth;
        public int audWndHeight;

        public string modPath;
        public List<string> moduleFiles;

        public static Settings loadSettings(String filename)
        {
            Settings settings = new Settings();

            EnamlData data = EnamlData.loadFromFile(filename);
            if (data == null) return settings;                      //didn't find a config file, return default settings value

            //global
            settings.version = data.getStringValue("Audion.version", "");

            settings.audWndX = data.getIntValue("Audion.windowX", 0);            
            settings.audWndY = data.getIntValue("Audion.windowY", 0);
            settings.audWndWidth = data.getIntValue("Audion.windowWidth", 600);
            settings.audWndHeight = data.getIntValue("Audion.windowHeight", 400);
            settings.winposset = true;

            //modules
            settings.modPath = data.getStringValue("Audion.modulefolder", "");
            settings.moduleFiles.Clear();
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
            EnamlData data = new EnamlData();

            //global
            data.setStringValue("Audion.version", version);

            data.setIntValue("Audion.windowX", audWndX);
            data.setIntValue("Audion.windowY", audWndY);
            data.setIntValue("Audion.windowWidth", audWndWidth);
            data.setIntValue("Audion.windowHeight", audWndHeight);

            //module
            data.setStringValue("Audion.modulefolder", modPath);
            for (int i = 0; i < moduleFiles.Count; i++)
            {
                data.setStringValue("Modules.module" + (i + 1).ToString("D4") + ".filename", moduleFiles[i]);                    
            }

            data.saveToFile(filename);
        }

        public Settings()
        {
            version = "0.1.0";

            winposset = false;
            audWndX = 0;
            audWndY = 0;
            audWndWidth = 600;
            audWndHeight = 400;

            modPath = Environment.CurrentDirectory;
            moduleFiles = new List<string>();
        }
    }
}
