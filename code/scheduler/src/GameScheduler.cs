using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using ai_scheduler.src.models;
using ai_scheduler.src.actions;
using System.Diagnostics;

namespace ai_scheduler.src
{
    public class GameScheduler
    {
        private readonly DataProvider _dataProvider;
        private List<VirtualResource> _resources = new List<VirtualResource>();
        private VirtualWorld _initialStateOfWorld = new VirtualWorld();
        public GameManager _gameManager;
        private string _outputScheduleFileName;
        private uint _numOutputSchedules;

        public GameScheduler()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Creates the schedule and saves the schedules for my country
        /// </summary>
        /// <param name="myCountryName">The name of the country that's labeled as my country</param>
        /// <param name="resourcesFileName">The csv file name containing names, weights and other information related to the resources</param>
        /// <param name="initialWorldStateFileName">The csv file name containing initial state of the world, the resources and it's quantity etc.</param>
        /// <param name="outputScheduleFileName">The file name where the output schedule will be saved</param>
        /// <param name="numOutputSchedules">The number of ordered list of output schedule to be written to the output schedule file</param>
        /// <param name="depthBound">The maximum depth to search assuming the initial depth is 0</param>
        /// <param name="gammaValue">Configurable gamma value for the Logistics function</param>
        /// <param name="c_val_failure_cost">Configurable C value failure cost factor to compute the expected utility</param>
        /// <param name="k_val_logistics_function">Configurable K value to compute the expected utility</param>
        public void CreateMyCountrySchedule(
            string myCountryName,
            string resourcesFileName,
            string initialWorldStateFileName,
            string outputScheduleFileName,
            uint numOutputSchedules,
            uint depthBound,
            double gammaValue = .9,
            double c_val_failure_cost = .8,
            double k_val_logistics_function = 1)
        {
            // Validat the inputs
            bool areInputValid = ValidateInputs(myCountryName, resourcesFileName, initialWorldStateFileName, outputScheduleFileName);
            if (!areInputValid)
            {
                return;
            }

            _outputScheduleFileName = outputScheduleFileName;
            _numOutputSchedules = numOutputSchedules;

            // Get the list of virtual resources and initial state of the virtual world from the csv files
            _resources = _dataProvider.GetResources(resourcesFileName);
            _initialStateOfWorld = _dataProvider.GetVirtualWorld(initialWorldStateFileName, _resources);

            Utils.GAMMA_VALUE = gammaValue;
            Utils.C_VAL_FAILURE_COST = c_val_failure_cost;
            Utils.K_VAL_LOGISTICS_FN = k_val_logistics_function;

            // Set one of the country as self
            VirtualCountry myCountry = null;
            if (!string.IsNullOrEmpty(myCountryName))
            {
                myCountry = _initialStateOfWorld.VirtualCountries.FirstOrDefault(c => string.Equals(c.CountryName, myCountryName, StringComparison.OrdinalIgnoreCase));
            }
            // Else set the first country as the self by default
            else
            {
                myCountry = _initialStateOfWorld.VirtualCountries.First();
            }

            // Set self to my country
            if (myCountry != null)
            {
                myCountry.IsSelf = true;
            }

            // Create the game manager to process a proposed schedule of the self country to the list of participating countries
            _gameManager = new GameManager(_resources, _initialStateOfWorld);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Generate the solution schedule from the game manager
            PriorityQueue solutions = _gameManager.GenerateSchedules(_initialStateOfWorld, depthBound);

            sw.Stop();
            Console.WriteLine($"Total time take for schedules (milliseconds): {sw.ElapsedMilliseconds}");

            // Print the output schedule
            GenerateGameSchedulesOutput(solutions, _numOutputSchedules, _outputScheduleFileName);

            // Propose the solution schedule
            bool outcome = ProposeSchedule(solutions);

            // Print the outcome message
            if (outcome)
            {
                Console.WriteLine($"Congratulations {myCountryName}, your proposed schedule has been accepted by the participating countries.");
            }
            else
            {
                Console.WriteLine($"Sorry {myCountryName}, your proposed schedule has not been acceped by paricipating countries.");
            }
        }


        public bool ProposeSchedule(PriorityQueue solution)
        {
            // If no solution is achieved just return
            if (solution == null || solution.Items?.Count == 0)
            {
                return false;
            }

            // Propose the highest utility schedule for self to the countries participating in the schedule
            VirtualWorld proposedState = solution.Items.OrderByDescending(s => s.ExpectedUtilityForSelf).First();
            List<TemplateBase> proposedSchedule = new List<TemplateBase>();
            List<string> participatingCountries = new List<string>();

            foreach (KeyValuePair<TemplateBase, List<string>> item in proposedState.ScheduleAndItsParticipatingConuntries)
            {
                proposedSchedule.Add(item.Key);
                foreach (string participatingCountry in item.Value)
                {
                    if (!participatingCountries.Contains(participatingCountry))
                    {
                        participatingCountries.Add(participatingCountry);
                    }
                }
            }

            // Proposed the highes utility schedule for self to the participating countries
            bool acceptanceResult = _gameManager.ProcessProposeSchedule(proposedSchedule, participatingCountries, proposedState);

            return acceptanceResult;
        }

        public void GenerateGameSchedulesOutput(PriorityQueue solutions, uint numberOfOutputSchedules, string outputScheduleFileName)
        {
            // In either of these conditions, schedules output cannot be generated
            if ((solutions == null || solutions.Items.Count == 0) || numberOfOutputSchedules <= 0 || string.IsNullOrEmpty(outputScheduleFileName))
            {
                return;
            }

            int counter = 0;
            string result = $"The schedules output for the given number of solutions generated at {DateTime.Now} : \n";
            while (counter < solutions.Items.Count && numberOfOutputSchedules > 0)
            {
                VirtualWorld solutionState = solutions.Items[counter];
                result = result + ScheduleSerializer.SerializeSchedules(solutionState) + "\n";
                counter++;
                numberOfOutputSchedules--;
            }

            if (!File.Exists(outputScheduleFileName))
            {
                File.Create(outputScheduleFileName);
            }

            File.WriteAllText(outputScheduleFileName, result);
        }

        #region Private Members

        // The function validates the input to the GameScheduler
        private bool ValidateInputs(
            string myCountryName,
            string resourcesFileName,
            string initialWorldStateFileName,
            string outputScheduleFileName)
        {
            // If the my country name is null or empty throw an exception
            if (string.IsNullOrEmpty(myCountryName))
            {
                Console.WriteLine("My country name cannot be null or empty");
                return false;
            }

            // If the resource file name is null or empty or if the file doesn't exist
            if (string.IsNullOrEmpty(resourcesFileName) || !File.Exists(resourcesFileName))
            {
                Console.WriteLine("The resource file name is null or empty or it doesn't exists");
                return false;
            }

            // If initial world state file name is null or empty or if the file doesn't exist
            if (string.IsNullOrEmpty(initialWorldStateFileName) || !File.Exists(initialWorldStateFileName))
            {
                Console.WriteLine("The initial world state file is null or empty or it doesn't exists");
                return false;
            }

            // If output schedule file name is null or empty 
            if (string.IsNullOrEmpty(outputScheduleFileName))
            {
                Console.WriteLine("The output schedule file is null or empty or it doesn't exists");
                return false;
            }

            return true;
        }

        #endregion Private Members
    }
}


