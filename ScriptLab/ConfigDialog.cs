using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using pyrochild.effects.common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace pyrochild.effects.scriptlab
{
    public partial class ConfigDialog : EffectConfigDialog
    {
        Dictionary<string, IEffectInfo> AvailableEffects = new Dictionary<string, IEffectInfo>();
        List<Image> AvailableEffectsIcons;
        List<string> AvailableEffectsNames;

        List<string> SearchResultEffects = new List<string>();
        Dictionary<int, int> SearchResultIndices = new Dictionary<int, int>();

        List<int> progressBars;

        string lastFilename = "Untitled";
        Point lbScript_LastMousePoint;
        Rectangle lbScript_SelectedItemSwatchRect;
        int _tilesPerEffect;

        public ConfigDialog()
        {
            InitializeComponent();

            progressBars = new List<int>();

            btnDonate.Image = new Bitmap(typeof(ScriptLab), "images.money.png");
            btnChangeColor.Image = new Bitmap(typeof(ScriptLab), "images.colorwheel.png");
            Text = ScriptLab.StaticDialogName;
        }

        protected override void OnLoad(EventArgs e)
        {
            string slName = this.Effect.Name;

            List<IEffectInfo> RawEffects = this.Services
                .GetService<IEffectsService>().EffectInfos
                .Where(x => x.Name != slName)
                .OrderBy(x => x.Name)
                .ToList();

            AvailableEffectsNames = new List<string>();
            AvailableEffectsIcons = new List<Image>();

            for (int i = 0; i < RawEffects.Count; i++)
            {
                if ((RawEffects[i].Category != EffectCategory.DoNotDisplay //effects that don't want to be shown
                    || RawEffects[i].Type.Name == nameof(RotateZoomEffect)) //unless it's the rotatezoomeffect which hides itself from the ffects menu so it can be put in Layers
                    && !AvailableEffects.ContainsKey(RawEffects[i].Type.FullName + ":" + RawEffects[i].Name))
                {
                    AvailableEffects.Add(RawEffects[i].Type.FullName + ":" + RawEffects[i].Name, RawEffects[i]);
                    AvailableEffectsNames.Add(RawEffects[i].Name);
                    AvailableEffectsIcons.Add(RawEffects[i].Image);
                    SearchResultEffects.Add(RawEffects[i].Type.FullName + ":" + RawEffects[i].Name);
                    SearchResultIndices.Add(i, i);
                }
            }

            lbAvailable.Items.AddRange(AvailableEffectsNames.ToArray());
            lbAvailable.Invalidate();

            base.OnLoad(e);
        }

        void lbScript_DrawItem(object sender, DrawItemEventArgs e)
        {           
            if (e.Index >= 0)
            {
                e.DrawBackground();
                e.DrawFocusRectangle();

                ScriptStep step = ((ConfigToken)theEffectToken).effects[e.Index];

                Image icon = step.Icon;
                string text = step.Name;

                if (!step.EffectAvailable) //effect wasn't found
                {
                    e.Graphics.DrawRectangle(Pens.Red, Rectangle.Inflate(e.Bounds, -1, -1));
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Red)), Rectangle.Inflate(e.Bounds, -1, -1));
                    icon = SystemIcons.Warning.ToBitmap();
                    text += " (NOT FOUND)";
                }
                else if (e.Index < progressBars.Count) //draw progress bars
                {
                    int totalbarwidth = 100;
                    int barheight = e.Bounds.Height * 2 / 3;
                    int progressWidth = totalbarwidth * progressBars[e.Index] / _tilesPerEffect;

                    Rectangle progressBarRect = new Rectangle(
                        e.Bounds.Right - e.Bounds.Height - totalbarwidth,
                        e.Bounds.Top + (e.Bounds.Height - barheight) / 2,
                        progressWidth,
                        barheight);

                    e.Graphics.FillRectangle(Brushes.Green, progressBarRect);

                    progressBarRect.Width = totalbarwidth;

                    e.Graphics.DrawRectangle(SystemPens.ButtonShadow, progressBarRect);
                }

                ListBoxDrawIcon(e, icon);
                ListBoxDrawText(e, text);

                ListBoxDrawSwatches(e, step.PrimaryColor, step.SecondaryColor);
            }
        }

        private void ListBoxDrawSwatches(DrawItemEventArgs e, ColorBgra primary, ColorBgra secondary)
        {
            int swatchareasidelength = e.Bounds.Height * 2 / 3;

            Point swatchareapoint = new Point(
                e.Bounds.Right - (e.Bounds.Height + swatchareasidelength) / 2,
                e.Bounds.Top + (e.Bounds.Height - swatchareasidelength) / 2);

            Size swatchareasize = new Size(swatchareasidelength, swatchareasidelength);
            Rectangle swatcharearect = new Rectangle(swatchareapoint, swatchareasize);

            // save the rectangle of the selected item's swatch rect so if the user double clicks we open the colors dialog instead of effect dialog
            if (lbScript.SelectedIndex == e.Index)
            {
                lbScript_SelectedItemSwatchRect = swatcharearect;
            }
            
            Point swatch1point = swatchareapoint;
            Point swatch2point = swatchareapoint;
            swatch2point = Point.Add(swatch2point, new Size((int)(swatchareasidelength / 3f), (int)(swatchareasidelength / 3f)));
            Size swatchsize = new Size((int)(swatchareasidelength * (2 / 3f)), (int)(swatchareasidelength * (2 / 3f)));
            Rectangle swatch1rect = new Rectangle(swatch1point, swatchsize);
            Rectangle swatch2rect = new Rectangle(swatch2point, swatchsize);

            using (Brush secondarybrush = new SolidBrush(secondary.ToColor()))
            using (Brush primarybrush = new SolidBrush(primary.ToColor()))
            {
                e.Graphics.FillRectangle(new SolidBrush(secondary.ToColor()), swatch2rect);
                e.Graphics.DrawRectangle(SystemPens.ButtonShadow, swatch2rect.X, swatch2rect.Y, swatch2rect.Width, swatch2rect.Height);

                e.Graphics.FillRectangle(new SolidBrush(primary.ToColor()), swatch1rect);
                e.Graphics.DrawRectangle(SystemPens.ButtonShadow, swatch1rect.X, swatch1rect.Y, swatch1rect.Width, swatch1rect.Height);
            }
        }

        void lbAvailable_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                e.DrawFocusRectangle();

                int index = SearchResultIndices[e.Index];
                
                Image icon = AvailableEffectsIcons[index];
                string text = AvailableEffectsNames[index];

                ListBoxDrawIcon(e, icon);
                ListBoxDrawText(e, text);
            }
        }

        private void ListBoxDrawText(DrawItemEventArgs e, string text)
        {
            // '&' precedes the underlined alt-character in windows menus. Strip it out. Preserves && -> &
            text = Regex.Replace(text, @"&(.)", "$1");
            
            Point textpoint = new Point(20, e.Bounds.Top + 2);

            using (Brush b = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(text, e.Font, b, textpoint);
        }

        private void ListBoxDrawIcon(DrawItemEventArgs e, Image icon)
        {
            if (icon != null)
            {
                Size iconsize = new Size(Math.Min(16, icon.Width), Math.Min(16, icon.Height));
                Point iconpoint = new Point(2, ((e.Bounds.Height - iconsize.Height) / 2) + e.Bounds.Top);
                Rectangle iconrect = new Rectangle(iconpoint, iconsize);
                e.Graphics.DrawImage(icon, iconrect);
            }
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new ConfigToken();
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            ConfigToken token = (ConfigToken)effectToken;
            theEffectToken = token;
            lbScript.Items.Clear();
            lbScript.Items.AddRange(token.effects.ConvertAll(step => step.Name).ToArray());
        }

        void AddSelectedEffect()
        {
            if (lbAvailable.SelectedItem != null)
            {
                ConfigToken token = (ConfigToken)theEffectToken;
                string effectidentifiername = SearchResultEffects[lbAvailable.SelectedIndex];
                IEffectInfo effectInfo = AvailableEffects[effectidentifiername];
                Effect effect = effectInfo.CreateInstance();
                effect.EnvironmentParameters = Effect.EnvironmentParameters;
                effect.Services = Services;
                if (effect.Options.Flags.HasFlag(EffectFlags.Configurable))
                {
                    try
                    {
                        EffectConfigDialog dialog = effect.CreateConfigDialog();
                        dialog.Effect = effect;

                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            token.effects.Add(new ScriptStep(effectInfo, dialog.EffectToken, Effect.EnvironmentParameters.PrimaryColor, Effect.EnvironmentParameters.SecondaryColor));
                            lbScript.Items.Add(effectInfo.Name);
                            FinishTokenUpdate();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this,
                            "Error occurred in configuration dialog for " + effectInfo.Name + ":\n\n" + e.ToString(),
                            effectInfo.Name+" Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    token.effects.Add(new ScriptStep(effectInfo, null, Effect.EnvironmentParameters.PrimaryColor, Effect.EnvironmentParameters.SecondaryColor));
                    lbScript.Items.Add(effectInfo.Name);
                    FinishTokenUpdate();
                }
            }
        }

        void RemoveSelectedEffect()
        {
            if (lbScript.SelectedItem != null)
            {
                int i = lbScript.SelectedIndex;
                while (lbScript.SelectedItems.Count > 0)
                {
                    i = lbScript.SelectedIndex;
                    lbScript.Items.RemoveAt(i);
                    ((ConfigToken)theEffectToken).effects.RemoveAt(i);
                }
                if (lbScript.Items.Count > 0)
                {
                    if (i < lbScript.Items.Count)
                    {
                        lbScript.SelectedIndices.Add(i);
                    }
                    else
                    {
                        lbScript.SelectedIndices.Add(i - 1);
                    }
                }
                FinishTokenUpdate();
                EnableScriptControlButtons();
            }
        }

        private void lbKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert || e.KeyCode == Keys.Enter)
            {
                AddSelectedEffect();
            }
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedEffect();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lbScript.Items.Clear();
            ((ConfigToken)theEffectToken).effects.Clear();
            FinishTokenUpdate();
            EnableScriptControlButtons();
        }

        private void ChangeSelectedEffectColors()
        {
            if (lbScript.SelectedItem != null)
            {
                ConfigToken token = (ConfigToken)theEffectToken;
                int i = lbScript.SelectedIndex;
                ScriptStep step = token.effects[i];
                using (TwoColorDialog cd = new TwoColorDialog())
                {
                    cd.SetColors(step.PrimaryColor, step.SecondaryColor);
                    if (cd.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach(int index in lbScript.SelectedIndices)
                        {
                            step = token.effects[index];
                            step.PrimaryColor = cd.Color1;
                            step.SecondaryColor = cd.Color2;
                                                    }
                        lbScript.Invalidate();
                        FinishTokenUpdate();
                    }
                }
            }
        }

        private void ChangeSelectedEffect()
        {
            if (lbScript.SelectedItem != null)
            {
                ConfigToken token = (ConfigToken)theEffectToken;
                int i = lbScript.SelectedIndex;
                ScriptStep step = token.effects[i];

                if (step.EffectAvailable)
                {
                    Effect effect = step.CreateInstance();
                    effect.EnvironmentParameters = Effect.EnvironmentParameters;
                    effect.Services = Services;
                    if (effect.Options.Flags.HasFlag(EffectFlags.Configurable))
                    {
                        try
                        {
                            EffectConfigDialog dialog = effect.CreateConfigDialog();
                            dialog.Effect = effect;
                            dialog.EffectToken = (EffectConfigToken)step.Token.Clone();

                            if (dialog.ShowDialog(this) == DialogResult.OK)
                            {
                                step.Token = dialog.EffectToken;
                                FinishTokenUpdate();
                            }
                        }
                        catch (Exception e)
                    {
                        MessageBox.Show(this,
                            "Error occurred in configuration dialog for " + effect.Name + ":\n\n" + e.ToString(),
                            effect.Name+" Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    }
                }
                else
                {
                    MessageBox.Show(this, step.Name + " is not installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Exception exception;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save User Script";
            sfd.DefaultExt = ".sls";
            sfd.Filter = "ScriptLab Scripts|*.sls|Text Files (Save only)|*.txt";
            sfd.OverwritePrompt = true;
            sfd.FileName = lastFilename;
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (sfd.FilterIndex)
                {
                    case 1:
                        if (!SaveSlsFile(sfd.FileName, out exception))
                        {
                            MessageBox.Show("Error saving script:\n\n" + exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        lastFilename = Path.GetFileName(sfd.FileName);
                        break;
                    case 2:
                        if (!SaveTxtFile(sfd.FileName, out exception))
                        {
                            MessageBox.Show("Error saving script:\n\n" + exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            Exception exception;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load User Script";
            ofd.DefaultExt = ".sls";
            ofd.Filter = "ScriptLab Scripts (*.SLS)|*.sls";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!LoadFile(ofd.FileName, out exception))
                {
                    MessageBox.Show("Error loading script:\n\n" + exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                lastFilename = Path.GetFileName(ofd.FileName);
                EnableScriptControlButtons();
            }
        }

        private bool SaveSlsFile(string path, out Exception exception)
        {
            return FileMan.SaveFileBinary(path, ScriptLabScript.FromToken((ConfigToken)theEffectToken), out exception, true);
        }

        private bool SaveTxtFile(string path, out Exception exception)
        {
            exception = null;
            ScriptLabScript sls = ScriptLabScript.FromToken((ConfigToken)theEffectToken);
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0; i < sls.Effects.Count; i++)
                {
                    sw.Write(sls.Names[i]);
                    sw.Write("\t(");
                    sw.Write(sls.Effects[i]);
                    sw.WriteLine(")");
                    sw.Write("\tPrimary:\t#");
                    sw.WriteLine(sls.Colors[i].First.ToHexString());
                    sw.Write("\tSecondary:\t#");
                    sw.WriteLine(sls.Colors[i].Second.ToHexString());
                    sw.WriteLine();

                    string effectname = sls.Effects[i];
                    IEffectInfo effectInfo;
                    if (AvailableEffects.TryGetValue(effectname, out effectInfo))
                    {
                        Effect effect = effectInfo.CreateInstance();
                        effect.EnvironmentParameters = Effect.EnvironmentParameters;
                        effect.Services = Services;
                        if (effect.Options.Flags.HasFlag(EffectFlags.Configurable))
                        {
                            EffectConfigDialog dialog = effect.CreateConfigDialog();
                            EffectConfigToken token = dialog.EffectToken;
                            if (effect is PropertyBasedEffect)
                            {
                                PropertyBasedEffectConfigToken pbect = token as PropertyBasedEffectConfigToken;
                                IEnumerator<Property> enumerator = pbect.Properties.Properties.GetEnumerator();
                                for (int ii = 0; ii < pbect.Properties.Count; ii++)
                                {
                                    enumerator.MoveNext();
                                    sw.Write("\t");
                                    sw.Write(enumerator.Current.Name);
                                    sw.Write(":\t");
                                    sw.WriteLine(sls.Properties[i][ii]);
                                }
                            }
                            else
                            {
                                Type tokentype = token.GetType();
                                PropertyInfo[] pi = tokentype.GetProperties();
                                FieldInfo[] fi = tokentype.GetFields();
                                for (int ii = 0; ii < pi.Length; ii++)
                                {
                                    sw.Write("\t");
                                    sw.Write(pi[ii].Name);
                                    sw.Write(":\t");
                                    sw.WriteLine(sls.Properties[i][ii]);
                                }
                                for (int ii = 0; ii < fi.Length; ii++)
                                {
                                    sw.Write("\t");
                                    sw.Write(fi[ii].Name);
                                    sw.Write(":\t");
                                    sw.WriteLine(sls.Fields[i][ii]);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int ii = 0; ii < sls.Properties.Count; ii++)
                        {
                            sw.Write("\tUnknown Property:\t");
                            sw.WriteLine(sls.Properties[i][ii]);
                        }
                        for (int ii = 0; ii < sls.Fields.Count; ii++)
                        {
                            sw.Write("\tUnknown Field:\t");
                            sw.WriteLine(sls.Fields[i][ii]);
                        }
                    }
                    sw.WriteLine();
                }
                sw.Flush();

                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }

        private bool LoadFile(string filename, out Exception exception)
        {
            exception = null;
            object o;
            if (FileMan.LoadFileBinary(filename, out o, true) || FileMan.LoadFileBinary(filename, out o, false))
            {
                ScriptLabScript sls = o as ScriptLabScript;
                sls.ForceCompatibility();
                ConfigToken token = sls.ToToken(AvailableEffects, Services, EnvironmentParameters.SourceSurface);
                lbScript.Items.Clear();
                ((ConfigToken)theEffectToken).effects.Clear();
                ((ConfigToken)theEffectToken).effects.AddRange(token.effects);
                lbScript.Items.AddRange(token.effects.ConvertAll(step => step.Name).ToArray());
                FinishTokenUpdate();
                return true;
            }
            else
            {
                exception = (Exception)o;
                return false;
            }
        }

        void Cancel_Click(object sender, EventArgs e)
        {
            Effect.SignalCancelRequest();
        }

        private void btnAddEffect_Click(object sender, EventArgs e)
        {
            AddSelectedEffect();
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            ChangeSelectedEffect();
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (lbScript.SelectedItem != null && lbScript.SelectedIndex > 0)
            {
                ConfigToken token = (ConfigToken)theEffectToken;
                int i = lbScript.SelectedIndex;
                object temp;

                temp = lbScript.Items[i];
                lbScript.Items[i] = lbScript.Items[i - 1];
                lbScript.Items[i - 1] = temp;

                temp = token.effects[i];
                token.effects[i] = token.effects[i - 1];
                token.effects[i - 1] = (ScriptStep)temp;

                lbScript.SelectedIndices.Clear();
                lbScript.SelectedIndices.Add(i - 1);

                FinishTokenUpdate();
            }
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (lbScript.SelectedItem != null && lbScript.SelectedIndex + 1 < lbScript.Items.Count)
            {
                ConfigToken token = (ConfigToken)theEffectToken;
                int i = lbScript.SelectedIndex;
                object temp;

                temp = lbScript.Items[i];
                lbScript.Items[i] = lbScript.Items[i + 1];
                lbScript.Items[i + 1] = temp;

                temp = token.effects[i];
                token.effects[i] = token.effects[i + 1];
                token.effects[i + 1] = (ScriptStep)temp;

                lbScript.SelectedIndices.Clear();
                lbScript.SelectedIndices.Add(i + 1);

                FinishTokenUpdate();
            }
        }

        private void btnDeleteEffect_Click(object sender, EventArgs e)
        {
            RemoveSelectedEffect();
        }

        private void lbAvailable_DoubleClick(object sender, EventArgs e)
        {
            AddSelectedEffect();
        }

        private void btnChangeColor_Click(object sender, EventArgs e)
        {
            ChangeSelectedEffectColors();
        }

        private void lbScript_MouseDown(object sender, MouseEventArgs e)
        {
            lbScript_LastMousePoint = e.Location;
            //todo: context menu.
        }

        private void lbScript_DoubleClick(object sender, EventArgs e)
        {
            if (lbScript_SelectedItemSwatchRect.Contains(lbScript_LastMousePoint))
            {
                ChangeSelectedEffectColors();
            }
            else
            {
                ChangeSelectedEffect();
            }
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            Services.GetService<PaintDotNet.AppModel.IShellService>().LaunchUrl(this, "http://forums.getpaint.net/index.php?showtopic=7291");
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            lbAvailable.BeginUpdate();

            if (txtSearch.Text != "search..." && txtSearch.Text != "")
            {
                lbAvailable.Items.Clear();
                SearchResultEffects.Clear();
                SearchResultIndices.Clear();
                IEnumerator<string> keyenumerator = AvailableEffects.Keys.GetEnumerator();
                List<string> resultnames = new List<string>();

                for (int i = 0; i < AvailableEffects.Count; i++)
                {
                    keyenumerator.MoveNext();
                    string name = keyenumerator.Current;
                    if (name.Contains("&"))
                    {
                        name = name.Remove(name.IndexOf("&"), 1);
                    }
                    IEffectInfo effect = AvailableEffects[keyenumerator.Current];
                    if (name.ToLower().Contains(txtSearch.Text.ToLower())
                        || (effect.SubMenuName != null && effect.SubMenuName.ToLower().Contains(txtSearch.Text.ToLower())))
                    {
                        SearchResultEffects.Add(keyenumerator.Current);
                        SearchResultIndices.Add(SearchResultEffects.Count - 1, i);
                        resultnames.Add(effect.Name);
                    }
                }
                lbAvailable.Items.AddRange(resultnames.ToArray());
            }
            else
            {
                lbAvailable.Items.Clear();
                lbAvailable.Items.AddRange(AvailableEffectsNames.ToArray());
                SearchResultEffects.Clear();
                SearchResultIndices.Clear();
                string[] s = new string[AvailableEffects.Count];
                AvailableEffects.Keys.CopyTo(s, 0);
                SearchResultEffects.AddRange(s);
                for (int i = 0; i < SearchResultEffects.Count; i++)
                {
                    SearchResultIndices.Add(i, i);
                }
            }

            lbAvailable.EndUpdate();
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                txtSearch.ForeColor = SystemColors.GrayText;
                txtSearch.Font = new Font(txtSearch.Font, FontStyle.Italic);
                txtSearch.Text = "search...";
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "search...")
            {
                txtSearch.ForeColor = SystemColors.WindowText;
                txtSearch.Font = new Font(txtSearch.Font, FontStyle.Regular);
                txtSearch.Text = "";
            }
        }

        public void SetProgressBarMaximum(int effects, int tilesPerEffect)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new MethodInvoker(() => SetProgressBarMaximum(effects, tilesPerEffect)));
                }
                catch (ObjectDisposedException) { }
            }
            else
            {
                _tilesPerEffect = tilesPerEffect;
                progressBars.AddRange(new int[effects]);
                pbar.Maximum = effects * tilesPerEffect;
                pbar.Visible = true;
            }
        }

        public void ClearProgressBars()
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new MethodInvoker(() => ClearProgressBars()));
                }
                catch (ObjectDisposedException) { }
            }
            else
            {
                progressBars.Clear();
                pbar.Value = 0;
                pbar.Visible = false;
                lbScript.Invalidate();
            }
        }

        public void IncrementProgressBarValue(int effect, int tileCount)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new MethodInvoker(() => IncrementProgressBarValue(effect, tileCount)));
                }
                catch (ObjectDisposedException) { }
            }
            else
            {
                if (tileCount == 1)
                {
                    pbar.Value += _tilesPerEffect;
                    progressBars[effect] += _tilesPerEffect;
                }
                else
                {
                    progressBars[effect]++;
                    pbar.Value++;
                }
                lbScript.Invalidate();
            }
        }

        public void EnableOKButton(bool enable)
        {
            if (InvokeRequired)
            {
                try { Invoke(new MethodInvoker(() => EnableOKButton(enable))); }
                catch (ObjectDisposedException) { }
            }
            else
            {
                buttonOK.Enabled = enable;
            }
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.ForeColor = SystemColors.GrayText;
            txtSearch.Font = new Font(txtSearch.Font, FontStyle.Italic);
            txtSearch.Text = "search...";
        }

        private void lbAvailable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbAvailable.SelectedIndex == -1)
            {
                btnAddEffect.Enabled = false;
            }
            else
            {
                btnAddEffect.Enabled = true;
            }
        }

        private void lbScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableScriptControlButtons();
        }

        private void EnableScriptControlButtons()
        {
            if (lbScript.SelectedIndices.Count == 0)
            {
                btnChange.Enabled = false;
                btnChangeColor.Enabled = false;
                btnMoveUp.Enabled = false;
                btnMoveDown.Enabled = false;
                btnDeleteEffect.Enabled = false;
            }
            else if (lbScript.SelectedIndices.Count == 1)
            {
                btnChange.Enabled = false;
                btnChangeColor.Enabled = true;
                btnMoveUp.Enabled = true;
                btnMoveDown.Enabled = true;
                btnDeleteEffect.Enabled = true;

                //check if the selected effect is configurable
                ConfigToken token = (ConfigToken)theEffectToken;
                int i = lbScript.SelectedIndex;
                ScriptStep step = token.effects[i];

                if (step.EffectAvailable)
                {
                    Effect effect = step.CreateInstance();
                    if (effect.Options.Flags.HasFlag(EffectFlags.Configurable))
                    {
                        btnChange.Enabled = true;
                    }
                }
            }
            else
            {
                btnChange.Enabled = false;
                btnChangeColor.Enabled = true;
                btnMoveUp.Enabled = false;
                btnMoveDown.Enabled = false;
                btnDeleteEffect.Enabled = true;
            }
        }

        private void ConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Effect.SignalCancelRequest();
        }
    }
}