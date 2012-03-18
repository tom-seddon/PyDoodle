namespace PyDoodle
{
    partial class GraphicsPanel
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
            this._showGridCheckBox = new System.Windows.Forms.CheckBox();
            this._graphicsControl = new PyDoodle.GraphicsControl();
            this.SuspendLayout();
            // 
            // _showGridCheckBox
            // 
            this._showGridCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._showGridCheckBox.AutoSize = true;
            this._showGridCheckBox.Location = new System.Drawing.Point(12, 233);
            this._showGridCheckBox.Name = "_showGridCheckBox";
            this._showGridCheckBox.Size = new System.Drawing.Size(75, 17);
            this._showGridCheckBox.TabIndex = 2;
            this._showGridCheckBox.Text = "Show Grid";
            this._showGridCheckBox.UseVisualStyleBackColor = true;
            this._showGridCheckBox.CheckedChanged += new System.EventHandler(this._showGridCheckBox_CheckedChanged);
            // 
            // _graphicsControl
            // 
            this._graphicsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._graphicsControl.BackColor = System.Drawing.Color.White;
            this._graphicsControl.Location = new System.Drawing.Point(0, 0);
            this._graphicsControl.Name = "_graphicsControl";
            this._graphicsControl.ShowGrid = true;
            this._graphicsControl.Size = new System.Drawing.Size(284, 227);
            this._graphicsControl.TabIndex = 0;
            // 
            // GraphicsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this._showGridCheckBox);
            this.Controls.Add(this._graphicsControl);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "GraphicsPanel";
            this.Text = "GraphicsPanel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GraphicsControl _graphicsControl;
        private System.Windows.Forms.CheckBox _showGridCheckBox;
    }
}