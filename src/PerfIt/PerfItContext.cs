using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerfIt
{
    public class PerfItContext
    {

        public PerfItContext(IEnumerable<PerfItCounterContext> counters)
        {
            Data = new Dictionary<string, object>();    
            Counters = counters;
        }

        public Dictionary<string, object> Data { get; private set; }

        public IEnumerable<PerfItCounterContext> Counters { get; set; } 
    }
}
