/* ----------------------------------------------------------------------------
Transonic Patch Library
Copyright (C) 1995-2018  George E Greaney

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;

namespace Transonic.Patch
{
    public class PatchPanel
    {
        public enum CONNECTIONTYPE 
        {
            SOURCE,
            DEST,
            NEITHER
        }

        public String panelType;
        public static int panelCount = 0;
        public int panelNum;

        public PatchBox patchbox;
        public List<PatchLine> connectors;
        public CONNECTIONTYPE connType;

        public Rectangle frame;
        public int frameWidth;
        public int frameHeight;

        public PatchPanel(PatchBox box)
        {
            panelType = "PatchPanel";
            panelNum = ++panelCount;
            patchbox = box;

            updateFrame(20, patchbox.frame.Width);

            connectors = new List<PatchLine>();
            connType = CONNECTIONTYPE.NEITHER;
        }

        public void updateFrame(int height, int width)
        {
            frameHeight = height;
            frameWidth = width;
            frame = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        public virtual void setPos(int xOfs, int yOfs)
        {
            frame.Offset(xOfs, yOfs);
            foreach(PatchLine connector in connectors)
            {
                if (connType == CONNECTIONTYPE.SOURCE)
                {
                    connector.SourceEndPos = this.ConnectionPoint;
                }
                if (connType == CONNECTIONTYPE.DEST)
                {
                    connector.DestEndPos = this.ConnectionPoint;
                }
            }
        }

        public virtual bool hitTest(Point p)
        {
            return (frame.Contains(p));
        }

//- connections ---------------------------------------------------------------

        public bool canConnectIn()
        {
            return (connType == CONNECTIONTYPE.DEST);
        }

        public bool canConnectOut()
        {
            return (connType == CONNECTIONTYPE.SOURCE);
        }

        //default connection point - dead center of the frame
        public virtual Point ConnectionPoint
        {
            get { return new Point(frame.Left + frame.Width / 2, frame.Top + frame.Height / 2); }
        }

        public virtual void connectLine(PatchLine line)
        {
            connectors.Add(line);
        }

        public virtual void disconnectLine(PatchLine line)
        {
            connectors.Remove(line);                
        }

        public bool isConnected()
        {
            return (connectors.Count != 0);
        }

        //called on source panel when a patch line connects two panels, so matching connection can be made in the backing model
        public virtual iPatchConnector makeConnection(PatchPanel destPanel)
        {
            return null;
        }

        //called on source panel when two panels are disconnected, so matching connection can be ended in the backing model
        public virtual void breakConnection(PatchPanel destPanel)
        {
        }

//- user input ----------------------------------------------------------------

        public virtual bool canTrackMouse()
        {
            return false;
        }

        public virtual void onMouseDown(Point pos)
        {
        }
        
        public virtual void onMouseMove(Point pos)
        {
        }
        
        public virtual void onMouseUp(Point pos)
        {
        }

        public virtual void onClick(Point pos)
        {
        }

        public virtual void onDoubleClick(Point pos)
        {
        }

        public virtual void onRightClick(Point pos)
        {
        }

//- painting ------------------------------------------------------------------

        public virtual void paint(Graphics g)
        {
            g.DrawRectangle(Pens.Black, frame);
        }

//- persistance ---------------------------------------------------------------

        static Dictionary<String, PatchPanelLoader> panelTypeList = new Dictionary<String, PatchPanelLoader>();

        public static void registerPanelType(String panelName, PatchPanelLoader loader)
        {
            panelTypeList.Add(panelName, loader);
        }

        //loading
        public static PatchPanel loadFromXML(PatchBox box, XmlNode panelNode)
        {
            PatchPanel panel = null;
            String panelName = panelNode.Attributes["paneltype"].Value;
            PatchPanelLoader loader = panelTypeList[panelName];
            if (loader != null)
            {
                panel = loader.loadFromXML(box, panelNode);
            }
            return panel;
        }

        public virtual void loadAttributesFromXML(XmlNode panelNode)
        {
            panelNum = Convert.ToInt32(panelNode.Attributes["number"].Value);            
        }
        
        //saving
        public void saveToXML(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("panel");
            saveAttributesToXML(xmlWriter);
            xmlWriter.WriteEndElement();
        }

        public virtual void saveAttributesToXML(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("paneltype", panelType);
            xmlWriter.WriteAttributeString("number", panelNum.ToString());            
        }
    }

//- panel loader class --------------------------------------------------------

    //subclassed by descendants so correct class will be loaded from XML file
    public class PatchPanelLoader
    {
        public virtual PatchPanel loadFromXML(PatchBox box, XmlNode panelNode)
        {
            PatchPanel panel = new PatchPanel(box);
            panel.loadAttributesFromXML(panelNode);
            return panel;
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
