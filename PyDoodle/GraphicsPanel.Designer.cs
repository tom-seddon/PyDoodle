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
            this._graphicsControl = new PyDoodle.GraphicsControl();
            this.SuspendLayout();
            // 
            // _graphicsControl
            // 
            this._graphicsControl.BackColor = System.Drawing.Color.White;
            this._graphicsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._graphicsControl.Location = new System.Drawing.Point(0, 0);
            this._graphicsControl.Name = "_graphicsControl";
            this._graphicsControl.Size = new System.Drawing.Size(284, 262);
            this._graphicsControl.TabIndex = 0;
            // 
            // GraphicsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this._graphicsControl);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "GraphicsPanel";
            this.Text = "GraphicsPanel";
            this.ResumeLayout(false);

        }

        #endregion

        private GraphicsControl _graphicsControl;
    }
}