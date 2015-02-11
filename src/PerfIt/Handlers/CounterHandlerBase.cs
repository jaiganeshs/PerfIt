using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace PerfIt.Handlers
{
    public abstract class CounterHandlerBase : ICounterHandler
    {

        protected string _instanceName;
        protected string _categoryName;
        protected string _uniqueName;

        public CounterHandlerBase( 
            string categoryName,
            string instanceName)
        {
            _categoryName = categoryName;
            _instanceName = instanceName;
            Name = CounterType;

            _uniqueName = PerfItRuntime.GetUniqueName(instanceName, Name);
        }

        public virtual void Dispose()
        {
            
        }

        /// <summary>
        /// type of counter. just a string identifier
        /// </summary>
        public abstract string CounterType { get; }

        /// <summary>
        /// called when request arrives in delegating handler
        /// </summary>
        /// <param name="context"></param>
        protected abstract void StartCounterRecording(PerfItContext context);

        /// <summary>
        /// called as the async continuation on the delegating handler (when response is sent back)
        /// </summary>
        /// <param name="context"></param>
        protected abstract void StopCounterRecording(PerfItContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newInstanceName"></param>
        protected abstract void BuildCounters(bool newInstanceName = false);


        /// <summary>
        /// Handler to return data for creating counters
        /// </summary>
        /// <param name="filter">Filter attribute defined</param>
        /// <returns></returns>
        protected abstract CounterCreationData[] DoGetCreationData();

        public string Name { get; private set; }
        public string UniqueName { get { return _uniqueName; } }

        public string GetUniqueName()
        {
            return _categoryName + _instanceName + Name;
        }

        public CounterCreationData[] BuildCreationData()
        {
            return DoGetCreationData();
        }

        protected string GetInstanceName(bool newName = false)
        {
            return
                _instanceName +
                (newName ? "_" + Guid.NewGuid().ToString("N").Substring(6) : string.Empty);
        }


        public void OnRequestStarting(PerfItContext context)
        {
            if (null == context) return;

            try
            {
                this.StartCounterRecording(context);

            }
            catch (InvalidOperationException exception)
            {

                Trace.TraceError(exception.ToString());
                BuildCounters(true);
                this.StartCounterRecording(context);
            }
        }

        public void OnRequestEnding(PerfItContext context)
        {
            if (null == context) return;

            try
            {
                this.StopCounterRecording(context);

            }
            catch (InvalidOperationException exception)
            {

                Trace.TraceError(exception.ToString());
                BuildCounters(true);
                this.StopCounterRecording(context);
            }
        }
    }
}
