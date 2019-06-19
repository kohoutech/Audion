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

#include "VSTPlugin.h"
#include "VSTHost.h"

//cons
VSTPlugin::VSTPlugin(VSTHost *_pHost)
{
	pHost = _pHost;
	pEffect = NULL;
	hModule = NULL;	

	outBufs = NULL;
	outBufCount = 0;

	midiBufCount = 0;
	pEvents = NULL;

	InitializeCriticalSection(&cs);
}

//destuct
VSTPlugin::~VSTPlugin()
{
	unload();

	enterCritical();

	if (outBufs)
	{
		for (int i = 0; i < outBufCount; i++)
			if (outBufs[i]) 
				delete[] outBufs[i];
		delete[] outBufs;
	}

	leaveCritical();
	DeleteCriticalSection(&cs);
}

bool VSTPlugin::load(const char *filename)
{
	//load VST dll into memory
	//printf("loading plugin %s\n",name);
	hModule = ::LoadLibrary(filename);		
	if (!hModule)
		return false;

	//get VST's entry point
	AEffect *(*pMain)(long (*audioMaster)(AEffect *effect, long opcode, long index, long value, void *ptr, float opt)) = NULL;
	pMain = (AEffect * (*)(long (*)(AEffect *,long,long,long,void *,float))) ::GetProcAddress(hModule, "VSTPluginMain");
	if (!pMain)
		pMain = (AEffect * (*)(long (*)(AEffect *,long,long,long,void *,float))) ::GetProcAddress(hModule, "main");

	if (!pMain)
		return false;

	//get effect struct from VST, pass host's callback func to VST
	//this calls the callback with audioMasterVersion to get the host's version before returning the effect struct
	pEffect = pMain(AudioMasterCallback);

	//check if valid effect
	if (pEffect && (pEffect->magic != kEffectMagic)) {
		pEffect = NULL;
		return false;
	}

	//store plugin obj 
	pEffect->user = this;

	//get output buffers
	if (pEffect->numOutputs)
	{
		int nAlloc = pEffect->numOutputs < 2 ? 2 : pEffect->numOutputs;		//min output = stereo channels
		outBufs = new float *[nAlloc];
		if (!outBufs)
			return false;
		for (int i = 0; i < pEffect->numOutputs; i++)
		{
			outBufs[i] = new float[OUTPUTBUFSIZE];		
			if (!outBufs[i])
				return false;
			for (int j = 0; j < OUTPUTBUFSIZE; j++)
				outBufs[i][j] = 0.0f;
		}
		outBufCount = nAlloc;
	}

	return true;
}

void VSTPlugin::unload()
{
	close();

	if (pEvents)
		delete pEvents;
	pEffect = NULL;

	//unload DLL from memory
	if (hModule)
	{
		::FreeLibrary(hModule);
		hModule = NULL;       
	}
}

void VSTPlugin::getPlugInfo(PlugInfo* pinfo) 
{
	pinfo->name = (char*) CoTaskMemAlloc(kVstMaxNameLen);
	getProductString(pinfo->name);
	pinfo->vendor = (char*) CoTaskMemAlloc(kVstMaxNameLen);
	getVendorString(pinfo->vendor);
	pinfo->version = getVstVersion();
	pinfo->numPrograms = pEffect->numPrograms;
	pinfo->numParameters = pEffect->numParams;
	pinfo->numInputs = pEffect->numInputs;
	pinfo->numOutputs = pEffect->numOutputs;
	pinfo->flags = pEffect->flags;
	pinfo->uniqueID = pEffect->uniqueID;

	if (pEffect->flags && effFlagsHasEditor != 0) {
		ERect* pRect;
		editGetRect(&pRect);
		pinfo->editorWidth = pRect->right - pRect->left;
		pinfo->editorHeight = pRect->bottom - pRect->top;
	} else {
		pinfo->editorWidth = 0;
		pinfo->editorHeight = 0;
	}
}

//- processing methods --------------------------------------------------------

void VSTPlugin::storeMidiShortMsg(int b1, int b2, int b3)
{
	if (b1 >= 0xf0)         //ignore realtime or sysex msgs
		return;                          

	//printf("storing midi msg = %i\n",b1);
	EnterCriticalSection(&cs);
	int timestamp = 0;
	if (midiBufCount < (MIDIBUFFERSIZE - 4))
	{	
		midiBuffer[midiBufCount++] = b1;
		midiBuffer[midiBufCount++] = b2;
		midiBuffer[midiBufCount++] = b3;
		midiBuffer[midiBufCount++] = timestamp;
	}
	LeaveCriticalSection(&cs);
}

