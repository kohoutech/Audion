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

#if !defined(VSTHOST_H)
#define VSTHOST_H

#include "vstsdk2.4/audioeffectx.h"
#include <windows.h>                    

class VSTPlugin;
class WaveOutDevice;

class VSTHost
{
public:
	VSTHost();
	~VSTHost();

	void setSampleRate(int rate);
	void setBlockSize (int size);

	void startEngine();
	void stopEngine();
	BOOL isEngineRunning() { return isRunning; }

	//plugin
	int loadPlugin(const char * filename);
    void unloadPlugin(int idx);
    void unloadAll();
	VSTPlugin* getPlugin(int idx);

protected:
    int sampleRate;
    long blockSize;
	float * emptyBuf[2];
	BOOL isRunning;

	VSTPlugin** plugins;		//array of plugins
	int pluginMax;				//array size
	int pluginCount;			//num of loaded plugins

	//timing
	UINT timerDuration;
	UINT timerID;
	TIMECAPS tc;
	DWORD dwRest;
	DWORD dwLastTime;
	DWORD dwStartStamp;

	void initTimer();
	BOOL startTimer();
	void stopTimer();
	static void CALLBACK timerCallback(UINT uID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);
	void handleTimer();

	//audio
	int waveOutDevId;
	WaveOutDevice* waveOut;
	BOOL resetWaveOutDevice();
	void processAudio(float **pBuffer, int nLength, int nChannels = 2, DWORD dwStamp = 0);
	float * pOutputs[2];
	//CRITICAL_SECTION cs;
};

#endif // VSTHOST_H
