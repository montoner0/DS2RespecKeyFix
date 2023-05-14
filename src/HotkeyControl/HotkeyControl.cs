using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
//
// Hotkey selection control, written by serenity@exscape.org, 2006-08-03
// Please mail me if you find a bug.
//

namespace exscape
{
    /// <summary>
    /// A simple control that allows the user to select pretty much any valid hotkey combination
    /// </summary>
    public class HotkeyControl : TextBox
    {
        // These variables store the current hotkey and modifier(s)
        private Keys _hotkey = Keys.None;
        private Keys _modifiers = Keys.None;
        private uint _scancode = 0;

        // Lists used to enforce the use of proper modifiers.
        // Shift+A isn't a valid hotkey, for instance, as it would screw up when the user is typing.
        private readonly List<Keys> _needNonShiftModifier = null;
        private readonly List<Keys> _needNonAltGrModifier = null;

        private readonly ContextMenu _dummy = new ContextMenu();

        /// <summary>
        /// Used to make sure that there is no right-click menu available
        /// </summary>
        public override ContextMenu ContextMenu
        {
            get => _dummy;
            set => base.ContextMenu = _dummy;
        }

        /// <summary>
        /// Forces the control to be non-multiline
        /// </summary>
        public override bool Multiline
        {
            get => base.Multiline;
            // Ignore what the user wants; force Multiline to false
            set => base.Multiline = false;
        }

        /// <summary>
        /// Creates a new HotkeyControl
        /// </summary>
        public HotkeyControl()
        {
            ContextMenu = _dummy; // Disable right-clicking
            Text = "";

            // Handle events that occurs when keys are pressed
            KeyPress += HotkeyControl_KeyPress;
            KeyUp += HotkeyControl_KeyUp;
            KeyDown += HotkeyControl_KeyDown;
            LostFocus += HotkeyControl_LostFocus;
            GotFocus += HotkeyControl_GotFocus;
//            this.AcceptsTab=true;
//            this.ReadOnly=true;
//            this.BackColor=System.Windows.Forms.Control.DefaultBackColor;
//             this.CanFocus=false;
//             this.CanSelect=false;

            // Fill the Lists that contain all invalid hotkey combinations
            _needNonShiftModifier = new List<Keys>();
            _needNonAltGrModifier = new List<Keys>();
            //PopulateModifierLists();
        }

        /// <summary>
        /// Populates the Lists specifying disallowed hotkeys
        /// such as Shift+A, Ctrl+Alt+4 (would produce a dollar sign) etc
        /// </summary>
        private void PopulateModifierLists()
        {
            // Shift + 0 - 9, A - Z
            for (var k = Keys.D0; k <= Keys.Z; k++)
                _needNonShiftModifier.Add(k);

            // Shift + Numpad keys
            for (var k = Keys.NumPad0; k <= Keys.NumPad9; k++)
                _needNonShiftModifier.Add(k);

            // Shift + Misc (,;<./ etc)
            for (var k = Keys.Oem1; k <= Keys.OemBackslash; k++)
                _needNonShiftModifier.Add(k);

            // Shift + Space, PgUp, PgDn, End, Home
            for (var k = Keys.Space; k <= Keys.Home; k++)
                _needNonShiftModifier.Add(k);

            // Misc keys that we can't loop through
            _needNonShiftModifier.Add(Keys.Insert);
            _needNonShiftModifier.Add(Keys.Help);
            _needNonShiftModifier.Add(Keys.Multiply);
            _needNonShiftModifier.Add(Keys.Add);
            _needNonShiftModifier.Add(Keys.Subtract);
            _needNonShiftModifier.Add(Keys.Divide);
            _needNonShiftModifier.Add(Keys.Decimal);
            _needNonShiftModifier.Add(Keys.Return);
            _needNonShiftModifier.Add(Keys.Escape);
            _needNonShiftModifier.Add(Keys.NumLock);
            _needNonShiftModifier.Add(Keys.Scroll);
            _needNonShiftModifier.Add(Keys.Pause);

            // Ctrl+Alt + 0 - 9
            for (var k = Keys.D0; k <= Keys.D9; k++)
                _needNonAltGrModifier.Add(k);
        }

