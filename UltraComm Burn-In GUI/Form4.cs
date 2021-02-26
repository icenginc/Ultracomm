using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UltraComm_Burn_In_GUI
{
	public partial class Form4 : Form
	{
		string lotnum;
		string filename;
		string jobnum;
		string part;
		

		public Form4()
		{
			InitializeComponent();
			initialize_comboboxes();
		}

		private void button2_Click(object sender, EventArgs e)// cancel button
		{
			//make load button come back, change state back to stopped
			if (this.Text.Contains("1"))
			{
				Form1.state1 = "STOPPED";
				Console.WriteLine("changing state to stopped");
				Form1.form4_lock[0] = false;
			}
			if (this.Text.Contains("2"))
			{
				Form1.state2 = "STOPPED";
				Form1.form4_lock[1] = false;
			}
			this.Close();
		}

		private void button1_Click(object sender, EventArgs e)// ok button
		{
			//lot_params new_lot_params = new lot_params();
			if (part == null)
				MessageBox.Show("Must select part!");
			else
			{
				if (this.Text.Contains("1"))
				{
					//new_lot_params.lotnum = lotnum;
					//new_lot_params.step_file_name = filename;

					Form1.lotnum[0] = lotnum;
					Form1.jobnum[0] = jobnum;
					Form1.partnum[0] = part;
					Form1.step_file_name[0] = filename;
					Console.WriteLine(Form1.step_file_name);
					Form1.read_step[0] = true;
					Form1.loaded_check[0] = true;
				}
				if (this.Text.Contains("2"))
				{
					Form1.lotnum[1] = lotnum;
					Form1.jobnum[1] = jobnum;
					Form1.partnum[1] = part;
					Form1.step_file_name[1] = filename;
					Form1.read_step[1] = true;
					Form1.loaded_check[1] = true;
				}
				this.Close();
			}
		}

		private void textbox_lotnum_TextChanged(object sender, EventArgs e)
		{
			lotnum = textBox_lotnum.Text;
		}

		private void initialize_comboboxes()
		{
			combobox_stepfile.Text = Form1.step_file_path;
			try
			{
				DirectoryInfo step_directory = new DirectoryInfo(Form1.step_file_path);


				var files = step_directory.GetFiles().ToList();

				foreach (FileInfo file in files)
				{
					if (file.Name.Contains("csv") && file.Name.Contains("step"))
						combobox_stepfile.Items.Add(file);
				}
			}
			catch
			{
				Console.WriteLine("Close the document first!"); //maybe introduce form3 for error?
			}

			foreach (string part in Form1.partnum_list)
				comboBox_part.Items.Add(part);
		}

		private void combobox_stepfile_SelectedIndexChanged(object sender, EventArgs e)
		{
			filename = combobox_stepfile.Text;
		}

		private void Form4_Load(object sender, EventArgs e)
		{

		}

		private void textBox_jobnum_TextChanged(object sender, EventArgs e)
		{
			jobnum = textBox_jobnum.Text;
		}

		private void comboBox_part_TextChanged(object sender, EventArgs e)
		{
			part = comboBox_part.Text;
		}
	}
}
