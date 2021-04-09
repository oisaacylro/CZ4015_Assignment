using CZ4015.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CZ4015
{
    class Program
    {
        static void Main(string[] args)
        {
            //reserved channels
            int ChannelReserved = 0;
            double[] dropped = new double[11];
            double[] blocked = new double[11];
            double[] successes = new double[11];


            List<double> tempDrop = new List<double>();
            List<double> tempBlock = new List<double>();
            tempDrop.Add(0);
            tempBlock.Add(0);
            //repeat for each possible reserve channel count
            while (ChannelReserved <=10)
            {
                double droppedSum = 0;
                double blockedSum = 0;


                //repeat simulation multiple times to obtain more accurate results
                for(int x = 0; x < Constants.RepeatCount; x ++)
                {
                    //Small ux to allow user to see progress
                    Console.Write(" "+ x+ " ");

                    //initialize stations
                    int[] bStation = new int[20];

                    List<Caller> callers = new List<Caller>();

                    for (int i = 0; i < 20; i++)
                    {
                        bStation[i] = 10;
                    }


                    //for data collection
                    double totalDropped = 0;
                    double totalBlocked = 0;
                    double totalCalls = 0;

                    //system clock
                    double clock = 0;


                    //Generate first caller and add to event list
                    callers.Add(new Caller(clock));
                    totalCalls++;

                    //Continue until specified amount of calls simulated
                    while (totalCalls < Constants.CallsToSimulate)
                    {
                        if (totalCalls == Constants.Warmup)
                        {
                            totalBlocked = 0;
                            totalDropped = 0;
                        }
                        clock = callers[0].nextEventTime;

                        //Call has not started, try to assign to channel in it's cell
                        if (!callers[0].started)
                        {
                            //Generate next caller if total calls to simulate has not been achieved

                            //check if station has available channel
                            if (bStation[callers[0].currentCell] > ChannelReserved)
                            {
                                //channel available, assign caller
                                bStation[callers[0].currentCell]--;
                                callers[0].startCall(clock);
                            }
                            else
                            {
                                //no channel available, block call
                                totalBlocked++;
                                callers.RemoveAt(0);
                            }
                            if (totalCalls < Constants.CallsToSimulate)
                            {
                                //Console.WriteLine("Generate new call");
                                callers.Add(new Caller(clock + getNextArrivalTime()));

                                //Console.WriteLine(callers[callers.Count-1].Print());

                                totalCalls++;
                            }
                        }
                        //call is already in progress and next event is either end of call or exiting highway
                        else if (callers[0].lastEvent)
                        {
                            bStation[callers[0].currentCell]++;
                            callers.RemoveAt(0);
                        }
                        //call is transferring
                        else
                        {
                            bStation[callers[0].currentCell]++;
                            //check if next station has available channel
                            if (bStation[callers[0].currentCell + callers[0].direction] > 0)
                            {
                                //channel available, transfer call
                                callers[0].moveToNextCell(clock);
                                bStation[callers[0].currentCell]--;
                            }
                            else
                            {
                                //no channel available, drop call
                                totalDropped++;
                                callers.RemoveAt(0);
                            }
                        }

                        //To obtain data for warmup
                        //if (ChannelReserved == 1 && totalCalls % 100 == 0 && tempBlock.Count==(totalCalls/100))
                        //{
                        //    tempBlock.Add(totalBlocked - tempBlock[tempBlock.Count-1]);
                        //    tempDrop.Add(totalDropped - tempDrop[tempDrop.Count - 1]);
                        //}

                        //Sort all callers based on next event time
                        callers.Sort();
                    }


                    droppedSum += totalDropped;
                    blockedSum += totalBlocked;
                }

                //Compute average data for n reserved channels
                Console.Write("\n");
                dropped[ChannelReserved] = ((droppedSum/Constants.RepeatCount) / (Constants.CallsToSimulate-Constants.Warmup)) * 100;
                blocked[ChannelReserved] = ((blockedSum / Constants.RepeatCount) / (Constants.CallsToSimulate - Constants.Warmup)) * 100;
                successes[ChannelReserved] = 100 - (dropped[ChannelReserved] + blocked[ChannelReserved]);
                Console.WriteLine("Done : " + ChannelReserved);

                ChannelReserved++;
            }


            //output data to user 
            for (int i = 0; i < ChannelReserved; i++)
            {
                Console.Write("\n======================================================");
                Console.WriteLine("\nChannels reserved : " + i);
                //Console.Write("\nSuccessful Calls : ");
                //Console.WriteLine(successes[i]);
                Console.Write("Successful % : ");
                Console.WriteLine(String.Format("{0:0.000}", (successes[i])));
                //Console.WriteLine("\nTotal Dropped : " + dropped[i]);
                Console.Write("Dropped % : ");
                Console.WriteLine(String.Format("{0:0.000}", (dropped[i])));
                //Console.WriteLine("\nTotal Blocked : " + blocked[i]);
                Console.Write("Blocked % : ");
                Console.WriteLine(String.Format("{0:0.000}", (blocked[i])));
            }
            Console.Write("\n======================================================");


            //To obtain data for warmup
            //var csv = new StringBuilder();
            //csv.AppendLine("Iteration,Blocked,Dropped");
            //for (int i = 0; i < tempBlock.Count; i++)
            //{
            //    csv.AppendLine(i + "," + tempBlock[i] + "," + tempDrop[i]);
            //}
            //File.WriteAllText("data2.csv", csv.ToString());
        }


        //Code to compute random arrival time

        public static double getNextArrivalTime()
        {
            Random rand = new Random();
            return Math.Abs(Constants.ArrivalStdDev * generateGaussian(rand) + Constants.ArrivalMean);
        }


        private static double generateGaussian(Random rand)
        {
            return Math.Sqrt(-2.0 * Math.Log(1.0 - rand.NextDouble())) * Math.Sin(2.0 * Math.PI * (1.0 - rand.NextDouble()));
        }
    }
}
