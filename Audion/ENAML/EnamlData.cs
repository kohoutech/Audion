/* ----------------------------------------------------------------------------
Kohoutech ENAML Library
Copyright (C) 2019-2020  George E Greaney

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

//version 1.0.0 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Kohoutech.ENAML
{
    public class EnamlData
    {
        const int INDENTSIZE = 2;

        ENAMLStem root;
        int indentSize = INDENTSIZE;
        String indentStr = "  ";

        public EnamlData()
        {
            root = new ENAMLStem("root");
        }

        //indent defaults to two spaces, but user can make this wider
        public void setIndentSize(int size)
        {
            if (size != indentSize && size >= INDENTSIZE)
            {
                indentSize = size;
                String s = "";
                for (int i = 0; i < indentSize; i++)
                {
                    s += " ";
                }
                indentStr = s;
            }            
        }

        //---------------------------------------------------------------------
        // READING IN
        //---------------------------------------------------------------------

        //load & parse ENAML data from file into tree of stems & leaves
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
                throw new ENAMLReadException(e.Message);
            }

            int lineNum = 0;
            enaml = new EnamlData();
            enaml.root = enaml.parseSubtree(lines, ref lineNum, "root");
            return enaml;
        }

        char[] wspace = new char[] { ' ' };

        private ENAMLStem parseSubtree(string[] lines, ref int lineNum, string path)
        {
            ENAMLStem curStem = new ENAMLStem(path);
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
                    return curStem;         //and we're done with this level, go back to parent
                }
                else
                {
                    line = line.TrimStart(wspace);                              //we have the indent count, remove the leading spaces
                    int colonpos = line.IndexOf(':');
                    String name = line.Substring(0, colonpos).Trim();
                    String subpath = path + "." + name;
                    if (colonpos + 1 != line.Length)                            //nnn : xxx
                    {
                        String val = line.Substring(colonpos + 1).Trim();
                        curStem.children.Add(name, new ENAMLLeaf(subpath, val));
                    }
                    else
                    {
                        ENAMLStem substem = parseSubtree(lines, ref lineNum, subpath);   //nnn :  - start of a subgroup
                        curStem.children.Add(name, substem);
                    }
                }
            }
            return curStem;
        }

        //- getting values ----------------------------------------------------

        public String getStringValue(String path, String defval)
        {
            if (root != null)
            {
                String strval = findLeafValue(path);
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
                String intstr = findLeafValue(path);
                if (intstr != null)
                {
                    try
                    {
                        int intval = Int32.Parse(intstr);
                        return intval;
                    }
                    catch (Exception e)
                    {
                        throw new ENAMLFormatException("Error reading integer value from ENAML data");
                    }
                }
            }
            return defval;
        }

        public double getFloatValue(String path, double defval)
        {
            if (root != null)
            {
                String floatstr = findLeafValue(path);
                if (floatstr != null)
                {
                    try
                    {
                        double floatval = Double.Parse(floatstr);
                        return floatval;
                    }
                    catch (Exception e)
                    {
                        throw new ENAMLFormatException("Error reading float value from ENAML data");
                    }
                }
            }
            return defval;
        }

        public bool getBoolValue(String path, bool defval)
        {
            if (root != null)
            {
                String strval = findLeafValue(path);
                if (strval != null)
                {
                    if (strval.ToUpper().Equals("TRUE"))
                    {
                        return true;
                    }
                    else if (strval.ToUpper().Equals("FALSE"))
                    {
                        return false;
                    }
                }
            }
            return defval;
        }

        //- path management ---------------------------------------------------

        //check if the path actually exists
        public bool doesPathExist(String path)
        {
            int endpos = path.LastIndexOf('.');
            if (endpos != -1)                                           //if we have a path xxx.yyy.zzz
            {
                string subpath = path.Substring(0, endpos);             //get the xxx.yyy part
                ENAMLStem stem = getSubPath(subpath);                   //and find the stem node for that path
                if (stem != null)
                {
                    string childname = path.Substring(endpos + 1);
                    return (stem.children.ContainsKey(childname));         //then check if it has zzz node (stem or leaf)
                }                
            }
            else
            {
                return root.children.ContainsKey(path);       //if we just have xxx, then check if child of root
            }
            return false;            
        }

        //returns an empty list if the path is invalid or ends in a leaf (ie has no children)
        //that way you can do a foreach loop on the list w/o first having to test for a null return val
        public List<String> getPathKeys(String path)
        {
            List<String> keyList = new List<string>();

            ENAMLStem subpath = getSubPath(path);

            if (subpath != null)
            {
                foreach (string key in subpath.children.Keys)
                {
                    keyList.Add(key);
                }
            }

            return keyList;
        }

        //removes stem & leaf nodes below end of supplied path AND works back up the path 
        //removing all stem nodes that don't have other leaf nodes
        //returns true if path removed, false if path was invalid
        public bool removePath(string path)
        {
            if (!doesPathExist(path))
            {
                return false;
            }

            int leafpos = path.LastIndexOf('.');
            string leafname = path.Substring(leafpos + 1);
            path = path.Substring(0, leafpos);                      //split aaa.bbb.ccc.ddd into aaa.bbb.ccc and ddd
            removeSubPath(root, path, leafname);                    //remove subpath ddd, and any empty node before it
            return true;
        }

        private void removeSubPath(ENAMLStem subpath, string path, string endName)
        {
            if (path.Length == 0)
            {
                if (subpath != null && subpath.children.ContainsKey(endName))
                {
                    subpath.children.Remove(endName);           //from the example above. we're at ccc now, so remove ddd
                }
            }
            else
            {
                int pathpos = path.IndexOf('.');                
                string name = (pathpos != -1) ? path.Substring(0, pathpos) : path;      //split aaa.bbb.ccc into aaa and bbb.ccc
                string rest = (pathpos != -1) ? path.Substring(pathpos + 1) : "";
                ENAMLStem node = (ENAMLStem)subpath.children[name];                     //get node aaa
                removeSubPath(node, rest, endName);                                     //and remove subpath ddd from bbb.ccc (etc...)
                if (node.children.Count == 0)                       //if aaa has no children now
                {
                    subpath.children.Remove(name);                  //then remove it too
                }
            }
        }

        char[] sep = new char[] { '.' };

        //gets stem node at end of a subpath (a path with the ending leaf node removed)
        private ENAMLStem getSubPath(String path)
        {
            string[] parts = path.Split(sep);                   //split xxx.yyy.zzz into (xxx, yyy, zzz)
            ENAMLNode subtree = root;
            for (int i = 0; i < parts.Length; i++)
            {
                string name = parts[i];
                if ((subtree is ENAMLStem) && (((ENAMLStem)subtree).children.ContainsKey(name)))
                {
                    subtree = ((ENAMLStem)subtree).children[name];
                }
                else
                {
                    return null;
                }
            }
            if (subtree is ENAMLStem)               //last node in subpath has to be a stem node
            {
                return (ENAMLStem)subtree;
            }
            return null;
        }

        //gets the string from a leaf node at end of a path
        private String findLeafValue(String path)
        {
            String result = null;

            String leafname = "";
            ENAMLStem subpath = null;

            int leafpos = path.LastIndexOf('.');
            if (leafpos != -1)
            {
                leafname = path.Substring(leafpos + 1);     //split the leaf node name from the end of the path
                path = path.Substring(0, leafpos);
                subpath = getSubPath(path);                 //get stem node that is parent to this leaf node
            }
            else
            {
                leafname = path;
                subpath = root;
            }

            if ((subpath != null) && (subpath.children.ContainsKey(leafname)))
            {
                ENAMLNode leaf = subpath.children[leafname];        //then get the leaf node child
                if (leaf != null && leaf is ENAMLLeaf)              //these should both be true, check anyway
                {
                    result = ((ENAMLLeaf)leaf).value;               //and return it's value
                }
            }

            return result;
        }

        //---------------------------------------------------------------------
        // WRITING OUT
        //---------------------------------------------------------------------

        public void saveToFile(String filename)
        {
            List<String> lines = new List<string>();
            storeSubTree(lines, root, "");
            try
            {
                File.WriteAllLines(filename, lines);
            }
            catch (Exception e)
            {
                throw new ENAMLWriteException(e.Message);
            }            
        }

        private void storeSubTree(List<string> lines, ENAMLStem stem, String indent)
        {
            List<string> childNameList = new List<string>(stem.children.Keys);
            foreach (String childname in childNameList)
            {
                storeNode(lines, stem.children[childname], indent, childname);
            }
        }

        private void storeNode(List<string> lines, ENAMLNode node, String indent, String name)
        {
            String line = indent + name + ":";
            if (node is ENAMLLeaf)
            {
                lines.Add(line + " " + ((ENAMLLeaf)node).value);
            }
            else
            {
                lines.Add(line);
                storeSubTree(lines, (ENAMLStem)node, indent + indentStr);
            }
        }

        //- setting values ----------------------------------------------------

        public void setStringValue(String path, String str)
        {
            setLeafValue(path, root, str);
        }
        
        public void setIntValue(String path, int val)
        {
            String intstr = val.ToString();
            setLeafValue(path, root, intstr);
        }

        public void setFloatValue(String path, double val)
        {
            String floatstr = val.ToString("G17");
            setLeafValue(path, root, floatstr);
        }

        public void setBoolValue(String path, bool val)
        {
            String boolstr = val ? "true" : "false";
            setLeafValue(path, root, boolstr);
        }

        private void setLeafValue(String path, ENAMLStem subtree, String val)
        {
            int dotpos = path.IndexOf('.');
            if (dotpos != -1)                                                           //path is name.subpath
            {
                String name = path.Substring(0, dotpos);
                String subpath = path.Substring(dotpos + 1);
                if (!subtree.children.ContainsKey(name))
                {
                    subtree.children[name] = new ENAMLStem(subtree.fullpath + '.' + name);
                }
                setLeafValue(subpath, (ENAMLStem)subtree.children[name], val);
            }
            else
            {
                if (!subtree.children.ContainsKey(path))
                {
                    subtree.children[path] = new ENAMLLeaf(subtree.fullpath + '.' + path, val);
                }
                else
                {
                    ENAMLNode pathend = subtree.children[path];         //last node in a path has to be a leaf node
                    if (pathend is ENAMLLeaf)
                    {
                        ((ENAMLLeaf)pathend).value = val;               //to (re)store val in
                    }
                    else
                    {
                        throw new ENAMLPathException("path [" + pathend.fullpath + "is not an endpoint");
                    }
                }
            }
        }

        //- internal tree node classes -----------------------------------------------------

        //base class
        private class ENAMLNode
        {
            public string fullpath;

            public ENAMLNode(string path)
            {
                fullpath = path;
            }
        }

        private class ENAMLStem : ENAMLNode
        {
            public Dictionary<string, ENAMLNode> children;

            public ENAMLStem(string path) : base(path)
            {
                children = new Dictionary<string, ENAMLNode>();
            }
        }

        private class ENAMLLeaf : ENAMLNode
        {
            public String value;

            public ENAMLLeaf(string path, String val) : base(path)
            {
                value = val;
            }
        }
    }

    //- exceptions ------------------------------------------------------------

    //base class - if you don't want a more specific detail as to what went wrong
    public class ENAMLException : Exception
    {
        public ENAMLException(String msg)
            : base(msg)
        {
        }
    }

    public class ENAMLReadException : ENAMLException
    {
        public ENAMLReadException(String msg)
            : base(msg)
        {
        }
    }

    public class ENAMLWriteException : ENAMLException
    {
        public ENAMLWriteException(String msg)
            : base(msg)
        {
        }
    }

    public class ENAMLFormatException : ENAMLException
    {
        public ENAMLFormatException(String msg)
            : base(msg)
        {
        }
    }

    public class ENAMLPathException : ENAMLException
    {
        public ENAMLPathException(String msg)
            : base(msg)
        {
        }
    }

}
