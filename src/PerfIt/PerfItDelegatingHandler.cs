using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace PerfIt
{
    public class PerfItDelegatingHandler : DelegatingHandler
    {
        private PerfItManager perftIfManager;
        public PerfItDelegatingHandler(string categoryName)
        {
            if (null == categoryName) throw new ArgumentNullException("categoryName");

            this.perftIfManager = new PerfItManager(categoryName);

            InstanceNameProvider = request =>
                string.Format("{0}_{1}", request.Method.Method.ToLower(), request.RequestUri.Host.ToLower());
        }

        /// <summary>
        /// Provides the performance counter instance name.
        /// Default impl combines method and the host name of the request.
        /// </summary>
        public Func<HttpRequestMessage, string> InstanceNameProvider { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {

            if (!this.perftIfManager.PublishCounters)
                return base.SendAsync(request, cancellationToken);

            var instanceName = InstanceNameProvider(request);

            var perfItContext = perftIfManager.StartCounterRecording(instanceName);
            
            return base.SendAsync(request, cancellationToken)
                .Then((response) =>
                    {
                        perftIfManager.StopCounterRecording(perfItContext);
                            
                        return response;

                    }, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                perftIfManager.Dispose(disposing);
            }
        }
    }
}
