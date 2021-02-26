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
using EasyModbus;

namespace UltraComm_Burn_In_GUI
{
	public partial class Form1 : Form
	{
		bool form3_lock = false;
		public static bool edit_lock = true; //for changing path in form 3 - settings
		bool[] header_lock = new bool[2];
		bool[] pretest_lock = new bool[2];
		public static bool[] loaded_check = Enumerable.Repeat<bool>(false, 2).ToArray();
		public static bool[] form4_lock = Enumerable.Repeat<bool>(false, 2).ToArray();
		public static bool[] read_step = new bool[2];
		bool[] step_sleep = new bool[2];
		bool[] step_transition = Enumerable.Repeat<bool>(true, 2).ToArray();
		bool connect_to_zeroc = false;
		//locks for safety/function

		public static int logging_interval_lot = 5;
		public static int logging_interval_temp = 5;
		public static string lot_report_path = "C:/Users/Public/Documents/Lot_Reports/";
		string[] lot_report_file = Enumerable.Repeat<string>("", 2).ToArray();
		public static string temp_report_path = "C:/Users/Public/Documents/Temp_Reports/";
		string[] temp_report_file = Enumerable.Repeat<string>("", 2).ToArray();
		public static string step_file_path;
		public static string slot_file_path;
		public static string[] step_file_name = Enumerable.Repeat<string>("NO STEP FILE", 2).ToArray();
		public static List<string> step_file_contents1 = new List<string>();
		public static List<string> step_file_contents2 = new List<string>();
		public static string ftp_upload_path;
		//step items


		string[] system_name = new string[2];
		public static string[] lotnum = Enumerable.Repeat<string>("000000", 2).ToArray();
		public static string[] jobnum = Enumerable.Repeat<string>("000000", 2).ToArray();
		public static string[] partnum = Enumerable.Repeat<string>("000000", 2).ToArray();
		public static List<string> partnum_list = new List<string>();
		string datetime = DateTime.Now.ToString("MM//dd//yy-HH:mm:ss");
		string datetime2 = DateTime.Now.ToString("yyyyMMdd-HH.mm.ss");//special for filename
		string[] elapsed_time = new string[2];
		int[] step_num = new int[2];
		string[] time_rem = Enumerable.Repeat<string>("00D:00H:00M", 2).ToArray();
		string[] step_time = Enumerable.Repeat<string>("00H:00M", 2).ToArray();
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
		int[] slot_ps_alarm = Enumerable.Repeat<int>(0, 16).ToArray();
		int alarm_reset_counter = 0;

		string[] alarm_msg = new string[2];
		string[] alarm_type = new string[2];

		public static List<step_params> step_list1 = new List<step_params>();
		public static List<step_params> step_list2 = new List<step_params>();
		public static step_params pretest_step1 = new step_params();
		public static step_params pretest_step2 = new step_params();
		//public static step_params current_step1 = new step_params();
		//public static step_params current_step2 = new step_params();
		public static step_params[] current_step = new step_params[2];
		public static slot_param[] slot_params = new slot_param[16];

		Stopwatch step_timer1 = new Stopwatch();
		Stopwatch step_timer2 = new Stopwatch();
		Stopwatch burn_in_timer1 = new Stopwatch();
		Stopwatch burn_in_timer2 = new Stopwatch();
		Stopwatch psu_timer = new Stopwatch();

		List<TurnButton> button_list = new List<TurnButton>();
		List<Label> label_bibsn_list = new List<Label>();
		//ZeroC stuff
		List<string> ZeroC_connections = new List<string>(); //this is to save what connections we have
		List<string> ZeroC_connections_fury = new List<string>();
		bool[] zeroc_Connected = Enumerable.Repeat<bool>(false, 16).ToArray();
		Dictionary<string, string> m_innovative_remoteConnections = new Dictionary<string, string>();        // id, endoint
		Dictionary<string, string> m_fury_remoteConnections = new Dictionary<string, string>();        // id, endoint
		InnovativeInterface m_innovative_interface;
		FuryInterface m_fury_interface;
		X80QC m_x80_hal;
		//mailbox stuff
		string[] PACKAGE_EXCHANGE = new string[16];
		string[] BI = Enumerable.Repeat<string>("0", 16).ToArray();
		string[] UC = Enumerable.Repeat<string>("0", 16).ToArray();
		string[] ACK = Enumerable.Repeat<string>("0", 16).ToArray();
		bool[] mailbox_wait_for_clear = new bool[16];
		bool[] mailbox_ack_received = new bool[16];
		bool[] mailbox_ready_received = new bool[16];

		//Hardware variables
		oven_params[] oven_param = new oven_params[2];
		int[] oven_alarm = Enumerable.Repeat<int>(0, 2).ToArray();

		//public static List<lot_params> lot_list = new List<lot_params>();


		public static slot_param[] slotparamextra = new slot_param[16];
		public static bool[] gpioreset = Enumerable.Repeat<bool>(false, 16).ToArray();
		public static bool[] gpioset = Enumerable.Repeat<bool>(false, 16).ToArray();
		public static bool[] ucreset = Enumerable.Repeat<bool>(false, 16).ToArray();
		public static bool[] checkbib = Enumerable.Repeat<bool>(false, 16).ToArray();
		public static bool[] bibready = Enumerable.Repeat<bool>(false, 16).ToArray();
		public static int tracker1 = 0;
		public static int tracker2 = 1;

		public static bool[] gpiosetdut = Enumerable.Repeat<bool>(false, 16).ToArray();
		public static bool[] gpioresetdut = Enumerable.Repeat<bool>(false, 16).ToArray();
		//int skip1 = 0;
		public static bool finishedscan = true;
		public static bool finishedscanslots = true;

		//Essentially the "MAIN" for this entire Form1.
		public Form1() //entry point
		{
			InitializeComponent();
			//below this for main slot display

			ThreadPool.GetMinThreads(out int threads, out int completion_threads);
			ThreadPool.SetMinThreads(100, completion_threads); //this sets available threads, so we can poll ZeroC on all slots instantly
			initialize_elements(); //initialize slot params array, and turnbutton list

			chamber_config_read();


			initialize_labels(1);
			initialize_labels(2);

			scan_zeroc_servers(); //look for zeroc servers, every 10 seconds -> maybe move this into after the gui comes up?
			maintain_zeroc_connection_list(); //to enable button

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


			BackgroundWorker mailbox_handler = new BackgroundWorker();
			mailbox_handler.DoWork += Mailbox_handler_DoWork;
			mailbox_handler.RunWorkerAsync(); //mailbox stuff

			BackgroundWorker oven_worker = new BackgroundWorker();
			oven_worker.DoWork += Oven_worker_DoWork;
			oven_worker.RunWorkerAsync();

			BackgroundWorker bib_detector = new BackgroundWorker();
			bib_detector.DoWork += Bib_detector_DoWork;
			bib_detector.RunWorkerAsync(); //keep scanning for bibs, and if we find one then read eeprom, set bools

			notready:
			//Circulates till Bibs have all been checked.
			if (tracker1 == tracker2)
			{
				Console.WriteLine("All bibs ready");
				goto isready;
			}
			else
			{
				Console.WriteLine("All bibs not ready");
				Thread.Sleep(1000);
				goto notready;
			}
			isready:

			for (int i = 0; i < 16; i++)
			{
				BackgroundWorker remote_comm_worker = create_remote_comm_worker();
				remote_comm_worker.RunWorkerAsync(i); //how often to poll data from ZeroC
			}

			update_tooltip_temp(min[0], low[0], set[0], high[0], max[0], 1); //update tooltip1 upon start
			update_tooltip_temp(min[1], low[1], set[1], high[1], max[1], 2); //update tooltip1 upon start

			temp_log_timer1.Interval = logging_interval_temp * 60000;
			lot_log_timer1.Interval = logging_interval_lot * 60000;
			psu_timer.Restart(); //this constantly counts, everytime psu is used it restarts.

			while (true)
			{

				update_status(); //constantly update certain display stuff

				//Chamber 1

				if (state1 == "RUNNING")
				{
					edit_lock = true;
					if (oven_param[0].heat() != 1 || oven_param[0].fan() != 1)
					{
						oven_param[0].fan_on();
						System.Threading.Thread.Sleep(2000);
						oven_param[0].heat_on();
					}//for resuming from stop condition



					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
						{
							if (!slot_params[i].psu_status_bib)
								slot_params[i].innovative_hal.PSUBIB(1);//if not on for some reason, make sure bib power is on.

							if (slot_params[i].psu_status != 1 && !slot_params[i].psu_busy && psu_timer.ElapsedMilliseconds > 5000) //if already setting, dont go in again
							{
								psu_zeroc(1, 0, pretest_step1.ps_voltage_set[0].ToString(), i);
								psu_zeroc(1, 1, pretest_step1.ps_voltage_set[1].ToString(), i);
								slot_params[i].toggle_firmwareEN(true);
								psu_timer.Restart(); //makes wait 5000ms
													 //check_psu_then_reset(i);
													 //psu_zeroc(0, 2, "0.0", i);
							}

						} //make sure psu's are on
					}

					if (step_transition[0])
					{
						Console.WriteLine("Finding new step");
						for (int i = 0; i < step_list1.Count; i++)
						{
							if (step_list1[i].step_done == false)
							{
								current_step_index[0] = i;
								current_step[0] = step_list1[i];

								break; //stop loop as soon as we find the first unfinished step -> stops the for loop
							}
						}// scan through steps, find step without step done

						step_transition[0] = false;
					}// only do this if we are in transition

					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Show();
						button_load1.Hide();
						button_clear1.Hide();
						button_start1.Hide();
						button_pretest1.Hide();
						label1.Hide();
						label7.Hide();

						if (temp_log_timer1.Enabled == false)
							temp_log_timer1.Enabled = true; //start logging at correct interval
						if (lot_log_timer1.Enabled == false)
							lot_log_timer1.Enabled = true;
						//temp_log_timer1.Interval = 30000;// current_step1.lot_log_interval * (1000) * 60;
						//lot_log_timer1.Interval = 20000;//current_step1.lot_log_interval * (1000) * 60;

						//need if condition with temp_ramped bool, and a loop to only start once we have the wait_for_clear bit set
						if ((current_step[0].temp_ramped == 0) && check_bool(false, mailbox_wait_for_clear, 1) && check_bool(true, slot_params.Select(x => x.m_innovativeConnected).ToArray(), mailbox_ready_received, 1) && !step_sleep[0]) //no outstanding clear we are waiting on, and also havent ramped yet
						{
							Console.WriteLine("NEW STEP - " + current_step[0].step_no);
							log_alarm("STEP TRANSITION: MOVING ON TO STEP " + current_step[0].step_no + "\n", "Chamber: 1", 0); //record moving steps
							BackgroundWorker logging_worker = create_logging_worker("temp", 0);
							logging_worker.RunWorkerAsync();
							Console.WriteLine("STARTING TEMP RAMP");

							for (int i = 0; i < 8; i++)
							{
								if (slot_params[i].m_innovativeConnected && slot_params[i].enabled && BI[i] == "5") //BI will be 5, after having acked the end of FILE step for Python
								{
									mailbox_ack_received[i] = false; //reset this, so we can see the next statements get acked
									slot_params[i].innovative_hal.WRITEMAILBOX("READY", "0"); //reset this so we get a new one
									slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", "RampingTemperature");
									slot_params[i].innovative_hal.WRITEMAILBOX("BI", "4"); //BUSY
									slot_params[i].python_busy = false; //put this back to false, done with prev step.
								}
							}

							float old = -100.00F;
							if (oven_param[0].connected())
							{
								while (oven_param[0].return_temp_set() != current_step[0].temp_set || oven_param[0].fan() != 1 || oven_param[0].heat() != 1)
								{
									oven_param[0].fan_on();//fan on oven
									oven_param[0].heat_on();//heater on oven

									old = oven_param[0].return_temp_set();

									oven_param[0].set_temp(current_step[0].temp_set);  //set_temp on oven, it should start heating up
								}//if the settings didnt stick for some reason, keep trying
								log_alarm("SETTING TEMP TO " + current_step[0].temp_set + " IN CHAMBER 1", "STATE", 0);
							}

							//determine whether we are heating up or cooling down
							if (old == -100.00F) //should only happen if not connected
							{
								current_step[0].temp_ramped = -1;
								textBox_chamber1.Text = "Oven Not Connected 1";
							}
							else if (current_step[0].temp_set < old) //cooling down
							{
								current_step[0].temp_ramped = 2;
								textBox_chamber1.Text = "Ramping Down Temperature";
							}
							else if (current_step[0].temp_set > old) //heating up
							{
								current_step[0].temp_ramped = 1;
								textBox_chamber1.Text = "Ramping Temperature";
							}
							else
							{
								current_step[0].temp_ramped = -1;
								textBox_chamber1.Text = "Oven Not Connected 2";
							}
							//oven not connected, or no change

						} //oven control condition


						if (check_bool(true, mailbox_ack_received, 1) && (current_step[0].temp_ramped != 0) && !step_timer1.IsRunning) //received ack from temp_ramped activation
						{
							//Console.WriteLine("TEMP IS RAMPING");
							//check temp, make sure it is at appropriate temp -> exported to oven backgroundworker
							//keep going in here until temp is reached

							for (int i = 0; i < 8; i++) //might need to put delay here so it doesn't use old values
							{
								if (UC[i] != "5") //UC will be 5 after acking the mailbox_num and then being cleared
									mailbox_ack_received[i] = false; //need to wait on ack for the oven ramp to move on, so reset
							}

							//might not even need this routine tbh.
						}
						//now we wait, in the open loop, until we satisfy below conditions
						string box = current_step[0].mailbox_string;
						if (current_step[0].temp_reached && !step_timer1.IsRunning)//use this statement once we reach temp, to restart timer; only happens once
						{
							step_timer1.Start();
							Console.WriteLine("TEMP HAS REACHED, \"ramping temp\" has been acked, start timer");
							for (int i = 0; i < 8; i++)
							{
								if (slot_params[i].m_innovativeConnected && slot_params[i].enabled) //UC will be 5 after acking the mailbox_num and then being cleared
								{
									mailbox_ready_received[i] = false; //make sure this is false, so we get a new one
									slot_params[i].innovative_hal.WRITEMAILBOX("READY", "0"); //reset this so we get a new one
									slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", current_step[0].mailbox_string);
									slot_params[i].innovative_hal.WRITEMAILBOX("BI", current_step[0].mailbox_num); //write burn in step #

									if (box.Contains("-1M"))
										slot_params[i].python_busy = true;

									log_alarm("TEMP REACHED, SENT MAILBOX INFORMATION TO BEAGLEBONE", "SLOT: " + (i + 1), 0);
								}
							}
							textBox_chamber1.Text = current_step[0].step_name;
						}

						//step lock releases upon seeing a READY
						double time1 = step_timer1.Elapsed.TotalMinutes, time2 = current_step[0].step_time;
						if (((time1 > time2) && !current_step[0].step_lock) || (time1 + 10 > time2 && box.Contains("Burn"))) //step lock is to stop moving on when on untimed steps //second part for when burn-in gets stuck
						{
							Console.WriteLine("STEP TRANSITIONING");

							step_sleep[0] = true;
							step_transition[0] = true;
							step_timer1.Stop(); //stop and reset timer after step complete
							step_timer1.Reset();

							step_list1[current_step_index[0]].step_done = true; //will make loop set current step to the next step
																				//unramp_power_supplies(1);       //mark step as done

							do_sleep_timer(1);

							for (int i = 0; i < 8; i++)
							{
								if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
									mailbox_ack_received[i] = false;
							}

							if (current_step[0].last_step) //this only happens if we come upon the last step
							{
								shutdown("FINISHED ALL STEPS, GOING TO DONE CONDITION", "STATE", 0);
								for (int j = 0; j < 8; j++)
								{
									if (slot_params[j].m_innovativeConnected && slot_params[j].enabled)
									{
										slot_params[j].innovative_hal.WRITEMAILBOX("BI", "1");
										slot_params[j].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", "TestComplete");
									}                               //set mailbox to done
								}
								//also, step transition stays true for next run.
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



				}//state RUNNING - run chamber 1 functionality
				else if (state1 == "STOPPED")
				{
					header_lock[0] = false;
					edit_lock = false;
					step_transition[0] = true;

					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Hide();
						button_load1.Show();
						button_clear1.Show();
						button_start1.Hide();
						button_pretest1.Hide();

						if (loaded_check[0])
						{
							//button_start1.Show();
							button_stop1.Hide();
							button_load1.Hide();
							button_pretest1.Hide();
						}//if restarting
						//if (current_step[0].last_step) //if finished
							//button_start1.Hide();
					});//if state is set to stopped

					timer_zeroc.Enabled = true; //scan for zeroc servers
					lot_log_timer1.Enabled = false; //stop logging
					temp_log_timer1.Enabled = false;
					if (oven_param[0].connected())
					{
						if (oven_param[0].heat() != 0)
							oven_param[0].heat_off();
					}
					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
						{
							if (slot_params[i].psu_status != 0 && !slot_params[i].psu_busy && psu_timer.ElapsedMilliseconds > 5000) //if already setting, dont go in again
							{
								//psu_zeroc(0, 0, "0.0", i);
								//psu_zeroc(0, 1, "0.0", i);
								//slot_params[i].toggle_firmwareEN(false);
								psu_zeroc(0, 2, "0.0", i);
								psu_timer.Restart();
							}

							if (slot_params[i].psu_status_bib)
								slot_params[i].innovative_hal.PSUBIB(0); //turn off bib power in stopped state, if on
						}

					} //make sure psu's are off
				}
				else if (state1 == "LOAD")
				{
					current_step[0] = pretest_step1; //set current step to pretest step
					this.Invoke((MethodInvoker)delegate
					{
						button_stop1.Hide();
						button_load1.Hide();
						button_clear1.Show();
						button_start1.Show();
						label7.Show();
						label1.Show();

						if (!check_bool(false, zeroc_Connected, 1)) //at least one true makes check_bool return a false, if we set it to look for a full false array
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

						button_pretest1.Enabled = true;

						current_step[0] = new step_params();
						step_file_name[0] = "NO STEP FILE";
						lot_report_file[0] = "";
						temp_report_file[0] = "";
						lotnum[0] = "000000";
						jobnum[0] = "000000";
						partnum[0] = "000000";
						step_num[0] = 0;
						step_file_contents1.Clear();
						step_list1.Clear();
						//clear actual parameters in data structure as well

						step_timer1.Reset();
						burn_in_timer1.Reset();

						if (oven_param[0].connected())
						{
							if (oven_param[0].return_temp_set() != 35.0F)
								oven_param[0].set_temp(35.0F); //room temp
						}

						initialize_labels(1);
						state1 = "STOPPED";
					});

					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].m_innovativeConnected)
						{
							slot_params[i].innovative_hal.WRITEMAILBOX("UC", "0");
							slot_params[i].innovative_hal.WRITEMAILBOX("BI", "0");
						}

