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
	public partial class Form2 : Form
	{

		//Information from previous form with all parameters of the current slot.
		slot_param current_slot;

		//Other Variables
		float[] temps = new float[20];
		float[] amps = new float[20];
		double[] psu = new double[6];
		double[] adc = new double[6];
		List<Label> labels = new List<Label>();
		public static bool[] update = new bool[16];
		int slot_num;
		
		
		//Chosen slot from Form1 are sent and stored in Form2 variables.
		public Form2(int slot)
		{
			InitializeComponent();
			update[slot] = true;
			slot_num = slot;
			current_slot = Form1.slot_params[slot];
			if (Form1.finishedscan == false)
			{
				busyscan.Show();
				button1.Hide();
				button_reset.Hide();
			}
			else
			{
				busyscan.Hide();
				button1.Show();
				button_reset.Show();
			}
			if (Form1.state1 == "RUNNING" || Form1.state2 == "RUNNING")
			{
				button1.Hide();
				button_reset.Hide();
				busyscan.Hide();

			}

		}//ctr


		//Literally just loads everything on the Form2 UI.
		//Creates a class Backgroundworker(class library C# already has) with the name updater
		//Assigns function Updater_Dowork to the class function doWork, runs the updater Asynchronously, runs other functions created.

		private void Form2_Load(object sender, EventArgs e)
		{
			//need another background process for live updating temps/amps
			BackgroundWorker updater = new BackgroundWorker();
			updater.DoWork += Updater_DoWork;
			updater.RunWorkerAsync();
			add_labels();
			update_labels();
		}



		//Assumed to be the "Main" function of the Form2 processes
		//Continuosly updates once for form2 for the chamber has been opened
		private void Updater_DoWork(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("--Opening " +this.Name+ " for slot " + (slot_num+1));
			while (update[slot_num])
			{
				//Console.WriteLine("Enter do work funct form2");
				try
				{
					this.Invoke((MethodInvoker)delegate
					{
						update_labels();
					});
				}//invoke should be in try loop to prevent errors
				catch
				{
				}
				System.Threading.Thread.Sleep(1000); //1 second at least between gui updates
			}
		}


		//Created DUT labels and put into the List of Labels created earlier
		private void add_labels()
		{
			Label[] label_range = { label1, label2, label3, label4, label5, label6, label7, label8, label9,
				label10, label11, label12, label13, label14, label15, label16, label17, label18, label19, label20};
			labels.AddRange(label_range);
		}

	
		//Updates the elements on form2 (Color, Temp, etc)
		private void update_labels()
		{
			temps = current_slot.dut_temp;
			amps = current_slot.dut_current;
			adc = current_slot.adc_measures;
			psu = current_slot.psu_measures;


			Console.WriteLine("---Updating elements on "+ this.Name +" for slot " +(slot_num+1));

			//Combined Nabeels old Update_Label_colors function into this function
			//Changes the color of the DUT
			for (int i = 0; i < 20; i++)
			{
				if (current_slot.dut_present[i])
					labels[i].BackColor = System.Drawing.Color.Gold;
				else
					labels[i].BackColor = SystemColors.ControlLight;
			}

			
			
			//Assigns the Elements inside the DUTs
			for(int i = 0; i < 20; i++)
				labels[i].Text = "DUT" + (i+1) + "\n\nTemp: " + temps[i].ToString("0.0") + "\nAmps: " + amps[i].ToString("0.000");
			/*
			label1.Text = "DUT1\n\nTemp: " + temps[0] + "\nAmps: " + amps[0];
			label2.Text = "DUT2\n\nTemp: " + temps[1] + "\nAmps: " + amps[1];
			label3.Text = "DUT3\n\nTemp: " + temps[2] + "\nAmps: " + amps[2];
			label4.Text = "DUT4\n\nTemp: " + temps[3] + "\nAmps: " + amps[3];
			label5.Text = "DUT5\n\nTemp: " + temps[4] + "\nAmps: " + amps[4];
			label6.Text = "DUT6\n\nTemp: " + temps[5] + "\nAmps: " + amps[5];
			label7.Text = "DUT7\n\nTemp: " + temps[6] + "\nAmps: " + amps[6];
			label8.Text = "DUT8\n\nTemp: " + temps[7] + "\nAmps: " + amps[7];
			label9.Text = "DUT9\n\nTemp: " + temps[8] + "\nAmps: " + amps[8];
			label10.Text = "DUT10\n\nTemp: " + temps[9] + "\nAmps: " + amps[9];
			label11.Text = "DUT11\n\nTemp: " + temps[10] + "\nAmps: " + amps[10];
			label12.Text = "DUT12\n\nTemp: " + temps[11] + "\nAmps: " + amps[11];
			label13.Text = "DUT13\n\nTemp: " + temps[12] + "\nAmps: " + amps[12];
			label14.Text = "DUT14\n\nTemp: " + temps[13] + "\nAmps: " + amps[13];
			label15.Text = "DUT15\n\nTemp: " + temps[14] + "\nAmps: " + amps[14];
			label16.Text = "DUT16\n\nTemp: " + temps[15] + "\nAmps: " + amps[15];
			label17.Text = "DUT17\n\nTemp: " + temps[16] + "\nAmps: " + amps[16];
			label18.Text = "DUT18\n\nTemp: " + temps[17] + "\nAmps: " + amps[17];
			label19.Text = "DUT19\n\nTemp: " + temps[18] + "\nAmps: " + amps[18];
			label20.Text = "DUT20\n\nTemp: " + temps[19] + "\nAmps: " + amps[19];
			*/ //a lot of yping, dont want to delete 

			/*try
			{*/

				//Various Text display assignments made here.
				textbox_controller_sn.Text = current_slot.controller_id;
				textbox_controller_mfg_date.Text = current_slot.controller_mfg_date;
				textbox_cal_date.Text = current_slot.controller_cal_date;
				textbox_controller_mfg_date.Text = current_slot.controller_mfg_date;
				textbox_controller_pcb.Text = current_slot.controller_pcb;
				textbox_cpld_version.Text = current_slot.controller_cpld_version;
				text_boxps0.Text = adc[0].ToString();
				text_boxps2.Text = String.Format("{0:F3}", adc[1]);
				text_boxBiB3v3.Text = adc[2].ToString();
				text_boxBiB5v0.Text = adc[3].ToString();
				text_boxuC2v5ref.Text = adc[4].ToString();
				textBox_imeasured.Text = adc[5].ToString();
				psu1v.Text = psu[0].ToString();
				psu2v.Text = psu[1].ToString();
				ps1i.Text = psu[2].ToString();
				ps2i.Text = psu[3].ToString();
				ps1temp.Text = psu[4].ToString();
				ps2temp.Text = psu[5].ToString();


				//Basically Chamber 1
				if (slot_num < 8)
				{
					text_box_vmin0.Text = Form1.current_step[0].ps_voltage_min[0].ToString();
					text_box_vmin1.Text = Form1.current_step[0].ps_voltage_min[1].ToString();
					text_box_vmax0.Text = Form1.current_step[0].ps_voltage_max[0].ToString();
					text_box_vmax1.Text = Form1.current_step[0].ps_voltage_max[1].ToString();
				} //hard limits defined in config

				//Basically Chamber 2
				if (slot_num > 7)
				{
					text_box_vmin0.Text = Form1.current_step[1].ps_voltage_min[0].ToString();
					text_box_vmin1.Text = Form1.current_step[1].ps_voltage_min[1].ToString();
					text_box_vmax0.Text = Form1.current_step[1].ps_voltage_max[0].ToString();
					text_box_vmax1.Text = Form1.current_step[1].ps_voltage_max[1].ToString();
				} //hard limits defined in config

			/*}//while it is not filled, dont throw an error
			catch
			{
			}*/

			//CN4
			if (Form1.slot_params[slot_num].scanning == false && button1.Visible == false)
			{
				Console.WriteLine("-------------------------------------------RESCAN BUTTON PUT BACK");
				busyscan.Visible = false;
				button_reset.Visible = true;
				button1.Visible = true;
				Form1.finishedscan = false;

			}
			if (Form1.state1 == "RUNNING" || Form1.state2 == "RUNNING")
			{
				button1.Hide();
				button_reset.Hide();
				busyscan.Hide();
			}
			

		}

		//when form 2 is closed, stop updating the boxes
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			update[slot_num] = false;
		}


		//uC reset button command
		private void button_reset_Click(object sender, EventArgs e)
		{
			current_slot.reset_uc(slot_num);
			//current_slot.reset_uc_extended();
		}


		//CN4
		//Rescan DUT function
		private void button1_Click(object sender, EventArgs e)
		{
			Form1.slot_params[slot_num].scanning = true;
			Console.WriteLine("-------------------------------------------BUSY SCANNING DUTS (SCANNING=TRUE)");
			Form1.slot_params[slot_num].scan_slot();
			button1.Visible = false;
			button_reset.Visible = false;
			busyscan.BringToFront();
			busyscan.Visible = true;
			Form1.finishedscan = false;
			
		}


		/*
protected override void OnPaint(PaintEventArgs e)
{
base.OnPaint(e);

System.Drawing.SolidBrush box_brush = new System.Drawing.SolidBrush(System.Drawing.Color.Gold);
System.Drawing.Graphics form_graphics = e.Graphics;

Rectangle one = new Rectangle(535, 50, 15, 98);
Rectangle two = new Rectangle(535, 180, 15, 98);
form_graphics.FillRectangle(box_brush, one);
form_graphics.FillRectangle(box_brush, two);


box_brush.Dispose();
form_graphics.Dispose();
}
*/
	}
}
