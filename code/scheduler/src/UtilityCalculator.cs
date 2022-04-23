using ai_scheduler.src.models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ai_scheduler.src.actions;

namespace ai_scheduler.src
{
    public class UtilityCalculator
    {

        /// <summary>
        /// Calculates the state quality of a country
        /// </summary>
        /// <param name="c">The virtual country object</param>
        /// <returns><see cref="double"/></returns>
        public double CalculateCountryStateQuality(VirtualCountry c)
        {
            if (c == null || c.ResourcesAndQunatities.Count == 0)
            {
                return 0;
            }

            // The state quality of a country = (Sum(ResourceWeight * ResourceQuantity) - Sum((mod(ElectronicWasteResourceWeight) * ElectronicsWasteWeightReductionFator)) + Sum(mod(WasteResourceWeight) * WasteMaterialWeightReductionFactor)))/PopulationCount

            // Get the population count for the country
            int populationCount = c.ResourcesAndQunatities.Where(rq => string.Equals(rq.VirtualResource.Name, "Population", StringComparison.OrdinalIgnoreCase)).First().Quantity;
            double totalRawAndCreatedResourceWeightTimesQuantity = 0.0;
            double totalWasteResourcesWeightTimesQuantity = 0.0;

            foreach (VirtualResourceAndQuantity vrq in c.ResourcesAndQunatities)
            {
                // For created resource calculate the total of the created resource * resource quantity
                if (vrq.VirtualResource.Kind != ResourceKind.Waste && vrq.Quantity > 0)
                {
                    totalRawAndCreatedResourceWeightTimesQuantity = totalRawAndCreatedResourceWeightTimesQuantity + vrq.VirtualResource.Weight * vrq.Quantity;
                }
                // For waste resource calculate the total weight of the Sum((weight of waste resource) * (resource quantity) * (waste material weight reduction factor))
                else if (vrq.VirtualResource.Kind == ResourceKind.Waste && vrq.Quantity > 0)
                {                    
                    double wasteWeightFactor = Constants.WASTE_MATERIAL_WEIGHT_REDUCTIONFACTOR;

                    // For electronics waste there's higher weight reduction factor
                    if (string.Equals(vrq.VirtualResource.Name, "ElectronicsWaste", StringComparison.OrdinalIgnoreCase))
                    {
                        wasteWeightFactor = Constants.ELECTRONICS_WASTE_WEIGHT_REDUCTIONFACTOR;
                    }

                    totalWasteResourcesWeightTimesQuantity = totalWasteResourcesWeightTimesQuantity + vrq.VirtualResource.Weight * wasteWeightFactor * vrq.Quantity;
                }
            }

            double balancedOutWeight = totalRawAndCreatedResourceWeightTimesQuantity + totalWasteResourcesWeightTimesQuantity;
            
            // Normalize the balanced out weight with respect to the population
            double stateQuality = balancedOutWeight / populationCount;
            return stateQuality;
        }

