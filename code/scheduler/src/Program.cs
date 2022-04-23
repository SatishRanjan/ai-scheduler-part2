using System;
using System.Collections.Generic;
using System.IO;
using ai_scheduler.src;
using System.Linq;

namespace ai_scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            // These are the parameters passed from the 
            /// <param name="myCountryName">The name of the country that's labeled as my country</param>
            /// <param name="resourcesFileName">The csv file name containing names, weights and other information related to the resources</param>
            /// <param name="initialWorldStateFileName">The csv file name containing initial state of the world, the resources and it's quantity etc.</param>
            /// <param name="outputScheduleFileName">The file name where the output schedule will be saved</param>
            /// <param name="numOutputSchedules">The number of ordered list of output schedule to be written to the output schedule file</param>
            /// <param name="depthBound">The maximum depth to search assuming the initial depth is 0</param>
            /// <param name="frontierMaxSize">The priority queue size to limit the size of the frontier and the extent of search</param>
            string myCountryName = "";
            string resourcesFileName = "";
            string initialWorldStateFileName = "";
            string outputScheduleFileName = "";
            uint numOutputSchedules = 10;
            uint depthBound = 5;
            uint frontierMaxSize = 500;

            if (args != null && args.Length > 1)
            {
                myCountryName = args[0];
                resourcesFileName = args[1];
                initialWorldStateFileName = args[2];
                outputScheduleFileName = args[3];
                numOutputSchedules = uint.Parse(args[4]);
                depthBound = uint.Parse(args[5]);
                frontierMaxSize = uint.Parse(args[6]);
            }

            Console.WriteLine($"myCountryName: {myCountryName}");
            Console.WriteLine($"resourcesFileName: {resourcesFileName}");
            Console.WriteLine($"initialWorldStateFileName: {initialWorldStateFileName}");
            Console.WriteLine($"outputScheduleFileName: {outputScheduleFileName}");
            Console.WriteLine($"numOutputSchedules: {numOutputSchedules}");
            Console.WriteLine($"depthBound: {depthBound}");
            Console.WriteLine($"frontierMaxSize: {frontierMaxSize}");

            GameScheduler gameScheduler = new GameScheduler();
            gameScheduler.CreateMyCountrySchedule(
                myCountryName,
                resourcesFileName,
                initialWorldStateFileName,
                outputScheduleFileName,
                numOutputSchedules,
                depthBound,
                frontierMaxSize);
        }
    }
}
