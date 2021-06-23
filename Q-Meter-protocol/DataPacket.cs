using System;
using System.Collections.Generic;
using System.Text;

namespace Resonator
{
    public class DataPacket
    {

        int status;
        ProtocolHandler.PacketType type;
        byte[] data;


        public DataPacket(byte[] buffer, int type, int status)
        {
            data = buffer;
            this.type = (ProtocolHandler.PacketType) type;
            this.status = status;
        }

        // This constructor bases on the rxBuffer only
        public DataPacket(List<Byte> rxBuffer, int length)
        {
            data = new byte[length];
            rxBuffer.CopyTo(4, data, 0, length);
            type = (ProtocolHandler.PacketType)rxBuffer[RS232.STXETX_TYPEOFFSET];
            status = rxBuffer[RS232.STXETX_STATUSOFFSET];
        }




        public int Status
        {
            get { return status; }
            set { status = value; }
        }



        public ProtocolHandler.PacketType Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Length
        {
            get { return data.Length; }
        }

        public byte[] Buffer
        {
            get { return data; }
        }
    }
}