void VSTPlugin::buildMIDIEvents()
{
	if (pEvents)
		delete[] pEvents;
	pEvents = NULL;

	if (midiBufCount == 0)          //no events in buf
		return;

	//enterCritical();
	int eventCount = midiBufCount / 4;

	int hdrSize = sizeof(VstEvents) + (eventCount * sizeof(VstMidiEvent *));
	int eventSize = hdrSize + (eventCount * sizeof(VstMidiEvent));
	BYTE* eventData = new BYTE[eventSize];								//alloc event data struct

	if (eventData)                            
	{           
		//set event hdr data
		pEvents = (VstEvents *) eventData;
		memset(pEvents, 0, eventSize);
		pEvents->numEvents = eventCount;

		//store event data
		VstMidiEvent* pEvent = ((VstMidiEvent *)(eventData + hdrSize));
		int midiBufPos = 0;
		for (int i = 0; i < eventCount; i ++)     
		{
			pEvents->events[i] = (VstEvent *)&pEvent[i];		//set next event ptr to VstEvent rec

			pEvent[i].type = kVstMidiType;
			pEvent[i].byteSize = sizeof(VstMidiEvent);
			pEvent[i].midiData[0] = (char)midiBuffer[midiBufPos++];
			pEvent[i].midiData[1] = (char)midiBuffer[midiBufPos++];
			pEvent[i].midiData[2] = (char)midiBuffer[midiBufPos++];			
			midiBufPos++;												//skip timestamp for now
			pEvent[i].deltaFrames = 0;
			pEvent[i].flags = kVstMidiEventIsRealtime;			
		}
	}

	midiBufCount = 0;				//clear out midi buf
	//leaveCritical();
}

float * VSTPlugin::getOutputBuffer(int bufIdx)
{
	if (bufIdx >= 0 && bufIdx < outBufCount)
		return outBufs[bufIdx];
	else
		return 0;
}

void VSTPlugin::doProcess(long sampleFrames)
{
	process(inBufs, outBufs, sampleFrames);
}

void VSTPlugin::doProcessReplacing(long sampleFrames)
{
	processReplacing(inBufs, outBufs, sampleFrames);
}

//- VST functions ----------------------------------------------------------

long VSTPlugin::dispatch(long opCode, long index, long value, void *ptr, float opt)
{
	if (!pEffect)
		return 0;

	return pEffect->dispatcher(pEffect, opCode, index, value, ptr, opt);
}

void VSTPlugin::setParameter(long index, float parameter)
{
	if (!pEffect)
		return;

	pEffect->setParameter(pEffect, index, parameter);
}

float VSTPlugin::getParameter(long index)
{
	if (!pEffect)
		return 0.0f;

	return pEffect->getParameter(pEffect, index);
}

void VSTPlugin::process(float **inputs, float **outputs, long sampleframes)
{
	if (!pEffect)
		return;

	pEffect->process(pEffect, inputs, outputs, sampleframes);
}

void VSTPlugin::processReplacing(float **inputs, float **outputs, long sampleframes)
{
	if ((!pEffect) ||
		(!(pEffect->flags & effFlagsCanReplacing)))
		return;

	pEffect->processReplacing(pEffect, inputs, outputs, sampleframes);
}

void VSTPlugin::processDoubleReplacing(double **inputs, double **outputs, long sampleFrames)
{
	if ((!pEffect) ||
		(!(pEffect->flags & effFlagsCanDoubleReplacing)))
		return;

	pEffect->processDoubleReplacing(pEffect, inputs, outputs, sampleFrames);
}

//- AudioEffect dispatcher functions -----------------------------------------------

void VSTPlugin::open() 
{ 
	dispatch(effOpen); 
}

void VSTPlugin::close() 
{ 
	dispatch(effClose); 
}

void VSTPlugin::setProgram(long lValue) 
{ 
	dispatch(effSetProgram, 0, lValue); 
}

long VSTPlugin::getProgram() 
{ 
	return dispatch(effGetProgram); 
}

void VSTPlugin::setProgramName(char *ptr) 
{ 
	dispatch(effSetProgramName, 0, 0, ptr); 
}

void VSTPlugin::getProgramName(char *ptr) 
{ 
	dispatch(effGetProgramName, 0, 0, ptr); 
}

void VSTPlugin::getParamLabel(long index, char *ptr) 
{ 
	dispatch(effGetParamLabel, index, 0, ptr); 
}

void VSTPlugin::getParamDisplay(long index, char *ptr) 
{ 
	dispatch(effGetParamDisplay, index, 0, ptr); 
}

void VSTPlugin::getParamName(long index, char *ptr) 
{ 
	dispatch(effGetParamName, index, 0, ptr); 
}

