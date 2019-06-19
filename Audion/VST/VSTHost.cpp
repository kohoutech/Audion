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

#include "VSTHost.h"
#include "VSTPlugin.h"
#include "WaveOutDevice.h"

#define DEFAULTSAMPLERATE 44100
#define DEFAULTBLOCKSIZE  2205			//block duration = 50 ms

//cons
VSTHost::VSTHost()
{
	//default vals
	sampleRate = DEFAULTSAMPLERATE;
	blockSize = DEFAULTBLOCKSIZE;
	
	waveOut = new WaveOutDevice();
	waveOutDevId = WAVE_MAPPER;				//use default in & out for now
	resetWaveOutDevice();					// open output device

	initTimer();
	isRunning = false;

	for (int i = 0; i < 2; i++)            
	{
		emptyBuf[i] = new float[DEFAULTSAMPLERATE];
		if (emptyBuf[i])
			for (int j = 0; j < DEFAULTSAMPLERATE; j++)
				emptyBuf[i][j] = 0.0f;
	}


	for (int i = 0; i < 2; i++)
		pOutputs[i] = new float[22050];			//0.5 sec @ 44.1 kHz

	pluginMax = 10;
	plugins = (VSTPlugin**)malloc(pluginMax * sizeof(VSTPlugin*));
	pluginCount = 0;

	//InitializeCriticalSection(&cs);
}

//destuct
VSTHost::~VSTHost()
{
	stopEngine();
	unloadAll();
	waveOut->close();

	//free allocated mem
	delete plugins;
	delete waveOut;

	for (int i = 0; i < 2; i++)
		if (emptyBuf[i])
			delete[] emptyBuf[i];

	for (int i = 0; i < 2; i++)
		delete[] pOutputs[i];

	//DeleteCriticalSection(&cs);
}

//- plugin methods ------------------------------------------------------------

void VSTHost::setSampleRate(int rate) 
{
	BOOL wasEngineRunning = isRunning;
	if (isRunning) stopEngine();                           

	sampleRate = rate; 

	for (int i = 0; i < pluginCount; i++) {
		VSTPlugin *plug = getPlugin(i);
		if (plug != NULL)
			plug->setSampleRate(sampleRate);
	}
	resetWaveOutDevice();					// open output device with new sample rate

	if (wasEngineRunning) startEngine();
}

void VSTHost::setBlockSize (int size) 
{ 
	BOOL wasEngineRunning = isRunning;
	if (isRunning) stopEngine();                           

	blockSize = size; 
	for (int i = 0; i < pluginCount; i++) {
		VSTPlugin *plug = getPlugin(i);
		if (plug != NULL)
			plug->setBlockSize(blockSize);
	}
	resetWaveOutDevice();					// open output device with new block size
	
	if (wasEngineRunning) startEngine();
}

//- plugin methods ------------------------------------------------------------

int VSTHost::loadPlugin(const char * filename) 
{
	BOOL wasEngineRunning = isRunning;
	if (isRunning) stopEngine();                           

	//load plugin
	VSTPlugin *plug = new VSTPlugin(this);      
	if (!plug->load(filename))      
	{
		delete plug;        
		return -1;
	}

	//store plugin obj in host array
	if (pluginCount >= pluginMax) 
	{
		pluginMax += 10;
		plugins = (VSTPlugin**)realloc(plugins,pluginMax * sizeof(VSTPlugin*));
	}
	int plugNum = pluginCount++;
	plugins[plugNum] = plug;

	//plugin setup
	plug->open();
	plug->setSampleRate(sampleRate);
	plug->setBlockSize(blockSize);
	plug->suspend();                  
	plug->resume();                   

	if (wasEngineRunning) startEngine();
	return plugNum;
}

VSTPlugin* VSTHost::getPlugin(int idx) 
{ 
	if ((idx >= 0) && (idx < pluginCount)) 
		return plugins[idx]; 
	else 
		return NULL; 
}

void VSTHost::unloadPlugin(int idx)
{
	if ((idx >= 0) && (idx < pluginCount)) 
	{
		BOOL wasEngineRunning = isRunning;         
		if (isRunning) stopEngine();                           

		VSTPlugin *plug = plugins[idx]; 
		if (plug != NULL) {
			plug->unload();
			delete plug;
		}
		plugins[idx] = NULL;

		if (wasEngineRunning) startEngine();
	}
}

void VSTHost::unloadAll()
{
	for (int i = 0; i < pluginCount; i++)
		unloadPlugin(i);
}

//- device methods ------------------------------------------------------------

