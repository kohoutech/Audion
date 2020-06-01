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

using Audion.Fast;

//AIL - Audion Intermediate Language

namespace Audion.Tidepool
{
    public class AILObject
    {
        //global plugin settings
        public String effectName;
        public String productName;
        public String vendorName;
        public String pluginID;
        public int pluginVersion;
        public float samplerate;
        public int blocksize;
        public int numPrograms;
        public int numInputs;
        public int numOuputs;

        public List<AILParameter> paramList;
        public List<AILModule> moduleList;
        public List<AILPatch> patchList;

        public AILObject()
        {
            effectName = "";
            productName = "";
            vendorName = "";
            pluginID = "";
            pluginVersion = 0;
            samplerate = 44100.0f;
            blocksize = 4410;
            numPrograms = 1;
            numInputs = 0;
            numOuputs = 0;

            paramList = new List<AILParameter>();
            moduleList = new List<AILModule>();
            patchList = new List<AILPatch>();
        }
    }

    public class AILModule
    {
        public string name;
        public ModuleDef def;
        public AILPatch[] ins;
        public AILPatch[] outs;

        public AILModule(string _name, ModuleDef _def)
        {
            name = _name;
            def = _def;
            ins = new AILPatch[def.inputCount];
            outs = new AILPatch[def.outputCount];
        }
    }

    public class AILPatch
    {
        public int num;
        public AILModule source;
        public AILModule dest;

        public AILPatch(AILModule _source, AILModule _dest)
        {
            num = 0;
            source = _source;
            dest = _dest;
        }
    }


    public class AILParameter
    {
        public int num;
        public string name;
        public int modnum;
        public AILModule module;

        public AILParameter(int _num, String _name, int _modnum, AILModule _module)
        {
            num = _num;
            name = _name;
            modnum = _modnum;
            module = _module;
        }
    }
}
