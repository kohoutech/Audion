/* ----------------------------------------------------------------------------
Transonic VST Library
Copyright (C) 2005-2019  George E Greaney

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

#include "WaveBuffer.h"

//cons
WaveBuffer::WaveBuffer(int bufSize)
{
	waveHdr = (LPWAVEHDR)malloc(sizeof(WAVEHDR));
	dataBuf = NULL;
	timeStamp = 0;
	inUse = FALSE;
	isRecording = FALSE;

	if (waveHdr)
	{
		memset((void*)waveHdr, 0, sizeof(WAVEHDR));

		dataBuf = (LPSTR) malloc(bufSize);
		waveHdr->lpData = dataBuf;

		waveHdr->dwBufferLength = bufSize;
		waveHdr->dwUser = (DWORD_PTR)this;			//we use waveHdr's generic dwUser field to point to parent WaveBuffer obj
		waveHdr->dwBytesRecorded = 0;
		memset(waveHdr->lpData, 0, bufSize);		//init buf to 0's
	}
}

//destruct
WaveBuffer::~WaveBuffer() 
{
	if (waveHdr)
		delete waveHdr;
	if (dataBuf)
		delete dataBuf;
}

//length for both use in input and output
DWORD WaveBuffer::Length()
{
	if (!waveHdr)                             
		return 0;                           
	if (isRecording)  
		return waveHdr->dwBytesRecorded;			//if recording, length = num of bytes recorded to buf
	else                                    
		return waveHdr->dwBufferLength;				//if playing, then length = the whole buffer (we fill it up)
}
