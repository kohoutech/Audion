/* ----------------------------------------------------------------------------
Transonic Widget Library
Copyright (C) 1996-2017  George E Greaney

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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Transonic.Widget
{
    public class KeyboardBar : UserControl
    {
        public enum Range {TWENTYFIVE = 0, THIRTYSEVEN, FORTYNINE, SIXTYONE, SEVENTYSIX, EIGHTYEIGHT, FULL }
        public enum KeySize { SMALL = 0, FULL };
        public enum KeyMode { 
            PLAYING = 0,            //playing mode = mouse down/up send to parent window with key's midi num
            SELECTING               //selecting mode = keys are just highlighted, used for defining a note mapping range
        };

        public Color selectedColor = Color.Red;

        IKeyboardWindow window;

        Key[] keys;
        Key[] whitekeys;
        Key[] blackkeys;
        Rectangle keyframe;
        Key mouseKey;                   //key that the mouse has clicked over
        
        int keytop;
        int keyleft;
        int keyright;
        int keybottom;

        int whitekeycount;
        int whitekeywidth;
        int whitekeyheight;

        int blackkeycount;
        int blackkeywidth;
        int blackkeyheight;

        int octavecount;
        int octavewidth;
        int keywidth;
        int barwidth;
        int barheight;
        int keycount;

        Range range;
        KeySize size;
        KeyMode mode;
        int dragstart;        

        //default keyboard bar, use for dropping into form from toolbox
        public KeyboardBar() : this(null, Range.EIGHTYEIGHT, KeyboardBar.KeySize.SMALL)
        {
        }

        public KeyboardBar(IKeyboardWindow _window, Range range, KeySize size)
        {
            window = _window;
            mode = KeyMode.SELECTING;
            InitDimensions(range, size);
            InitializeComponent();
            initKeyboard();

            mouseKey = null;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // KeyboardBar
            // 
            this.BackColor = System.Drawing.Color.White;
            this.DoubleBuffered = true;
            this.Name = "KeyboardBar";
            this.Size = new System.Drawing.Size(650, 200);
            this.ResumeLayout(false);
        }


//- layout --------------------------------------------------------------------

        //key counts = number of white/black keys depending on keyboard range
        //key width/height = size of keys depending on keyboard size
        int[] whiteKeyCounts = { 15, 22, 29, 36, 45, 52, 75 };
        int[] whiteKeyWidths = {12,16};
        int[] whiteKeyHeights = {66,88};

        int[] blackKeyCounts = {10,15,20,25,31,36,53};
        int[] blackKeyWidths = {8,10};
        int[] blackKeyHeights = {41,55};

        //white keys are all adjacent to each other, the blank keys are spaced apart
        int[,] blackKeyOffsets = new int[,] { { 8, 8, 8, 8, 19, 9}, { 10, 10, 10, 10, 25, 13 } };
        int[,] blackKeySpacings = new int[,] { { 13, 22, 13, 13, 23 }, { 18, 29, 18, 18, 29 } };

        //number of octaves, lowest midi note in keyboard depending on keyboard range
        int[] octaveCounts = { 2, 3, 4, 5, 5, 7, 10 };
        int[] midiBases = { 48, 48, 36, 36, 28, 21, 0 };
        int midibase;
        int midimax;
        
        private void InitDimensions(Range _range, KeySize _size)
        {
            range = _range;
            size = _size;

            keytop = 10;                //keyboard margins in widget
            keyleft = 10;
            keyright = 10;
            keybottom = 10;

            whitekeycount = whiteKeyCounts[(int)range];         
            whitekeywidth = whiteKeyWidths[(int)size];
            whitekeyheight = whiteKeyHeights[(int)size];
            
            blackkeycount = blackKeyCounts[(int)range];
            blackkeywidth = blackKeyWidths[(int)size];
            blackkeyheight = blackKeyHeights[(int)size];

            octavecount = octaveCounts[(int)range];
            octavewidth = (whitekeywidth * 7);
            keywidth = whitekeycount * whitekeywidth;
            barwidth = keyleft + keywidth + keyright;
            barheight = keytop + whitekeyheight + keybottom;
            keycount = whitekeycount + blackkeycount;
        }

        private void initKeyboard()
        {
            //doesn't this depend on keyboard size?
            this.ClientSize = new System.Drawing.Size(10 + 576 + 10, 113);
            keyframe = new Rectangle(keyleft, keytop, keywidth, whitekeyheight);

            //midi keys
            keys = new Key[keycount];
            midibase = midiBases[(int)range];           //lowest midi note 
            midimax = midibase + keycount - 1;          //highest midi note
            int midinum = midibase;
            for (int key = 0; key < keycount; key++)
            {
                keys[key] = new Key(midinum++);         //set key's midi num
            }

            //key colors
            whitekeys = new Key[whitekeycount];
            blackkeys = new Key[blackkeycount];

            int keynum = 0;
            int whitekeynum = 0;
            int blackkeynum = 0;

            //sort out white and black keys
            //special cases for 76 and 88 keys
            if (range == Range.SEVENTYSIX)
            {
                whitekeys[whitekeynum++] = keys[keynum++];          //low E
                whitekeys[whitekeynum++] = keys[keynum++];          //low F
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];          //low G
                blackkeys[blackkeynum++] = keys[keynum++];
            }

            if (range == Range.SEVENTYSIX || range == Range.EIGHTYEIGHT)
            {
                whitekeys[whitekeynum++] = keys[keynum++];          //low A
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];          //low B
            }

            for (int oct = 0; oct < octavecount; oct++)
            {
                whitekeys[whitekeynum++] = keys[keynum++];      //C
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];      //D
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];      //E
                whitekeys[whitekeynum++] = keys[keynum++];      //F
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];      //G
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];      //A
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];      //B
            };
            whitekeys[whitekeynum++] = keys[keynum++];      //high C

            //special cases for 76 and 128 keys
            if (range == Range.SEVENTYSIX || range == Range.FULL)
            {
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];          //high D
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];          //high E
                whitekeys[whitekeynum++] = keys[keynum++];          //high F
                blackkeys[blackkeynum++] = keys[keynum++];
                whitekeys[whitekeynum++] = keys[keynum++];          //high G
            }

            //white keys
            Rectangle whitekeyrect = new Rectangle(keyleft, keytop, whitekeywidth, whitekeyheight);
            for (int wk = 0; wk < whitekeycount; wk++)
            {
                whitekeys[wk].setShape(Key.KeyColor.WHITE, whitekeyrect);
                whitekeyrect.Offset(whitekeywidth, 0);
            }

            //black keys
            blackkeynum = 0;
            Rectangle blackkeyrect = new Rectangle(keyleft + blackKeyOffsets[(int)size,(int)range], keytop, blackkeywidth, blackkeyheight);

            //special cases for 76 and 88 keys
            if (range == Range.SEVENTYSIX)
            {
                blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                blackkeyrect.Offset(blackKeySpacings[(int)size, 2], 0);
                blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                blackkeyrect.Offset(blackKeySpacings[(int)size, 3], 0);
            }

            if (range == Range.SEVENTYSIX || range == Range.EIGHTYEIGHT)
            {
                blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                blackkeyrect.Offset(blackKeySpacings[(int)size, 4], 0);
            }

            for (int oct = 0; oct < octavecount; oct++)
            {
                for (int i = 0; i < 5; i++)
                {
                    blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                    blackkeyrect.Offset(blackKeySpacings[(int)size, i], 0);
                }
            }

            //special cases for 76 and 128 keys
            if (range == Range.SEVENTYSIX || range == Range.FULL)
            {
                blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                blackkeyrect.Offset(blackKeySpacings[(int)size, 0], 0);
                blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                blackkeyrect.Offset(blackKeySpacings[(int)size, 1], 0);
                blackkeys[blackkeynum++].setShape(Key.KeyColor.BLACK, blackkeyrect);
                blackkeyrect.Offset(blackKeySpacings[(int)size, 2], 0);
            }
        }

//- i/o -----------------------------------------------------------------------

        //programmatically press & release key(s)
        public void setKeyDown(int midiNum)
        {
            if (midiNum >= midibase && midiNum <= midimax)
            {
                keys[midiNum - midibase].pressed = true;
                Invalidate();
            }
        }

        public void setKeyUp(int midiNum)
        {
            if (midiNum >= midibase && midiNum <= midimax)
            {
                keys[midiNum - midibase].pressed = false;
                Invalidate();
            }
        }

        public void allKeysUp()
        {
            for (int i = 0; i < keycount; i++)
            {
                keys[i].pressed = false;
            }
            Invalidate();
        }

        public void setKeyRange(int loRange, int hiRange)
        {
            allKeysUp();
            for (int midiNum = loRange; midiNum <= hiRange; midiNum++)
            {
                int keyNum = midiNum - midibase;
                if (!((keyNum < 0) || (keyNum >= keycount)))
                {
                    keys[keyNum].pressed = true;
                }
            }
            Invalidate();
        }

        public void setKeyRange(List<int> range)
        {
            allKeysUp();
            foreach (int midiNum in range)
            {
                int keyNum = midiNum - midibase;
                if (!((keyNum < 0) || (keyNum >= keycount)))
                {
                    keys[keyNum].pressed = true;
                }
            }
            Invalidate();
        }

        public List<int> getKeyRange()
        {
            List<int> range = new List<int>();
            for (int i = 0; i < keycount; i++)
            {
                if (keys[i].pressed)
                    range.Add(keys[i].midinum);
            }
            return range;
        }

//- mouse events --------------------------------------------------------------

        //press & release key(s) using the mouse
        private Key hitTest(Point p)
        {
            Key result = null;
            if (keyframe.Contains(p))
            {
                //check black keys
                if (p.Y < (keytop + blackkeyheight))
                {
                    for (int i = 0; i < blackkeycount; i++)
                    {
                        if (blackkeys[i].shape.Contains(p))
                        {
                            result = blackkeys[i];
                            break;
                        }
                    }
                }

                //check white keys
                if (result == null)
                {
                    for (int i = 0; i < whitekeycount; i++)
                    {
                        if (whitekeys[i].shape.Contains(p))
                        {
                            result = whitekeys[i];
                            break;
                        }
                    }
                }
            }
            return result;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouseKey = hitTest(e.Location);         
            if (mouseKey != null)                   //if mouse has clicked on a key
            {
                if (mode == KeyMode.PLAYING)
                {
                    if (!mouseKey.sustain)
                    {
                        mouseKey.pressed = true;
                        window.onKeyPress(mouseKey.midinum);                    //send press to parent window
                        mouseKey.sustain = (e.Button == MouseButtons.Right);
                        Invalidate();
                    }
                    else
                    {
                        mouseKey.sustain = false;
                    }
                }

                //selecting
                else
                {
                    allKeysUp();                                //clicking on keyboard deselected prev range
                    mouseKey.pressed = !mouseKey.pressed;
                    dragstart = mouseKey.midinum;
                    Invalidate();
                }
            }
        }

        //allow user to drag mouse over or off keyboard
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseKey != null)
            {
                Key newkey = hitTest(e.Location);
                if (newkey != mouseKey)                 //if dragged to another key
                {
                    if (mode == KeyMode.PLAYING)
                    {
                        if (!mouseKey.sustain)
                        {
                            mouseKey.pressed = false;                       //we've dragged off the prev key
                            window.onKeyRelease(mouseKey.midinum);          //send release to parent window
                        }
                        mouseKey = newkey;
                        if (mouseKey != null)       //if dragged to another key
                        {
                            if (!mouseKey.sustain)
                            {
                                mouseKey.pressed = true;
                                window.onKeyPress(mouseKey.midinum);                    //send press to parent window
                                mouseKey.sustain = (e.Button == MouseButtons.Right);
                            }
                            else
                            {
                                mouseKey.sustain = false;
                            }
                        }
                        Invalidate();
                    }

                    //selecting
                    else
                    {
                        if (newkey != null)       //if dragged to another key
                        {
                            int nextkeynum;
                            if (dragstart == mouseKey.midinum)           //initial drag
                            {
                                newkey.pressed = !newkey.pressed;       
                                if (newkey.midinum > mouseKey.midinum)
                                {
                                    nextkeynum = (newkey.midinum - 1);  //dragging up
                                }
                                else
                                {
                                    nextkeynum = (newkey.midinum + 1);  //dragging back
                                }
                                if (nextkeynum != mouseKey.midinum)
                                    keys[nextkeynum - midibase].pressed = !keys[nextkeynum - midibase].pressed;
                                mouseKey = newkey;

                            } 
                            else
                            if (dragstart < mouseKey.midinum)           //dragging to the right
                            {
                                if (newkey.midinum > mouseKey.midinum)
                                {
                                    newkey.pressed = !newkey.pressed;       //dragging up
                                    nextkeynum = (newkey.midinum - 1);
                                }
                                else
                                {
                                    mouseKey.pressed = !mouseKey.pressed;   //dragging back
                                    nextkeynum = (newkey.midinum + 1);
                                }
                                if (nextkeynum != mouseKey.midinum)
                                    keys[nextkeynum - midibase].pressed = !keys[nextkeynum - midibase].pressed;
                                mouseKey = newkey;
                            }
                            else
                            {
                                if (newkey.midinum < mouseKey.midinum)      //dragging to the left
                                {
                                    newkey.pressed = !newkey.pressed;       //dragging up
                                    nextkeynum = (newkey.midinum + 1);
                                }
                                else
                                {
                                    mouseKey.pressed = !mouseKey.pressed;   //dragging back
                                    nextkeynum = (newkey.midinum - 1);
                                }
                                if (nextkeynum != mouseKey.midinum)
                                    keys[nextkeynum - midibase].pressed = !keys[nextkeynum - midibase].pressed;
                                mouseKey = newkey;
                            }
                            Invalidate();
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (mouseKey != null)
            {
                if (mode == KeyMode.PLAYING)
                {
                    if (!mouseKey.sustain)
                    {
                        mouseKey.pressed = false;
                        window.onKeyRelease(mouseKey.midinum);          //send release to parent window
                        Invalidate();
                    }
                    mouseKey = null;
                }

                //selecting
                else
                {
                    mouseKey = null;                    
                }
            }
            else
            {
                foreach (Key key in keys)
                {
                    key.pressed = false;        //if user clicks outside keyboard, deselect all keys on mouseup
                }
                Invalidate();
            }
        }

//- key events ----------------------------------------------------------------

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    //if (e.KeyCode == Keys.Delete)
        //}

        //protected override void OnKeyUp(KeyEventArgs e)
        //{
        //    //if (e.KeyCode == Keys.Delete)
        //}
        
//- painting ------------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Brush selectedBrush = new SolidBrush(selectedColor);

            g.DrawRectangle(Pens.Black, keyframe); //keyboard outline
            for (int i = 0; i < whitekeycount; i++)
            {
                g.DrawRectangle(Pens.Black, whitekeys[i].shape);
                g.FillRectangle(whitekeys[i].pressed ? selectedBrush : Brushes.White, whitekeys[i].interior);
                
            }
            for (int i = 0; i < blackkeycount; i++)
            {
                g.FillRectangle(Brushes.Black, blackkeys[i].shape);
                g.FillRectangle(blackkeys[i].pressed ? selectedBrush : Brushes.Black, blackkeys[i].interior);
            }
            selectedBrush.Dispose();
        }
    }

//-----------------------------------------------------------------------------

    public class Key
    {
        public enum KeyColor
        {
            WHITE,
            BLACK
        }

        public int midinum;
        public bool pressed;
        public bool sustain;
        public Rectangle shape;
        public Rectangle interior;
        public KeyColor color;

        public Key(int _midinum)
        {
            midinum = _midinum;
            pressed = false;
            sustain = false;
        }

        public void setShape(KeyColor _color, Rectangle _shape) 
        {
            color = _color;
            shape = _shape;
            interior = new Rectangle(shape.X + 1, shape.Y + 1, shape.Width - 2, shape.Height - 2);
        }
    }

//-----------------------------------------------------------------------------

    public interface IKeyboardWindow
    {
        void onKeyPress(int keyNumber);

        void onKeyRelease(int keyNumber);
    }

}
