using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;


using DynamexalClass;

namespace miniHubo
{
	class MainClass
	{
		int HUBO_JOINT_COUNT = 42;              ///> The max number of joints

//                struct hubo_ref {
//                  double[HUBO_JOINT_COUNT] r;// = new double[HUBO_JOINT_COUNT];   // joint reference
//                  int[HUBO_JOINT_COUNT] mode[HUBO_JOINT_COUNT];//   = new double[HUBO_JOINT_COUNT];         // mode 0 = filter mode, 1 = direct reference mode
//                };
		
		static System.Timers.Timer aTimer;
		static IK2D doIK;
		static conducting conduct;		// conducting class
		public static DynClass dynTop;		// Top dynamexel controller
		public static DynClass dynBottom;		// Bottom dynamexel controllers
		static int theTick = 0;
		static int oldTime = 0;
		static int timeCurrent = 0;
		static int T = 20;	// 50ms time
		//static int stepPeriod = 2000; 		// in ms
		static double legBentPercent = 0.2;
		static double stepPercent = 0.4;		// percentage to step forward
		static double stepPercentDefault = 0.4;		// percentage to step forward
		static bool leftStep = true;		// true if taking the left step
		static double stepPeriod = 4;
		static double stepPeriodDefault = 4;
		static double stepHeight = 0.4;
		static double stepHeightDefault = 0.4;
		static double t = 0;				// walking time
		static double hipSway = 0.3;		// amount the hip sways in reference to the leg length
		static double hipSwayDefault = 0.3;		// amount the hip sways in reference to the leg length
		static double rampValue = 0;		// ramp value
		static bool rampUp = true;			// if true will ramp up if false will ramp down
		static bool menuLoop = true;		// if lowered the menu will exit after the next choice
		static bool timerSet = false;		// states if timere is set or not
		static double hipPhase = 0;	// shift hip phase
		static randMovements rm = new randMovements();
		static int ii = 0;
		public static void Main (string[] args)
		{
			dynTop = new DynClass();		// dynamexel class left side
			dynBottom = new DynClass();		// dynamexel class right side
			doIK = new IK2D();			// IK 2d class
			conduct = new conducting();	// conducting class
			
			//setTimer(50);		// set timer loop to 50ms
								// this will also start the interrupt timer rutine
			Console.WriteLine ("Mini Hubo V1");

			doIK.ini("miniHUBOJointINI.txt");	// initilize all joint position in the IK solver
			dispMotorOff();
			
			
			
			iniDynAuto(dynTop, dynBottom, 0, 1, 115200);
			Console.WriteLine("Press any key to continue");
			Console.ReadLine();
			
			Console.WriteLine("Motor Z-Phase");
			doZPhase();
			Console.WriteLine("**Finish** Motor Z-Phase");
			Console.WriteLine("Press <enter> to Continue");
			
			menu();		// start the menu
			
			//setTimer(T);		// set timer loop to T ms
			//Console.WriteLine("timer is running");
			//Console.WriteLine();
			
			//doTestAllJoints();
			
			//Console.ReadLine();
			//setMotorPosMan(dynTop);
			//checkIKman(doIK);
			//setTimer(500);
			
			/*
			while(true)
			{
				//setMotorPosMan(dynBottom);
				checkIKman(doIK);
				Console.ReadLine();
			}
			*/
		}
		private static void setAllSlopes(DynClass dTop, DynClass dBottom)
		{
			int upperBody = 9;
			int lowerBody = 13;
			
			byte[] cTop = new byte[upperBody];
			byte[] cBottom = new byte[lowerBody];
			
			byte[] cTmotor = new byte[upperBody];
			byte[] cBmotor = new byte[lowerBody];
			
			for( int i = 0; i < cTop.Length; i++)
			{
				cTop[i] = 4;
			}
			
			for( int i = 0; i < cBottom.Length; i++)
			{
				cBottom[i] = 4;
			}
			
			
			
			int iT = 0;
			int iB = 0;
			
			for(int i = 0; i<doIK.numberOfMotors; i++)
			{
				string CMD = doIK.motorName[i];
				if(CMD == "LSP" || CMD == "LSR" || CMD == "LSY" || CMD == "LEB" || CMD == "RSP" || CMD == "RSR" || CMD == "RSY" || CMD == "REB" || CMD == "NK")		//TOP DEF
				{
					cTmotor[iT] = (byte)doIK.motorNum[i];
					iT++;
				}
				else if(CMD == "RHP" || CMD == "RHR" || CMD == "RHY" || CMD == "RKN" || CMD == "RAP" || CMD == "RAR" || CMD == "LHP" || CMD == "LHR" || CMD == "LHY" || CMD == "LKN" || CMD == "LAP" || CMD == "LAR" || CMD == "WST")
				{
					cBmotor[iB] = (byte)doIK.motorNum[i];
					iB++;
				}
			}
			
			dTop.doSyncWriteComplienceSlopeCW(cTmotor, cTop);
			dTop.doSyncWriteComplienceSlopeCCW(cTmotor, cTop);
			
			dBottom.doSyncWriteComplienceSlopeCW(cBmotor, cBottom);
			dBottom.doSyncWriteComplienceSlopeCCW(cBottom, cBottom);
			
		}
		private static void doTestAllJoints()
		{
			
			
			bool runThis = true;
			while(runThis)
			{
				Console.Write("Test all motors (y/n)");
				string theStr2 = Console.ReadLine();
				if(theStr2 == "y" || theStr2 == "Y")
				{
					testAllJoints(dynTop, dynBottom);
					Console.WriteLine("Test Complete");
					runThis = false;
					
				}
				else if (theStr2 == "n" || theStr2 == "N" )
				{
					Console.WriteLine("Test aborted");
					runThis = false;
				}
				else
				{
					Console.WriteLine("***Not a valid entry please try again****");
				}
				
			
			}
			
		}
		
		private static void doZPhase()
		{
			bool runThis = true;
			while(runThis)
			{
				Console.Write("Run ZPhase (y/n)");
				string theStr2 = Console.ReadLine();
				if(theStr2 == "y" || theStr2 == "Y")
				{
					MotorZPhase(dynTop, dynBottom);
					Console.WriteLine("ZPhase Complete");
					runThis = false;
					
				}
				else if (theStr2 == "n" || theStr2 == "N" )
				{
					Console.WriteLine("ZPhase aborted");
					runThis = false;
				}
				else
				{
					Console.WriteLine("***Not a valid entry please try again****");
				}
				
			
			}
		}
		private static void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			
			//**************************
			//---Classes to run Start---
			//**************************
			//conduct.mainLoop(doIK);      // conducting loop
			
			//**************************
			//----Classes to run End----
			//**************************
			
			//-----------------------------------------------------
			//-----------------------------------------------------
			//Start area to set the desired location for each joint
			//-----------------------------------------------------
			//-----------------------------------------------------
			
			
			
			walkFeetPos(dynTop, dynBottom, t, stepPeriod, stepPercent, stepHeight ,hipSway, rampValue); 
			
