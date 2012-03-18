namespace PyDoodle
{
    partial class TweakV2Control
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._xTextBox = new System.Windows.Forms.TextBox();
            this._yTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y";
            // 
            // _xTextBox
            // 
            this._xTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._xTextBox.Location = new System.Drawing.Point(23, 3);
            this._xTextBox.Name = "_xTextBox";
            this._xTextBox.Size = new System.Drawing.Size(124, 20);
            this._xTextBox.TabIndex = 2;
            this._xTextBox.TextChanged += new System.EventHandler(this._xTextBox_TextChanged);
            // 
            // _yTextBox
            // 
            this._yTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._yTextBox.Location = new System.Drawing.Point(23, 29);
            this._yTextBox.Name = "_yTextBox";
            this._yTextBox.Size = new System.Drawing.Size(124, 20);
            this._yTextBox.TabIndex = 3;
            this._yTextBox.TextChanged += new System.EventHandler(this._yTextBox_TextChanged);
            // 
            // TweakV2Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._yTextBox);
            this.Controls.Add(this._xTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "TweakV2Control";
            this.Size = new System.Drawing.Size(150, 56);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _xTextBox;
        private System.Windows.Forms.TextBox _yTextBox;
    }
}
