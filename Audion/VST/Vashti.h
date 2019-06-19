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

#if !defined(Vashti_H)
#define Vashti_H

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

#include "VSTPlugin.h"
#include "VSTHost.h"

class WaveOutDevice;

class Vashti
{
public:
	Vashti();
	~Vashti();

	static Vashti* vashtiB;		//for front end communication

	VSTHost* vstHost;

	//host
	void setPlugAudioIn(int vstnum, int idx);
	void setPlugAudioOut(int vstnum, int idx);

	//plugin 
	char* getProgramName(int vstNum, int paramnum);
	void setProgram(int vstNum, int prognum);
};

#endif // Vashti_H

