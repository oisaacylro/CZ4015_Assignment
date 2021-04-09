using System;
using System.Collections.Generic;
using System.Text;

namespace CZ4015.Classes
{
    public class Caller : IComparable<Caller>
    {

        Random rand;
        public double speed;
        public int direction;
        public int currentCell;
        public double timeRemaining;
        public double timeToNextCell;
        public double nextEventTime;
        public bool started = false;
        public bool lastEvent = false;


        public Caller(double arrivalTime)
        {
            rand = new Random();
            direction = generateDirection();
            speed = generateSpeed();
            timeRemaining = generateDuration();
            timeToNextCell = getTimeToNextCell();
            currentCell = rand.Next(0, 20);
            nextEventTime = arrivalTime;
        }

        public void startCall(double clock)
        {
            started = true;
            calculateNextEventTime(clock);
        }

        public string Print()
        {
            return "Direction : " + direction +"\nSpeed : " + speed + "\nCurrentCell : " + currentCell + "\ntimeRemaining : " + timeRemaining + "\ntimeToNextCell : " + timeToNextCell + "\nNext Event in : " + nextEventTime + "\n\n" ;
        }

        private void calculateNextEventTime(double clock)
        {
            //next event will be at the end of current cell
            if (timeToNextCell < timeRemaining)
            {
                nextEventTime = timeToNextCell + clock;

                //check if caller is at last cell before exit
                if (currentCell + direction >    19 || currentCell + direction < 0)
                    lastEvent = true;
            }
            //next event will be call ending. next cell will not be reached
            else
            {
                nextEventTime = timeRemaining + clock;
                lastEvent = true;
            }
        }

        private double generateGaussian()
        {
            return Math.Sqrt(-2.0 * Math.Log(1.0 - rand.NextDouble())) * Math.Sin(2.0 * Math.PI * (1.0 - rand.NextDouble()));
        }

        private int generateDirection()
        {
            if (rand.NextDouble() > 0.5)
                return 1;
            return -1;
        }
        private double generateSpeed()
        {
            return Math.Abs(Constants.VelocityStdDev * generateGaussian() + Constants.VelocityMean);
        }
        private double generateDuration()
        {
            return Math.Abs(Constants.DurationStdDev * generateGaussian() + Constants.DurationMean);
        }
        private double getTimeToNextCell()
        {
            return (2/speed)*60*60;
        }

        public void moveToNextCell(double clock)
        {
            timeRemaining -= timeToNextCell;    
            currentCell += direction;
            calculateNextEventTime(clock);
        }

        public int CompareTo(Caller other)
        {
            return nextEventTime.CompareTo(other.nextEventTime);
        }
    }
}
