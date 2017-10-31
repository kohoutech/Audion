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

#if !defined(PROJECT_H)
#define PROJECT_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

class Waverly;
class Transport;
class Track;

class Project
{
public:
	Project(Waverly* AWaverly, int sampleRate, int dataSize, int duration);
	Project(Waverly* AWaverly, char* filename);
	~Project();

	void close();
	void save(char * filename);

	Waverly* AWaverly;
	Transport* transport;

	int sampleRate;
	int duration;			//in seconds
	int dataSize;
	float leftLevel;
	float rightLevel;
	float getLeftLevel();
	float getRightLevel();

	Track** tracks;
	int trackCount;
	int trackLimit;

	Track* getTrack(int trackNum);
	Track* addTrack(int trackNum);
	void deleteTrack(int trackNum);
	Track* copyTrack(int trackNum);

	void loadTrackData(int trackNum, void* inhdl);
	void expandTrackLengths(int newDuration);

	int importTracksFromWavFile(char* filename);
	BOOL exportTracksToWavFile(char* filename);

protected:
};

#endif // PROJECT_H
