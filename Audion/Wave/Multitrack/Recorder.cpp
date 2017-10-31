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

#include "Project.h"
#include "Waverly.h"
#include "Transport.h"
#include "Track.h"

#include <math.h>

#define TRACKLIMIT 10
#define TRACKDELTA 5

//new project
Project::Project(Waverly* _waverly, int _sampleRate, int _dataSize, int _duration) {

	AWaverly = _waverly;
	transport = AWaverly->transport;
	transport->setProject(this);

	sampleRate = _sampleRate;
	dataSize = _dataSize;
	duration = _duration;

	leftLevel = 0.0f;
	rightLevel = 0.0f;

	trackLimit = TRACKLIMIT;
	tracks = new Track* [trackLimit];
	for (int i = 0; i < trackLimit; i++) {
		tracks[i] = NULL;
	}
	trackCount = 0;
}

//open prev project from file
Project::Project(Waverly* _waverly, char* filename) {
}

//destruct
Project::~Project() {

	transport->stop();

	for (int i = 0; i < trackLimit; i++) {
		if (tracks[i] != NULL)
			delete tracks[i];
	}
	delete[] tracks;
}

//- project i/o methods -------------------------------------------------------

void Project::close() {
}

void Project::save(char * filename) {
}

//- track management ----------------------------------------------------------

Track* Project::getTrack(int trackNum) { 

	Track* result = NULL;
	for (int i = 0; i < trackLimit; i++) {
		if (tracks[i] != NULL && tracks[i]->trackNum == trackNum) {
			result = tracks[i];
			break;
		}
	}
	return result;
}

Track* Project::addTrack(int trackNum) {

	//expand track array if needed
	if (trackCount == trackLimit) {
		trackLimit += TRACKDELTA;
		Track** temp = new Track* [trackLimit];
		for (int i = 0; i < trackLimit; i++) {
			temp[i] = NULL;
		}
		for (int i = 0; i < trackCount; i++) {
			temp[i] = tracks[i];
		}
		delete[] tracks;
		tracks = temp;
	}

	//find an open slot
	int slot = 0;
	for (int i = 0; i < trackLimit; i++) {
		if (tracks[i] == NULL) {
			slot = i;
			break;
		}
	}

	Track* track = new Track(this, trackNum, dataSize);
	tracks[slot] = track;
	trackCount++;
	return track;
}

void Project::deleteTrack(int trackNum) {

	Track* track = NULL;
	for (int i = 0; i < trackLimit; i++) {
		if (tracks[trackNum] != NULL && tracks[trackNum]->trackNum == trackNum) {
			Track* track = tracks[trackNum];
			tracks[trackNum] = NULL;
			delete track;
			trackCount--;
			break;
		}
	}
}

Track* Project::copyTrack(int trackNum) {

	return NULL;
}

void Project::expandTrackLengths(int newDuration) {

	int newDataSize = newDuration * sampleRate;
	for (int i = 0; i < trackLimit; i++) {
		if (tracks[i] != NULL)
			tracks[i]->expandLength(newDataSize);
	}
}

float Project::getLeftLevel() { 
	return (transport->isCurPlaying() ? leftLevel : 0.0f); 
}

float Project::getRightLevel() { 
	return (transport->isCurPlaying() ? rightLevel : 0.0f); 
}


//- load saved track data -----------------------------------------------------

void Project::loadTrackData(int trackNum, void* _inhdl) {

	HANDLE inhdl = (HANDLE) _inhdl;

	Track* track = addTrack(trackNum);
	byte* inbuf = (byte*) track->dataBuf;
	int insize = dataSize * 4;
	DWORD amountRead = 0;
	ReadFile(inhdl, inbuf, insize, &amountRead, NULL);
	track->calcTrackEnvelope();
}

//- data import/export methods ------------------------------------------------

typedef struct wavHdr {
	char sig[4];
	int filesize;
	char format[4];
} WavHdr;

typedef struct fmtChunk {
	char sig[4];
	int size;
	short format;
	short channels;
	int samplerate;
	int byterate;
	short blockallign;
	short bitdepth;
} FmtChunk;

typedef struct dataChunk {
	char sig[4];
	int size;
} DataChunk;

