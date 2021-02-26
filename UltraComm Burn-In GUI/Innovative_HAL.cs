
namespace UltraComm_Burn_In_GUI
{
	public class Innovative_HAL
	{
		InnovativeInterface m_dev_if;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dev_if"></param>
		public Innovative_HAL(InnovativeInterface dev_if)
		{
			m_dev_if = dev_if;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DUT"></param>
		public void Address(int DUT)
		{
			m_dev_if.GPIO(DUT);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		/// <param name="phase"></param>
		/// <param name="voltage"></param>
		public void PSU(int state, int phase, byte[] voltage)
		{
			m_dev_if.PSU(state, phase, voltage);
		}

		public void PSUSTATE(int state)
		{
			m_dev_if.PSUSTATE(state);
		}

		public void PSUBIB(int state)
		{
			m_dev_if.PSUBIB(state);
		}

		public byte[] PSUPOLL()
		{
			var psu_poll = new byte[12];
			for (int i = 0; i < psu_poll.Length; i++)
			{
				psu_poll[i] = 0x00;
			}
			return m_dev_if.PSUPOLL();

		}

		public short[] ADCPOLL()
		{
			return m_dev_if.ADCPOLL();
		}

		public void WRITEMAILBOX(string mailboxName, string mailboxContents)
		{
			m_dev_if.WRITEMAILBOX(mailboxName, mailboxContents);
		}

		public string READMAILBOX(string mailboxName)
		{
			return m_dev_if.READMAILBOX(mailboxName);
		}

		public string QUERYMAILBOX(string writeMailbox, string mailboxContents, string readMailbox)
		{
			return m_dev_if.QUERYMAILBOX(writeMailbox, mailboxContents, readMailbox);
		}

		public void WRITETEMP(string temp)
		{
			m_dev_if.WRITETEMP(temp);
		}

		public string READTEMP()
		{
			return m_dev_if.READTEMP();
		}

		public string READMEMORY(string select)
		{
			return m_dev_if.READMEMORY(select);
		}

		public string CHECKBIB()
		{
			return m_dev_if.CHECKBIB();
		}

		public void SETGPIO(int pin, int state)
		{
			m_dev_if.SETGPIO(pin, state);
		}
    }
}
