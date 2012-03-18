namespace PyDoodle
{
    partial class TweaksPanel
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
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._leftPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._leftPanel);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            this._splitContainer.Size = new System.Drawing.Size(284, 262);
            this._splitContainer.SplitterDistance = 94;
            this._splitContainer.TabIndex = 0;
            // 
            // _rightPanel
            // 
            this._rightPanel.BackColor = System.Drawing.SystemColors.Control;
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(0, 0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(186, 262);
            this._rightPanel.TabIndex = 0;
            // 
            // _leftPanel
            // 
            this._leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._leftPanel.Location = new System.Drawing.Point(0, 0);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Size = new System.Drawing.Size(94, 262);
            this._leftPanel.TabIndex = 0;
            // 
            // TweaksPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this._splitContainer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TweaksPanel";
            this.Text = "Tweaks";
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.Panel _leftPanel;
        private System.Windows.Forms.Panel _rightPanel;
    }
}