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

#include <windows.h>
#include <stdio.h>
#include <math.h>

#include "Transport.h"
#include "..\Waverly.h"
#include "AudioData.h"
#include "..\System\WaveInDevice.h"
#include "..\System\WaveOutDevice.h"

//cons
Transport::Transport(Waverly *_waverly)
{
	waverly = _waverly;
	audioData = NULL;
	waveIn = NULL;
	waveOut = NULL;

	//default vals
	sampleRate = 44100;
	blockSize = sampleRate / 10;		//default duration = 100ms, 10 blocks / sec
	dataBuf = new float[blockSize];

	isRunning = FALSE;
	isPlaying = FALSE;
	isPaused = FALSE;

	sampleCount = 0;
	playbackPos = 0;
//	playSpeed = 1;

	timerID = 0;                              
	timeGetDevCaps(&tc, sizeof(tc));       

	for (int i = 0; i < 2; i++)
		outputBuf[i] = new float[blockSize];
	leftOutLevel = 1.0f;
	rightOutLevel = 1.0f;
	leftMax = 0.0f;
	rightMax = 0.0f;

	InitializeCriticalSection(&cs);
}

//destruct
Transport::~Transport()
{
	for (int i = 0; i < 2; i++)
		delete[] outputBuf[i];
}

void Transport::setBlockSize (int _size) { 

	if (!isPlaying) {
		blockSize = _size; 
		dataBuf = new float[blockSize];
	}
}

//- transport control ---------------------------------------------------------

void Transport::play() {

	isPlaying = TRUE;
	startUp();
}

void Transport::record(){

	isPlaying = TRUE;
	isRecording = TRUE;
	startUp();
	waveIn->start();
}

void Transport::pause(){

	isPaused = !isPaused;
}

void Transport::stop(){

	shutDown();
	isPlaying = FALSE;
	isPaused = FALSE;
	playbackPos = 0;
}

void Transport::rewind(){
}

//int speedFactor[4] = {1, 2, 4, 8};

void Transport::fastForward(int speed){

	//playSpeed = speedFactor[speed];
}

//- transport management ---------------------------------------------------------

void Transport::startUp()
{
	if (isRunning)
		return;

	sampleRate = audioData->sampleRate;
	sampleCount = audioData->sampleCount;	

//start output device and timer to send track data to it
	waveOut->start();		
	int timerDuration = (blockSize * 1000) / sampleRate;		//get timer period in msec
	startTimer(timerDuration);

	isRunning = TRUE;
}

void Transport::shutDown()
{
	if (!isRunning)
		return;

	//stop output device
	stopTimer(); 
	waveOut->stop();
	isRunning = FALSE;
}

void Transport::setCurrentPos(int pos) 
{ 
 	playbackPos = pos; 
}

//- timer methods -------------------------------------------------------------

BOOL Transport::startTimer(UINT msSec)
{
	if (msSec < tc.wPeriodMin)          
		msSec = tc.wPeriodMin;

	int resolution = msSec / 10;
	timeBeginPeriod(resolution);        

	timerID = timeSetEvent(msSec, (resolution > 1) ? resolution / 2 : 1, timerCallback, (DWORD)this, TIME_PERIODIC);

	return (timerID != NULL);
}

void Transport::stopTimer() 
{
	if (timerID != NULL)
	{
		timeKillEvent(timerID);
		timerID = 0;
		timeEndPeriod(tc.wPeriodMin);
	}
}

//send a block of audio data to wave out device
void CALLBACK Transport::timerCallback(UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2)
{
	if (dwUser)                             
		((Transport *)dwUser)->audioOut();
}

//- processing methods --------------------------------------------------------

//record in mono for now, ignoring channel num
//void Transport::audioIn(float** pBuffers, int size, int channels, DWORD timestamp, Track* track) 
//{
//	if (isRecording && !isPaused) {
//		if (track->getRecording()) {
//
//			int trackPos = recordPos;
//			float* dataBuf = track->dataBuf;
//			float* inBuf = pBuffers[0];
//			for (int samp = 0; samp < size; samp++) {
//				dataBuf[trackPos++] = inBuf[samp];
//				if (trackPos > sampleCount)
//					trackPos = 0;
//			}
//		}
//		recordPos += size;
//		if (recordPos > sampleCount)
//			recordPos = 0;
//	}
//}

void Transport::audioOut()
{	
	float* leftOut = outputBuf[0];
	float* rightOut = outputBuf[1];

//zero out left & right output bufs - if paused, this is all we need
	for (int samp = 0; samp < blockSize; samp++) {
		leftOut[samp] = rightOut[samp] = 0.0f;		
	}

	if (!isPaused) {

		//sum audio data from each channel into left & right output buffers, based on vol & pan settings
		for (int i = 0; i < audioData->getchannelCount(); i++) {

			audioData->getchannelData(i, dataBuf, playbackPos, blockSize);
			float leftPan = audioData->getLeftPan(i);
			float rightPan = audioData->getRightPan(i);

			for (int samp = 0; samp < blockSize; samp++) {

				leftOut[samp] += (dataBuf[samp] * leftPan * leftOutLevel);
				rightOut[samp] += (dataBuf[samp] * rightPan * rightOutLevel);
			}
		}

		//update playback pos, wrap pos if at end of track buf
		playbackPos += blockSize;
		if (playbackPos > sampleCount)
			playbackPos = 0;

		//scaling outputs - get max vales for both channels for SignalsA level meter display
		//use hard clipping to keep output values between -1.0 and 1.0, maybe use an actual clipping algorithm?
		leftMax = 0.0f;
		rightMax = 0.0f;
		float fMult = (float)sqrt(1.0f / (2));
		for (int samp = 0; samp < blockSize; samp++) {

			leftOut[samp] *= fMult;
			if (leftOut[samp] > leftMax) leftMax = leftOut[samp];		
			if (leftOut[samp] > 1.0f) leftOut[samp] = 1.0f;				//HARD clip - not pleasent sounding

			rightOut[samp] *= fMult;
			if (rightOut[samp] > rightMax) rightMax = rightOut[samp];
			if (rightOut[samp] > 1.0f) rightOut[samp] = 1.0f;
		}
	}

	// send output buffers to the Wave Output device
	waveOut->writeOut(outputBuf, blockSize);	
}

//printf("there's no sun in the shadow of the wizard.\n");