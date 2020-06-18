/* ----------------------------------------------------------------------------
Audion : a audio plugin creator
Copyright (C) 2011-2020  George E Greaney

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

using Kohoutech.VST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Audion.Breadboard;

//Tidepool Model E - a self-contained, small C compiler

//"Wheels within wheels in a spiral array, a pattern so grand and complex"

//"It is advisable to look from the tide pool to the stars and then back to the tide pool again.” - John Steinbeck

namespace Audion.Tidepool
{
    public class TidepoolE
    {
        public VSTPlugin buildVST(AudionPatch patch)
        {
            List<String> plugSource = new List<string>();

            plugSource.Add("void initProtege() {");
            plugSource.Add("protege.sampleRate = SAMPLERATE;");
            plugSource.Add("protege.blockSize = BLOCKSIZE;");
            plugSource.Add("protege.numParams = NPARAMS;");
            plugSource.Add("protege.numPrograms = NPROGS;");
            plugSource.Add("protege.curProgram = 0;");
            plugSource.Add("protege.ID = PlugID;");
            plugSource.Add("protege.version = PlugVer;");
            plugSource.Add("protege.numInputs = NINS;");
            plugSource.Add("protege.numOutputs = NOUTS;");
            plugSource.Add("protege.hasEditor = false;");
            plugSource.Add("protege.canProcessReplacing = true;");
            plugSource.Add("protege.canDoubleReplacing = false;");
            plugSource.Add("protege.programsAreChunks = true;");
            plugSource.Add("protege.isSynth = true;");
            plugSource.Add("protege.noTail = true;");
            plugSource.Add("protege.initialDelay = 0;");
            plugSource.Add("initModule();");
            plugSource.Add("}\n");

            plugSource.Add("BOOL getEffectName(char* name) {");
            plugSource.Add("art_strcpy(name, effectName);");
            plugSource.Add("return true;");
            plugSource.Add("}\n");

            plugSource.Add("BOOL getProductString(char* text) {");
            plugSource.Add("art_strcpy(text, productName);");
            plugSource.Add("return true;");
            plugSource.Add("}\n");

            plugSource.Add("BOOL getVendorString(char* text) {");
            plugSource.Add("art_strcpy(text, vendorName);");
            plugSource.Add("return true;");
            plugSource.Add("}\n");

            plugSource.Add("int getVendorVersion() {");
            plugSource.Add("return PlugVer;");
            plugSource.Add("}\n");

            plugSource.Add("void setSampleRate(float _sampleRate) {");
            plugSource.Add("protege.sampleRate = _sampleRate;");
            plugSource.Add("}\n");

            plugSource.Add("void setBlockSize(int _blockSize) {");
            plugSource.Add("protege.blockSize = _blockSize;");
            plugSource.Add("}\n");

            plugSource.Add("void setParameter(int index, float value) {");
            plugSource.Add("setModuleParam(index, value);");
            plugSource.Add("}\n");

            plugSource.Add("float getParameter(int index) {");
            plugSource.Add("float result = 0.0f;");
            plugSource.Add("result = getModuleParam(index);");
            plugSource.Add("return result;");
            plugSource.Add("}\n");

            plugSource.Add("void getParameterName(int index, char* text) {");
            plugSource.Add("switch (index)");
            plugSource.Add("{");
            plugSource.Add("default:");
            plugSource.Add("art_strcpy(text, \"No Idea \");");
            plugSource.Add("}");
            plugSource.Add("}\n");

            plugSource.Add("void getParameterDisplay(int index, char* text) {");
            plugSource.Add("getModuleParamDisplay(index, text);");
            plugSource.Add("}\n");

            plugSource.Add("void getParameterLabel(int index, char* label)	{");
            plugSource.Add("getModuleParamLabel(index, label);");
            plugSource.Add("}\n");

            plugSource.Add("void process(float** inputs, float** outputs, int count) {");
            plugSource.Add("//float* in1 = inputs[0];");
            plugSource.Add("//float* in2 = inputs[1];");
            plugSource.Add("float * out1 = outputs[0];");
            plugSource.Add("//float* out2 = outputs[1];");
            plugSource.Add("preGen();");
            plugSource.Add("for (int i = 0; i < count; i++)");
            plugSource.Add("{");
            plugSource.Add("out1[i] = generate(0, 0.0f);");
            plugSource.Add("//out2[i] = fProcess(1, in2[i]");
            plugSource.Add("}");
            plugSource.Add("postGen();");
            plugSource.Add("}\n");

            File.WriteAllLines("protege.out.txt", plugSource.ToArray());

            return null;
        }
    }
}
