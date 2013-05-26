using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;

//For Dyn Class
using System.IO.Ports;



namespace DynamexalClass
{/*
    public partial class Form1 : Form
    {
        public double speed = 0;
        DynClass DynSystem = new DynClass();
        public Form1()
        {
            InitializeComponent();
            DynSystem.ComPort = "COM6";
            DynSystem.ComBaud = 1000000;
            DynSystem.ini();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            speed = (trackBar1.Value / 1000.0);
            labelValue.Text = speed.ToString();

            try
            {
                //DynSystem.setSpeed(1, speed);
                DynSystem.setPos(34, speed);
                string temp = ":";
                for (int i = 0; i < DynSystem.txbuff.Length; i++)
                {
                    temp = temp + DynSystem.txbuff[i].ToString() + " ";
                }
                labelComTX.Text = temp;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message.ToString());
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            //DynSystem.setSpeed(1, speed);
            //DynSystem.setSpeed(1, .2);
            //DynSystem.setPos(0, 0.5);

            DynSystem.test();
            
        }
    }
	*/ // Dyn class forum

    public class DynClass
    {
        public string ComPort = "COM1";             // Set the active com port (Default = COM1)
        public int ComBaud = 57142;                 // Baud rate (Default = 57142)
        private SerialPort DynPort;           // COM port object

        public byte[] txbuff;

        public bool ini()   // initilize the com port (Return: true if successful, false if not)
        {
            bool output = true;
            try
            {
                DynPort = new SerialPort(ComPort, ComBaud, Parity.None, 8, StopBits.One);
				//DynPort = new SerialPort("COM1", 1000000, Parity.None, 8, StopBits.One);
                DynPort.Open();
            }
            catch (Exception ee)
            {
				//Console.WriteLine(ee.Message.ToString());
                output = false;
            }

            return output;
        }

        public void setSpeed(byte id, double n)    // set speed of a rotational servo with (id) and speed n ( -1<=n<=1)
        {
            if (n > 1)
                n = 1;
            else if (n < -1)
                n = -1;

            int m = (int)Math.Floor(n * 1024);

            if (m > 1023)
                m = 1023;
            else if (m < -1023)
                m = -1023;

            setSpeedBytes(id, getSpeedByteConvert(m));
        }

        private void setSpeedBytes(byte id, byte[] n)     //set the speed of a rotational servo
        {
            byte theStart = 255;
            // byte tempLength = 5;
            // byte theInstruction = (byte)0x04;
            byte tempLength = 5;
            byte theInstruction = (byte)0x03;
            byte theStartingReg = 32;
            byte theCheckSum = (byte)((int)(id + tempLength + theInstruction + theStartingReg + n[0] + n[1]) & 0xff);
            theCheckSum = (byte)~theCheckSum;

            /*
            Mvalue0.Text = theStart.ToString();
            Mvalue1.Text = theStart.ToString();
            Mvalue2.Text = id.ToString();               // id
            Mvalue3.Text = tempLength.ToString();       // length
            Mvalue4.Text = theInstruction.ToString();   // the instruction
            Mvalue5.Text = theStartingReg.ToString();   // the starting reg value
            Mvalue6.Text = n[0].ToString();             // ls byte speed
            Mvalue7.Text = n[1].ToString();             // ms byte speed
            Mvalue8.Text = theCheckSum.ToString();      // checksum
            */

            DynPort.Write(new byte[] { 255, 255, (byte)id, tempLength, theInstruction, theStartingReg, n[0], n[1], theCheckSum }, 0, 9);
            //setTorque(id, 1);
        }

        private byte[] getSpeedByteConvert(int n)
        {

            byte[] messageHold = new byte[2];      // initial torque values
            /*
            messageHold[0] = (byte)(n & 0xff);          // LSB
            messageHold[1] = (byte)((n >> 8) & 0xff);   // MSB
            messageHold[1] = (byte)(messageHold[1] & 0x03);
            */

            if (n >= 0)
            {
                messageHold[0] = (byte)(n & 0xff);          // LSB
                messageHold[1] = (byte)((n >> 8) & 0xff);   // MSB
                messageHold[1] = (byte)(messageHold[1] & 0x03);
                messageHold[1] = (byte)(messageHold[1] + 0x04);   // set bit 2 ToolBar turn cw

            }
            else
            {
                //messageHold[1] = (byte)(0x3ff-messageHold[1]);
                n = n = n & 0x3ff;
                n = 0x3ff - n;
                messageHold[0] = (byte)(n & 0xff);          // LSB
                messageHold[1] = (byte)((n >> 8) & 0xff);   // MSB
                messageHold[1] = (byte)(messageHold[1] & 0x03);
                //messageHold[1] = (byte)(messageHold[1] + 0x04);   // set bit 2 ToolBar turn cw
            }

            return messageHold;
        }



