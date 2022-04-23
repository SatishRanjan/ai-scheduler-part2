using System;
using Xunit;
using ai_scheduler.src;
using ai_scheduler.src.models;

namespace AiSchedulerTest
{
    public class PriorityQueueTest
    {
        [Fact]
        public void Enqueue_PriorityQueueTest()
        {
            uint queueDepth = 3;
            PriorityQueue pq = new PriorityQueue(queueDepth);
            pq.Enqueue(new VirtualWorld());
            pq.Enqueue(new VirtualWorld());
            pq.Enqueue(new VirtualWorld());
            pq.Enqueue(new VirtualWorld());
            pq.Enqueue(new VirtualWorld());

            Assert.Equal(pq.Count(), queueDepth);
        }
    }
}
