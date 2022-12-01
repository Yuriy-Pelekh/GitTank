using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTank.Loggers
{
    internal class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) {
                return;
            }

            var logEventProperty = propertyFactory.CreateProperty("EscapedException", logEvent.MessageTemplate.Text.Replace("\r\n", ""));
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}
