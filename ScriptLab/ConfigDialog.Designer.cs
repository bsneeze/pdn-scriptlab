namespace pyrochild.effects.scriptlab
{
    partial class ConfigDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.lblAvailableEffects = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAddEffect = new System.Windows.Forms.Button();
            this.btnDeleteEffect = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnChange = new System.Windows.Forms.Button();
            this.btnChangeColor = new System.Windows.Forms.Button();
            this.pbar = new System.Windows.Forms.ProgressBar();
            this.btnDonate = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnClearSearch = new System.Windows.Forms.Button();
            this.lbScript = new pyrochild.effects.scriptlab.EffectsListBox();
            this.lbAvailable = new pyrochild.effects.scriptlab.EffectsListBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(546, 428);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(466, 428);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // lblAvailableEffects
            // 
            this.lblAvailableEffects.AutoSize = true;
            this.lblAvailableEffects.Location = new System.Drawing.Point(12, 8);
            this.lblAvailableEffects.Name = "lblAvailableEffects";
            this.lblAvailableEffects.Size = new System.Drawing.Size(111, 16);
            this.lblAvailableEffects.TabIndex = 5;
            this.lblAvailableEffects.Text = "Available Effects:";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(362, 2);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(65, 23);
            this.btnLoad.TabIndex = 11;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(546, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(67, 23);
            this.btnClear.TabIndex = 13;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(297, 2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(65, 23);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAddEffect
            // 
            this.btnAddEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddEffect.Enabled = false;
            this.btnAddEffect.Location = new System.Drawing.Point(11, 390);
            this.btnAddEffect.Name = "btnAddEffect";
            this.btnAddEffect.Size = new System.Drawing.Size(282, 25);
            this.btnAddEffect.TabIndex = 17;
            this.btnAddEffect.Text = "Add Effect →";
            this.btnAddEffect.UseVisualStyleBackColor = true;
            this.btnAddEffect.Click += new System.EventHandler(this.btnAddEffect_Click);
            // 
            // btnDeleteEffect
            // 
            this.btnDeleteEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteEffect.Enabled = false;
            this.btnDeleteEffect.Location = new System.Drawing.Point(546, 390);
            this.btnDeleteEffect.Name = "btnDeleteEffect";
            this.btnDeleteEffect.Size = new System.Drawing.Size(67, 25);
            this.btnDeleteEffect.TabIndex = 22;
            this.btnDeleteEffect.Text = "Delete ×";
            this.btnDeleteEffect.UseVisualStyleBackColor = true;
            this.btnDeleteEffect.Click += new System.EventHandler(this.btnDeleteEffect_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveDown.Enabled = false;
            this.btnMoveDown.Location = new System.Drawing.Point(476, 390);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(70, 25);
            this.btnMoveDown.TabIndex = 21;
            this.btnMoveDown.Text = "Down ↓";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveUp.Enabled = false;
            this.btnMoveUp.Location = new System.Drawing.Point(406, 390);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(70, 25);
            this.btnMoveUp.TabIndex = 20;
            this.btnMoveUp.Text = "Up ↑";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnChange
            // 
            this.btnChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChange.Enabled = false;
            this.btnChange.Location = new System.Drawing.Point(297, 390);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(85, 25);
            this.btnChange.TabIndex = 19;
            this.btnChange.Text = "Parameters";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // btnChangeColor
            // 
            this.btnChangeColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeColor.Enabled = false;
            this.btnChangeColor.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnChangeColor.Location = new System.Drawing.Point(382, 390);
            this.btnChangeColor.Name = "btnChangeColor";
            this.btnChangeColor.Size = new System.Drawing.Size(24, 25);
            this.btnChangeColor.TabIndex = 34;
            this.btnChangeColor.UseVisualStyleBackColor = true;
            this.btnChangeColor.Click += new System.EventHandler(this.btnChangeColor_Click);
            // 
            // pbar
            // 
            this.pbar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pbar.Location = new System.Drawing.Point(298, 429);
            this.pbar.Name = "pbar";
            this.pbar.Size = new System.Drawing.Size(162, 21);
            this.pbar.TabIndex = 35;
            this.pbar.Visible = false;
            // 
            // btnDonate
            // 
            this.btnDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDonate.AutoSize = true;
            this.btnDonate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDonate.Location = new System.Drawing.Point(3, 425);
            this.btnDonate.Name = "btnDonate";
            this.btnDonate.Size = new System.Drawing.Size(65, 26);
            this.btnDonate.TabIndex = 37;
            this.btnDonate.Text = "Donate!";
            this.btnDonate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDonate.Click += new System.EventHandler(this.btnDonate_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtSearch.Location = new System.Drawing.Point(167, 4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(105, 20);
            this.txtSearch.TabIndex = 33;
            this.txtSearch.Text = "search...";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.Enter += new System.EventHandler(this.txtSearch_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.txtSearch_Leave);
            // 
            // btnClearSearch
            // 
            this.btnClearSearch.Location = new System.Drawing.Point(273, 3);
            this.btnClearSearch.Name = "btnClearSearch";
            this.btnClearSearch.Size = new System.Drawing.Size(20, 22);
            this.btnClearSearch.TabIndex = 34;
            this.btnClearSearch.Text = "×";
            this.btnClearSearch.UseVisualStyleBackColor = true;
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);
            // 
            // lbScript
            // 
            this.lbScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbScript.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbScript.IntegralHeight = false;
            this.lbScript.ItemHeight = 20;
            this.lbScript.Location = new System.Drawing.Point(298, 26);
            this.lbScript.Name = "lbScript";
            this.lbScript.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbScript.Size = new System.Drawing.Size(314, 363);
            this.lbScript.TabIndex = 36;
            this.lbScript.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbScript_DrawItem);
            this.lbScript.SelectedIndexChanged += new System.EventHandler(this.lbScript_SelectedIndexChanged);
            this.lbScript.DoubleClick += new System.EventHandler(this.lbScript_DoubleClick);
            this.lbScript.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbScript_MouseDown);
            // 
            // lbAvailable
            // 
            this.lbAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbAvailable.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbAvailable.IntegralHeight = false;
            this.lbAvailable.ItemHeight = 20;
            this.lbAvailable.Location = new System.Drawing.Point(12, 26);
            this.lbAvailable.Name = "lbAvailable";
            this.lbAvailable.Size = new System.Drawing.Size(280, 363);
            this.lbAvailable.TabIndex = 37;
            this.lbAvailable.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbAvailable_DrawItem);
            this.lbAvailable.SelectedIndexChanged += new System.EventHandler(this.lbAvailable_SelectedIndexChanged);
            this.lbAvailable.DoubleClick += new System.EventHandler(this.lbAvailable_DoubleClick);
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(624, 454);
            this.Controls.Add(this.btnClearSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.pbar);
            this.Controls.Add(this.btnAddEffect);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lbScript);
            this.Controls.Add(this.btnDonate);
            this.Controls.Add(this.lblAvailableEffects);
            this.Controls.Add(this.lbAvailable);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.btnChange);
            this.Controls.Add(this.btnChangeColor);
            this.Controls.Add(this.btnMoveUp);
            this.Controls.Add(this.btnMoveDown);
            this.Controls.Add(this.btnDeleteEffect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(0, 0);
            this.MinimumSize = new System.Drawing.Size(640, 320);
            this.Name = "ConfigDialog";
            this.Text = "ScriptLab";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigDialog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void LbScript_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label lblAvailableEffects;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnAddEffect;
        private System.Windows.Forms.Button btnChange;
        private System.Windows.Forms.Button btnDeleteEffect;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnDonate;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnChangeColor;
        private System.Windows.Forms.Button btnClearSearch;
        private System.Windows.Forms.ProgressBar pbar;
        private EffectsListBox lbScript;
        private EffectsListBox lbAvailable;

        #endregion
    }
}