						unblacklist_slot(i);
					}
				}
				else if (state1 == "PRETEST")
				{
					bool[] chamber1ready = Enumerable.Repeat<bool>(true, 8).ToArray();
					//bool[] chamber2ready = Enumerable.Repeat<bool>(true, 8).ToArray();
					if (header_lock[0] != true)
						make_lot_header(1);

					header_lock[0] = true;


					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
						{
							if (mailbox_ack_received[i] == false)   //make sure we have an ack for pretest before moving to start
							{
								chamber1ready[i] = false;
							}
							if (slot_params[i].psu_busy) //ack and psu is not busy
							{
								chamber1ready[i] = false;
							}
							if (slot_params[i].file_passed == false)
							{
								chamber1ready[i] = false;
							}
						}
					} //go through all slots connected in chamber 1. if any do not display ack bit, then block start


					Console.WriteLine("---------------------------"+chamber1ready[0]+" "+chamber1ready[1] + " " + chamber1ready[2] + " " + chamber1ready[3] + " " + chamber1ready[4] + " " + chamber1ready[5] + " " + chamber1ready[6] + " " + chamber1ready[7]);
					int totalready = 0;
					for (int i = 0; i < chamber1ready.Length; i++) {
						if (chamber1ready[i])
						{
							totalready++;
							Console.WriteLine("Total ready "+totalready);
						}
						else
						{
							Console.WriteLine("---------------------------Chamber 1 slot " + i + " is not ready.");
						}
						if(totalready == 8)
						{
							this.Invoke((MethodInvoker)delegate
							{
								button_start1.Show();
							});
						}
					
					}

					for (int i = 0; i < 8; i++)
					{
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
						{
							if (!slot_params[i].psu_status_bib)
								slot_params[i].innovative_hal.PSUBIB(1);//if not on for some reason, make sure bib power is on.

							if (slot_params[i].psu_status != 1 && !slot_params[i].psu_busy && psu_timer.ElapsedMilliseconds > 5000) //if already setting, dont go in again
							{
								//psu_zeroc(1, 0, pretest_step1.ps_voltage_set[0].ToString(), i);
								//psu_zeroc(1, 1, pretest_step1.ps_voltage_set[1].ToString(), i);
								psu_timer.Restart();
								//psu_zeroc(0, 2, "0.0", i);
							}

						} //make sure psu's are on
					}

					
				}





				//CHAMBER TWO

				if (state2 == "RUNNING")
				{
					edit_lock = true;

					for (int i = 0; i < step_list2.Count; i++)
					{
						if (step_list2[i].step_done == false)
						{
							current_step_index[1] = i;
							current_step[1] = step_list2[i];
							break; //stop loop as soon as we find the first unfinished step
						}
					}

					this.Invoke((MethodInvoker)delegate
					{
						button_stop2.Show();
						button_load2.Hide();
						button_clear2.Hide();
						button_start2.Hide();

						temp_log_timer2.Enabled = true;//start logging
						lot_log_timer2.Enabled = true;
						temp_log_timer2.Interval = current_step[1].lot_log_interval * (1000) * 60;
						lot_log_timer2.Interval = current_step[1].lot_log_interval * (1000) * 60;

						if (step_timer2.Elapsed.TotalMinutes > current_step[1].step_time)
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
					step_transition[1] = true;

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
						if (current_step[1].last_step) //if finished
							button_start2.Hide();
					});//if state is set to stopped

					timer_zeroc.Enabled = true;
					lot_log_timer2.Enabled = false;
					temp_log_timer2.Enabled = false;

					if (oven_param[1].connected())
					{
						if (oven_param[1].heat() != 0)
							oven_param[1].heat_off();
					}

					for (int i = 8; i < 16; i++)
					{
						if (slot_params[i].psu_status != 0 && !slot_params[i].psu_busy && psu_timer.ElapsedMilliseconds > 5000)
						{
							//psu_zeroc(0, 0, "0.0", i);
							//psu_zeroc(0, 1, "0.0", i);
							psu_zeroc(0, 2, "0.0", i);
							psu_timer.Restart(); //once we do an operation, reset it to wait 10 more sec
						}
					} //make sure psu's are off					
				}
				else if (state2 == "LOAD")
				{
					current_step[1] = pretest_step2; //have to run pretest
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

						button_pretest2.Enabled = true;

						current_step[1] = new step_params();
						step_file_name[1] = "NO STEP FILE";
						lot_report_file[1] = "";
						temp_report_file[1] = "";
						lotnum[1] = "000000";
						jobnum[1] = "000000";
						partnum[1] = "000000";
						step_num[1] = 0;

						step_file_contents2.Clear();
						step_list2.Clear();
						//clear actual parameters in data structure as well

						step_timer2.Reset();
						burn_in_timer2.Reset();
						if (oven_param[1].connected())
						{
							if (oven_param[1].return_temp_set() != 35.0F)
								oven_param[1].set_temp(35.0F); //room temp
						}

						initialize_labels(2);
						state2 = "STOPPED";

					});

				}

				//THIS IS WHERE YOU CAN MAKE THE START HAPPEN AFTER THE PY SCRIPTS ARE GOOD
				else if (state2 == "PRETEST")
				{
					bool check = true;

					if (header_lock[1] != true)
						make_lot_header(2);

					header_lock[1] = true;

					for (int i = 8; i < 16; i++)
					{
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
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
							button_start2.Show();
						});
					}

					for (int i = 8; i < 16; i++)
					{
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
						{
							if (!slot_params[i].psu_status_bib)
								slot_params[i].innovative_hal.PSUBIB(1);//if not on for some reason, make sure bib power is on.

							if (slot_params[i].psu_status != 1 && !slot_params[i].psu_busy && psu_timer.ElapsedMilliseconds > 5000) //if already setting, dont go in again
							{
								psu_zeroc(1, 0, pretest_step2.ps_voltage_set[0].ToString(), i);
								psu_zeroc(1, 1, pretest_step2.ps_voltage_set[1].ToString(), i);
								psu_timer.Restart();
								//psu_zeroc(0, 2, "0.0", i);
							}

						} //make sure psu's are on
					}
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

		///BACKGROUND WORKERS BELOW THIS \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


		private void Oven_worker_DoWork(object sender, DoWorkEventArgs e)
		{

			for (int i = 0; i < 2; i++)
			{

				Console.WriteLine("Connecting to Chamber " + i + " oven");
				try
				{
					oven_param[i].modbus_client.IPAddress = oven_param[i].ip;
					oven_param[i].modbus_client.Port = Int32.Parse(oven_param[i].port);
				}
				catch
				{
					Console.WriteLine("Incorrect connection parameters, chamber" + i);
				}//assign the ip address and port

				try
				{
					oven_param[i].modbus_client.Connect();
				}
				catch
				{
					Console.WriteLine("Could not connect to chamber " + i);
				}//connect to oven

			}

			int[] counter = Enumerable.Repeat<int>(0, 2).ToArray();
			Random random = new Random();

			while (true)
			{
				int randomNumber = random.Next(0, 1000);
				for (int i = 0; i < 2; i++) //current step and oven_param should both refer to chamber
				{
					float current_temp;
					float set_temp;
					if (oven_param[i].connected()) //will skip all of this if oven not connected
					{
						current_temp = oven_param[i].temp_current();
						set_temp = oven_param[i].return_temp_set();
						
						if (current_temp == -1)
							continue; //if we read back a bad temp reading, then skip this iteration
						oven_param[i].oven_string = current_temp.ToString();

						for (int j = i * 8; j < ((i + 1) * 8); j++)
						{
							if (slot_params[j].m_innovativeConnected && slot_params[j].enabled)
								slot_params[j].innovative_hal.WRITETEMP(oven_param[i].oven_string);
						}//write it into the mailbox, correct slots

						if ((state1 == "RUNNING" && i == 0) || (state2 == "RUNNING" && i == 1))//if running, then display alarms in correct chamber
						{
							if (set_temp != current_step[i].temp_set)
								oven_param[i].set_temp(current_step[i].temp_set); //if the temps dont match for some reason, then set the oven

							alarm_check_oven(i, current_temp); //log alarms

							string alarm = oven_param[i].report_alarms(); //make alarm reports using internal function (build string and returns, utilize in log alarm below)

							if (alarm.Length > 0)
							{
								alarm = oven_param[i].report_alarms(); //double check the alarms immediately, to remove false reports
								log_alarm("ALARM: OVEN ALARM(S) REPORTED - " + alarm, "CHAMBER: " + (i + 1), i);
							}

							if (!current_step[i].temp_reached)
							{
								//check for the oven temp, if it matches the step then set the bool
								if (Math.Abs(current_temp - current_step[i].temp_set) < 3.0)
								{
									Console.WriteLine("TEMP MATCH FOR STEP, INCREMENTING COUNTER");
									counter[i]++;
								}
								else if (counter[i] > 0) //if not in temp range, and counter is already counted up then reset because temp left range
								{
									Console.WriteLine("TEMP LEFT RANGE AFTER ENTERING, CLEARING COUNTER");
									counter[i] = 0;
								}

								if (counter[i] > 3) //30 sec of stable temp
								{
									Console.WriteLine("TEMP MATCHED 4 TIMES, SET TEMP REACHED");
									current_step[i].temp_reached = true;
									counter[i] = 0;
								}
							} //if we are currently ramping, check if the temp is close enough
						}
						else if (oven_param[i].temp < 40.0) //this is the "idle" temp
						{
							oven_param[i].heat_off();
							oven_param[i].fan_off();
						}//turn fan off, we are cooled to room (during not running)
						else
							oven_param[i].fan_on();

					}//scan for temp in here, also error codes and conditionals
				}

				System.Threading.Thread.Sleep(9000 + randomNumber);
			}
		}

		private void Bib_detector_DoWork(object sender, DoWorkEventArgs e)
		{

			while (true)
			 {
				 bool[] change = new bool[16];
				 for (int i = 0; i < 16; i++)
				 {
					 if (slot_params[i].m_innovativeConnected && slot_params[i].m_furyConnected && slot_params[i].enabled)
					 {
						 Console.WriteLine("BiB detector scanning..");
						 bool old_state = slot_params[i].bib_present;// old state before this scan
						 int result = slot_params[i].check_bib(i);//use zeroc to check i2c register
						 bool new_state;
						Thread.Sleep(100);
						 if (result == 1)
						 {
							new_state = true;
							Form1.bibready[i] = true;
						 }
						 else if (result == 0)
						 {
							new_state = false;
							Form1.bibready[i] = false;
						 }
						 else    //result = -1 (invalid read or error)
						 {
							 new_state = old_state; //do nothing basically, wait for next scan
							 try
							 {
								 this.Invoke((MethodInvoker)delegate
								 {
									 label_bibsn_list[i].Text = "Reseat BiB";
								 });
							 }
							 catch { };
						 }

						 slot_params[i].bib_present = new_state; //set new state into variable, after saving old state (redundant, sets in funct)

						 if (new_state != old_state) //see whether we need to do a scan, if the old state and new state are different
							 change[i] = true;
					 }//if the zeroc server is there
					
				 }//scan through all slots				 

				System.Threading.Thread.Sleep(10000); //10 secs between scans
				int cur = 0;
				int index1 = 0;

				while (index1 != -1)
				{
					index1 = Array.IndexOf(checkbib, true, cur, checkbib.Length - cur);
					if (index1 != -1)
					{
						Console.WriteLine("Slots checked: " + index1);
					}
					cur = index1 + 1;
					tracker1++;
				}
				cur = 0;
				index1 = 0;
				while (index1 != -1)
				{
					index1 = Array.IndexOf(bibready, true, cur, bibready.Length - cur);
					if (index1 != -1)
					{
						Console.WriteLine("Slots ready: " + index1);
					}
					cur = index1 + 1;
					tracker2++;
				}
				tracker2--;

				if (tracker1 == tracker2)
				{
					for (int i = 0; i < 16; i++)
					{
						if (change[i] && slot_params[i].bib_present && slot_params[i].m_innovativeConnected) //check bib_present even though its set above, redundant check
						{
							this.UseWaitCursor = true;
							//CNboolloop
							//retry:
							//if (finishedscanslots)
							//{
								slot_params[i].scan_slot();
							//}
							//else {
							//	Thread.Sleep(250);
							//	goto retry;
							//}
							this.UseWaitCursor = false;
						}
						//if previously determined this slot was just populated
						else if (change[i] && !slot_params[i].bib_present) //if it changes, but the other way (unplug)
						{
							Console.WriteLine("BIB UNPLUGGED");
							slot_params[i].bib_id = "";
							slot_params[i].eeprom_BiB = string.Empty;
						}
					}
				}

			}//keep scanning*/
		}
		 /*
		 public void Slot_scanner_DoWork(object sender, DoWorkEventArgs e) //moved into the slot param object
		 {
			 int i = (int)e.Argument;
			 slot_params[i].reset_uc(); //reset the UC before scanning
			 Console.WriteLine("BIB DETECTED: SLOT " + (i + 1) + " , DOING BIB DETECT PROCEDURE (firmware+eeprom)");
			 this.UseWaitCursor = true;
			 slot_params[i].innovative_hal.PSUBIB(1); //turn on BIB power
													  //System.Threading.Thread.Sleep(5000);//250 + (i*15)); //make sure they are on first, give it some time
													  //do eeprom read of bib, scan for dut_present
			 for (int j = 0; j < 20; j++)
			 {
				 Console.WriteLine("Scanning dut present for dut " + (j + 1) + " slot " + (i + 1));
				 //slot_params[i].innovative_hal.Address(j + 1); //old, slow way
				 set_gpio(i, slot_params[i].gpio_pins, j + 1); //new way
				 System.Threading.Thread.Sleep(20);
				 try
				 {
					 slot_params[i].fury_hal.FirmwareEn = true; //enable fury controls
					 if (Convert.ToBoolean(slot_params[i].fury_hal.DUTPRESENT))
					 {
						 Console.WriteLine("DUT FOUND SITE " + (j + 1));
						 slot_params[i].dut_present[j] = true;//fill dut present array with dut presrent valu
					 }
					 else
						 slot_params[i].dut_present[j] = false;

				 }
				 catch (Ice.UnknownException)
				 {
					 System.Threading.Thread.Sleep(50);
					 try
					 {
						 slot_params[i].fury_hal.FirmwareEn = true; //enable fury controls
						 slot_params[i].dut_present[j] = Convert.ToBoolean(slot_params[i].fury_hal.DUTPRESENT); //fill dut present array with dut presrent valu
					 }
					 catch
					 {
						 MessageBox.Show("DUT " + (j + 1) + " scan slot " + (i + 1) + " failed... click slot to check DUTs for accuracy before proceeding.");
					 }
				 }
			 }//scan through duts
			 slot_params[i].read_eeprom_bib();
			 //slot_params[i].innovative_hal.PSUBIB(0); //turn off BIB power, in case operator disconnects it
			 slot_params[i].psu_status_bib = false; //should be off after this!
			 this.UseWaitCursor = false;
		 }
		 */



			private void initial_zeroc_DoWork(object sender, DoWorkEventArgs e)
		{
			int[] temp_array = { 1, 1, 1, 1, 1 }; //this is to tell the set_gpio function to set all pins

			for (int i = 0; i < 16; i++)
			{
				if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
				{
					Console.WriteLine("setting mailbox, dut select to default state");
					slot_params[i].innovative_hal.WRITEMAILBOX("BI", "1");
					slot_params[i].innovative_hal.WRITEMAILBOX("UC", "0");

					set_gpio(i, temp_array, 0);

				}//write mailbox to 1, set address to 0
			}
			for (int i = 0; i < 16; i++)
			{
				if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
				{
					Console.WriteLine("Reading eeproms for slot " + i);
					slot_params[i].read_eeprom_controller();
				}//if controller is present, read controller only (not bib)
			}
		}

		private void Mailbox_handler_DoWork(object sender, DoWorkEventArgs e)
		{
			Random random = new Random();
			int number = 4000;
			while (true)
			{
				int randomNumber = random.Next(0, 1000);
				System.Threading.Thread.Sleep(number + randomNumber);

				for (int i = 0; i < 16; i++) //scan through all 16 BBB
				{
					int chamber = (int)((i / 8));
					/*
					if (chamber == 0)
					{
						if (state1 == "RUNNING")
							number = 9000;
						else
							number = 3000;
					}
					else if (chamber == 1)
					{
						if (state2 == "RUNNING")
							number = 9000;
						else
							number = 3000;
					}
					*/ //doesnt work because number gets set to 9000 unless both are running
					if (slot_params[i].m_innovativeConnected && slot_params[i].enabled) //if the slot being iterated upon is connected, proceed to read
					{
						Console.WriteLine("Reading mailbox for slot " + (i + 1));

						if (!slot_params[i].psu_busy)
						{
							UC[i] = slot_params[i].innovative_hal.READMAILBOX("UC");
							BI[i] = slot_params[i].innovative_hal.READMAILBOX("BI");
						}

						if (UC[i] == "" || UC[i] == "5") //empty represents first cycle of clearing after PRETEST, 5 represents a clear from the Python
						{
							ACK[i] = slot_params[i].innovative_hal.READMAILBOX("ACK"); //check to make sure we got the ACK
																					   //slot_params[i].innovative_hal.WRITEMAILBOX("BI", "");
																					   //slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", "");
																					   //slot_params[i].innovative_hal.WRITEMAILBOX("UC", "5");
																					   ///clear mailboxes upon sending clear command -> moved to firmware

							System.Threading.Thread.Sleep(400); //wait for 1s, so we can have a "clear" state for Python to detect. THen do FILE.

							if (ACK[i] == "1")
							{
								mailbox_ack_received[i] = true;
								//slot_params[i].innovative_hal.WRITEMAILBOX("UC", "");
								slot_params[i].innovative_hal.WRITEMAILBOX("ACK", "0");
							}
						} //we wont see the UC at 2, because in mailbox.sh it automatically sets the 2 to a 5.

						if (BI[i] == "5") //an ack from BI has been cleared, responding to BUSY or READY
						{
							mailbox_wait_for_clear[i] = false; //Python has cleared the ack, we can proceed
							if (!mailbox_ready_received[i])
							{
								string result = slot_params[i].innovative_hal.READMAILBOX("READY");
								if (result == "1")
								{
									mailbox_ready_received[i] = true;
									slot_params[i].innovative_hal.WRITEMAILBOX("READY", "0"); //reset it
								}
							}

							if (check_bool(true, slot_params.Select(x => x.m_innovativeConnected).ToArray(), mailbox_ready_received, chamber+1) && current_step[chamber].temp_reached) //temp reached ensures we have passed oven ramp
								current_step[chamber].step_lock = false;
						}


						if (UC[i] == "3")//if we see a 3, that means time-based measurement is done. ack and move on from step
						{
							current_step[chamber].step_lock = false;

							///slot_params[i].innovative_hal.WRITEMAILBOX("BI", "2"); //send ack -> moved to firmware
							mailbox_wait_for_clear[i] = true;   //wait for the clear
						}/// obsolete 
						if (UC[i] == "4") //if we see a 4, need to acknowledge
						{
							///slot_params[i].innovative_hal.WRITEMAILBOX("BI", "2"); //send ack -> moved to firmware
							mailbox_wait_for_clear[i] = true; //wati for the clear
						}/// obsolete 

						if (UC[i] == "-1")
						{
							log_alarm("ALARM: MAILBOX REPORTED FAILURE (-1) FROM PYTHON", "SLOT: " + (i + 1), chamber);
							Console.WriteLine("ALARM: MAILBOX REPORTED FAILURE (-1) FROM PYTHON SLOT: " + (i + 1));
							blacklist_slot(i);                      //also stop the process
						}
						if (BI[i] == "-1")
						{
							string alarm = slot_params[i].innovative_hal.READMAILBOX("PACKAGE_EXCHANGE");
							log_alarm("ALARM: MAILBOX REPORTED FAILURE (-1) FROM BURN-IN: " + alarm, "SLOT: " + (i + 1), chamber);
							Console.WriteLine("ALARM: MAILBOX REPORTED FAILURE (-1) FROM BURN-IN SLOT: " + (i + 1) + alarm);
							blacklist_slot(i);                      //also stop the process
						}
					}
				}
			}
		}

		private void remote_comm_worker_dowork(object sender, DoWorkEventArgs e)
		{
			float constant = .0004394531F;
			var now = DateTime.Now.ToLocalTime().ToString();
			int slot = (int)e.Argument;
			Random random = new Random();
			int chamber = (int)(slot / 8);
			
			while (true)
			{
				try
				{
					int randomNumber = random.Next(0, 1000);
					System.Threading.Thread.Sleep(9000 + randomNumber);
					//System.Threading.Thread.Sleep(9000+(slot*1000));
					//Console.WriteLine("I am remote worker for ZeroC slot polling, Slot " + slot);
					if (slot_params[slot].m_innovativeConnected && slot_params[slot].enabled)
					{

						slot_params[slot].adcpsumeasurements(slot);


						if (state1 == "RUNNING" && slot < 8)
						{
							alarm_check_adc(slot, 1);
							alarm_check_dut(slot, 0); //pass chamber as 0 based
						}//alarm checks
						if (state2 == "RUNNING" && slot > 8)
						{
							alarm_check_adc(slot, 2);
							alarm_check_dut(slot, 1); //pass chamber as 0 based
						}//alarm checks


					}
				}
				catch (Exception x)
				{
					Console.WriteLine("Error in remote worker, slot " + (slot + 1) + ": " + x.Message);
				}
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
					if (zeroc_Connected[i])
					{
						line += Environment.NewLine + "MEASUREMENTS: SLOT " + (i + 1);
						line += Environment.NewLine;
						line += datetime + ", Total: " + elapsed_time[0] + ", Step: " + step_time[0];
						line += ", PSU0: " + slot_params[i].adc_measures[0] + ", PSU1: " + slot_params[i].adc_measures[1];
						line += ", BiB3V3: " + slot_params[i].adc_measures[2] + ", BiB5V0: " + slot_params[i].adc_measures[3];
						line += ", uC2V5: " + slot_params[i].adc_measures[4] + ", Current: " + slot_params[i].adc_measures[5];

						for (int j = 0; j < 20; j++)
						{
							if (slot_params[i].dut_present[j])
							{
								line += Environment.NewLine;
								line += datetime + ", Total: " + elapsed_time[0] + ", Step: " + step_time[0];
								line += ", Slot: " + (i + 1) + ", DUT: " + (j + 1);
								line += ", Current: " + slot_params[i].dut_current[j] + ", Temp: " + slot_params[i].dut_temp[j];
							}//filter inactive DUTs
						}//j for DUTs
					}//filter inactive slots
				}//i for slot level
				writer.WriteLine(line); //change to be actual data
			}
		} //log relevant data to text files (lot reports)

		private void logging_worker_temp_dowork1(object sender, DoWorkEventArgs e)
		{
			if (!temp_report_file[0].Contains(".txt"))
				temp_report_file[0] = temp_report_path + (system_name[0] + "_" + datetime2 + "_" + lotnum[0] + "_" + jobnum[0] + "_" + partnum[0] + ".txt");
			handle_log_files();
			//add the filename if the file path was changed
			Console.WriteLine("writing temp for chamber 1");

			using (StreamWriter writer = File.AppendText(temp_report_file[0]))
			{
				string line = datetime + ", Total: " + elapsed_time[0] + ", Step: " + step_time[0];
				line += ", Oven Temp Chamber 1: " + oven_param[0].oven_string;
				//line += Environment.NewLine;
				writer.WriteLine(line);
			}//write data
		} //log relevant data to text files (temp reports)

		private void logging_worker_lot_dowork2(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("writing lot for chamber 2");
			using (StreamWriter writer = File.AppendText(lot_report_file[1]))
			{
				string line = String.Empty;
				for (int i = 8; i < 16; i++)
				{
					if (zeroc_Connected[i])
					{
						line += Environment.NewLine + "MEASUREMENTS: SLOT " + (i + 1);
						line += Environment.NewLine;
						line += datetime + ", Total: " + elapsed_time[1] + ", Step: " + step_time[1];
						line += ", PSU0: " + slot_params[i].adc_measures[0] + ", PSU1: " + slot_params[i].adc_measures[1];
						line += ", BiB3V3: " + slot_params[i].adc_measures[2] + ", BiB5V0: " + slot_params[i].adc_measures[3];
						line += ", uC2V5: " + slot_params[i].adc_measures[4] + ", Current: " + slot_params[i].adc_measures[5];
						for (int j = 0; j < 20; j++)
						{
							if (slot_params[i].dut_present[j])
							{
								line += Environment.NewLine;
								line += datetime + ", Total: " + elapsed_time[1] + ", Step: " + elapsed_time[1];
								line += ", Slot: " + (i + 1) + ", DUT: " + (j + 1);
								line += ", Current: " + slot_params[i].dut_current[j] + ", Temp: " + slot_params[i].dut_temp[j];
							}//filter inactive DUTs
						}//j for DUTs
					}//filter inactive slots
				}//i for slot level

				writer.WriteLine(line); //change to be actual data
			}
		} //log relevant data to text files (lot reports)

		private void logging_worker_temp_dowork2(object sender, DoWorkEventArgs e)
		{
			if (!temp_report_file[1].Contains(".txt"))
				temp_report_file[1] = temp_report_path + (system_name[1] + "_" + datetime2 + "_" + lotnum[1] + "_" + jobnum[1] + "_" + partnum[1] + ".txt");
			handle_log_files();
			//add the filename if the file path was changed
			Console.WriteLine("writing temp for chamber 2");
			using (StreamWriter writer = File.AppendText(temp_report_file[1]))
			{
				string line = datetime + ", Total: " + elapsed_time[1] + ", Step: " + step_time[1];
				line += ", Oven Temp Chamber 2: " + oven_param[1].oven_string;
				//line += Environment.NewLine;
				writer.WriteLine(line);
			}//write data
		} //log relevant data to text files (temp reports)

		private void Logging_worker_alarm_DoWork2(object sender, DoWorkEventArgs e)
		{
			datetime = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss:ff");
			try
			{
				if (header_lock[1] == true)
				{
					Console.WriteLine("writing alarm for chamber 2 - " + alarm_msg[1]);
					using (StreamWriter writer = File.AppendText(lot_report_file[1]))
					{
						string line = datetime + ", Total: " + elapsed_time[1] + ", " + alarm_type[1] + ", " + alarm_msg[1];

						writer.WriteLine(line);
					}
				}
			}
			catch (System.ArgumentException)
			{
				Console.WriteLine("cleared while writing lot.. skip");
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
			datetime = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss:ff");
			try
			{
				if (header_lock[0] == true) //means header has already been written, so past pretest initialization
				{
					Console.WriteLine("writing alarm for chamber 1 - " + alarm_msg[0]);
					using (StreamWriter writer = File.AppendText(lot_report_file[0]))
					{
						string line = datetime + ", Total: " + elapsed_time[0] + ", " + alarm_type[0] + ", " + alarm_msg[0];

						writer.WriteLine(line);
					}
				}
			}
			catch (System.ArgumentException)
			{
				Console.WriteLine("cleared while writing lot.. skip");
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
			if (chamber == 0)
			{
				if (input == "lot")
					logging_worker.DoWork += new DoWorkEventHandler(logging_worker_lot_dowork1);
				if (input == "temp")
					logging_worker.DoWork += new DoWorkEventHandler(logging_worker_temp_dowork1);
				if (input == "alarm")
					logging_worker.DoWork += Logging_worker_alarm_DoWork1; //this adds to the file, above 2 are automatic
			}
			if (chamber == 1)
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

			maintain_zeroc_connection_list();
		}

		private void Step_sleep_timer_DoWork2(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("Sleeping chamber 2: " + current_step[1].step_wait + " seconds");
			System.Threading.Thread.Sleep(current_step[1].step_wait * 1000);
			step_sleep[1] = false;
		}

		private void Step_sleep_timer_DoWork1(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine("Sleeping chamber 1: " + current_step[0].step_wait + " seconds");
			System.Threading.Thread.Sleep(current_step[0].step_wait * 1000);
			step_sleep[0] = false;
		}
		///BACKGROUND WORKERS ABOVE ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
		///VARIOUS FUNCTIONS BELOW |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

		//Literally does what it says, Scans for servers.
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

			InnovativeICEConnection.Sort();
			ZeroC_connections.Sort();

			if (InnovativeICEConnection.SequenceEqual(ZeroC_connections))
				connect_to_zeroc = false; //dont need to reconnect
			else
				connect_to_zeroc = true; //need to reconnect, dont do any zeroc stuff while this is happening
			
			//WHere does this come from? what the hell
			timer_zeroc.Start(); //after initial scan, do a scan every 10 seconds(if stopped)

			ZeroC_connections = InnovativeICEConnection; //set what we need to connect to
			ZeroC_connections_fury = FuryICEConnection;

			call_innovative_clickbox(); //open all connections
		}

		//Checks to see if the connections to the innovative and fury are still available.
		private void maintain_zeroc_connection_list()
		{
			for (int i = 0; i < slot_params.Length; i++)
			{
				if (slot_params[i].m_innovativeConnected && slot_params[i].m_furyConnected)
					zeroc_Connected[i] = true;
			}
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

		private void log_alarm(string input, string alarm, int chamber)
		{
			if (chamber == 0)
			{
				alarm_msg[0] = input;
				alarm_type[0] = alarm;
			}
			if (chamber == 1)
			{
				alarm_msg[1] = input;
				alarm_type[1] = alarm;
			}
			BackgroundWorker log_alarm = create_logging_worker("alarm", chamber);
			log_alarm.RunWorkerAsync();
		}

		private void alarm_check_oven(int chamber, float current_temp)
		{
			if (current_step[chamber].temp_ramped == 2 || current_step[chamber].temp_reached)//if heating, dont look for low violation
			{
				if (current_temp < current_step[chamber].temp_min)
				{
					log_alarm("CRITICAL ALARM: MIN UNDERTEMP VALUE REACHED( " + current_temp + ")", "CHAMBER:" + (chamber + 1), chamber);
					oven_alarm[chamber]++;
				}
				else if (current_temp < current_step[chamber].temp_low_flag)
					log_alarm("ALARM: TEMP LOW FLAG REACHED( " + current_temp + ")", "CHAMBER: " + (chamber + 1), chamber);
			}

			if (current_step[chamber].temp_ramped == 1 || current_step[chamber].temp_reached)//if cooling, dont look for high violation
			{
				if (current_temp > current_step[chamber].temp_max)
				{
					log_alarm("CRITICAL ALARM: MAX OVERTEMP VALUE REACHED( " + current_temp + ")", "CHAMBER:" + (chamber + 1), chamber);
					oven_alarm[chamber]++;
				}
				else if (current_temp > current_step[chamber].temp_high_flag)
					log_alarm("ALARM: TEMP HIGH FLAG REACHED( " + current_temp + ")", "CHAMBER: " + (chamber + 1), chamber);
			}

			if (current_step[chamber].temp_ramped != 0)//if in steady state, and we are in a good temp, then set alarm counter to 0.
			{
				if ((Math.Abs(current_temp - oven_param[chamber].temp_desired)) < 3)
					oven_alarm[chamber] = 0;
			}

			if (oven_alarm[chamber] > 3)
				emergency_shutdown("TOO MANY CRITICAL ALARMS: OVEN, SHUTTING DOWN", "CHAMBER:" + (chamber + 1), chamber);
		}

		private void alarm_check_adc(int slot, int chamber)
		{
			chamber -= 1; //change to zero based

			//ps0 and ps1 alarms - voltage
			if (slot_params[slot].adc_measures[0] < current_step[chamber].ps_voltage_min[0]) //ps0 min
			{
				log_alarm("CRITICAL ALARM: UNDERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[0] + ")", "SLOT:" + (slot + 1) + " PS:0", chamber);
				slot_ps_alarm[slot]++;
			}
			else if (slot_params[slot].adc_measures[0] < current_step[chamber].ps_voltage_low_flag[0]) //ps0 low flag
				log_alarm("ALARM: UNDERVOLTAGE FLAG REACHED(" + slot_params[slot].adc_measures[0] + ")", "SLOT:" + (slot + 1) + " PS:0", chamber);

			if (slot_params[slot].adc_measures[0] > current_step[chamber].ps_voltage_max[0]) //ps0 max
			{
				log_alarm("CRITICAL ALARM: OVERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[0] + ")", "SLOT:" + (slot + 1) + " PS:0", chamber);
				slot_ps_alarm[slot]++;
			}
			else if (slot_params[slot].adc_measures[0] > current_step[chamber].ps_voltage_high_flag[0]) //ps0 high flag
				log_alarm("ALARM: OVERVOLTAGE FLAG REACHED(" + slot_params[slot].adc_measures[0] + ")", "SLOT:" + (slot + 1) + " PS:0", chamber);

			if (slot_params[slot].adc_measures[1] < current_step[chamber].ps_voltage_min[1]) //ps1 min
			{
				log_alarm("CRITICAL ALARM: UNDERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[1] + ")", "SLOT:" + (slot + 1) + " PS:1", chamber);
				slot_ps_alarm[slot]++;
			}
			else if (slot_params[slot].adc_measures[1] < current_step[chamber].ps_voltage_low_flag[1]) //ps1 low flag
				log_alarm("ALARM: UNDERVOLTAGE FLAG REACHED(" + slot_params[slot].adc_measures[1] + ")", "SLOT:" + (slot + 1) + " PS:1", chamber);

			if (slot_params[slot].adc_measures[1] > current_step[chamber].ps_voltage_max[1]) //ps1 max
			{
				log_alarm("CRITICAL ALARM: OVERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[1] + ")", "SLOT:" + (slot + 1) + " PS:1", chamber);
				slot_ps_alarm[slot]++;
			}
			else if (slot_params[slot].adc_measures[1] > current_step[chamber].ps_voltage_high_flag[1]) //ps1 high flag
				log_alarm("ALARM: OVERVOLTAGE FLAG REACHED(" + slot_params[slot].adc_measures[1] + ")", "SLOT:" + (slot + 1) + " PS:1", chamber);

			//3v3 and 5v0 alarms

			if (slot_params[slot].adc_measures[3] > 6.0) //5v0 max
			{
				log_alarm("CRITICAL ALARM: OVERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[3] + ")", "SLOT:" + (slot + 1) + " PS:5V0", chamber);
				slot_ps_alarm[slot]++;
			}
			if (slot_params[slot].adc_measures[3] < 4.0) //5v0 min
			{
				log_alarm("CRITICAL ALARM: UNDERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[3] + ")", "SLOT:" + (slot + 1) + " PS:5V0", chamber);
				slot_ps_alarm[slot]++;
			}
			if (slot_params[slot].adc_measures[2] > 4.1) //3v3 max
			{
				log_alarm("CRITICAL ALARM: OVERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[2] + ")", "SLOT:" + (slot + 1) + " PS:3V3", chamber);
				slot_ps_alarm[slot]++;
			}
			if (slot_params[slot].adc_measures[2] < 2.5) //3v3 min
			{
				log_alarm("CRITICAL ALARM: UNDERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[2] + ")", "SLOT:" + (slot + 1) + " PS:3V3", chamber);
				slot_ps_alarm[slot]++;
			}


			//uc2v5ref and current alarms

			if (slot_params[slot].adc_measures[4] > 3.3) //2V5 max
			{
				log_alarm("CRITICAL ALARM: OVERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[4] + ")", "SLOT:" + (slot + 1) + " PS:2V5", chamber);
				slot_ps_alarm[slot]++;
			}
			if (slot_params[slot].adc_measures[4] < 1.7) //2V5 min
			{
				log_alarm("CRITICAL ALARM: UNDERVOLTAGE LIMIT REACHED(" + slot_params[slot].adc_measures[4] + ")", "SLOT:" + (slot + 1) + " PS:2V5", chamber);
				slot_ps_alarm[slot]++;
			}

			/*

			if (slot_params[slot].adc_measures[5] > current_step[chamber].current_max) //current max
			{
				log_alarm("CRITICAL ALARM CURRENT: OVERCURRENT LIMIT REACHED(" + slot_params[slot].adc_measures[5] + ") ON SLOT " + (slot + 1), chamber);
				slot_ps_alarm[slot]++;
			}
			if (slot_params[slot].adc_measures[5] < current_step[chamber].current_min) //current min
			{
				log_alarm("CRITICAL ALARM CURRENT: UNDERCURRENT LIMIT REACHED(" + slot_params[slot].adc_measures[5] + ") ON SLOT " + (slot + 1), chamber);
				slot_ps_alarm[slot]++;
			}

			*/ //we dont have a parameter for overall board current yet - only DUT and individual PS ones (not being checked here)

			//once we are done looking for violtions, then evaluate if we need to shut down or not
			if (slot_ps_alarm[slot] > 3)
			{
				if (slot_params[slot].enabled)
				{
					log_alarm("CRITICAL ALARM: TOO MANY PS LIMIT VIOLATIONS, SHUTTING DOWN  SLOT: " + (slot + 1), "SLOT: " + (slot + 1), chamber);
					blacklist_slot(slot);//blacklist this slot -> disable on GUI, set bool which will shut down functions
				}//only blacklist if it is not disabled yet
			}
		}

		private void alarm_check_dut(int slot, int chamber)
		{
			for (int i = 0; i < 20; i++)
			{
				if (slot_params[slot].dut_present[i] && false) //if dut is present //made false for now because DUT doesnt report back properly
				{
					if (slot_params[slot].dut_current[i] < current_step[chamber].dut_ps_current_min) //dut min
					{
						log_alarm("CRITICAL ALARM: " + " UNDERCURRENT LIMIT REACHED: " + slot_params[slot].dut_current[i], "DUT: " + slot + ":" + (i + 1), chamber); slot_params[slot].dut_alarm[i]++;
					}
					else if (slot_params[slot].dut_temp[i] < current_step[chamber].dut_ps_current_low_flag) //dut low flag
						log_alarm("ALARM: " + " UNDERCURRENT FLAG REACHED: " + slot_params[slot].dut_current[i], "DUT: " + slot + ":" + (i + 1), chamber);

					if (slot_params[slot].dut_current[i] > current_step[chamber].dut_ps_current_max) //dut max
					{
						log_alarm("CRITICAL ALARM: " + " OVERCURRENT LIMIT REACHED: " + slot_params[slot].dut_current[i], "DUT: " + slot + ":" + (i + 1), chamber);
						slot_params[slot].dut_alarm[i]++;
					}
					else if (slot_params[slot].dut_temp[i] > current_step[chamber].dut_ps_current_high_flag) //dut_ps_current_high_flag
						log_alarm("ALARM: " + " OVERCURRENT FLAG REACHED: " + slot_params[slot].dut_current[i], "DUT: " + slot + ":" + (i + 1), chamber);
					///also define DUT TEMP alarms in here later
				}

				if (slot_params[slot].dut_alarm[i] > 3)
					blacklist_slot(slot);
				//blacklist this slot -> disable on GUI, set bool which will shut down functions


			}//check dut temp and current bounds
		}


		//Nabeels ZeroC connection stuff.
		private void call_innovative_clickbox() //should open connectoin to all available servers
		{
			string connection = null;
			string connection_fury = null;

			int slot = 0;
			try
			{
				if (connect_to_zeroc)
				{
					/*
					for (int i = 0; i < 16; i++)
					{
						slot_params[i].m_innovativeConnected = false;
						slot_params[i].innovative_hal = null;
						slot_params[i].m_furyConnected = false;
						slot_params[i].fury_hal = null;
					} //blank out the connections before making them
					*/
					for (int i = 0; i < ZeroC_connections.Count; i++)
					{
						connection = ZeroC_connections[i];
						connection_fury = ZeroC_connections_fury[i];

						int dash_index = connection.IndexOf('-');
						string substring = connection.Substring(dash_index + 1, connection.Length - (dash_index + 1)); //grabs last octet
						slot = Int32.Parse(substring) - 64; //last octet converted to the slot based on naming scheme

						Console.WriteLine("Connecting to " + connection);
						if (connection.Length == 0)
						{
							throw new Exception("Cannot connect without Ember connection type selection.");
						}

						if (m_innovative_remoteConnections.ContainsKey(connection))
						{
							m_innovative_interface = new InnovativeIceInterface(m_innovative_remoteConnections[connection]);
							connection = connection + ";" + m_innovative_remoteConnections[connection];
							slot_params[slot].m_innovativeConnected = true; //set the slot connection register to true
						}

						if (m_fury_remoteConnections.ContainsKey(connection_fury))
						{
							m_fury_interface = new FuryIceInterface(m_fury_remoteConnections[connection_fury]);
							connection_fury = connection_fury + ";" + m_fury_remoteConnections[connection_fury];
							slot_params[slot].m_furyConnected = true;
						}

						if (m_innovative_interface == null)
						{
							throw new Exception("ERROR: Unknown connection type.");
						}

						Innovative_HAL m_innovative_hal_temp = new Innovative_HAL(m_innovative_interface);
						X80QC m_fury_hal_temp = new X80QC(m_fury_interface);

						slot_params[slot].innovative_hal = m_innovative_hal_temp; //make this read the IP octet and then put in the right place in the array
						slot_params[slot].fury_hal = m_fury_hal_temp;
					}
					BackgroundWorker initial_zeroc = new BackgroundWorker();
					initial_zeroc.DoWork += initial_zeroc_DoWork; //also does mailbox and gpio setup
					initial_zeroc.RunWorkerAsync(); //read board eeproms immediately after making connections
				}
				else
				{
					//system_InnovativeExceptionCleanUp(); //comment this out, dont want to remove connections just because were not opening new ones
				}
			}
			catch (Exception ex)
			{
				system_InnovativeExceptionCleanUp(slot);
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

		public static void reset_gpio(int slot_index)
		{
			slot_params[slot_index].innovative_hal.SETGPIO(66, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(67, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(68, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(69, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(45, 0);

			for (int i = 0; i < slot_params[slot_index].gpio_pins.Length; i++)
				slot_params[slot_index].gpio_pins[i] = 0;

			Form1.gpioreset[slot_index] = true;
		}


		public static void reset_gpio_dut(int slot_index)
		{
			slot_params[slot_index].innovative_hal.SETGPIO(66, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(67, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(68, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(69, 0);
			slot_params[slot_index].innovative_hal.SETGPIO(45, 0);

			for (int i = 0; i < slot_params[slot_index].gpio_pins.Length; i++)
				slot_params[slot_index].gpio_pins[i] = 0;

			Form1.gpioresetdut[slot_index] = true;

		}

		public static void set_gpio(int slot_index, int[] gpio_state, int num)
		{
			int gpio_66 = 0; //state 0
			int gpio_67 = 0; //state 1
			int gpio_68 = 0; //state 2
			int gpio_69 = 0; //state 3
			int gpio_45 = 0; //state 4

			if (num > 20)
				Console.WriteLine("Invalid DUT Number");

			if (num > 15)
				gpio_66 = 1;
			num = num % 16;

			if (num > 7)
				gpio_45 = 1;
			num = num % 8;

			if (num > 3)
				gpio_68 = 1;
			num = num % 4;

			if (num > 1)
				gpio_69 = 1;
			num = num % 2;

			if (num == 1)
				gpio_67 = 1;


			//below if logic: if it is not matching the previous state, then go set it. if it matches then dont

			if (gpio_66 != gpio_state[0])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(66, gpio_66);
				slot_params[slot_index].gpio_pins[0] = gpio_66;
			}
			if (gpio_67 != gpio_state[1])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(67, gpio_67);
				slot_params[slot_index].gpio_pins[1] = gpio_67;
			}
			if (gpio_68 != gpio_state[2])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(68, gpio_68);
				slot_params[slot_index].gpio_pins[2] = gpio_68;
			}
			if (gpio_69 != gpio_state[3])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(69, gpio_69);
				slot_params[slot_index].gpio_pins[3] = gpio_69;
			}
			if (gpio_45 != gpio_state[4])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(45, gpio_45);
				slot_params[slot_index].gpio_pins[4] = gpio_45;
			}

			Form1.gpioset[slot_index] = true;
		}

		public static void set_gpio_dut(int slot_index, int[] gpio_state, int num)
		{
			int gpio_66 = 0; //state 0
			int gpio_67 = 0; //state 1
			int gpio_68 = 0; //state 2
			int gpio_69 = 0; //state 3
			int gpio_45 = 0; //state 4

			if (num > 20)
				Console.WriteLine("Invalid DUT Number");

			if (num > 15)
				gpio_66 = 1;
			num = num % 16;

			if (num > 7)
				gpio_45 = 1;
			num = num % 8;

			if (num > 3)
				gpio_68 = 1;
			num = num % 4;

			if (num > 1)
				gpio_69 = 1;
			num = num % 2;

			if (num == 1)
				gpio_67 = 1;


			//below if logic: if it is not matching the previous state, then go set it. if it matches then dont

			if (gpio_66 != gpio_state[0])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(66, gpio_66);
				slot_params[slot_index].gpio_pins[0] = gpio_66;
			}
			if (gpio_67 != gpio_state[1])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(67, gpio_67);
				slot_params[slot_index].gpio_pins[1] = gpio_67;
			}
			if (gpio_68 != gpio_state[2])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(68, gpio_68);
				slot_params[slot_index].gpio_pins[2] = gpio_68;
			}
			if (gpio_69 != gpio_state[3])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(69, gpio_69);
				slot_params[slot_index].gpio_pins[3] = gpio_69;
			}
			if (gpio_45 != gpio_state[4])
			{
				slot_params[slot_index].innovative_hal.SETGPIO(45, gpio_45);
				slot_params[slot_index].gpio_pins[4] = gpio_45;
			}

			Form1.gpiosetdut[slot_index] = true;
		}

		private void do_pretest(int chamber)
		{
			List<int> slots = new List<int>();// later on, implement how many slots we are pretesting for
			Cursor.Current = Cursors.WaitCursor;
			if (chamber == 1)
			{
				timer_zeroc.Enabled = false;
				pretest_lock[0] = true;

				Console.WriteLine("TURNING ON PSUS CHAMBER 1");
				log_alarm("CHAMBER " + chamber + 1 + " PRETEST", "STATE", chamber - 1);
				//do chamber 1 controls

				for (int i = 0; i < 8; i++)
				{
					string mailbox_message = "PreTEST,T" + pretest_step1.temp_set + "C," + pretest_step1.ps_voltage_set[0] + "V," + pretest_step1.measure_interval + "M," + pretest_step1.step_time + "M";
					if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
					{
						slot_params[i].innovative_hal.PSUBIB(1); //turn on bib psu before test

						slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", mailbox_message); //write mailbox
						slot_params[i].innovative_hal.WRITEMAILBOX("BI", "10"); //write mailbox
						mailbox_ack_received[i] = false;

						psu_zeroc(1, 0, pretest_step1.ps_voltage_set[0].ToString(), i);
						psu_zeroc(1, 1, pretest_step1.ps_voltage_set[1].ToString(), i);
						slot_params[i].toggle_firmwareEN(true);

						//check_psu_then_reset(i);
						string name = build_slot_file(i);//scan DUTs and create file

						send_file_name(i, name); //this will send file in mailbox, and wait for an ACK in the background

						//launch python script
						launch_python(i); //<- test this first befor eimplementing
					}
				}
				//button_pretest1.Hide();
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
					if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
					{
						slot_params[i].innovative_hal.PSUBIB(1); //turn on bib psu before test

						slot_params[i].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", mailbox_message); //write mailbox
						slot_params[i].innovative_hal.WRITEMAILBOX("BI", "10"); //write mailbox
						mailbox_ack_received[i] = false;

						psu_zeroc(1, 0, pretest_step2.ps_voltage_set[0].ToString(), i);
						psu_zeroc(1, 1, pretest_step2.ps_voltage_set[1].ToString(), i);
						slot_params[i].toggle_firmwareEN(true);

						//check_psu_then_reset(i);
						string name = build_slot_file(i);

						send_file_name(i, name);

						//launch python script
						launch_python(i); //<- test this first befor eimplementing
					}
				}
				button_pretest2.Hide();
				//button_start2.Show();
			}
			oven_param[chamber - 1].set_temp(40.0F);
			Cursor.Current = Cursors.Default;
		}

		private void launch_python(int slot) //0 based slot
		{
			BackgroundWorker launch_python = new BackgroundWorker();
			launch_python.DoWork += Launch_python_DoWork;
			launch_python.RunWorkerAsync(slot);
		}

		private void Launch_python_DoWork(object sender, DoWorkEventArgs e)
		{
			int slot = (int)e.Argument;
			//while (!slot_params[slot].uc_reset)
			//	System.Threading.Thread.Sleep(500);

			int ip = slot + 64;
			//string argument = "`$Host.UI.RawUI.WindowTitle=`Slot " + (ip - 64) + "`"; //try to name the window, ddidn twork
			Process python = new Process();
			python.StartInfo.UseShellExecute = false;
			python.StartInfo.WorkingDirectory = "C:\\Python27amd64";
			python.StartInfo.FileName = "C:\\Python27amd64\\python.exe";
			//python.StartInfo.CreateNoWindow = true;
			python.StartInfo.Arguments = "C:\\UC\\UCBurninICEAPP\\UCBurninICEAPP\\UCBurninICEAPP.py" + " " + ip;// + " " + argument;
			//python.StartInfo.RedirectStandardOutput = true;
			python.Start();
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
			var package = (Tuple<int, string>)e.Argument;

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

		private string ZeroC_activate(string arguments, int slot_num) ///obsolete function, dlls' now integrated into this program
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

		private void check_psu_then_reset(int slot)
		{
			BackgroundWorker checker = new BackgroundWorker();
			checker.DoWork += Checker_DoWork;
			checker.RunWorkerAsync(slot);
		}

		private void Checker_DoWork(object sender, DoWorkEventArgs e)
		{
			int slot = (int)e.Argument;
			slot_params[slot].psu_status = 0;
			while (slot_params[slot].psu_status != 1)
			{
				System.Threading.Thread.Sleep(500);
			}
			slot_params[slot].reset_uc(slot);
			System.Threading.Thread.Sleep(1000);
			//CNboolloop
			//retry:
			//if (finishedscanslots)
			//{
			slot_params[slot].scan_slot();
			//}
			//else
			//{
			//	Thread.Sleep(250);
			//	goto retry;
			//}
			slot_params[slot].uc_reset = true;

			/*
			top:
			if (Form1.bibready[slot])
			{
				slot_params[slot].psu_status = 0;
				while (slot_params[slot].psu_status != 1)
				{
					System.Threading.Thread.Sleep(500);
				}
				slot_params[slot].reset_uc(slot);
				System.Threading.Thread.Sleep(1000);
				//CNboolloop
				//retry:
				//if (finishedscanslots)
				//{
					slot_params[slot].scan_slot();
				//}
				//else
				//{
				//	Thread.Sleep(250);
				//	goto retry;
				//}
					slot_params[slot].uc_reset = true;
			}
			else
			{
				goto top;
			}*/
			
		}
	

		private void psu_zeroc(int state, int phase, string voltage, int slot)
		{
			while (slot_params[slot].psu_busy)//zeroc in progress
				System.Threading.Thread.Sleep(50);

			int chamber = (int)(slot / 8);

			log_alarm("RAMPING PSU" + phase + ": " + voltage + "V", "SLOT: " + (slot + 1), chamber);

			byte[] volts_ba = new byte[4];
			volts_ba = convert_to_byte(voltage);

			psu_params psu_param = new psu_params(state, phase, voltage, slot, chamber, volts_ba);

			if (slot_params[slot].m_innovativeConnected && slot_params[slot].enabled)
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

			slot_params[slot].psu_busy = true; //mark as busy so we cant go in elsewhere

			Console.WriteLine("Backgroundworker for setting PSU" + phase);

			if (phase == 2)
				slot_params[slot].innovative_hal.PSUSTATE(state);
			else
				slot_params[slot].innovative_hal.PSU(state, phase, volts_ba);

			slot_params[slot].psu_busy = false; //take off lock after doing set/poll
			
			//-> this is an additional check, within the psu routine. removed because checks and looping moved to main loop
			/*var psu_reads = slot_params[slot].innovative_hal.PSUPOLL(); //takes forever...
			//System.Threading.Thread.Sleep(500); //gives better performance, prevents extra threads from firing
			var psu_reads_float = convert_to_float(psu_reads);

			slot_params[slot].psu_busy = false; //take off lock after doing set/poll

			Console.WriteLine("---------------------------------------FIND ME AGAIN!!!!!!!!");

			for (int y = 0; y < psu_reads_float.Length; y++)
			{
				Console.WriteLine(psu_reads_float[y]);
			}



			/*if (phase == 2)//double psu
			{
				if (Math.Abs(psu_reads_float[0] - psu_reads_float[1]) > 1.5) //psus should not be set different form each other after using "2"
				{
						psu_retries++;
						if (psu_retries > 4)
							emergency_shutdown("CRITICAL ALARM: PSU COULD NOT BE SET, STOPPING", "SLOT: " + (slot + 1) + " PS: " + phase, chamber);
						psu_zeroc(state, phase, voltage, slot);
				}
				else
					psu_retries = 0;
			}
			else if (state == 0) //single psu, off
			{
				if (psu_reads_float[phase] > .2)
				{
					psu_retries++;
					if (psu_retries > 4)
						emergency_shutdown("CRITICAL ALARM: PSU COULD NOT BE SET, STOPPING", "SLOT: " + (slot + 1) + " PS: " + phase, chamber);
					psu_zeroc(state, phase, voltage, slot);
				}
				else
					psu_retries = 0;
			}
			else if (state == 1) //single psu, on
			{
				if (Math.Abs(psu_reads_float[phase] - float.Parse(voltage)) > .2)
				{
					psu_retries++;
					if (psu_retries > 4)
						emergency_shutdown("CRITICAL ALARM: PSU COULD NOT BE SET, STOPPING", "SLOT: " + (slot + 1) + " PS: " + phase, chamber);
					psu_zeroc(state, phase, voltage, slot);
				}
				else
					psu_retries = 0;
			}*/

		}

		//Initialize all of the elements on form 1.
		private void initialize_elements()
		{
			for (int i = 0; i < 16; i++)
			{
				slot_params[i] = new slot_param(i);
			}

			for (int i = 0; i < 2; i++)
			{
				oven_param[i] = new oven_params();
				current_step[i] = new step_params();
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

			label_bibsn_list.Add(label_bibsn1);
			label_bibsn_list.Add(label_bibsn2);
			label_bibsn_list.Add(label_bibsn3);
			label_bibsn_list.Add(label_bibsn4);
			/*label_bibsn_list.Add(label_bibsn5);
			label_bibsn_list.Add(label_bibsn6);
			label_bibsn_list.Add(label_bibsn7);
			label_bibsn_list.Add(label_bibsn8);
			label_bibsn_list.Add(label_bibsn9);
			label_bibsn_list.Add(label_bibsn10);
			label_bibsn_list.Add(label_bibsn11);
			label_bibsn_list.Add(label_bibsn12);
			label_bibsn_list.Add(label_bibsn13);
			label_bibsn_list.Add(label_bibsn14);
			label_bibsn_list.Add(label_bibsn15);
			label_bibsn_list.Add(label_bibsn16);*/
		}

		private float[] convert_to_float(byte[] ba)
		{
			float[] values = new float[6];

			for (int x = 0; x < 6; x++)
			{
				values[x] = BitConverter.ToSingle(ba, (4*x));
			}

			return values;
		}

		public byte[] convert_to_byte(string volts)
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
						if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
						{
							button_list[i].Enabled = true;
							if (slot_params[i].bib_id != "")
							{
								label_bibsn_list[i].Text = "BIB: " + slot_params[i].bib_id;
								label_bibsn_list[i].Visible = true;
							} //sets bibsn if one exists
							else
								label_bibsn_list[i].Visible = false;
						}
						else
							button_list[i].Enabled = false;
					}

					//label_state2.Text = state2;
					label_state1.Text = state1;




					if (oven_param[0].connected())
					{
						label_temp1.Text = oven_param[0].temp.ToString() + "° C";
						label_settemp1.Text = "Set: " + oven_param[0].temp_desired.ToString() + "° C";

						if (current_step[0].temp_reached)
							label_temp1.ForeColor = System.Drawing.Color.Black;
						else if (current_step[0].temp_ramped == 1)
							label_temp1.ForeColor = System.Drawing.Color.OrangeRed;
						else if (current_step[0].temp_ramped == 2)
							label_temp1.ForeColor = System.Drawing.Color.DeepSkyBlue;
						else
							label_temp1.ForeColor = System.Drawing.Color.Black;
					}

					/*if (oven_param[1].connected())
					{
						label_temp2.Text = oven_param[1].temp.ToString() + "° C";
						label_settemp2.Text = "Set: " + oven_param[1].temp_desired.ToString() + "° C";

						if (current_step[1].temp_reached)
							label_temp2.ForeColor = System.Drawing.Color.Black;
						else if (current_step[1].temp_ramped == 1)
							label_temp2.ForeColor = System.Drawing.Color.OrangeRed;
						else if (current_step[1].temp_ramped == 2)
							label_temp2.ForeColor = System.Drawing.Color.DeepSkyBlue;
						else
							label_temp2.ForeColor = System.Drawing.Color.Black;
					}*/


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

					/*if (label_state2.Text == "RUNNING")
						label_state2.ForeColor = System.Drawing.Color.YellowGreen;
					else if (label_state2.Text == "STOPPED")
						label_state2.ForeColor = System.Drawing.Color.Black;
					else if (label_state2.Text == "LOAD")
						label_state2.ForeColor = System.Drawing.Color.SlateBlue;
					else if (label_state2.Text == "CLEARING")
						label_state2.ForeColor = System.Drawing.Color.Red;
					else if (label_state2.Text == "PRETEST")
						label_state2.ForeColor = System.Drawing.Color.SteelBlue;*/

					if (loaded_check[0] && textBox_chamber1.Text == "")
						textBox_chamber1.Text = "Recipe Loaded";

					if (loaded_check[1] && textBox_chamber2.Text == "")
						textBox_chamber2.Text = "Recipe Loaded";
				});
			} //constantly update labels to match state
			catch { }

			for (int i = 0; i < 2; i++)
				step_num[i] = current_step[i].step_no;

		}

		private bool check_bool(bool t_f, bool[] array_to_find_num_from, bool[] input, int chamber)
		{
			int number = 0;
			for (int j = 0; j < array_to_find_num_from.Length; j++)
			{
				if (array_to_find_num_from[j])
					number++;
			}
			int start = (8 * (chamber - 1));
			int stop = 8 + start;
			int counter = 0;
			for (int i = start; i < stop; i++)
			{
				if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
				{
					if (input[i] == true)
						counter++;
				}
			}
			if (counter == number)
				return true;
			else
				return false;
		}

		private bool check_bool(bool t_f, bool[] input, int chamber) //input if all true or all false desired, array to check, and chamber
		{
			int start = (8 * (chamber - 1));
			int stop = 8 + start;

			for (int i = start; i < stop; i++)
			{
				if (slot_params[i].m_innovativeConnected && slot_params[i].enabled)
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
					lot_report_file[0] = lot_report_path + (system_name[0] + "_" + datetime2 + "_" + lotnum[0] + "_" + jobnum[0] + "_" + partnum[0] + ".txt");
			}
			if (chamber == 2)
			{
				if (!lot_report_file[1].Contains(".txt"))
					lot_report_file[1] = lot_report_path + (system_name[1] + "_" + datetime2 + "_" + lotnum[1] + "_" + jobnum[1] + "_" + partnum[1] + ".txt");
			}
			handle_log_files();
			//add the filename if the file path was changed
			Console.WriteLine("writing header for chamber " + chamber + "at" + lot_report_path);
			using (StreamWriter writer = File.AppendText(lot_report_file[chamber - 1]))
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
				writer.WriteLine("-----DateTime----,--elapsed_time--,-------------------Measurements------------------");
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
			string file_name = slot + "_" + "18027654" + "_" + DateTime.Now.ToString("MMddyyyy-hhmmss") + "_" + "SlotFile" + ".csv"; //18027654 is work order
			int chamber = (int)(slot / 8);
			int ip = slot + 64; //hardcoded ip address scheme

			string[] serial_nums = new string[20]; //from firmware\

			string opcode = String.Empty; //from firmware
			int[] psu_map = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

			for (int i = 0; i < 20; i++)
			{
				if (slot_params[slot].dut_present[i])
					serial_nums[i] = (i + 1).ToString();
			} //auto generate slot file from scanning in duts in the beginning

			string write = "";
			try
			{
				using (var writer = new StreamWriter(slot_file_path + file_name))
				{
					write += ("Field,Value,PSU Phase" + Environment.NewLine);
					write += ("BBB_IP_ADDRESS," + "192.168.121." + ip + Environment.NewLine);
					write += ("SLOT_NUMBER," + (slot + 1) + Environment.NewLine);
					write += ("ICE_WORK_ORDER_ID," + slot_params[slot].work_order + Environment.NewLine);
					write += ("ICE_DATALOG_FILE_ID," + Environment.NewLine);
					write += ("ICE_SOFTWARE_VERSION_ID," + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + Environment.NewLine);
					write += ("ICE_LOT_ID," + lotnum[chamber] + Environment.NewLine);
					write += ("JOB_START_TIME," + DateTime.Now.ToString("MM/dd/yyyy hh:mm") + Environment.NewLine);
					write += ("DUT_OPCODE_ID," + opcode + Environment.NewLine);
					write += ("DUT_PART_ID," + partnum[chamber] + Environment.NewLine);
					write += ("DUT_CONTROLLER_ID," + slot_params[slot].controller_id + Environment.NewLine);
					write += ("DUT_BOARD_ID," + slot_params[slot].bib_id + Environment.NewLine);
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
				using (var read = new StreamReader(step_file_path + step_file_name[chamber - 1]))
				{
					Console.WriteLine(step_file_path + step_file_name[chamber - 1]);
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
				Console.WriteLine("Error in step file read/parse" + "-" + (step_file_path + step_file_name[chamber - 1]) + "\n" + e);

			}
		}

		private void parse_step_file(List<string> input, int chamber)
		{
			int i;
			int loops;
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
						/*
						if (entries[step_index + 14][4] == "END")
							new_step.last_step = true;
						*/ //moved to loop condition below
						if (chamber == 1)
							step_list1.Add(new_step);
						if (chamber == 2)
							step_list2.Add(new_step);
					}//if "step name" found, use that as an index to add all data to a new step

					if (entries[i][1] == "Loop Condition")
					{
						int equals = entries[i][2].LastIndexOf("=") + 1;
						int end = entries[i][2].IndexOf("END");
						string substring = entries[i][2].Substring(equals, end - equals);
						loops = Int32.Parse(substring);

						int count = step_list1.Count;
						for (int n = 0; n < loops; n++)
						{
							for (int m = 0; m < count; m++)
							{
								step_params new_step = new step_params();

								if (chamber == 1)
								{
									new_step = step_list1[m].Copy();//copy by value not ref
									new_step.step_no = step_list1.Count() + 1;
									step_list1.Add(new_step);
								}
								else if (chamber == 2)
								{
									new_step = step_list2[m].Copy();//copy by value not ref
									new_step.step_no = step_list2.Count() + 1;
									step_list2.Add(new_step);
								}
							}
						}

						if (chamber == 1)
							step_list1[step_list1.Count - 1].last_step = true;
						else if (chamber == 2)
							step_list2[step_list2.Count - 1].last_step = true;

						Console.WriteLine("Step List1 Length: " + step_list1.Count);
						Console.WriteLine("Step List2 Length: " + step_list2.Count);
					}

				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error In parse function: " + e.Message);
			}

			if (chamber == 2)
			{

			}
		}

		

		//Just reads Nabeels CSV to extract a bunch of parameters instead of coding it in.
		//HE ALSO DOES OVEN STUFF IN HERE!
		private void chamber_config_read()
		{
			string config_path = "C:\\UC\\ConfigFiles\\UC_FURY.system_config.csv";

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
							step_file_path = values[3] + "\\";
						if (values[0] == "SlotFilePath")
							slot_file_path = values[3] + "\\";
						if (values[0] == "LotReportPath")
							lot_report_path = values[3] + "\\";
						if (values[0] == "TempReportPath")
							temp_report_path = values[3] + "\\";
						if (values[0] == "FTPUpload")
							ftp_upload_path = values[3];
						if (values[0] == "PartTypes")
						{
							var parts = values[3].Split(';');
							foreach (string part in parts)
								partnum_list.Add(part);
						}
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
							//textbox_system2.Text = values[3];
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
			catch (Exception e)
			{
				Console.WriteLine("No config file found" + "-" + e);
			}


			//RIGHT HERE! HE DOES IT HERE!!!
			for (int i = 0; i < 2; i++)
			{
				oven_param[i].enable = chamber_enable[i];
				oven_param[i].ip = chamber_ip_add[i];
				oven_param[i].port = chamber_comm_port[i];
			}
		}

		private void launch_monitor(int slot)
		{
			slot--;
			
			//CN1 CODE START
			if (Application.OpenForms.OfType<Form2>().Count() == 1)
			{
				Application.OpenForms.OfType<Form2>().First().Close();
			}
			Form2 dut_monitor = new Form2(slot);
			dut_monitor.Text = "Slot " + (slot+1);

			//CN1 CODE END
			dut_monitor.Show();
		}

		private void timer_alarm_clear_Tick(object sender, EventArgs e)
		{
			//30s interval
			for (int j = 0; j < 16; j++)
			{
				for (int i = 0; i < 20; i++)
					slot_params[j].dut_alarm[i] = 0; //clear all 20 dut alarms

				slot_ps_alarm[j] = 0; //clear ps alarms for each slot
			}

			if ((alarm_reset_counter % 2) == 0) //mod 2 is half as often
			{
				for (int i = 0; i < 2; i++) //make this go twice as slow -> use mod funct if needed
					oven_alarm[i] = 0;

			}
			alarm_reset_counter++;

			if (alarm_reset_counter == 100)
				alarm_reset_counter = 0;
		}

		private void launch_zeroc_scan(object sender, EventArgs e)
		{
			BackgroundWorker zeroc_scanner = new BackgroundWorker();
			zeroc_scanner.DoWork += Zeroc_scanner_DoWork;
			zeroc_scanner.RunWorkerAsync();
		}

		private void launch_temp_log1(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("temp", 0);
			logging_worker.RunWorkerAsync();
		}

		private void launch_lot_log1(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("lot", 0);
			logging_worker.RunWorkerAsync();
		}

		private void launch_temp_log2(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("temp", 1);
			logging_worker.RunWorkerAsync();
		}

		private void launch_lot_log2(object sender, EventArgs e)
		{
			BackgroundWorker logging_worker = create_logging_worker("lot", 1);
			logging_worker.RunWorkerAsync();
		}
		

		//initializes labels for all of the individual ovens in each chamber.
		private void initialize_labels(int chamber)
		{

			string step_file_name_trim = "";


			try
			{
				//Takes only 36 for UI purposes
				if (step_file_name[chamber - 1].Length > 36)
					step_file_name_trim = step_file_name[chamber - 1].Substring(0, 36);
				else step_file_name_trim = step_file_name[chamber - 1];
			}
			catch (System.NullReferenceException)
			{
				if (chamber == 1)
					button_pretest1.Enabled = false;
				if (chamber == 2)
					button_pretest2.Enabled = false;
				MessageBox.Show("Invalid step file entered, CLEAR then LOAD again");
			}

			update_elapsed_time(chamber);

			//Console.WriteLine("changing turnbutton text");// Lotnum/Step File: " + lotnum[0] + "/" + step_file_name_trim);

			for (int i = (0 + (8 * (chamber - 1))); i < (8 + (8 * (chamber - 1))); i++) //0-8 if chamber 1, 8-16 if chamber 2
			{
				button_list[i].NewText = "File: " + step_file_name_trim + "\n" + "Lot: " + lotnum[chamber - 1] + " -- Job: " + jobnum[chamber - 1] + " -- Part: " + partnum[chamber - 1] +
					"\n\nStep: " + step_num[chamber - 1] + " - Time: " + step_time[chamber - 1] + " -- Burn-In Time: " + elapsed_time[chamber - 1];
				button_list[i].Refresh();
			} //redraw turnbutton labels

		}

		//Does what the function title says.
		private void update_elapsed_time(int chamber)
		{
			string time;
			if (chamber == 1)
			{
				int seconds = (int)step_timer1.Elapsed.Seconds;
				int minutes = step_timer1.Elapsed.Minutes;
				int hours = step_timer1.Elapsed.Hours;
				int days = step_timer1.Elapsed.Days;
				time = hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString(); //days.ToString() + ":" +  <- can add this back in
				step_time[0] = time;

				//TimeSpan step_time = new TimeSpan();//(0, current_step[0].step_time, 0);
				//step_time = TimeSpan.FromMinutes(current_step[0].step_time);

				seconds = burn_in_timer1.Elapsed.Seconds;
				minutes = burn_in_timer1.Elapsed.Minutes;
				hours = burn_in_timer1.Elapsed.Hours;
				days = burn_in_timer1.Elapsed.Days;


				elapsed_time[0] = days.ToString() + ":" + hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();//total burn-in time
			}
			if (chamber == 2)
			{
				int seconds = (int)step_timer2.Elapsed.Seconds;
				int minutes = step_timer2.Elapsed.Minutes;
				int hours = step_timer2.Elapsed.Hours;
				int days = step_timer2.Elapsed.Days;
				time = hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString(); //days.ToString() + ":" +  <- can add this back in
				step_time[1] = time;

				seconds = burn_in_timer2.Elapsed.Seconds;
				minutes = burn_in_timer2.Elapsed.Minutes;
				hours = burn_in_timer2.Elapsed.Hours;
				days = burn_in_timer2.Elapsed.Days;


				elapsed_time[1] = days.ToString() + ":" + hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();//total burn-in time
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
					if (select == 1)
						tooltip_temp1.SetToolTip(this.label_temp1, min_str + low_str + set_str + high_str + max_str);
					//if (select == 2)
						//tooltip_temp2.SetToolTip(this.label_temp2, min_str + low_str + set_str + high_str + max_str);
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

		private void emergency_shutdown(string alarm_msg, string alarm_type, int chamber)
		{
			if (chamber == 0)
			{
				step_timer1.Stop();
				burn_in_timer1.Stop();

				state1 = "STOPPED";
				this.Invoke((MethodInvoker)delegate
				{
					textBox_chamber1.Text = "Emergency Shutdown";
				});
			}
			if (chamber == 1)
			{
				step_timer2.Stop();
				burn_in_timer2.Stop();

				state2 = "STOPPED";
				this.Invoke((MethodInvoker)delegate
				{
					textBox_chamber2.Text = "Emergency Shutdown";
				});
			}
			log_alarm(alarm_msg, alarm_type, chamber);
		}

		private void shutdown(string alarm_msg, string alarm_type, int chamber)
		{
			if (chamber == 0)
			{
				step_timer1.Stop();
				burn_in_timer1.Stop();
				if (oven_param[0].connected())
				{
					if (oven_param[0].return_temp_set() != 35.0)
					{
						oven_param[0].set_temp(35.0F);
					}
				}//bring oven temp down

				state1 = "STOPPED";
				this.Invoke((MethodInvoker)delegate
				{
					textBox_chamber1.Text = "Complete";
				});
			}
			if (chamber == 1)
			{
				step_timer2.Stop();
				burn_in_timer2.Stop();
				//bring oven temp down
				if (oven_param[1].connected())
				{
					if (oven_param[1].return_temp_set() != 35.0)
					{
						oven_param[1].set_temp(35.0F);
					}
				}//bring oven temp down
				state2 = "STOPPED";
				this.Invoke((MethodInvoker)delegate
				{
					textBox_chamber2.Text = "Complete";
				});
			}
			log_alarm(alarm_msg, alarm_type, chamber);
		}

		private void blacklist_slot(int slot)
		{
			this.Invoke((MethodInvoker)delegate
			{
				button_list[slot].Enabled = false;
			});
			//disconnect zeroc in here? maybe not so we can continue to monitor 

			slot_params[slot].enabled = false;//set the relevant slot member false
		}

		private void unblacklist_slot(int slot)
		{
			this.Invoke((MethodInvoker)delegate
			{
				button_list[slot].Enabled = true;
			});
			//disconnect zeroc in here? maybe not so we can continue to monitor 

			slot_params[slot].enabled = true;//set the relevant slot member false
		}



		///BUTTONS BELOW |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

		private void button1_Click(object sender, EventArgs e)
		{
			int slot = remove_alpha(System.Reflection.MethodBase.GetCurrentMethod().Name);
			launch_monitor(slot);
			Console.WriteLine("BUSY GATHERING DEVICES");
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
			log_alarm("CHAMBER 1 LOAD", "STATE", 0);
		}
		private void button_start_click(object sender, EventArgs e)
		{
			state1 = "RUNNING";
			burn_in_timer1.Start();
			log_alarm("CHAMBER 1 RUNNING", "STATE", 0);
		}
		private void button_stop_click(object sender, EventArgs e)
		{
			state1 = "STOPPED";
			log_alarm("CHAMBER 1 STOPPED", "STATE", 0);
		}
		private void button_clear_click(object sender, EventArgs e)
		{
			state1 = "CLEARING";
			log_alarm("CHAMBER 1 CLEARING", "STATE", 0);
		}
		private void button_load2_Click(object sender, EventArgs e)
		{
			state2 = "LOAD";
			log_alarm("CHAMBER 2 LOAD", "STATE", 0);
		}
		private void button_clear2_Click(object sender, EventArgs e)
		{
			state2 = "CLEARING";
			log_alarm("CHAMBER 2 CLEARING", "STATE", 0);
		}
		private void button_start2_Click(object sender, EventArgs e)
		{
			state2 = "RUNNING";
			burn_in_timer2.Start();
			log_alarm("CHAMBER 2 RUNNING", "STATE", 0);
		}
		private void button_stop2_Click(object sender, EventArgs e)
		{
			state2 = "STOPPED";
			log_alarm("CHAMBER 2 STOPPED", "STATE", 0);
		}

		private void button_pretest1_Click(object sender, EventArgs e)
		{
			state1 = "PRETEST";
			if (pretest_lock[0] != true)
			{
				do_pretest(1);
			}
		}

		private void button_pretest2_Click(object sender, EventArgs e)
		{
			state2 = "PRETEST";
			if (pretest_lock[1] != true)
			{
				//log_alarm("STATE: CHAMBER 2 PRETEST", 1); -> moved inside pretest funct
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

			Rectangle one = new Rectangle(50, 415, 1000, 1);

			form_graphics.FillRectangle(box_brush, one);

			box_brush.Dispose();
			form_graphics.Dispose();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			launch_python(0);
		}

		private void button1_Click_1(object sender, EventArgs e)
		{
			slot_params[0].innovative_hal.WRITEMAILBOX("BI", "20"); //write burn in step #
			slot_params[0].innovative_hal.WRITEMAILBOX("PACKAGE_EXCHANGE", "Baseline;T40C;3.3V;-1M;1M;0mA");
		}

		private void label2_Click(object sender, EventArgs e)
		{

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

			Brush b = new SolidBrush(ForeColor = System.Drawing.Color.Green);
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
			 string[] elapsed_time = new string[2];
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
		public step_params Copy()
		{
			return (step_params)this.MemberwiseClone();
		}

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
		public int temp_ramped = 0; //-1 for no data, 0 for default, 1 for heat up, 2 for cool down
		public bool temp_reached = false;
		public bool file_passed = false;
		public bool last_step = false;

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
			Console.WriteLine("reading curr file" + " Path: " + (Form1.step_file_path + curr_config_path + ".csv"));
			using (var read = new StreamReader(Form1.step_file_path + curr_config_path + ".csv"))
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
			using (var read = new StreamReader(Form1.step_file_path + ps_config_path + ".csv"))
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

			using (var read = new StreamReader(Form1.step_file_path + temp_config_path + ".csv"))
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
					temp_set = float.Parse(entries[3]);
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
		public float[] dut_current = new float[20];
		public float[] dut_temp = new float[20];
		public int[] dut_alarm = Enumerable.Repeat<int>(0, 20).ToArray();

		public double[] adc_measures = new double[6]; //psu0, psu1, ucref2.5, bib 3.3, bib 5.0

		//cn3
		public double[] psu_measures = new double[6]; //ps1 v, ps2 v, ps1 i, ps2 i, ps1 temp, ps2 temp

		public int[] gpio_pins = new int[5]; //to keep track of what dut select is on

		public int slot_num;
		public Innovative_HAL innovative_hal = new Innovative_HAL(null);
		public X80QC fury_hal = new X80QC(null);

		public bool enabled = true;

		public bool m_furyConnected;
		public bool m_innovativeConnected;

		public int psu_status = 0; //0 both off, 1 both on, 2 one off one on
		public bool psu_status_bib = false;
		public bool psu_busy = false;
		public bool python_busy = false;
		public bool uc_reset = false;

		public bool file_passed = false;

		public bool bib_present = false;
		public bool[] dut_present = Enumerable.Repeat<bool>(false, 20).ToArray(); //if bib present, this applies

		public string eeprom_controller = String.Empty;
		public string eeprom_BiB = String.Empty;

		public string work_order = String.Empty;
		public string controller_id = String.Empty;
		public string controller_pcb = String.Empty;
		public string controller_cpld_version = String.Empty;

		public string controller_cal_date = String.Empty;
		public string controller_mfg_date = String.Empty;

		public string bib_id = String.Empty;

		public bool scanning=false;
		public bool setoff = true;

		//public float[] psu=new float[6];

		public slot_param(int slot)
		{
			this.slot_num = slot;
		}

		public void scan_slot()
		{
			BackgroundWorker slot_scanner = new BackgroundWorker();
			Form1.finishedscanslots = false;
			slot_scanner.DoWork += Slot_scanner_DoWork;
			slot_scanner.RunWorkerAsync(slot_num);
		}

		private void Slot_scanner_DoWork(object sender, DoWorkEventArgs e)
		{
			int i = (int)e.Argument;
			int firstcount = 0;
			int secondcount = 0;
			int thirdcount = 0;
			int lastcount = 0;
			
			//reset_uc_extended(); //reset the UC before scanning
			Console.WriteLine("BIB DETECTED: SLOT " + (i + 1) + " , DOING BIB DETECT PROCEDURE (firmware+eeprom)");
			/*
			var byte_array = BitConverter.GetBytes(0);
			innovative_hal.PSUBIB(0);
			innovative_hal.PSU(0, 0, byte_array);
			innovative_hal.PSU(0, 1, byte_array);
			System.Threading.Thread.Sleep(1000);
			var byte_array1 = BitConverter.GetBytes(3.3);
			innovative_hal.PSUBIB(1); //turn on BIB power
			innovative_hal.PSU(1, 0, byte_array1);
			innovative_hal.PSU(1, 1, byte_array1);
			*/
			recheck:
			if (Form1.bibready[i])
			{
				innovative_hal.PSUBIB(1);
				Form1.reset_gpio(i); //force all gpio to 0, record in memory
				reset_uc(slot_num);
				System.Threading.Thread.Sleep(500);
				Console.WriteLine("-------------------------------gpioreset: " + Form1.gpioreset[i] + " + ucreset: " + Form1.ucreset[i]);
				begin:
				if (Form1.gpioreset[i] == true && Form1.ucreset[i] == true)
				{
					Console.WriteLine("-------------------------------Entering into the main dowork loop");
					for (int j = 0; j < 20; j++) //each dut
					{
						Console.WriteLine("Scanning dut present for dut " + (j + 1) + " slot " + (i + 1));
						//innovative_hal.Address(j + 1); //old, slow way
						Form1.set_gpio(i, gpio_pins, j + 1); //new way  *** Very slow process ***
						fury_hal.FirmwareEn = false; //enable fury controls // ** add flag for GPIO to finish before contiuing
						setoff = false;//System.Threading.Thread.Sleep(500); 
						second:
						if (Form1.gpioset[i] == true)
						{
							tp:
							if (!setoff)
							{

								try
								{
									fury_hal.FirmwareEn = true; //enable fury controls
																//System.Threading.Thread.Sleep(100);   //probably not neccessary (MIKE)
									if (Convert.ToBoolean(fury_hal.DUTPRESENT))
									{
										Console.WriteLine("DUT FOUND SITE " + (j + 1));
										dut_present[j] = true;//fill dut present array with dut presrent valu
									}
									else
									{
										dut_present[j] = false;
									}
									//System.Threading.Thread.Sleep(50);   //probably not neccessary (MIKE)
									fury_hal.FirmwareEn = false;
								}
								catch (Ice.UnknownException)
								{
									//System.Threading.Thread.Sleep(50);    //probably not neccessary (MIKE)
									try
									{
										fury_hal.FirmwareEn = true; //enable fury controls
										dut_present[j] = Convert.ToBoolean(fury_hal.DUTPRESENT); //fill dut present array with dut presrent valu
									}
									catch
									{
										MessageBox.Show("DUT " + (j + 1) + " scan slot " + (i + 1) + " failed... click slot to check DUTs for accuracy before proceeding.");
									}
								}
								setoff = true;
							}
							else
							{
								Console.WriteLine("---------------------FURY NOT FALSE.");
								secondcount++;
								if (secondcount > 1000)
								{
									goto pt;
								}
								goto tp;
							}
							pt:
							Console.WriteLine("------------FREED FROM FURY LOOP");
						}
						else
						{
							Console.WriteLine("---------------------GPIOSET NOT TRUE.");
							thirdcount++;
							if (thirdcount > 1000)
							{
								goto gpioout;
							}
							goto second;
						}//scan through duts
						gpioout:
						Console.WriteLine("-------------FREED FROM GPIO LOOP");

						if (!uc_reset) //only the first time
						{
							read_eeprom_bib();
							innovative_hal.PSUBIB(0); //turn off BIB power, in case operator disconnects it
							psu_status_bib = false; //should be off after this!
						}
						Form1.gpioset[i] = false;
					}
					clearflags(i);
					Console.WriteLine("-------------------------------------------FINISHED SCANNING DUTS (SCANNING=FALSE)");

				}
				else
				{
					Console.WriteLine("-----------------------GPIORESET / UCRESET NOT CORRECT.");
					firstcount++;
					if (firstcount > 1000)
					{
						goto freeme;
					}
					goto begin;
				}
				freeme:
				Console.WriteLine("-----------------------FREED FROM BOTH NOT SET.");
			}
			else
			{
				Console.WriteLine("-----------------------BIB NOT READY TO SET/RESET ANYTHING.");
				lastcount++;
				if (lastcount > 1000)
				{
					goto freeme2;
				}
				goto recheck;
			}
			freeme2:
			Console.WriteLine("---------------------FREE FROM ENTIRE LOOP");
			Form1.finishedscanslots = true;
		}

		public void toggle_firmwareEN(bool toggle)
		{
			for (int i = 0; i < dut_present.Length; i++)
			{
				if (dut_present[i])
					fury_hal.FirmwareEn = toggle;
			}
		} //turns off all firmware EN bits on detected DUTs

		public int check_bib(int inputslot) //convert from hex string to bool
		{
			if (m_innovativeConnected)
			{
				int value = 0;
				bool check = true;
				string output = innovative_hal.CHECKBIB();
				Thread.Sleep(200);
				output = output.Replace("\n", "");
				try
				{
					value = Convert.ToInt32(output, 16);
					check = Convert.ToBoolean(128 & value);

					if (check == false)
					{
						bib_present = true;
						Form1.checkbib[inputslot] = true;
						return 1;
					}
					else if (check == true)
					{
						bib_present = false;
						Form1.checkbib[inputslot] = false;
						return 0;
					}
				}
				catch
				{
					Console.WriteLine("Invalid Read from BiB present bit - skipping");
				}
				Form1.checkbib[inputslot] = true;
				return -1;
			}
			else
			{
				Form1.checkbib[inputslot] = true;
				return -1;
			}
		}

		public void read_dut_data() //present is determined one level higher, then this function is utilized
		{
			Console.WriteLine("-----------------------INSIDE READ DUT DATA!");
			if (bib_present && psu_status_bib && !python_busy && Form1.bibready[slot_num]) // && false) //make sure board is present, and its psus are on, and not in LI, for now turned off
			{
				Console.WriteLine("Reading DUT data, slot " + (slot_num + 1));
				Form1.reset_gpio_dut(slot_num); //reset gpio to 00000
				System.Threading.Thread.Sleep(100);
				bool[] dut_firmwareEN = new bool[20];
				fir:
				if (Form1.gpioresetdut[slot_num])
				{
					for (int dut = 0; dut < 20; dut++)
					{
						if (dut_present[dut] && psu_status == 1)// && uc_reset) //if its there
						{
							Form1.set_gpio_dut(slot_num, gpio_pins, dut + 1); //set address
							sec:
							if (Form1.gpiosetdut[slot_num])
							{//System.Threading.Thread.Sleep(20);
								try
								{
									dut_firmwareEN[dut] = Convert.ToBoolean(fury_hal.DUTPRESENT);
									System.Threading.Thread.Sleep(50);
									dut_current[dut] = fury_hal.DUT_CURRENT; //get current
																			 //System.Threading.Thread.Sleep(50);
									dut_temp[dut] = fury_hal.DUT_TEMP; //get temp
																		   //System.Threading.Thread.Sleep(50);
								}
								catch (Ice.UnknownException)
								{
									Console.WriteLine("Error taking DUT data, slot " + (slot_num + 1));
								} //sometimes, the fury command needs more time and throws an error. in this case, dont stop on error
								Form1.gpiosetdut[slot_num] = false;
							}
							else
							{
								goto sec;
							}
						}
						else
						{
							dut_current[dut] = -1;
							dut_temp[dut] = -1;
						}
						Console.WriteLine("---------------DUT " + dut + " MEASUREMENTS: CURRENT: " + dut_current[dut] + " TEMP: " + dut_temp[dut]);
					}
				}//loop through all duts
				else
				{
					goto fir;
				}
			
			}
			/*if (bib_present && psu_status_bib && !python_busy) // && false) //make sure board is present, and its psus are on, and not in LI, for now turned off
			{
				Console.WriteLine("Reading DUT data, slot " + (slot_num + 1));
				Form1.reset_gpio(slot_num); //reset gpio to 00000
				System.Threading.Thread.Sleep(100);
				bool[] dut_firmwareEN = new bool[20];
				for (int dut = 0; dut < 20; dut++)
				{
					if (dut_present[dut] && psu_status == 1)// && uc_reset) //if its there
					{
						Form1.set_gpio(slot_num, gpio_pins, dut + 1); //set address
																	  //System.Threading.Thread.Sleep(20);
						try
						{
							dut_firmwareEN[dut] = Convert.ToBoolean(fury_hal.DUTPRESENT);
							System.Threading.Thread.Sleep(50);
							dut_current[dut] = fury_hal.DUT_CURRENT; //get current
							//System.Threading.Thread.Sleep(50);
							dut_temp[dut] = fury_hal.DUT_TEMP; //get temp
							//System.Threading.Thread.Sleep(50);

						}
						catch (Ice.UnknownException)
						{
							Console.WriteLine("Error taking DUT data, slot " + (slot_num + 1));
						} //sometimes, the fury command needs more time and throws an error. in this case, dont stop on error
					}
					else
					{
						dut_current[dut] = -1;
						dut_temp[dut] = -1;
					}
				}//loop through all duts
			}*/
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

			if ((adc_reads_float[0] > 1.6) && (adc_reads_float[1] > 1.6)) //both on
				psu_status = 1;
			else if ((adc_reads_float[0] < 1.6) && (adc_reads_float[1] < 1.6)) //both off
				psu_status = 0;
			else
				psu_status = 2; //unknown state (one off one on, etc)

			if (adc_reads_float[2] > 1 && adc_reads_float[3] > 1) //sets psu status bib bit
				psu_status_bib = true;
			else
				psu_status_bib = false;

			return adc_reads_float;
		}
		

		public float[] do_psu_reads_float()
		{
			float[] values = new float[6];
			
			var psu_reads = innovative_hal.PSUPOLL();

			for (int i = 0; i < 6; i++)
			{
				values[i] = BitConverter.ToSingle(psu_reads, (4*i));
			}

			var psu_reads_float = values;

			return psu_reads_float;
		}

		public void adcpsumeasurements(int inputslot){
			//then move onto adc measurements
			double[] poll_results = Enumerable.Repeat<double>(0, 6).ToArray();
			float[] floats = Enumerable.Repeat<float>(0, 6).ToArray();


			//BBB ADC MEASUREMENTS
			//Generates phase 0, phase 1, 3v3, 5v0, uc2v5ref, amps
			if (!Form1.slot_params[inputslot].psu_busy)
			{
				poll_results = Form1.slot_params[inputslot].do_adc_reads_float();

				Form1.slot_params[inputslot].read_dut_data(); //automatically sets it within the slot param
			}

			for (int x = 0; x < 6; x++)
			{
				Form1.slot_params[inputslot].adc_measures[x] = Math.Round(poll_results[x], 3);
			}


			//BBB PSU MEASUREMENTS
			//Generate ps1 volt, ps2 volt, ps1 amp, ps2 amp, ps1 temp, ps2 temp
			if (!Form1.slot_params[inputslot].psu_busy)
			{
				floats = Form1.slot_params[inputslot].do_psu_reads_float();

				Form1.slot_params[inputslot].read_dut_data(); //automatically sets it within the slot param
			}
			for (int y = 0; y < 6; y++)
			{
				Form1.slot_params[inputslot].psu_measures[y] = Math.Round(floats[y], 3);
			}
			
			Console.WriteLine("-----------------------------------------------------SLOT " + (inputslot + 1) + " ADC MEASUREMENTS!!!!!!");

			for (int i = 0; i < Form1.slot_params[inputslot].adc_measures.Length; i++)
			{
				Console.Write(Form1.slot_params[inputslot].adc_measures[i] + " ");
			}
			Console.Write("\n");
			Console.WriteLine("-----------------------------------------------------SLOT " + (inputslot + 1) + " PSU MEASUREMENTS!!!!!!");

			for (int i = 0; i < floats.Length; i++)
			{
				Console.Write(floats[i] + " ");
			}
			Console.Write("\n");
			Console.WriteLine("-----------------------------------------------------");

		}


		private string eeprom_line_parse(string line)
		{
			int index = line.LastIndexOf(',') + 1;
			return line.Substring(index, line.Length - index);
		}

		public void read_eeprom_controller()
		{
			eeprom_controller = innovative_hal.READMEMORY("0");
			var controller = eeprom_controller.Split(';');

			string cal_month = string.Empty;
			string cal_date = string.Empty;
			string cal_year = string.Empty;
			string mfg_month = string.Empty;
			string mfg_date = string.Empty;
			string mfg_year = string.Empty;

			foreach (string line in controller)
			{
				if (line.Contains("WO"))
					work_order = eeprom_line_parse(line);
				if (line.Contains("Serial Number"))
				{
					controller_id = eeprom_line_parse(line);
					controller_id = controller_id.Replace("ICE-", "");
				}
				if (line.Contains("CAL Month"))
					cal_month = eeprom_line_parse(line);
				if (line.Contains("CAL Date"))
					cal_date = eeprom_line_parse(line);
				if (line.Contains("CAL Year"))
					cal_year = eeprom_line_parse(line);
				if (line.Contains("MFG Month"))
					mfg_month = eeprom_line_parse(line);
				if (line.Contains("MFG Date"))
					mfg_date = eeprom_line_parse(line);
				if (line.Contains("MFG Year"))
					mfg_year = eeprom_line_parse(line);
				if (line.Contains("PCB"))
					controller_pcb = eeprom_line_parse(line);
				if (line.Contains("CPLD"))
					controller_cpld_version = eeprom_line_parse(line);
			}
			controller_cal_date = cal_month + "/" + cal_date + "/" + cal_year;
			controller_mfg_date = mfg_month + "/" + mfg_date + "/" + mfg_year;
		}

		public void read_eeprom_bib()
		{
			eeprom_BiB = innovative_hal.READMEMORY("1");
			var bib = eeprom_BiB.Split(';');


			foreach (string line in bib)
			{
				if (line.Contains("Serial Number"))
				{
					int index = line.LastIndexOf(',') + 1;
					bib_id = line.Substring(index, line.Length - index);
					bib_id = bib_id.Replace("ICE-", "");
				}
			}
		}

		public void clearflags(int inputslot)
		{
			scanning = false;
			Form1.gpioreset[inputslot] = false;
			Form1.ucreset[inputslot] = false;
			Form1.gpioset[inputslot] = false;
			Console.WriteLine("--------------------CLEARED ALL FLAGS FOR SLOT " + inputslot+ "!");
		}

		public void reset_uc_extended()
		{
			innovative_hal.SETGPIO(44, 0); 
			innovative_hal.PSUBIB(0);
			innovative_hal.PSUSTATE(0);

			System.Threading.Thread.Sleep(500);
			innovative_hal.PSUBIB(1);
			System.Threading.Thread.Sleep(500);
			innovative_hal.SETGPIO(44, 1);
			System.Threading.Thread.Sleep(500);
			innovative_hal.PSUSTATE(1);
			System.Threading.Thread.Sleep(1000);//250 + (i*15)); //make sure they are on first, give it some time

		}

		public void reset_uc(int inputslot)
		{
			Console.WriteLine("-----------TURNING OFF uC for slot "+ inputslot+" :(");
			innovative_hal.SETGPIO(44, 0);
			System.Threading.Thread.Sleep(500);
			innovative_hal.SETGPIO(44, 1);
			Console.WriteLine("-------------------TURNING ON uC " + inputslot + " :)");
			Form1.ucreset[inputslot] = true;
		}

	}//used to store each slot's parameters

	public class oven_params
	{
		public oven_params()
		{
			ip = "92.168.121.10";
			port = "502";
			enable = false;
			oven_string = "0.0";
		}//set default in constructor


		public ModbusClient modbus_client = new ModbusClient();
		public string ip, port;
		public string oven_string;
		public float temp;
		public float temp_desired;

		public bool enable;

		public bool connected()
		{
			return modbus_client.Connected;
		}

		public void fan_off()
		{
			try
			{
				modbus_client.WriteSingleCoil(16385, false);
			}
			catch
			{
				Console.WriteLine("Error setting modbus register fan_off");
			}
		}

		public void fan_on()
		{
			try
			{
				modbus_client.WriteSingleCoil(16385, true);
			}
			catch
			{
				Console.WriteLine("Error setting modbus register fan_on");
			}
		}

		public void heat_off()
		{
			try
			{
				modbus_client.WriteSingleCoil(16386, false);
			}
			catch
			{
				Console.WriteLine("Error setting modbus register heat_off");
			}
		}

		public void heat_on()
		{
			try
			{
				modbus_client.WriteSingleCoil(16386, true);
			}
			catch
			{
				Console.WriteLine("Error setting modbus register heat_on");
			}
		}

		public void set_temp(float input)
		{
			try
			{
				temp_desired = input;
				float value = input * 10;
				int temp_10 = (int)value;
				modbus_client.WriteSingleRegister(2, temp_10);
			}
			catch
			{
				Console.WriteLine("Error setting modbus register set_temp");
			}
		}

		public int fan()
		{
			try
			{
				var result = modbus_client.ReadCoils(16385, 1);
				return Convert.ToInt32(result[0]);
			}
			catch
			{
				Console.WriteLine("Error reading modbus register fan");
				return -1;
			}
		}

		public int heat()
		{
			try
			{
				var result = modbus_client.ReadCoils(16386, 1);
				return Convert.ToInt32(result[0]);
			}
			catch
			{
				Console.WriteLine("Error reading modbus register heat");
				return -1;
			}
		}

		public float temp_current()
		{
			try
			{
				var result = modbus_client.ReadInputRegisters(0, 1);
				temp = result[0] / 10.0F;
				return temp;
			}
			catch
			{
				Console.WriteLine("Error reading modbus register temp_current");
				return -1;
			}
		}

		public float return_temp_set()
		{
			try
			{
				var result = modbus_client.ReadInputRegisters(2, 1);
				var float_result = result[0] / 10.0F;
				temp_desired = float_result;
				return float_result;
			}
			catch
			{
				Console.WriteLine("Error reading modbus register return_temp_set");
				return -1;
			}
		}

		private bool[] alarms()
		{
			try
			{
				var result = modbus_client.ReadCoils(0, 7);
				return result;
			}
			catch
			{
				Console.WriteLine("Error reading modbus register alarms()");
				return new bool[] { false, false, false, false, false, false, false };
			}
		}

		public string report_alarms()
		{
			var alarm_array = alarms(); //overtemp, fan fail, ac ppower, door, motor, blank, damper(6)
			string result = "";
			for (int i = 0; i < alarm_array.Length; i++)
			{
				switch (i)
				{
					case 0:
						if (alarm_array[i])
							result += "OVERTEMP; ";
						break;
					case 1:
						if (alarm_array[i])
							result += "FAN FAIL; ";
						break;
					case 2:
						if (alarm_array[i])
							result += "AC POWER; ";
						break;
					case 3:
						if (alarm_array[i])
							result += "DOOR; ";
						break;
					case 4:
						if (alarm_array[i])
							result += "MOTOR; ";
						break;
					case 6:
						if (alarm_array[i])
							result += "DAMPER; ";
						break;
				}//switch to switch between strings for the alarm
			}//loop through alarm array
			return result;
		}
	} //class for containing oven functions
}
