﻿using Caliburn.Micro;
using RampantSlug.Common.Devices;
using RampantSlug.PinballClient.ContractImplementations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RampantSlug.Common.Commands;
using RampantSlug.PinballClient.CommonViewModels;

namespace RampantSlug.PinballClient.ClientDisplays.DeviceInformation
{

    public class DynamicWireIconViewModel : Screen
    {
        #region Fields

        private SolidColorBrush _primaryWireColor;
        private SolidColorBrush _secondaryWireColor;

        #endregion

        #region Properties

        public SolidColorBrush PrimaryWireColor
        {
            get { return _primaryWireColor; }
            set
            {
                _primaryWireColor = value;
                NotifyOfPropertyChange(() => PrimaryWireColor);
            }
        }

        public SolidColorBrush SecondaryWireColor
        {
            get { return _secondaryWireColor; }
            set
            {
                _secondaryWireColor = value;
                NotifyOfPropertyChange(() => SecondaryWireColor);
            }
        }
        

        #endregion

        #region Constructor
        

        public DynamicWireIconViewModel()
        {

        }

        #endregion

       
    }
}
