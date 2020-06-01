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
using System.IO;

using Kohoutech.Win32;

namespace Audion.Fast
{
    public class ModuleDef
    {
        const uint SIGNATURE = 0x54534146;      //"FAST" (little endian)

        public String name;
        public String fullName;
        public String version;
        public String vendor;
        public int inputCount;
        public int outputCount;

        public List<ModuleParameter> paramList;

        public string initFunc;
        public string paramSetFunc;
        public string paramGetFunc;
        public string paramDisplayFunc;
        public string paramLabelFunc;
        public string preprocessFunc;
        public string processFunc;
        public string postprocessFunc;

        public ModuleBlock codeBlock;
        public ModuleBlock dataBlock;
        public ModuleBlock bssBlock;

        //load def obj from xxx.mod file
        public static ModuleDef loadModuleDef(String filename)
        {
            SourceFile modfile = new SourceFile(filename);

            //module header - check sig first
            uint sig = modfile.getFour();
            if (sig != SIGNATURE)
            {
                return null;
            }

            uint modversion = modfile.getFour();
            string name = modfile.getAsciiZString();
            ModuleDef def = new ModuleDef(name);

            def.fullName = modfile.getAsciiZString();
            def.version = modfile.getAsciiZString();
            def.vendor = modfile.getAsciiZString();
            def.inputCount = (int)modfile.getTwo();
            def.outputCount = (int)modfile.getTwo();

            //param list
            uint paramCount = modfile.getTwo();
            for (int i = 0; i < paramCount; i++)
            {
                string paramname = modfile.getAsciiZString();
                ModuleParameter.TYPE paramtype = (ModuleParameter.TYPE)modfile.getOne();
                switch (paramtype)
                {
                    case ModuleParameter.TYPE.RANGE:
                        ModuleParameter rangeparam = new ModuleParameter(paramname);
                        def.addParameter(rangeparam);
                        break;

                    case ModuleParameter.TYPE.ONOFF:
                        OnOffParameter onoffparam = new OnOffParameter(paramname);
                        def.addParameter(onoffparam);
                        break;

                    case ModuleParameter.TYPE.LIST:
                        uint itemCount = modfile.getTwo();
                        List<String> itemList = new List<string>();
                        for (int j = 0; j < itemCount; j++)
                        {
                            string itemname = modfile.getAsciiZString();
                            itemList.Add(itemname);
                        }
                        ListParameter listparam = new ListParameter(paramname, itemList);
                        def.addParameter(listparam);
                        break;

                    default:
                        break;
                }
            }

            //func list
            def.initFunc = modfile.getAsciiZString();
            def.paramSetFunc = modfile.getAsciiZString();
            def.paramGetFunc = modfile.getAsciiZString();
            def.paramDisplayFunc = modfile.getAsciiZString();
            def.paramLabelFunc = modfile.getAsciiZString();
            def.preprocessFunc = modfile.getAsciiZString();
            def.processFunc = modfile.getAsciiZString();
            def.postprocessFunc = modfile.getAsciiZString();
            
            //block list
            uint blockCount = modfile.getOne();
            for (int i = 0; i < blockCount; i++)
            {
                //block header
                string blockname = modfile.getAsciiZString();
                ModuleBlock block = new ModuleBlock(blockname);
                block.addr = modfile.getFour();
                block.size = modfile.getFour();
                uint importstart = modfile.getFour();
                uint importcount = modfile.getFour();
                uint exportstart = modfile.getFour();
                uint exportcount = modfile.getFour();

                //block data
                uint mark = modfile.getPos();
                modfile.seek(block.addr);
                block.data = modfile.getRange(block.size);
                modfile.seek(importstart);
                for (int j = 0; j < importcount; j++)
                {
                    string importsym = modfile.getAsciiZString();
                    uint importaddr = modfile.getFour();
                    ModuleFixup import = new ModuleFixup(importsym, importaddr);
                    block.imports.Add(import);
                }
                modfile.seek(exportstart);
                for (int j = 0; j < exportcount; j++)
                {
                    string exportsym = modfile.getAsciiZString();
                    uint exportaddr = modfile.getFour();
                    ModuleFixup export = new ModuleFixup(exportsym, exportaddr);
                    block.exports.Add(export);
                }
                modfile.seek(mark);
                switch (block.name)
                {
                    case "CODE":
                        def.codeBlock = block;
                        break;

                    case "DATA":
                        def.dataBlock = block;
                        break;

                    case "BSS":
                        def.bssBlock = block;
                        break;

                    default:
                        break;
                }
            }


            //def.addParameter(new ModuleParameter("In", ModuleParameter.DIRECTION.IN));
            //def.addParameter(new ModuleParameter("Cutoff", ModuleParameter.DIRECTION.IN));
            //def.addParameter(new ModuleParameter("Resonance", ModuleParameter.DIRECTION.IN));
            //def.addParameter(new ModuleParameter("Out", ModuleParameter.DIRECTION.OUT));

            return def;
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
    // PARAMETER CLASSES
    //-------------------------------------------------------------------------

    //base class
    public class ModuleParameter
    {
        public enum TYPE { RANGE, ONOFF, LIST };
        public string[] typeNames = { "Range", "OnOff", "List" };

        public string name;
        public TYPE typ;

        public ModuleParameter(String _name)
        {
            name = _name;
            typ = TYPE.RANGE;
        }

        public string getParamType()
        {
            return typeNames[(int)typ];
        }
    }

    //-------------------------------------------------------------------------

    public class OnOffParameter : ModuleParameter
    {

        public OnOffParameter(String _name) : base(_name)
        {
            typ = TYPE.ONOFF;
        }
    }

    //-------------------------------------------------------------------------

    public class ListParameter : ModuleParameter
    {
        public List<String> paramList;

        public ListParameter(String _name, List<String> _paramList) : base(_name)
        {
            typ = TYPE.LIST;
            paramList = _paramList;
            
        }
    }

    //-------------------------------------------------------------------------
    // MODULE CODE/DATA BLOCKS
    //-------------------------------------------------------------------------

    public class ModuleBlock
    {
        public string name;
        public uint addr;
        public uint size;
        public byte[] data;
        public List<ModuleFixup> imports;
        public List<ModuleFixup> exports;

        public ModuleBlock(string _name)
        {
            name = _name;
            addr = 0;
            size = 0;
            data = null;
            imports = new List<ModuleFixup>();
            exports = new List<ModuleFixup>();
        }
    }

    public class ModuleFixup
    {
        public string symbol;
        public uint addr;

        public ModuleFixup(string _sym, uint _addr)
        {
            symbol = _sym;
            addr = _addr;
        }
    }
}