        private void setPosBytes(byte id, byte[] n)     //set the speed of a rotational servo
        {
            byte theStart = 255;
            // byte tempLength = 5;
            // byte theInstruction = (byte)0x04;
            byte tempLength = 5;
            byte theInstruction = (byte)0x03;
            byte theStartingReg = 30;
            byte theCheckSum = (byte)((int)(id + tempLength + theInstruction + theStartingReg + n[0] + n[1]) & 0xff);
            theCheckSum = (byte)~theCheckSum;

            /*
            Mvalue0.Text = theStart.ToString();
            Mvalue1.Text = theStart.ToString();
            Mvalue2.Text = id.ToString();               // id
            Mvalue3.Text = tempLength.ToString();       // length
            Mvalue4.Text = theInstruction.ToString();   // the instruction
            Mvalue5.Text = theStartingReg.ToString();   // the starting reg value
            Mvalue6.Text = n[0].ToString();             // ls byte speed
            Mvalue7.Text = n[1].ToString();             // ms byte speed
            Mvalue8.Text = theCheckSum.ToString();      // checksum
            */
            txbuff = new byte[] {255, 255, (byte)id, tempLength, theInstruction, theStartingReg, n[0], n[1], theCheckSum};
            DynPort.Write(new byte[] { 255, 255, (byte)id, tempLength, theInstruction, theStartingReg, n[0], n[1], theCheckSum }, 0, 9);
            //setTorque(id, 1);
        }

        public void setPos(byte id, double n)    // set speed of a rotational servo with (id) and speed n ( -1<=n<=1)
        {

			//n = -n;
			
            if (n > 1)
                n = 1;
            else if (n < 0)
                n = 0;

            int m = (int)Math.Floor(n * 1024);

            if (m > 1023)
                m = 1023;
            else if (m < 0)
                m = 0;

            setPosBytes(id, getDegByteConvert(m));
        }

        private byte[] getDegByteConvert(int n)
        {
            byte[] messageHold = new byte[2];      // initial torque values

            if (n >= 0)
            {
                messageHold[0] = (byte)(n & 0xff);          // LSB
                messageHold[1] = (byte)((n >> 8) & 0xff);   // MSB
                messageHold[1] = (byte)(messageHold[1] & 0x03);
                //messageHold[1] = (byte)(messageHold[1] + 0x04);   // set bit 2 ToolBar turn cw

            }
            else
            {
                messageHold[0] = (byte)0;           // LSB
                messageHold[1] = (byte)0;           // MSB
            }

            return messageHold;
        }
		
		private byte[] getByteConvert(int n)
        {
            byte[] messageHold = new byte[2];      // initial torque values

            if (n >= 0)
            {
                messageHold[0] = (byte)(n & 0xff);          // LSB
                messageHold[1] = (byte)((n >> 8) & 0xff);   // MSB
                messageHold[1] = (byte)(messageHold[1] & 0x03);
                //messageHold[1] = (byte)(messageHold[1] + 0x04);   // set bit 2 ToolBar turn cw

            }
            else
            {
                messageHold[0] = (byte)0;           // LSB
                messageHold[1] = (byte)0;           // MSB
            }

            return messageHold;
        }

        public void test()
        {
            DynPort.Write(new byte[] { 255, 255, 1, 2, 3, 4, 5, 6, 7 }, 0, 9);
        }
		
		public void doSyncWritePos(byte[] motor, double[] pos)
		{
			// cmd = command address
			// motor[] = motor number
			// pos[] = data for the corrosponding motor (horrozontal)
			
			int L = 2;					// length of setting the angle 
			int N = motor.Length;		// number of motors
			byte tLength = (byte)((L+1)*N+4);		// calculate the length
			
			txbuff = new byte[tLength + 4];
			//Console.WriteLine("txBuff Length = "+txbuff.Length.ToString());
			
			txbuff[0] = (byte)255;
			txbuff[1] = (byte)255;
			txbuff[2] = (byte)254;		// hex 0xFE for ID
			txbuff[3] = (byte)tLength;	// length of all commands
			txbuff[4] = (byte)131;		// hex 0x83 (instruction)
			txbuff[5] = (byte)30;		// hex 0x1E goal position
			txbuff[6] = (byte)2;		// length of data for each motor (2) for pos
			
			int iTx = 7;
			for(int i = 0; i < motor.Length ; i++ )
			{
				byte[] temp = deg2ByteArg(pos[i]);
				
				txbuff[iTx] = motor[i];
				txbuff[iTx+1] = temp[0];
				txbuff[iTx+2] = temp[1];
				//Console.WriteLine("pos: "+pos[i].ToString()+"  B[0]: "+temp[0].ToString()+"  B[1]: "+temp[1].ToString());
				iTx = iTx + 3;
			}
			
			// make checksum
			int checkSum = 0;
			
			for(int i = 2; i < (txbuff.Length-1); i++)
			{
				checkSum = checkSum + txbuff[i];
			}
			
			byte[] checkTemp = BitConverter.GetBytes(checkSum);
			
			byte checkSumFinal = (byte)~checkTemp[0];
			//Console.WriteLine("The Checksum: "+checkTemp[0].ToString()+"  then: "+checkSumFinal.ToString());
			
			
			txbuff[txbuff.Length-1] = checkSumFinal;
				
			/*	
			Console.WriteLine("To send:");
			for(int i = 0; i < txbuff.Length ; i++)
			{
				Console.Write(" "+txbuff[i].ToString());
			}
			*/
			
			DynPort.Write(txbuff, 0, txbuff.Length);
		}
		
