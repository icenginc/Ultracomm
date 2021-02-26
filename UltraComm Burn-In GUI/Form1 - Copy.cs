using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UltraComm_Burn_In_GUI
{
	public partial class Form1 : Form
	{
		bool form3_lock = false;
		public static bool edit_lock = true; //for changing path in form 3 - settings
		bool[] header_lock = new bool[2];
		bool[] pretest_lock = new bool[2];		public static bool[] loaded_check = Enumerable.Repeat<bool>(false, 2).ToArray();
		public static bool[] form4_lock = Enumerable.Repeat<bool>(false, 2).ToArray();
		public static bool[] read_step = new bool[2];
		bool[] step_sleep = new bool[2];
		bool[] step_transition = Enumerable.Repeat<bool>(false, 2).ToArray();
		bool connect_to_zeroc = false;
		public static bool[] temp_ramp = Enumerable.Repeat<bool>(false, 2).ToArray();
		//locks for safety/function

		public static int logging_interval_lot = 5;
		public static int logging_interval_temp = 5;
		public static string lot_report_path = "C:/Users/Public/Documents/Lot_Reports/";
		string[] lot_report_file = Enumerable.Repeat<string>("", 2).ToArray();
		public static string temp_report_path = "C:/Users/Public/Documents/Temp_Reports/";
		string[] temp_report_file = Enumerable.Repeat<string>("", 2).ToArray();
		public static string step_file_path;
		public static string[] step_file_name = Enumerable.Repeat<string>("NO STEP FILE", 2).ToArray();
		public static List<string> step_file_contents1 = new List<string>();
		public static List<string> step_file_contents2 = new List<string>();
		public static string ftp_upload_path;
		//step items
		

		string[] system_name = new string[2];
		public static string[] lotnum = Enumerable.Repeat<string>("123456", 2).ToArray();
		public static string[] jobnum = Enumerable.Repeat<string>("654321", 2).ToArray();
		public static string[] partnum = Enumerable.Repeat<string>("000000", 2).ToArray();
		string datetime = DateTime.Now.ToString("MM//dd//yy-HH:mm:ss");
		string datetime2 = DateTime.Now.ToString("yyyyMMdd-HH.mm.ss");//special for filename
		string[] duration = new string[2];
		int[] step_num = new int[2];
		string[] time_rem = Enumerable.Repeat<string>("00D:00H:00M", 2).ToArray();
		//lot items

		public static string state1 = "STOPPED";
		public static string state2 = "STOPPED";

		public static int[] min = new int[2];
		public static int[] low = new int[2];
		public static int[] set = new int[2];
		public static int[] high = new int[2];
		public static int[] max = new int[2];
		int[] current_step_index = new int[2];
		int psu_retries = 0;

		string[] alarm_msg = new string[2];
		string[] elapsed_time = new string[2];

		public static List<step_params> step_list1 = new List<step_params>();
		public static List<step_params> step_list2 = new List<step_params>();
		public static step_params pretest_step1 = new step_params();
		public static step_params pretest_step2 = new step_params();
		public static step_params current_step1 = new step_params();
		public static step_params current_step2 = new step_params();
		public static slot_param[] slot_params = new slot_param[16];
		Stopwatch step_timer1 = new Stopwatch();
		Stopwatch step_timer2 = new Stopwatch();
		List<TurnButton> button_list = new List<TurnButton>();

		//ZeroC stuff
		List<string> ZeroC_connections = new List<string>(); //this is to save what connections we have
		List<string> ZeroC_connections_fury = new List<string>();
		Dictionary<string, string> m_innovative_remoteConnections = new Dictionary<string, string>();        // id, endoint
		Dictionary<string, string> m_fury_remoteConnections = new Dictionary<string, string>();        // id, endoint
		InnovativeInterface m_innovative_interface;
		FuryInterface m_fury_interface;
		X80QC m_x80_hal;
		string[] PACKAGE_EXCHANGE = new string[16];
		string[] BI = Enumerable.Repeat<string>("0", 16).ToArray();
		string[] UC = Enumerable.Repeat<string>("0", 16).ToArray();
		bool[] mailbox_wait_for_clear = new bool[16];
		bool[] mailbox_ack_received = new bool[16];

		//Hardware variables
		string[] oven_temp = Enumerable.Repeat<string>("0.00", 2).ToArray();

		//public static List<lot_params> lot_list = new List<lot_params>();
		//below this for lot reporting

		public Form1() //entry point
		{
			InitializeComponent();
			//below this for main slot display

			ThreadPool.GetMinThreads(out int threads, out int completion_threads);
			ThreadPool.SetMinThreads(20, completion_threads); //this sets available threads, so we can poll ZeroC on all slots instantly
			
			chamber_config_read();

			initialize_elements(); //initialize slot params array, and turnbutton list

			initialize_labels(1);
			initialize_labels(2);

			scan_zeroc_servers();

			Console.WriteLine("create background worker");

			BackgroundWorker background_worker = new BackgroundWorker   //create new thread for "Core" loop
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};

			background_worker.DoWork += new DoWorkEventHandler(background_worker_dowork);
			background_worker.ProgressChanged += new ProgressChangedEventHandler(background_worker_progress);
			background_worker.RunWorkerCompleted += background_worker_complete;
			if (!this.IsHandleCreated)
				this.CreateHandle(); //force window to come up, in case that it doesn't
			background_worker.RunWorkerAsync(); //launch into core background process of program
		}


		///BACKGROUND WORKER BELOW |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

		private void background_worker_dowork(object sender, DoWorkEventArgs e)
		{

			Console.WriteLine("enter core backgroundworker do work function");

			for (int i = 0; i < 16; i++)
			{
				BackgroundWorker remote_comm_worker = create_remote_comm_worker();
				remote_comm_worker.RunWorkerAsync(i); //how often to poll data from ZeroC
			}

			BackgroundWorker mailbox_handler = new BackgroundWorker();
			mailbox_handler.DoWork += Mailbox_handler_DoWork;
			mailbox_handler.RunWorkerAsync(); //mailbox stuff

			
			update_tooltip_temp(min[0], low[0], set[0], high[0], max[0], 1); //update tooltip1 upon start
			update_tooltip_temp(min[1], low[1], set[1], high[1], max[1], 2); //update tooltip1 upon start

			//temp_log_timer1.Interval = logging_interval_temp * 60000;
			//lot_log_timer1.Interval = logging_interval_lot * 60000;

			while (true)
			{
				update_status(); //constantly update certain display stuff

				if (state1 == "RUNNING")
				{
					edit_lock = true;

					for (int i = 0; i < step_list1.Count; i++)
					{
						if (step_list1[i].step_done == false)
						{
							current_step_index[0] = i;
							current_step1 = step_list1[i];
							if (step_transition[0])
							{
								for (int j = 0; j < 8; j++) //write mailbox stuff according to new step
								{
									slot_params[j].innovative_hal.WRITEMAILBOX("BI", current_step1.mailbox_num);
									slot_params[j].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", current_step1.mailbox_string);
									mailbox_ack_received[j] = false; //wait for the ack received after changing steps
									step_transition[0] = false;
								} //do mailbox stuff immediately once transition flag is raised - this only happens once
							}
							break; //stop loop as soon as we find the first unfinished step
						}
					}

					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Show();
						button_load1.Hide();
						button_clear1.Hide();
						button_start1.Hide();

						if (header_lock[0] != true)
							make_lot_header(1);

						header_lock[0] = true;

						temp_log_timer1.Enabled = true; //start logging at correct interval
						lot_log_timer1.Enabled = true;
						temp_log_timer1.Interval = 30000;// current_step1.lot_log_interval * (1000) * 60;
						lot_log_timer1.Interval = 20000;//current_step1.lot_log_interval * (1000) * 60;

						//need if condition with temp_ramped bool, and a loop to only start once we have the wait_for_clear bit set
						if (!current_step1.temp_ramped && check_bool(false, mailbox_wait_for_clear, 1) && !step_sleep[0]) //no outstanding clear we are waiting on, and also havent ramped yet
						{
							Console.WriteLine("STARTING TEMP RAMP");
							for (int i = 0; i < 8; i++)
							{
								if (slot_params[i].m_innovativeConnected && BI[i] != "4")
								{
									slot_params[i].innovative_hal.WRITEMAILBOX("BI", "4"); //BUSY
									slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", "RampingTemperature");
									mailbox_ack_received[i] = false; //need to wait on ack for the oven ramp to move on
								}
							}
							//fan on oven
							//heater on oven
							//set_temp on oven							
							current_step1.temp_ramped = true; //activated ramp, now heating up
						} //oven control condition


						if (check_bool(true, mailbox_ack_received, 1) && current_step1.temp_ramped)
						{
							Console.WriteLine("TEMP IS RAMPING");
							//check temp, make sure it is at appropriate temp
							//keep going in here until temp is reached

							for (int i = 0; i < 8; i++)
							{
								if (slot_params[i].m_innovativeConnected && BI[i] != current_step1.mailbox_num)
								{
									slot_params[i].innovative_hal.WRITEMAILBOX("BI", current_step1.mailbox_num); //BUSY
									slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", current_step1.mailbox_string);
									mailbox_ack_received[i] = false; //need to wait on ack for the oven ramp to move on
								}
							}

							if (current_step1.temp_reached)//use this statement once we reach temp, to restart timer
							{
								step_timer1.Start();
								Console.WriteLine("Temp has reached, \"ramping temp\" has been acked, start timer");
							}
						}

						if ((step_timer1.Elapsed.TotalSeconds > current_step1.step_time) && !current_step1.step_lock) //step lock is to stop moving on when on untimed steps
						{
							Console.WriteLine("step transitioning");

							step_sleep[0] = true;
							step_transition[0] = true;
							step_timer1.Stop(); //stop and reset timer after step complete
							step_timer1.Reset();

							step_list1[current_step_index[0]].step_done = true; //will make loop set current step to the next step
																				//unramp_power_supplies(1);       //mark step as done
							do_sleep_timer(1);
							for (int i = 0; i < 8; i++)
							{
								if(slot_params[i].m_innovativeConnected)
									mailbox_ack_received[i] = false;
							}
							//System.Threading.Thread.Sleep(current_step1.step_wait * 1000); //sleep how long designated btwn steps
						} //wait for step to finish

						//do pretest
						if (pretest_lock[0] != true)
						{
							do_pretest(1);
							Console.WriteLine("DOING PRETEST NOW");
							state1 = "RUNNING";
						}

						initialize_labels(1);
					});//if state is set to running, start logging



				}//run chamber 1 functs
				else if (state1 == "STOPPED")
				{
					header_lock[0] = false;
					edit_lock = false;
					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Hide();
						button_load1.Show();
						button_clear1.Show();
						button_start1.Hide();
						button_pretest1.Hide();
						if (loaded_check[0])
						{
							button_start1.Show();
							button_stop1.Hide();
							button_load1.Hide();
							button_pretest1.Hide();
						}//if restarting
					});//if state is set to stopped

					timer_zeroc.Enabled = true; //scan for zeroc servers
					lot_log_timer1.Enabled = false; //stop logging
					temp_log_timer1.Enabled = false;

					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].psu_status)
						{
							psu_zeroc(0, 0, "0.0", i);
							psu_zeroc(0, 1, "0.0", i);
						}
					} //make sure psu's are off
				}
				else if (state1 == "LOAD")
				{
					current_step1 = pretest_step1; //set current step to pretest step
					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Hide();
						button_load1.Hide();
						button_clear1.Show();
						button_start1.Hide();
						button_pretest1.Show();

						if (!CheckOpened("Load Step File - Chamber 1") && !form4_lock[0])
						{
							Console.WriteLine("LAUNCHING FORM 4");
							form4_lock[0] = true;
							launch_form4(1);
						}
						if (read_step[0] == true)
						{
							read_step_file(1);
							initialize_labels(1);
							Console.Write("Step List (Chamber 1): " + step_list1.Count);
						}

					});
				}
				else if (state1 == "CLEARING")
				{
					form4_lock[0] = false;
					loaded_check[0] = false;
					pretest_lock[0] = false;

					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Hide();
						button_load1.Hide();
						button_clear1.Hide();
						button_start1.Hide();

						current_step1 = new step_params();
						step_file_name[0] = "NO STEP FILE";
						lot_report_file[0] = "";
						temp_report_file[0] = "";
						lotnum[0] = "123456";
						jobnum[0] = "654321";
						partnum[0] = "000000";
						step_num[0] = 0;
						step_file_contents1.Clear();
						step_list1.Clear();
						//clear actual parameters in data structure as well

						step_timer1.Reset();

						initialize_labels(1);
						state1 = "STOPPED";
					});

				}
				else if (state1 == "PRETEST")
				{
					bool check = true;
					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].m_innovativeConnected)
						{
							if (mailbox_ack_received[i] == false)   //make sure we have an ack for pretest before moving to start
								check = false;
							if (slot_params[i].psu_busy) //ack and psu is not busy
								check = false;
							if (slot_params[i].file_passed == false)
								check = false;
						}
					} //go through all slots connected in chamber 1. if any do not display ack bit, then block start
					if (check)
					{
						this.Invoke((MethodInvoker)delegate
						{
							button_start1.Show();
						});
					}
				}

				if (state2 == "RUNNING")
				{
					edit_lock = true;

					for (int i = 0; i < step_list2.Count; i++)
					{
						if (step_list2[i].step_done == false)
						{
							current_step_index[1] = i;
							current_step2 = step_list2[i];
							break; //stop loop as soon as we find the first unfinished step
						}
					}

					this.Invoke((MethodInvoker)delegate
					{
						button_stop2.Show();
						button_load2.Hide();
						button_clear2.Hide();
						button_start2.Hide();

						if (header_lock[1] != true)
							make_lot_header(2);

						header_lock[1] = true;

						temp_log_timer2.Enabled = true;//start logging
						lot_log_timer2.Enabled = true;
						temp_log_timer2.Interval = current_step2.lot_log_interval * (1000) * 60;
						lot_log_timer2.Interval = current_step2.lot_log_interval * (1000) * 60;

						if (step_timer2.Elapsed.TotalSeconds > current_step2.step_time)
						{
							step_sleep[1] = true;
							step_timer2.Stop(); //stop and reset timer after step complete
							step_timer2.Reset();

							step_list2[current_step_index[1]].step_done = true; //will make loop set current step to the next step
							//unramp_power_supplies(2);       //mark step as done

							do_sleep_timer(2);
							//System.Threading.Thread.Sleep(current_step1.step_wait * 1000); //sleep how long designated btwn steps
						} //wait for step to finish

						initialize_labels(2);
					});//if state is set to running, start logging

					
				}//run chamber 2 functs
				else if (state2 == "STOPPED")
				{
					header_lock[1] = false;
					edit_lock = false;
					this.Invoke((MethodInvoker)delegate
					{
						button_stop2.Hide();
						button_load2.Show();
						button_clear2.Show();
						button_start2.Hide();
						button_pretest2.Hide();
						if (loaded_check[1])
						{
							button_start2.Show();
							button_load2.Hide();
							button_stop2.Hide();
							button_pretest2.Hide();
						}//if restarting
					});//if state is set to stopped

					timer_zeroc.Enabled = true;
					lot_log_timer2.Enabled = false;
					temp_log_timer2.Enabled = false;


					for (int i = 8; i < 16; i++)
					{
						if (slot_params[i].psu_status)
						{
							psu_zeroc(0, 0, "0.0", i);
							psu_zeroc(0, 1, "0.0", i);
						}
					} //make sure psu's are off					
				}
				else if (state2 == "LOAD")
				{
					current_step2 = pretest_step2; //have to run pretest
					this.Invoke((MethodInvoker)delegate
					{
						button_stop2.Hide();
						button_load2.Hide();
						button_clear2.Show();
						button_start2.Show();
						button_pretest2.Show();

						if (!CheckOpened("Load Step File - Chamber 2") && !form4_lock[1])
						{
							
							launch_form4(2);
							form4_lock[1] = true;
						}
						if (read_step[1] == true)
						{
							read_step_file(2);
							initialize_labels(2);
							Console.Write("Step List (Chamber 2): " + step_list2.Count);
						}
					});
				}
				else if (state2 == "CLEARING")
				{
					form4_lock[1] = false;
					loaded_check[1] = false;
					pretest_lock[1] = false;

					this.Invoke((MethodInvoker)delegate
					{
						button_stop2.Hide();
						button_load2.Hide();
						button_clear2.Hide();
						button_start2.Hide();

						current_step2 = new step_params();
						step_file_name[1] = "NO STEP FILE";
						lot_report_file[1] = "";
						temp_report_file[1] = "";
						lotnum[1] = "123456";
						jobnum[1] = "654321";
						partnum[1] = "000000";
						step_num[1] = 0;
						
						step_file_contents2.Clear();
						step_list2.Clear();
						//clear actual parameters in data structure as well

						step_timer2.Reset();

						initialize_labels(2);
						state2 = "STOPPED";
						
					});
					
				}

				if (CheckOpened("Settings"))
				{
					form3_lock = true;
					update_tooltip_temp(min[0], low[0], set[0], high[0], max[0], 1); //keep updating tooltip if form3 accessed
					update_tooltip_temp(min[1], low[1], set[1], high[1], max[1], 2); //keep updating tooltip if form3 accessed
					if (logging_interval_temp > 0)
					{
						temp_log_timer1.Interval = logging_interval_temp * 60000;
						temp_log_timer2.Interval = logging_interval_temp * 60000;
					}
					if (logging_interval_lot > 0)
					{
						lot_log_timer1.Interval = logging_interval_lot * 60000;
						lot_log_timer2.Interval = logging_interval_lot * 60000;
					}
				} //do this every 1 sec
				else
					form3_lock = false;

				System.Threading.Thread.Sleep(500);
			}
		}

		private void EEPROM_reader_DoWork(object sender, DoWorkEventArgs e)
		{
			for (int i = 0; i < 16; i++)
			{
				if (slot_params[i].m_innovativeConnected)
				{
					Console.WriteLine("setting GPIO and mailbox to default state");
					slot_params[i].innovative_hal.WRITEMAILBOX("BI", "1");
					slot_params[i].innovative_hal.Address(i + 1);
				}
			}
			for (int i = 0; i < 16; i++)
			{
				if (slot_params[i].m_innovativeConnected)
				{
					Console.WriteLine("Reading eeproms for slot " + i);
					slot_params[i].read_eeprom_data();
				}//if controller is present
			}
		}

		private void Mailbox_handler_DoWork(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				for (int i = 0; i < 16; i++) //scan through all 16 BBB
				{
					if (slot_params[i].m_innovativeConnected) //if the slot being iterated upon is connected, proceed to read
					{
						Console.WriteLine("Reading mailbox for slot " + i);

						if (!slot_params[i].psu_busy)
						{
							UC[i] = slot_params[i].innovative_hal.READMAILBOX("UC");
							BI[i] = slot_params[i].innovative_hal.READMAILBOX("BI");
						}

						if (UC[i] == "2") //if its an ACK, clear it out in UC automatically
						{
							slot_params[i].innovative_hal.WRITEMAILBOX("UC", "5");
							mailbox_ack_received[i] = true;
						}

						if (BI[i] == "5")
							mailbox_wait_for_clear[i] = false; //Python has cleared the ack, we can proceed

						if (UC[i] == "3")//if we see a 3, that means time-based measurement is done. ack and move on from step
						{
							if (i < 8)
								current_step1.step_lock = false;
							else if (i > 7)
								current_step2.step_lock = false;

							slot_params[i].innovative_hal.WRITEMAILBOX("BI", "2"); //send ack
							mailbox_wait_for_clear[i] = true;	//wait for the clear
						}
						if (UC[i] == "4") //if we see a 4, need to acknowledge
						{
							slot_params[i].innovative_hal.WRITEMAILBOX("BI", "2"); //send ack
							mailbox_wait_for_clear[i] = true; //wati for the clear
						}
					}
				}
				System.Threading.Thread.Sleep(5000);
			}
		}

		private void remote_comm_worker_dowork(object sender, DoWorkEventArgs e)
		{
			float constant = .0004394531F;
			var now = DateTime.Now.ToLocalTime().ToString();
			int slot = (int) e.Argument;
			Random random = new Random();
			int randomNumber = random.Next(0, 1000);

			while (true)
			{
				//Console.WriteLine("I am remote worker for ZeroC slot polling, Slot " + slot);
				if (slot_params[slot].m_innovativeConnected)
				{
					double[] poll_results = Enumerable.Repeat<double>(0, 6).ToArray();

					if (!slot_params[slot].psu_busy)
					{
						poll_results = slot_params[slot].do_adc_reads_float();
					}
						double[] poll_current; //not implemented
						double[] poll_temp; //not implemented
					
					/*
					float[] voltages = new float[6];

					voltages[0] = float.Parse(new_poll[0]) * 4 * constant;//phase 0
					voltages[1] = float.Parse(new_poll[1]) * 4 * constant;//phase 1
					voltages[2] = float.Parse(new_poll[2]) * 4 * constant;//bib 3v3
					voltages[3] = float.Parse(new_poll[3]) * 4 * constant;//bib 5v0
					voltages[4] = float.Parse(new_poll[4]) * 2.3F * constant;//uc2v5ref
					voltages[5] = float.Parse(new_poll[5]) * 9.66F * constant;//12v amps
					 //converting from adc bits to float - done in ICE app

					adc_poll[0 + (slot*6)] = poll_results[0].ToString();
					adc_poll[1 + (slot * 6)] = poll_results[1].ToString();
					adc_poll[2 + (slot * 6)] = poll_results[2].ToString();
					adc_poll[3 + (slot * 6)] = poll_results[3].ToString();
					adc_poll[4 + (slot * 6)] = poll_results[4].ToString();
					adc_poll[5 + (slot * 6)] = poll_results[5].ToString();
					*/
					slot_params[slot].adc_measures[0] = Math.Round(poll_results[0], 3);
					slot_params[slot].adc_measures[1] = Math.Round(poll_results[1], 3);
					slot_params[slot].adc_measures[2] = Math.Round(poll_results[2], 3);
					slot_params[slot].adc_measures[3] = Math.Round(poll_results[3], 3);
					slot_params[slot].adc_measures[4] = Math.Round(poll_results[4], 3);
					slot_params[slot].adc_measures[5] = Math.Round(poll_results[5], 3);

					for (int i = 0; i < poll_results.Length; i++)
						Console.Write(poll_results[i] + " ");
					Console.WriteLine("");
				}
				
				System.Threading.Thread.Sleep(4000 + randomNumber);
			}
		} //get data piped from zeroC app, then utilize it (depth 2) (log or display)

		

		private void remote_comm_worker_progress(object sender, ProgressChangedEventArgs e)
		{

		}
		private void remote_comm_worker_complete(object sender, RunWorkerCompletedEventArgs e)
		{
		
		}

		private void logging_worker_lot_dowork1(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("writing lot for chamber 1");
			using (StreamWriter writer = File.AppendText(lot_report_file[0]))
			{
				string line = String.Empty;
				for (int i = 0; i < 8; i++)
				{
					line += datetime + ", Total: " + duration[0] + ", Step: " + elapsed_time[0];
					line += ", PSU0: " + slot_params[i].adc_measures[0] + ", PSU1: " + slot_params[i].adc_measures[1];
					line += ", BiB3V3: " + slot_params[i].adc_measures[2] + ", BiB5V0: " + slot_params[i].adc_measures[3];
					line += ", uC2V5: " + slot_params[i].adc_measures[4] + ", Current: " + slot_params[i].adc_measures[5];
					line += Environment.NewLine;
					for (int j = 0; j < 20; j++)
					{
						line += datetime + ", Total: " + duration[0] + ", Step: " + elapsed_time[0];
						line += ", Slot: " + i + ", DUT: " + j;
						line += ", Current: " + slot_params[i].dut_current[j] + ", Temp: " + slot_params[i].dut_temp[j];
						line += Environment.NewLine;
					}//j for DUTs
				}//i for slot level
				writer.WriteLine(line); //change to be actual data
			}
		} //log relevant data to text files (lot reports)

		private void logging_worker_temp_dowork1(object sender, DoWorkEventArgs e)
		{
			if (!temp_report_file[0].Contains(".txt"))
				temp_report_file[0] = temp_report_path + (system_name[0] + "_" + lotnum[0] + "_" + datetime2 + "_" + duration[0] + ".txt");
			handle_log_files();
			//add the filename if the file path was changed
			Console.WriteLine("writing temp for chamber 1");

			using (StreamWriter writer = File.AppendText(temp_report_file[0]))
			{
				string line = datetime + ", Total: " + duration[0] + ", Step: " + elapsed_time[0];
				line += ", Oven Temp Chamber 1: " + oven_temp[0];
				line += Environment.NewLine;
				writer.WriteLine(line);
			}//write data
		} //log relevant data to text files (temp reports)

		private void logging_worker_lot_dowork2(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("writing lot for chamber 2");
			using (StreamWriter writer = File.AppendText(lot_report_file[1]))
			{
				string line = String.Empty;
				for (int i =8; i <16; i++)
				{
					line += datetime + ", Total: " + duration[1] + ", Step: " + elapsed_time[1];
					line += ", PSU0: " + slot_params[i].adc_measures[0] + ", PSU1: " + slot_params[i].adc_measures[1];
					line += ", BiB3V3: " + slot_params[i].adc_measures[2] + ", BiB5V0: " + slot_params[i].adc_measures[3];
					line += ", uC2V5: " + slot_params[i].adc_measures[4] + ", Current: " + slot_params[i].adc_measures[5];
					line += Environment.NewLine;
					for (int j = 0; j < 20; j++)
					{
						line += datetime + ", Total: " + duration[1] + ", Step: " + elapsed_time[1];
						line += ", Slot: " + i + ", DUT: " + j;
						line += ", Current: " + slot_params[i].dut_current[j] + ", Temp: " + slot_params[i].dut_temp[j];
						line += Environment.NewLine;
					}//j for DUTs
				}//i for slot level

				writer.WriteLine(line); //change to be actual data
			}
		} //log relevant data to text files (lot reports)

		private void logging_worker_temp_dowork2(object sender, DoWorkEventArgs e)
		{
			if (!temp_report_file[1].Contains(".txt"))
				temp_report_file[1] = temp_report_path + (system_name[1] + "_" + lotnum[1] + "_" + datetime2 + "_" + duration[1] + ".txt");
			handle_log_files();
			//add the filename if the file path was changed
			Console.WriteLine("writing temp for chamber 2");
			using (StreamWriter writer = File.AppendText(temp_report_file[1]))
			{
				string line = datetime + ", Total: " + duration[1] + ", Step: " + elapsed_time[1];
				line += ", Oven Temp Chamber 2: " + oven_temp[1];
				line += Environment.NewLine;
				writer.WriteLine(line);
			}//write data
		} //log relevant data to text files (temp reports)

		private void Logging_worker_alarm_DoWork2(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("writing alarm for chamber 2");
			datetime = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss:ff");
			try
			{
				if (header_lock[1] == true)
				{
					using (StreamWriter writer = File.AppendText(lot_report_file[1]))
					{
						string line = datetime + ", " + elapsed_time[1] + ", Slot: " + "" + ", " + alarm_msg[1];

						writer.WriteLine(line);
					}
				}
			}
			catch
			{
				BackgroundWorker tempworker = new BackgroundWorker();
				tempworker.DoWork += Logging_worker_alarm_DoWork2;
				System.Threading.Thread.Sleep(250);
				tempworker.RunWorkerAsync();
			}
		}

		private void Logging_worker_alarm_DoWork1(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("writing alarm for chamber 1");
			datetime = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss:ff");
			try
			{
				if (header_lock[0] == true) //means header has already been written
				{
					using (StreamWriter writer = File.AppendText(lot_report_file[0]))
					{
						string line = datetime + ", " + elapsed_time[0] + ", Slot: " + "" + ", " + alarm_msg[0];

						writer.WriteLine(line);
					}
				}
			}
			catch
			{
				BackgroundWorker tempworker = new BackgroundWorker();
				tempworker.DoWork += Logging_worker_alarm_DoWork1;
				System.Threading.Thread.Sleep(250);
				tempworker.RunWorkerAsync();
			}

		}

		private void background_worker_progress(object sender, ProgressChangedEventArgs e)
		{
			
		}

		private void background_worker_complete(object sender, RunWorkerCompletedEventArgs e)
		{

		}

		private BackgroundWorker create_remote_comm_worker()
		{
			BackgroundWorker remote_comm_worker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			remote_comm_worker.DoWork += new DoWorkEventHandler(remote_comm_worker_dowork);
			remote_comm_worker.ProgressChanged += new ProgressChangedEventHandler(remote_comm_worker_progress);
			remote_comm_worker.RunWorkerCompleted += remote_comm_worker_complete;

			return remote_comm_worker;
		}

		private BackgroundWorker create_logging_worker(string input, int chamber)
		{
			BackgroundWorker logging_worker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			if (chamber == 1)
			{
				if (input == "lot")
					logging_worker.DoWork += new DoWorkEventHandler(logging_worker_lot_dowork1);
				if (input == "temp")
					logging_worker.DoWork += new DoWorkEventHandler(logging_worker_temp_dowork1);
				if (input == "alarm")
					logging_worker.DoWork += Logging_worker_alarm_DoWork1;
			}
			if (chamber == 2)
			{
				if (input == "lot")
					logging_worker.DoWork += new DoWorkEventHandler(logging_worker_lot_dowork2);
				if (input == "temp")
					logging_worker.DoWork += new DoWorkEventHandler(logging_worker_temp_dowork2);
				if (input == "alarm")
					logging_worker.DoWork += Logging_worker_alarm_DoWork2;
			}
				//logging_worker.ProgressChanged += new ProgressChangedEventHandler(logging_worker_progress);
				//logging_worker.RunWorkerCompleted += logging_worker_complete;
				return logging_worker;
		}

		private void Zeroc_scanner_DoWork(object sender, DoWorkEventArgs e)
		{
			scan_zeroc_servers();
			//call_innovative_clickbox(0); //i put this into scan_zeroc_servers
		} 

		private void Step_sleep_timer_DoWork2(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("Sleeping chamber 2: " + current_step2.step_wait + " seconds");
			System.Threading.Thread.Sleep(current_step2.step_wait * 1000);
			step_sleep[1] = false;
		}

		private void Step_sleep_timer_DoWork1(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("Sleeping chamber 1: " + current_step1.step_wait + " seconds");
			System.Threading.Thread.Sleep(current_step1.step_wait * 1000);
			step_sleep[0] = false;
		}
		///BACKGROUND WORKERS ABOVE ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
		///VARIOUS FUNCTIONS BELOW |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


		private void scan_zeroc_servers()
		{
			List<string> InnovativeICEConnection = new List<string>(); //this is to store results of scan
			List<string> FuryICEConnection = new List<string>();

			if (File.Exists("ServicePublisher.dll")) // Check for the .dll file
			{
				try
				{
					//look for remote hosts
					Util.Query(InnovativeICEConnection, m_innovative_remoteConnections, "Innovative", "", 10099);
					Util.Query(FuryICEConnection, m_fury_remoteConnections, "fury", "", 10099);
				}
				catch (Exception ex)
				{
					MessageBox.Show("An error occurred while querying for ice connections: " + ex.Message);
				}
			}

			if (InnovativeICEConnection.SequenceEqual(ZeroC_connections))
				connect_to_zeroc = false; //dont need to reconnect
			else 
				connect_to_zeroc = true; //need to reconnect, dont do any zeroc stuff while this is happening

			timer_zeroc.Start(); //after initial scan, do a scan every 10 seconds(if stopped)

			ZeroC_connections = InnovativeICEConnection; //set what we need to connect to
			ZeroC_connections_fury = FuryICEConnection;

			call_innovative_clickbox(); //open all connections
		}

		private void do_sleep_timer(int chamber)
		{
			BackgroundWorker step_sleep_timer = new BackgroundWorker();
			if (chamber == 1)
				step_sleep_timer.DoWork += Step_sleep_timer_DoWork1;
			if (chamber == 2)
				step_sleep_timer.DoWork += Step_sleep_timer_DoWork2;
			step_sleep_timer.RunWorkerAsync();
		}

		private void log_alarm(string input, int chamber)
		{
			if (chamber == 1)
				alarm_msg[0] = input;
			if (chamber == 2)
				alarm_msg[1] = input;
			BackgroundWorker log_alarm = create_logging_worker("alarm", chamber);
			log_alarm.RunWorkerAsync();
		}


		private void call_innovative_clickbox() //should open connectoin to all available servers
		{
			string connection = null;
			string connection_fury = null;

			int index = 0;
			try
			{
				if (connect_to_zeroc)
				{
					for (int i = 0; i < 16; i++)
					{
						slot_params[i].m_innovativeConnected = false;
						slot_params[i].innovative_hal = null;
						slot_params[i].m_furyConnected = false;
						slot_params[i].fury_hal = null;
					} //blank out the connections before making them

					for (int i = 0; i < ZeroC_connections.Count; i++)
					{
						connection = ZeroC_connections[i];
						connection_fury = ZeroC_connections_fury[i];

						int dash_index = connection.IndexOf('-');
						string substring = connection.Substring(dash_index + 1, connection.Length - (dash_index + 1)); //make this based on the last IP octet later
						index = Int32.Parse(substring) - 64;

						Console.WriteLine("Connecting to " + connection);
						if (connection.Length == 0)
						{
							throw new Exception("Cannot connect without Ember connection type selection.");
						}

						if (m_innovative_remoteConnections.ContainsKey(connection))
						{
							m_innovative_interface = new InnovativeIceInterface(m_innovative_remoteConnections[connection]);
							connection = connection + ";" + m_innovative_remoteConnections[connection];
							slot_params[index].m_innovativeConnected = true; //set the slot connection register to true
						}

						if (m_fury_remoteConnections.ContainsKey(connection_fury))
						{
							m_fury_interface = new FuryIceInterface(m_fury_remoteConnections[connection_fury]);
							connection_fury = connection_fury + ";" + m_fury_remoteConnections[connection_fury];
							slot_params[index].m_furyConnected = true;
						}

						if (m_innovative_interface == null)
						{
							throw new Exception("ERROR: Unknown connection type.");
						}

						Innovative_HAL m_innovative_hal_temp = new Innovative_HAL(m_innovative_interface);
						X80QC m_fury_hal_temp = new X80QC(m_fury_interface);

						m_innovative_hal_temp.WRITEMAILBOX("BI", "1");
						m_innovative_hal_temp.Address(1);
						string news = "";
						try
						{
							news = System.Text.Encoding.ASCII.GetString(m_fury_hal_temp.ProductCode);
						}
						catch
						{ }
						//byte[] temp = { 0, 0, 0, 0 }; //what is this for? not sure
						
						slot_params[index].innovative_hal = m_innovative_hal_temp; //make this read the IP octet and then put in the right place in the array
						slot_params[index].fury_hal = m_fury_hal_temp;
					}
					BackgroundWorker EEPROM_reader = new BackgroundWorker();
					EEPROM_reader.DoWork += EEPROM_reader_DoWork; //also does mailbox and gpio setup
					EEPROM_reader.RunWorkerAsync(); //read board eeproms immediately after making connections
				}
				else
				{
					//system_InnovativeExceptionCleanUp(); //comment this out, dont want to remove connections just because were not opening new ones
				}
			}
			catch (Exception ex)
			{
				system_InnovativeExceptionCleanUp(index);
				if (ex.Message.StartsWith("Access is denied"))
				{
					MessageBox.Show(string.Format("Exception raised - Can't open {0} (Already in use by another program?).", connection), "");
				}
				else
				{
					MessageBox.Show(string.Format("Exception raised - ({0}).", ex.Message.Trim()), "");
				}
			}
		}//connecting to the zeroc server

		private void system_InnovativeExceptionCleanUp(int index) // Clean up the app when an exception is thrown
		{
			if (m_innovative_interface != null) // For interface
			{
				m_innovative_interface = null;
			}

			// Non specific clean-up
			//if(slot_params[index].innovative_hal != null)
			slot_params[index].innovative_hal = null;

			//innovativeConnectBox.Checked = false;
			slot_params[index].m_innovativeConnected = false;

			//EnableInnovativeWidgets(false);
		}

		private string trim_poll_results(string input)
		{
			input = Regex.Replace(input, @"\t|\n|\r", "");

			return input;
		}

		private void do_pretest(int chamber)
		{
			List<int> slots = new List<int>();// later on, implement how many slots we are pretesting for
			
			if (chamber == 1)
			{
				timer_zeroc.Enabled = false;
				pretest_lock[0] = true;

				Console.WriteLine("TURNING ON PSUS CHAMBER 1");
				//do chamber 1 controls

				for (int i = 0; i < 8; i++)
				{
					string mailbox_message = "PreTEST,T" + pretest_step1.temp_set + "C," + pretest_step1.ps_voltage_set[0] + "V," + pretest_step1.measure_interval + "M," + pretest_step1.step_time + "M";
					if (slot_params[i].m_innovativeConnected)
					{
						slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", mailbox_message); //write mailbox
						slot_params[i].innovative_hal.WRITEMAILBOX("BI", "10"); //write mailbox
						mailbox_ack_received[i] = false;

						psu_zeroc(1, 0, pretest_step1.ps_voltage_set[0].ToString(), i);
						psu_zeroc(1, 1, pretest_step1.ps_voltage_set[1].ToString(), i);
						string name = build_slot_file(i);//scan DUTs and create file

						send_file_name(i, name);
					}
				}
				button_pretest1.Hide();
				//button_start1.Show(); //moved this to state pretest loop
			}
			if (chamber == 2)
			{
				timer_zeroc.Enabled = false;
				pretest_lock[1] = true;
				string arguments = "SETPSU " + pretest_step2.ps_voltage_set[0].ToString() + " " + pretest_step2.ps_voltage_set[1].ToString();
				//do chamber 2 controls
				Console.WriteLine("TURNING ON PSUS CHAMBER 2");

				for (int i = 8; i < 16; i++)
				{
					string mailbox_message = "PreTEST,T" + pretest_step2.temp_set + "C," + pretest_step2.ps_voltage_set[0] + "V," + pretest_step2.measure_interval + "M," + pretest_step2.step_time + "M";
					if (slot_params[i].m_innovativeConnected)
					{
						slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", mailbox_message); //write mailbox
						slot_params[i].innovative_hal.WRITEMAILBOX("BI", "10"); //write mailbox
						mailbox_ack_received[i] = false;

						psu_zeroc(1, 0, pretest_step2.ps_voltage_set[0].ToString(), i);
						psu_zeroc(1, 1, pretest_step2.ps_voltage_set[1].ToString(), i);
						string name = build_slot_file(i);

						send_file_name(i, name);
					}
				}
				button_pretest2.Hide();
				//button_start2.Show();
			}
		}

		private void send_file_name(int slot, string file_path)
		{
			file_path = file_path.Replace("\\", "\\\\\\\\");
			var package = Tuple.Create(slot, file_path);

			BackgroundWorker file_sender = new BackgroundWorker();
			file_sender.DoWork += File_sender_DoWork;
			file_sender.RunWorkerAsync(package);
		} //just package and send the data into a background worker, which will keep going until the slot has receied ack for showing the file

		private void File_sender_DoWork(object sender, DoWorkEventArgs e)
		{
			var package = (Tuple <int,string>) e.Argument;

			int slot = package.Item1;
			string file_path = package.Item2;

			while (mailbox_ack_received[slot] == false)
			{
				System.Threading.Thread.Sleep(500);
			} //once we get an ack for this, then break out of loop
			
			slot_params[slot].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", file_path); //write filename in string exchange
			slot_params[slot].innovative_hal.WRITEMAILBOX("BI", "99"); //write flag for FILE into mailbox
			mailbox_ack_received[slot] = false;
			
			slot_params[slot].file_passed = true;
		} //this funciton keeps going until the python program acks the FILE portion of mailbox

		private void read_min_current()
		{

		}

		private string ZeroC_activate(string arguments, int slot_num) //obsolete funct
		{
			Process ZeroC = new Process();
			ZeroC.StartInfo.UseShellExecute = false;
			ZeroC.StartInfo.WorkingDirectory = "C:\\Users\\nabeelz\\Documents\\UltraComm FURY Controller\\ZeroC Server\\Example APP\\InnovativeICEApp\\InnovativeICEApp\\bin\\Debug";
			ZeroC.StartInfo.FileName = "C:\\Users\\nabeelz\\Documents\\UltraComm FURY Controller\\ZeroC Server\\Example APP\\InnovativeICEApp\\InnovativeICEApp\\bin\\Debug\\InnovativeICEApp.exe";
			ZeroC.StartInfo.CreateNoWindow = true;
			ZeroC.StartInfo.Arguments = arguments; //command, PS1, PS2 or command, DUT or command
			//later, add slot num into arguments to set the BBB its controlling
			ZeroC.StartInfo.RedirectStandardOutput = true;
			ZeroC.Start();
			//maybe set up arguments so there is a keyword to determine what to do. EG: ps_config, or ps_poll
			string output = ZeroC.StandardOutput.ReadToEnd();
			ZeroC.WaitForExit();

			return output;
		}

		private void psu_zeroc(int state, int phase, string voltage, int slot)
		{
			int chamber = (int)(slot / 8);

			log_alarm("RAMPING PSU" + phase + ": " + voltage + "V", chamber);

			byte[] volts_ba = new byte[4];
			volts_ba = convert_to_byte(voltage);
			
			psu_params psu_param = new psu_params(state, phase, voltage, slot, chamber, volts_ba);

			if (slot_params[slot].m_innovativeConnected && !slot_params[slot].psu_busy)
			{
				BackgroundWorker psu_worker = new BackgroundWorker();
				psu_worker.DoWork += Psu_worker_DoWork;
				psu_worker.RunWorkerAsync(psu_param);
			}
		}

		private void Psu_worker_DoWork(object sender, DoWorkEventArgs e)
		{
			psu_params psu_param = (psu_params)e.Argument;

			int state = psu_param.state;
			int phase = psu_param.phase;
			string voltage = psu_param.voltage;
			int slot = psu_param.slot;
			int chamber = psu_param.chamber;
			byte[] volts_ba = psu_param.volts_ba;

			slot_params[slot].psu_busy = true; //mark as busy so we cant go in

			Console.WriteLine("Backgroundworker for setting PSU" + phase);
			//throw new NotImplementedException();
			//implement later? to test with multiple
			
			slot_params[slot].innovative_hal.PSU(state, phase, volts_ba);

			var psu_reads = slot_params[slot].innovative_hal.PSUPOLL();
			var psu_reads_float = convert_to_float(psu_reads);

			slot_params[slot].psu_busy = false; //take off lock after doing set/poll

			if (state == 0)
			{
				if (psu_reads_float[phase] > .2)
				{
					System.Threading.Thread.Sleep(200);
					psu_retries++;
					if (psu_retries > 5)
						emergency_shutdown("CRITICAL ALARM: PSU " + slot + " COULD NOT BE SET, STOPPING", chamber);
					psu_zeroc(state, phase, voltage, slot);
				}
				else
					psu_retries = 0;
			}
			if (state == 1)
			{
				if (Math.Abs(psu_reads_float[phase] - float.Parse(voltage)) > .2)
				{
					System.Threading.Thread.Sleep(200);
					psu_retries++;
					if (psu_retries > 4)
						emergency_shutdown("CRITICAL ALARM: PSU " + slot + " COULD NOT BE SET, STOPPING", chamber);
					psu_zeroc(state, phase, voltage, slot);
				}
				else
					psu_retries = 0;
			}


		}

		private void initialize_elements()
		{
			for (int i = 0; i < 16; i++)
			{
				slot_params[i] = new slot_param(i);
			}

			button_list.Add(turnButton1);
			button_list.Add(turnButton2);
			button_list.Add(turnButton3);
			button_list.Add(turnButton4);
			button_list.Add(turnButton5);
			button_list.Add(turnButton6);
			button_list.Add(turnButton7);
			button_list.Add(turnButton8);
			button_list.Add(turnButton9);
			button_list.Add(turnButton10);
			button_list.Add(turnButton11);
			button_list.Add(turnButton12);
			button_list.Add(turnButton13);
			button_list.Add(turnButton14);
			button_list.Add(turnButton15);
			button_list.Add(turnButton16);
		}

		private float[] convert_to_float(byte[] ba)
		{
			float[] values = new float[6];

			values[0] = BitConverter.ToSingle(ba, 0);
			values[1] = BitConverter.ToSingle(ba, 4);
			values[2] = BitConverter.ToSingle(ba, 8);
			values[3] = BitConverter.ToSingle(ba, 12);
			values[4] = BitConverter.ToSingle(ba, 16);
			values[5] = BitConverter.ToSingle(ba, 20);

			return values;
		}

		private byte[] convert_to_byte(string volts)
		{
			byte[] byte_array;
			float volts_float = float.Parse(volts);

			byte_array = BitConverter.GetBytes(volts_float);

			return byte_array;
		}

		private void update_status()
		{
			datetime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ff");
			datetime2 = DateTime.Now.ToString("yyyyMMdd-HH.mm.ss");

			try
			{
				this.Invoke((MethodInvoker)delegate
				{
					for (int i = 0; i < 16; i++)
					{
						if (slot_params[i].m_innovativeConnected)
							button_list[i].Enabled = true;
						else
							button_list[i].Enabled = false;
					}

					label_state2.Text = state2;
					label_state1.Text = state1;

					if (label_state1.Text == "RUNNING")
						label_state1.ForeColor = System.Drawing.Color.YellowGreen;
					else if (label_state1.Text == "STOPPED")
						label_state1.ForeColor = System.Drawing.Color.Black;
					else if (label_state1.Text == "LOAD")
						label_state1.ForeColor = System.Drawing.Color.SlateBlue;
					else if (label_state1.Text == "CLEARING")
						label_state1.ForeColor = System.Drawing.Color.Red;
					else if (label_state1.Text == "PRETEST")
						label_state1.ForeColor = System.Drawing.Color.SteelBlue;

					if (label_state2.Text == "RUNNING")
						label_state2.ForeColor = System.Drawing.Color.YellowGreen;
					else if (label_state2.Text == "STOPPED")
						label_state2.ForeColor = System.Drawing.Color.Black;
					else if (label_state2.Text == "LOAD")
						label_state2.ForeColor = System.Drawing.Color.SlateBlue;
					else if (label_state2.Text == "CLEARING")
						label_state2.ForeColor = System.Drawing.Color.Red;
					else if (label_state2.Text == "PRETEST")
						label_state2.ForeColor = System.Drawing.Color.SteelBlue;

					if (loaded_check[0])
						textBox_chamber1.Text = "Recipe Loaded";
					else
						textBox_chamber1.Text = "";

					if (loaded_check[1])
						textBox_chamber2.Text = "Recipe Loaded";
					else
						textBox_chamber2.Text = "";
				});
			} //constantly update labels to match state
			catch { }

			step_num[0] = current_step1.step_no;
			step_num[1] = current_step2.step_no;
		}

		private bool check_bool(bool t_f, bool[] input, int chamber) //input if all true or all false desired, array to check, and chamber
		{
			int start = (8 * (chamber - 1));
			int stop = 8 + start;

			for (int i = start; i < stop; i++)
			{
				if (slot_params[i].m_innovativeConnected)
				{
					if (input[i] == !t_f) //if we are aiming for all trues, one opposite will return false, and vice versa
						return false;
				}
			}
			return true;
		}

		private void make_lot_header(int chamber)
		{
			if (chamber == 1)
			{
				if (!lot_report_file[0].Contains(".txt"))
					lot_report_file[0] = lot_report_path + (system_name[0] + "_" + lotnum[0] + "_" + datetime2 + "_" + duration[0] + ".txt");
			}
			if (chamber == 2)
			{
				if (!lot_report_file[1].Contains(".txt"))
					lot_report_file[1] = lot_report_path + (system_name[0] + "_" + lotnum[0] + "_" + datetime2 + "_" + duration[0] + ".txt");
			}
			handle_log_files();
			//add the filename if the file path was changed
			Console.WriteLine("writing header for chamber " + chamber + "at" + lot_report_path);
			using (StreamWriter writer = File.AppendText(lot_report_file[chamber-1]))
			{
				if (chamber == 1)
				{
					for (int i = 0; i < step_file_contents1.Count; i++)
					{
						writer.WriteLine(step_file_contents1[i].ToString()); //change to be actual data
						Console.WriteLine("writing header for lot 1 report: " + step_file_contents1[i].ToString());
					}
				}
				if (chamber == 2)
				{
					for (int i = 0; i < step_file_contents2.Count; i++)
					{
						writer.WriteLine(step_file_contents2[i].ToString()); //change to be actual data
						Console.WriteLine("writing header for lot 1 report: " + step_file_contents2[i].ToString());
					}
				}
				writer.WriteLine("Lot Number: " + lotnum[chamber - 1] + " , Job Number: " + jobnum[chamber - 1] + " , Part: " + partnum[chamber - 1]);
				writer.WriteLine("-----DateTime----,--Duration--,-------------------Measurements------------------");
			}
		}

		private void launch_form4(int chamber)
		{
			Form4 form4 = new Form4();
			form4.Text += (" - Chamber " + chamber.ToString());
			form4.ShowDialog();
		}

		private string build_slot_file(int slot)
		{
			string slot_file_path = "C:\\Fury\\SlotFiles\\";
			string file_name = slot + "_" + "18027654" + "_" + DateTime.Now.ToString("MMddyyyy") + "_" + "SlotFile" + ".csv"; //18027654 is work order
			int chamber = (int)(slot / 8);
			int ip = slot + 64; //hardcoded ip address scheme

			string[] serial_nums = new string[20]; //from firmware\

			string opcode = String.Empty; //from firmware
			int[] psu_map = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

			string write = "";
			try
			{
				using (var writer = new StreamWriter(slot_file_path + file_name))
				{
					write += ("Field,Value,PSU Phase" + Environment.NewLine);
					write += ("BBB_IP_ADDRESS, " + "192.168.121." + ip + Environment.NewLine);
					write += ("SLOT_NUMBER, " + (slot + 1) + Environment.NewLine);
					write += ("ICE_WORK_ORDER_ID, " + slot_params[slot].work_order + Environment.NewLine);
					write += ("ICE_DATALOG_FILE_ID," + Environment.NewLine);
					write += ("ICE_SOFTWARE_VERSION_ID," + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + Environment.NewLine);
					write += ("ICE_LOT_ID," + lotnum[chamber] + Environment.NewLine);
					write += ("JOB_START_TIME, " + DateTime.Now.ToString("MM/dd/yyyy hh:mm") + Environment.NewLine);
					write += ("DUT_OPCODE_ID, " + opcode + Environment.NewLine);
					write += ("DUT_PART_ID, " + partnum[chamber] + Environment.NewLine);
					write += ("DUT_CONTROLLER_ID, " + slot_params[slot].controller_id + Environment.NewLine);
					write += ("DUT_BOARD_ID, " + slot_params[slot].bib_id + Environment.NewLine);
					for (int i = 0; i < 20; i++)
					{
						write += ("DUT_SERIAL_ID_" + (i + 1) + "," + serial_nums[i] + "," + psu_map[i] + Environment.NewLine);
					}
					writer.Write(write);
				}
			}
			catch { }
			return slot_file_path + file_name;
		}

		private void read_step_file(int chamber)
		{
			Console.WriteLine("reading step file");
			if (chamber == 1)
			{
				read_step[0] = false;
			}
			if (chamber == 2)
			{
				read_step[1] = false;
			}

			string line;

			try
			{
				using (var read = new StreamReader(step_file_path+"\\" + step_file_name[chamber - 1]))
				{
				Console.WriteLine(step_file_path + "\\" + step_file_name[chamber-1]);
					while ((line = read.ReadLine()) != null)
					{
						//var values = line.Split(',');
						if(chamber == 1)
							step_file_contents1.Add(line);
						if(chamber == 2)
							step_file_contents2.Add(line);
					}
					if(chamber == 1)
						parse_step_file(step_file_contents1, chamber);
					if(chamber == 2)
						parse_step_file(step_file_contents2, chamber);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error in step file read/parse" + "-" + (step_file_path+"\\" + step_file_name[chamber - 1]) +  "\n" + e);

			}
		}

		private void parse_step_file(List<string> input, int chamber)
		{			
			int i;
			try
			{
				step_params pretest_step = new step_params();
				//read the header, and each step make a new step object and add it to the list
				for (i = 1; i < 6; i++)
				{
					

					string line = input[i];
					var pretest_entries = line.Split(',');

					if (i == 2)
					{
						pretest_step.ps_config_path = pretest_entries[4];
					} //ps config file for pretest
					if (i == 3)
					{
						pretest_step.mailbox_num = pretest_entries[4];
					} //file path for c# utility - pipe into remote comms
					if (i == 4)
					{
						pretest_step.mailbox_string = pretest_entries[4];
					}
					if (i == 5)
					{
						pretest_step.curr_config_path = pretest_entries[4];

						pretest_step.read_files_pretest();

						if (chamber == 1)
							pretest_step1 = pretest_step;
						if (chamber == 2)
							pretest_step2 = pretest_step;
					} //config file for finding min current required in pretest

				}//do pretest  stuff - only looking at first 6 lines


				int step_index;
				List<string[]> entries = new List<string[]>();

				for (i = 0; i < input.Count; i++)
				{
					string line = input[i];
					var entry = line.Split(',');
					entries.Add(entry);
				}//split up all lines and add to array for easy indexing
				for (i = 6; i < entries.Count; i++)
				{
					if (entries[i][1] == "Step Number")
					{
						step_index = i;
						step_params new_step = new step_params();

						new_step.step_no = Int32.Parse(entries[step_index][4]);

						new_step.ps_config_path = entries[step_index + 1][4];
						new_step.temp_config_path = entries[step_index + 2][4];
						new_step.curr_config_path = entries[step_index + 3][4];

						Console.WriteLine(new_step.ps_config_path);
						Console.WriteLine(new_step.temp_config_path);
						Console.WriteLine(new_step.curr_config_path);

						new_step.read_files();

						new_step.step_name = entries[step_index + 4][4];
						new_step.step_time = Int32.Parse(entries[step_index + 5][4]);
						if (new_step.step_time < 0)
							new_step.step_lock = true; //if it is -1, then set the bool to not automatically move on from step
						new_step.step_wait = Int32.Parse(entries[step_index + 6][4]);
						new_step.lot_log_interval = Int32.Parse(entries[step_index + 8][4]);
						new_step.measure_interval = Int32.Parse(entries[step_index + 9][4]);
						new_step.mailbox_num = entries[step_index + 12][4];
						new_step.mailbox_string = entries[step_index + 13][4];
						if (chamber == 1)
							step_list1.Add(new_step);
						if (chamber == 2)
							step_list2.Add(new_step);
					}//if "step name" found, use that as an index to add all data to a new step

				}
			}
			catch
			{
				Console.WriteLine("Error In parse function");
			}

			if (chamber == 2)
			{

			}
		}

		private void chamber_config_read()
		{
			string config_path = "C:\\Fury\\ConfigFiles\\UC_FURY.system_config.csv";

			string factory_email; //mike@, nabeelz@
			string factory_email_user;
			string factory_email_pass;

			var chamber_name = new string[2];
			var chamber_mfg = new string[2];
			var chamber_control_type = new string[2];
			var chamber_ip_add = new string[2]; //ip plus com port
			var chamber_comm_port = new string[2];
			
			var chamber_enable = new bool[2];

			string ftp_path;
			int chamber_count;

			try
			{
				using (var read = new StreamReader(config_path))
				{
					while (!read.EndOfStream)
					{
						string line = read.ReadLine();
						var values = line.Split(',');
						Console.WriteLine("Rading Line" + values[0]);

						if (values[0] == "FactoryName")
							label_factory.Text = values[3];
						if (values[0] == "FactoryEmail")
							factory_email = values[3];
						if (values[0] == "FactoryEmailUser")
							factory_email_user = values[3];
						if (values[0] == "FactoryEmailPw")
							factory_email_pass = values[3];
						if (values[0] == "StepFilePath")
							step_file_path = values[3];
						if (values[0] == "LotReportPath")
							lot_report_path = values[3] + "\\";
						if (values[0] == "TempReportPath")
							temp_report_path = values[3] + "\\";
						if (values[0] == "FTPUpload")
							ftp_upload_path = values[3];
						if (values[0] == "chambers")
							chamber_count = Int32.Parse(values[3]);
						if (values[0] == "CEnable-1")
						{
							chamber_enable[0] = Convert.ToBoolean(Convert.ToInt16(values[3]));
						}
						if (values[0] == "CName-1")
						{
							textbox_system1.Text = values[3];
							system_name[0] = values[3];
						}
						if (values[0] == "CController-1")
							chamber_control_type[0] = values[3];
						if (values[0] == "CAddress-1")
						{
							var address = values[3].Split(':');
							chamber_ip_add[0] = address[0];
							chamber_comm_port[0] = address[1];
						}
						if (values[0] == "CTempMax-1")
							max[0] = Int32.Parse(values[3]);
						if (values[0] == "CTempMin-1")
							min[0] = Int32.Parse(values[3]);
						if (values[0] == "CEnable-2")
							chamber_enable[1] = Convert.ToBoolean(Convert.ToInt16(values[3]));
						if (values[0] == "CName-2")
						{
							textbox_system2.Text = values[3];
							system_name[1] = values[3];
						}
						if (values[0] == "CController-2")
							chamber_control_type[1] = values[3];
						if (values[0] == "CAddress-2")
						{
							var address = values[3].Split(':');
							chamber_ip_add[1] = address[0];
							chamber_comm_port[1] = address[1];
						}
						if (values[0] == "CTempMax-2")
							max[1] = Int32.Parse(values[3]);
						if (values[0] == "CTempMin-2")
							min[1] = Int32.Parse(values[3]);

					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("No config file found" + "-" +  e);
			}
		}

		private void launch_monitor(int slot)
		{
			slot--;
			Form2 dut_monitor = new Form2(slot);
			dut_monitor.Text = "Slot " + slot;
			dut_monitor.Show();
		}

		private void launch_zeroc_scan(object sender, EventArgs e)
		{
			BackgroundWorker zeroc_scanner = new BackgroundWorker();
			zeroc_scanner.DoWork += Zeroc_scanner_DoWork;
			zeroc_scanner.RunWorkerAsync();
		}

		

		private void launch_temp_log1(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("temp", 1);
			logging_worker.RunWorkerAsync();
		}

		private void launch_lot_log1(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("lot", 1);
			logging_worker.RunWorkerAsync();
		}

		private void launch_temp_log2(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("temp", 2);
			logging_worker.RunWorkerAsync();
		}

		private void launch_lot_log2(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("lot", 2);
			logging_worker.RunWorkerAsync();
		}

		private void initialize_labels(int chamber)
		{

			string step_file_name_trim;

			if (step_file_name[chamber - 1].Length > 36)
				step_file_name_trim = step_file_name[chamber-1].Substring(0, 36);
			else step_file_name_trim = step_file_name[chamber-1];

			update_elapsed_time(chamber);

			Console.WriteLine("changing turnbutton text - Lotnum/Step File: " + lotnum[0] + "/" + step_file_name_trim);

			for (int i = (0 + (8 * (chamber - 1))); i < (8 + (8 * (chamber - 1))); i++) //0-8 if chamber 1, 8-16 if chamber 2
			{
				button_list[i].NewText = "File: " + step_file_name_trim + "\n" + "Lot: " + lotnum[chamber - 1] + " -- Job: " + jobnum[chamber - 1] + " -- Part: " + partnum[chamber - 1] + "\n\nStep: " + step_num[chamber - 1] + " -- Time Left: " + time_rem[chamber - 1];
				button_list[i].Refresh();
			} //redraw turnbutton labels
			
		}

		private void update_elapsed_time(int chamber)
		{
			string time;
			if (chamber == 1)
			{
				int seconds = (int)step_timer1.Elapsed.Seconds;
				int minutes = step_timer1.Elapsed.Minutes;
				int hours = step_timer1.Elapsed.Hours;
				int days = step_timer1.Elapsed.Days;
				time = days.ToString() + ":" + hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();
				elapsed_time[0] = time;
			}
			if (chamber == 2)
			{
				int seconds = (int)step_timer2.Elapsed.Seconds;
				int minutes = step_timer2.Elapsed.Minutes;
				int hours = step_timer2.Elapsed.Hours;
				int days = step_timer2.Elapsed.Days;
				time = days.ToString() + ":" + hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();
				elapsed_time[1] = time;
			}

		}

		private void update_tooltip_temp(int min, int low, int set, int high, int max, int select)
		{
			string min_str = "Min: " + min.ToString() + "\n";
			string low_str = "Low: " + low.ToString() + "\n";
			string set_str = "Set: " + set.ToString() + "\n";
			string high_str = "High: " + high.ToString() + "\n";
			string max_str = "Max: " + max.ToString();
			try
			{
				this.Invoke(new MethodInvoker(delegate ()
				{
					if(select == 1)
						tooltip_temp1.SetToolTip(this.label_temp1, min_str + low_str + set_str + high_str + max_str);
					if(select == 2)
						tooltip_temp2.SetToolTip(this.label_temp2, min_str + low_str + set_str + high_str + max_str);
				}));
			}
			catch
			{
			}
		}

		private void handle_log_files() //make sure path exists for log files (temp/log reports folder)
		{
			FileInfo lot = new FileInfo(lot_report_path);
			FileInfo temp = new FileInfo(temp_report_path);

			if (!lot.Directory.Exists)
				lot.Directory.Create();
			if (!temp.Directory.Exists)
				temp.Directory.Create();
		}

		private bool CheckOpened(string name)
		{
			FormCollection fc = Application.OpenForms;
			try
			{
				foreach (Form frm in fc)
				{
					if (frm.Text == name)
					{
						return true;
					}
				}
				return false;
			}
			catch
			{
				return false;
			}
		} //check if form 3 is open, because we need to update the interval if it is

		private int remove_alpha(string input)
		{
			int i = 0;
			while (i < input.Length)
			{
				if (!Char.IsDigit(input[i]))
				{
					input = input.Remove(i, 1);
					i--;
				}
				i++;
			}
			return Int32.Parse(input);
		}//remove chars from string

		private void emergency_shutdown(string alarm_msg, int chamber)
		{
			if (chamber == 1)
			{
				step_timer1.Stop();
				state1 = "STOPPED";
			}
			if (chamber == 2)
			{
				step_timer2.Stop();
				state2 = "STOPPED";
			}
			log_alarm(alarm_msg, chamber);
		}


///BUTTONS BELOW |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

		private void button1_Click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton1_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton2_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton3_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton4_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton5_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton6_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton7_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton8_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton9_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton10_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton11_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton12_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton13_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton14_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton15_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}
		private void turnButton16_click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
		}

		private void button_load_click(object sender, EventArgs e)
		{
			state1 = "LOAD";
			log_alarm("STATE: CHAMBER 1 LOAD", 1);
		}
		private void button_start_click(object sender, EventArgs e)
		{
			state1 = "RUNNING";
			step_timer1.Start(); //start timer
			log_alarm("STATE: CHAMBER 1 RUNNING", 1);

		}
		private void button_stop_click(object sender, EventArgs e)
		{
			state1 = "STOPPED";
			log_alarm("STATE: CHAMBER 1 STOP", 1);
		}
		private void button_clear_click(object sender, EventArgs e)
		{
			state1 = "CLEARING";
			log_alarm("STATE: CHAMBER 1 CLEARING", 1);
		}
		private void button_load2_Click(object sender, EventArgs e)
		{
			state2 = "LOAD";
			log_alarm("STATE: CHAMBER 2 LOAD", 2);
		}
		private void button_clear2_Click(object sender, EventArgs e)
		{
			state2 = "CLEARING";
			log_alarm("STATE: CHAMBER 2 CLEARING", 2);
		}
		private void button_start2_Click(object sender, EventArgs e)
		{
			state2 = "RUNNING";
			step_timer2.Start(); //start timer
			log_alarm("STATE: CHAMBER 2 RUNNING", 2);
		}
		private void button_stop2_Click(object sender, EventArgs e)
		{
			state2 = "STOPPED";
			log_alarm("STATE: CHAMBER 2 STOP", 2);
		}

		private void button_pretest1_Click(object sender, EventArgs e)
		{
			state1 = "PRETEST";
			if (pretest_lock[0] != true)
			{
				log_alarm("STATE: CHAMBER 1 PRETEST", 1);
				do_pretest(1);
			}
		}

		private void button_pretest2_Click(object sender, EventArgs e)
		{
			state2 = "PRETEST";
			if (pretest_lock[1] != true)
			{
				log_alarm("STATE: CHAMBER 2 PRETEST", 2);
				do_pretest(2);
			}
		}

		//OTHER GUI FUNCTS BELOW |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


		private void intervalsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (form3_lock == false)
			{
				Form3 settings_window = new Form3();
				settings_window.Text = "Settings";
				settings_window.ShowDialog();
			} //only if not already open
			//while form3 is open, update the interval.
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			System.Drawing.SolidBrush box_brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
			System.Drawing.Graphics form_graphics = e.Graphics;

			Rectangle one = new Rectangle(50, 405, 1000, 1);
			
			form_graphics.FillRectangle(box_brush, one);
			
			box_brush.Dispose();
			form_graphics.Dispose();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			slot_params[0].innovative_hal.WRITETEMP("25.0");
		}


		private void button1_Click_1(object sender, EventArgs e)
		{
			//
			string[] temps = new string[20];
			for (int i = 0; i < 20; i++)
			{
				slot_params[0].innovative_hal.Address(i+1);
				//temps[i] = slot_params[0].fury_hal..ToString();
				//System.Threading.Thread.Sleep(1000);
				//fws[0] = System.Text.Encoding.ASCII.GetString(slot_params[0].fury_hal.ProductCode);
			}

		}
	}
	//CLASS OVERLOADS BELOW ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
	class myLabel : System.Windows.Forms.Label
	{
		public int RotateAngle { get; set; }  // to rotate text
		public string NewText { get; set; }   // to draw text



		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			int mx = this.Size.Width / 2;
			int my = this.Size.Height / 2;

			SizeF size = e.Graphics.MeasureString(Text, Font);

			Brush b = new SolidBrush(this.ForeColor);
			e.Graphics.TranslateTransform(this.Width / 2, this.Height / 2);
			e.Graphics.RotateTransform(this.RotateAngle);
			e.Graphics.DrawString(this.NewText, this.Font, b, mx - (int)size.Width / 2, my - (int)size.Height / 2);
			base.OnPaint(e);
		}
	}

	class myButton : System.Windows.Forms.Button
	{
		public int RotateAngle { get; set; }  // to rotate text
		public string NewText { get; set; }   // to draw text


		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			int mx = this.Size.Width / 2;
			int my = this.Size.Height / 2;

			string temp = Text;
			Text = "";

			base.OnPaint(e);

			Brush b = new SolidBrush(this.ForeColor);
			Text = temp;
			SizeF size = e.Graphics.MeasureString(Text, Font);

			e.Graphics.TranslateTransform(0, 0);
			e.Graphics.RotateTransform(this.RotateAngle);
			e.Graphics.DrawString(this.Text, this.Font, b, mx - (int)size.Width / 2, my - (int)size.Height / 2);
			
		}
	}
	
	class TurnButton : Button
	{
		int angle = 90;   // current rotation
		Point oMid;      // original center
		public string NewText { get; set; }

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			if (oMid == Point.Empty) oMid = new Point(Left + Width / 2, Top + Height / 2);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			int mx = this.Size.Width / 2;
			int my = this.Size.Height / 2;
			Font new_font = new Font(this.Font.Name, this.Font.Size + 1.0F, FontStyle.Bold, this.Font.Unit);
			SizeF size = e.Graphics.MeasureString(NewText, new_font);
			//string t_ = Text;
			//Text = "";
			
			base.OnPaint(e);

			if (!this.DesignMode)
			{
				Brush b = new SolidBrush(this.ForeColor);
				//Text = t_;
				e.Graphics.TranslateTransform(mx, my);
				e.Graphics.RotateTransform(angle);
				e.Graphics.TranslateTransform(-mx, -my);

				e.Graphics.DrawString(NewText, new_font, b, mx - (int)size.Width / 2, my - (int)size.Height / 2);
			}
		}
	}//redefine class to be able to paint text sideway
