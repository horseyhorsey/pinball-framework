using System;
using System.ComponentModel;
using Caliburn.Micro;
using RampantSlug.Common.Devices;
using RampantSlug.ServerLibrary;
using RampantSlug.ServerLibrary.Events;
using RampantSlug.ServerLibrary.Hardware;
using RampantSlug.ServerLibrary.ServerDisplays;

namespace RampantSlug.PinballServerDemo
{
    public class ShellViewModel : Conductor<IScreen>.Collection.AllActive, IShell, IHandle<UpdateDisplayEvent>
    {
        private IGameController _gameController;
        private IEventAggregator _eventAggregator;
     
        public IDisplayBackgroundVideo BackgroundVideo { get; private set; }
        public IDisplayMainScore MainScore { get; private set; }

        public ShellViewModel() 
        {

            
        }

        public void Exit() 
        { 
            _gameController.ServerBusController.Stop(); 
        }


        public void SaveConfig() 
        {
            _gameController.SaveConfigurationToFile();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            _gameController = IoC.Get<IGameController>();
            if (_gameController.Configure())
            {
                BackgroundVideo = _gameController.BackgroundVideo;
                MainScore = _gameController.MainScore;
                NotifyOfPropertyChange(()=>BackgroundVideo);
                NotifyOfPropertyChange(() => MainScore);

                _gameController.ConnectToHardware();
            }

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            _gameController.DisconnectFromHardware();

            Exit();
            base.OnDeactivate(close);
        }

        public void Handle(UpdateDisplayEvent message)
        {
          //  PlayerScore += message.PlayerScore;
        }
    }
}