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

#include "WaveInDevice.h"
#include "..\Waverly.h"
#include "..\Engine\Transport.h"
#include "WaveBuffer.h"
#include <math.h>

//default vals
#define WAVEINBUFCOUNT  10
#define WAVEINBUFDURATION   100		//buf duration in ms

//cons
WaveInDevice::WaveInDevice (Waverly *_waverly)
{
	waverly = _waverly;
	hDev = NULL;
	devName = NULL;
	isOpen = FALSE;
	
	//default wave in format - 16 bits in stereo @ 44.1kHZ
	wf.wFormatTag = WAVE_FORMAT_PCM;
	wf.nSamplesPerSec = 44100;
	wf.wBitsPerSample = 16;
	wf.nChannels = 1;
	wf.nBlockAlign = (wf.nChannels * wf.wBitsPerSample) / 8;		//num bytes in a block
	wf.nAvgBytesPerSec = wf.nSamplesPerSec * wf.nBlockAlign;		//blocks / sec in bytes
	wf.cbSize = 0;													//no extra data
	
	//no wave buffers allocated yet
	bufferDuration = WAVEINBUFDURATION;
	bufferSize = (int)(wf.nSamplesPerSec * (bufferDuration / 1000.0f) + 1.0f) * wf.nBlockAlign;
	bufferCount = WAVEINBUFCOUNT;
	buffers = NULL;

	//alloc input data bufs
	inData[0] = new float[bufferSize];
	inData[1] = new float[bufferSize];

	isRecording = FALSE;
	startTime = 0;
	recTrack = NULL;
}

//destruct
WaveInDevice::~WaveInDevice ( )
{
	if (isOpen)
	  close();

	if (inData[0])
	  delete[] inData[0];
	if (inData[1])
	  delete[] inData[1];	
}

//- buffer management ---------------------------------------------------------

void WaveInDevice::allocateBuffers ()
{	
	freeBuffers();

	if (bufferCount == 0)
		return;

	bufferSize = (int)(wf.nSamplesPerSec * (bufferDuration / 1000.0f) + 1.0f) * wf.nBlockAlign;		//buf size in bytes
	buffers = new WaveBuffer* [bufferCount];
	for (int i = 0; i < bufferCount; i++) {
		WaveBuffer* buf = new WaveBuffer(bufferSize);
		buf->setDevRecording(TRUE);
		buffers[i] = buf;
		MMRESULT result = waveInPrepareHeader(hDev, buf->waveHdr, sizeof(WAVEHDR));
	}
}

void WaveInDevice::freeBuffers ( )
{
	if (buffers)
	{
		for (int i = 0; i < bufferCount; i++)
			if (buffers[i]) {
				WaveBuffer* buf = buffers[i];
				waveInUnprepareHeader(hDev, buf->waveHdr, sizeof(WAVEHDR));
				delete buf;
			}
			delete[] buffers;
	}
	buffers = NULL;
}

void WaveInDevice::setBufferDuration(int duration)
{
	if (isRecording)
		return;

	bufferDuration = duration;
	allocateBuffers();	
}

//- device management --------------------------------------------------------------

//open device with optional sample rate, sample size & channel count
//if opened, sets callback & allocates the buffers
BOOL WaveInDevice::open(int devID, DWORD sampleRate, WORD sampleSize, WORD channels)
{
	if (hDev != NULL) 
	  close();        

	//update wave in format
	wf.nChannels = channels;
	wf.nSamplesPerSec = sampleRate;
	wf.nAvgBytesPerSec = sampleRate * channels * sampleSize / 8;
	wf.nBlockAlign = channels * sampleSize / 8;
	wf.wBitsPerSample = sampleSize;	
	
	//open it
	//devID = device id, wf = format of data we're receiving from it, 
	//waveinproc = callback when it has more data, using CALLBACK_FUNCTION (instead of window, thread or event)
	//this = ref to self returned in callback, so the callback can call funcs on self
	//we get back a handle to open device if no err
	WORD result = waveInOpen(&hDev, devID, &wf, (DWORD)WaveInProc, (DWORD)this, CALLBACK_FUNCTION);

	//check for error
	if (result)	
	{
		waverly->reportStatus(devName, "could not open device for output");
	    hDev = NULL; 
	    return FALSE;
	}
	
	//if opened, pause output & allocate bufs
	isOpen = TRUE;
	allocateBuffers();

	return TRUE;
}

