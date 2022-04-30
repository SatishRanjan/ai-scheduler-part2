using ai_scheduler.src;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AiSchedulerTest
{
    public class GameManagerTest
    {
        [Fact]
        public void RandomInt_Test()
        {
            int min = 5;
            int max = 15;
            for (int i = 0; i < 100; ++i)
            {
                int result = Utils.GenerateRandomInt(min, max);
                Assert.True(result >= min && result <= max);
            }          
        }
    }
}
