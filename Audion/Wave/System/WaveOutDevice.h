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

#if !defined(WAVEOUTDEVICE_H)
#define WAVEOUTDEVICE_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

class Waverly;
class WaveBuffer;

class WaveOutDevice
{
public:
	WaveOutDevice(Waverly *_waverly);
	virtual ~WaveOutDevice();

//basic operations
	BOOL open(int devID, DWORD sampleRate = 44100, WORD bitDepth = 16, WORD channels = 2);
	BOOL start();
	BOOL pause();
	BOOL stop();
	BOOL close();
	void writeOut(float **outData, int size);

//currently opened wave device attributes
    BOOL isDevOpen() { return isOpen; }

	//audio data params
    int getSampleRate()      { return (isOpen) ? (int)wf.nSamplesPerSec : 0; }
    int getSampleSize()		 { return (isOpen) ? (int)wf.wBitsPerSample : 0; }
    int getChannelCount()    { return (isOpen) ? (int)wf.nChannels : 0; }
    int getBytesPerSecond()  { return (isOpen) ? (int)wf.nAvgBytesPerSec : 0; }
    int getBlockAlignment()  { return (isOpen) ? (int)wf.nBlockAlign : 0; }

//buffer mgmt

	//maximum amount of data we can send to <writeOut> at any one time is bufferCount * bufferDuration
	//if we send more than this, we'll get a overrun condition, and need to increase num of buffers
	int getBufferCount() { return bufferCount; }
	void setBufferCount(int count) { bufferCount = count;}

	int getBufferDuration() {return bufferDuration; }		//buffer length in msec
	void setBufferDuration(int duration);

protected:
	Waverly *waverly;		//for reporting status to controller
	char* devName;
	BOOL isOpen;			//device status (opened/closed)
    HWAVEOUT hDev;			//device handle for making func calls
	WAVEFORMATEX wf;		//format of audio data we'll be sending to device
	BOOL isPlaying;			//are we currently sending the device audio data?

	//output buffers
	WaveBuffer **buffers;		//the buffer array we pass data to the device with
	int bufferCount;
	int bufferDuration;			//buf size in msec - we set buf size with this
	int bufferSize;				//buf size in bytes - depends on sample rate, sample size & channel count

	void allocateBuffers();
	void freeBuffers();

//the waveoutproc callback
    static void CALLBACK WaveOutProc(HMIDIOUT hWaveOut, UINT wMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2);
};

#endif // WAVEOUTDEVICE_H
