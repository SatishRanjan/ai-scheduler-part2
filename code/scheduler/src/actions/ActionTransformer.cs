using ai_scheduler.src.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ai_scheduler.src.actions
{
    public class ActionTransformer
    {
        public VirtualWorld ApplyTransformTemplate(VirtualWorld worldState, TransformTemplate transformOperation)
        {
            // Get the name of the country and it's resources and quantities from the given world state
            string countryName = transformOperation.CountryName;
            List<VirtualResourceAndQuantity> resourcesAndQuantitiesOfTheCountry = worldState.VirtualCountries.Where(c => string.Equals(c.CountryName, countryName, StringComparison.OrdinalIgnoreCase)).First().ResourcesAndQunatities;

            bool isOperationValid = ValidateTheResouceAndQuantityForOperation(resourcesAndQuantitiesOfTheCountry, transformOperation.INPUTS);
            // If the country tring to perform the transform action doesn't have the enough resources then the transformation will fail
            // Resource transformation validation check
            if (!isOperationValid)
            {
                return null;
            }

            // If the precondition of the transformation passes, subtract the input quantity
            foreach (KeyValuePair<string, int> inputResourceAndQuantity in transformOperation.INPUTS)
            {
                VirtualResourceAndQuantity currentResourceAndQuantity = resourcesAndQuantitiesOfTheCountry.Where(rq => string.Equals(rq.VirtualResource.Name, inputResourceAndQuantity.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                currentResourceAndQuantity.Quantity = currentResourceAndQuantity.Quantity - inputResourceAndQuantity.Value;
            }

            // Add the output resources produces as part of the transform action
            foreach (KeyValuePair<string, int> outputResourceAndQuantity in transformOperation.OUTPUTS)
            {
                VirtualResourceAndQuantity currentResourceAndQuantity = resourcesAndQuantitiesOfTheCountry.Where(rq => string.Equals(rq.VirtualResource.Name, outputResourceAndQuantity.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                currentResourceAndQuantity.Quantity = currentResourceAndQuantity.Quantity + outputResourceAndQuantity.Value;
            }
           
            worldState.ScheduleAndItsParticipatingConuntries.Add(transformOperation, new List<string> { countryName });
            worldState.AppliedAction = transformOperation;
            return worldState;
        }

        public VirtualWorld ApplyTransferTemplate(VirtualWorld worldState, TransferTemplate transferOperation)
        {
            VirtualCountry fromCountry = worldState.VirtualCountries.Where(c => string.Equals(c.CountryName, transferOperation.FromCountry, StringComparison.OrdinalIgnoreCase)).First();
            VirtualCountry toCountry = worldState.VirtualCountries.Where(c => string.Equals(c.CountryName, transferOperation.ToCountry, StringComparison.OrdinalIgnoreCase)).First();

            // Validate if the from country have the required quantity of the resources to transfer
            // Precondition - if not satisfied return null
            bool isOperationValid = ValidateTheResouceAndQuantityForOperation(fromCountry.ResourcesAndQunatities, transferOperation.ResourceAndQuantityMapToTransfer);

            // If the country is trying to perform the transfer action doesn't have the enough resources then the transfer cannot proceed
            // Resource transfer validation check
            if (!isOperationValid)
            {
                return null;
            }

            // If the preconditions have been satisfied, perform the transfer
            foreach (KeyValuePair<string, int> resourceAndQuantity in transferOperation.ResourceAndQuantityMapToTransfer)
            {
                // substract the resource quantity from country and add the quantity to the tocountry
                VirtualResourceAndQuantity fromCountryResourceAndQuantity = fromCountry.ResourcesAndQunatities.Where(rq => string.Equals(rq.VirtualResource.Name, resourceAndQuantity.Key, StringComparison.OrdinalIgnoreCase)).First();
                fromCountryResourceAndQuantity.Quantity = fromCountryResourceAndQuantity.Quantity - resourceAndQuantity.Value;

                VirtualResourceAndQuantity toCountryVirtualResourceAndQuantity = toCountry.ResourcesAndQunatities.Where(rq => string.Equals(rq.VirtualResource.Name, resourceAndQuantity.Key, StringComparison.OrdinalIgnoreCase)).First();
                toCountryVirtualResourceAndQuantity.Quantity = toCountryVirtualResourceAndQuantity.Quantity + resourceAndQuantity.Value;
            }

            // Keep track of the operation that changed this state of the world, and the participating countries
            // This forms the data for finding the list of the participating countries in a schedule           
            worldState.ScheduleAndItsParticipatingConuntries.Add(transferOperation, new List<string> { fromCountry.CountryName, toCountry.CountryName });
            worldState.AppliedAction = transferOperation;
            return worldState;
        }

        public bool ValidateTheResouceAndQuantityForOperation(List<VirtualResourceAndQuantity> resourcesAndQuantitiesOfTheCountry, Dictionary<string, int> requiredResourcesAndQuantities)
        {
            if (resourcesAndQuantitiesOfTheCountry == null || resourcesAndQuantitiesOfTheCountry.Count() == 0 || requiredResourcesAndQuantities == null || requiredResourcesAndQuantities.Count() == 0)
            {
                return false;
            }

            foreach (KeyValuePair<string, int> resourceAndQuantity in requiredResourcesAndQuantities)
            {
                // If the country tring to perform the transform action doesn't have the enough resources then the template action will fail
                // Precondition for the resources of the country to apply the operation
                VirtualResourceAndQuantity currentStateResourceAndQuantity = resourcesAndQuantitiesOfTheCountry.Where(
                    rq => string.Equals(rq.VirtualResource.Name, resourceAndQuantity.Key, StringComparison.OrdinalIgnoreCase)
                    && rq.Quantity >= resourceAndQuantity.Value).FirstOrDefault();

                if (currentStateResourceAndQuantity == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