int Project::importTracksFromWavFile(char* filename) {

	FILE* wavFile = fopen (filename, "rb"); 

	//file hdr
	WavHdr wavHdr;
	int bread = fread (&wavHdr, sizeof(WavHdr), 1, wavFile);

	//fmt chunk
	FmtChunk fmtChunk;
	bread = fread (&fmtChunk, sizeof(FmtChunk), 1, wavFile);

	//data chunk
	DataChunk dataChunk;
	bread = fread (&dataChunk, sizeof(DataChunk), 1, wavFile);
	byte* wavData = new byte[dataChunk.size];
	bread = fread (wavData, dataChunk.size, 1, wavFile);

	int trackDataSize = dataChunk.size / fmtChunk.blockallign;
	if (trackDataSize > dataSize) {

		duration = (trackDataSize + sampleRate - 1)/sampleRate;
		expandTrackLengths(duration);
		dataSize = duration * sampleRate;
	}
	int sampleSize = fmtChunk.bitdepth / 8;

	int trackNum = trackCount;
	for (int channel = 0; channel < fmtChunk.channels; channel++) {

		Track* track = addTrack(trackNum++);
		int bpos = channel * sampleSize;
		for (int i = 0; i < trackDataSize; i++ ) {

			float sampleVal;
			switch (sampleSize) {
			case 1 : 
				sampleVal = (wavData[bpos] / 255.0f) - 0.5f;
				break;
			case 2 : {
				short samp = *(short*)&wavData[bpos];
				sampleVal = (samp / 32767.0f);
				break;
					 }
			default: 
				sampleVal = 0.0f;
				break;
			}
			track->dataBuf[i] = sampleVal;
			bpos += fmtChunk.blockallign;			
		}
		track->calcTrackEnvelope();

	}
	delete [] wavData;

	sampleRate = fmtChunk.samplerate;

	return fmtChunk.channels;
}

BOOL Project::exportTracksToWavFile(char* filename) {

	FILE* wavFile = fopen (filename, "wb"); 
	int fileDataSize = dataSize * 2 * 2;		//2 channels @ 16 bits

	//file hdr
	WavHdr wavHdr;
	memcpy(wavHdr.sig, "RIFF", 4);
	wavHdr.filesize = fileDataSize + sizeof(FmtChunk) + sizeof(DataChunk) + 4;
	memcpy(wavHdr.format, "WAVE", 4);
	int bwrite = fwrite(&wavHdr, sizeof(WavHdr), 1, wavFile);

	//fmt chunk
	FmtChunk fmtChunk;
	memcpy(fmtChunk.sig, "fmt ", 4);
	fmtChunk.size = 16;
	fmtChunk.format = 1;
	fmtChunk.channels = 2;
	fmtChunk.samplerate = sampleRate;
	fmtChunk.byterate = sampleRate * 4;		//4 bytes / sample
	fmtChunk.blockallign = 4;	
	fmtChunk.bitdepth = 16;
	bwrite = fwrite (&fmtChunk, sizeof(FmtChunk), 1, wavFile);

	//data chunk
	DataChunk dataChunk;
	memcpy(dataChunk.sig, "data", 4);
	dataChunk.size = fileDataSize;
	bwrite = fwrite (&dataChunk, sizeof(DataChunk), 1, wavFile);

	//mix track data down to left/right output, translate into byte vals and write to output wav file, one block at a time
	int blockSize = sampleRate * 10;
	float* leftOut = new float[blockSize];
	float* rightOut = new float[blockSize];
	int remainder = dataSize;
	int dataBufPos = 0;
	
	short* wavFileData = new short[blockSize *2];

	while (remainder > 0) {

		if (remainder < blockSize)		//last chunk 
			blockSize = remainder;

		//zero out output bufs to start
		for (int samp = 0; samp < blockSize; samp++) {
			leftOut[samp] = 0.0f;
			rightOut[samp] = 0.0f;
		}

		//sum audio data from each track into left & right output buffers, based on vol & pan settings
		for (int i = 0; i < trackCount; i++) {

			if (!tracks[i]->getMute()) {

				int trackDataPos = dataBufPos;
				float* dataBuf = tracks[i]->dataBuf;
				float trackVol = tracks[i]->getVolume();
				float leftPan = tracks[i]->getLeftPan();
				float rightPan = tracks[i]->getRightPan();

				for (int samp = 0; samp < blockSize; samp++) {
					leftOut[samp] += (dataBuf[trackDataPos] * trackVol * leftPan);
					rightOut[samp] += (dataBuf[trackDataPos] * trackVol * rightPan);
					trackDataPos++;
				}
			}
		}

		//scaling outputs - use hard clipping to keep output values between -1.0 and 1.0
		float fMult = (float)sqrt(1.0f / (trackCount));
		for (int samp = 0; samp < blockSize; samp++) {

			leftOut[samp] *= fMult;
			if (leftOut[samp] > 1.0f) leftOut[samp] = 1.0f;				//HARD clip - not pleasent sounding
			rightOut[samp] *= fMult;
			if (rightOut[samp] > 1.0f) rightOut[samp] = 1.0f;
		}

		//now convert mixed output float data to interleaved stero bytes
		for (int samp = 0; samp < blockSize; samp++ ) {
			 
			short leftout = (short)(leftOut[samp] * 32767.0f);
			wavFileData[samp * 2] = leftout;
			short rightout = (short)(rightOut[samp] * 32767.0f);
			wavFileData[samp * 2 + 1] = rightout;
		}

		//write block to wav file
		bwrite = fwrite (wavFileData, sizeof(short), blockSize * 2, wavFile);

		remainder -= blockSize;
		dataBufPos += blockSize;
	}

	fclose(wavFile);
	delete[] leftOut;
	delete[] rightOut;
	delete[] wavFileData;

	return TRUE;
}

//printf("there's no sun in the shadow of the wizard.\n");
