using System;
using System.Collections.Generic;
using System.Text;

namespace Resonator
{
    public class MeasPacket
    {
        //int frequency;
        double frequency;
        float adc;
        int scanNumber;

        public MeasPacket()
        {
            frequency = 0;
            adc = 0;
            scanNumber = 0;
        }


        // Creates MeasPacket from received DataPacket 
        public MeasPacket(DataPacket dp)
        {
            scanNumber = dp.Status;
            frequency = (double)BitConverter.ToInt32(dp.Buffer, 0)/1000;  // Conversion from kHz to MHz becuase...
                // ...microcontroller sends frequency as integer in 1 kHz units and there is a need to provide for a graph
                // frequency given in MHz (to avoid long numbers with zeros at the end)
                // There is no big difference for algorithms when data is stored in integer kHz or double MHz.
            adc = BitConverter.ToSingle(dp.Buffer, 4);
        }

        // Creates MeasPacket from pure data
        public MeasPacket(double frequency, double adc, int scanNumber)
        {
            this.frequency = frequency;
            this.adc = (float)adc;
            this.scanNumber = scanNumber;
        }


  /*      public int Frequency
        {
            get { return frequency; }
        }    */

        public double Frequency
        {
            get { return frequency; }
        } 

        public float Adc
        {
            get { return adc; }
        }

        public int ScanNumber
        {
            get { return scanNumber; }
        }

    }
}
