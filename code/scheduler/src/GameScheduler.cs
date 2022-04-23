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
        private readonly ActionTransformer _actionTransformer = new ActionTransformer();
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
        /// <param name="frontierMaxSize">The priority queue size to limit the size of the frontier and the extent of search</param>
        public void CreateMyCountrySchedule(
            string myCountryName,
            string resourcesFileName,
            string initialWorldStateFileName,
            string outputScheduleFileName,
            uint numOutputSchedules,
            uint depthBound,
            uint frontierMaxSize)
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

            // Call the function that generates the schedule
            GenerateSchedules(_initialStateOfWorld, depthBound, frontierMaxSize);
        }

        public void GenerateSchedules(VirtualWorld initialState, uint depthBound, uint frontierMaxSize)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (initialState == null || initialState.VirtualCountries.Count == 0)
            {
                throw new ArgumentNullException("The countries with initial state cannot be null or empty");
            }

            // Initialize the solutions as an empty priority queue
            PriorityQueue solutions = new PriorityQueue();

            // Initialize and add the initial world state to the frontier priority queue
            PriorityQueue frontier = new PriorityQueue(frontierMaxSize);
            frontier.Enqueue(initialState);

            while (!frontier.IsEmpty())
            {
                VirtualWorld worldState = frontier.Dequeue();
                // If the schedule list to the world state reaches the search depth bound, add it to the solution priority queue 
                if (worldState.SearchDepth == depthBound)
                {
                    solutions.Enqueue(worldState);
                }
                else
                {
                    // Generate the successors
                    List<VirtualWorld> successors = GenerateSuccessors(worldState);
                    if (successors == null || successors.Count == 0)
                    {
                        continue;
                    }

                    foreach (VirtualWorld successor in successors)
                    {
                        frontier.Enqueue(successor);
                    }
                }
            }

            GenerateGameSchedulesOutput(solutions, _numOutputSchedules, _outputScheduleFileName);
            sw.Stop();
            Console.WriteLine($"Total time take for schedules (milliseconds): {sw.ElapsedMilliseconds}");
        }

        /// <summary>
        /// This function generates the successor state of the world by applying the transformations and transfers
        /// </summary>
        /// <param name="currentState">The current state of the world</param>
        /// <returns><see cref="List<VirtualWorld>"/></returns>
        public List<VirtualWorld> GenerateSuccessors(VirtualWorld currentState)
        {
            List<VirtualWorld> successors = new List<VirtualWorld>();
            if (currentState == null)
            {
                return successors;
            }

            /*
             * At this time to make the project functional, I'm taking the below approach to generate the successor
             * If my country doesn't have the housing then it transforms the available elements to produce the raw materials needed for the housing
             * First, if my county (self) doesn't have MatallicAlloys needed for the housing, it produces some
             * Then it transfer Timber as well 
             * Then produce housing
             */

            VirtualCountry myCountry = currentState.VirtualCountries.Where(c => c.IsSelf).FirstOrDefault();
            List<TemplateBase> templates = TemplateProvider.GetAllTemplates();

            VirtualResourceAndQuantity myCountryMetallicAlloys = myCountry.ResourcesAndQunatities.Where(vr => vr.VirtualResource.Name == "MetallicAlloys").FirstOrDefault();
            // If there's no metallic alloy, apply the transformation to create some
            if (myCountryMetallicAlloys != null && myCountryMetallicAlloys.Quantity == 0)
            {
                AlloyTransformTemplate alloyTransformTemplate = TemplateProvider.GetTransformTemplate("MetallicAlloys") as AlloyTransformTemplate;
                if (alloyTransformTemplate != null)
                {
                    // Increase the yield to produce the metallic alloys
                    alloyTransformTemplate.IncreaseYield(20);
                    alloyTransformTemplate.CountryName = myCountry.CountryName;
                    VirtualWorld clone = currentState.Clone();
                    clone.Parent = currentState;
                    VirtualWorld transformedWorld = _actionTransformer.ApplyTransformTemplate(clone, alloyTransformTemplate);
                    if (transformedWorld != null)
                    {
                        transformedWorld.SearchDepth++;
                        successors.Add(transformedWorld);
                    }
                }
            }

            // Transfer timber from country first country to the self
            VirtualWorld clone1 = currentState.Clone();
            clone1.Parent = currentState;
            TransferTemplate transferTemplate = TemplateProvider.GetTransferTemplate();
            transferTemplate.FromCountry = clone1.VirtualCountries[0].CountryName;
            transferTemplate.ToCountry = myCountry.CountryName;
            transferTemplate.ResourceAndQuantityMapToTransfer.Add("Timber", 50);
            VirtualWorld transformedWorld1 = _actionTransformer.ApplyTransferTemplate(clone1, transferTemplate);
            if (transformedWorld1 != null)
            {
                transformedWorld1.SearchDepth++;
                successors.Add(transformedWorld1);
            }

            // If there're materials available then transform into the housing
            if (myCountryMetallicAlloys != null && myCountryMetallicAlloys.Quantity > 0)
            {
                HousingTransformTemplate housingTransformTemplate = TemplateProvider.GetTransformTemplate("Housing") as HousingTransformTemplate;
                housingTransformTemplate.IncreaseYield(5);
                housingTransformTemplate.CountryName = myCountry.CountryName;
                VirtualWorld clone = currentState.Clone();
                clone.Parent = currentState;
                VirtualWorld transformedWorld = _actionTransformer.ApplyTransformTemplate(clone, housingTransformTemplate);
                if (transformedWorld != null)
                {
                    transformedWorld.SearchDepth++;
                    successors.Add(transformedWorld);
                }
            }

            return successors;
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
