using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.models
{
    public class VirtualResource
    {
        /// <summary>
        /// Gets or sets the name of the resource
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the weight of the resource
        /// </summary>
        public double Weight
        {
            get;
            set;
        }

        /// <summary>
        /// The flag indicating if a resource is renewable
        /// </summary>
        public bool IsRenewable
        {
            get;
            set;
        }

        /// <summary>
        /// The flag indicating if a resource is the waste
        /// </summary>
        public bool IsWaste
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if a resource is transferrable
        /// </summary>
        public bool IsTransferrable
        {
            get;
            set;
        }

        /// <summary>
        /// The kind of resources
        /// </summary>
        public ResourceKind Kind
        {
            get;
            set;
        }

        /// <summary>
        /// Deep clone the virtual resource
        /// </summary>
        /// <returns><see cref="VirtualResource"/></returns>
        public VirtualResource Clone()
        {
            VirtualResource vr = new VirtualResource
            {
                Name = this.Name,
                Kind = this.Kind,
                Weight = this.Weight,
                IsRenewable = this.IsRenewable,
                IsTransferrable = this.IsTransferrable,
                IsWaste = this.IsWaste
            };

            return vr;
        }
    }
}
