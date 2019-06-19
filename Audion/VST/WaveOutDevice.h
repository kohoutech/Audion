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

#if !defined(WAVEOUTDEVICE_H)
#define WAVEOUTDEVICE_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

#include "WaveBuffer.h"

class WaveOutDevice
{
public:
	WaveOutDevice();
	virtual ~WaveOutDevice();

//basic operations
	BOOL open(int devID, DWORD sampleRate = 44100, WORD bitDepth = 16, WORD channels = 2);
	BOOL start();
	void writeOut(float **outData, int size);
	BOOL pause();
	BOOL stop();
	BOOL close();

//currently opened wave device attributes
    BOOL isDevOpen() { return isOpen; }

    int getSampleRate()      { return (isOpen) ? (int)wf.nSamplesPerSec : 0; }
    int getBitDepth()		 { return (isOpen) ? (int)wf.wBitsPerSample : 0; }
    int getChannelCount()    { return (isOpen) ? (int)wf.nChannels : 0; }
    int getBytesPerSecond()  { return (isOpen) ? (int)wf.nAvgBytesPerSec : 0; }
    int getBlockAlignment()  { return (isOpen) ? (int)wf.nBlockAlign : 0; }

//buffer mgmt
	int getBufferCount() { return bufferCount; }
	void setBufferCount(int count) { bufferCount = count;}

	int getBufferDuration() {return bufferDuration; }
	void setBufferDuration(int duration);

protected:
	BOOL isOpen;
    HWAVEOUT hDev;                      
	WAVEFORMATEX wf;
	BOOL isPlaying;

	WaveBuffer **buffers;
	int bufferSize;
	int bufferCount;				
	int bufferDuration;				//buf duration in ms

	void allocateBuffers();
	void freeBuffers();

//the waveoutproc callback
    static void CALLBACK WaveOutProc(HMIDIOUT hWaveOut, UINT wMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2);
};

#endif // WAVEOUTDEVICE_H
