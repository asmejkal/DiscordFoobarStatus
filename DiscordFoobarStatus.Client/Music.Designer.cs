namespace DiscordFoobarStatus.Client
{
    partial class Music
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
            this.components = new System.ComponentModel.Container();
            this.CallbackLoopTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // CallbackLoopTimer
            // 
            this.CallbackLoopTimer.Enabled = true;
            this.CallbackLoopTimer.Interval = 200;
            this.CallbackLoopTimer.Tick += new System.EventHandler(this.CallbackLoopTimer_Tick);
            // 
            // Music
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "Music";
            this.Text = "Music";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer CallbackLoopTimer;
    }
}