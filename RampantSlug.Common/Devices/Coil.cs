﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RampantSlug.Common.Devices
{
    public class Coil: Device, IDevice
    {

        public Coil()
        {
            State = "Inactive";      
        }

   //     public override void UpdateNumberFromAddress()
   //     {
   //         Number = ushort.Parse(Address);
   //     }

        public override bool IsActive
        {
            get
            {
                return false;
            }
        }
    }
}