        public double UnDiscountedRewardForACountry(VirtualWorld currentState, string countryName)
        {
            if (currentState == null || (currentState.VirtualCountries == null || currentState.VirtualCountries.Count == 0))
            {
                return 0;
            }

            // Calculate the current state quality of the given country
            VirtualCountry vc = currentState.VirtualCountries.Where(vc => string.Equals(vc.CountryName, countryName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            double currentStateQuality = CalculateCountryStateQuality(vc);

            // Calculate the state quality of the given country from the initial world state
            // by trversing the parent until it's null indicating it's initial state
            VirtualWorld temp = currentState;
            while (temp.Parent != null)
            {
                temp = temp.Parent;
            }

            // Calculate the initial state quality of the given country
            VirtualCountry countryInitialState = temp.VirtualCountries.Where(vc => string.Equals(vc.CountryName, countryName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            double initialStateQuality = CalculateCountryStateQuality(countryInitialState);

            // Calculate the Undiscounted reward by substracting initial state quality from the current state quality
            double discountedRewardForTheCountry = currentStateQuality - initialStateQuality;
            return discountedRewardForTheCountry;
        }

        public double DiscountedRewardForACountry(VirtualWorld currentState, string countryName)
        {
            if (currentState == null || (currentState.VirtualCountries == null || currentState.VirtualCountries.Count == 0) || string.IsNullOrEmpty(countryName))
            {
                return 0;
            }

            // Calculate the undiscounted reward for the country
            double undiscountedRewardForTheCountry = UnDiscountedRewardForACountry(currentState, countryName);

            // Compute the discounted reward by raising the gamma value to the power of the number of operations applied to the current state
            // this is the depth of the current state is at in a schedule path
            double discountedReward = Math.Pow(Constants.GAMMA_VALUE, currentState.ScheduleAndItsParticipatingConuntries.Count) * undiscountedRewardForTheCountry;
            return discountedReward;
        }


        public double CalcProbabilityOfACountryParticipatingInAScheduleToAccept(VirtualWorld currentState, string countryName)
        {
            double k_val = Constants.K_VAL_LOGISTICS_FN;
            double x_0 = Constants.X_0_LOGISTICS_FN;
            double l_val = 1;

            // Calculate the discounted reward for the given country
            double discountedRewardForTheCountry = DiscountedRewardForACountry(currentState, countryName);
            // Calculate -k(x-x0) in the logistic function, where x = discounted reward for a country, k = constant, x0 = constant

            // Using logistic function and discounted reward, calculate the probability
            double logisticVariableVal = -k_val * (discountedRewardForTheCountry - x_0);
            double logisticFnVal = l_val / (1 + Math.Exp(logisticVariableVal));
            return logisticFnVal;
        }

        public double CalcProbalityOfCountriesParticipatingInAScheduleToAccept(VirtualWorld currentState)
        {
            if (currentState == null || (currentState.VirtualCountries == null || currentState.VirtualCountries.Count == 0))
            {
                return 0;
            }

            // Get the list of countries participating in a schedule
            List<string> countriesParticipatingInASchedule = new List<string>();
            foreach (KeyValuePair<TemplateBase, List<string>> opsAndCountries in currentState.ScheduleAndItsParticipatingConuntries)
            {
                foreach (string country in opsAndCountries.Value)
                {
                    if (!countriesParticipatingInASchedule.Contains(country))
                    {
                        countriesParticipatingInASchedule.Add(country);
                    }
                }
            }

            // If there're no countries participating in a schedule then return 0
            if (countriesParticipatingInASchedule.Count == 0)
            {
                return 0;
            }

            // Calculate the probability of countries participating in a schedule,
            // by multiplying the individual probability of a country in a schedule
            double probalityOfCountriesParticipatingInTheScheduleToSucceed = 1;
            foreach (string country in countriesParticipatingInASchedule)
            {
                double probabilityOfIndividualCountry = CalcProbabilityOfACountryParticipatingInAScheduleToAccept(currentState, country);
                probalityOfCountriesParticipatingInTheScheduleToSucceed = probalityOfCountriesParticipatingInTheScheduleToSucceed * probabilityOfIndividualCountry;
            }           

            return probalityOfCountriesParticipatingInTheScheduleToSucceed;
        }

        public double CalcExpectedUtilityToACountryInASchedule(VirtualWorld currentState, string countryName)
        {
            if (currentState == null || (currentState.VirtualCountries == null || currentState.VirtualCountries.Count == 0))
            {
                return 0;
            }

            // EU(c_i, s_j) = (P(s_j) * DR(c_i, s_j)) + ((1-P(s_j)) * C), where c_i = self
            // Where P(s_j) = probability of countries participating in a schedule to succeed
            // DR(c_i, s_j) = discounted reward for a country
            // C = A negative constant value representing the failure cost of a schedule

            // P(s_j)
            double probabilityOfCountriesParticipatingInASchedule = CalcProbalityOfCountriesParticipatingInAScheduleToAccept(currentState);

            // DR(c_i, s_j)
            double discountedRewardForTheCountry = DiscountedRewardForACountry(currentState, countryName);
            double failure_cost_factor = Constants.C_VAL_FAILURE_COST;

            // EU(c_i, s_j)
            double expectedUtility = (probabilityOfCountriesParticipatingInASchedule * discountedRewardForTheCountry) + (1 - probabilityOfCountriesParticipatingInASchedule) * failure_cost_factor;
            return expectedUtility;
        }
    }
}
