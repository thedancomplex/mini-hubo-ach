using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace miniHubo
{
	public class conducting
	{
		public bool doLoop = false;
		public bool doUdpLoop = false;
		public int T = 20;	// 50ms time (default)
		public double t = 0;	// time
		public double BPM = 120;		// tempo in beats per min to count to 
		private int beat = 1;		// what beat you are on;
		public int beatDelay = 1;		// delay in ms for beat
		
		// networking stuff
		//private Socket rxSocket;
		private IPAddress rxAddress = IPAddress.Any;
		private IPEndPoint rxEndPoint;
		private UdpClient sock;
		//private byte[] rxBuff = new byte[1024];
		public int socketNum = 5005;
		
		
		public conducting ()
		{
			
		}
		public void mainLoop(IK2D theIK)
		{
			if( doLoop)
				conductingLoop( theIK);
		}
			
		
		private void conductingLoop( IK2D theIK)
		{
			t = t+(double)T/1000.0;
			double beatSeconds = 60.0/BPM;		// seconds per beat
			if (t >= beatSeconds)
			{
				runState(44, theIK);
				t = 0;
			}
		}
		
		private void setState( int[] mot, double[] deg, IK2D theIK)
		{
			try{
				for(int i = 0; i < mot.Length; i++)
				{
					theIK.motorDesAngle[mot[i]] = deg[i];
				}
			}
			catch( Exception ee)
			{
				Console.WriteLine(ee.Message.ToString());
			}
		}
		
		public void doNextBeat( int timeSig, double theTempo, IK2D theIK)
		{
			// this is where it pauses
			//double ratio = 0.5;		//(sleep amount)
			int ms = (int)Math.Floor(60.0/theTempo*1000.0);	// tempo period in ms
			
			//int rDelay = 300;	// robot inherent delay
			//int rDelay = 300;	// robot inherent delay  // for 120bpm and 144bpm
			//int rDelay = 800;	// robot inherent delay	// for 72bpm
			int rDelay = 100;
			
			Thread.Sleep(ms-rDelay);
			
			
			if( theTempo > 150 )
			{
				runState(22, theIK);
			}
			else
			{
				runState(timeSig, theIK);
			}
			
			
		}
		
		private void runState( int timeSig, IK2D theIK)
		{
			// int timeSig = time signiture where 	44 = 4/4 time
			//										34 = 3/4 time
			//										78 = 7/8 time
			//										etc...
			
			// Deffinitions
			
			
			
			
			//SU
			int[] su_motor = {theIK.LSP, theIK.LSY};
			double[] su1_deg = {120.0, 90.0};
			double[] su2_deg = {60.0, 90.0};
			
			//SL
			int[] sl_motor = {theIK.LSR, theIK.LEB, theIK.LSY};
			double[] sl1_deg = {-20.0, 40.0, 90.0};
			double[] sl2_deg = {20.0, -40.0, 90.0};
			double[] sl3_deg = {0.0, 0.0, 90.0};
			
			if ( beat <= 0)
			{
				beat = 1;
			}
			
			
			
			switch(timeSig)
			{
				case 44:
					if( beat == 1 )
					{
						setState(su_motor, su2_deg, theIK);		// set su2, sl3
						setState(sl_motor, sl3_deg, theIK);
					}
					else if( beat == 2 )
					{
						setState(su_motor, su2_deg, theIK);
						setState(sl_motor, sl1_deg, theIK);
					}
					else if( beat == 3 )
					{
						setState(su_motor, su2_deg, theIK);
						setState(sl_motor, sl2_deg, theIK);
					}
					else if( beat == 4 )
					{
						setState(su_motor, su1_deg, theIK);
						setState(sl_motor, sl3_deg, theIK);
					}
					
					beat = beat + 1;
					if( beat > 4 )
					{
						beat = 1;
					}
					break;
				case 22:
					if( beat == 1 )
					{
						setState(su_motor, su2_deg, theIK);
						setState(sl_motor, sl2_deg, theIK);
					}
					else if( beat == 2 )
					{
						setState(su_motor, su2_deg, theIK);
						setState(sl_motor, sl1_deg, theIK);
					}
					else if( beat == 3 )
					{
						setState(su_motor, su2_deg, theIK);
						setState(sl_motor, sl2_deg, theIK);
					}
					else if( beat == 4 )
					{
						setState(su_motor, su2_deg, theIK);
						setState(sl_motor, sl1_deg, theIK);
					}
					
					beat = beat + 1;
					if( beat > 4 )
					{
						beat = 1;
					}
					break;
			}
		
		}
		
		public void udpINI()
		{
			
			//rxSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			rxEndPoint = new IPEndPoint(rxAddress, socketNum);
			sock = new UdpClient(socketNum);
		}
		
		public void udpLoop(int mode, IK2D theIK)
		{
			
			// int mode = 	1: tap on receve of ['B', int timeSig ]
			//				Commands the robot to go to the next beat
			while(doUdpLoop)
			{	


                                if (mode == 1)
				{
					byte[] rxData = sock.Receive(ref rxEndPoint);
					double WST = BitConverter.ToDouble(rxData,0*4);
					double RHY = BitConverter.ToDouble(rxData,26*4);
				        Console.WriteLine("WST = %f , RHY = %f",WST,RHY);	
				}

				if( mode == 111 )	// mode 1
				{
					Console.WriteLine("Waiting for UDP");
					byte[] rxData = sock.Receive(ref rxEndPoint);
					if(rxData[0] == 66) // == B
					{
						Thread.Sleep(beatDelay);
						double theTempo = BitConverter.ToDouble(rxData,2);
						Console.WriteLine("Tempo = "+BitConverter.ToDouble(rxData,2).ToString());
						doNextBeat(rxData[1], theTempo, theIK);
						
						MainClass.setMotorAll(MainClass.dynTop, MainClass.dynBottom);
					}
					
					int beatToSend = beat - 1;
					
					if (beatToSend < 1)
						beatToSend = 4;
					
					
					
					Console.Write("On Beat {0} - Received ({1}): ", beatToSend.ToString(), rxEndPoint.ToString());
					for( int i = 0; i < rxData.Length ; i++)
					{
						Console.Write(" "+rxData[i].ToString());
				      //sock.Close();
					}
					Console.WriteLine();
				}
			}
		}
			
	}
}

