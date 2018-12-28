#include "WaveBuffer.h"
#include "WaveOutDevice.h"
#include <math.h>

//default vals
#define WAVEOUTBUFCOUNT  10              
#define WAVEOUTBUFSIZE   4410L           

CRITICAL_SECTION WaveOutDevice::waveCriticalSection;		//static

//cons
WaveOutDevice::WaveOutDevice()
{
	hDev = NULL;		//device closed
	bIsOpen = FALSE;

	//default wave out format - 16 bits in stereo @ 44.1kHZ
	wf.wFormatTag = WAVE_FORMAT_PCM;
	wf.nSamplesPerSec = 44100;
	wf.wBitsPerSample = 16;
	wf.nChannels = 2;
	wf.nBlockAlign = (wf.nChannels * wf.wBitsPerSample) / 8;		//num bytes in a block
	wf.nAvgBytesPerSec = wf.nSamplesPerSec * wf.nBlockAlign;		//blocks / sec in bytes
	wf.cbSize = 0;													//no extra data

	//no buffers allocated yet
	bufferSize = WAVEOUTBUFSIZE;
	bufferCount = WAVEOUTBUFCOUNT;
	allocBufferCount = 0;
	usedBufferCount = 0;
	buffers = NULL;

	InitializeCriticalSection(&waveCriticalSection);
}

//destruct
WaveOutDevice::~WaveOutDevice()
{
	if (hDev != NULL) close();
	DeleteCriticalSection(&waveCriticalSection);
}

//- buffer management ---------------------------------------------------------

//alocate a previously set number of data buffers for output
void WaveOutDevice::allocateBuffers()
{
	freeBuffers();
	if (bufferCount == 0)
		return;

	bufferSize = ((wf.nSamplesPerSec + bufferCount - 1) / bufferCount) * wf.nBlockAlign;		//buf size in bytes
	buffers = new WaveBuffer* [bufferCount];
	for (int i = 0; i < bufferCount; i++)
		buffers[i] = new WaveBuffer(bufferSize);
	allocBufferCount = bufferCount;
	usedBufferCount = 0;
}

void WaveOutDevice::freeBuffers()
{
	if (buffers)
	{
		for (int i = 0; i < bufferCount; i++)
			if (buffers[i])
				delete buffers[i];
		delete[] buffers;
	}
	buffers = NULL;
	allocBufferCount = 0;
	usedBufferCount = 0;
}

//- device management --------------------------------------------------------------

//open
BOOL WaveOutDevice::open(int devID, DWORD sampleRate, WORD bitDepth, WORD channels)
{
	if (hDev != NULL) 
		close();

	//update wave out format
	wf.nChannels = channels;
	wf.nSamplesPerSec = sampleRate;
	wf.nAvgBytesPerSec = sampleRate * channels * bitDepth / 8;
	wf.nBlockAlign = channels * bitDepth / 8;
	wf.wBitsPerSample = bitDepth;	

	//open it
	WORD result = waveOutOpen(&hDev, devID, &wf, (DWORD)WaveOutProc, (DWORD)this, CALLBACK_FUNCTION);

	//check for error
	if (result)
	{
		WCHAR errMsg[256];
		waveOutGetErrorText(result, errMsg, sizeof(errMsg));
		//printf("ERROR: opening wave output device %d: %s\n", devID, errMsg);
		hDev = NULL;                         
		return FALSE;
	}

	//if opened, pause output & allocate bufs
	bIsOpen = TRUE;
	//waveOutPause(hDev);
	allocateBuffers();	

	waveOutGetDevCaps(devID, &wc, sizeof(wc));

	return TRUE;
}

//close
BOOL WaveOutDevice::close()
{
	if (!bIsOpen) 
		return FALSE;            

	waveOutReset(hDev);			//stop playback
	waveOutClose(hDev);			//and close device
	hDev = NULL;
	bIsOpen = FALSE;

	freeBuffers();
	return TRUE;
}

//- device control ------------------------------------------------------------

BOOL WaveOutDevice::start ( )
{
	if (!bIsOpen) 
		return FALSE; 
	waveOutRestart(hDev);
	return TRUE;         
}

BOOL WaveOutDevice::pause ( )
{
	if (!bIsOpen)
		return FALSE;
	waveOutPause(hDev); 
	return TRUE;        
}

BOOL WaveOutDevice::stop ( )
{
	if (!bIsOpen)    
		return FALSE;  
	waveOutReset(hDev); 
	return TRUE;        
}

//- output audio --------------------------------------------------------------

