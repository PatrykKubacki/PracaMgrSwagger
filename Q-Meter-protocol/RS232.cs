using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Resonator
{
    public class RS232: SerialPort
    {
        #region Local Variables
        
        string sBuffer = String.Empty;  // buffer for received data
        //List<byte> bBuffer = new List<byte>();
        BinaryCircularBuffer bBuffer = new BinaryCircularBuffer(20000, false);
        //internal MasterController master;
        // 
        ProtocolHandler ph;

        const int PACKET_START = 0xff;
        const int PACKET_SIZE = 1 + 4 + 4 + 2; //1 + 4 + 2 ;  // (bye) PACKET_START + (long) frequency + (short) adc

        // ETX/STX constants
        // packet markers
        const int STX = 0x02;				    // start transmission marker
        const int ETX = 0x03;				    // end transmission marker
        // packet length parameters
        const int STXETX_HEADERLENGTH = 4;	    // number of bytes required for packet header
        const int STXETX_TRAILERLENGTH = 2;	    // number of bytes required for packet trailer
        // packet field offsets
        public const int STXETX_STATUSOFFSET = 1;	    // number of bytes from STX to STATUS
        public const int STXETX_TYPEOFFSET = 2;	    // number of bytes from STX to TYPE
        const int STXETX_LENGTHOFFSET = 3;	    // number of bytes from STX to LENGTH
        const int STXETX_DATAOFFSET = 4;	    // number of bytes from STX to the data
        const int STXETX_CHECKSUMOFFSET = 4;	// number of bytes from STX+[length] to CHECKSUM
        const int STXETX_NOETXSTXCHECKSUM = 3;	// number of bytes used by STX,ETX,CHECKSUM
        int packetError = 0;    // number of received packetc with wrong CRC

        public int PacketError
        {
            get { return packetError; }
        }

        int len_sum = 0;
        int len_i = 0;

        #endregion


        public RS232(ProtocolHandler protocolHandler)
        {
            this.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            this.ph = protocolHandler;
        }

        public void Connect(string PortName)
        {
            if (this.IsOpen) this.Close();
            else
            {
                // Set the port's settings
                this.BaudRate = 921600;
                this.DataBits = 8;
                this.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
                this.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
                this.PortName = PortName;
                this.ReadBufferSize = 20000; // buffer size set manually, because default value (4096) is too small for one sweep

                // Open the port
                this.Open();
            }


        }


        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            bBuffer.PutFromRS232(this);
            ProcessStxEtxBuffer(bBuffer);
        }


        void ProcessStxEtxBuffer(BinaryCircularBuffer rxBuffer)
        {
            int i;
            int length;
            byte checksum;
            float avr;

            len_sum += rxBuffer.Count;
            len_i++;
            avr = len_sum / len_i;

            // process the buffer
            // go through buffer looking for packets
            // the STX must be located at least STXETX_HEADERLENGTH+STXETX_TRAILERLENGTH from end
            // otherwise we must not have a complete packet
            while (rxBuffer.Count >= (STXETX_HEADERLENGTH + STXETX_TRAILERLENGTH))
            {
                // look for a potential start of packet
                if (rxBuffer[0] == STX)
                {
                    // if this is a start, then get the length
                    length = rxBuffer[STXETX_LENGTHOFFSET];

                    // now we must have at least STXETX_HEADERLENGTH+length+STXETX_TRAILERLENGTH in buffer to continue
                    if (rxBuffer.Count >= (STXETX_HEADERLENGTH + length + STXETX_TRAILERLENGTH))
                    {
                        // check to see if ETX is in the right position
                        if (rxBuffer[STXETX_HEADERLENGTH + length + STXETX_TRAILERLENGTH - 1] == ETX)
                        {
                            // found potential packet
                            // test checksum
                            checksum = 0;
                            // sum data between STX and ETX, not including checksum itself
                            // (u16) casting needed to avoid unsigned/signed mismatch
                            for (i = 0; i < (STXETX_HEADERLENGTH + length + STXETX_TRAILERLENGTH - STXETX_NOETXSTXCHECKSUM); i++)
                            {
                                checksum += rxBuffer[i + STXETX_STATUSOFFSET];
                            }
                            // compare checksums
                            if (checksum == rxBuffer[STXETX_CHECKSUMOFFSET + length])
                            {
                                //we do have a packet!

                                byte[] packet = new byte[length];
                                rxBuffer.CopyTo(4, packet, 0, length);
                                DataPacket dp = new DataPacket(packet, rxBuffer[STXETX_TYPEOFFSET], rxBuffer[STXETX_STATUSOFFSET]);
                                ph.processPacket(dp);

                                // dump this packet from the
                                rxBuffer.RemoveNBytes(STXETX_HEADERLENGTH + length + STXETX_TRAILERLENGTH);
                            }
                            else
                            {
                                // bad checksum - remove first byte from the buffer (=dump STX) and try again to find a complete frame from the next position in the buffer
                                //rxBuffer.previewLast10Bytes(); 
                                rxBuffer.Remove1Byte();
                                packetError++;
                            }
                        }
                        else
                        {
                            // no ETX or ETX in wrong position
                            // dump this STX
                            //rxBuffer.previewLast10Bytes();
                            rxBuffer.Remove1Byte();
                            packetError++;
                        }
                    }
                    else
                    {
                        // not enough data in buffer to decode pending packet
                        // wait until next time
                        break;
                    }
                }
                else
                {
                    // this is not a start, dump it
                    rxBuffer.Remove1Byte();
                    packetError++;
                }
            }

            // check if receive buffer is full with no packets decoding
            // (ie. deadlocked on garbage data or packet that exceeds buffer size)
            if (rxBuffer.IsFull)   
            {
                // dump receive buffer contents to relieve deadlock
                rxBuffer.Clear();
            }
        }


        public void createStxEtxPacket(byte status, ProtocolHandler.PacketType type, byte[] data)
        {
            int datalength;
	        int checksum = 0;
	        int i;
            byte[] buffer;

            // if port is closed, exit quickly...
            if (!this.IsOpen)
            {
                switch (type)  // ...but for critical packet types show a message, that it was not possible to send the packet
                {
                    case ProtocolHandler.PacketType.ptDetFreqHeaders:
                    case ProtocolHandler.PacketType.ptDetPwrHeaders:
                    case ProtocolHandler.PacketType.ptDetData:
                    case ProtocolHandler.PacketType.ptDetSaveToFlash:
                    case ProtocolHandler.PacketType.ptGenFreqHeaders:
                    case ProtocolHandler.PacketType.ptGenData:
                    case ProtocolHandler.PacketType.ptCalibrationOther:
                        throw new InvalidOperationException("Unable to send packet " + type.ToString() + " because port is closed.");
                    default:                                            return;  
                }
            }

            if (data == null)
            {
                datalength = 0;
                buffer = new byte[6]; // 6 bytes for only packet header and footer
            }
            else
            {
                datalength = data.Length;
                buffer = new byte[datalength + 6]; // 6 bytes extra for packet header and footer
            }

            // write packet header
            i = 0;
            buffer[i++] = STX;
            buffer[i++] = status;
            buffer[i++] = (byte) type;
            buffer[i++] = (byte) datalength;

            // update checksum
	        checksum += status + (int)type + datalength;

            if (datalength != 0)
            {
                // copy data into packet
                data.CopyTo(buffer, 4);
                i += data.Length;

                foreach (byte bbb in data)
                {
                    checksum += bbb;
                }
            }
            // write packet trailer
            buffer[i++] = (byte) checksum;
            buffer[i++] = ETX;

            try
            {
                if (this.IsOpen)
                    this.Write(buffer, 0, datalength + 6);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to send packet to the device. Probably hardware is not connceted to PC. Original message: " + ex.Message);
            }
        }

        public void emptyRxBuffer()
        {
            bBuffer.Clear();
        }

    }
}
