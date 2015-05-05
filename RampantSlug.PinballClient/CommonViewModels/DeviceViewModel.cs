﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using RampantSlug.Common.Devices;
using RampantSlug.PinballClient.Events;

namespace RampantSlug.PinballClient.CommonViewModels
{
    public class DeviceViewModel : Screen, IDeviceViewModel
    {

        protected IDevice _device;
        private ObservableCollection<HistoryRowViewModel> _previousStates;
        protected bool _isSelected;
        protected bool _isVisible;
        private ObservableCollection<DeviceViewModel> _children;

        #region Config persisted Properties

        public ushort Number
        {
            get
            {
                return _device.Number;
            }
        }

        public string Name
        {
            get
            {
                return Device.Name;
            }
            set
            {
                Device.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string State
        {
            get
            {
                return Device.State;
            }
            set
            {
                Device.State = value;
                NotifyOfPropertyChange(() => State);
            }
        }

        public string Address
        {
            get
            {
                return Device.Address;
            }
            set
            {
                Device.Address = value;
                NotifyOfPropertyChange(() => Address);
            }
        }

        public double VirtualLocationX
        {
            get
            {
                return Device.VirtualLocationX;
            }
            set
            {
                Device.VirtualLocationX = value;
                NotifyOfPropertyChange(() => VirtualLocationX);
            }
        }

        public double VirtualLocationY
        {
            get
            {
                return Device.VirtualLocationY;
            }
            set
            {
                Device.VirtualLocationY = value;
                NotifyOfPropertyChange(() => VirtualLocationY);
            }
        }

        public string RefinedType
        {
            get
            {
                return Device.RefinedType;
            }
            set
            {
                Device.RefinedType = value;
                NotifyOfPropertyChange(() => RefinedType);
            }
        }

        

        #endregion

        #region View support properties

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public ObservableCollection<DeviceViewModel> Children
        {
            get { return _children; }
        }

        #endregion

       

        public bool IsDeviceActive
        {
            get
            {
                return _device.IsActive;
            }
        }

        public IDevice Device
        {
            get
            {
                return _device;
            }
            set
            {
                _device = value;
                NotifyOfPropertyChange(() => Device);

                // If this has been updated then need to refresh all child properties               
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => State);
                NotifyOfPropertyChange(() => IsDeviceActive);
                NotifyOfPropertyChange(() => Address);
                NotifyOfPropertyChange(() => VirtualLocationX);
                NotifyOfPropertyChange(() => VirtualLocationY);

            }
        }

        public virtual string DeviceType {
            get { return "Generic Device"; }
        }


        public ObservableCollection<HistoryRowViewModel> PreviousStates
        {
            get
            {
                return _previousStates;
            }
            private set
            {
                _previousStates = value;
                NotifyOfPropertyChange(() => PreviousStates);
            }
        }


       

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DeviceViewModel()
        {
            _children = new ObservableCollection<DeviceViewModel>();
            _isVisible = true;
            _previousStates = new ObservableCollection<HistoryRowViewModel>();
        }

        #endregion



       

        public void ChangeVisibility()
        {
            IsVisible = !_isVisible;
        }


        public virtual void ConfigureDevice()
        {
        }


        public virtual void HighlightSelected()
        {
            
        }

        public void UpdateDeviceInfo(IDevice device, DateTime timestamp)
        {
            Device = device;
            
            if (PreviousStates.Count > 10)
            {
                PreviousStates.Remove(PreviousStates.Last());
            }
            PreviousStates.Insert(0, new HistoryRowViewModel()
            {
                Timestamp = timestamp.ToString(),
                State = device.State
            });

        }

    }
}

