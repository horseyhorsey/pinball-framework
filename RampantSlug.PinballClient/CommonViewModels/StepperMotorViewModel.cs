﻿using System;
using System.Linq;
using Caliburn.Micro;
using RampantSlug.Common.Commands;
using RampantSlug.Common.Devices;
using RampantSlug.PinballClient.Events;

namespace RampantSlug.PinballClient.CommonViewModels
{
    public class StepperMotorViewModel : DeviceViewModel
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
            get { return "Stepper Motor"; }
        }

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepperMotorDevice"></param>
        public StepperMotorViewModel(StepperMotor stepperMotorDevice)
        {
            Device = stepperMotorDevice;
        }

        #endregion

        #region Device Command Methods

        public void RotateClockwise()
        {
            var busController = IoC.Get<IClientBusController>();
            var stepperMotor = Device as StepperMotor;
            busController.SendCommandDeviceMessage(stepperMotor, StepperMotorCommand.ToClockwiseLimit);
        }

        public void RotateCounterClockwise()
        {
            var busController = IoC.Get<IClientBusController>();
            var stepperMotor = Device as StepperMotor;
            busController.SendCommandDeviceMessage(stepperMotor, StepperMotorCommand.ToCounterClockwiseLimit);
        }

        #endregion

        public override void ConfigureDevice()
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnUIThread(new ShowStepperMotorConfig() { StepperMotorVm = this });
        }

        public override void HighlightSelected()
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnUIThread(new HighlightStepperMotor() { StepperMotorVm = this });
        }

        public void UpdateDeviceInfo(StepperMotor stepperMotor, DateTime timestamp)
        {
            base.UpdateDeviceInfo(stepperMotor, timestamp);

            // Update stuff.
            //_device = stepperMotor;
            //NotifyOfPropertyChange(() => IsActive);

        }
    }
}
