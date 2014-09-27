using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.UI.Components
{
    public class Counter : ICounter
    {
        private int increment = 1;
        private int initialValue = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Counter"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value for the counter.</param>
        /// <param name="increment">The amount to be used for incrementing/decrementing.</param>
        public Counter(int initialValue = 0, int increment = 1)
        {
            this.initialValue = initialValue;
            this.increment = increment;
            this.Count = initialValue;
        }
        
        public int Count { get; private set; }

        /// <summary>
        /// Increments this instance.
        /// </summary>
        public bool Increment()
        {
            if (Count == int.MaxValue)
                return false;

            try
            {
                Count = checked(Count + this.increment);
            }
            catch (System.OverflowException)
            {
                Count = int.MaxValue;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decrements this instance.
        /// </summary>
        public bool Decrement()
        {
            if (Count == int.MinValue)
                return false;

            try
            {
                Count = checked(Count - this.increment);
            }
            catch (System.OverflowException)
            {
                Count = int.MinValue;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Count = initialValue;
        }

        /// <summary>
        /// Sets the count.
        /// </summary>
        public void SetCount(int value)
        {
            if (Count != value)
                Count = value;
        }
    }

    public interface ICounter
    {
        int Count { get; }

        bool Increment();
        bool Decrement();
        void Reset();
        void SetCount(int value);
    }
}