/*
	public class lot_params //make new lot params each time you hit load
	{
		public  int logging_interval_lot = 5;
		public  int logging_interval_temp = 5;
		public  string lot_report_path = "C:/Users/Public/Documents/Lot_Reports/";
		public  string temp_report_path = "C:/Users/Public/Documents/Temp_Reports/";
		public  string step_file_path;
		public  string step_file_name = "NO STEP FILE";
		public  List<string> step_file_contents = new List<string>();
		
		public  string ftp_upload_path;

		string[] system_name = new string[2];
		public string lotnum = "123456";
		string datetime = DateTime.Now.ToString("MMddyy-HHmmss");
		string[] duration = new string[2];
		int[] step = new int[2];
		string[] time_rem = Enumerable.Repeat<string>("00D:00H:00M", 2).ToArray();

		public void initiate_read()
		{
			read_step_file(1);
		}

		private void read_step_file(int chamber)
		{
			Console.WriteLine("reading step file");
			if (chamber == 1)
			{
				read_step[0] = false;
			}
			if (chamber == 2)
			{
				read_step[1] = false;
			}

			string line;

			try
			{
				using (var read = new StreamReader(step_file_path + "\\" + step_file_name))
				{
					Console.WriteLine(step_file_path + "\\" + step_file_name);
					while ((line = read.ReadLine()) != null)
					{
						//var values = line.Split(',');
						if (chamber == 1)
							step_file_contents1.Add(line);
						if (chamber == 2)
							step_file_contents2.Add(line);
					}
					if (chamber == 1)
						parse_step_file(step_file_contents1, chamber);
					if (chamber == 2)
						parse_step_file(step_file_contents2, chamber);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error in step file read/parse" + "-" + (step_file_path + "\\" + step_file_name) + "\n" + e);

			}
		}

		private void parse_step_file(List<string> input, int chamber)
		{

			string c_sharp_path; //not implemented yet

			int i;
			try
			{
				step_params pretest_step = new step_params();
				//read the header, and each step make a new class and add it to the list
				for (i = 1; i < 5; i++)
				{


					string line = input[i];
					var pretest_entries = line.Split(',');

					if (i == 2)
					{
						pretest_step.ps_config_path = pretest_entries[4];
					} //ps config file for pretest
					if (i == 3)
					{
						c_sharp_path = pretest_entries[4];
					} //file path for c# utility - pipe into remote comms
					if (i == 4)
					{
						pretest_step.min_curr_config_path = pretest_entries[4];

						pretest_step.read_files_pretest();

						if (chamber == 1)
							pretest_step1 = pretest_step;
						if (chamber == 2)
							pretest_step2 = pretest_step;
					} //config file for finding min current required in pretest

				}//do pretest  stuff - only looking at first 5 lines


				int step_index;
				List<string[]> entries = new List<string[]>();

				for (i = 0; i < input.Count; i++)
				{
					string line = input[i];
					var entry = line.Split(',');
					entries.Add(entry);
				}//split up all lines and add to array for easy indexing
				for (i = 5; i < entries.Count; i++)
				{
					if (entries[i][1] == "Step Number")
					{
						step_index = i;
						step_params new_step = new step_params();

						new_step.step_no = Int32.Parse(entries[step_index][4]);

						new_step.ps_config_path = entries[step_index + 1][4];
						new_step.temp_config_path = entries[step_index + 2][4];
						Console.WriteLine(new_step.ps_config_path);
						new_step.read_files();

						new_step.step_name = entries[step_index + 3][4];
						new_step.step_time = Int32.Parse(entries[step_index + 4][4]);
						new_step.temp_wait = Int32.Parse(entries[step_index + 5][4]);
						new_step.lot_log_interval = Int32.Parse(entries[step_index + 7][4]);
						if (chamber == 1)
							step_list1.Add(new_step);
						if (chamber == 2)
							step_list2.Add(new_step);
					}//if "step name" found, use that as an index to add all data to a new step

				}
			}
			catch
			{
				Console.WriteLine("Error In parse function");
			}

			if (chamber == 2)
			{

			}
		}
	}
*/
	public class step_params
	{
		public string ps_config_path;
		public string curr_config_path;
		public string temp_config_path;
		public int step_no;
		public string step_name;
		public int step_time;
		public int step_wait;
		public int lot_log_interval;
		public int measure_interval;

		public string mailbox_num;
		public string mailbox_string;

		public float temp_low_flag;
		public float temp_high_flag;
		public float temp_min;
		public float temp_max;
		public float temp_set;

		public float current_min;
		public float current_set;
		public float current_max;

		public float[] ps_voltage_low_flag = new float[2];
		public float[] ps_voltage_high_flag = new float[2];
		public float[] ps_voltage_min = new float[2];
		public float[] ps_voltage_max = new float[2];
		public float[] ps_voltage_set = new float[2];

		public float[] ps_current_low_flag = new float[2];
		public float[] ps_current_high_flag = new float[2];
		public float[] ps_current_min = new float[2];
		public float[] ps_current_max = new float[2];
		public float[] ps_current_set = new float[2];

		public float dut_ps_current_low_flag;
		public float dut_ps_current_high_flag;
		public float dut_ps_current_min;
		public float dut_ps_current_max;
		public float dut_ps_current_set;

		public bool step_done = false;
		public bool step_lock = false;
		public bool temp_ramped = false;
		public bool temp_reached = false;
		public bool file_passed = false;

		public void read_files()
		{
			Console.WriteLine("Parse Files Step: " + step_no);

			read_ps_config();
			
			read_temp_config();

			read_curr_config();
		}

		public void read_files_pretest()
		{
			Console.WriteLine("Parse Files PreTest");

			read_ps_config();

			read_curr_config();
		}

		private void read_curr_config()
		{
			List<string> file = new List<string>();
			string line;
			Console.WriteLine("reading curr file" + " Path: " + ("C:/Fury/ConfigFiles/" + curr_config_path + ".csv"));
			using (var read = new StreamReader("C:/Fury/ConfigFiles/" + curr_config_path + ".csv"))
			{
				while ((line = read.ReadLine()) != null)
				{
					file.Add(line);
				}
			}

			var entry = file[3].Split(',');
			current_min = float.Parse(entry[3]); //min current - pretest
			current_set = float.Parse(entry[4]); //set current - expected/VCSEL
			current_max = float.Parse(entry[5]); //max current - pretest
		}

		private void read_ps_config()
		{
			List<string> file = new List<string>();
			string line;
			using (var read = new StreamReader("C:/Fury/ConfigFiles/" + ps_config_path + ".csv"))
			{
				while ((line = read.ReadLine()) != null)
				{
					file.Add(line);
				}
			}

			for (int i = 3; i < file.Count; i++)
			{
				var entries = file[i].Split(',');
				if (entries[1] == "VPS0")
				{
					ps_voltage_low_flag[0] = float.Parse(entries[7]);
					ps_voltage_high_flag[0] = float.Parse(entries[8]);
					ps_voltage_max[0] = float.Parse(entries[6]);
					ps_voltage_min[0] = float.Parse(entries[5]);
					ps_voltage_set[0] = float.Parse(entries[4]);
				}
				if (entries[1] == "VPS1")
				{
					ps_voltage_low_flag[1] = float.Parse(entries[7]);
					ps_voltage_high_flag[1] = float.Parse(entries[8]);
					ps_voltage_max[1] = float.Parse(entries[6]);
					ps_voltage_min[1] = float.Parse(entries[5]);
					ps_voltage_set[1] = float.Parse(entries[4]);
				}
				if (entries[1] == "IPS0")
				{
					ps_current_low_flag[0] = float.Parse(entries[7]);
					ps_current_high_flag[0] = float.Parse(entries[8]);
					ps_current_max[0] = float.Parse(entries[6]);
					ps_current_min[0] = float.Parse(entries[5]);
					ps_current_set[0] = float.Parse(entries[4]);
				}
				if (entries[1] == "IPS1")
				{
					ps_current_low_flag[1] = float.Parse(entries[7]);
					ps_current_high_flag[1] = float.Parse(entries[8]);
					ps_current_max[1] = float.Parse(entries[6]);
					ps_current_min[1] = float.Parse(entries[5]);
					ps_current_set[1] = float.Parse(entries[4]);
				}
				if (entries[1] == "IDUT")
				{
					dut_ps_current_low_flag = float.Parse(entries[7]);
					dut_ps_current_high_flag = float.Parse(entries[8]);
					dut_ps_current_max = float.Parse(entries[6]);
					dut_ps_current_min = float.Parse(entries[5]);
					dut_ps_current_set = float.Parse(entries[4]);
				}
			}
			Console.WriteLine("PS0 VOLTAGE: " + ps_voltage_set[0] + " PS1 VOLTAGE: " + ps_voltage_set[1]);
		}

		private void read_temp_config()
		{
			List<string> file = new List<string>();

			using (var read = new StreamReader("C:/Fury/ConfigFiles/" + temp_config_path + ".csv"))
			{
				while (!read.EndOfStream)
				{
					string line = read.ReadLine();
					file.Add(line);
				}
			}

			for (int i = 2; i < file.Count; i++)
			{
				var entries = file[i].Split(',');
				if (entries[0] == "Tchamber")
				{
					temp_low_flag = float.Parse(entries[6]);
					temp_high_flag = float.Parse(entries[7]);
					temp_min = float.Parse(entries[4]);
					temp_max = float.Parse(entries[5]);
					temp_set = float.Parse(entries[4]);
				}
			}

		}
	}//for 

	public class psu_params
	{
		public int state;
		public int phase;
		public string voltage;
		public int slot;
		public int chamber;
		public byte[] volts_ba;

		public psu_params(int state, int phase, string voltage, int slot, int chamber, byte[] volts_ba)
		{
			this.state = state;
			this.phase = phase;
			this.voltage = voltage;
			this.slot = slot;
			this.chamber = chamber;
			this.volts_ba = volts_ba;
		}
	} //used for passing all these parameters to backgroundworker in one object

	public class slot_param
	{
		public double[] dut_current = new double[20];
		public double[] dut_temp = new double[20];

		public double[] adc_measures = new double[6]; //psu0, psu1, ucref2.5, bib 3.3, bib 5.0

		public int slot_num;
		public Innovative_HAL innovative_hal = new Innovative_HAL(null);
		public X80QC fury_hal = new X80QC(null);

		public bool m_furyConnected;
		public bool m_innovativeConnected;

		public bool psu_status = false;
		public bool psu_busy = false;

		public bool file_passed = false;

		public string eeprom_controller = String.Empty;
		public string eeprom_BiB = String.Empty;

		public string work_order = String.Empty;
		public string controller_id = String.Empty;
		public string bib_id = String.Empty;
		

		public slot_param(int slot)
		{
			this.slot_num = slot;
		}

		public double[] do_adc_reads_float()
		{
			float constant = .0004394531F;
			short[] adc_reads = new short[6];
	
			adc_reads = innovative_hal.ADCPOLL();

			double[] adc_reads_float = new double[6];

			adc_reads_float[0] = (adc_reads[0] * 4 * constant * 1.025F);//phase 0
			adc_reads_float[1] = (adc_reads[1] * 4 * constant * 1.047F);//phase 1
			adc_reads_float[2] = (adc_reads[2] * 4 * constant * 1.0F);//bib 3v3
			adc_reads_float[3] = (adc_reads[3] * 4 * constant * 1.12F);//bib 5v0
			adc_reads_float[4] = (adc_reads[4] * 2.3F * constant * 1.05F);//uc2v5ref
			adc_reads_float[5] = (adc_reads[5] * 9.66F * constant * 1.0F);//amps

			if (adc_reads_float[0] > .5 & adc_reads_float[1] > .5)
				psu_status = true;
			else
				psu_status = false;

			return adc_reads_float;
		}

		public void read_eeprom_data()
		{
			eeprom_controller = innovative_hal.READMEMORY("0");
			eeprom_BiB = innovative_hal.READMEMORY("1");

			var controller = eeprom_controller.Split(';');
			var bib = eeprom_BiB.Split(';');

			foreach (string line in controller)
			{
				if (line.Contains("WO"))
				{
					int index = line.LastIndexOf(',') + 1;
					work_order = line.Substring(index, line.Length - index);
				}

				if (line.Contains("Serial Number"))
				{
					int index = line.LastIndexOf(',') + 1;
					controller_id = line.Substring(index, line.Length - index);
					controller_id = controller_id.Replace("ICE-", "");
				}
			}

			foreach (string line in bib)
			{
				if(line.Contains("Serial Number"))
				{
					int index = line.LastIndexOf(',') + 1;
					bib_id = line.Substring(index, line.Length - index);
					bib_id = controller_id.Replace("ICE-", "");
				}
			}

		}

	}//used to store each slot's parameters
}
