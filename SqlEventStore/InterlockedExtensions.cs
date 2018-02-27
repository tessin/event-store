using System.Collections.Generic;
using System.Threading;

namespace EventCore.EventModel
{
    static class InterlockedExtensions
    {
        public static void CompareExchangeAdd<TKey, TValue>(ref Dictionary<TKey, TValue> d, TKey k, TValue v)
        {
            // while this code copies the entire contents of the dictionary
            // on every add operation, it is surprisingly efficent when the
            // dictionary is expected to eventually reach a read only state

            // total cost of copying 1000s of entires 1000s of times is 
            // still less than 10 ms (in total)
            for (; ; )
            {
                var d2 = d; // make local variable
                var d3 = new Dictionary<TKey, TValue>(d2, d2.Comparer); // copy
                d3.Add(k, v); // mutate copy
                if (Interlocked.CompareExchange(ref d, d3, d2) == d2) // atempt atomic update
                {
                    // success
                    break;
                }
                // data race detected! (retry)
            }
        }
    }
}
