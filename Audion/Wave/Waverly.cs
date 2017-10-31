/* ----------------------------------------------------------------------------
LibTransWave : a library for playing, editing and storing audio wave data
Copyright (C) 2005-2017  George E Greaney

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

using Transonic.Wave.System;

namespace Transonic.Wave
{
    public class Waverly
    {
        //communication with wave.dll
        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WaverlyInit();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WaverlyShutDown();

//transport calls -----------------------------------------------------

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportPlay();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportStop();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportPause();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportRewind();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportFastForward(int speed);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportRecord();

        //[DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void TransportSetVolume(float volume);

        //[DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void TransportSetBalance(float balance);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportSetLeftLevel(float level);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportSetRightLevel(float level);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportGetLeftLevel();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern float TransportGetRightLevel();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int TransportGetCurrentPos();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int TransportSetCurrentPos(int pos);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TransportSetWaveOut(int deviceIdx);
        
//audio data calls -------------------------------------------------------

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioNew(int SampleRate, int duration);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioOpen(string filename);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioClose();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioSave(string filename);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AudioGetSampleRate();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AudioGetDataSize();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AudioGetDuration();

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AudioImportWavFile(string filename);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool AudioExportWavFile(string filename);

//channel calls --------------------------------------------------------------

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ProjectAddChannel(int channelNum);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ProjectDeleteChannel(int channelNum);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ChannelSetWaveIn(int channelNum, int deviceIdx);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelVolume(int channelNum, float volume);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelPan(int channelNum, float pan);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelMute(int channelNum, bool mute);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelRecord(int channelNum, bool record);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LoadChannelData(int channelNum, IntPtr inhdl);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SaveChannelData(int channelNum, IntPtr outhdl);

        [DllImport("Waverly.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PaintChannelData(int channelNum, IntPtr hdc, int width, int startpos);

//- signals methods -----------------------------------------------------------

        IWaveView waveWindow;        
        public WaveDevices waveDevices;

        public Waverly(IWaveView _mw)
        {
            waveWindow = _mw;
            waveDevices = new WaveDevices(this);
            WaverlyInit();
        }

        public void shutDown() 
        {
            WaverlyShutDown();
        }

//- transport methods ---------------------------------------------------------

        public void playTransport()
        {
            TransportPlay();
        }

        public void pauseTransport()
        {
            TransportPause();
        }

        public void stopTransport()
        {
            TransportStop();
        }

        public void rewindTransport()
        {
            TransportRewind();
        }

        public void fastForwardTransport(int speed)
        {
            //TransportFastForward(speed);      //doesn't work!
        }

        public void recordTransport()
        {
            TransportRecord();
        }

        public void setTransportLeftLevel(float level)
        {
            TransportSetLeftLevel(level);
        }

        public void setTransportRightLevel(float level)
        {
            TransportSetRightLevel(level);
        }

        public float getTransportLeftLevel()
        {
            return TransportGetLeftLevel();
        }

        public float getTransportRightLevel()
        {
            return TransportGetRightLevel();
        }

        public int getCurrentTransportPos()
        {
            return TransportGetCurrentPos();    //in samples
        }

        public void setCurrentTransportPos(int pos)
        {
            TransportSetCurrentPos(pos);		//in samples
        }

        public void setTransportOutDevice(int devIdx)
        {
            TransportSetWaveOut(devIdx);
        }
        
//- audio data methods --------------------------------------------------------

        public void newAudioProject(int sampleRate, int duration)
        {
            AudioNew(sampleRate, duration);
        }

        public void openAudioProject(String filename)
        {
            AudioOpen(filename);
        }

        public void closeAudioProject()
        {
            AudioClose();
        }

        public int importWaveFile(string filename)
        {
            return AudioImportWavFile(filename);
        }

        public int getAudioSampleRate()
        {
            return AudioGetSampleRate();
        }

        public int getAudioDataSize()
        {
            return AudioGetDataSize();
        }

        public int getAudioDuration()
        {
            return AudioGetDuration();
        }

        public List<String> getInputDeviceList()
        {
            return waveDevices.getInDevNameList();
        }

        public List<String> getOutputDeviceList()
        {
            return waveDevices.getOutDevNameList();
        }


//- channel methods -----------------------------------------------------------

        public void addChannel(int channelNum)
        {
            ProjectAddChannel(channelNum);
        }

        public void deleteChannel(int channelNum)
        {
            ProjectDeleteChannel(channelNum);
        }

        public void loadChannelData(int channelNum, IntPtr inhdl)
        {
            LoadChannelData(channelNum, inhdl);
        }

        public void setChannelWaveIn(int channelNum, int deviceIdx)
        {
            ChannelSetWaveIn(channelNum, deviceIdx);
        }

        public void setChannelVolume(int channelNum, float volume)
        {
            SetChannelVolume(channelNum, volume);
        }

        public void setChannelPan(int channelNum, float volume)
        {
            SetChannelPan(channelNum, volume);
        }

        public void setChannelMute(int channelNum, bool mute)
        {
            SetChannelMute(channelNum, mute);
        }

        public void setChannelRecord(int channelNum, bool record)
        {
            SetChannelRecord(channelNum, record);
        }

        public void paintChannelData(int channelNum, IntPtr hdc, int width, int startpos)
        {
            PaintChannelData(channelNum, hdc, width, startpos);
        }

        public void saveChannelData(int channelNum, IntPtr outhdl)
        {
            SaveChannelData(channelNum, outhdl);
        }

        public bool exportToWaveFile(string filename)
        {
            return AudioExportWavFile(filename);
        }
    }
}

//  Console.WriteLine(" there's no sun in the shadow of the wizard");