        /// <summary>
        /// Resets this hotkey control to None
        /// </summary>
        public new void Clear()
        {
            Hotkey = Keys.None;
            HotkeyModifiers = Keys.None;
        }

        const uint MAPVK_VK_TO_VSC = 0;
        const uint MAPVK_VSC_TO_VK = 1;
        const uint MAPVK_VK_TO_CHAR = 2;
        const uint MAPVK_VSC_TO_VK_EX = 3;
        const uint MAPVK_VK_TO_VSC_EX = 4;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

        /// <summary>
        /// Fires when a key is pushed down. Here, we'll want to update the text in the box
        /// to notify the user what combination is currently pressed.
        /// </summary>
        void HotkeyControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Clear the current hotkey
//             if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
//             {
//                 ResetHotkey();
//                 return;
//             }
//             else
            {
                _modifiers = e.Modifiers;
                _hotkey = e.KeyCode;
                _scancode = MapVirtualKey((uint)_hotkey, MAPVK_VK_TO_VSC);

                Redraw();
            }
            e.Handled=true;
        }

        /// <summary>
        /// Fires when all keys are released. If the current hotkey isn't valid, reset it.
        /// Otherwise, do nothing and keep the text and hotkey as it was.
        /// </summary>
        void HotkeyControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (_hotkey == Keys.None && Control.ModifierKeys == Keys.None)
            {
                ResetHotkey();
                return;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Prevents the letter/whatever entered to show up in the TextBox
        /// Without this, a "A" key press would appear as "aControl, Alt + A"
        /// </summary>
        void HotkeyControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        void HotkeyControl_LostFocus(object sender, EventArgs e)
        {
            Redraw(true);
            Select(0, 0);
        }

       void HotkeyControl_GotFocus(object sender, EventArgs e)
       {
          Redraw(true);
          Select(0, 0);
       }

//         protected override void WndProc(ref Message m)
//         {
//             if (m.Msg==0x112) // WM_SYSCOMMAND
//                if (((uint)m.WParam & 0xFFF0) == 0xF100) // SC_KEYMENU
//                   m.Result = IntPtr.Zero;
//
//             base.WndProc(ref m);
//         }

//         protected override bool ProcessKeyMessage(ref Message m)
//         {
//             if (m.Msg==0x0104) // WM_SYSKEYDOWN)
//                if ((Keys)m.WParam == Keys.Menu/* || (Keys)m.WParam == Keys.F10*/) // SC_KEYMENU)        // Process, but do nothing
//                   return true;
//
//            return base.ProcessKeyMessage(ref m);
//         }

        /// <summary>
        /// Handles some misc keys, such as Ctrl+Delete and Shift+Insert
        /// </summary>
//         protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
//         {
//             if (keyData == Keys.Delete || keyData == (Keys.Control | Keys.Delete))
//             {
//                 ResetHotkey();
//                 return true;
//             }
//
//             if (keyData == (Keys.Shift | Keys.Insert)) // Paste
//                 return true; // Don't allow
//
//             // Allow the rest
//             return base.ProcessCmdKey(ref msg, keyData);
//         }

        /// <summary>
        /// Clears the current hotkey and resets the TextBox
        /// </summary>
        public void ResetHotkey()
        {
            _hotkey = Keys.None;
            _modifiers = Keys.None;
            _scancode = 0;
            Redraw();
        }

        /// <summary>
        /// Used to get/set the hotkey (e.g. Keys.A)
        /// </summary>
        public Keys Hotkey
        {
            get => _hotkey;
            set {
                _hotkey = value;
                _scancode = MapVirtualKey((uint)_hotkey, MAPVK_VK_TO_VSC);
                Redraw(true);
            }
        }

        /// <summary>
        /// Used to get/set the modifier keys (e.g. Keys.Alt | Keys.Control)
        /// </summary>
        public Keys HotkeyModifiers
        {
            get => _modifiers;
            set {
                _modifiers = value;
                Redraw(true);
            }
        }

        /// <summary>
        /// Used to get/set the scancode
        /// </summary>
        public uint Scancode
        {
            get => _scancode;
            set {
                _scancode = value;
                _hotkey = (Keys)MapVirtualKey((uint)_scancode, MAPVK_VSC_TO_VK);
                Redraw(true);
            }
        }

        /// <summary>
        /// Helper function
        /// </summary>
        private void Redraw()
        {
            Redraw(false);
        }

        /// <summary>
        /// Redraws the TextBox when necessary.
        /// </summary>
        /// <param name="bCalledProgramatically">Specifies whether this function was called by the Hotkey/HotkeyModifiers properties or by the user.</param>
        private void Redraw(bool bCalledProgramatically)
        {
            // No hotkey set
//             if (this._hotkey == Keys.None)
//             {
//                 this.Text = "None";
//                 return;
//             }

            // LWin/RWin doesn't work as hotkeys (neither do they work as modifier keys in .NET 2.0)
//             if (this._hotkey == Keys.LWin || this._hotkey == Keys.RWin)
//             {
//                 this.Text = "None";
//                 return;
//             }

            // Only validate input if it comes from the user
            if (!bCalledProgramatically)
            {
                // No modifier or shift only, AND a hotkey that needs another modifier
                /*if ((this._modifiers == Keys.Shift || this._modifiers == Keys.None) &&
                this.needNonShiftModifier.Contains(this._hotkey))
                {
                    if (this._modifiers == Keys.None)
                    {
                        // Set Ctrl+Alt as the modifier unless Ctrl+Alt+<key> won't work...
                        if (needNonAltGrModifier.Contains(this._hotkey) == false)
                            this._modifiers = Keys.Alt | Keys.Control;
                        else // ... in that case, use Shift+Alt instead.
                            this._modifiers = Keys.Alt | Keys.Shift;
                    }
                    else
                    {
                        // User pressed Shift and an invalid key (e.g. a letter or a number),
                        // that needs another set of modifier keys
                        this._hotkey = Keys.None;
                        this.Text = this._modifiers.ToString() + " + Invalid key";
                        return;
                    }
                } */
                // Check all Ctrl+Alt keys
                /*if ((this._modifiers == (Keys.Alt | Keys.Control)) &&
                    this.needNonAltGrModifier.Contains(this._hotkey))
                {
                    // Ctrl+Alt+4 etc won't work; reset hotkey and tell the user
                    this._hotkey = Keys.None;
                    this.Text = this._modifiers.ToString() + " + Invalid key";
                    return;
                } */
            }

//             if (this._modifiers == Keys.None)
//             {
//                 if (this._hotkey == Keys.None)
//                 {
//                     this.Text = "None";
//                     return;
//                 }
//                 else
//                 {
//                     // We get here if we've got a hotkey that is valid without a modifier,
//                     // like F1-F12, Media-keys etc.
//                     this.Text = this._hotkey.ToString();
//                     return;
//                 }
//             }

            // I have no idea why this is needed, but it is. Without this code, pressing only Ctrl
            // will show up as "Control + ControlKey", etc.
            if ((_hotkey == Keys.Menu && _modifiers==Keys.Alt) /* Alt */ || (_hotkey == Keys.ShiftKey && _modifiers==Keys.Shift) ||
                (_hotkey == Keys.ControlKey && _modifiers==Keys.Control) )
                _modifiers = Keys.None;

            if ((_hotkey == Keys.Menu /* Alt */ || _hotkey == Keys.ShiftKey || _hotkey == Keys.ControlKey) && _modifiers!=Keys.None )
               _hotkey=Keys.None;

            var converter = new KeysConverter();
            Text = converter.ConvertToString(_hotkey | _modifiers);

//             if (this._hotkey >= Keys.D0 && this._hotkey <= Keys.D9) {
//                this.Text = this.Text.Substring(0, this.Text.Length - 1) + this.Text[this.Text.Length - 1];
//             }

            //this.Text = (this._modifiers != Keys.None ? this._modifiers.ToString() + " + " : "") + this._hotkey.ToString();
        }
    }
}
