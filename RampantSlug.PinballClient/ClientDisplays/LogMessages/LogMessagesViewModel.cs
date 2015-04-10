﻿using Caliburn.Micro;
using RampantSlug.PinballClient.ContractImplementations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RampantSlug.PinballClient.Events;

namespace RampantSlug.PinballClient.ClientDisplays.LogMessages
{
    //[Export(typeof(IClientDisplay))]
    public class LogMessagesViewModel : Screen, IHandle<DisplayMessageResults>, ILogMessages//, IClientDisplay
    {
        private BindableCollection<DisplayMessageResults> _events;

        public string ClientDisplayName { get { return "Log"; } }

        public BindableCollection<DisplayMessageResults> Events
        {
            get
            {
                return _events;
            }
            set
            {
                _events = value;
                NotifyOfPropertyChange(() => Events);
            }
        }

        public LogMessagesViewModel() 
        {
           

            _events = new BindableCollection<DisplayMessageResults>();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.Subscribe(this);
        }


        public void Handle(DisplayMessageResults message)
        {
            while(Events.Count >= 200)
            {
                // Remove from bottom if overflowing
                Events.RemoveAt(Events.Count - 1);
            }

            // Place most recent message at the top
            Events.Insert(0, message);
        }
    }
}
