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

#if !defined(WAVEBUFFER_H)
#define WAVEBUFFER_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

//used by both WaveInDevice and WaveOutDevice
//isRecording tells if being used for input or output
class WaveBuffer
{
public:
	WaveBuffer(int size);
	~WaveBuffer();

    DWORD Length();                     
    
	BOOL isInUse() { return inUse; }
	void setInUse(BOOL _inUse) { inUse = _inUse; }

	BOOL isDevRecording() { return isRecording; }
	void setDevRecording(BOOL _recording) { isRecording = _recording; }

    DWORD getTimestamp() { return timeStamp; }
    void setTimestamp(DWORD _time) { timeStamp = _time; }

    LPWAVEHDR waveHdr;      //used by Windows for reading/writing audio data
	LPSTR dataBuf;			//the audio data buffer

protected:
	BOOL inUse;				//being used by Windows, false means we need to handle the data
	BOOL isRecording;		//being used for input
    DWORD timeStamp;		//if used for input, the time the data was recorded
};

#endif // WAVEBUFFER_H
