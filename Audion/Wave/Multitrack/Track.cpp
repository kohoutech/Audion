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

#include <stdio.h>
#include <windows.h>

#include "Track.h"
#include "Project.h"
#include "Transport.h"

Track::Track(Project* _project, int _trackNum, int _dataSize) {

	project = _project;
	transport = project->transport;

	trackNum = _trackNum;

//create new data buf & set to silence
	dataSize = _dataSize;
	dataBuf = new float[dataSize];
	for (int i = 0; i < dataSize; i++)
		dataBuf[i] = 0.0f;

//default params
	mute = FALSE;
	recording = FALSE;
	volume = 1.0f;
	leftPan = 0.5f;
	rightPan = 0.5f;

	zoomFactor = 5;			//for drawing wave envelope - 5 pixels / sec
	dataEnvPos = NULL;
	dataEnvNeg = NULL;
	prevEnvSize = 0;
	calcTrackEnvelope();
}

Track::~Track() {

	if (dataBuf != NULL) 
		delete[] dataBuf;
	if (dataEnvPos != NULL) 
		delete[] dataEnvPos;
	if (dataEnvNeg != NULL) 
		delete[] dataEnvNeg;
}

void Track::expandLength(int newDataSize) {

	if (newDataSize > dataSize) {

		float* tempBuf = new float[newDataSize];
		for (int i = 0; i < dataSize; i++)
			tempBuf[i] = dataBuf[i];
		for (int i = dataSize; i < newDataSize; i++)
			tempBuf[i] = 0.0f;
		delete[] dataBuf;
		dataBuf = tempBuf;
		dataSize = newDataSize;
	}
	calcTrackEnvelope();
}

//- painting track data in SignalsA -------------------------------------------

//asuume a fixed track height of 130
#define TRACKHEIGHT 130
#define CENTERLINE 64

//calc min and max for each 
void Track::calcTrackEnvelope() 
{
	samplesPerPixel = project->sampleRate / zoomFactor;
	envSize = dataSize / samplesPerPixel;

	if (envSize != prevEnvSize) {
	if (dataEnvPos != NULL) 
		delete[] dataEnvPos;
	if (dataEnvNeg != NULL) 
		delete[] dataEnvNeg;

	dataEnvPos = new POINT[envSize];
	dataEnvNeg = new POINT[envSize];
	}

	int envPos = 0;
	for (int tick = 0; tick < envSize; tick++) {
		float maxVal = 0.0f;
		float minVal = 0.0f;
		for (int samp = 0; samp < samplesPerPixel; samp++) {
			if (dataBuf[envPos] > maxVal) maxVal = dataBuf[envPos];
			if (dataBuf[envPos] < minVal) minVal = dataBuf[envPos];
			envPos++;
		}
		dataEnvPos[tick].x = tick;
		dataEnvPos[tick].y = (CENTERLINE - (int)(1.0f * maxVal * CENTERLINE));
		dataEnvNeg[tick].x = tick;
		dataEnvNeg[tick].y = (CENTERLINE + (int)(-1.0f * minVal * CENTERLINE));			
	}
	dataEnvPos[0].x = 0;			//fix to make envelope polygon close along the baseline
	dataEnvPos[0].y = CENTERLINE;
	dataEnvNeg[0].x = 0;
	dataEnvNeg[0].y = CENTERLINE;
	prevEnvSize = envSize;
}

void  Track::paintTrackData (void* _hdc, int width, int startpos) 
{
	HDC hdc = (HDC) _hdc;
	HPEN hPen1, hPen2, hPen3;
	HBRUSH hBrush1, hBrush2;
	HDC Memhdc;
	HBITMAP Membitmap;

	//if recording, recalc envelope so we can see the waveform as we are inputing audio data
	if (recording && transport->isCurPlaying())
		calcTrackEnvelope();

	//double buffering to avoid "summer lightning" (annoying flicker)
	Memhdc = CreateCompatibleDC(hdc);
	Membitmap = CreateCompatibleBitmap(hdc, width, TRACKHEIGHT);
	SelectObject(Memhdc, Membitmap);

	//scroll viewport based in startpos
	POINT prevpos;
	int shiftpos = startpos / samplesPerPixel;
	SetWindowOrgEx(hdc, shiftpos, 0, &prevpos);

	//background - grey (b0c4de)
	hBrush1 = CreateSolidBrush (RGB (176, 196, 222));
	SelectObject (Memhdc, hBrush1);
	Rectangle(Memhdc, 0, 0, width, TRACKHEIGHT);

	//data envelopes pos & neg - dodger blue (1e90ff)
	hPen1 = CreatePen (PS_SOLID, 1, RGB (30, 144, 255));
	hBrush2 = CreateSolidBrush (RGB (30, 144, 255));
	SelectObject (Memhdc, hPen1);
	SelectObject (Memhdc, hBrush2);
	Polygon (Memhdc, dataEnvPos, envSize) ;
	Polygon (Memhdc, dataEnvNeg, envSize) ;

	//center line - black
	hPen2 = CreatePen (PS_SOLID, 1, RGB (0, 0, 0));
	SelectObject (Memhdc, hPen2);
	MoveToEx (Memhdc, 0, CENTERLINE, NULL);
    LineTo (Memhdc, width, CENTERLINE);

	//cur pos marker - 
	int curPos = transport->getCurrentPos() / samplesPerPixel;
	hPen3 = CreatePen (PS_SOLID, 1, RGB (255, 0, 0));
	SelectObject (Memhdc, hPen3);
	MoveToEx (Memhdc, curPos, 0, NULL) ;
    LineTo (Memhdc, curPos, TRACKHEIGHT) ;

	BitBlt(hdc, 0, 0, width, TRACKHEIGHT, Memhdc, 0, 0, SRCCOPY);

	SetWindowOrgEx(hdc, prevpos.x, prevpos.y, NULL);

	DeleteObject(Membitmap);
	DeleteDC (Memhdc);

	DeleteObject(hPen1);
	DeleteObject(hPen2);
	DeleteObject(hPen3);
	DeleteObject(hBrush1);
	DeleteObject(hBrush2);
}

//- i/o -----------------------------------------------------------------------

void Track::saveTrackData(void* _outhdl) {	

	HANDLE outhdl = (HANDLE) _outhdl;
	byte* outbuf = (byte*) dataBuf;
	int outsize = dataSize * 4;
	DWORD amountRead = 0;
	WriteFile(outhdl, outbuf, outsize, &amountRead, NULL);
}

	//printf("there's no sun in the shadow of the wizard.\n");
