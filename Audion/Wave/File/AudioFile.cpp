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

#include "AudioFile.h"
#include "..\Waverly.h"
#include "..\Engine\Transport.h"

#include <math.h>

#define TRACKLIMIT 10
#define TRACKDELTA 5

//new AudioFile
AudioFile::AudioFile(Waverly* _AWaverly, char* filename) : AudioData (_AWaverly) {

	level = new float[2];
	leftPan = new float[2];
	rightPan = new float[2];

	for (int i = 0; i < 2; i++) 
	{
		level[i] = 1.0f;
		leftPan[i] = 0.5f;
		rightPan[i] = 0.5f;
	}

	channelCount = 2;
	tracks[0] = NULL;
	tracks[1] = NULL;

	importTracksFromWavFile(filename);
}

//destruct
AudioFile::~AudioFile() {

	for (int i = 0; i < 2; i++) {
		if (tracks[i] != NULL)
			delete tracks[i];
	}
}

//- channel management ----------------------------------------------------------

void AudioFile::getchannelData(int channelNum, float* dataBuf, int dataPos, int dataSize) {

	float* track = tracks[channelNum];
	int endPos = dataPos + dataSize;
	if (sampleCount > endPos) 
	{
		for (int i = dataPos, j = 0; (i < endPos); i++, j++)
			dataBuf[j] = track[i];
		//memcpy(dataBuf, &track[dataPos], dataSize * sizeof(float));
	} 
	else 
	{
		int copySize = sampleCount - dataPos;
		for (int i = dataPos, j = 0; i < copySize; i++, j++)
			dataBuf[j] = track[i];
		int remainder = dataSize - copySize;
		for (int i = dataPos; i < remainder; i++)
			dataBuf[i] = 0.0f;
	}
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

int AudioFile::importTracksFromWavFile(char* filename) {

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

	sampleCount = dataChunk.size / fmtChunk.blockallign;
	int sampleSize = fmtChunk.bitdepth / 8;

	int trackNum = 0;
	for (int channel = 0; channel < fmtChunk.channels; channel++) {

		float* track = new float[sampleCount];
		int bpos = channel * sampleSize;
		for (int i = 0; i < sampleCount; i++ ) {

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
			track[i] = sampleVal;
			bpos += fmtChunk.blockallign;			
		}
		tracks[channel] = track;
	}
	delete [] wavData;

	sampleRate = fmtChunk.samplerate;
	duration = (sampleCount + sampleRate - 1) / sampleRate;

	return fmtChunk.channels;
}

//printf("there's no sun in the shadow of the wizard.\n");
