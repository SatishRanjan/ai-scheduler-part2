using ai_scheduler.src.actions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace ai_scheduler.src.models
{
    /// <summary>
    /// VirtualCoutry consisting of CountryName and a list of VirtualResourceAndQuantity
    /// </summary>
    public class VirtualCountry
    {
        // Initialize and empty VirtualResourceAndQuantity list
        public VirtualCountry()
        {
            ResourcesAndQunatities = new List<VirtualResourceAndQuantity>();
            ScheduleList = new List<TemplateBase>();
        }

        /// <summary>
        /// The list of applied templates, aka. schedules
        /// </summary>
        public List<TemplateBase> ScheduleList { get; set; }

        /// <summary>
        /// Returns the name of the country
        /// </summary>
        public string CountryName
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the list of virtual resources and it's quantities for the country
        /// </summary>
        public List<VirtualResourceAndQuantity> ResourcesAndQunatities
        {
            get;
            set;
        }

        /// <summary>
        /// A flag indicating if a country is "self" or not (self = true else false)
        /// </summary>
        public bool IsSelf
        {
            get;
            set;
        }

        public double StateQuality
        {
            get;
            set;
        }

        public int SearchDepth
        {
            get;
            set;
        }

        public VirtualCountry Clone()
        {
            VirtualCountry clonedVc = new VirtualCountry
            {
                CountryName = this.CountryName,
                ResourcesAndQunatities = new List<VirtualResourceAndQuantity>(),
                ScheduleList = new List<TemplateBase>()
            };

            foreach (VirtualResourceAndQuantity vRQ in this.ResourcesAndQunatities)
            {
                VirtualResourceAndQuantity newVrQ = new VirtualResourceAndQuantity
                {
                    VirtualResource = new VirtualResource
                    {
                        Name = vRQ.VirtualResource.Name,
                        Kind = vRQ.VirtualResource.Kind,
                        Weight = vRQ.VirtualResource.Weight,
                        IsWaste = vRQ.VirtualResource.IsWaste,
                        IsRenewable = vRQ.VirtualResource.IsRenewable,
                        IsTransferrable = vRQ.VirtualResource.IsTransferrable
                    },
                    Quantity = vRQ.Quantity
                };

                clonedVc.ResourcesAndQunatities.Add(newVrQ);
            }

            clonedVc.SearchDepth = this.SearchDepth;
            clonedVc.IsSelf = this.IsSelf;

            foreach (TemplateBase schedule in this.ScheduleList)
            {
                ScheduleList.Add(schedule);
            }

            return clonedVc;
        }
    }
}
