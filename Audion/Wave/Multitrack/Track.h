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

#if !defined(TRACK_H)
#define TRACK_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

class Project;
class Transport;

class Track
{
public:
	Track(Project* _project, int _trackNum, int _dataSize);
	~Track();

	int trackNum;
	float* dataBuf;
	int dataSize;

	inline BOOL getMute() { return mute; }
	void setMute(BOOL _mute) { mute = _mute; }
	inline BOOL getRecording() { return recording; }
	void setRecording(BOOL _rec) { recording = _rec; }
	inline float getVolume() { return volume; }
	void setVolume(float _vol) { volume = _vol; }
	inline float getLeftPan() { return leftPan; }
	inline float getRightPan() { return rightPan; }
	void setPan(float _pan) { rightPan = _pan; leftPan = 1.0f - rightPan; }

	void calcTrackEnvelope();
	void paintTrackData (void* _hdc, int width, int startpos);

	void expandLength(int newDataSize);
	void saveTrackData(void* outhdl);

protected:	
	Project* project;
	Transport* transport;

	BOOL mute;
	BOOL recording;
	float volume;
	float leftPan, rightPan;

	int zoomFactor;
	POINT* dataEnvPos;
	POINT* dataEnvNeg;
	int samplesPerPixel;
	int envSize;
	int prevEnvSize;
};

#endif // TRACK_H