		public byte[] deg2ByteArg(double nn)
        {

			//n = -n;
			
			double n = nn/300.0;
            if (n > 1)
                n = 1;
            else if (n < 0)
                n = 0;

            int m = (int)Math.Floor(n * 1024);

            if (m > 1023)
                m = 1023;
            else if (m < 0)
                m = 0;
			
			byte[] theOut = BitConverter.GetBytes(m);
			
			return theOut;
		}
		
		public void doSyncWriteComplienceSlopeCW(byte[] motor, byte[] CW)
		{
			// cmd = command address
			// motor[] = motor number
			// pos[] = data for the corrosponding motor (horrozontal)
			
			int L = 2;					// length of setting the angle 
			int N = motor.Length;		// number of motors
			byte tLength = (byte)((L+1)*N+4);		// calculate the length
			
			txbuff = new byte[tLength + 4];
			//Console.WriteLine("txBuff Length = "+txbuff.Length.ToString());
			
			txbuff[0] = (byte)255;
			txbuff[1] = (byte)255;
			txbuff[2] = (byte)254;		// hex 0xFE for ID
			txbuff[3] = (byte)tLength;	// length of all commands
			txbuff[4] = (byte)131;		// hex 0x83 (instruction)
			txbuff[5] = (byte)28;		// hex 0x1E goal position
			txbuff[6] = (byte)1;		// length of data for each motor (2) for pos
			
			int iTx = 7;
			for(int i = 0; i < motor.Length ; i++ )
			{
				byte temp = CW[i];
				
				txbuff[iTx] = motor[i];
				txbuff[iTx+1] = temp;
				//Console.WriteLine("pos: "+pos[i].ToString()+"  B[0]: "+temp[0].ToString()+"  B[1]: "+temp[1].ToString());
				iTx = iTx + 2;
			}
			
			// make checksum
			int checkSum = 0;
			
			for(int i = 2; i < (txbuff.Length-1); i++)
			{
				checkSum = checkSum + txbuff[i];
			}
			
			byte[] checkTemp = BitConverter.GetBytes(checkSum);
			
			byte checkSumFinal = (byte)~checkTemp[0];
			//Console.WriteLine("The Checksum: "+checkTemp[0].ToString()+"  then: "+checkSumFinal.ToString());
			
			
			txbuff[txbuff.Length-1] = checkSumFinal;
				
			/*	
			Console.WriteLine("To send:");
			for(int i = 0; i < txbuff.Length ; i++)
			{
				Console.Write(" "+txbuff[i].ToString());
			}
			*/
			
			DynPort.Write(txbuff, 0, txbuff.Length);
		}
		
		public void doSyncWriteComplienceSlopeCCW(byte[] motor, byte[] CCW)
		{
			// cmd = command address
			// motor[] = motor number
			// pos[] = data for the corrosponding motor (horrozontal)
			
			int L = 2;					// length of setting the angle 
			int N = motor.Length;		// number of motors
			byte tLength = (byte)((L+1)*N+4);		// calculate the length
			
			txbuff = new byte[tLength + 4];
			//Console.WriteLine("txBuff Length = "+txbuff.Length.ToString());
			
			txbuff[0] = (byte)255;
			txbuff[1] = (byte)255;
			txbuff[2] = (byte)254;		// hex 0xFE for ID
			txbuff[3] = (byte)tLength;	// length of all commands
			txbuff[4] = (byte)131;		// hex 0x83 (instruction)
			txbuff[5] = (byte)28;		// hex 0x1E goal position
			txbuff[6] = (byte)1;		// length of data for each motor (2) for pos
			
			int iTx = 7;
			for(int i = 0; i < motor.Length ; i++ )
			{
				byte temp = CCW[i];
				
				txbuff[iTx] = motor[i];
				txbuff[iTx+1] = temp;
				//Console.WriteLine("pos: "+pos[i].ToString()+"  B[0]: "+temp[0].ToString()+"  B[1]: "+temp[1].ToString());
				iTx = iTx + 2;
			}
			
			// make checksum
			int checkSum = 0;
			
			for(int i = 2; i < (txbuff.Length-1); i++)
			{
				checkSum = checkSum + txbuff[i];
			}
			
			byte[] checkTemp = BitConverter.GetBytes(checkSum);
			
			byte checkSumFinal = (byte)~checkTemp[0];
			//Console.WriteLine("The Checksum: "+checkTemp[0].ToString()+"  then: "+checkSumFinal.ToString());
			
			
			txbuff[txbuff.Length-1] = checkSumFinal;
				
			/*	
			Console.WriteLine("To send:");
			for(int i = 0; i < txbuff.Length ; i++)
			{
				Console.Write(" "+txbuff[i].ToString());
			}
			*/
			
			DynPort.Write(txbuff, 0, txbuff.Length);
		}

		

    }
}