BOOL WaveInDevice::close ( )
{
	if (!isOpen)
	  return FALSE;

	stop();
	//reset();
	
	waveInClose(hDev);
	hDev = NULL;
	isOpen = FALSE;

	freeBuffers();
	return TRUE;
}

//- device control ------------------------------------------------------------

BOOL WaveInDevice::start ( )
{
	if (!isOpen || isRecording || (recTrack == NULL))
	  return FALSE;                        
	
	addBuffer();				//prime the device's input queue, 3 buffers should be all we need?
	addBuffer();
	addBuffer();
	waveInStart(hDev);					//start recording data
	startTime = timeGetTime();			//and get the start time for time stamping
	isRecording = TRUE;                    
	return TRUE;
}

BOOL WaveInDevice::reset ( )
{
	if (!isOpen)
	  return FALSE;

	stop();
	//waveInReset(hDev);

	return TRUE;
}

BOOL WaveInDevice::stop ( )
{
	if (!isOpen || !IsRecording())
	  return FALSE;
	
	isRecording = FALSE;
	waveInStop(hDev);

	return TRUE;
}

//- input audio --------------------------------------------------------------

BOOL WaveInDevice::addBuffer()
{
	//get free input buffer & mark it being used 
	int i;
	for (i = 0; i < bufferCount; i++)
		if (!buffers[i]->isInUse())
			break;
	if (i >= bufferCount)                   
	{
		waverly->reportStatus(devName, "buffer underrun during recording");
		return FALSE;                             
	}
	WaveBuffer* buf = buffers[i];
	buf->setInUse(TRUE);
	
	buf->waveHdr->dwBytesRecorded = 0;
	
	MMRESULT result = waveInAddBuffer(hDev, buf->waveHdr, sizeof(WAVEHDR));
	return (result == MMSYSERR_NOERROR);
}

void WaveInDevice::readIn(WaveBuffer* buf)
{
	if (!isOpen)
		return;	

	//convert interleaved integer sample data into multi channel floating point data
	short sample;
	int	sampleCount = buf->waveHdr->dwBytesRecorded / wf.nBlockAlign;
	int bytesPerSample = wf.wBitsPerSample / 8;

	for (int nSample = 0; nSample < sampleCount; nSample++)
	{
		for (int nChannel = 0; nChannel < wf.nChannels; nChannel++)
		{
			LPSTR sampPos = buf->dataBuf + (nSample * wf.nBlockAlign) + (nChannel * bytesPerSample);

			switch (bytesPerSample)
			{
			case 1 :                  
				sample = ((short)(*(BYTE *)sampPos) * 256) - (short)32768;
				break;
			case 2 :                 
				sample = *((short *)sampPos);
				break;
			default :                 
				sample = 0;
				break;
			}

			inData[nChannel][nSample] = ((sample + 0.5f) / 32767.5f);
		}
	}

	//transport->audioIn(inData, sampleCount, wf.nChannels,  buf->getTimestamp(), recTrack);
}


//-----------------------------------------------------------------------------
 
// callback procedure - called when device driver has filled up a buffer
void CALLBACK WaveInDevice::WaveInProc(HWAVEIN hMidiIn, UINT wMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2)
{
	switch (wMsg)
	{
	case WIM_CLOSE : 		//don't cares
	case WIM_OPEN : 
		break;

	case WIM_DATA : 

		WaveInDevice * pInDev = (WaveInDevice *)dwInstance;
		pInDev->addBuffer();									//immediately pass another empty buffer to take this one's place in queue

		LPWAVEHDR lphdr = (LPWAVEHDR)dwParam1;
		WaveBuffer* wb = (WaveBuffer *) lphdr->dwUser;

		if (pInDev->isRecording && (lphdr->dwBytesRecorded > 0))
		{
			//set buffer's timepstamp to the time of the first sample in buf
			DWORD now = timeGetTime() - pInDev->startTime;
			DWORD recTime = (lphdr->dwBytesRecorded  * 1000)/ pInDev->wf.nAvgBytesPerSec;
			DWORD timestamp = now - recTime;
			wb->setTimestamp(timestamp);

			pInDev->readIn(wb);		//handle this buffer's recorded sample data
		}

		wb->setInUse(FALSE);			//return this buffer to empty buf pool
		break;
	}
}

//printf("there's no sun in the shadow of the wizard.\n");