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

#include "WaveOutDevice.h"
#include "..\Waverly.h"
#include "WaveBuffer.h"
#include <math.h>

//default vals - gives 1 sec w/o buffer overrun, should be enough
#define WAVEOUTBUFCOUNT  10
#define WAVEOUTBUFDURATION   100		//buf duration in ms

//cons
WaveOutDevice::WaveOutDevice(Waverly *_waverly)
{
	waverly = _waverly;
	devName = NULL;
	hDev = NULL;		//device closed
	isOpen = FALSE;

	//default wave out format - 16 bits in stereo @ 44.1kHZ
	wf.wFormatTag = WAVE_FORMAT_PCM;
	wf.nSamplesPerSec = 44100;
	wf.wBitsPerSample = 16;
	wf.nChannels = 2;
	wf.nBlockAlign = (wf.nChannels * wf.wBitsPerSample) / 8;		//num bytes in a block
	wf.nAvgBytesPerSec = wf.nSamplesPerSec * wf.nBlockAlign;		//blocks / sec in bytes
	wf.cbSize = 0;													//no extra data

	//no buffers allocated yet, we set the buf count & buf len
	bufferDuration = WAVEOUTBUFDURATION;
	bufferCount = WAVEOUTBUFCOUNT;
	bufferSize = (int)(wf.nSamplesPerSec * (bufferDuration / 1000.0f) + 1.0f) * wf.nBlockAlign;
	buffers = NULL;

	isPlaying = FALSE;
}

//destruct
WaveOutDevice::~WaveOutDevice()
{
	if (hDev != NULL) 
		close();	
}

//- buffer management ---------------------------------------------------------

//alocate a previously set number of data buffers for output
void WaveOutDevice::allocateBuffers()
{
	freeBuffers();
	if (bufferCount == 0)
		return;

	bufferSize = (int)(wf.nSamplesPerSec * (bufferDuration / 1000.0f) + 1.0f) * wf.nBlockAlign;		//buf size in bytes
	buffers = new WaveBuffer* [bufferCount];
	for (int i = 0; i < bufferCount; i++) {
		WaveBuffer* buf = new WaveBuffer(bufferSize);
		buffers[i] = buf;
		MMRESULT result = !waveOutPrepareHeader(hDev, buf->waveHdr, sizeof(WAVEHDR));
	}
}

void WaveOutDevice::freeBuffers()
{
	if (buffers)
	{
		for (int i = 0; i < bufferCount; i++)
			if (buffers[i]) {
				WaveBuffer* buf = buffers[i];
				waveOutUnprepareHeader(hDev, buf->waveHdr, sizeof(WAVEHDR));
				delete buf;
			}
		delete[] buffers;
	}
	buffers = NULL;
}

void WaveOutDevice::setBufferDuration(int duration)
{
	if (isPlaying)
		return;

	bufferDuration = duration;
	allocateBuffers();	
}

//- device management --------------------------------------------------------------

//open device with optional sample rate, sample size & channel count
//if opened, sets callback & allocates the buffers
BOOL WaveOutDevice::open(int devID, DWORD sampleRate, WORD sampleSize, WORD channels)
{
	if (hDev != NULL)			//if already open
		close();

	//update wave out format
	wf.nChannels = channels;
	wf.nSamplesPerSec = sampleRate;
	wf.wBitsPerSample = sampleSize;	
	wf.nBlockAlign = channels * sampleSize / 8;
	wf.nAvgBytesPerSec = sampleRate * channels * sampleSize / 8;

	//open it
	//devID = device id, wf = format of data we're passing to it, 
	//waveoutproc = callback when it needs more data, using CALLBACK_FUNCTION (instead of window, thread or event)
	//this = ref to self returned in callback, so the callback can call funcs on self
	//we get back a handle to open device if no err
	WORD result = waveOutOpen(&hDev, devID, &wf, (DWORD)WaveOutProc, (DWORD)this, CALLBACK_FUNCTION);

	//check for error
	if (result)
	{
		waverly->reportStatus(devName, "could not open device for output");
		hDev = NULL;                         
		return FALSE;
	}

	//if opened, pause output & allocate bufs
	isOpen = TRUE;
	waveOutPause(hDev);
	allocateBuffers();	

	return TRUE;
}