			t = t+(double)T/1000.0;
			

			if(rampUp)
			{
				rampValue = rampValue + ((double)T/1000.0)/stepPeriod;
				if( rampValue >= 1)
					rampValue = 1;
			}
			else
			{
				rampValue = rampValue - ((double)T/1000.0)/stepPeriod;
				if(rampValue <= 0)
					rampValue = 0;
			}
			
			if(t>=stepPeriod)
			{
				t = 0;
				leftStep = !leftStep;
				
			}
			

			//-----------------------------------------------------
			//-----------------------------------------------------
			//End area to set the desired location for each joint
			//-----------------------------------------------------
			//-----------------------------------------------------
			
			
			//-----------------------------------------------------
			//-----------------------------------------------------
			//Start send command to motors
			//-----------------------------------------------------
			//-----------------------------------------------------
			//setAllMotorsSlow(dynTop, dynBottom);
			//-----------------------------------------------------
			//-----------------------------------------------------
			//Start send command to motors
			//-----------------------------------------------------
			//-----------------------------------------------------
			
			//doIK.motorDesAngle[doIK.LEB] = 20;
			//doIK.setMotorValues();
			doRandMovements();
			//doIK.motorDesAngle[doIK.LSP] = 90.0;
			setMotorAll(dynTop, dynBottom);		// sets all motor values
		}
		
		
		static void doRandMovements()
		{
			if (ii > 70)
			{
				ii = 0;
				rm.setRandValues();
				rm.setRandLowerBody();
			}
			
			
			rm.doSmooth ();
			rm.doRandPitchAndAnkkles(doIK);
			rm.doRandTop(doIK);
			ii = ii+1;
			
		}
		
		
		static void walkFeetPos(DynClass dTop, DynClass dBottom, double tt, double period, double deltaX, double deltaZ, double deltaY, double Sw)
		{
			// tt = time in seconds 
			// period = total time of one step 
			// deltaX = change in x (step length)
			// deltaZ = step height (percent of total leg length)
			// Sw = step weight
			
			double t = tt/2;		// cut in half because need more room for pause
			
			
			double zLeg = (doIK.LH + doIK.LK) -((doIK.LH + doIK.LK)*legBentPercent);
			double dZ = zLeg*deltaZ;
			double dY = deltaY*zLeg;
			
			
			double zStep = 0;
			double xStep = 0;
			double yStep = 0;
			
			double zNotStep = 0;
			double xNotStep = 0;
			double yNotStep = 0;
			
			double dYNotStep = 0;
			double dYstep = 0;
			
			
			double periodN = period/4.0;		// new period
			
			if( t <= periodN )		// hip shift portion
			{ 
				zStep = zLeg + (((dZ*Math.Cos(2*Math.PI*0/periodN)-1)/2)*Sw);
				xStep = -(((deltaX*Math.Cos(Math.PI*0/periodN)-1)/2)*Sw);
			 	yStep = (deltaY*Math.Cos(Math.PI*0/periodN)*Sw);
			
			 	zNotStep = zLeg + (((dZ*Math.Cos(2*Math.PI*0/periodN)-1)/2)*Sw);
			 	xNotStep = -xStep;
			 	yNotStep = -yStep;
			 	
				dYNotStep = (dY*Math.Cos(Math.PI*t/periodN+hipPhase)*Sw);
				dYstep = -dYNotStep;
			}
			else
			{
				zStep = zLeg + (((dZ*Math.Cos(2*Math.PI*t/periodN)-1)/2)*Sw);
				xStep = -(((deltaX*Math.Cos(Math.PI*t/periodN)-1)/2)*Sw);
			 	yStep = (deltaY*Math.Cos(Math.PI*0/periodN)*Sw);
			
			 	zNotStep = zLeg + (((dZ*Math.Cos(2*Math.PI*0/periodN)-1)/2)*Sw);
			 	xNotStep = -xStep;
			 	yNotStep = -yStep;
			 	
				dYNotStep = -(dY*Math.Cos(Math.PI*0/periodN+hipPhase)*Sw);
				dYstep = -dYNotStep;
			}
			//double dYstep = -(dY*Math.Cos(Math.PI*t/period-Math.PI/4.0)*Sw);
			
			double[] theOutStep = doIK.getIK2D(doIK.LH, doIK.LK, doIK.x0, doIK.z0, xStep, zStep);
			double[] theOutNotStep = doIK.getIK2D(doIK.LH, doIK.LK, doIK.x0, doIK.z0, xNotStep, zNotStep);
			double[] theOutHipStep = doIK.getIK2DHip(zLeg, dYstep);
			double[] theOutHipNotStep = doIK.getIK2DHip(zLeg, dYNotStep);
			
			if(leftStep)
			{
				doIK.motorDesAngle[doIK.LHP] = theOutStep[0]+doIK.motorIK[doIK.LHP]*Sw;
				doIK.motorDesAngle[doIK.LKN] = theOutStep[1];
				doIK.motorDesAngle[doIK.LAP] = theOutStep[2];
				
				doIK.motorDesAngle[doIK.LHR] = theOutHipStep[0]+doIK.motorIK[doIK.LHR]*Sw;
				//doIK.motorDesAngle[doIK.LHR] = theOutHipStep[0];
				doIK.motorDesAngle[doIK.LAR] = theOutHipStep[1]+doIK.motorIK[doIK.LAR]*Sw;
				
				//Console.Write("  Hip: "+theOutHipStep[0].ToString()+"  ");
				
				doIK.motorDesAngle[doIK.RHP] = theOutNotStep[0]+doIK.motorIK[doIK.RHP]*Sw;
				doIK.motorDesAngle[doIK.RKN] = theOutNotStep[1];
				doIK.motorDesAngle[doIK.RAP] = theOutNotStep[2];
				
				doIK.motorDesAngle[doIK.RHR] = theOutHipStep[0]+doIK.motorIK[doIK.RHR]*Sw;
				//doIK.motorDesAngle[doIK.RHR] = theOutHipStep[0];
				doIK.motorDesAngle[doIK.RAR] = theOutHipStep[1]+doIK.motorIK[doIK.RAR]*Sw;
			}
			else
			{
				doIK.motorDesAngle[doIK.RHP] = theOutStep[0]+doIK.motorIK[doIK.RHP]*Sw;
				doIK.motorDesAngle[doIK.RKN] = theOutStep[1];
				doIK.motorDesAngle[doIK.RAP] = theOutStep[2];
				
				doIK.motorDesAngle[doIK.RHR] = theOutHipNotStep[0]+doIK.motorIK[doIK.RHR]*Sw;
				//doIK.motorDesAngle[doIK.RHR] = theOutHipNotStep[0];
				doIK.motorDesAngle[doIK.RAR] = theOutHipNotStep[1]+doIK.motorIK[doIK.RAR]*Sw;
				//Console.Write("  Hip: "+theOutHipNotStep[0].ToString()+"  ");
				
				doIK.motorDesAngle[doIK.LHP] = theOutNotStep[0]+doIK.motorIK[doIK.LHP]*Sw;
				doIK.motorDesAngle[doIK.LKN] = theOutNotStep[1];
				doIK.motorDesAngle[doIK.LAP] = theOutNotStep[2];
				
				doIK.motorDesAngle[doIK.LHR] = theOutHipNotStep[0]+doIK.motorIK[doIK.LHR]*Sw;
				//doIK.motorDesAngle[doIK.LHR] = theOutHipNotStep[0];
				doIK.motorDesAngle[doIK.LAR] = theOutHipNotStep[1]+doIK.motorIK[doIK.LAR]*Sw;
			}
				
			doIK.setMotorValues();
//			setMotorAll(dTop, dBottom);
			
			
		}
		
