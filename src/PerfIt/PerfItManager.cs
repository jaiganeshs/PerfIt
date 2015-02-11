using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PerfIt
{
    public class PerfItManager
    {
        private ConcurrentDictionary<string, Lazy<PerfItCounterContext>> _counterContexts =
          new ConcurrentDictionary<string, Lazy<PerfItCounterContext>>();

        private string _categoryName;

        public PerfItManager(string categoryName)
        {
            _categoryName = categoryName;
            PublishCounters = true;
            RaisePublishErrors = true;

            SetErrorPolicy();
            SetPublish();
        }

        public bool PublishCounters { get; set; }

        public bool RaisePublishErrors { get; set; }

        private string GetKey(string counterName, string instanceName)
        {
            return string.Format("{0}_{1}", counterName, instanceName);
        }

        public PerfItContext StartCounterRecording(string instanceName)
        {
            var contexts = new List<PerfItCounterContext>();
            var perfItContext = new PerfItContext(contexts);

            try
            {
                foreach (var handlerFactory in PerfItRuntime.HandlerFactories)
                {
                    var key = GetKey(handlerFactory.Key, instanceName);
                    var ctx = _counterContexts.GetOrAdd(key, k =>
                        new Lazy<PerfItCounterContext>(() => new PerfItCounterContext()
                        {
                            Handler = handlerFactory.Value(_categoryName, instanceName)
                        }));
                    contexts.Add(ctx.Value);
                }

                foreach (var context in contexts)
                {
                    context.Handler.OnRequestStarting(perfItContext);
                }    
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());

                if (RaisePublishErrors)
                    throw exception;
            }

            return perfItContext;
        }

        public void StopCounterRecording(PerfItContext perfItContext)
        {
            if (null == perfItContext)
            {
                return;
            }

            try
            {

                foreach (var counter in perfItContext.Counters)
                {
                    counter.Handler.OnRequestEnding(perfItContext);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                if (RaisePublishErrors)
                    throw e;
            }
        }
           
        private void SetPublish()
        {
            var value = ConfigurationManager.AppSettings[Constants.PerfItPublishCounters] ?? "true";
            PublishCounters = Convert.ToBoolean(value);
        }

        protected void SetErrorPolicy()
        {
            var value = ConfigurationManager.AppSettings[Constants.PerfItPublishErrors] ?? RaisePublishErrors.ToString();
            RaisePublishErrors = Convert.ToBoolean(value);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var context in _counterContexts.Values)
                {
                    context.Value.Handler.Dispose();
                }
                _counterContexts.Clear();
            }
        }
    }

    public class PerfItCounterContext
    {
        public string Name { get; set; }
        public ICounterHandler Handler { get; set; }
    }
}
