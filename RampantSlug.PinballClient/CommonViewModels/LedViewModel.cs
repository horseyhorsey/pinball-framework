﻿using System;
using System.Linq;
using Caliburn.Micro;
using RampantSlug.Common.Commands;
using RampantSlug.Common.Devices;
using RampantSlug.PinballClient.Events;

namespace RampantSlug.PinballClient.CommonViewModels
{
    public class LedViewModel : DeviceViewModel
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

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ledDevice"></param>
        public LedViewModel(Led ledDevice)
        {
            _device = ledDevice;
        }

        #endregion

        #region Device Command Methods

        public void ActivateDeviceState()
        {
            var busController = IoC.Get<IClientBusController>();
            var led = Device as Led;
            busController.SendCommandDeviceMessage(led, LedCommand.MidIntesityOn);
        }

        #endregion

        public override void ConfigureDevice()
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnUIThread(new ShowLedConfig() { LedVm = this });
        }

        public override void HighlightSelected()
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnUIThread(new HighlightLed() { LedVm = this });
        }

        public void UpdateDeviceInfo(Led ledDevice, DateTime timestamp)
        {
            // TODO: Move this into DeviceViewModel section
            if (PreviousStates.Count > 10)
            {
                PreviousStates.Remove(PreviousStates.Last());
            }
            PreviousStates.Insert(0, new HistoryRowViewModel()
            {
                Timestamp = timestamp.ToString(),
                State = "No led states exist yet."
            });

            // Update stuff.
            _device = ledDevice;
            NotifyOfPropertyChange(() => IsDeviceActive);

        }
    }
}
