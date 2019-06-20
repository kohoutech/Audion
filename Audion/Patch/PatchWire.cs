/* ----------------------------------------------------------------------------
Transonic Patch Library
Copyright (C) 1995-2019  George E Greaney

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
using System.Drawing.Drawing2D;

namespace Transonic.Patch
{
    public class PatchWire
    {
        public PatchCanvas canvas;

        public PatchPanel srcPanel;         //connections in view
        public PatchPanel destPanel;
        public Point srcEnd;                //line's endpoints on the panels
        public Point destEnd;

        Color lineColor;                    //for rendering line
        Color selectedColor;
        float lineWidth;
        GraphicsPath path;

        bool isSelected;                    
        
        //for reloading existing connections from a stored patch
        public PatchWire(PatchPanel srcPanel, PatchPanel destPanel)
        {
            canvas = null;
            connectSourceJack(srcPanel);
            connectDestJack(destPanel);

            //default appearance
            lineColor = Color.Black;
            selectedColor = Color.Red;
            lineWidth = 2.0f;

            path = new GraphicsPath();
            updatePath();

            isSelected = false;
        }

//- parameters ----------------------------------------------------------------

        //these can be set by the user
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }
        
//- connections ---------------------------------------------------------------

        public void connectSourceJack(PatchPanel _srcPanel)
        {
            srcPanel = _srcPanel;
            srcEnd = srcPanel.ConnectionPoint;
            srcPanel.connectLine(this);                     //connect line & source panel in view     
        }

        public void connectDestJack(PatchPanel _destPanel)
        {
            destPanel = _destPanel;
            destEnd = destPanel.ConnectionPoint;
            destPanel.connectLine(this);                            //connect line & dest panel in view            
        }

        public void disconnect()
        {
            if (srcPanel != null)
            {
                srcPanel.disconnectLine(this);
                srcPanel = null;
            }

            if (destPanel != null)
            {
                destPanel.disconnectLine(this);
                destPanel = null;                                           //disconnect line from both panels in view
            }
            path = null;
        }

        //these are called if the source / dest panels are moved and the line's path needs to be updated
        public Point SourceEndPos
        {
            set
            {
                srcEnd = value;
                updatePath();
            }
        }

        public Point DestEndPos
        {
            set
            {
                destEnd = value;
                updatePath();
            }
        }

        public void updatePath() 
        {
            path.Reset();
            path.AddLine(srcEnd, destEnd);
        }

//- displaying ----------------------------------------------------------------

        public bool hitTest(Point p)
        {
            Pen linePen = new Pen(lineColor, lineWidth);
            return path.IsOutlineVisible(p, linePen);
        }

        public bool Selected 
        {
            get { return isSelected; }              //don't know if get is needed?
            set { isSelected = value; }
        }

//- user input ----------------------------------------------------------------

        public virtual void onDoubleClick(Point pos)
        {
        }

        public virtual void onRightClick(Point pos)
        {
        }

//- painting ------------------------------------------------------------------

        public void paint(Graphics g)
        {
            using (Pen linePen = new Pen(isSelected ? selectedColor : lineColor, lineWidth))
            {
                g.DrawPath(linePen, path);
            }
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the Wizard");
