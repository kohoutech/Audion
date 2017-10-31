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

#include "Waverly.h"
#include "Engine\Transport.h"
#include "System\WaveInDevice.h"
#include "System\WaveOutDevice.h"
#include "File\AudioFile.h"

#include <conio.h>

#define WAVEBUFCOUNT  10
#define WAVEBUFDURATION   100		//buf duration in ms

Waverly* Waverly::AWaverly;

//- waverly iface exports ----------------------------------------------------

extern "C" __declspec(dllexport) void WaverlyInit() {

	Waverly::AWaverly = new Waverly();
}

extern "C" __declspec(dllexport) void WaverlyShutDown() {

	delete Waverly::AWaverly;
}

//- transport exports ---------------------------------------------------------

extern "C" __declspec(dllexport) void TransportPlay() {

	Waverly::AWaverly->transport->play();	
}

extern "C" __declspec(dllexport) void TransportPause() {

	Waverly::AWaverly->transport->pause();
}

extern "C" __declspec(dllexport) void TransportStop() {

	Waverly::AWaverly->transport->stop();
}

extern "C" __declspec(dllexport) void TransportRewind(int speed) {

//	Waverly::AWaverly->transport->rewind(speed);
}

extern "C" __declspec(dllexport) void TransportFastForward(int speed) {

//	Waverly::AWaverly->transport->fastForward(speed);
}

extern "C" __declspec(dllexport) void TransportSetVolume(float volume) {

	Waverly::AWaverly->transport->setLeftOutLevel(volume);
	Waverly::AWaverly->transport->setRightOutLevel(volume);
}

extern "C" __declspec(dllexport) void TransportSetBalance(float balance) {

	return Waverly::AWaverly->currentAudioFile->setPan(balance, 0);
}

extern "C" __declspec(dllexport) float TransportGetLeftLevel() {

	return Waverly::AWaverly->transport->getLeftMaxLevel();	
}

extern "C" __declspec(dllexport) float TransportGetRightLevel() {

	return Waverly::AWaverly->transport->getRightMaxLevel();
}

extern "C" __declspec(dllexport) int TransportGetCurrentPos() {

	return Waverly::AWaverly->transport->getCurrentPos();		//in samples
}

extern "C" __declspec(dllexport) void TransportSetCurrentPos(int curPos) {

	Waverly::AWaverly->transport->setCurrentPos(curPos);		//in samples
}

//not implemented yet
extern "C" __declspec(dllexport) void TransportSetWaveOut(int deviceIdx) {
}

//- project exports -----------------------------------------------------------

extern "C" __declspec(dllexport) void AudioOpen(char* filename) {

	Waverly::AWaverly->openAudioFile(filename);
}

extern "C" __declspec(dllexport) void AudioClose() {

	Waverly::AWaverly->closeAudioFile();
}

extern "C" __declspec(dllexport) int AudioGetSampleRate() {

	return Waverly::AWaverly->currentAudioFile->sampleRate;
}

extern "C" __declspec(dllexport) int AudioGetDuration() {

	return Waverly::AWaverly->currentAudioFile->duration;
}

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------

//cons
Waverly::Waverly()
{
	transport = new Transport(this);
	
	//use default in & out for now
	loadWaveInDevice(WAVE_MAPPER);		// open input devices 
	loadWaveOutDevice(WAVE_MAPPER);		// open output device

	currentAudioFile = NULL;	
}

//shut down
Waverly::~Waverly()
{
	closeAudioFile();
	waveOut->close();	
	delete transport;
}

void Waverly::reportStatus(char* source, char* msg)
{
	strncpy_s(statusSource, source, 256);
	strncpy_s(statusMsg, msg, 256);
}

//- project methods ------------------------------------------------------------

void Waverly::openAudioFile(char* filename) 
{
	currentAudioFile = new AudioFile(this, filename);
}

void Waverly::closeAudioFile() {

	if (currentAudioFile != NULL) {
		delete currentAudioFile;
	}
	currentAudioFile = NULL;
}

//- device methods ------------------------------------------------------------

BOOL Waverly::loadWaveInDevice(int devID)
{
	BOOL result = FALSE;

	waveIn = new WaveInDevice(this);
	waveIn->setBufferCount(WAVEBUFCOUNT);
	waveIn->setBufferDuration(WAVEBUFDURATION);
	result = waveIn->open(devID, 44100, 16, 1);		//mono in

	transport->setWaveIn(waveIn);
	waveIn->transport = transport;

	return result;
}

BOOL Waverly::loadWaveOutDevice	(int devID)
{
	BOOL result = FALSE;

	waveOut = new WaveOutDevice(this);
	waveOut->setBufferCount(WAVEBUFCOUNT);
	waveOut->setBufferDuration(WAVEBUFDURATION);
	result = waveOut->open(devID, 44100, 16, 2);		//stereo out

	transport->setWaveOut(waveOut);
	transport->setBlockSize(4410);

	return result;
}

	//printf("there's no sun in the shadow of the wizard.\n");