//close
BOOL WaveOutDevice::close()
{
	if (!isOpen) 
		return FALSE;            

	waveOutReset(hDev);			//stop playback
	waveOutClose(hDev);			//and close device
	hDev = NULL;
	isOpen = FALSE;

	freeBuffers();				//delete buffers
	return TRUE;
}

//- device control ------------------------------------------------------------

BOOL WaveOutDevice::start ( )
{
	if (!isOpen) 
		return FALSE; 
	waveOutRestart(hDev);
	isPlaying = TRUE;

	return TRUE;         
}

BOOL WaveOutDevice::pause ( )
{
	if (!isOpen)
		return FALSE;
	waveOutPause(hDev); 
	return TRUE;        
}

BOOL WaveOutDevice::stop ( )
{
	if (!isOpen)    
		return FALSE;  
	waveOutReset(hDev);				//stops playback & marks pending playback bufs as done
	isPlaying = FALSE;

	return TRUE;        
}

//- output audio --------------------------------------------------------------

//send audio data to output device
//converts array of floats (array size = channel count) to interleaved ints
//so each sample "block" size = sample size (in bytes) * num of channels
//keeps filling up output bufs & handing them off to device queue until 
//end of data or we run out of free buffers (overrun condition)
void WaveOutDevice::writeOut(float **outData, int length)
{
	if (!isOpen)
		return;	

	int nBufLen = bufferSize / wf.nBlockAlign;		//buf size in samples
	int outPos = 0;

	//keep filling output buffers until at end of data
	while (length > 0)
	{
		int sampleCount = (length <= nBufLen) ? length : nBufLen;	//num of samples to fill for this buffer

		//get free output buffer & mark it being used 
		int i;
		for (i = 0; i < bufferCount; i++)
			if (!buffers[i]->isInUse())
				break;
		if (i >= bufferCount)		//if no free buffers, we have overrun condition
		{
			waverly->reportStatus(devName, "buffer overrun during playback");
			return;                             
		}
		WaveBuffer* buf = buffers[i];
		buf->setInUse(TRUE);

		//store multi channel floating point data into interleaved integer data
		int bytesPerSample = wf.wBitsPerSample / 8;
		for (int sampNum = 0; sampNum < sampleCount; sampNum++)            
		{
			for (int chanNum = 0; chanNum < wf.nChannels; chanNum++)         
			{
				//get output sample val (as a float)
				float sampleFloat = outData[chanNum][sampNum + outPos];
				LPSTR sampPos = buf->dataBuf + (sampNum * wf.nBlockAlign) + (chanNum * bytesPerSample);				

				//store one and two byte samples in outbuf buf - other sizes aren't handled
				switch (bytesPerSample)
				{
				case 1 :
					*((BYTE *)sampPos) = (BYTE)((sampleFloat * 127.5f) + 127.5f);
					break;
				case 2 :                             
					*((short *)sampPos) = (short) ((sampleFloat * 32767.5f) - 0.5f);
					break;
				default :
					break;		//do nothing		
				}
			}
		}

		DWORD bcount = sampleCount * wf.nBlockAlign;		//number of data bytes in output buffer
		buf->waveHdr->dwBufferLength = bcount;				//set size of data in wavehdr to be played
		buf->waveHdr->dwBytesRecorded = bcount;

		//write buffer to output device
		waveOutWrite(hDev, buf->waveHdr, sizeof(WAVEHDR));

		//goto next buffer's worth of output data
		outPos += sampleCount;
		length -= sampleCount;
	}
}

//-----------------------------------------------------------------------------

// callback procedure - called when device driver has played a buffer, we return it to the free buffer list
void CALLBACK WaveOutDevice::WaveOutProc(HMIDIOUT hWaveOut,	UINT wMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2)
{
	switch (wMsg)
	{
	case WOM_OPEN :		//don't cares
	case WOM_CLOSE : 	    
		break;

	case WOM_DONE : 
		LPWAVEHDR lphdr = (LPWAVEHDR)dwParam1;
		WaveBuffer* wb = (WaveBuffer *) lphdr->dwUser;		//we get the WaveBuffer obj from wavehdr's dwUser field
		wb->setInUse(FALSE);								//mark WaveBuffer as free to reuse
		break;
	}
}

//printf("there's no sun in the shadow of the wizard.\n");