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
            /// <param name="gammaValue">Configurable gamma value for the Logistics function</param>
            /// <param name="c_val_failure_cost">Configurable C value failure cost factor to compute the expected utility</param>
            /// <param name="k_val_logistics_function">Configurable K value to compute the expected utility</param>
            string myCountryName = "";
            string resourcesFileName = "";
            string initialWorldStateFileName = "";
            string outputScheduleFileName = "";
            uint numOutputSchedules = 10;
            uint depthBound = 5;
            double gammaValue = .9;
            double c_val_failure_cost = .8;
            double k_val_logistics_function = 1;

            if (args != null && args.Length > 1)
            {
                myCountryName = args[0];
                resourcesFileName = args[1];
                initialWorldStateFileName = args[2];
                outputScheduleFileName = args[3];
                numOutputSchedules = uint.Parse(args[4]);
                depthBound = uint.Parse(args[5]);
                gammaValue = double.Parse(args[6]);
                c_val_failure_cost = double.Parse(args[7]);
                k_val_logistics_function = double.Parse(args[8]);
            }

            Console.WriteLine($"myCountryName: {myCountryName}");
            Console.WriteLine($"resourcesFileName: {resourcesFileName}");
            Console.WriteLine($"initialWorldStateFileName: {initialWorldStateFileName}");
            Console.WriteLine($"outputScheduleFileName: {outputScheduleFileName}");
            Console.WriteLine($"numOutputSchedules: {numOutputSchedules}");
            Console.WriteLine($"depthBound: {depthBound}");
            Console.WriteLine($"gammaValue: {gammaValue}");
            Console.WriteLine($"c_val_failure_cost: {c_val_failure_cost}");
            Console.WriteLine($"k_val_logistics_function: {k_val_logistics_function}");

            GameScheduler gameScheduler = new GameScheduler();
            gameScheduler.CreateMyCountrySchedule(
                myCountryName,
                resourcesFileName,
                initialWorldStateFileName,
                outputScheduleFileName,
                numOutputSchedules,
                depthBound,
                gammaValue,
                c_val_failure_cost,
                k_val_logistics_function);
        }
    }
}