void VSTPlugin::setSampleRate(float fSampleRate) 
{ 
	dispatch(effSetSampleRate, 0, 0, 0, fSampleRate); 
}

void VSTPlugin::setBlockSize(long value) 
{ 
	dispatch(effSetBlockSize, 0, value); 
}

void VSTPlugin::suspend() 
{ 
	dispatch(effMainsChanged, 0, false); 
}

void VSTPlugin::resume() 
{ 
	dispatch(effMainsChanged, 0, true); 
}

long VSTPlugin::getChunk(void **data, bool isPreset) 
{ 
	return dispatch(effGetChunk, isPreset, 0, data); 
}

long VSTPlugin::setChunk(void *data, long byteSize, bool isPreset) 
{ 
	return dispatch(effSetChunk, isPreset, byteSize, data); 
}

//- AudioEffectX dispatcher functions -----------------------------------------------

long VSTPlugin::processEvents() 
{ 
	return dispatch(effProcessEvents, 0, 0, pEvents); 
}

long VSTPlugin::canParameterBeAutomated(long index) 
{ 
	return dispatch(effCanBeAutomated, index); 
}

long VSTPlugin::string2Parameter(long index, char *ptr) 
{ 
	return dispatch(effString2Parameter, index, 0, ptr); 
}

long VSTPlugin::getProgramNameIndexed(long category, long index, char* text) 
{ 
	return dispatch(effGetProgramNameIndexed, index, category, text); 
}

long VSTPlugin::getInputProperties(long index, VstPinProperties *ptr) 
{ 
	return dispatch(effGetInputProperties, index, 0, ptr); 
}

long VSTPlugin::getOutputProperties(long index, VstPinProperties *ptr) 
{ 
	return dispatch(effGetOutputProperties, index, 0, ptr); 
}

long VSTPlugin::getPlugCategory() 
{ 
	return dispatch(effGetPlugCategory); 
}

long VSTPlugin::offlineNotify(VstAudioFile* ptr, long numAudioFiles, bool start) 
{ 
	return dispatch(effOfflineNotify, start, numAudioFiles, ptr); 
}

long VSTPlugin::offlinePrepare(VstOfflineTask *ptr, long count) 
{ 
	return dispatch(effOfflinePrepare, 0, count, ptr); 
}

long VSTPlugin::offlineRun(VstOfflineTask *ptr, long count) 
{ 
	return dispatch(effOfflineRun, 0, count, ptr); 
}

long VSTPlugin::setSpeakerArrangement(VstSpeakerArrangement* pluginInput, VstSpeakerArrangement* pluginOutput) 
{ 
	return dispatch(effSetSpeakerArrangement, 0, (long)pluginInput, pluginOutput); 
}

long VSTPlugin::processVarIo(VstVariableIo* varIo) 
{ 
	return dispatch(effProcessVarIo, 0, 0, varIo); 
}

long VSTPlugin::setBypass(bool onOff) 
{ 
	return dispatch(effSetBypass, 0, onOff); 
}

long VSTPlugin::getEffectName(char *ptr) 
{ 
	return dispatch(effGetEffectName, 0, 0, ptr); 
}

long VSTPlugin::getVendorString(char *ptr) 
{ 
	return dispatch(effGetVendorString, 0, 0, ptr); 
}

long VSTPlugin::getProductString(char *ptr) 
{ 
	return dispatch(effGetProductString, 0, 0, ptr); 
}

long VSTPlugin::getVendorVersion() 
{ 
	return dispatch(effGetVendorVersion); 
}

long VSTPlugin::vendorSpecific(long index, long value, void *ptr, float opt) 
{ 
	return dispatch(effVendorSpecific, index, value, ptr, opt); 
}

long VSTPlugin::canDo(const char *ptr) 
{ 
	return dispatch(effCanDo, 0, 0, (void *)ptr);
}

long VSTPlugin::getTailSize() 
{ 
	return dispatch(effGetTailSize); 
}

long VSTPlugin::getParameterProperties(long index, VstParameterProperties* ptr) 
{ 
	return dispatch(effGetParameterProperties, index, 0, ptr); 
}

long VSTPlugin::getVstVersion() 
{ 
	return dispatch(effGetVstVersion); 
}

long VSTPlugin::getMidiProgramName(long channel, MidiProgramName* midiProgramName) 
{ 
	return dispatch(effGetMidiProgramName, channel, 0, midiProgramName); 
}

long VSTPlugin::getCurrentMidiProgram (long channel, MidiProgramName* currentProgram) 
{ 
	return dispatch(effGetCurrentMidiProgram, channel, 0, currentProgram); 
}

