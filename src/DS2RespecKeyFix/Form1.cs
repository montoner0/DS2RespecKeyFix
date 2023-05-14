using System;
using System.IO;
using System.Windows.Forms;
using exscape;

namespace DS2RespecKeyFix
{
    public partial class Form1 : Form
    {
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_KEYMENU = 0xF100;

        private readonly HotkeyControl _hkControl1;
        private readonly string _controlsFilePath;
        private readonly string _controlsFileDir;

        public Form1()
        {
            _controlsFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EA Games", "Dead Space 2");
            _controlsFilePath = $"{_controlsFileDir}\\controls.rmp";
            if (!File.Exists(_controlsFilePath)) {
                MessageBox.Show("Dead Space 2 controls settings file is not found.\nRun the game at least once.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            var fi = new FileInfo(_controlsFilePath);
            if (fi.Length != 1008) {
                MessageBox.Show("Dead Space 2 controls settings file is incorrect.\nPlease inform the tool developer about this.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            InitializeComponent();

            _hkControl1 = new HotkeyControl { Location = textBox1.Location, Size = textBox1.Size, TabStop = textBox1.TabStop };
            textBox1.Visible = false;
            _hkControl1.TextChanged += hkControl1_TextChanged;
            Controls.Add(_hkControl1);
            try {
                using (var fs = File.Open(_controlsFilePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    fs.Seek(0x228, SeekOrigin.Begin);
                    _hkControl1.Scancode = (byte)fs.ReadByte();
                }
            } catch (Exception ee) {
                MessageBox.Show($"There was an error during the reading of the Dead Space 2 control settings file:\n\"{ee}\"",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND && (uint)m.WParam == SC_KEYMENU) {
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }

        private void btnCopyDown_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(this.hkControl1.Scancode.ToString());
            try {
                //File.Delete(controlfiledir+"\\controls.rmp.bak");
                if (!File.Exists($"{_controlsFileDir}\\controls.rmp.bak"))
                    File.Copy(_controlsFilePath, $"{_controlsFileDir}\\controls.rmp.bak");

                using (var fs = File.Open(_controlsFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                    fs.Seek(0x228, SeekOrigin.Begin);
                    var info = (byte)_hkControl1.Scancode;
                    fs.WriteByte(info);

                    fs.Seek(0x228, SeekOrigin.Begin);
                    info = (byte)fs.ReadByte();
                    if (info == (byte)_hkControl1.Scancode)
                        MessageBox.Show($"Respec button has successfully been changed to '{_hkControl1.Text}'.",
                                        "Information",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    else
                        MessageBox.Show("There was an error during a writing to the Dead Space 2 controls settings file.",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                }
            } catch (Exception ee) {
                MessageBox.Show($"There was an error during the access to the Dead Space 2 controls settings file:\n\"{ee}\"",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void hkControl1_TextChanged(object sender, EventArgs e)
        {
            btnSave.Enabled = _hkControl1.HotkeyModifiers == Keys.None
                              && _hkControl1.Hotkey != Keys.LWin
                              && _hkControl1.Hotkey != Keys.RWin
                              && _hkControl1.Hotkey != Keys.Apps
                              && _hkControl1.Hotkey != Keys.NumLock
                              && _hkControl1.Hotkey != Keys.Escape
                              && _hkControl1.Hotkey != Keys.Pause
                              && _hkControl1.Hotkey != Keys.Tab;
        }
    }
}