using System;

namespace miniHubo
{
	public class randMovements
	{
		static private IK2D m = new IK2D();		// only use for motor num def
		static private int theLength = 70;
		static private int nMotor = 25;
		
		static private double[,] motorBuff = new double[nMotor,theLength];
		static private double[] motorDegSet = new double[nMotor];
		static private double[] motorBuffSum = new double[nMotor];
		static private double[] motorDeg = new double[nMotor];
		static private double[] motorMin = new double[nMotor];
		static private double[] motorMax = new double[nMotor];
		static private int[] motori = new int[nMotor];
		static private Random rand = new Random();
		static private double dMax = 30.0;
		static private double dMin = -30.0;
		
		static private double temp = 0;
		
		static public double degMove = 12.0;
		static public double degPitch = 8.0;
		static public double degSide = 10.0;
		
		public randMovements ()
		{
			setMaxMin();	
		}
		
		
		public void setDeg(IK2D m, int[] ii)
		{
			for(int i = 0; i < ii.Length; i++)
			{
				m.motorDesAngle[ii[i]]=motorBuffSum[ii[i]]/(double)theLength;
			}
		}
		
		public void doRandTop(IK2D m)
		{
			//int[] n = {m.LSP, m.LSR, m.LSY, m.RSP, m.RSR, m.RSY, m.REB, m.LEB, m.WST, m.NK};
			int[] n = {m.LSP, m.LSR, m.LSY, m.RSP, m.RSR, m.RSY, m.REB, m.LEB, m.WST, m.NK};
			setDeg(m,n);
		}
		
		public void doRandPitchAndAnkkles(IK2D m)
		{
			//int[] n = {m.RAR, m.LAR, m.RHR, m.LHR, m.RHP, m.LHP};
			int[] n = {m.RAR, m.LAR, m.RHR, m.LHR};
			setDeg(m,n);
		}
		
		
		public void doSmooth()
		{
			
			for(int i = 0; i < nMotor; i++)
			{
				motorBuffSum[i] = motorBuffSum[i] - motorBuff[i,motori[i]] + motorDeg[i];
				motorBuff[i,motori[i]] = motorDeg[i];
				//m.motorDesAngle[i] = motorBuffSum[i]/(double)theLength;
				
				motori[i] = motori[i]+1;
				if(motori[i] >= theLength)
				{
					motori[i] = 0;
				}
			}
			
		}
		
		public void setMaxMin()
		{
			for(int i = 0; i < nMotor; i++)
			{
				motorMin[i] = dMin;
				motorMax[i] = dMax;
			}
			
		}
		
		public void setRandLowerBody()
		{
			double tt = rand.NextDouble()*degPitch;
			//motorDeg[m.LHP] = tt;
			//motorDeg[m.RHP] = tt;
			
			tt = 2.0*(rand.NextDouble()-0.5)*degSide;
			motorDeg[m.LHR] = tt;
			motorDeg[m.RHR] = tt;
			motorDeg[m.LAR] = -tt;
			motorDeg[m.RAR] = -tt;
		}
		
		public void setRandValues()
		{
			for (int i = 0; i < nMotor; i++)
			{
				motorDeg[i] = 2.0*(rand.NextDouble() - 0.5)*degMove;
				if(motorDeg[i] > dMax)
				{
					motorDeg[i] = dMax;
				}
				else if(motorDeg[i] < dMin)
				{
					motorDeg[i] = dMin;
				}
			}
		}
	}
}

