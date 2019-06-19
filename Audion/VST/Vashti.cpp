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

#include "Vashti.h"

#include "VSTPlugin.h"
#include "VSTHost.h"
#include "WaveOutDevice.h"

#define DEFAULTSAMPLERATE 44100
#define WAVEBUFCOUNT  20			//num buffers / sec
#define WAVEBUFDURATION   50		//buf duration in ms

Vashti* Vashti::vashtiB;

//- Vashti iface exports ----------------------------------------------------

extern "C" __declspec(dllexport) void VashtiInit() {

	Vashti::vashtiB = new Vashti();
}

extern "C" __declspec(dllexport) void VashtiShutDown() {

	delete Vashti::vashtiB;
}

//- host exports ------------------------------------------------------------

extern "C" __declspec(dllexport) void VashtiStartEngine() {

	Vashti::vashtiB->vstHost->startEngine();
}

extern "C" __declspec(dllexport) void VashtiStopEngine() {

	Vashti::vashtiB->vstHost->stopEngine();
}

extern "C" __declspec(dllexport) int VashtiLoadPlugin(char* name) {

	return Vashti::vashtiB->vstHost->loadPlugin(name);
}

extern "C" __declspec(dllexport) void VashtiUnloadPlugin(int vstnum) {

	Vashti::vashtiB->vstHost->unloadPlugin(vstnum);
}

extern "C" __declspec(dllexport) void VashtiSetSampleRate(int rate) {

	Vashti::vashtiB->vstHost->setSampleRate(rate);
}

extern "C" __declspec(dllexport) void VashtiSetBlockSize(int size) {

	Vashti::vashtiB->vstHost->setBlockSize(size);
}

//- plugin exports ------------------------------------------------------------
//-----------------------------------------------------------------------------

extern "C" __declspec(dllexport) void VashtiSetPluginAudioIn(int vstnum, int idx) {

	Vashti::vashtiB->setPlugAudioIn(vstnum, idx);
}

extern "C" __declspec(dllexport) void VashtiSetPluginAudioOut(int vstnum, int idx) {

	Vashti::vashtiB->setPlugAudioOut(vstnum, idx);
}

extern "C" __declspec(dllexport) void VashtiGetPluginInfo(int vstnum, PlugInfo* pinfo) {

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		vst->getPlugInfo(pinfo);
	}
}

//- plugin params -------------------------------------------------------------

extern "C" __declspec(dllexport) float VashtiGetParamValue(int vstnum, int paramnum){

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		return vst->getParameter(paramnum);
	}
	return 0.0f;	
}

extern "C" __declspec(dllexport) void VashtiSetParamValue(int vstnum, int paramnum, float paramval){

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{	
		vst->setParameter(paramnum, paramval);
	}	
}

extern "C" __declspec(dllexport) char* VashtiGetParamLabel(int vstnum, int paramnum){

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		char* paramname = (char*) CoTaskMemAlloc(kVstMaxNameLen);
		vst->getParamLabel(paramnum, paramname);
		return paramname;
	}	
	return NULL;
}

extern "C" __declspec(dllexport) char* VashtiGetParamDisplay(int vstnum, int paramnum){

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		char* paramname = (char*) CoTaskMemAlloc(kVstMaxNameLen);
		vst->getParamDisplay(paramnum, paramname);
		return paramname;
	}	
	return NULL;
}

extern "C" __declspec(dllexport) char* VashtiGetParamName(int vstnum, int paramnum){

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		char* paramname = (char*) CoTaskMemAlloc(kVstMaxNameLen);
		vst->getParamName(paramnum, paramname);
		return paramname;
	}	
	return NULL;
}

//- plugin programs -----------------------------------------------------------

extern "C" __declspec(dllexport) char* VashtiGetProgramName(int vstnum, int prognum) {

	return Vashti::vashtiB->getProgramName(vstnum, prognum);
}

extern "C" __declspec(dllexport) void VashtiSetProgram(int vstnum, int prognum) {

	Vashti::vashtiB->setProgram(vstnum, prognum);
}

//- plugin editor -------------------------------------------------------------

extern "C" __declspec(dllexport) void VashtiOpenEditor(int vstnum, void* hwnd) {

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		vst->editOpen(hwnd);
	}	
}

extern "C" __declspec(dllexport) void VashtiCloseEditor(int vstnum) {

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		vst->editClose();
	}	
}

extern "C" __declspec(dllexport) void VashtiHandleMidiMsg(int vstnum, int b1, int b2, int b3) {

	VSTPlugin* vst = Vashti::vashtiB->vstHost->getPlugin(vstnum);
	if (vst) 
	{
		vst->storeMidiShortMsg(b1, b2, b3);
	}	
}

//---------------------------------------------------------------------------

Vashti::Vashti() 
{
	vstHost = new VSTHost();
}

Vashti::~Vashti() 
{
	delete vstHost;
}

//- plugin methods ------------------------------------------------------------

void Vashti::setPlugAudioIn(int vstnum, int idx)
{
}

void Vashti::setPlugAudioOut(int vstnum, int idx)
{
}

//- plugin programs -----------------------------------------------------------

char* Vashti::getProgramName(int vstNum, int prognum) 
{
	VSTPlugin* vst = vstHost->getPlugin(vstNum);
	if (vst) 
	{
		char* progname = (char*) CoTaskMemAlloc(kVstMaxNameLen);
		vst->getProgramNameIndexed(0, prognum, progname);		
		return progname;
	}
}

void Vashti::setProgram(int vstNum, int prognum) 
{
	VSTPlugin* vst = vstHost->getPlugin(vstNum);
	if (vst) 
	{
		vst->setProgram(prognum);
	}
}


//printf("there's no sun in the shadow of the wizard.\n");
