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

#if !defined(WAVEINDEVICE_H)
#define WAVEINDEVICE_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

#include "WaveBuffer.h"

class Waverly;
class WaveBuffer;
class Transport;
class Track;

class WaveInDevice
{
public:
	WaveInDevice(Waverly *_waverly);
	~WaveInDevice();

//basic operations
	BOOL open(int devID, DWORD sampleRate, WORD bitDepth, WORD channels);
	BOOL start();
	BOOL stop();
	BOOL reset();
	BOOL close();

//currently opened wave device attributes
public:
	BOOL isDevOpen()	{ return isOpen; }
	BOOL IsRecording()	{ return isRecording; }

	int getSampleRate()			{ return (isOpen) ? (int)wf.nSamplesPerSec : 0; }
	int getSampleSize()			{ return (isOpen) ? (int)wf.wBitsPerSample : 0; }
	int getChannelCount()		{ return (isOpen) ? (int)wf.nChannels : 0; }
	int getBytesPerSecond()     { return (isOpen) ? (int)wf.nAvgBytesPerSec : 0; }
	int getBlockAlignment()		{ return (isOpen) ? (int)wf.nBlockAlign : 0; }

	void setStartTime(DWORD time) { startTime = time; }
	int getSampleCount(WaveBuffer &buf) { return getBlockAlignment() ? buf.Length() / getBlockAlignment() : 0; }	

//buffer mgmt
	int getBufferCount() { return bufferCount; }
	void setBufferCount(int count) { bufferCount = count;}

	int getBufferDuration() {return bufferDuration; }
	void setBufferDuration(int duration);

	Transport* transport;
	Track* recTrack;

protected:
	Waverly *waverly;		//for reporting status to controller
	BOOL isOpen;                       
	char* devName;
	HWAVEIN hDev;                       
	WAVEFORMATEX wf;                    
	BOOL isRecording;
	DWORD startTime;

	//input buffers
	WaveBuffer **buffers;		//the buffer array we get data from the device with
	int bufferCount;
	int bufferDuration;			//buf size in msec - we set buf size with this
	int bufferSize;				//buf size in bytes - depends on sample rate, sample size & channel count

	void allocateBuffers();
	void freeBuffers();
	BOOL addBuffer();    		//put an empty data buffer in device's queue to be filled up with audio data

	float * inData[2];			//stereo buffer to hold converted data that we send to transport

	void readIn(WaveBuffer* buf);

//the waveinproc callback
	static void CALLBACK WaveInProc(HWAVEIN hMidiIn, UINT wMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2);
};

#endif // !defined(WAVEINDEVICE_H)
