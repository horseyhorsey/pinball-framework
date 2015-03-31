﻿using Caliburn.Micro;
using MassTransit;
using RampantSlug.Common.Devices;
using RampantSlug.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RampantSlug.PinballClient.ContractImplementations
{
    class DeviceCommandMessage: IDeviceCommandMessage
    {        
        public DateTime Timestamp { get; set; }

        public IDevice Device { get; set; }

        public string TempControllerMessage { get; set; }

    }
}
