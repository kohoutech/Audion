/* ----------------------------------------------------------------------------
Origami ENAML Library
Copyright (C) 2019  George E Greaney

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

//ENAML - (just what) Everybody Needs - Another Markup Language 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Origami.ENAML
{
    public class EnamlData
    {
        SettingsStem root;

        public EnamlData()
        {
            root = null;
        }

        //- reading in --------------------------------------------------------

        public static EnamlData loadFromFile(String filename)
        {
            EnamlData enaml = null;
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(filename);
            }
            catch (Exception e)
            {
                return enaml;
            }
            enaml = new EnamlData();
            enaml.parseRoot(lines);
            return enaml;
        }

        char[] wspace = new char[] { ' ' };

        //no error checking yet!
        private void parseRoot(string[] lines)
        {
            int lineNum = 0;
            root = parseSubtree(lines, ref lineNum);
        }

        private SettingsStem parseSubtree(string[] lines, ref int lineNum)
        {
            SettingsStem curStem = new SettingsStem();
            int indentLevel = -1;
            while (lineNum < lines.Length)
            {
                String line = lines[lineNum++].TrimEnd(wspace);
                if (line.Length == 0 || line[0] == '#')                 //skip blank lines & comments
                {
                    continue;
                }

                int indent = 0;
                while ((indent < line.Length) && (line[indent] == ' ')) indent++;   //get line indent
                if (indentLevel == -1) indentLevel = indent;                        //if first line of subtree, get indent level

                if (indent < indentLevel)
                {
                    lineNum--;              //this line is not in subgroup so back up one line
                    return curStem;
                }
                else
                {
                    line = line.TrimStart(wspace);                              //we have the indent count, remove the leading spaces
                    int colonpos = line.IndexOf(':');
                    String name = line.Substring(0, colonpos).Trim();
                    if (colonpos + 1 != line.Length)                                //nnn : xxx
                    {
                        String val = line.Substring(colonpos + 1).Trim();
                        curStem.children.Add(name, new SettingsLeaf(val));
                    }
                    else
                    {
                        SettingsStem substem = parseSubtree(lines, ref lineNum);
                        curStem.children.Add(name, substem);
                    }
                }
            }
            return curStem;
        }

        //- getting values ----------------------------------------------------

        private String findLeafValue(String path, SettingsStem subtree)
        {
            String result = null;
            int dotpos = path.IndexOf('.');
            if (dotpos != -1)                                   //path is name.subpath
            {
                String name = path.Substring(0, dotpos);
                String subpath = path.Substring(dotpos + 1);    //break path apart
                if (subtree.children.ContainsKey(name))
                {
                    SettingsNode val = subtree.children[name];
                    if (val != null && val is SettingsStem)
                    {
                        result = findLeafValue(subpath, (SettingsStem)val);
                    }
                }
            }
            else
            {
                if (subtree.children.ContainsKey(path))
                {
                    SettingsNode leaf = subtree.children[path];
                    if (leaf != null && leaf is SettingsLeaf)
                    {
                        result = ((SettingsLeaf)leaf).value;
                    }
                }
            }
            return result;
        }

        public String getStringValue(String path, String defval)
        {
            if (root != null)
            {
                String strval = findLeafValue(path, root);
                if (strval != null)
                {
                    return strval;
                }
            }
            return defval;
        }

        public int getIntValue(String path, int defval)
        {
            if (root != null)
            {
                String intstr = findLeafValue(path, root);
                if (intstr != null)
                {
                    try
                    {
                        int intval = Int32.Parse(intstr);
                        return intval;
                    }
                    catch (Exception e)
                    {                        
                    }
                }
            }
            return defval;
        }

        private void getSubpathKeys(SettingsStem subtree, String path, List<String> keyList)
        {
            if (subtree == null) return;

            int dotpos = path.IndexOf('.');
            if (dotpos != -1)                                   //not at end of path - path is name.subpath
            {
                String name = path.Substring(0, dotpos);
                String subpath = path.Substring(dotpos + 1);              //remove name from start of path
                if (subtree.children.ContainsKey(name))
                {
                    SettingsNode val = subtree.children[name];
                    if (val != null && val is SettingsStem)
                    {
                        getSubpathKeys((SettingsStem)val, subpath, keyList);
                    }
                }
            }
            else
            {
                if (subtree.children.ContainsKey(path))      //at end of path
                {
                    SettingsNode val = subtree.children[path];
                    if (val != null && val is SettingsStem)
                    {
                        foreach (string key in ((SettingsStem)val).children.Keys)
                        {
                            keyList.Add(key);
                        }
                    }
                }
            }            
        }

        //returns an empty list if the path is invalid
        public List<String> getPathKeys(String path)
        {
            List<string> keyList = new List<String>();
            getSubpathKeys(root, path, keyList);
            return keyList;
        }

        //- setting values ----------------------------------------------------

        private void setLeafValue(String path, SettingsStem subtree, String val)
        {
            int dotpos = path.IndexOf('.');
            if (dotpos != -1)                                                           //path is name.subpath
            {
                String name = path.Substring(0, dotpos);
                String subpath = path.Substring(dotpos + 1);
                if (!subtree.children.ContainsKey(name))
                {
                    subtree.children[name] = new SettingsStem();
                }
                setLeafValue(subpath, (SettingsStem)subtree.children[name], val);
            }
            else
            {
                if (!subtree.children.ContainsKey(path))
                {
                    subtree.children[path] = new SettingsLeaf(val);
                }
                else
                {
                    ((SettingsLeaf)subtree.children[path]).value = val;
                }
            }
        }

        public void setStringValue(String path, String str)
        {
            if (root == null)
            {
                root = new SettingsStem();
            }
            setLeafValue(path, root, str);
        }


        public void setIntValue(String path, int val)
        {
            String intstr = val.ToString();
            if (root == null)
            {
                root = new SettingsStem();
            }
            setLeafValue(path, root, intstr);
        }

        //- storing out ------------------------------------------------

        public bool saveToFile(String filename)
        {
            List<String> lines = new List<string>();
            storeSubTree(lines, root, "");
            try
            {
                File.WriteAllLines(filename, lines);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private void storeSubTree(List<string> lines, SettingsStem stem, String indent)
        {
            List<string> childNameList = new List<string>(stem.children.Keys);
            foreach (String childname in childNameList)
            {
                storeNode(lines, stem.children[childname], indent + ((stem != root) ? "  " : ""), childname);
            }
        }

        private void storeNode(List<string> lines, SettingsNode node, String indent, String name)
        {
            String line = indent + name + ":";
            if (node is SettingsLeaf)
            {
                lines.Add(line + " " + ((SettingsLeaf)node).value);
            }
            else
            {
                lines.Add(line);
                storeSubTree(lines, (SettingsStem)node, indent);
            }
        }

        //- internal tree node classes -----------------------------------------------------

        //base class
        private class SettingsNode
        {
        }

        private class SettingsStem : SettingsNode
        {
            public Dictionary<string, SettingsNode> children;

            public SettingsStem()
            {
                children = new Dictionary<string, SettingsNode>();
            }
        }

        private class SettingsLeaf : SettingsNode
        {
            public String value;

            public SettingsLeaf(String val)
            {
                value = val;
            }
        }
    }
}
