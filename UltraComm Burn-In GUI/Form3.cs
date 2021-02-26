using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UltraComm_Burn_In_GUI
{
	public partial class Form3 : Form
	{
		private int lot_days;
		private int lot_hrs;
		private int lot_mins;
		
		private int temp_days;
		private int temp_hrs;
		private int temp_mins;

		private int min;
		private int low;
		private int set;
		private int high;
		private int max;

		public Form3()
		{
			InitializeComponent();
			initialize_text();
		}

		private void initialize_text()
		{
			textbox_lot_days.Text = "0";
			textbox_lot_hrs.Text = "0";
			textbox_lot_mins.Text = Form1.logging_interval_lot.ToString();

			textbox_temp_days.Text = "0";
			textbox_temp_hrs.Text = "0";
			textbox_temp_mins.Text = Form1.logging_interval_temp.ToString();

			textbox_min_temp.Text = Form1.min[0].ToString();
			textbox_low_temp.Text = Form1.low[0].ToString();
			textbox_set_temp.Text = Form1.set[0].ToString();
			textbox_high_temp.Text = Form1.high[0].ToString();
			textbox_max_temp.Text = Form1.max[0].ToString();

			textbox_lotreports.Text = Form1.lot_report_path;
			textbox_temp_reports.Text = Form1.temp_report_path;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			folderBrowserDialog2.ShowDialog();
			string path = folderBrowserDialog2.SelectedPath;
			//path.Replace("\\", "/");
			if (!Form1.edit_lock)
				textbox_lotreports.Text = path + "\\Temp_Reports\\";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.ShowDialog();
			string path = folderBrowserDialog1.SelectedPath;
			//path.Replace("\\", "/");
			if (!Form1.edit_lock)
				textbox_lotreports.Text = path + "\\Lot_Reports\\";
		}

		private void textbox_lot_days_TextChanged(object sender, EventArgs e)
		{
			if (textbox_lot_days.Text == "")
				textbox_lot_days.Text = "0";
			bool success = Int32.TryParse(textbox_lot_days.Text, out lot_days);
			if(success && !Form1.edit_lock)
				Form1.logging_interval_lot = (lot_days * 1440) + (lot_hrs * 60) + lot_mins;
		}
		private void textbox_lot_hrs_TextChanged(object sender, EventArgs e)
		{
			if (textbox_lot_hrs.Text == "")
				textbox_lot_hrs.Text = "0";
			bool success = Int32.TryParse(textbox_lot_hrs.Text, out lot_hrs);
			if (success && !Form1.edit_lock)
				Form1.logging_interval_lot = (lot_days * 1440) + (lot_hrs * 60) + lot_mins;
		}
		private void textbox_lot_mins_TextChanged(object sender, EventArgs e)
		{
			if (textbox_lot_mins.Text == "")
				textbox_lot_mins.Text = "0";
			bool success = Int32.TryParse(textbox_lot_mins.Text, out lot_mins);
			if (success && !Form1.edit_lock)
				Form1.logging_interval_lot = (lot_days * 1440) + (lot_hrs * 60) + lot_mins;
		}
		private void textbox_temp_days_TextChanged(object sender, EventArgs e)
		{
			if (textbox_temp_days.Text == "")
				textbox_temp_days.Text = "0";
			bool success = Int32.TryParse(textbox_temp_days.Text, out temp_days);
			if(success && !Form1.edit_lock)
				Form1.logging_interval_temp = (temp_days * 1440) + (temp_hrs * 60) + temp_mins;
		}
		private void textbox_temp_hrs_TextChanged(object sender, EventArgs e)
		{
			if (textbox_temp_hrs.Text == "")
				textbox_temp_hrs.Text = "0";
			bool success = Int32.TryParse(textbox_temp_hrs.Text, out temp_hrs);
			if (success && !Form1.edit_lock)
				Form1.logging_interval_temp = (temp_days * 1440) + (temp_hrs * 60) + temp_mins;
		}
		private void textbox_temp_mins_TextChanged(object sender, EventArgs e)
		{
			if (textbox_temp_mins.Text == "")
				textbox_temp_mins.Text = "0";
			bool success = Int32.TryParse(textbox_temp_mins.Text, out temp_mins);
			if (success && !Form1.edit_lock)
				Form1.logging_interval_temp = (temp_days * 1440) + (temp_hrs * 60) + temp_mins;
		}
		private void textbox_lotreports_TextChanged(object sender, EventArgs e)
		{
			if(!Form1.edit_lock)
				Form1.lot_report_path = textbox_lotreports.Text;
		}
		private void textbox_temp_reports_TextChanged(object sender, EventArgs e)
		{
			if (!Form1.edit_lock)
				Form1.temp_report_path = textbox_temp_reports.Text;
		}

		private void textbox_min_temp_TextChanged(object sender, EventArgs e)
		{
			if (textbox_min_temp.Text == "")
				textbox_min_temp.Text = "0";
			bool success = Int32.TryParse(textbox_min_temp.Text, out min);
			if (success && !Form1.edit_lock)
			{
				//Form1.min[0] = min; //turn off, user shouldn't be able to control this
			}
		}

		private void textbox_low_temp_TextChanged(object sender, EventArgs e)
		{
			if (textbox_low_temp.Text == "")
				textbox_low_temp.Text = "0";
			bool success = Int32.TryParse(textbox_low_temp.Text, out low);
			if (success && !Form1.edit_lock)
				if (combobox_chamber_select.Text == "1")
					Form1.low[0] = low;
				else if (combobox_chamber_select.Text == "2")
					Form1.low[1] = low;
		}

		private void textbox_set_temp_TextChanged(object sender, EventArgs e)
		{
			if (textbox_set_temp.Text == "")
				textbox_set_temp.Text = "0";
			bool success = Int32.TryParse(textbox_set_temp.Text, out set);
			if (success && !Form1.edit_lock)
				if (combobox_chamber_select.Text == "1")
					Form1.set[0] = set;
				else if (combobox_chamber_select.Text == "2")
					Form1.set[1] = set;
		}

		private void textbox_high_temp_TextChanged(object sender, EventArgs e)
		{
			if (textbox_high_temp.Text == "")
				textbox_high_temp.Text = "0";
			bool success = Int32.TryParse(textbox_high_temp.Text, out high);
			if (success && !Form1.edit_lock)
				if(combobox_chamber_select.Text == "1")
					Form1.high[0] = high;
				else if(combobox_chamber_select.Text == "2")
					Form1.high[1] = high;
		}

		private void textbox_max_temp_TextChanged(object sender, EventArgs e)
		{
			if (textbox_max_temp.Text == "")
				textbox_max_temp.Text = "0";
			bool success = Int32.TryParse(textbox_max_temp.Text, out max);
			if (success && !Form1.edit_lock)
			{
				//Form1.max[0] = max; ////turn off, user shouldn't be able to control this
			}
		}

		private void combobox_chamber_select_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (combobox_chamber_select.Text == "1")
			{
				textbox_min_temp.Text = Form1.min[0].ToString();
				textbox_low_temp.Text = Form1.low[0].ToString();
				textbox_set_temp.Text = Form1.set[0].ToString();
				textbox_high_temp.Text = Form1.high[0].ToString();
				textbox_max_temp.Text = Form1.max[0].ToString();
			}
			if (combobox_chamber_select.Text == "2")
			{
				textbox_min_temp.Text = Form1.min[1].ToString();
				textbox_low_temp.Text = Form1.low[1].ToString();
				textbox_set_temp.Text = Form1.set[1].ToString();
				textbox_high_temp.Text = Form1.high[1].ToString();
				textbox_max_temp.Text = Form1.max[1].ToString();
			}
		}
	}
}
