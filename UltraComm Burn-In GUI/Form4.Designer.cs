namespace UltraComm_Burn_In_GUI
{
	partial class Form4
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
			this.label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.combobox_stepfile = new System.Windows.Forms.ComboBox();
			this.label29 = new System.Windows.Forms.Label();
			this.textBox_jobnum = new System.Windows.Forms.TextBox();
			this.label31 = new System.Windows.Forms.Label();
			this.textBox_lotnum = new System.Windows.Forms.TextBox();
			this.label32 = new System.Windows.Forms.Label();
			this.comboBox_part = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(5, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(54, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Step File: ";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(209, 131);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(290, 131);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 5;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// combobox_stepfile
			// 
			this.combobox_stepfile.FormattingEnabled = true;
			this.combobox_stepfile.Location = new System.Drawing.Point(65, 93);
			this.combobox_stepfile.Name = "combobox_stepfile";
			this.combobox_stepfile.Size = new System.Drawing.Size(300, 21);
			this.combobox_stepfile.TabIndex = 6;
			this.combobox_stepfile.SelectedIndexChanged += new System.EventHandler(this.combobox_stepfile_SelectedIndexChanged);
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(291, 23);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(32, 13);
			this.label29.TabIndex = 134;
			this.label29.Text = "Part: ";
			// 
			// textBox_jobnum
			// 
			this.textBox_jobnum.Location = new System.Drawing.Point(193, 41);
			this.textBox_jobnum.Name = "textBox_jobnum";
			this.textBox_jobnum.Size = new System.Drawing.Size(84, 20);
			this.textBox_jobnum.TabIndex = 133;
			this.textBox_jobnum.TextChanged += new System.EventHandler(this.textBox_jobnum_TextChanged);
			// 
			// label31
			// 
			this.label31.AutoSize = true;
			this.label31.Location = new System.Drawing.Point(190, 23);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(52, 13);
			this.label31.TabIndex = 132;
			this.label31.Text = "Job Num:";
			// 
			// textBox_lotnum
			// 
			this.textBox_lotnum.Location = new System.Drawing.Point(66, 41);
			this.textBox_lotnum.Name = "textBox_lotnum";
			this.textBox_lotnum.Size = new System.Drawing.Size(111, 20);
			this.textBox_lotnum.TabIndex = 131;
			this.textBox_lotnum.TextChanged += new System.EventHandler(this.textbox_lotnum_TextChanged);
			// 
			// label32
			// 
			this.label32.AutoSize = true;
			this.label32.Location = new System.Drawing.Point(63, 23);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(50, 13);
			this.label32.TabIndex = 130;
			this.label32.Text = "Lot Num:";
			// 
			// comboBox_part
			// 
			this.comboBox_part.FormattingEnabled = true;
			this.comboBox_part.Location = new System.Drawing.Point(294, 40);
			this.comboBox_part.Name = "comboBox_part";
			this.comboBox_part.Size = new System.Drawing.Size(71, 21);
			this.comboBox_part.TabIndex = 136;
			this.comboBox_part.TextChanged += new System.EventHandler(this.comboBox_part_TextChanged);
			// 
			// Form4
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(399, 183);
			this.Controls.Add(this.comboBox_part);
			this.Controls.Add(this.label29);
			this.Controls.Add(this.textBox_jobnum);
			this.Controls.Add(this.label31);
			this.Controls.Add(this.textBox_lotnum);
			this.Controls.Add(this.label32);
			this.Controls.Add(this.combobox_stepfile);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label2);
			this.Name = "Form4";
			this.Text = "Load Step File";
			this.Load += new System.EventHandler(this.Form4_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ComboBox combobox_stepfile;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.TextBox textBox_jobnum;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.TextBox textBox_lotnum;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.ComboBox comboBox_part;
	}
}