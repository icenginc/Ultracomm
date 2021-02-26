namespace UltraComm_Burn_In_GUI
{
	partial class Form3
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textbox_lotreports = new System.Windows.Forms.TextBox();
			this.textbox_temp_reports = new System.Windows.Forms.TextBox();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.folderBrowserDialog2 = new System.Windows.Forms.FolderBrowserDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.textbox_lot_days = new System.Windows.Forms.TextBox();
			this.textbox_lot_hrs = new System.Windows.Forms.TextBox();
			this.textbox_lot_mins = new System.Windows.Forms.TextBox();
			this.textbox_temp_days = new System.Windows.Forms.TextBox();
			this.textbox_temp_hrs = new System.Windows.Forms.TextBox();
			this.textbox_temp_mins = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.textbox_set_temp = new System.Windows.Forms.TextBox();
			this.textbox_high_temp = new System.Windows.Forms.TextBox();
			this.textbox_max_temp = new System.Windows.Forms.TextBox();
			this.textbox_low_temp = new System.Windows.Forms.TextBox();
			this.textbox_min_temp = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.combobox_chamber_select = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(31, 183);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "- Lot Report Files:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(31, 202);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(102, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "- Temp Report Files:";
			// 
			// textbox_lotreports
			// 
			this.textbox_lotreports.Location = new System.Drawing.Point(155, 183);
			this.textbox_lotreports.Name = "textbox_lotreports";
			this.textbox_lotreports.Size = new System.Drawing.Size(215, 20);
			this.textbox_lotreports.TabIndex = 2;
			this.textbox_lotreports.TextChanged += new System.EventHandler(this.textbox_lotreports_TextChanged);
			// 
			// textbox_temp_reports
			// 
			this.textbox_temp_reports.Location = new System.Drawing.Point(155, 202);
			this.textbox_temp_reports.Name = "textbox_temp_reports";
			this.textbox_temp_reports.Size = new System.Drawing.Size(215, 20);
			this.textbox_temp_reports.TabIndex = 3;
			this.textbox_temp_reports.TextChanged += new System.EventHandler(this.textbox_temp_reports_TextChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(375, 182);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 21);
			this.button1.TabIndex = 4;
			this.button1.Text = "...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(375, 202);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(24, 21);
			this.button2.TabIndex = 5;
			this.button2.Text = "...";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(26, 162);
			this.label3.Name = "label3";
			this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label3.Size = new System.Drawing.Size(75, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "File Locations:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(26, 83);
			this.label4.Name = "label4";
			this.label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label4.Size = new System.Drawing.Size(91, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Logging Intervals:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(31, 104);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "- Lot Logging:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(31, 123);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(84, 13);
			this.label6.TabIndex = 9;
			this.label6.Text = "- Temp Logging:";
			// 
			// textbox_lot_days
			// 
			this.textbox_lot_days.Location = new System.Drawing.Point(155, 101);
			this.textbox_lot_days.Name = "textbox_lot_days";
			this.textbox_lot_days.Size = new System.Drawing.Size(43, 20);
			this.textbox_lot_days.TabIndex = 10;
			this.textbox_lot_days.TextChanged += new System.EventHandler(this.textbox_lot_days_TextChanged);
			// 
			// textbox_lot_hrs
			// 
			this.textbox_lot_hrs.Location = new System.Drawing.Point(241, 101);
			this.textbox_lot_hrs.Name = "textbox_lot_hrs";
			this.textbox_lot_hrs.Size = new System.Drawing.Size(43, 20);
			this.textbox_lot_hrs.TabIndex = 11;
			this.textbox_lot_hrs.TextChanged += new System.EventHandler(this.textbox_lot_hrs_TextChanged);
			// 
			// textbox_lot_mins
			// 
			this.textbox_lot_mins.Location = new System.Drawing.Point(327, 101);
			this.textbox_lot_mins.Name = "textbox_lot_mins";
			this.textbox_lot_mins.Size = new System.Drawing.Size(43, 20);
			this.textbox_lot_mins.TabIndex = 12;
			this.textbox_lot_mins.TextChanged += new System.EventHandler(this.textbox_lot_mins_TextChanged);
			// 
			// textbox_temp_days
			// 
			this.textbox_temp_days.Location = new System.Drawing.Point(155, 120);
			this.textbox_temp_days.Name = "textbox_temp_days";
			this.textbox_temp_days.Size = new System.Drawing.Size(43, 20);
			this.textbox_temp_days.TabIndex = 13;
			this.textbox_temp_days.TextChanged += new System.EventHandler(this.textbox_temp_days_TextChanged);
			// 
			// textbox_temp_hrs
			// 
			this.textbox_temp_hrs.Location = new System.Drawing.Point(241, 120);
			this.textbox_temp_hrs.Name = "textbox_temp_hrs";
			this.textbox_temp_hrs.Size = new System.Drawing.Size(43, 20);
			this.textbox_temp_hrs.TabIndex = 14;
			this.textbox_temp_hrs.TextChanged += new System.EventHandler(this.textbox_temp_hrs_TextChanged);
			// 
			// textbox_temp_mins
			// 
			this.textbox_temp_mins.Location = new System.Drawing.Point(327, 120);
			this.textbox_temp_mins.Name = "textbox_temp_mins";
			this.textbox_temp_mins.Size = new System.Drawing.Size(43, 20);
			this.textbox_temp_mins.TabIndex = 15;
			this.textbox_temp_mins.TextChanged += new System.EventHandler(this.textbox_temp_mins_TextChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(121, 123);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(34, 13);
			this.label8.TabIndex = 17;
			this.label8.Text = "Days:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(121, 104);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(34, 13);
			this.label7.TabIndex = 18;
			this.label7.Text = "Days:";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(208, 104);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(26, 13);
			this.label9.TabIndex = 19;
			this.label9.Text = "Hrs:";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(292, 104);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(32, 13);
			this.label10.TabIndex = 20;
			this.label10.Text = "Mins:";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(208, 123);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(26, 13);
			this.label11.TabIndex = 21;
			this.label11.Text = "Hrs:";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(292, 123);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(32, 13);
			this.label12.TabIndex = 22;
			this.label12.Text = "Mins:";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(26, 19);
			this.label13.Name = "label13";
			this.label13.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label13.Size = new System.Drawing.Size(66, 13);
			this.label13.TabIndex = 23;
			this.label13.Text = "Temp Limits:";
			// 
			// textbox_set_temp
			// 
			this.textbox_set_temp.Location = new System.Drawing.Point(241, 48);
			this.textbox_set_temp.Name = "textbox_set_temp";
			this.textbox_set_temp.Size = new System.Drawing.Size(43, 20);
			this.textbox_set_temp.TabIndex = 24;
			this.textbox_set_temp.TextChanged += new System.EventHandler(this.textbox_set_temp_TextChanged);
			// 
			// textbox_high_temp
			// 
			this.textbox_high_temp.Location = new System.Drawing.Point(284, 48);
			this.textbox_high_temp.Name = "textbox_high_temp";
			this.textbox_high_temp.Size = new System.Drawing.Size(43, 20);
			this.textbox_high_temp.TabIndex = 25;
			this.textbox_high_temp.TextChanged += new System.EventHandler(this.textbox_high_temp_TextChanged);
			// 
			// textbox_max_temp
			// 
			this.textbox_max_temp.Location = new System.Drawing.Point(327, 48);
			this.textbox_max_temp.Name = "textbox_max_temp";
			this.textbox_max_temp.Size = new System.Drawing.Size(43, 20);
			this.textbox_max_temp.TabIndex = 26;
			this.textbox_max_temp.TextChanged += new System.EventHandler(this.textbox_max_temp_TextChanged);
			// 
			// textbox_low_temp
			// 
			this.textbox_low_temp.Location = new System.Drawing.Point(198, 48);
			this.textbox_low_temp.Name = "textbox_low_temp";
			this.textbox_low_temp.Size = new System.Drawing.Size(43, 20);
			this.textbox_low_temp.TabIndex = 27;
			this.textbox_low_temp.TextChanged += new System.EventHandler(this.textbox_low_temp_TextChanged);
			// 
			// textbox_min_temp
			// 
			this.textbox_min_temp.Location = new System.Drawing.Point(155, 48);
			this.textbox_min_temp.Name = "textbox_min_temp";
			this.textbox_min_temp.Size = new System.Drawing.Size(43, 20);
			this.textbox_min_temp.TabIndex = 28;
			this.textbox_min_temp.TextChanged += new System.EventHandler(this.textbox_min_temp_TextChanged);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(165, 29);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(24, 13);
			this.label14.TabIndex = 29;
			this.label14.Text = "Min";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(205, 29);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(27, 13);
			this.label15.TabIndex = 30;
			this.label15.Text = "Low";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(251, 29);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(23, 13);
			this.label16.TabIndex = 31;
			this.label16.Text = "Set";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(289, 29);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(29, 13);
			this.label17.TabIndex = 32;
			this.label17.Text = "High";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(333, 29);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(27, 13);
			this.label18.TabIndex = 33;
			this.label18.Text = "Max";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(31, 51);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(58, 13);
			this.label19.TabIndex = 34;
			this.label19.Text = "- Chamber:";
			// 
			// combobox_chamber_select
			// 
			this.combobox_chamber_select.FormattingEnabled = true;
			this.combobox_chamber_select.Items.AddRange(new object[] {
            "1",
            "2"});
			this.combobox_chamber_select.Location = new System.Drawing.Point(94, 47);
			this.combobox_chamber_select.Name = "combobox_chamber_select";
			this.combobox_chamber_select.Size = new System.Drawing.Size(31, 21);
			this.combobox_chamber_select.TabIndex = 35;
			this.combobox_chamber_select.Text = "1";
			this.combobox_chamber_select.SelectedIndexChanged += new System.EventHandler(this.combobox_chamber_select_SelectedIndexChanged);
			// 
			// Form3
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(415, 252);
			this.Controls.Add(this.combobox_chamber_select);
			this.Controls.Add(this.label19);
			this.Controls.Add(this.label18);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.textbox_min_temp);
			this.Controls.Add(this.textbox_low_temp);
			this.Controls.Add(this.textbox_max_temp);
			this.Controls.Add(this.textbox_high_temp);
			this.Controls.Add(this.textbox_set_temp);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textbox_temp_mins);
			this.Controls.Add(this.textbox_temp_hrs);
			this.Controls.Add(this.textbox_temp_days);
			this.Controls.Add(this.textbox_lot_mins);
			this.Controls.Add(this.textbox_lot_hrs);
			this.Controls.Add(this.textbox_lot_days);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textbox_temp_reports);
			this.Controls.Add(this.textbox_lotreports);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "Form3";
			this.Text = "Form3";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textbox_lotreports;
		private System.Windows.Forms.TextBox textbox_temp_reports;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textbox_lot_days;
		private System.Windows.Forms.TextBox textbox_lot_hrs;
		private System.Windows.Forms.TextBox textbox_lot_mins;
		private System.Windows.Forms.TextBox textbox_temp_days;
		private System.Windows.Forms.TextBox textbox_temp_hrs;
		private System.Windows.Forms.TextBox textbox_temp_mins;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.TextBox textbox_set_temp;
		private System.Windows.Forms.TextBox textbox_high_temp;
		private System.Windows.Forms.TextBox textbox_max_temp;
		private System.Windows.Forms.TextBox textbox_low_temp;
		private System.Windows.Forms.TextBox textbox_min_temp;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.ComboBox combobox_chamber_select;
	}
}