
namespace Fougere
{
    partial class CompareGUI
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
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.openButton1 = new System.Windows.Forms.Button();
            this.filePathTextBox1 = new System.Windows.Forms.TextBox();
            this.filePathTextBox2 = new System.Windows.Forms.TextBox();
            this.openButton2 = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // outputTextBox
            // 
            this.outputTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.outputTextBox.ForeColor = System.Drawing.Color.White;
            this.outputTextBox.Location = new System.Drawing.Point(12, 95);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(652, 366);
            this.outputTextBox.TabIndex = 0;
            // 
            // openButton1
            // 
            this.openButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openButton1.ForeColor = System.Drawing.Color.White;
            this.openButton1.Location = new System.Drawing.Point(508, 22);
            this.openButton1.Name = "openButton1";
            this.openButton1.Size = new System.Drawing.Size(75, 23);
            this.openButton1.TabIndex = 1;
            this.openButton1.Text = "Open";
            this.openButton1.UseVisualStyleBackColor = true;
            this.openButton1.Click += new System.EventHandler(this.OpenButton1_Click);
            // 
            // filePathTextBox1
            // 
            this.filePathTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.filePathTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.filePathTextBox1.ForeColor = System.Drawing.Color.White;
            this.filePathTextBox1.Location = new System.Drawing.Point(50, 25);
            this.filePathTextBox1.Name = "filePathTextBox1";
            this.filePathTextBox1.ReadOnly = true;
            this.filePathTextBox1.Size = new System.Drawing.Size(452, 20);
            this.filePathTextBox1.TabIndex = 3;
            // 
            // filePathTextBox2
            // 
            this.filePathTextBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.filePathTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.filePathTextBox2.ForeColor = System.Drawing.Color.White;
            this.filePathTextBox2.Location = new System.Drawing.Point(50, 60);
            this.filePathTextBox2.Name = "filePathTextBox2";
            this.filePathTextBox2.ReadOnly = true;
            this.filePathTextBox2.Size = new System.Drawing.Size(452, 20);
            this.filePathTextBox2.TabIndex = 5;
            // 
            // openButton2
            // 
            this.openButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openButton2.ForeColor = System.Drawing.Color.White;
            this.openButton2.Location = new System.Drawing.Point(508, 57);
            this.openButton2.Name = "openButton2";
            this.openButton2.Size = new System.Drawing.Size(75, 23);
            this.openButton2.TabIndex = 4;
            this.openButton2.Text = "Open";
            this.openButton2.UseVisualStyleBackColor = true;
            this.openButton2.Click += new System.EventHandler(this.OpenButton2_Click);
            // 
            // runButton
            // 
            this.runButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.runButton.ForeColor = System.Drawing.Color.White;
            this.runButton.Location = new System.Drawing.Point(589, 57);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 23);
            this.runButton.TabIndex = 6;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "File 1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "File 2";
            // 
            // CompareGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.ClientSize = new System.Drawing.Size(679, 476);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.filePathTextBox2);
            this.Controls.Add(this.openButton2);
            this.Controls.Add(this.filePathTextBox1);
            this.Controls.Add(this.openButton1);
            this.Controls.Add(this.outputTextBox);
            this.Name = "CompareGUI";
            this.Text = "CompareGUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button openButton1;
        private System.Windows.Forms.TextBox filePathTextBox1;
        private System.Windows.Forms.TextBox filePathTextBox2;
        private System.Windows.Forms.Button openButton2;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}