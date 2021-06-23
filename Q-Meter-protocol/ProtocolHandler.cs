using System;
using System.IO.Ports;
using System.Globalization;
using System.ComponentModel;
using System.Threading;
using Q_Meter_protocol.Properties;
using System.Text;

namespace Resonator
{
    public class ProtocolHandler
    {
        private RS232 comport;
        private MeasManager measManager;
        public string comStatus = "";
        private BackgroundWorker comWorker = new BackgroundWorker();
        bool packetReceived;   // flag indicating that a datapacket has been received
        bool packetAlivereceived;
        const int WORKER_DELAY = 200; // half second of delay
        public string QMeterDescription = "---";
        public int QMeterMinFrequency = 0;      // lowest frequency supported by Q-Meter device [MHz]
        public int QMeterMaxFrequency = 0;      // highest frequency supported by Q-Meter device [MHz]
        private bool hardwareIsConnected = false;

        public bool HardwareIsConnected
        {
            get
            {
                return hardwareIsConnected;
            }
        }

        public enum PacketType
        {
            ptSettings = 0,
            ptDetFreqHeaders, ptDetPwrHeaders, ptDetData, ptDetSaveToFlash, ptGenFreqHeaders, ptGenData, ptGenCalibration,
            
            ptGetCalibrationData, ptGetAdcRawValues,

            ptCalibratedMeasDbm, ptAdcRawValue, ptStateIdle, ptMeasType, ptTresholdFrequency, ptManualMode, ptUncalPll, ptAlive, ptCalibrationOther, ptFirmwareUpgrade,

            ptManualFrequency
        };

        public enum MeasType
        {
            mtUndefined, mtS21, mtPower
        };

        const byte STX_STATUS = 0x00;
        const byte STX_ALIVE = 0xff;

        public ProtocolHandler(MeasManager mm)
        {
             this.measManager = mm;

             comport = new RS232(this);

             packetReceived = false;
             packetAlivereceived = false;
             //initializing comWorker
             comWorker.DoWork += comWorker_DoWork;
             comWorker.ProgressChanged += comWorker_ProgressChanged;
             comWorker.RunWorkerCompleted += comWorker_RunWorkerCompleted;
             comWorker.WorkerReportsProgress = true;
             comWorker.WorkerSupportsCancellation = true;
             comWorker.RunWorkerAsync();
             
        }

        private void comWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ComConnectionStatus ccs = new ComConnectionStatus();

            comWorker.ReportProgress(0, ccs);

