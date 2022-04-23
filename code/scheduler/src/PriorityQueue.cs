using System;
using System.Collections.Generic;
using System.Text;
using ai_scheduler.src.models;
using System.Linq;

namespace ai_scheduler.src
{
    public class PriorityQueue
    {
        private List<VirtualWorld> _items = new List<VirtualWorld>();
        private readonly uint _queueMaxSize = uint.MaxValue;

        public PriorityQueue(uint queueMaxSize = 0)
        {
            _queueMaxSize = queueMaxSize;
        }

        public void Enqueue(VirtualWorld worldState)
        {
            _items.Add(worldState);
            _items = _items.OrderByDescending(item => item.CalcExpectedUtilityForSelf()).ToList();

            if (_queueMaxSize > 1 && _items.Count() > _queueMaxSize)
            {
                // Removes the items from the list starting from the item queMaxSize till the end of the list
                // i.e. it only keeps the _queueMaxSize world state in the List with the highest value of ExpectedUtilityForSelf
                _items.RemoveRange((int)_queueMaxSize, _items.Count() - (int)_queueMaxSize);
            }
        }

        public VirtualWorld Dequeue()
        {
            if (_items.Count() == 0)
            {
                return null;
            }

            VirtualWorld dequeuedItem = _items[0];
            _items.RemoveAt(0);
            return dequeuedItem;
        }

        public bool IsEmpty()
        {
            return _items.Count() <= 0;
        }

        public uint Count()
        {
            return (uint)_items.Count;
        }

        public List<VirtualWorld> Items
        {
            get
            {
                return _items;
            }
        }
    }
}
