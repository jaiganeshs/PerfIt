using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Http;
using PerfIt.Handlers;

namespace PerfIt
{
    public static class PerfItRuntime
    {
        static PerfItRuntime()
        {

            HandlerFactories = new Dictionary<string, Func<string, string, ICounterHandler>>();

            HandlerFactories.Add(CounterTypes.TotalNoOfOperations, 
                (categoryName, instanceName) => new TotalCountHandler(categoryName, instanceName));

            HandlerFactories.Add(CounterTypes.AverageTimeTaken,
                (categoryName, instanceName) => new AverageTimeHandler(categoryName, instanceName));

            HandlerFactories.Add(CounterTypes.LastOperationExecutionTime,
                (categoryName, instanceName) => new LastOperationExecutionTimeHandler(categoryName, instanceName));

            HandlerFactories.Add(CounterTypes.NumberOfOperationsPerSecond,
                (categoryName, instanceName) => new NumberOfOperationsPerSecondHandler(categoryName, instanceName));

        }

        /// <summary>
        /// Counter handler factories with counter type as the key.
        /// Factory's first param is applicationName and second is the filter
        /// Use it to register your own counters or replace built-in implementations
        /// </summary>
        public static Dictionary<string, Func<string, string, ICounterHandler>> HandlerFactories { get; private set; }
    
        internal static string GetUniqueName(string instanceName, string counterType)
        {
            return string.Format("{0}.{1}", instanceName, counterType);
        }

        internal static string GetCounterInstanceName(Type controllerType, string actionName)
        {
            return string.Format("{0}.{1}", controllerType.Name, actionName);
        }
    }
}