void WaveOutDevice::writeOut(float **outData, int length)
{
	if (!bIsOpen)
		return;	

	int nBufLen = bufferSize / wf.nBlockAlign;		//buf size in samples
	int outPos = 0;

	//keep filling output buffers until at end of data
	while (length > 0)
	{
		int blockCount = (length <= nBufLen) ? length : nBufLen;

		//get free output buffer
		int i;
		for (i = 0; i < bufferCount; i++)
			if (!buffers[i]->isInUse())
				break;
		if (i >= bufferCount)                   
		{
			printf("writeData(%d): no buffers available\n", length);
			return;                             
		}
		WaveBuffer* buf = buffers[i];
		buf->setInUse(TRUE);
		EnterCriticalSection(&waveCriticalSection);
		usedBufferCount++;                          
		LeaveCriticalSection(&waveCriticalSection);


		//store multi channel floating point data into interleaved integer data
		int bytesPerSample = wf.wBitsPerSample / 8;
		for (int sampNum = 0; sampNum < blockCount; sampNum++)            
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

		DWORD bcount = blockCount * wf.nBlockAlign;
		buf->lpHdr->dwBufferLength = bcount;
		buf->lpHdr->dwBytesRecorded = bcount;

		//write buffer to output device
		MMRESULT result = !waveOutPrepareHeader(hDev, buf->lpHdr, sizeof(WAVEHDR));
		if (result)
		{
			waveOutWrite(hDev, buf->lpHdr, sizeof(WAVEHDR));
		}

		//goto next buffer's worth of output data
		outPos += blockCount;
		length -= blockCount;
	}
}

//-----------------------------------------------------------------------------

// callback procedure - called when device driver has played a buffer
void CALLBACK WaveOutDevice::WaveOutProc
	(
	HMIDIOUT hWaveOut,
	UINT wMsg,
	DWORD dwInstance,			//for WOM_DONE, this will be the WaveOutDevice instance
	DWORD dwParam1,				//and this will be the wave hdr struct
	DWORD dwParam2
	)
{
	switch (wMsg)
	{
	case WOM_OPEN :		//don't cares
	case WOM_CLOSE : 	    
		break;

	case WOM_DONE : 
		WaveOutDevice * pOutDev = (WaveOutDevice *)dwInstance;
		pOutDev->bufferFinished((LPWAVEHDR)dwParam1);
		break;

	}
}

void WaveOutDevice::bufferFinished(LPWAVEHDR lphdr)
{
	waveOutUnprepareHeader(hDev, lphdr, sizeof(WAVEHDR));
	lphdr->dwFlags &= ~WHDR_DONE;       
	WaveBuffer* wb = (WaveBuffer *) lphdr->dwUser;
	wb->setInUse(FALSE);
	EnterCriticalSection(&waveCriticalSection);
	if (usedBufferCount > 0)
		usedBufferCount--;                     
	LeaveCriticalSection(&waveCriticalSection);
}

//- WaveOut func wrappers -----------------------------------------------------

BOOL WaveOutDevice::getPosition(LPMMTIME pMMT,	UINT MMTSize)
{
	if (!bIsOpen)                        
		return FALSE;                        
	return !waveOutGetPosition(hDev, pMMT, MMTSize);
}

BOOL WaveOutDevice::getPitch(LPDWORD pdwPitch)
{
	if ((!bIsOpen) || (!(wc.dwSupport & WAVECAPS_PITCH))) 
		return FALSE;                       
	return !waveOutGetPitch(hDev, pdwPitch);
}

BOOL WaveOutDevice::setPitch(DWORD dwPitch)
{
	if ((!bIsOpen) || (!(wc.dwSupport & WAVECAPS_PITCH))) 
		return FALSE;                       
	return !waveOutSetPitch(hDev, dwPitch);
}

BOOL WaveOutDevice::getPlaybackRate(LPDWORD pdwPlaybackRate)
{
	if ((!bIsOpen) || (!(wc.dwSupport & WAVECAPS_PLAYBACKRATE)))
		return FALSE;                     
	return !waveOutGetPlaybackRate(hDev, pdwPlaybackRate);
}

BOOL WaveOutDevice::setPlaybackRate(DWORD dwPlaybackRate)
{
	if ((!bIsOpen) || (!(wc.dwSupport & WAVECAPS_PLAYBACKRATE)))
		return FALSE;     
	return !waveOutSetPlaybackRate(hDev, dwPlaybackRate);
}

BOOL WaveOutDevice::getVolume(LPDWORD pdwVolume)
{
	if ((!bIsOpen) || (!(wc.dwSupport & WAVECAPS_VOLUME)))
		return FALSE;           
	return !waveOutGetVolume(hDev, pdwVolume);
}

BOOL WaveOutDevice::setVolume(DWORD dwVolume)
{
	if ((!bIsOpen) || (!(wc.dwSupport & WAVECAPS_VOLUME)))
		return FALSE;    
	return !waveOutSetVolume(hDev, dwVolume);
}

BOOL WaveOutDevice::breakLoop()
{
	if (!bIsOpen)     
		return FALSE;     
	waveOutBreakLoop(hDev);
	return TRUE;           
}

DWORD WaveOutDevice::sendMessage(UINT msg, DWORD param1, DWORD param2)
{
	DWORD result = 0;

	if (hDev)
		result = waveOutMessage(hDev, msg, param1, param2);
	return result;
}
