﻿using System;
using System.Linq;
using Caliburn.Micro;
using RampantSlug.Common.Commands;
using RampantSlug.Common.Devices;
using RampantSlug.PinballClient.Events;

namespace RampantSlug.PinballClient.CommonViewModels
{
    public class CoilViewModel : DeviceViewModel
    {

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                HighlightSelected();
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public override string DeviceType
        {
            get { return "Coil"; }
        }


        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coilDevice"></param>
        public CoilViewModel(Coil coilDevice)
        {
            Device = coilDevice;
        }

        #endregion


        #region Device Command Methods

        public void ActivateDeviceState()
        {
            var busController = IoC.Get<IClientBusController>();
            var coil = Device as Coil;
            busController.SendCommandDeviceMessage(coil, CoilCommand.PulseActive);
        }

        #endregion

        public override void ConfigureDevice()
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnUIThread(new ShowCoilConfig() { CoilVm = this });
        }

        public override void HighlightSelected()
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnUIThread(new HighlightCoil() { CoilVm = this });
        }

        public void UpdateDeviceInfo(Coil coilDevice, DateTime timestamp)
        {
            base.UpdateDeviceInfo(coilDevice, timestamp);

            // Update stuff.
            //_device = coilDevice;
            //NotifyOfPropertyChange(() => IsActive);

        }

    }
}