		static void walkFeetPosOld(DynClass dTop, DynClass dBottom, double t, double period, double deltaX, double deltaZ, double deltaY, double Sw)
		{
			// t = time in seconds 
			// period = total time of one step 
			// deltaX = change in x (step length)
			// deltaZ = step height (percent of total leg length)
			// Sw = step weight
			
			double zLeg = (doIK.LH + doIK.LK) -((doIK.LH + doIK.LK)*legBentPercent);
			double dZ = zLeg*deltaZ;
			double dY = deltaY*zLeg;
			
			double zStep = zLeg + (((dZ*Math.Cos(2*Math.PI*t/period)-1)/2)*Sw);
			double xStep = -(deltaX*Math.Cos(Math.PI*t/period)*Sw);
			double yStep = deltaY*Math.Cos(Math.PI*t/period)*Sw;
			
			double zNotStep = zLeg + (((dZ*Math.Cos(2*Math.PI*0/period)-1)/2)*Sw);
			double xNotStep = -xStep;
			double yNotStep = -yStep;
			
			//double dYstep = -(dY*Math.Cos(Math.PI*t/period-Math.PI/4.0)*Sw);
			double dYstep = -(dY*Math.Cos(Math.PI*t/period+hipPhase)*Sw);
			double dYNotStep = -dYstep;
			
			double[] theOutStep = doIK.getIK2D(doIK.LH, doIK.LK, doIK.x0, doIK.z0, xStep, zStep);
			double[] theOutNotStep = doIK.getIK2D(doIK.LH, doIK.LK, doIK.x0, doIK.z0, xNotStep, zNotStep);
			double[] theOutHipStep = doIK.getIK2DHip(zLeg, dYstep);
			double[] theOutHipNotStep = doIK.getIK2DHip(zLeg, dYNotStep);
			
			if(leftStep)
			{
				doIK.motorDesAngle[doIK.LHP] = theOutStep[0]+doIK.motorIK[doIK.LHP]*Sw;
				doIK.motorDesAngle[doIK.LKN] = theOutStep[1];
				doIK.motorDesAngle[doIK.LAP] = theOutStep[2];
				
				doIK.motorDesAngle[doIK.LHR] = theOutHipStep[0];
				doIK.motorDesAngle[doIK.LAR] = theOutHipStep[1];
				
				//Console.Write("  Hip: "+theOutHipStep[0].ToString()+"  ");
				
				doIK.motorDesAngle[doIK.RHP] = theOutNotStep[0]+doIK.motorIK[doIK.RHP]*Sw;
				doIK.motorDesAngle[doIK.RKN] = theOutNotStep[1];
				doIK.motorDesAngle[doIK.RAP] = theOutNotStep[2];
				
				doIK.motorDesAngle[doIK.RHR] = theOutHipStep[0];
				doIK.motorDesAngle[doIK.RAR] = theOutHipStep[1];
			}
			else
			{
				doIK.motorDesAngle[doIK.RHP] = theOutStep[0]+doIK.motorIK[doIK.RHP]*Sw;
				doIK.motorDesAngle[doIK.RKN] = theOutStep[1];
				doIK.motorDesAngle[doIK.RAP] = theOutStep[2];
				
				doIK.motorDesAngle[doIK.RHR] = theOutHipNotStep[0];
				doIK.motorDesAngle[doIK.RAR] = theOutHipNotStep[1];
				//Console.Write("  Hip: "+theOutHipNotStep[0].ToString()+"  ");
				
				doIK.motorDesAngle[doIK.LHP] = theOutNotStep[0]+doIK.motorIK[doIK.LHP]*Sw;
				doIK.motorDesAngle[doIK.LKN] = theOutNotStep[1];
				doIK.motorDesAngle[doIK.LAP] = theOutNotStep[2];
				
				doIK.motorDesAngle[doIK.LHR] = theOutHipNotStep[0];
				doIK.motorDesAngle[doIK.LAR] = theOutHipNotStep[1];
			}
				
			doIK.setMotorValues();
//			setMotorAll(dTop, dBottom);
			
			
		}
		
		
		static void setAllMotorsSlow(DynClass dTop, DynClass dBottom)
		{
			string CMD = "L";
			doIK.setMotorValues();
			for(int i = 0; i<doIK.numberOfMotors; i++)
			{
				CMD = doIK.motorName[i];
				if(CMD == "LSP" || CMD == "LSR" || CMD == "LSY" || CMD == "LEB" || CMD == "RSP" || CMD == "RSR" || CMD == "RSY" || CMD == "REB" || CMD == "NK")		//TOP DEF
				{
					dTop.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorDesAngleWithOffset[i]));
				}
				else if(CMD == "RHP" || CMD == "RHR" || CMD == "RHY" || CMD == "RKN" || CMD == "RAP" || CMD == "RAR" || CMD == "LHP" || CMD == "LHR" || CMD == "LHY" || CMD == "LKN" || CMD == "LAP" || CMD == "LAR" || CMD == "WST")
				{
					dBottom.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorDesAngleWithOffset[i]));
				}
			}
		}
		
		static void setMotorWithIK(int motorNum, DynClass dTop, DynClass dBottom)
		{
			int i = motorNum;
			string CMD = doIK.motorName[i];
			if(CMD == "LSP" || CMD == "LSR" || CMD == "LSY" || CMD == "LEB" || CMD == "RSP" || CMD == "RSR" || CMD == "RSY" || CMD == "REB" || CMD == "NK")		//TOP DEF
			{
				dTop.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorDesAngleWithOffsetIK[i]));
			}
			else if(CMD == "RHP" || CMD == "RHR" || CMD == "RHY" || CMD == "RKN" || CMD == "RAP" || CMD == "RAR" || CMD == "LHP" || CMD == "LHR" || CMD == "LHY" || CMD == "LKN" || CMD == "LAP" || CMD == "LAR" || CMD == "WST")
			{
				dBottom.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorDesAngleWithOffsetIK[i]));
			}
		}
		
		static void setMotor(int motorNum, DynClass dTop, DynClass dBottom)
		{
			int i = motorNum;
			string CMD = doIK.motorName[i];
			if(CMD == "LSP" || CMD == "LSR" || CMD == "LSY" || CMD == "LEB" || CMD == "RSP" || CMD == "RSR" || CMD == "RSY" || CMD == "REB" || CMD == "NK")		//TOP DEF
			{
				dTop.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorDesAngleWithOffset[i]));
			}
			else if(CMD == "RHP" || CMD == "RHR" || CMD == "RHY" || CMD == "RKN" || CMD == "RAP" || CMD == "RAR" || CMD == "LHP" || CMD == "LHR" || CMD == "LHY" || CMD == "LKN" || CMD == "LAP" || CMD == "LAR" || CMD == "WST")
			{
				dBottom.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorDesAngleWithOffset[i]));
			}
		}
		
		public static void setMotorAll(DynClass dTop, DynClass dBottom)
		{
			int upperBody = 9;
			int lowerBody = 13;
			
			byte[] motorNumTop = new byte[upperBody];
			byte[] motorNumBottom = new byte[lowerBody];
			
			double[] motorPosTop = new double[upperBody];
			double[] motorPosBottom = new double[lowerBody];
			
			int iT = 0;		// top indexer
			int iB = 0;		// bottom indexer
			
			string CMD = "L";
			
			
			
			
			
			doIK.setMotorValues();
			for(int i = 0; i<doIK.numberOfMotors; i++)
			{
				CMD = doIK.motorName[i];
				if(CMD == "LSP" || CMD == "LSR" || CMD == "LSY" || CMD == "LEB" || CMD == "RSP" || CMD == "RSR" || CMD == "RSY" || CMD == "REB" || CMD == "NK")		//TOP DEF
				{
					motorNumTop[iT] = (byte)doIK.motorNum[i];
					motorPosTop[iT] = doIK.motorDesAngleWithOffset[i];
					iT++;
				}
				else if(CMD == "RHP" || CMD == "RHR" || CMD == "RHY" || CMD == "RKN" || CMD == "RAP" || CMD == "RAR" || CMD == "LHP" || CMD == "LHR" || CMD == "LHY" || CMD == "LKN" || CMD == "LAP" || CMD == "LAR" || CMD == "WST")
				{
					motorNumBottom[iB] = (byte)doIK.motorNum[i];
					motorPosBottom[iB] = doIK.motorDesAngleWithOffset[i];
					iB++;
				}
			}
			
			dTop.doSyncWritePos(motorNumTop, motorPosTop);
			dBottom.doSyncWritePos(motorNumBottom, motorPosBottom);
		}
		
		
		static void dispMotorOff()
		{
			for(int i = 0; i<doIK.numberOfMotors; i++)
			{
				Console.WriteLine(doIK.motorName[i]+": Num = "+doIK.motorNum[i].ToString()+"  Offset = "+doIK.motorOfset[i].ToString()+" IK Off = " + doIK.motorIK[i].ToString()+"  Direct = "+doIK.motorDir[i].ToString());
			}
		}
		
		static void MotorZPhase(DynClass dTop, DynClass dBottom)
		{
			// sets all motors to their zero position as defined by the offset file
			for(int i =0; i<doIK.numberOfMotors; i++)
			{
				//Console.Write(i.ToString());
				string CMD = doIK.motorName[i];
				if(CMD == "LSP" || CMD == "LSR" || CMD == "LSY" || CMD == "LEB" || CMD == "RSP" || CMD == "RSR" || CMD == "RSY" || CMD == "REB" || CMD == "NK")		//TOP DEF
				{
					Console.WriteLine("Top: Motor: "+((byte)doIK.motorNum[i]).ToString()+" to: "+doIK.motorOfset[i].ToString());
					Console.ReadLine();
					dTop.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorOfset[i]));
				}
				else if(CMD == "RHP" || CMD == "RHR" || CMD == "RHY" || CMD == "RKN" || CMD == "RAP" || CMD == "RAR" || CMD == "LHP" || CMD == "LHR" || CMD == "LHY" || CMD == "LKN" || CMD == "LAP" || CMD == "LAR" || CMD == "WST")
				{
					Console.WriteLine("Bottom: Motor: "+((byte)doIK.motorNum[i]).ToString()+" to: "+doIK.motorOfset[i].ToString());
					Console.ReadLine();
					dBottom.setPos((byte)doIK.motorNum[i], deg2one(doIK.motorOfset[i]));
				}
				
			}
		}
		private static void setTimer(double t)
		{
			// t = the even timing
			if(!timerSet)
			{
				aTimer = new System.Timers.Timer(t);
		
		        // Hook up the Elapsed event for the timer.
		        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
		
		        // Set the Interval to 2 seconds (2000 milliseconds).
		        //aTimer.Interval = 2000;
		        aTimer.Enabled = true;
		
		        //Console.WriteLine("Press the Enter key to exit the program.");
		        //Console.ReadLine();
				
				timerSet = true;
				Console.WriteLine("Timer now is running");
			}
		}
		private static void OnTimedEventMainTimer(object source, ElapsedEventArgs e)
		{
			
		}
		private static void OnTimedEventTest(object source, ElapsedEventArgs e)
    	{
      		//Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
			doIK.getIK2D(0.1,0.8,1,1,1,1);
			doIK.getIK2D(0.1,0.8,1,1,1,1);
			if(theTick < 50)
			{
				theTick++;
			}
			else
			{
				theTick = 0;
				int theOut = 0;
				int oldTemp = e.SignalTime.Millisecond;
				if(oldTemp > oldTime)
				{
					theOut = oldTemp-oldTime;
				}
				else
				{
					theOut = oldTime -oldTemp;
				}
				
				oldTime = oldTemp;
				Console.WriteLine("The Elapsed event was raised at {0}", theOut);
			}
				
    	}

		
		public static void iniDynAuto(DynClass dLeft, DynClass dRight, int cleft, int cright, int theBaud)
		{
			bool keepRunning = true;
			int osChoice = 1;
			string comLeft = "COM";
			string comRight = "COM";
			Console.WriteLine("Ini for Mini hubo (auto)");
			while(keepRunning)
			{
				try
				{
					Console.WriteLine("System: ");
					Console.WriteLine("(1) - Linux");
					Console.WriteLine("(2) - Windows");
					Console.Write("Choice: ");
					string theChoice = Console.ReadLine();
					
					int theNUM = int.Parse(theChoice);
					if(theNUM < 3 && theNUM > 0)
					{
						osChoice = theNUM;
						keepRunning = false;
					}
					else
						Console.WriteLine("Invalad Entry Please try again");

				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			if(osChoice == 1)
			{
				comLeft = "/dev/ttyUSB";
				comRight = "/dev/ttyUSB";
			}
			else if(osChoice == 2)
			{
				comLeft = "COM";
				comRight = "COM";
			}
			
			comLeft = comLeft+cleft.ToString();
			comRight = comRight+cright.ToString();
			
		
			
			dLeft.ComBaud = theBaud;
			dRight.ComBaud = theBaud;
			dLeft.ComPort = comLeft;
			dRight.ComPort = comRight;
			
			bool theOut = dLeft.ini();
			if(theOut)
				Console.WriteLine("Dyn LEFT Setup Successfully");
			else
				Console.WriteLine("***ERROR*** Dyn LEFT NOT Setup Successfully: "+comLeft.ToString());
			
			theOut = dRight.ini();
			if(theOut)
				Console.WriteLine("Dyn RIGHT Setup Successfully");
			else
				Console.WriteLine("***ERROR*** Dyn RIGHT NOT Setup Successfully "+comRight.ToString());
			
		}
			
		public static void iniDyn(DynClass dyn, string theName)
		{
			bool keepRunning = true;
			int osChoice = 1;
			Console.WriteLine("Ini for Mini hubo "+theName+" side");
			while(keepRunning)
			{
				try
				{
					Console.WriteLine("System: ");
					Console.WriteLine("(1) - Linux");
					Console.WriteLine("(2) - Windows");
					Console.WriteLine("(3) - Custom Com Def");
					Console.Write("Choice: ");
					string theChoice = Console.ReadLine();
					
					int theNUM = int.Parse(theChoice);
					dyn.ComPort = "COM"+theNUM.ToString();
					Console.WriteLine("Dyn COM Port Set - "+dyn.ComPort);
					Console.WriteLine(" ");
					if(theNUM < 4 && theNUM > -1)
					{
						osChoice = theNUM;
						keepRunning = false;
					}
					else
						Console.WriteLine("Invalad Entry Please try again");

				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			string theOSChoice = "";
			switch (osChoice)
			{
			case 1:
				theOSChoice = "/dev/ttyUSB";
				break;
			case 2:
				theOSChoice = "COM";
				break;
			case 3:
				theOSChoice ="";
				break;
			}
			while(true)
			{
				try
				{
					Console.Write("Dyn Com Port: ");
					string theCOM = Console.ReadLine();
					
					int theNUM = int.Parse(theCOM);
					dyn.ComPort = theOSChoice+theNUM.ToString();
					Console.WriteLine("Dyn COM Port Set - "+dyn.ComPort);
					Console.WriteLine(" ");
					break;
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			
			Console.WriteLine("Select Dyn Com Baud:");
			Console.WriteLine("(1) - 1000000 Baud (Default)");
			Console.WriteLine("(2) - 500000 Baud");
			Console.WriteLine("(3) - 115200 Baud");
			Console.WriteLine("(4) - 57600 Baud");
			Console.Write("(5) - 19200 Baud  -  Select: ");
			string theBaud = Console.ReadLine();
		
			int theNum = 0;
			try
			{
				int.TryParse(theBaud, out theNum);
			}
			catch(Exception e)
			{
				Console.WriteLine("Not a valid input setting default value");
			}
			switch (theNum)
			{
				case 1:
					dyn.ComBaud = 1000000;
					Console.WriteLine("Dyn Baud Set - 1000000 Baud");
					break;
					
				case 2:
					dyn.ComBaud = 500000;
					Console.WriteLine("Dyn Baud Set - 500000 Baud");
					break;
					
				case 3:
					dyn.ComBaud = 115200;
					Console.WriteLine("Dyn Baud Set - 115200 Baud");
					break;
				
				case 4:
					dyn.ComBaud = 57600;
					Console.WriteLine("Dyn Baud Set - 57600 Baud");
					break;
				
				case 5:
					dyn.ComBaud = 19200;
					Console.WriteLine("Dyn Baud Set - 19200 Baud");
					break;
					
				default:
					dyn.ComBaud = 1000000;
					Console.WriteLine("Not a valid input setting default value");
					Console.WriteLine("(By Default) - Dyn Baud Set - 1000000 Baud");
					break;
				
			}
			
			bool theOut = dyn.ini();
			if(theOut)
				Console.WriteLine("Dyn Setup Successfully");
			else
				Console.WriteLine("***ERROR*** Dyn NOT Setup Successfully");
			
			//Console.ReadLine();
			
		
			
			
		}
		
		public static void setMotorPosMan(DynClass dyn)
		{
			// set the motor position by a command line input
			// only for possition commands
			
			byte theNum = 0;		// motor number
			double theDeg = 0; 	// motor pos in degreese
				
				
			while(true)
			{
				try
				{
					Console.Write("Dyn Motor Num: ");
					string theMotorNum = Console.ReadLine();
					
					theNum = byte.Parse(theMotorNum);
					Console.WriteLine("Motor Num Set - "+theNum.ToString());
					Console.WriteLine(" ");
					break;
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			
			
			while(true)
			{
				try
				{
					
					Console.Write("Dyn Motor "+theNum.ToString()+" Desired Pos (0-300 deg): ");
					string theMotorPos = Console.ReadLine();
					
					theDeg = double.Parse(theMotorPos);
					
					Console.WriteLine("Motor "+theNum.ToString()+" Pos Set - "+theDeg.ToString()+" deg");
					Console.WriteLine(" ");
					break;
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			
			if(theDeg > 300)
			{
				theDeg = 300;
			}
			
			if( theDeg < 0)
			{
				theDeg = 0;
			}
			
			dyn.setPos(theNum, deg2one(theDeg));
			
			Console.WriteLine("Position Command Sent for Motor "+theNum.ToString()+" to pos "+theDeg.ToString()+" deg");
		}
		
		static double deg2one(double deg)
		{
			// converts 0-300 deg into -1 to 1 value with:
			// 
			// 0 = 0 deg
			// 1 = 300 deg
			
			if(deg > 300)
				deg = 300;
			if(deg < 0)
				deg = 0;
			
			double theOut = deg/300;
			
			Console.WriteLine("num sent = "+theOut.ToString());
			return theOut;
		}
		
		static void checkIKman(IK2D doIK)
		{
			Console.WriteLine("Check 2D IK for Jaemi Hubo");
			Console.WriteLine("Note LH = " + doIK.LH.ToString());
			Console.WriteLine("     LK = " + doIK.LK.ToString());
			
			double x = 0;
			double z = 0;
			while(true)
			{
				try{
					Console.Write("Des x = ");
					string theStr = Console.ReadLine();
					x = double.Parse(theStr);
					break;
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			
			while(true)
			{
				try{
					Console.Write("Des z = ");
					string theStr2 = Console.ReadLine();
					z = double.Parse(theStr2);
					break;
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}
			
			
			double[] theOut = doIK.getIK2D(doIK.LH, doIK.LK, doIK.x0, doIK.z0, x,z);
			
			Console.WriteLine("2D IK Results: TH = "+theOut[0].ToString());
			Console.WriteLine("               TK = "+theOut[1].ToString());
			Console.WriteLine("               TA = "+theOut[2].ToString());
			
			doIK.motorDesAngle[doIK.LHP] = theOut[0];
			doIK.motorDesAngle[doIK.LKN] = theOut[1];
			doIK.motorDesAngle[doIK.LAP] = theOut[2];
			
			doIK.motorDesAngle[doIK.RHP] = theOut[0];
			doIK.motorDesAngle[doIK.RKN] = theOut[1];
			doIK.motorDesAngle[doIK.RAP] = theOut[2];
			
			doIK.setMotorValues();
			
			Console.WriteLine("After offsets: TH = "+(doIK.motorDesAngleWithOffset[doIK.LHP]).ToString());
			Console.WriteLine("               TK = "+(doIK.motorDesAngleWithOffset[doIK.LKN]).ToString());
			Console.WriteLine("               TA = "+(doIK.motorDesAngleWithOffset[doIK.LAP]).ToString());
			
			Console.WriteLine("After IK offsets: TH = "+(doIK.motorDesAngleWithOffsetIK[doIK.LHP]).ToString());
			Console.WriteLine("                  TK = "+(doIK.motorDesAngleWithOffsetIK[doIK.LKN]).ToString());
			Console.WriteLine("                  TA = "+(doIK.motorDesAngleWithOffsetIK[doIK.LAP]).ToString());
			
			Console.WriteLine("After IK offsets only: TH = "+(doIK.motorDesAngleWithOffsetIK[doIK.LHP]-doIK.motorOfset[doIK.LHP]).ToString());
			Console.WriteLine("                       TK = "+(doIK.motorDesAngleWithOffsetIK[doIK.LKN]-doIK.motorOfset[doIK.LKN]).ToString());
			Console.WriteLine("                       TA = "+(doIK.motorDesAngleWithOffsetIK[doIK.LAP]-doIK.motorOfset[doIK.LAP]).ToString());
			
			bool runThis = true;
			while(runThis)
			{
				Console.Write("Send to Left leg (y/n)");
				string theStr2 = Console.ReadLine();
				if(theStr2 == "y" || theStr2 == "Y")
				{
					/*
					setMotor(doIK.LHP, dynTop, dynBottom);
					Thread.Sleep(100);
					setMotor(doIK.LKN, dynTop, dynBottom);
					Thread.Sleep(100);
					setMotor(doIK.LAP, dynTop, dynBottom);
					Thread.Sleep(100);
					
					setMotor(doIK.RHP, dynTop, dynBottom);
					Thread.Sleep(100);
					setMotor(doIK.RKN, dynTop, dynBottom);
					Thread.Sleep(100);
					setMotor(doIK.RAP, dynTop, dynBottom);
					Thread.Sleep(100);
					*/
					
					setMotorAll(dynTop, dynBottom);
					runThis = false;
					
				}
				else if (theStr2 == "n" || theStr2 == "N" )
				{
					runThis = false;
				}
				else
				{
					Console.WriteLine("***Not a valid entry please try again****");
				}
				
			
			}
			
			
			
		}
		
		static void checkLoopSpeed(IK2D doIK, int n)
		{
			// checks the amount of time it takes to finish n loops of doIK
			
			
		}
		
		static void testAllJoints(DynClass dTop, DynClass dBottom)
		{
			for(int i = 0; i < doIK.numberOfMotors ; i++)
			{
				doIK.motorDesAngle[i] = 15;
			}
			
			doIK.setMotorValues();
			
			for(int i = 0; i < doIK.numberOfMotors ; i++)
			{
				setMotor(i, dTop, dBottom);
				Thread.Sleep(100);
			}
			
			for(int i = 0; i < doIK.numberOfMotors ; i++)
			{
				doIK.motorDesAngle[i] = -15;
			}
			
			doIK.setMotorValues();
			
			for(int i = 0; i < doIK.numberOfMotors ; i++)
			{
				setMotor(i, dTop, dBottom);
				Thread.Sleep(100);
			}
			
		}
		
		static void doConducting()
		{
		}
		

static double rad2deg(double rad) {
        
	return (rad*180.0/3.1415962);
}

		static void doAch2UDP() {
			Console.WriteLine("Udp2Ref Mini-Hubo from Hubo-Ach");
                        int RHYi = 26; //       Right Hip Yaw
                        int RHRi = 27; //       Right Hip Roll
                        int RHPi = 28; //       Right Hip Pitch
                        int RKNi = 29; //       Right Knee Pitch
                        int RAPi = 30; //       Right Ankle Pitch
                        int RARi = 31; //       Right Ankle Roll

                        int LHYi = 19; //       Left Hip Yaw
                        int LHRi = 20; //       Left Hip Roll
                        int LHPi = 21; //       Left Hip Pitch
                        int LKNi = 22; //       Left Knee Pitch
                        int LAPi = 23; //       Left Ankle Pitch
                        int LARi = 24; //       Left Ankle Roll

                        int RSPi = 11; //       Right Shoulder Pitch
                        int RSRi = 12; //       Right Shoulder Roll
                        int RSYi = 13; //       Right Shoulder Yaw
                        int REBi = 14; //       Right Elbow Pitch
                        int RWYi = 15; //  right wrist yaw
                        int RWRi = 16; //  right wrist roll
                        int RWPi = 17; //  right wrist Pitch

                        int LSPi = 4;  //       Left Shoulder Pitch
                        int LSRi = 5;  //       Left Shoulder Yaw
                        int LSYi = 6;  //       Left Shoulder Roll
                        int LEBi = 7;  //       Left Elbow Pitch
                        int LWYi = 8;  //  left wrist yaw
                        int LWRi = 9;  //  left wrist roll
                        int LWPi = 10; //  left wrist pitch

                        int NKYi = 1;  //  neck yaw
                        int NK1i = 2;  //  neck 1
                        int NK2i = 3;  //  neck 2

                        int WSTi = 0;  //       Trunk Yaw

                        int RF1i = 32; //       Right Finger
                        int RF2i = 33; //       Right Finger
                        int RF3i = 34; //       Right Finger
                        int RF4i = 35; //       Right Finger
                        int RF5i = 36; //       Right Finger
                        int LF1i = 37; //       Left Finger
                        int LF2i = 38; //       Left Finger
                        int LF3i = 39; //       Left Finger
                        int LF4i = 40; //       Left Finger
                        int LF5i = 41; //       Left Finger

			IPAddress rxAddress = IPAddress.Any;
               		IPEndPoint rxEndPoint;
                	UdpClient sock;
                	//private byte[] rxBuff = new byte[1024];
                	int socketNum = 5005;	
			rxEndPoint = new IPEndPoint(rxAddress, socketNum);
                        sock = new UdpClient(socketNum);	
			int bitL = 8;
			while(true){
                          byte[] rxData = sock.Receive(ref rxEndPoint);
//                          double WST = BitConverter.ToDouble(rxData,0*4);
//                          double RHY = BitConverter.ToDouble(rxData,1*8);
//			  Console.WriteLine("WST = "+WST.ToString()+" NKY = "+ RHY.ToString());

                          doIK.motorDesAngle[doIK.WST] = -rad2deg(BitConverter.ToDouble(rxData,WSTi*bitL));

                          doIK.motorDesAngle[doIK.RSP] = -rad2deg(BitConverter.ToDouble(rxData,RSPi*bitL));
                          doIK.motorDesAngle[doIK.RSR] = -rad2deg(BitConverter.ToDouble(rxData,RSRi*bitL));
                          doIK.motorDesAngle[doIK.RSY] = -rad2deg(BitConverter.ToDouble(rxData,RSYi*bitL));
                          doIK.motorDesAngle[doIK.REB] = rad2deg(BitConverter.ToDouble(rxData,REBi*bitL));  // Broken

                          doIK.motorDesAngle[doIK.LSP] = -rad2deg(BitConverter.ToDouble(rxData,LSPi*bitL));
                          doIK.motorDesAngle[doIK.LSR] =  rad2deg(BitConverter.ToDouble(rxData,LSRi*bitL));
                          doIK.motorDesAngle[doIK.LSY] = -rad2deg(BitConverter.ToDouble(rxData,LSYi*bitL));
                          doIK.motorDesAngle[doIK.LEB] = -rad2deg(BitConverter.ToDouble(rxData,LEBi*bitL));

                          doIK.motorDesAngle[doIK.RHP] =  rad2deg(BitConverter.ToDouble(rxData,RHPi*bitL));
                          doIK.motorDesAngle[doIK.RHR] =  rad2deg(BitConverter.ToDouble(rxData,RHRi*bitL));
                          doIK.motorDesAngle[doIK.RHY] =  rad2deg(BitConverter.ToDouble(rxData,RHYi*bitL));
                          doIK.motorDesAngle[doIK.RKN] =  rad2deg(BitConverter.ToDouble(rxData,RKNi*bitL));
                          doIK.motorDesAngle[doIK.RAP] =  rad2deg(BitConverter.ToDouble(rxData,RAPi*bitL));
                          doIK.motorDesAngle[doIK.RAR] =  rad2deg(BitConverter.ToDouble(rxData,RARi*bitL));

                          doIK.motorDesAngle[doIK.LHP] =  rad2deg(BitConverter.ToDouble(rxData,LHPi*bitL));
                          doIK.motorDesAngle[doIK.LHR] =  rad2deg(BitConverter.ToDouble(rxData,LHRi*bitL));
                          doIK.motorDesAngle[doIK.LHY] =  rad2deg(BitConverter.ToDouble(rxData,LHYi*bitL));
                          doIK.motorDesAngle[doIK.LKN] =  rad2deg(BitConverter.ToDouble(rxData,LKNi*bitL));
                          doIK.motorDesAngle[doIK.LAP] =  rad2deg(BitConverter.ToDouble(rxData,LAPi*bitL));
                          doIK.motorDesAngle[doIK.LAR] =  rad2deg(BitConverter.ToDouble(rxData,LARi*bitL));
//			Console.WriteLine(doIK.motorDesAngle[doIK.RHR].ToString());
//			  doIK.motorDesAngle[doIK.RAP] = 0;
			  setMotorAll(dynTop, dynBottom);
			}
		}
		static void menu()
		{
			
			
			
			while(menuLoop)
			{
				try
				{
					Console.WriteLine("miniHubo Main Menue:");
					Console.WriteLine("(0) Exit");
					Console.WriteLine("(1) Start Main Loop");
					Console.WriteLine("(2) Walk in place");
					Console.WriteLine("(3) Walk Forwards");
					Console.WriteLine("(4) Walk Backwards");
					Console.WriteLine("(5) Stop");
					Console.WriteLine("(6) Change Default Values");
					Console.WriteLine("(7) Start Conducting");
					Console.WriteLine("(8) Stop Conducting");
					Console.WriteLine("(9) Set Motor Angel");
					Console.WriteLine("(10) UDP Conducting Loop");
					Console.WriteLine("(11) UDP Conducting Loop STOP");
					Console.WriteLine("(12) Ach2UDP");
					Console.Write("Choice: ");
					string theChoiceString = Console.ReadLine();
					
					int theChoice = int.Parse(theChoiceString);
					
					Console.WriteLine("Option "+theChoice.ToString()+" chosen");
					bool theChoiceBool = onChoice(theChoice);
					if(!theChoiceBool)
					{
						Console.WriteLine("Invalad Entry Please try again");
					}
					Console.WriteLine("******************************");
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}	

		}
		
		static bool onChoice(int choice)
		{
			bool choiceBool = true;
			
			switch (choice)
			{
			case 0:			//	Exit
				Console.WriteLine("Exit Program");
				onExit();
				break;
			case 1:			// Start Main Loop
				rampUp = false;
				setTimer(T);		// set timer loop to T ms
				Console.WriteLine("Main Loop Started");
				break;
				
			case 2:			// Walk in place
				stepHeight = stepHeightDefault;
				stepPercent = 0;
				stepPeriod = stepPeriodDefault;
				hipSway = hipSwayDefault;
				rampUp = true;
				Console.WriteLine("Walk in Place Started");
				break;
				
			case 3:			// Walk Forwards
				stepHeight = stepHeightDefault;
				stepPercent = stepPercentDefault;
				hipSway = hipSwayDefault;
				rampUp = true;
				Console.WriteLine("Walk Forwards");
				break;
				
			case 4:			// Walk Backwards
				stepHeight = stepHeightDefault;
				stepPercent = -stepPercentDefault;
				hipSway = hipSwayDefault;
				rampUp = true;
				Console.WriteLine("Walk Backwards");
				break;
				
			case 5: 		// Stop
				rampUp = false;
				Console.WriteLine("Stop Walking");
				break;
				
			case 6:			// Set Default Values
				//rampUp = false;		// Stops the movement
				onSetDefaultValue();
				break;
				
			case 7:			// Conducting
				conduct.T = T;
				conduct.doLoop = true;
				break;
				
			case 8:
				conduct.doLoop = false;
				break;
			case 9:
				setJointsCommandLine();
				break;
			case 10:
				Console.WriteLine("Going to UDP Conducting Mode");
				conduct.doLoop = false;		// turn off the main loop
				conduct.doUdpLoop = true;
				 // start main loop to put in crouch pos
				//setTimer(T);		// set timer loop to T ms
				//Thread.Sleep(2*T);
				//aTimer.Stop();
				walkFeetPos(dynTop, dynBottom, t, stepPeriod, stepPercent, stepHeight ,hipSway, rampValue); 
				setMotorAll(dynTop, dynBottom);		// sets all motor values
				conduct.udpINI();
				conduct.udpLoop(1, doIK);
				break;
			case 11:
				conduct.doUdpLoop = false;
				break;
			case 12:
				Console.WriteLine("Ach2UDP");
				doAch2UDP();
				break;
				
			default:
				choiceBool = false;
				break;
			}
			
			return choiceBool;
		}
		
		static void onExit()
		{
			//aTimer.Stop();
			//aTimer.Close();
			//aTimer.Dispose();
			timerSet = false;
			menuLoop = false;
			Environment.Exit(Environment.ExitCode);
			
		}
		
		static void setJointsCommandLine()
		{
			bool loopRun = true;
			while(loopRun)
			{
				try{
					Console.WriteLine("### Set Motor Angles");
					Console.WriteLine("(0) Exit");
					Console.WriteLine("-- Top --");
					Console.WriteLine("(1) LSP");
					Console.WriteLine("(2) LSR");
					Console.WriteLine("(3) LSY");
					Console.WriteLine("(4) LEB");
					Console.WriteLine("(5) RSP");
					Console.WriteLine("(6) RSR");
					Console.WriteLine("(7) RSY");
					Console.WriteLine("(8) REB");
					Console.WriteLine("(9) WST");
					Console.WriteLine("-- Bottom --");
					Console.WriteLine("(10)  LHP");
					Console.WriteLine("(11) LHR");
					Console.WriteLine("(12) LHY");
					Console.WriteLine("(13) LKN");
					Console.WriteLine("(14) LAP");
					Console.WriteLine("(15) LAR");
					Console.WriteLine("(16) RHP");
					Console.WriteLine("(17) RHR");
					Console.WriteLine("(18) RHY");
					Console.WriteLine("(19) RKN");
					Console.WriteLine("(20) RAP");
					Console.WriteLine("(21) RAR");
					Console.Write("Choice: ");
					string theChoiceString = Console.ReadLine();
					int theChoice = int.Parse(theChoiceString);
					int theJoint = -1;
					
					switch (theChoice){
					case 0:
						loopRun = false;
						break;
					case 1:
						theJoint = doIK.LSP;
						break;
					case 2:
						theJoint = doIK.LSR;
						break;
					case 3:
						theJoint = doIK.LSY;
						break;
					case 4:
						theJoint = doIK.LEB;
						break;
					case 5:
						theJoint = doIK.RSP;
						break;
					case 6:
						theJoint = doIK.RSR;
						break;
					case 7:
						theJoint = doIK.RSY;
						break;
					case 8:
						theJoint = doIK.REB;
						break;
					case 9:
						theJoint = doIK.WST;
						break;
					case 10:
						theJoint = doIK.LHP;
						break;
					case 11:
						theJoint = doIK.LHR;
						break;
					case 12:
						theJoint = doIK.LHY;
						break;
					case 13:
						theJoint = doIK.LKN;
						break;
					case 14:
						theJoint = doIK.LAP;
						break;
					case 15:
						theJoint = doIK.LAR;
						break;
					case 16:
						theJoint = doIK.RHP;
						break;
					case 17:
						theJoint = doIK.RHR;
						break;
					case 18:
						theJoint = doIK.RHY;
						break;
					case 19:
						theJoint = doIK.RKN;
						break;
					case 20:
						theJoint = doIK.RAP;
						break;
					case 21:
						theJoint = doIK.RAR;
						break;
					default:
						theJoint = -1;
						break;
						
					}
					
					if (theJoint >=0)
					{
						Console.Write("Set "+doIK.motorName[theJoint]+" to (deg): ");
						string tChoice = Console.ReadLine();
						double dChoice = double.Parse(tChoice);
						doIK.motorDesAngle[theJoint] = dChoice;
						doIK.setMotorValues();
					}
					
					setMotorAll(dynTop, dynBottom);
					
				}
				catch( Exception ee)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
				
			}
		}
		
		
		static void onSetDefaultValue()
		{
			Console.WriteLine("Current Values:");
			Console.WriteLine("[] stepHeightDefault  (%) = "+stepHeightDefault.ToString());
			Console.WriteLine("[] stepPercentDefault (%) = "+stepPercentDefault.ToString());
			Console.WriteLine("[] hipSwayDefault     (%) = "+hipSwayDefault.ToString());
			Console.WriteLine("******************************");
			bool loopRun = true;
			while(loopRun)
			{
				try
				{
					Console.WriteLine("### Change Default Values:");
					Console.WriteLine("(0) Exit");
					Console.WriteLine("(1) Step Height (%)");
					Console.WriteLine("(2) Step Length (%)");
					Console.WriteLine("(3) Hip Sway (%)");
					Console.WriteLine("(4) Hip Phase (rad)");
					Console.Write("Choice: ");
					string theChoiceString = Console.ReadLine();
					
					int theChoice = int.Parse(theChoiceString);
					
					Console.WriteLine("Option "+theChoice.ToString()+" chosen");
					
					switch (theChoice)
					{
					case 0:
						loopRun = false;
						break;
						
					case 1:
						try
						{
							Console.Write("[][]Step Height (%) (Curent= "+stepHeightDefault.ToString()+" Set: ");
							string tChoice = Console.ReadLine();
							double dChoice = double.Parse(tChoice);
							stepHeightDefault = dChoice;
						}
						catch( Exception ee)
						{
							Console.WriteLine("Invalad Entry Please try again");
						}
						break;
						
					case 2:
						try
						{
							Console.Write("[][]Step Length (%) (Curent= "+stepPercentDefault.ToString()+" Set: ");
							string tChoice = Console.ReadLine();
							double dChoice = double.Parse(tChoice);
							stepPercentDefault = dChoice;
						}
						catch( Exception ee)
						{
							Console.WriteLine("Invalad Entry Please try again");
						}
						break;
						
						
					case 3:
						try
						{
							Console.Write("[][]Hip Sway (%) (Curent= "+hipSwayDefault.ToString()+" Set: ");
							string tChoice = Console.ReadLine();
							double dChoice = double.Parse(tChoice);
							hipSwayDefault = dChoice;
						}
						catch( Exception ee)
						{
							Console.WriteLine("Invalad Entry Please try again");
						}
						break;
						
					case 4:
						try
						{
							Console.Write("[][]Hip Phase (rad) (Curent= "+ hipPhase.ToString()+" Set: ");
							string tChoice = Console.ReadLine();
							double dChoice = double.Parse(tChoice);
							hipPhase = dChoice;
						}
						catch( Exception ee)
						{
							Console.WriteLine("Invalad Entry Please try again");
						}
						break;
						
				
					
					default:
							Console.WriteLine("Invalad Entry Please try again");
						break;
						
				}
					Console.WriteLine("Default Values Set:");
					Console.WriteLine("[] stepHeightDefault  (%) = "+stepHeightDefault.ToString());
					Console.WriteLine("[] stepPercentDefault (%) = "+stepPercentDefault.ToString());
					Console.WriteLine("[] hipSwayDefault     (%) = "+hipSwayDefault.ToString());
					Console.WriteLine("[] hipPhase	       (rad) = "+hipPhase.ToString());
					Console.WriteLine("******************************");
	
				}
				catch(Exception e)
				{
					Console.WriteLine("Invalad Entry Please try again");
				}
			}	
			
			Console.WriteLine("Default Values Set:");
			Console.WriteLine("[] stepHeightDefault  (%) = "+stepHeightDefault.ToString());
			Console.WriteLine("[] stepPercentDefault (%) = "+stepPercentDefault.ToString());
			Console.WriteLine("[] hipSwayDefault     (%) = "+hipSwayDefault.ToString());
			Console.WriteLine("[] hipPhase	       (rad) = "+hipPhase.ToString());
			Console.WriteLine("******************************");
		}
	}
}
