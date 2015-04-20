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
using RampantSlug.Common.Commands;
using RampantSlug.PinballClient.CommonViewModels;

namespace RampantSlug.PinballClient.ClientDisplays.DeviceInformation
{

    public class ServoConfigurationViewModel : Screen, IDeviceConfigurationScreen
    {
        #region Fields

        private ServoViewModel _servo;

        #endregion

        #region Properties

        public ServoViewModel Coil
        {
            get { return _servo; }
            set
            {
                _servo = value;
                NotifyOfPropertyChange(() => Coil);
            }
        }

        public string Address
        {
            get { return _servo.Address; }
            set
            {
                _servo.Address = value;
                NotifyOfPropertyChange(() => Address);
            }
        }


        public ObservableCollection<HistoryRowViewModel> PreviousStates
        {
            get
            {
                return _servo.PreviousStates;
            }

        }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servo"></param>
        public ServoConfigurationViewModel(ServoViewModel servo) 
        {
            _servo = servo;
        }

        #endregion

        public void PulseState()
        {
            var busController = IoC.Get<IClientBusController>();
            busController.SendCommandDeviceMessage(_servo.Device as Servo, ServoCommand.PulseActive);
        }

        public void HoldState()
        {
            var busController = IoC.Get<IClientBusController>();
            busController.SendCommandDeviceMessage(_servo.Device as Servo, ServoCommand.HoldActive);
        }

        public void SaveDevice()
        {
            var busController = IoC.Get<IClientBusController>();
            busController.SendConfigureDeviceMessage(_servo.Device as Servo); 
        }
 
    }

}
