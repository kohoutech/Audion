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

#if !defined(AUDIODATA_H)
#define AUDIODATA_H

class Waverly;
class Transport;

class AudioData
{
public:
	AudioData(Waverly* AWaverly);
	~AudioData();

	Waverly* AWaverly;
	Transport* transport;

	//for all channels
	int sampleRate;
	int sampleCount;		//samples stored as floats
	int duration;			//in seconds

	inline float getLevel(int channelNum) { return (channelNum < channelCount) ? level[channelNum] : 0; }
	inline float getLeftPan(int channelNum) { return (channelNum < channelCount) ? leftPan[channelNum] : 0; }
	inline float getRightPan(int channelNum) { return (channelNum < channelCount) ? rightPan[channelNum] : 0; }

	void setLevel(int channelNum, float _level);
	void setPan(int channelNum, float _pan);

	int getchannelCount() { return channelCount; }
	virtual void setchannelCount(int count);
	virtual void getchannelData(int channelNum, float* dataBuf, int dataPos, int dataSize);

protected:
	//for each channel, range = 0.0 to 1.0
	float* level;			
	float* leftPan;
	float* rightPan;

	//channel data
	int channelCount;
};

#endif // AUDIODATA_H
