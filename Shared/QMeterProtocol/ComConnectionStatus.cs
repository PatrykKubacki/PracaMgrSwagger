using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace QMeterProtocol
{
    class ComConnectionStatus
    {
        public enum StatusType
        {
            Active,
            NoAnswer,
            Oppening,
            Closed,
            Idle
        };

        StatusType status;
        string text;
        Color color;
        string portName;

        public string PortName
        {
            get { return portName; }
            set { portName = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        
        public ComConnectionStatus()
        {
            text = "---";
            color = Color.FromKnownColor(KnownColor.Window);
            portName = "";
        }

        public void changeStatus(StatusType st)
        {
            string temp;

            switch (st)
            {
                case StatusType.Active: temp = "Active"; color = Color.LightGreen; break;
                case StatusType.Closed: temp = "Closed"; color = Color.Yellow; break;
                case StatusType.Idle: temp = "Idle"; color = Color.LightGreen; break;
                case StatusType.NoAnswer: temp = "No answer from device"; color = Color.Yellow; break;
                case StatusType.Oppening: temp = "Trying to open"; color = Color.Yellow; break;
                default: temp = "..."; color = Color.Yellow; break;
            }
            text = "[" + PortName + "]: "+temp;

            status = st;
        }

        public StatusType Status
        {
            get { return status; }
        }


    }
}
