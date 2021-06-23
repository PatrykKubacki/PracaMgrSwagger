using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;


namespace QMeterProtocol
{
    public class MeasManager
    {
        private MeasResultsList averagedMeasList;
        private MeasResultsList lastMeasList;
        private MeasResultsList lorenzList;         // points for visualisation of theoretical Lorenz curve
        private int currentScanListIdx = int.MinValue;
        private int currentDpScanNumber = -1;
        private int averaging = 5;  // SHOULD BE set by the user!!!
        private List<MeasResultsList> scanList = new List<MeasResultsList>();
        private const double E_NOMOREPOINTS = double.MaxValue;
        private bool uncalPll;
        private double lastStep=0;
        private PerformanceCounter performanceCounter = new PerformanceCounter();
        private int inAddFunction = 0;
        private ConcurrentQueue<MeasResultsList> actualResultsList = new ConcurrentQueue<MeasResultsList>();

        public int Averaging
        {
            get { return averaging; }
            set { averaging = value; }
        }

        public MeasManager()
        {
            averagedMeasList = new MeasResultsList();
            lastMeasList = new MeasResultsList();
            lorenzList = new MeasResultsList();
            uncalPll = false;
        }

        public void addNewMeasResult(MeasPacket mp)
        {
            try
            {
                inAddFunction++;

                if (inAddFunction > 1)
                    throw new InvalidOperationException("DEBUG: Consecutive execution in adNewMeasResult()!");

                double currentAverageStep;

                if (mp.ScanNumber != currentDpScanNumber) // we have detected, that hardware has started a new scan
                {
                    if (scanList.Count > 0)
                    {
                        // If user has changed the frequency step, we should remove all previous results to avoid averaging of points from different frequency step (it makes /\/\/\/\ on the graph after span/step change)
                        currentAverageStep = scanList[currentScanListIdx].calculateAverageStep();
                        if (Math.Abs(lastStep - currentAverageStep) > 0.0009)  // Frequency step change is recognized when current step and step from previous scan are different more than 0.9 kHz (steps lower than 1 kHz are not possible, but we need some margin for i.e. packet loss)
                        {
                            lastStep = currentAverageStep;
                            while (scanList.Count > 0)
                                scanList.RemoveAt(0);
                        }
                    }

                    if (scanList.Count > 0)
                    {
                        performanceCounter.storePointsFromOneScan(scanList[scanList.Count - 1].Count);
                        lastMeasList.makeFromPointListCopy(scanList[scanList.Count - 1].getPointList());
                        measResultsProcessing();

                    }

                    scanList.Add(new MeasResultsList());

                    while (scanList.Count > Averaging)   // sometimes it is necessary to remove more than one MeasResultList - i.e. when user has decreased averaging factor
                        scanList.RemoveAt(0);

                    currentDpScanNumber = mp.ScanNumber;
                    currentScanListIdx = scanList.Count - 1;
                }
                scanList[currentScanListIdx].addDataPoint(mp);   // add the datapacket to the current MeasResultsList
            }
            catch (Exception ex)
            {
                Console.WriteLine("##### Exception: {0}", ex.Message);
            }

            inAddFunction--;
        }

        // Reaction to change of meas-type annouced by the remote device
        // This function should setup proper axis legends on the graph and clear data buffers
        public void measTypeChanged(ProtocolHandler.MeasType mt)
        {
       //     graphManager.graphMeasTypeSetup(mt);
            scanList.Clear();
            currentDpScanNumber = -1;
        }

        // 1. Calculate new values for the averagedMeasList
        // 2. Show the AML
        // 3. Calculate resonator's Q depending on AML data
        // 4. Remove unnecessary data from SML
        private void measResultsProcessing()
        {
            int i, j;
            PointPair[] ppBuffer;
            double minF;
            double powerSum;
//            QFactorCalculator qCalculator;
//            QFactorResult qResult;

            averagedMeasList.clear();
            scanList.ForEach(delegate(MeasResultsList mrl)
            {
                mrl.nextResultReset();
            });

            ppBuffer = new PointPair[scanList.Count];

            do
            {
                i = 0;
                minF = E_NOMOREPOINTS;

                while (i < scanList.Count)
                {
                    ppBuffer[i] = scanList[i].getNextResult();
                    if (ppBuffer[i] != null)
                    {
                        if (ppBuffer[i].frequency < minF)   // let's calculate the minimum frequency in this dataset
                            minF = ppBuffer[i].frequency;
                    }
                    i++;
                }
                if (minF == E_NOMOREPOINTS)
                {
                    // 2. Graph is refreshed in 100ms intervals autonomicaly by graphTimer object placed on frmMain.
                    //graphManager.suggestGraphRefresh();
                    break; // we have ended the analyse of the scanList array
                }

                // Calculation of the average power in the minF point
                j = 0;
                powerSum = 0;
                for (i = 0; i < ppBuffer.Length; i++)
                {
                    if (ppBuffer[i]!=null)
                        if (ppBuffer[i].frequency == minF)
                        {
                            powerSum += ppBuffer[i].gain;
                            j++;
                            scanList[i].resultAccepted();
                        }
                }
                averagedMeasList.addFreqPower(minF, powerSum / j);
            }
            while (true); // loop until all buffered data is processed

            if (averagedMeasList.Count > 0)
            {
                MeasResultsList useless;
                if (actualResultsList.Count > 2)            // avoid too many results in the queue, because this would cause a lag between performed measurements and presentation on screen
                    actualResultsList.TryDequeue(out useless);   
                actualResultsList.Enqueue(averagedMeasList);
            }

            // 3. Calculate resonator's Q depending on AML data
    /*        if (averagedMeasList.Count > 0)
            {
                qCalculator = new QFactorCalculator(averagedMeasList, qFactorSettings);
                qResult = qCalculator.calculateQFactor();
                qResult.UncalPll = uncalPll;    
                graphManager.displayQFactor(qResult);
                generateLorenzCurve(qResult.Q_factor, qResult.CenterFrequency, qResult.PeakTransmittance, 50);
                uncalPll = false;                   // reset of the uncalPLL flag until next meas processing
            }  */
        }

        public MeasResultsList getActualMeasResultsList()
        {
            MeasResultsList mrl;
            actualResultsList.TryDequeue(out mrl);
            return mrl;
        }

        // Generates theoretical Lorenz curve with given Q-factor, center frequency [MHz] and peak transmittance [dB]
        // The curve contains N-points
        public void generateLorenzCurve(double Q, double f0, double peakTransmittance, int n)
        {
            int i;
            double H;   // |H(f)|
            double f;   // frequency
            const int range = 3;  // range of frequency in which the curve is generated (range*delta_f where delta_f = f0/Q)
            double fmin, rangedf;

            lorenzList.clear();

            rangedf = range * (f0 / Q);
            fmin = f0 - 0.5 * rangedf;

            for (i = 0; i < n; i++)
            {
                f = fmin + (double)i / n * rangedf;
                H = 20 * Math.Log10(1 / Math.Sqrt(1+Q*Q*Math.Pow((f/f0-f0/f),2)))+peakTransmittance;
                lorenzList.addFreqPower(f, H);
            }
        }




        public int LastMeasRate
        {
            get { return (int)performanceCounter.MeasRate; }
        }


    }
}
