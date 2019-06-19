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

#if !defined(VSTPLUGIN_H)
#define VSTPLUGIN_H

#include "vstsdk2.4/audioeffectx.h"

#include <windows.h>                    
#include <stdio.h>                      
#include <math.h>                       

#define MIDIBUFFERSIZE 1000 * 4		//space for 1000 short midi msgs (3 bytes + timestamp)
#define OUTPUTBUFSIZE 22050			//0.5 sec @ 44.1 KHz

typedef struct PluginInfo
{
	char* name;
	char* vendor;
	int version;
	int numPrograms;
	int numParameters;
	int numInputs;
	int numOutputs;
	int flags;
	int uniqueID;
	int editorWidth;
	int editorHeight;
} PlugInfo;

class VSTHost;

class VSTPlugin
{
public:
	VSTPlugin(VSTHost *pHost);
	~VSTPlugin();

	bool load(const char *name);
	void unload();
	void getPlugInfo(PlugInfo* pinfo);

	//processing
	void storeMidiShortMsg(int b1, int b2, int b3);
	void buildMIDIEvents();

	float * getOutputBuffer(int bufIdx);
	void doProcess(long sampleFrames);
	void doProcessReplacing(long sampleFrames);	

	//plugin function calls
	long dispatch(long opCode, long index = 0, long value = 0, void *ptr = NULL, float opt = 0.0f);
	void setParameter(long index, float parameter);
	float getParameter(long index);
	void process(float **inputs, float **outputs, long sampleframes);
	void processReplacing(float **inputs, float **outputs, long sampleframes);
	void processDoubleReplacing(double** inputs, double** outputs, long sampleFrames);

	//plugin AudioEffect dispatcher calls
	void open();
	void close();
	void setProgram(long lValue);
	long getProgram();
	void setProgramName(char *ptr);
	void getProgramName(char *ptr);
	void getParamLabel(long index, char *ptr);
	void getParamDisplay(long index, char *ptr);
	void getParamName(long index, char *ptr);
	void setSampleRate(float fSampleRate);
	void setBlockSize(long value);
	void suspend();
	void resume();
	long getChunk(void **data, bool isPreset);
	long setChunk(void *data, long byteSize, bool isPreset);

	//plugin AudioEffectX dispatcher calls
	long processEvents();
	long canParameterBeAutomated(long index);
	long string2Parameter(long index, char *ptr);
	long getProgramNameIndexed(long category, long index, char* text);
    long getInputProperties(long index, VstPinProperties *ptr);
    long getOutputProperties(long index, VstPinProperties *ptr);
    long getPlugCategory();
    long offlineNotify(VstAudioFile* ptr, long numAudioFiles, bool start);
    long offlinePrepare(VstOfflineTask *ptr, long count);
    long offlineRun(VstOfflineTask *ptr, long count);
    long setSpeakerArrangement(VstSpeakerArrangement* pluginInput, VstSpeakerArrangement* pluginOutput);
    long processVarIo(VstVariableIo* varIo);
    long setBypass(bool onOff);
    long getEffectName(char *ptr);
    long getVendorString(char *ptr);
    long getProductString(char *ptr);
    long getVendorVersion();
    long vendorSpecific(long index, long value, void *ptr, float opt);
    long canDo(const char *ptr);
    long getTailSize();
    long getParameterProperties(long index, VstParameterProperties* ptr);
    long getVstVersion();
    long getMidiProgramName(long channel, MidiProgramName* midiProgramName);
    long getCurrentMidiProgram (long channel, MidiProgramName* currentProgram);
    long getMidiProgramCategory (long channel, MidiProgramCategory* category);
    long hasMidiProgramsChanged (long channel);
    long getMidiKeyName(long channel, MidiKeyName* keyName);
    long beginSetProgram();
    long endSetProgram();
    long getSpeakerArrangement(VstSpeakerArrangement** pluginInput, VstSpeakerArrangement** pluginOutput);
    long setTotalSampleToProcess (long value);
    long getNextShellPlugin(char *name);
    long startProcess();
    long stopProcess();
    long setPanLaw(long type, float val);
    long beginLoadBank(VstPatchChunkInfo* ptr);
    long beginLoadProgram(VstPatchChunkInfo* ptr);
    long setProcessPrecision(long precision);
    long getNumMidiInputChannels();
    long getNumMidiOutputChannels();

	//editor calls
	long editGetRect(ERect **ptr);
	long editOpen(void *ptr);
	void editClose();
	void editIdle();
    long editkeyDown(VstKeyCode &keyCode);
    long editkeyUp(VstKeyCode &keyCode);
    void setEditKnobMode(long value);

	//plugin callback handlers
	static long VSTCALLBACK AudioMasterCallback(AEffect *effect, long opcode, long index, long value, void *ptr, float opt);
	long OnAudioMasterCallback(int nEffect, long opcode, long index, long value, void *ptr, float opt);

	void enterCritical();
	void leaveCritical();

	VstEvents *pEvents;			//midi events

	int inBufCount;
	int outBufCount;
	float** inBufs;
	float** outBufs;

protected:
	VSTHost* pHost;
	AEffect* pEffect;    
	HMODULE hModule;

	CRITICAL_SECTION cs;
	int midiBufCount;
	int midiBuffer[MIDIBUFFERSIZE];
};

#endif // VSTPLUGIN_H
