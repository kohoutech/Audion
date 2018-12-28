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
    BOOL isOpen() { return bIsOpen; }

    int getSampleRate()      { return (bIsOpen) ? (int)wf.nSamplesPerSec : 0; }
    int getBitDepth()		 { return (bIsOpen) ? (int)wf.wBitsPerSample : 0; }
    int getChannelCount()    { return (bIsOpen) ? (int)wf.nChannels : 0; }
    int getBytesPerSecond()  { return (bIsOpen) ? (int)wf.nAvgBytesPerSec : 0; }
    int getBlockAlignment()  { return (bIsOpen) ? (int)wf.nBlockAlign : 0; }

//buffer mgmt
	int getBufferCount() { return bufferCount; }
	void setBufferCount(int count) { bufferCount = count;}
	int getBufferSize() {return bufferSize; }
    int getAllocatedBuffers() { return allocBufferCount; }
    int getPendingBuffers() { return usedBufferCount; }

//wrap misc WaveOut functions
    BOOL getPosition(LPMMTIME pMMT, UINT MMTSize = sizeof(MMTIME));
    BOOL getPitch(LPDWORD pPitch);
    BOOL setPitch(DWORD pitch);
    BOOL getPlaybackRate(LPDWORD pPlaybackRate);
    BOOL setPlaybackRate(DWORD playbackRate);
    BOOL getVolume(LPDWORD pVolume);
    BOOL setVolume(DWORD volume);
    BOOL breakLoop();
    DWORD sendMessage(UINT msg, DWORD dw1, DWORD dw2);

protected:
	BOOL bIsOpen;
    HWAVEOUT hDev;                      
	WAVEFORMATEX wf;
    WAVEOUTCAPS wc;			//for WaveOut funcs

	WaveBuffer **buffers;
	int bufferSize;
	int bufferCount;
	int allocBufferCount;
	int usedBufferCount;

	static CRITICAL_SECTION waveCriticalSection;

	void allocateBuffers();
	void freeBuffers();

//the waveoutproc callback
    static void CALLBACK WaveOutProc(HMIDIOUT hWaveOut, UINT wMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2);
	void bufferFinished(LPWAVEHDR lphdr);
};

#endif // WAVEOUTDEVICE_H
