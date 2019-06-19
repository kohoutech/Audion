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

namespace Transonic.VST
{
    public class VSTHost
    {
        //- host exports ------------------------------------------------------------

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiStartEngine();

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiStopEngine();

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VashtiLoadPlugin(string filename);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiUnloadPlugin(int vstnum);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiSetSampleRate(int rate);

        [DllImport("Vashti.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VashtiSetBlockSize(int size);

        //- host methods ----------------------------------------------------------

        public Vashti vashti;
        public WaveDevices waveDevices;
        public bool isEngineRunning;

        public int sampleRate;
        public int blockSize;

        public List<VSTPlugin> plugins;

        public VSTHost(Vashti _vashti)
        {
            vashti = _vashti;
            waveDevices = new WaveDevices();

            isEngineRunning = false;

            setSampleRate(44100);
            setBlockSize(2205);

            plugins = new List<VSTPlugin>();
        }

        public void shutdown()
        {
        }

        public void startEngine()
        {
            if (!isEngineRunning)
            {
                VashtiStartEngine();
                isEngineRunning = true;
            }
        }

        public void stopEngine()
        {
            if (isEngineRunning)
            {
                VashtiStopEngine();
                isEngineRunning = false;
            }
        }

        public VSTPlugin loadPlugin(string filename)
        {
            VSTPlugin plugin = null;
            int plugid = VashtiLoadPlugin(filename);        //load plugin in backend, returns -1 if load failed
            if (plugid != -1)
            {
                plugin = new VSTPlugin(this, filename, plugid);
                plugins.Add(plugin);
            }
            return plugin;
        }

        public void unloadPlugin(VSTPlugin plugin)
        {
            plugins.Remove(plugin);
            VashtiUnloadPlugin(plugin.id);
        }

        public void setSampleRate(int rate)
        {
            if (sampleRate != rate)
            {
                sampleRate = rate;
                VashtiSetSampleRate(rate);
            }
        }

        public void setBlockSize(int size)
        {
            if (blockSize != size)
            {
                blockSize = size;
                VashtiSetBlockSize(size);
            }
        }
    }
}
