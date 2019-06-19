/* ----------------------------------------------------------------------------
Transonic VST Library
Copyright (C) 2005-2019  George E Greaney

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
using System.Runtime.InteropServices;

using Audimat;
using Audimat.UI;

namespace Transonic.VST
{
    public class VSTPlugin
    {
        //- plugin exports ------------------------------------------------------------

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiSetPluginAudioIn(int vstnum, int audioidx);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiSetPluginAudioOut(int vstnum, int audioidx);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiGetPluginInfo(int vstnum, ref PluginInfo pinfo);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern String VashtiGetParamName(int vstnum, int paramnum);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float VashtiGetParamValue(int vstnum, int paramnum);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiSetParamValue(int vstnum, int paramnum, float paramval);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern String VashtiGetProgramName(int vstnum, int prognum);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiSetProgram(int vstnum, int prognum);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiOpenEditor(int vstnum, IntPtr hwnd);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiCloseEditor(int vstnum);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiHandleMidiMsg(int vstnum, int b1, int b2, int b3);

        //---------------------------------------------------------------------

        public VSTHost host;
        public String filename;

        public int audioInIdx;
        public int audioOutIdx;

        //these are supplied by the plugin
        public int id;
        public String name;
        public String vendor;
        public int version;
        public int numPrograms;
        public int numParams;
        public int numInputs;
        public int numOutputs;
        public int flags;
        public int uniqueID;

        public VSTParam[] parameters;
        public VSTProgram[] programs;
        int curProgramNum;

        public bool hasEditor;
        public int editorWidth;
        public int editorHeight;

        //cons
        public VSTPlugin(VSTHost _host, String _filename, int _id)
        {
            host = _host;
            filename = _filename;
            id = _id;

            //-1 == not set yet
            audioInIdx = -1;
            audioOutIdx = -1;

            //get info from plugin
            PluginInfo pluginfo = new PluginInfo();
            getPluginInfo(ref pluginfo);
            name = pluginfo.name;
            vendor = pluginfo.vendor;
            version = pluginfo.version;
            numPrograms = pluginfo.numPrograms;
            numParams = pluginfo.numParameters;
            numInputs = pluginfo.numInputs;
            numOutputs = pluginfo.numOutputs;
            flags = pluginfo.flags;
            uniqueID = pluginfo.uniqueID;
            editorWidth = pluginfo.editorWidth;
            editorHeight = pluginfo.editorHeight;

            parameters = new VSTParam[numParams];
            for (int i = 0; i < numParams; i++)
            {
                String paramName = getParamName(i);
                float paramVal = getParamValue(i);
                parameters[i] = new VSTParam(i, paramName, paramVal);
            }

            if (numPrograms > 0)
            {
                programs = new VSTProgram[numPrograms];
                for (int i = 0; i < numPrograms; i++)
                {
                    String progName = getProgramName(i);
                    programs[i] = new VSTProgram(i, progName);
                }
            }
            else
            {
                //programs = new VSTProgram[1];
                programs = null;
                //programs[0] = new VSTProgram(0, "no programs");
            }
            curProgramNum = 0;
        }

        //- settings ----------------------------------------------------------

        public void setAudioIn(int idx)
        {
            if (audioInIdx != idx)
            {
                audioInIdx = idx;
            }
        }

        public void setAudioOut(int idx)
        {
            if (audioOutIdx != idx)
            {
                audioOutIdx = idx;
            }
        }

        //- backend methods ----------------------------------------------------------

        public void getPluginInfo(ref PluginInfo pluginfo)
        {
            VashtiGetPluginInfo(id, ref pluginfo);
        }

        public String getParamName(int paramnum)
        {
            return VashtiGetParamName(id, paramnum);
        }

        public float getParamValue(int paramnum)
        {
            return VashtiGetParamValue(id, paramnum);
        }

        public void setParamValue(int paramnum, float paramval)
        {
            parameters[paramnum].value = paramval;
            VashtiSetParamValue(id, paramnum, paramval);
        }

        public String getProgramName(int prognum)
        {
            return VashtiGetProgramName(id, prognum);
        }

        public void setProgram(int prognum)
        {
            curProgramNum = prognum;
            VashtiSetProgram(id, prognum);
        }

        public void openEditorWindow(IntPtr hwnd)
        {
            VashtiOpenEditor(id, hwnd);
        }

        public void closeEditorWindow()
        {
            VashtiCloseEditor(id);
        }

        public void sendShortMidiMessage(int b1, int b2, int b3)
        {
            VashtiHandleMidiMsg(id, b1, b2, b3);
        }        
    }

    //-----------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public struct PluginInfo
    {
        public string name;
        public string vendor;
        public int version;
        public int numPrograms;
        public int numParameters;
        public int numInputs;
        public int numOutputs;
        public int flags;
        public int uniqueID;
        public int editorWidth;
        public int editorHeight;
    }

    public class VSTParam
    {
        public int num;
        public String name;
        public float value;

        public VSTParam(int _num, String _name, float _val)
        {
            num = _num;
            name = _name;
            value = _val;
        }
    }

    public class VSTProgram
    {
        public int num;
        public String _name;

        public String name
        {
            get { return _name; }
        }

        public VSTProgram(int _num, String __name)
        {
            num = _num;
            _name = __name;
        }
    }
}

//  Console.WriteLine(" there's no sun in the shadow of the wizard");
