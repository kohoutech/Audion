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

#if !defined(TRANSPORT_H)
#define TRANSPORT_H

#include <windows.h>                    

class Waverly;
class AudioData;
class Project;
class Track;
class WaveInDevice;
class WaveOutDevice;

class Transport
{
public:
	Transport(Waverly *_waverly);
	~Transport();

	void setAudioData(AudioData* _audioData) {audioData = _audioData; }
	void setWaveIn (WaveInDevice* _waveIn) { waveIn = _waveIn; }
	void setWaveOut (WaveOutDevice* _waveOut) { waveOut = _waveOut; }
	void setBlockSize (int _size);

	void play();
	void pause();
	void stop();
	void rewind();
	void fastForward(int speed);
	void record();

	BOOL isCurRunning() { return isRunning; }
	BOOL isCurPlaying() { return isPlaying; }
	BOOL isCurRecording() { return isRecording; }

	int getCurrentPos() { return playbackPos; }
	void setCurrentPos(int pos);
	void setLeftOutLevel(float level) { leftOutLevel = level; }
	void setRightOutLevel(float level) { rightOutLevel = level; }
	float getLeftMaxLevel() { return isCurPlaying() ? leftMax : 0.0f; }
	float getRightMaxLevel() { return isCurPlaying() ? rightMax : 0.0f; }

	//input
//	void audioIn(float** pBuffers, int dataSize, int channels, DWORD timestamp, Track* track);

protected:
	Waverly * waverly;
	AudioData* audioData;
	WaveInDevice * waveIn;
	WaveOutDevice * waveOut;

	//status
	BOOL isRunning;
	BOOL isPaused;
	BOOL isPlaying;
	BOOL isRecording;

	int sampleRate;
	int sampleCount;					//total AudioData sample count
    long blockSize;						//num samples per block for one channel

	float* dataBuf;						//holds the data we get from an AudioData channel
	int recordPos;
	int playbackPos;					//where we are in the audio data, if the data is generated dynamically (VST) this has no meaning

	float* outputBuf[2];				//output buf for mixing down to stereo
	float leftOutLevel;					//set left/right output volume
	float rightOutLevel;
	float leftMax;						//max output level during block - for frontend level meter
	float rightMax;

//	int playSpeed;					
	CRITICAL_SECTION cs;

	//management
	void startUp();
	void shutDown();

	//timing
	UINT timerID;
	TIMECAPS tc;
	BOOL startTimer(UINT msSec=0);
	void stopTimer();
	static void CALLBACK timerCallback(UINT uID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);

	//output
    void audioOut();
};

#endif // TRANSPORT_H