BOOL VSTHost::resetWaveOutDevice()
{
	BOOL result = FALSE;

	if (waveOut->isDevOpen()) waveOut->close();

	waveOut->setBufferCount(sampleRate/blockSize);					//num of buf / sec
	waveOut->setBufferDuration((1000 * blockSize) / sampleRate);	//buf len in ms
	result = waveOut->open(waveOutDevId, sampleRate, 16, 2);		//16 bit stereo out

	return result;
}

//- host engine methods -------------------------------------------------------

void VSTHost::startEngine() 
{
	if (isRunning)
		return;

	timerDuration = (blockSize * 1000) / sampleRate;		//timer len in msec
	if (timerDuration < tc.wPeriodMin)          
		timerDuration = tc.wPeriodMin;

	//start output device and timer to send audio data to it
	waveOut->start();		
	startTimer();

	isRunning = TRUE;
}

void VSTHost::stopEngine() 
{
	if (!isRunning)
		return;

	//stop output device
	stopTimer(); 
	waveOut->stop();

	isRunning = FALSE;
}

//- timer methods -------------------------------------------------------------

void VSTHost::initTimer()
{
	timeGetDevCaps(&tc, sizeof(tc));		//get timer capabilities
	timerID = 0;                              
	dwRest = 0;
	dwLastTime = 0;
}

BOOL VSTHost::startTimer()
{
	int resolution = timerDuration / 10;
	timeBeginPeriod(resolution);        

	//timer will call <timerCallback> func every <timerDuration> millisecs
	timerID = timeSetEvent(timerDuration, (resolution > 1) ? resolution / 2 : 1, timerCallback, (DWORD)this, 
		TIME_PERIODIC || TIME_KILL_SYNCHRONOUS);

	return (timerID != NULL);
}

void VSTHost::stopTimer() 
{
	if (timerID != NULL)
	{
		timeKillEvent(timerID);
		timerID = 0;
		timeEndPeriod(tc.wPeriodMin);
	}
}

void CALLBACK VSTHost::timerCallback(UINT uID,	UINT uMsg,	DWORD dwUser,	DWORD dw1,	DWORD dw2)
{
	if (dwUser)                             
		((VSTHost *)dwUser)->handleTimer();		//use dwUser field to xlate Windows call back to class method
}

//timer callback runs in its own thread
void VSTHost::handleTimer()
{
	static DWORD now;                
	static DWORD dwOffset;                 
	static WORD  i;                        

	now = timeGetTime();             
	dwOffset = now - dwLastTime;			//time since last timer call
	if (dwOffset > now)              
	{
		dwLastTime = 0;                    
		dwOffset = now;              
	}                                   

	dwOffset += dwRest;						//ofs from last timer duration - compensate for timer drift
	if (dwOffset > timerDuration)			//if we've passed the next timer duration
	{
		dwLastTime = now;            
		dwRest = dwOffset - timerDuration;     
		processAudio(emptyBuf, (sampleRate * timerDuration / 1000), 2, now - dwStartStamp);
	}
}

//- processing methods --------------------------------------------------------

void VSTHost::processAudio(float **pBuffer, int nLength, int nChannels, DWORD dwStamp)
{
	VSTPlugin *pEff;
	float *pBuf1, *pBuf2;

	if (nLength > blockSize)               
		nLength = blockSize;

	//zero out output buf
	for (int j = 0; j < 2; j++)
	{
		pBuf1 = pOutputs[j];
		if (!pBuf1)
			break;
		for (int k = 0; k < nLength; k++)   
			*pBuf1++ = 0.0f;
	}

	float fMult = 1.0f / pluginCount;

	//for each plugin
	for (int i = 0; i < pluginCount; i++) {

		pEff = getPlugin(i);
		if (pEff == NULL)
			continue;

		pEff->enterCritical();

		pEff->buildMIDIEvents();
		if (pEff->pEvents) {
			pEff->processEvents();	    
		}

		pEff->doProcessReplacing(nLength);	

		//sum plugin output
		for (int j = 0; j < 2; j++)
		{
			pBuf1 = pOutputs[j];
			pBuf2 = pEff->getOutputBuffer(j);
			if ((!pBuf1) || (!pBuf2))
				break;
			for (int k = 0; k < nLength; k++)   
				*pBuf1++ += *pBuf2++ * fMult;
		}

		pEff->leaveCritical();              
	}

	// now that we've got a populated output buffer set, send it to 
	// the Wave Output device.
	waveOut->writeOut(pOutputs, nLength);
}
