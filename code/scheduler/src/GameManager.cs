using ai_scheduler.src.actions;
using ai_scheduler.src.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace ai_scheduler.src
{
    /// <summary>
    /// The GameManger has the logic to generate the schedule, and the logic for a 
    /// </summary>
    public class GameManager
    {
        private List<string> _participatingPlayers = new List<string>();
        private List<TemplateBase> _proposedSchedule = new List<TemplateBase>();
        private readonly ActionTransformer _actionTransformer = new ActionTransformer();
        private List<VirtualResource> _resources = new List<VirtualResource>();
        private VirtualWorld _initialStateOfWorld = new VirtualWorld();
        private readonly UtilityCalculator _utilityCalculator = new UtilityCalculator();

        public GameManager(List<VirtualResource> resources, VirtualWorld initialStateOfWorld)
        {
            _resources = resources;
            _initialStateOfWorld = initialStateOfWorld;
        }

        /// <summary>
        /// The function to process the proposed schedule
        /// </summary>
        /// <param name="proposedSchedule"> The proposed schedule</param>
        /// <param name="participatingCountries">The list of participating countries </param>
        /// <param name="proposedWorldState">The proposed macro world state</param>
        /// <returns></returns>
        public bool ProcessProposeSchedule(List<TemplateBase> proposedSchedule, List<string> participatingCountries, VirtualWorld proposedWorldState)
        {
            // If the either of the input parameters are null or empty return false
            if (proposedSchedule == null
                || proposedSchedule.Count == 0
                || participatingCountries == null
                || proposedWorldState == null)
            {
                return false;
            }

            // Count the number of countries participating in a schedule to accept the proposed schedule
            // Proposed schedule acceptence criteria
            /*
                For a given participating country, if the solution state expected utility is greate than the initial state expected utility, 
                then the participating country is going to accept the schedule
            */
            int acceptingCountriesCount = 0;
            foreach (string country in participatingCountries)
            {
                double initialStateExpectedUtility = _utilityCalculator.CalcExpectedUtilityToACountryInASchedule(_initialStateOfWorld, country);
                double finalStateExpectedUtility = _utilityCalculator.CalcExpectedUtilityToACountryInASchedule(proposedWorldState, country);

                // If the final state macro expected utility for a participating country is greater than the initial state expected utility
                // then the country will accept the schedule
                if (finalStateExpectedUtility > initialStateExpectedUtility)
                {
                    acceptingCountriesCount++;
                }
            }

            // If more than half of the participating country accept the utility then the schedule will be considered as accepted
            if (acceptingCountriesCount > participatingCountries.Count / 2)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// This function generates the successor state of the world by applying the transformations and transfers
        /// </summary>
        /// <param name="currentState">The current state of the world</param>
        /// <returns><see cref="List<VirtualWorld>"/></returns>
        public List<VirtualWorld> GenerateSuccessorsDecisionForest(VirtualWorld currentState)
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

            // Randomly select a non-waste resource for the transfer
            List<VirtualResource> nonWasteResoureCount = _resources.Where(r => !r.IsWaste).ToList();
            string resourceToTransfer = nonWasteResoureCount[Utils.GenerateRandomInt(0, nonWasteResoureCount.Count)].Name;

            VirtualResourceAndQuantity myCountryMetallicAlloys = myCountry.ResourcesAndQunatities.Where(vr => vr.VirtualResource.Name == resourceToTransfer).FirstOrDefault();
            TransformTemplate transformTemplate = TemplateProvider.GetTransformTemplate(resourceToTransfer);
            if (transformTemplate != null)
            {
                // Increase the yield to produce the metallic alloys
                transformTemplate.IncreaseYield(20);
                transformTemplate.CountryName = myCountry.CountryName;
                VirtualWorld clone = currentState.Clone();
                clone.Parent = currentState;
                VirtualWorld transformedWorld = _actionTransformer.ApplyTransformTemplate(clone, transformTemplate);
                if (transformedWorld != null)
                {
                    transformedWorld.SearchDepth++;
                    successors.Add(transformedWorld);
                }
            }

            // Transfer timber from country first country to the self
            VirtualWorld clone1 = currentState.Clone();
            clone1.Parent = currentState;
            TransferTemplate transferTemplate = TemplateProvider.GetTransferTemplate();
            transferTemplate.FromCountry = clone1.VirtualCountries[0].CountryName;
            transferTemplate.ToCountry = myCountry.CountryName;

            // Randomly select  created resource to transfer
            List<VirtualResource> createdResoureCount = _resources.Where(r => r.Kind == ResourceKind.Created).ToList();
            string randomResourceToTransfer = nonWasteResoureCount[Utils.GenerateRandomInt(0, nonWasteResoureCount.Count)].Name;

            transferTemplate.ResourceAndQuantityMapToTransfer.Add(randomResourceToTransfer, 50);
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

        public PriorityQueue GenerateSchedules(VirtualWorld initialState, uint depthBound)
        {
            if (initialState == null || initialState.VirtualCountries.Count == 0)
            {
                throw new ArgumentNullException("The countries with initial state cannot be null or empty");
            }

            // Initialize the solutions as an empty priority queue
            PriorityQueue solutions = new PriorityQueue();

            // Initialize and add the initial world state to the frontier Stack
            // This is the implementaion of the heuristic depth first search
            Stack<VirtualWorld> frontier = new Stack<VirtualWorld>();
            frontier.Push(initialState);

            while (frontier.Count > 0)
            {
                // Start explorign the depth bounded search picking the state with max Expected utility for the self
                // This is the implentation of the heuristic depth first search
                VirtualWorld worldState = frontier.Pop();
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

                    // This is the implementation of heuristic depth first search
                    successors.OrderByDescending(s => s.ExpectedUtilityForSelf);
                    foreach (VirtualWorld successor in successors)
                    {
                        frontier.Push(successor);
                    }
                }
            }

            return solutions;
        }
    }
}