            // main loop
            while (true)
            {
                if (Settings.Default.SerialPort == "Auto")
                {

                    while (!comport.IsOpen)  // this loop tries to open COM port
                    {
                        // COM port seems to be not opened - let's try to open it!
                        foreach (string s in SerialPort.GetPortNames())
                        {
                            try
                            {
                                ccs.PortName = s;
                                ccs.changeStatus(ComConnectionStatus.StatusType.Oppening);
                                comWorker.ReportProgress(0, ccs);
                                comport.Connect(s);
                                if (comport.IsOpen)
                                {   // we've found openable COM port - we have to check whether device is connected to it or not
                                    packetAlivereceived = false;
                                    cmdIsAlive();
                                    Thread.Sleep(WORKER_DELAY);
                                    if (packetAlivereceived)   // correct port is found, answer from device received
                                    {
                                        ccs.changeStatus(ComConnectionStatus.StatusType.Active);
                                        comWorker.ReportProgress(0, ccs);
                                        hardwareIsConnected = true;
                                        break;
                                    }
                                    else
                                    {
                                        ccs.changeStatus(ComConnectionStatus.StatusType.NoAnswer);
                                        comWorker.ReportProgress(0, ccs);
                                        comport.Close(); // Close port if we've got no answer
                                        hardwareIsConnected = false;
                                        Thread.Sleep(WORKER_DELAY);
                                    }
                                }
                                if (comWorker.CancellationPending)
                                {
                                    ccs.changeStatus(ComConnectionStatus.StatusType.Closed);
                                    comWorker.ReportProgress(0, ccs);
                                    hardwareIsConnected = false;
                                    return;
                                }
                            }
                            catch 
                            {
                                // do nothing, when port cannot be opened
                            }
                            Thread.Sleep(WORKER_DELAY);
                        }

                        // If comport is opened, loop and wait for possible disconnection
                        // OR when comport is still closed, it means that device is not connected and we should start checking all ports from the beginning
                        if (comport.IsOpen)
                        {
                            cmdIsAlive();
                            Thread.Sleep(WORKER_DELAY);

                            while (packetAlivereceived)
                            {
                                
                                packetAlivereceived = false;
                                packetReceived = false;
                                Thread.Sleep(WORKER_DELAY);

                                while (packetReceived)
                                {
                                    packetReceived = false;

                                    packetAlivereceived = false;
                                    ccs.changeStatus(ComConnectionStatus.StatusType.Active);
                                    comWorker.ReportProgress(0, ccs);
                                    hardwareIsConnected = true;
                                    Thread.Sleep(WORKER_DELAY*6);  // *6 is added to avoid toggling "Idle"/"Active" in 10 GHz Q-Meter in case, when grouped data transfer is in use
                                    if (comWorker.CancellationPending)
                                    {
                                        ccs.changeStatus(ComConnectionStatus.StatusType.Closed);
                                        comWorker.ReportProgress(0, ccs);
                                        hardwareIsConnected = false;
                                        return;
                                    }

                                }
                                ccs.changeStatus(ComConnectionStatus.StatusType.Idle);
                                comWorker.ReportProgress(0, ccs);
                                cmdIsAlive();
                                hardwareIsConnected = true;

                                if (comWorker.CancellationPending)
                                {
                                    ccs.changeStatus(ComConnectionStatus.StatusType.Closed);
                                    comWorker.ReportProgress(0, ccs);
                                    hardwareIsConnected = false;
                                    return;
                                }
                                Thread.Sleep(WORKER_DELAY);
                        
                            }
                            ccs.changeStatus(ComConnectionStatus.StatusType.NoAnswer);
                            comWorker.ReportProgress(0, ccs);
                            hardwareIsConnected = false;
                            comport.Close();  // No answer from device -> let's close the com port and start looking for device
                        }
                    }
                }
                else
                {
                    ccs.changeStatus(ComConnectionStatus.StatusType.Oppening);
                    ccs.PortName = Settings.Default.SerialPort;
                    comWorker.ReportProgress(0, ccs);
                    hardwareIsConnected = false;
                    try
                    {
                        comport.Connect(Settings.Default.SerialPort);
                    }
                    catch 
                    { }
                    Thread.Sleep(WORKER_DELAY);
                    if (comport.IsOpen && Settings.Default.SerialPort != "Auto")
                    {
                        cmdIsAlive();   // This is sent to get frequency range of the device, firmware version and etc.
                        while (Settings.Default.SerialPort != "Auto")
                        {
                            if (packetReceived)
                            {
                                ccs.changeStatus(ComConnectionStatus.StatusType.Active);
                            } 
                            else
                            {
                                ccs.changeStatus(ComConnectionStatus.StatusType.Idle);
                            }
                            comWorker.ReportProgress(0, ccs);
                            hardwareIsConnected = true;
                            packetReceived = false;
                            
                            if (comWorker.CancellationPending)
                            {
                                ccs.changeStatus(ComConnectionStatus.StatusType.Closed);
                                comWorker.ReportProgress(0, ccs);
                                hardwareIsConnected = false;
                                return;
                            }
                            Thread.Sleep(WORKER_DELAY);
                        }
                    }
                }
            }
        }

