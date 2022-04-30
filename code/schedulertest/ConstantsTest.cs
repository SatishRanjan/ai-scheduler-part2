using ai_scheduler.src;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AiSchedulerTest
{
    public class TestConstants
    {
        public const string ResourceFileName = @"C:\Users\satishra\Dropbox\MyDocsLib\MS-Vanderbilt\0-Courses\cs-5260-Artificial-Intelligence\ai-scheduler\initial_data\resource_seeding_data.csv";
        public const string InitialWorldStateFileName = @"C:\Users\satishra\Dropbox\MyDocsLib\MS-Vanderbilt\0-Courses\cs-5260-Artificial-Intelligence\ai-scheduler\initial_data\country_resource_seeding_data.csv";
        public const string OutputScheduleFileName = @"C:\Users\satishra\Dropbox\MyDocsLib\MS-Vanderbilt\0-Courses\cs-5260-Artificial-Intelligence\ai-scheduler\output_data\schedules.txt";

        [Fact]
        public void TestDefaultGamaValue()
        {
            double defaultGama = Utils.GAMMA_VALUE;
            Assert.True(defaultGama == .9);
        }

        [Fact]
        public void TestSetGamaValue()
        {
            double gammaVal = 0.88;
            Utils.GAMMA_VALUE = gammaVal;
            Assert.True(Utils.GAMMA_VALUE == gammaVal);
        }
    }
}
