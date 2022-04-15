namespace DiscordFoobarStatus.Configuration
{
    partial class PreferencesPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TopLineFormatTextBox = new System.Windows.Forms.TextBox();
            this.BottomLineFormatTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DisabledCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopLineFormatTextBox
            // 
            this.TopLineFormatTextBox.Location = new System.Drawing.Point(8, 40);
            this.TopLineFormatTextBox.Name = "TopLineFormatTextBox";
            this.TopLineFormatTextBox.Size = new System.Drawing.Size(474, 23);
            this.TopLineFormatTextBox.TabIndex = 0;
            this.TopLineFormatTextBox.TextChanged += new System.EventHandler(this.TopLineFormatTextBox_TextChanged);
            // 
            // BottomLineFormatTextBox
            // 
            this.BottomLineFormatTextBox.Location = new System.Drawing.Point(9, 90);
            this.BottomLineFormatTextBox.Name = "BottomLineFormatTextBox";
            this.BottomLineFormatTextBox.Size = new System.Drawing.Size(473, 23);
            this.BottomLineFormatTextBox.TabIndex = 1;
            this.BottomLineFormatTextBox.TextChanged += new System.EventHandler(this.BottomLineFormatTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Top line format (track info):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(211, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Bottom line format (artist/album info):";
            // 
            // DisabledCheckBox
            // 
            this.DisabledCheckBox.AutoSize = true;
            this.DisabledCheckBox.Location = new System.Drawing.Point(4, 143);
            this.DisabledCheckBox.Name = "DisabledCheckBox";
            this.DisabledCheckBox.Size = new System.Drawing.Size(64, 19);
            this.DisabledCheckBox.TabIndex = 4;
            this.DisabledCheckBox.Text = "Disable";
            this.DisabledCheckBox.UseVisualStyleBackColor = true;
            this.DisabledCheckBox.CheckedChanged += new System.EventHandler(this.DisabledCheckBox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BottomLineFormatTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TopLineFormatTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(489, 133);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Formatting";
            // 
            // PreferencesPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DisabledCheckBox);
            this.Controls.Add(this.groupBox1);
            this.Name = "PreferencesPage";
            this.Size = new System.Drawing.Size(497, 183);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TopLineFormatTextBox;
        private System.Windows.Forms.TextBox BottomLineFormatTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox DisabledCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