        private void comWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ComConnectionStatus ccs = (ComConnectionStatus)e.UserState;
            comStatus = ccs.Text;
            //comStatus.BackColor = ccs.Color;

        }

        private void comWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           // MessageBox.Show("DEBUG: comWorker has stopped his job - it should never happen!");
        }

        public void cancelConnectionThread()
        {
            int i = 50;   // timeout when Q-Meter is not connected
            comWorker.CancelAsync();
            
            // wait for the thread is stopped
            while (comWorker.IsBusy && i-->0)
            {
                Thread.Sleep(50);
                
            }
        }

        public bool getComConnectionStatus()
        {
            return comport.IsOpen;
        }

        // sends command given as argument
        public void SendCommand(string cmd)
        {
            if (comport.IsOpen)
            {
                comport.Write(cmd + "\r\n");

                try
                {
                    //Application.DoEvents();
                    System.Threading.Thread.Sleep(10); // give a little time for Master Unit and HPUs to proceed command
                    //Application.DoEvents();
                }
                catch 
                {

                }
            }
            else
            {
                throw new InvalidOperationException("It is not possible to send command because serial port is closed. Do you want to open serial port before sending command?");
            }
        }

 


        public int getPacketErrors()
        {
            return comport.PacketError;
        }



        public void processPacket(DataPacket dp)
        {
            try
            {
                packetReceived = true;

                switch (dp.Type)
                {
                    case PacketType.ptCalibratedMeasDbm: measManager.addNewMeasResult(new MeasPacket(dp)); break;
            /*        case PacketType.ptDetFreqHeaders:
                    case PacketType.ptDetPwrHeaders:
                    case PacketType.ptDetData: 
                    case PacketType.ptGenFreqHeaders:
                    case PacketType.ptGenData:          
                    case PacketType.ptCalibrationOther: if (fMain.FDetectorCalibration != null) fMain.FDetectorCalibration.processCalibrationDataPacket(dp); break;
                    case PacketType.ptAdcRawValue:      if (fMain!=null) fMain.Invoke(new EventHandler(delegate { if (fMain.FDetectorCalibration != null) fMain.FDetectorCalibration.processAdcRawData(dp); })); break;
              */      case PacketType.ptMeasType:         measManager.measTypeChanged((MeasType)dp.Status); break;
              //      case PacketType.ptManualMode:       if (fMain!=null) fMain.Invoke(new EventHandler(delegate { if (fMain.FManualMode != null) fMain.FManualMode.processDataPacket(dp); })); break;
              //      case PacketType.ptUncalPll:         measManager.setUncalPll(); break;
                    case PacketType.ptAlive: if (dp.Status != STX_ALIVE) { packetAlivereceived = true; this.processAlivePacket(dp); } break;
                }
            }
            catch (Exception ex)
            {
                cmdStateIdle();  // I want to stop the device to not kill the user with tons of error messageboxes!
                comport.emptyRxBuffer();
                throw new InvalidOperationException("Error in datapacket handling: " + ex.ToString() + "\nThe device will stop now."); 
            }
            
            
        }

        public int RS232Flush()
        {
            return comport.BytesToRead;
        }

        // Sends to the device scanning range; frequencies given in kHz
        public void cmdSetScanRange(int minFrequency, int maxFrequency, int step, MeasType mt, int oversampling, int preciseScanning)
        {
            byte[] data;
            data = new byte[6 * 4];
            BitConverter.GetBytes(minFrequency).CopyTo(data, 0);
            BitConverter.GetBytes(maxFrequency).CopyTo(data, 4);
            BitConverter.GetBytes(step).CopyTo(data, 8);
            BitConverter.GetBytes((int)mt).CopyTo(data, 12);
            BitConverter.GetBytes(oversampling).CopyTo(data, 16);
            BitConverter.GetBytes(preciseScanning).CopyTo(data, 20);
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptSettings, data);
        }



        void wait()
        {
            Thread.Sleep(100);
        //    Application.DoEvents();
        }

        public void processAlivePacket(DataPacket dp)
        {
            int lowFrequencyRange = BitConverter.ToInt32(dp.Buffer, 0);     // in [kHz]
            int highFrequencyRange = BitConverter.ToInt32(dp.Buffer, 4);   // in [kHz]

            QMeterMinFrequency = (int)lowFrequencyRange / 1000;
            QMeterMaxFrequency = (int)highFrequencyRange / 1000;

/*            try
            {
                frequencyRangeSetup(lowMhz, highMhz);
                if (!firstRangeSetupWasDone)
                {
                    unzoomGraph();
                    firstRangeSetupWasDone = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in frequency range change.\n Exception: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */

            StringBuilder sb = new StringBuilder();

            sb.Append("Firmware version: ");

            byte ch = 1; // value doesn't matter
            for (int i = 8; (i < dp.Buffer.Length) && (ch != 0); i++)
            {
                ch = dp.Buffer[i];
                sb.Append((char)ch);
            }
            sb.Remove(sb.Length - 1, 1); // remove \0 character
            sb.Append(" Frequency range: ");
            sb.Append(QMeterMinFrequency.ToString());
            sb.Append(" - ");
            sb.Append(QMeterMaxFrequency.ToString());
            sb.Append(" [MHz]");

            this.QMeterDescription = sb.ToString();
            Console.WriteLine("Connected device: " + this.QMeterDescription);
        }


        public void cmdGetCalibrationData()
        {
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptGetCalibrationData, null);
        }

        public void cmdGetAdcRawValues()
        {
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptGetAdcRawValues, null);
        }

        public void cmdDetSaveToFlash()
        {
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptDetSaveToFlash, null);
        }

        public void cmdStateIdle()
        {
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptStateIdle, null);
        }

        private void cmdIsAlive()
        {
            comport.createStxEtxPacket(STX_ALIVE, PacketType.ptAlive, null);
        }

        public void cmdFirmwareUpgrade()
        {
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptFirmwareUpgrade, null);
        }

        public void cmdGeneratorCalibration(int startFrequency, int stopFrequency, float startAttenuator, float stopAttenuator)
        {
            byte[] data;
            data = new byte[4 * 4];
            BitConverter.GetBytes(startFrequency).CopyTo(data, 0);
            BitConverter.GetBytes(stopFrequency).CopyTo(data, 4);
            BitConverter.GetBytes(startAttenuator).CopyTo(data, 8);
            BitConverter.GetBytes(stopAttenuator).CopyTo(data, 12);
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptGenCalibration, data);
        }

        public void cmdManualModeConfig(byte[] data)
        {
            comport.createStxEtxPacket(STX_STATUS, PacketType.ptManualMode, data);
        }

        public void cmdManualFrequency(int frequency_kHz)
        {
            byte[] data;
            data = new byte[1 * 4];

            BitConverter.GetBytes(frequency_kHz).CopyTo(data, 0);

            comport.createStxEtxPacket(STX_STATUS, PacketType.ptManualFrequency, data);
        }

        // use it carefully! ADVICE: Connection thread should be turned off.
        public void closeComPort()
        {
            comport.Close();
        }

    }
}