long VSTPlugin::getMidiProgramCategory (long channel, MidiProgramCategory* category) 
{ 
	return dispatch(effGetMidiProgramCategory, channel, 0, category); 
}

long VSTPlugin::hasMidiProgramsChanged (long channel) 
{ 
	return dispatch(effHasMidiProgramsChanged, channel); 
}

long VSTPlugin::getMidiKeyName(long channel, MidiKeyName* keyName) 
{ 
	return dispatch(effGetMidiKeyName, channel, 0, keyName); 
}

long VSTPlugin::beginSetProgram() 
{ 
	return dispatch(effBeginSetProgram); 
}

long VSTPlugin::endSetProgram() 
{ 
	return dispatch(effEndSetProgram); 
}

long VSTPlugin::getSpeakerArrangement(VstSpeakerArrangement** pluginInput, VstSpeakerArrangement** pluginOutput)
{ 
	return dispatch(effGetSpeakerArrangement, 0, (long)pluginInput, pluginOutput); 
}

long VSTPlugin::setTotalSampleToProcess (long value) 
{ 
	return dispatch(effSetTotalSampleToProcess, 0, value); 
}

long VSTPlugin::getNextShellPlugin(char *name)
{ 
	return dispatch(effShellGetNextPlugin, 0, 0, name); 
}

long VSTPlugin::startProcess() 
{ 
	return dispatch(effStartProcess); 
}

long VSTPlugin::stopProcess() 
{ 
	return dispatch(effStopProcess); 
}

long VSTPlugin::setPanLaw(long type, float val) 
{ 
	return dispatch(effSetPanLaw, 0, type, 0, val); 
}

long VSTPlugin::beginLoadBank(VstPatchChunkInfo* ptr) 
{ 
	return dispatch(effBeginLoadBank, 0, 0, ptr); 
}

long VSTPlugin::beginLoadProgram(VstPatchChunkInfo* ptr) 
{ 
	return dispatch(effBeginLoadProgram, 0, 0, ptr); 
}

long VSTPlugin::setProcessPrecision(long precision) 
{ 
	return dispatch(effSetProcessPrecision, 0, precision, 0); 
}

long VSTPlugin::getNumMidiInputChannels() 
{ 
	return dispatch(effGetNumMidiInputChannels, 0, 0, 0); 
}

long VSTPlugin::getNumMidiOutputChannels() 
{ 
	return dispatch(effGetNumMidiOutputChannels, 0, 0, 0); 
}

//- editor functions -----------------------------------------------

long VSTPlugin::editGetRect(ERect **ptr) 
{ 
	return dispatch(effEditGetRect, 0, 0, ptr); 
}

long VSTPlugin::editOpen(void *ptr) 
{ 
	return dispatch(effEditOpen, 0, 0, ptr); 
}

void VSTPlugin::editClose() 
{ 
	dispatch(effEditClose); 
}

void VSTPlugin::editIdle() 
{ 
	dispatch(effEditIdle); 
}

long VSTPlugin::editkeyDown(VstKeyCode &keyCode) 
{ 
	return dispatch(effEditKeyDown, keyCode.character, keyCode.virt, 0, keyCode.modifier); 
}

long VSTPlugin::editkeyUp(VstKeyCode &keyCode) 
{ 
	return dispatch(effEditKeyUp, keyCode.character, keyCode.virt, 0, keyCode.modifier); 
}

void VSTPlugin::setEditKnobMode(long value) 
{ 
	dispatch(effSetEditKnobMode, 0, value); 
}

//- host callback functions ---------------------------------------------------

//callback func that gets passed to the plugin on loading
//use static host var that points to this instance to xlate this call to class method class

long VSTCALLBACK VSTPlugin::AudioMasterCallback
	(
	AEffect *effect,
	long opcode,
	long index,
	long value,
	void *ptr,
	float opt
	)
{
	if (opcode == audioMasterVersion)		//this is called from <VSTPluginMain> before the user field is set
		return 2400;

	VSTPlugin* plugin = (VSTPlugin*)effect->user;
	return plugin->OnAudioMasterCallback(0, opcode, index, value, ptr, opt);
}

//class method version of above callback
long VSTPlugin::OnAudioMasterCallback
	(
	int nEffect,
	long opcode,
	long index,
	long value,
	void *ptr,
	float opt
	)
{
	return 0;			//to be implemented
}

//-----------------------------------------------------------------------------

void VSTPlugin::enterCritical()
{
	EnterCriticalSection(&cs);
}

void VSTPlugin::leaveCritical()
{
	LeaveCriticalSection(&cs);
}
