using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ai_scheduler.src.models;
using System.Linq;

namespace ai_scheduler.src
{
    public class DataProvider
    {
        public VirtualWorld GetVirtualWorld(string initialStateOfVirtualWorldFilePath, List<VirtualResource> virtualResources)
        {
            VirtualWorld virtualWorld = new VirtualWorld();
            virtualWorld.VirtualCountries = new List<VirtualCountry>();

            // If the initial state file doesn't exist then return the empty state of the virtual world
            if (!File.Exists(initialStateOfVirtualWorldFilePath))
            {
                return virtualWorld;
            }

            //string[] countryResourceFileLines = File.ReadAllLines(initialStateOfVirtualWorldFilePath);
            string[] countryResourceFileLines = GetLinesFromFile(initialStateOfVirtualWorldFilePath);

            if (countryResourceFileLines.Length > 1)
            {
                string[] resources = countryResourceFileLines[0].Split(',');
                for (int i = 1; i < countryResourceFileLines.Length; ++i)
                {
                    string[] countryAndResourceQuantities = countryResourceFileLines[i].Split(',');
                    string countryName = countryAndResourceQuantities[0].Trim();
                    VirtualCountry virtualCountry = new VirtualCountry
                    {
                        CountryName = countryName,
                        ResourcesAndQunatities = new List<VirtualResourceAndQuantity>()
                    };

                    for (int j = 1; j < countryAndResourceQuantities.Length; ++j)
                    {
                        VirtualResourceAndQuantity virtualResourceAndQuantity = new VirtualResourceAndQuantity();

                        // Retrieve the VirtualResource from the resource list
                        VirtualResource vr = null;
                        if (!string.IsNullOrEmpty(resources[j].Trim()))
                        {
                            vr = virtualResources.Where(r => r.Name == resources[j].Trim()).FirstOrDefault();
                        }

                        if (vr != null)
                        {
                            virtualResourceAndQuantity.VirtualResource = vr.Clone();
                        }
                        else
                        {
                            virtualResourceAndQuantity.VirtualResource = new VirtualResource
                            {
                                Name = resources[j].Trim()
                            };
                        }

                        virtualResourceAndQuantity.Quantity = int.Parse(countryAndResourceQuantities[j].Trim());
                        virtualCountry.ResourcesAndQunatities.Add(virtualResourceAndQuantity);
                    }

                    virtualWorld.VirtualCountries.Add(virtualCountry);
                }
            }

            return virtualWorld;
        }

        public List<VirtualResource> GetResources(string resourceInfoFilePath)
        {
            List<VirtualResource> resourceInfoList = new List<VirtualResource>();

            // If the resource file doesn't exist, return the empty list of VirtualResource
            if (!File.Exists(resourceInfoFilePath))
            {
                return resourceInfoList;
            }

            //string[] resourceInfoLines = File.ReadAllLines(resourceInfoFilePath);
            string[] resourceInfoLines = GetLinesFromFile(resourceInfoFilePath);
            if (resourceInfoLines.Length > 1)
            {
                VirtualResource vr = null;
                for (int i = 1; i < resourceInfoLines.Length; ++i)
                {
                    string[] eachLines = resourceInfoLines[i].Split(',');
                    vr = new VirtualResource
                    {
                        Name = eachLines[0].Trim(),
                        Weight = double.Parse(eachLines[1].Trim()),
                        Kind = (ResourceKind)Enum.Parse(typeof(ResourceKind), eachLines[2].Trim()),
                        IsRenewable = eachLines[3].Trim() == "yes" ? true : false,
                        IsTransferrable = eachLines[4].Trim() == "yes" ? true : false,
                        IsWaste = eachLines[5].Trim() == "yes" ? true : false,
                    };

                    resourceInfoList.Add(vr);
                }
            }

            return resourceInfoList;
        }

        /// <summary>
        /// Reads the lines from the file
        /// </summary>
        /// <param name="filePath">The file path to read</param>
        /// <returns><see cref="string[]"/></returns>
        private string[] GetLinesFromFile(string filePath)
        {
            List<string> lines = new List<string>();
            using (FileStream readFs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(readFs))
                {
                    while (sr.Peek() >= 0) // reading the live data if exists
                    {
                        string str = sr.ReadLine();
                        if (str != null)
                        {
                            lines.Add(str);
                        }
                    }
                }
            }

            return lines.ToArray();
        }
    }
}
