﻿using RampantSlug.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RampantSlug.Common.Logging;

namespace RampantSlug.ServerLibrary.ContractImplementations
{
    class LogMessage: ILogMessage
    {
        public DateTime Timestamp { get; set; }

        public LogEventType EventType { get; set; }

        public OriginatorType OriginatorType { get; set; }

        public string OriginatorName { get; set; }

        public string Status { get; set; }

        public string Information { get; set; }

    }
}
