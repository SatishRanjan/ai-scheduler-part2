using ai_scheduler.src;
using ai_scheduler.src.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AiSchedulerTest
{
    public class UtilityCalculatorTest
    {
        private readonly VirtualWorld _virtualWorld;
        private readonly List<VirtualResource> _resources;
        private readonly UtilityCalculator _utilityCalculator = new UtilityCalculator();
        public UtilityCalculatorTest()
        {
            DataProvider dataProvider = new DataProvider();
            _resources = dataProvider.GetResources(TestConstants.ResourceFileName);
            _virtualWorld = dataProvider.GetVirtualWorld(TestConstants.InitialWorldStateFileName, _resources);
            VirtualCountry vc = _virtualWorld.VirtualCountries.Where(c => c.CountryName == "Carpania").First();
            vc.IsSelf = true;
        }

        [Fact]
        public void StateQuality_Test()
        {
            if (_virtualWorld == null 
                || (_virtualWorld.VirtualCountries == null || _virtualWorld.VirtualCountries.Count() == 0))
            {
                return;
            }
            // 5.0333333333333332
            // 5.208333333333333
            VirtualCountry self = _virtualWorld.VirtualCountries.Where(c => c.IsSelf).FirstOrDefault();
            double stateQualitySelf = _utilityCalculator.CalculateCountryStateQuality(self);
            Assert.True(stateQualitySelf != 0);

            // State Quality of the other countries
            foreach (VirtualCountry vc in _virtualWorld.VirtualCountries)
            {
                double stateQuality = _utilityCalculator.CalculateCountryStateQuality(vc);
                Assert.True(stateQuality != 0);
            }
        }
    }
}
