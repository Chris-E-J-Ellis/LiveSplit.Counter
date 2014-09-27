using Fetze.WinFormsColor;
using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model.Input;
using System.Threading;

namespace LiveSplit.UI.Components
{
    public partial class CounterComponentSettings : UserControl
    {
        public CounterComponentSettings()
        {
            InitializeComponent();

            this.Hook = new CompositeHook();

            // Set default values.
            GlobalHotkeysEnabled = false;
            CounterFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            OverrideCounterFont = false;
            CounterTextColor = Color.FromArgb(255, 255, 255, 255);
            CounterValueColor = Color.FromArgb(255, 255, 255, 255);
            OverrideTextColor = false;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            CounterText = "Counter:";
            InitialValue = 0;
            Increment = 1;

            // Hotkeys
            IncrementKey = new KeyOrButton(Keys.Add);
            DecrementKey = new KeyOrButton(Keys.Subtract);
            ResetKey = new KeyOrButton(Keys.NumPad0);

            // Set bindings.
            txtIncrement.DataBindings.Add("Text", this, "IncrementKey", false, DataSourceUpdateMode.OnPropertyChanged);
            txtDecrement.DataBindings.Add("Text", this, "DecrementKey", false, DataSourceUpdateMode.OnPropertyChanged);
            txtReset.DataBindings.Add("Text", this, "ResetKey", false, DataSourceUpdateMode.OnPropertyChanged);
            txtCounterText.DataBindings.Add("Text", this, "CounterText");
            numInitialValue.DataBindings.Add("Value", this, "InitialValue");
            numIncrement.DataBindings.Add("Value", this, "Increment");
            chkGlobalHotKeys.DataBindings.Add("Checked", this, "GlobalHotkeysEnabled", false, DataSourceUpdateMode.OnPropertyChanged);
            chkFont.DataBindings.Add("Checked", this, "OverrideCounterFont", false, DataSourceUpdateMode.OnPropertyChanged);
            lblFont.DataBindings.Add("Text", this, "CounterFontString", false, DataSourceUpdateMode.OnPropertyChanged);
            chkColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor.DataBindings.Add("BackColor", this, "CounterTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor3.DataBindings.Add("BackColor", this, "CounterValueColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);

            // Assign event handlers.
            cmbGradientType.SelectedIndexChanged += cmbGradientType_SelectedIndexChanged;
            chkFont.CheckedChanged += chkFont_CheckedChanged;
            chkColor.CheckedChanged += chkColor_CheckedChanged;
            chkGlobalHotKeys.CheckedChanged += chkGlobalHotKeys_CheckedChanged;

            this.Load += CounterSettings_Load;

            RegisterHotKeys();

            // Launch Input Polling task. Main LiveSplit Hook doesn't seem to be visible.
            // Nothing fancy, if anything goes wrong, we'll just lose gamepad input.
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    try
                    {
                        this.Hook.Poll();
                    }
                    catch (Exception) { break; }
                }
            });
        }

        public CompositeHook Hook { get; set; }

        public bool GlobalHotkeysEnabled { get; set; }

        public Color CounterTextColor { get; set; }
        public Color CounterValueColor { get; set; }
        public bool OverrideTextColor { get; set; }

        public string CounterFontString { get { return String.Format("{0} {1}", CounterFont.FontFamily.Name, CounterFont.Style); } }
        public Font CounterFont { get; set; }
        public bool OverrideCounterFont { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public String GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public string CounterText { get; set; }
        public int InitialValue { get; set; }
        public int Increment { get; set; }

        public KeyOrButton IncrementKey { get; set; }
        public KeyOrButton DecrementKey { get; set; }
        public KeyOrButton ResetKey { get; set; }
        
        public event EventHandler CounterReinitialiseRequired = delegate { };
        
        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version;
            if (element["Version"] != null)
                version = Version.Parse(element["Version"].InnerText);
            else
                version = new Version(1, 0);

            if (version >= new Version(1, 0))
            {
                GlobalHotkeysEnabled = Boolean.Parse(element["GlobalHotkeysEnabled"].InnerText);
                CounterTextColor = ParseColor(element["CounterTextColor"]);
                CounterValueColor = ParseColor(element["CounterValueColor"]);
                CounterFont = GetFontFromElement(element["CounterFont"]);
                OverrideCounterFont = Boolean.Parse(element["OverrideCounterFont"].InnerText);
                OverrideTextColor = Boolean.Parse(element["OverrideTextColor"].InnerText);
                BackgroundColor = ParseColor(element["BackgroundColor"]);
                BackgroundColor2 = ParseColor(element["BackgroundColor2"]);
                GradientString = element["BackgroundGradient"].InnerText;
                CounterText = element["CounterText"].InnerText;
                InitialValue = Int32.Parse(element["InitialValue"].InnerText);
                Increment = Int32.Parse(element["Increment"].InnerText);

                XmlElement incrementElement = element["IncrementKey"];
                IncrementKey = string.IsNullOrEmpty(incrementElement.InnerText) ? null : new KeyOrButton(incrementElement.InnerText);
                XmlElement decrementElement = element["DecrementKey"];
                DecrementKey = string.IsNullOrEmpty(decrementElement.InnerText) ? null : new KeyOrButton(decrementElement.InnerText);
                XmlElement resetElement = element["ResetKey"];
                ResetKey = string.IsNullOrEmpty(resetElement.InnerText) ? null : new KeyOrButton(resetElement.InnerText);
            }
            else
            {
                GlobalHotkeysEnabled = false;
                CounterTextColor = Color.FromArgb(255, 255, 255, 255);
                CounterValueColor = Color.FromArgb(255, 255, 255, 255);
                CounterFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
                OverrideCounterFont = false;
                OverrideTextColor = false;
                BackgroundColor = Color.Transparent;
                BackgroundColor2 = Color.Transparent;
                BackgroundGradient = GradientType.Plain;
                IncrementKey = new KeyOrButton(Keys.Add);
                DecrementKey = new KeyOrButton(Keys.Subtract);
                ResetKey = new KeyOrButton(Keys.NumPad0);
                CounterText = "Counter: ";
                InitialValue = 0;
                Increment = 1;
            }

            RegisterHotKeys();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(ToElement(document, "Version", "1.0"));
            parent.AppendChild(ToElement(document, "GlobalHotkeysEnabled", GlobalHotkeysEnabled));
            parent.AppendChild(ToElement(document, "OverrideCounterFont", OverrideCounterFont));
            parent.AppendChild(ToElement(document, "OverrideTextColor", OverrideTextColor));
            parent.AppendChild(CreateFontElement(document, "CounterFont", CounterFont));
            parent.AppendChild(ToElement(document, CounterTextColor, "CounterTextColor"));
            parent.AppendChild(ToElement(document, CounterValueColor, "CounterValueColor"));
            parent.AppendChild(ToElement(document, BackgroundColor, "BackgroundColor"));
            parent.AppendChild(ToElement(document, BackgroundColor2, "BackgroundColor2"));
            parent.AppendChild(ToElement(document, "BackgroundGradient", BackgroundGradient));
            parent.AppendChild(ToElement(document, "CounterText", CounterText));
            parent.AppendChild(ToElement(document, "InitialValue", InitialValue));
            parent.AppendChild(ToElement(document, "Increment", Increment));
            parent.AppendChild(ToElement(document, "IncrementKey", IncrementKey));
            parent.AppendChild(ToElement(document, "DecrementKey", DecrementKey));
            parent.AppendChild(ToElement(document, "ResetKey", ResetKey));

            return parent;
        }

        private Font ChooseFont(Font previousFont, int minSize, int maxSize)
        {
            var dialog = new FontDialog();
            dialog.Font = previousFont;
            dialog.MinSize = minSize;
            dialog.MaxSize = maxSize;

            try
            {
                var result = dialog.ShowDialog(this);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.Font;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                MessageBox.Show("This font is not supported.", "Font Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return previousFont;
        }

        private Font GetFontFromElement(XmlElement element)
        {
            if (!element.IsEmpty)
            {
                var bf = new BinaryFormatter();

                var base64String = element.InnerText;
                var data = Convert.FromBase64String(base64String);
                var ms = new MemoryStream(data);
                return (Font)bf.Deserialize(ms);
            }

            return null;
        }

        private XmlElement CreateFontElement(XmlDocument document, String elementName, Font font)
        {
            var element = document.CreateElement(elementName);

            if (font != null)
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();

                    bf.Serialize(ms, font);
                    var data = ms.ToArray();
                    var cdata = document.CreateCDataSection(Convert.ToBase64String(data));
                    element.InnerXml = cdata.OuterXml;
                }
            }

            return element;
        }
        private Color ParseColor(XmlElement colorElement)
        {
            return Color.FromArgb(Int32.Parse(colorElement.InnerText, NumberStyles.HexNumber));
        }

        private XmlElement ToElement(XmlDocument document, Color color, string name)
        {
            var element = document.CreateElement(name);
            element.InnerText = color.ToArgb().ToString("X8");
            return element;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }

        // Behaviour essentially Lifted from LiveSplit Settings.
        private void SetHotkeyHandlers(TextBox txtBox, Action<KeyOrButton> keySetCallback)
        {
            string oldText = txtBox.Text;
            txtBox.Text = "Set Hotkey...";
            txtBox.Select(0, 0);

            KeyEventHandler handlerDown = null;
            KeyEventHandler handlerUp = null;
            EventHandler leaveHandler = null;
            EventHandlerT<GamepadButton> gamepadButtonPressed = null;

            // Remove Input handlers.
            Action unregisterEvents = (Action)(() =>
            {
                txtBox.KeyDown -= handlerDown;
                txtBox.KeyUp -= handlerUp;
                txtBox.Leave -= leaveHandler;
                this.Hook.AnyGamepadButtonPressed -= gamepadButtonPressed;
            });

            // Handler for KeyDown
            handlerDown = (s, x) =>
            {
                KeyOrButton keyOrButton = x.KeyCode == Keys.Escape ? null : new KeyOrButton(x.KeyCode | x.Modifiers);

                // No action for special keys.
                if (x.KeyCode == Keys.ControlKey || x.KeyCode == Keys.ShiftKey || x.KeyCode == Keys.Menu)
                    return;

                keySetCallback(keyOrButton);
                unregisterEvents();

                // Remove Focus.
                txtBox.Select(0, 0);
                this.chkGlobalHotKeys.Select();

                txtBox.Text = this.FormatKey(keyOrButton);

                // Re-Register inputs.
                RegisterHotKeys();
            };

            // Handler for KeyUp (allows setting of special keys, shift, ctrl etc.).
            handlerUp = (s, x) =>
            {
                KeyOrButton keyOrButton = x.KeyCode == Keys.Escape ? null : new KeyOrButton(x.KeyCode | x.Modifiers);

                // No action for normal keys.
                if (x.KeyCode != Keys.ControlKey && x.KeyCode != Keys.ShiftKey && x.KeyCode != Keys.Menu)
                    return;

                keySetCallback(keyOrButton);
                unregisterEvents();
                txtBox.Select(0, 0);
                this.chkGlobalHotKeys.Select();
                txtBox.Text = this.FormatKey(keyOrButton);
                RegisterHotKeys();
            };

            leaveHandler = (s, x) =>
            {
                unregisterEvents();
                txtBox.Text = oldText;
            };

            // Handler for gamepad/joystick inputs.
            gamepadButtonPressed = (s, x) =>
            {
                KeyOrButton key = new KeyOrButton(x);
                keySetCallback(key);
                unregisterEvents();

                Action keyOrButton = () =>
                {
                    txtBox.Select(0, 0);
                    this.chkGlobalHotKeys.Select();
                    txtBox.Text = this.FormatKey(key);
                    RegisterHotKeys();
                };

                // May not be in the UI thread (likely).
                if (this.InvokeRequired)
                    this.Invoke(keyOrButton);
                else
                    keyOrButton();
            };

            txtBox.KeyDown += handlerDown;
            txtBox.KeyUp += handlerUp;
            txtBox.Leave += leaveHandler;

            this.Hook.AnyGamepadButtonPressed += gamepadButtonPressed;
        }

        /// <summary>
        /// Registers the hot keys (unregisters existing Hotkeys).
        /// </summary>
        private void RegisterHotKeys()
        {
            try
            {
                this.UnregisterAllHotkeys(Hook);

                Hook.RegisterHotKey(this.IncrementKey);
                Hook.RegisterHotKey(this.DecrementKey);
                Hook.RegisterHotKey(this.ResetKey);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Unregisters all hotkeys.
        /// </summary>
        public void UnregisterAllHotkeys(CompositeHook hook)
        {
            hook.UnregisterAllHotkeys();
            HotkeyHook.Instance.UnregisterAllHotkeys();
        }

        private string FormatKey(KeyOrButton key)
        {
            if (!(key != (KeyOrButton)null))
                return "None";
            string str = key.ToString();
            if (key.IsButton)
            {
                int length = str.LastIndexOf(' ');
                if (length != -1)
                    str = str.Substring(0, length);
            }
            return str;
        }

        private void CounterSettings_Load(object sender, EventArgs e)
        {
            chkColor_CheckedChanged(null, null);
            chkFont_CheckedChanged(null, null);
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var picker = new ColorPickerDialog();
            picker.SelectedColor = picker.OldColor = button.BackColor;
            picker.SelectedColorChanged += (s, x) => button.BackColor = picker.SelectedColor;
            picker.ShowDialog(this);
            button.BackColor = picker.SelectedColor;
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            CounterFont = ChooseFont(CounterFont, 7, 20);
            lblFont.Text = CounterFontString;
        }

        private void chkColor_CheckedChanged(object sender, EventArgs e)
        {
            label3.Enabled = btnColor.Enabled = label5.Enabled = btnColor3.Enabled = chkColor.Checked;
        }

        void chkFont_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = lblFont.Enabled = btnFont.Enabled = chkFont.Checked;
        }
        void chkGlobalHotKeys_CheckedChanged(object sender, EventArgs e)
        {
            GlobalHotkeysEnabled = chkGlobalHotKeys.Checked;
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        private void txtIncrement_Enter(object sender, EventArgs e)
        {
            this.SetHotkeyHandlers((TextBox)sender, (Action<KeyOrButton>)(x => this.IncrementKey = x));
        }

        private void txtIncrement_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        private void txtDecrement_Enter(object sender, EventArgs e)
        {
            this.SetHotkeyHandlers((TextBox)sender, (Action<KeyOrButton>)(x => this.DecrementKey = x));
        }

        private void txtDecrement_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        private void txtReset_Enter(object sender, EventArgs e)
        {
            this.SetHotkeyHandlers((TextBox)sender, (Action<KeyOrButton>)(x => this.ResetKey = x));
        }

        private void txtReset_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        private void numInitialValue_ValueChanged(object sender, EventArgs e)
        {
            this.InitialValue = (int)Math.Round(numInitialValue.Value, 0);
            CounterReinitialiseRequired(this, EventArgs.Empty);
        }
    }
}
