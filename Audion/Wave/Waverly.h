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

#if !defined(Waverly_H)
#define Waverly_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

class AudioFile;
class Transport;
class WaveInDevice;
class WaveOutDevice;

class Waverly
{
public:
	Waverly();
	~Waverly();

	static Waverly* AWaverly;		//for front end communication

	void reportStatus(char* source, char* msg);		//report status & errors back to front end

	Transport* transport;
	AudioFile* currentAudioFile;

	void openAudioFile(char* filename);
	void closeAudioFile();

	WaveInDevice* waveIn;
	WaveOutDevice* waveOut;

protected:
	char statusSource[256];
	char statusMsg[256];

	BOOL loadWaveInDevice(int devID);
	BOOL loadWaveOutDevice(int devID);
};

#endif // Waverly_H
