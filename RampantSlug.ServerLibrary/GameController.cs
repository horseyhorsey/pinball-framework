﻿using Caliburn.Micro;
using RampantSlug.Common.Devices;
using RampantSlug.ServerLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RampantSlug.Common;
using RampantSlug.Common.Commands;
using RampantSlug.Common.Logging;
using RampantSlug.ServerLibrary.Events;
using RampantSlug.ServerLibrary.Hardware;
using RampantSlug.ServerLibrary.Hardware.Arduino;
using RampantSlug.ServerLibrary.Hardware.Proc;
using RampantSlug.ServerLibrary.Logging;
using RampantSlug.ServerLibrary.Modes;
using RampantSlug.ServerLibrary.ServerDisplays;

namespace RampantSlug.ServerLibrary
{
    public class GameController : IGameController,          
        IHandle<RequestConfigEvent>,
        IHandle<RestartServerEvent>,
        IHandle<ConfigureMachineEvent>,
        IHandle<StartupCompleteEvent>,
 
        // Configure devices
        IHandle<ConfigureSwitchEvent>,
        IHandle<ConfigureCoilEvent>,
        IHandle<ConfigureStepperMotorEvent>,
        IHandle<ConfigureServoEvent>,
        IHandle<ConfigureLedEvent>,

        // Client command requests
        IHandle<SwitchCommandResult>,
        IHandle<CoilCommandResult>,
        IHandle<StepperMotorCommandResult>,
        IHandle<ServoCommandResult>,
        IHandle<LedCommandResult>,

        // Device State Changes
        IHandle<UpdateSwitchEvent>,
        IHandle<UpdateCoilEvent>,
        IHandle<UpdateStepperMotorEvent>,
        IHandle<UpdateServoEvent>,
        IHandle<UpdateLedEvent>
    {
        // Services used by GameController  
        private IEventAggregator _eventAggregator;
        public IServerBusController ServerBusController { get; private set; }


        protected ModeQueue _modes;

        // Hardware Controllers
        private IProcController _procController;
        private IArduinoController _arduinoController;


        // Devices used by Pinball Hardware
        private AttrCollection<ushort, string, Switch> _switches;
        private AttrCollection<ushort, string, Coil> _coils;
        private AttrCollection<ushort, string, StepperMotor> _stepperMotors;
        private AttrCollection<ushort, string, Servo> _servos;
        private AttrCollection<ushort, string, Led> _leds;


        // Display Elements
        public IDisplayBackgroundVideo BackgroundVideo { get; private set; }

        public IDisplayMainScore MainScore { get; private set; }


        public AttrCollection<ushort, string, Switch> Switches
        {
            get { return _switches; }
            set { _switches = value; }
        }

        public AttrCollection<ushort, string, Coil> Coils
        {
            get { return _coils; }
            set { _coils = value; }
        }

        public AttrCollection<ushort, string, StepperMotor> StepperMotors
        {
            get { return _stepperMotors; }
            set { _stepperMotors = value; }
        }

        public AttrCollection<ushort, string, Servo> Servos
        {
            get { return _servos; }
            set { _servos = value; }
        }

        public AttrCollection<ushort, string, Led> Leds
        {
            get { return _leds; }
            set { _leds = value; }
        }

        /// <summary>
        /// The current list of modes that are active in the game
        /// </summary>
        public ModeQueue Modes
        {
            get { return _modes; }
            set { _modes = value; }
        }

        public string ServerName { get; set; }

        public string ServerIcon { get; set; }

        public bool UseHardware { get; set; }


        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public GameController() 
        {
            _modes = new ModeQueue(this);
        }

        #endregion




        #region Setup / Teardown

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Configure(bool isRestart = false)
        {
            if (!isRestart)
            {
                ServerBusController = IoC.Get<IServerBusController>();
                ServerBusController.Start();
            }
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            _procController = IoC.Get<IProcController>();
            _arduinoController = IoC.Get<IArduinoController>();

            BackgroundVideo = IoC.Get<IDisplayBackgroundVideo>();
            MainScore = IoC.Get<IDisplayMainScore>();

            try
            {
                // Retrieve saved configuration information
                var filePath = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
                var gameConfiguration = Configuration.FromFile(filePath + @"\Configuration\machine.json");

                // Update local information
                _switches = DeviceCollection<Switch>.CreateCollection(gameConfiguration.Switches, RsLogManager.GetCurrent);
                _coils = DeviceCollection<Coil>.CreateCollection(gameConfiguration.Coils, RsLogManager.GetCurrent);
                _stepperMotors = DeviceCollection<StepperMotor>.CreateCollection(gameConfiguration.StepperMotors, RsLogManager.GetCurrent);
                _servos = DeviceCollection<Servo>.CreateCollection(gameConfiguration.Servos, RsLogManager.GetCurrent);
                _leds = DeviceCollection<Led>.CreateCollection(gameConfiguration.Leds, RsLogManager.GetCurrent);

                ServerName = gameConfiguration.ServerName;
                ServerIcon = gameConfiguration.ServerIcon;
                UseHardware = gameConfiguration.UseHardware;

                return true;
            }
            catch (Exception ex)
            {
                RsLogManager.GetCurrent.LogTestMessage("Error Processing Configuration " + ex.Message);
                return false;
            }
        }

        public void ConnectToHardware()
        {
            if (UseHardware)
            {
                _procController.Setup();
                _arduinoController.Setup();
            }
            else
            {
                RsLogManager.GetCurrent.LogTestMessage("Server not using hardware. Simulation only.");
            }
        }

        public void DisconnectFromHardware()
        {
            if(_procController != null)
                _procController.Close();

            if (_arduinoController != null)
                _arduinoController.Close();
        }

        public void Handle(StartupCompleteEvent message)
        {
            Attract = new Attract(this);
            BaseGameMode = new BaseGame(this);
            string[] troughSwitchnames = new string[5] { "trough1", "trough2", "trough3", "trough4", "trough5" };
            BallTrough = new BallTrough(this,
                                troughSwitchnames,
                                "trough5",
                                "trough",
                                new string[] { "leftOutlane", "rightOutlane" },
                                "shooterLane");
            _modes.Add(Attract);
            _modes.Add(BallTrough);
        }

        #endregion

        

        #region Respond to Commands sent from Client to control/fake hardware

        public void Handle(SwitchCommandResult message)
        {
            var updatedSwitch = message.Device;

            if (message.Command == SwitchCommand.PressActive)
                {
                    updatedSwitch.State = string.Equals(updatedSwitch.State, "Open") ? "Closed" : "Open";
                    _eventAggregator.PublishOnUIThread(new UpdateSwitchEvent() { Device = updatedSwitch });

                    updatedSwitch.State = string.Equals(updatedSwitch.State, "Open") ? "Closed" : "Open";

                    TimedAction.ExecuteWithDelay(new System.Action(delegate
                    {
                        _eventAggregator.PublishOnUIThread(new UpdateSwitchEvent()
                        {
                            Device = updatedSwitch
                        });
                    }), TimeSpan.FromSeconds(0.5));
                    
                }
                else if (message.Command == SwitchCommand.HoldActive)
                {
                    updatedSwitch.State = string.Equals(updatedSwitch.State, "Open") ? "Closed" : "Open";
                    _eventAggregator.PublishOnUIThread(new UpdateSwitchEvent() { Device = updatedSwitch });

                }
        }

        public void Handle(CoilCommandResult message)
        {
            var updatedCoil = message.Device;

            if (message.Command == CoilCommand.PulseActive)
            {
                RsLogManager.GetCurrent.LogTestMessage("Command to pulse Coil: " + message.Device.Name + " to " + message.Command.ToString());
                
                updatedCoil.State = "Pulsing";

                _eventAggregator.PublishOnUIThread(new UpdateCoilEvent() { Device = updatedCoil });

                updatedCoil.State = "Inactive";

                TimedAction.ExecuteWithDelay(new System.Action(delegate
                {
                    _eventAggregator.PublishOnUIThread(new UpdateCoilEvent()
                    {
                        Device = updatedCoil
                    });
                }), TimeSpan.FromSeconds(0.5));
            }  
        }

        public void Handle(StepperMotorCommandResult message)
        {
            var updatedStepperMotor = message.Device;

            if (message.Command == StepperMotorCommand.ToClockwiseLimit)
            {
                // TODO: Look up name of position for clockwise limit for this device
                updatedStepperMotor.State = "ClockwiseLimit";

                _eventAggregator.PublishOnUIThread(new UpdateStepperMotorEvent() { Device = updatedStepperMotor });
            }
            else if (message.Command == StepperMotorCommand.ToCounterClockwiseLimit)
            {
                updatedStepperMotor.State = "CounterClockwiseLimit";
                _eventAggregator.PublishOnUIThread(new UpdateStepperMotorEvent() { Device = updatedStepperMotor });
            }

            RsLogManager.GetCurrent.LogTestMessage("Command to rotate Stepper Motor: " + message.Device.Name + " to " + message.Command.ToString());

           

            // Set the device into the desired state
            // message.Device


            // If appropriate hardware is connected then also drive the hardware to that state

            // if (_tempArduino == null)
            //  {
            //     _tempArduino = new ArduinoDevice();
            //  }
            // RsLogManager.GetCurrent.LogTestMessage("Received device command request from client: " + message.TempControllerMessage);
            // _tempArduino.SendRequestToArduinoBoard(message.TempControllerMessage);

            //MainScore.PlayerScore += 10;
        }

        public void Handle(ServoCommandResult message)
        {
            var updatedServo = message.Device;

            if (message.Command == ServoCommand.ToClockwiseLimit)
            {
                // TODO: Look up name of position for clockwise limit for this device
                updatedServo.State = "ClockwiseLimit";

                _eventAggregator.PublishOnUIThread(new UpdateServoEvent() { Device = updatedServo });
            }
            else if (message.Command == ServoCommand.ToCounterClockwiseLimit)
            {
                updatedServo.State = "CounterClockwiseLimit";
                _eventAggregator.PublishOnUIThread(new UpdateServoEvent() { Device = updatedServo });
            }

            RsLogManager.GetCurrent.LogTestMessage("Command to rotate Servo: " + message.Device.Name + " to " + message.Command.ToString());

        }

        public void Handle(LedCommandResult message)
        {
            var updatedLed = message.Device;

            if (message.Command == LedCommand.MidIntesityOn)
            {
                updatedLed.State = "MidIntesityOn";

                _eventAggregator.PublishOnUIThread(new UpdateLedEvent() { Device = updatedLed });
            }
            else if (message.Command == LedCommand.FullOff)
            {
                updatedLed.State = "Off";
                _eventAggregator.PublishOnUIThread(new UpdateLedEvent() { Device = updatedLed });
            }

            RsLogManager.GetCurrent.LogTestMessage("Command on Led : " + message.Device.Name + " is changing to " + message.Command.ToString());
        }

        #endregion

        #region Respond to Events from Hardware Devices

        public void Handle(UpdateSwitchEvent message)
        {
            // Update local state of switch...
            var sw = message.Device;
            if (sw != null)
            {
                _switches.Update(sw.Number, sw);

                // Update score
                // TODO: Clean this up and come up with a better solution
                //_eventAggregator.PublishOnUIThread(new UpdateDisplayEvent{PlayerScore = 10});
                MainScore.PlayerScore += 10;

                //TODO: ... Temp Event ..
                var evt = new Event()
                {
                    Time = 5,
                    Type = EventType.SwitchClosedDebounced,
                    Value = sw.Number
                };

                _modes.handle_event(evt);

                ServerBusController.SendUpdateDeviceMessage(sw);
            }
        }

        public void Handle(UpdateCoilEvent message)
        {
            // Update local state of coil...
            var coil = message.Device;
            if (coil != null)
            {
                _coils.Update(coil.Number, coil);

                // Notify Client if listening
                ServerBusController.SendUpdateDeviceMessage(coil);
            }
        }

        public void Handle(UpdateStepperMotorEvent message)
        {
            // Update local state of stepper motor...
            var stepperMotor = message.Device;
            if (stepperMotor != null)
            {
                _stepperMotors.Update(stepperMotor.Number, stepperMotor);

                // Notify Client if listening
                ServerBusController.SendUpdateDeviceMessage(stepperMotor);
            }
        }

        public void Handle(UpdateServoEvent message)
        {
            // Update local state of servo...
            var servo = message.Device;
            if (servo != null)
            {
                _servos.Update(servo.Number, servo);

                // Notify Client if listening
                ServerBusController.SendUpdateDeviceMessage(servo);
            }
        }

        public void Handle(UpdateLedEvent message)
        {
            // Update local state of led...
            var led = message.Device;
            if (led != null)
            {
                _leds.Update(led.Number, led);

                // Notify Client if listening
                ServerBusController.SendUpdateDeviceMessage(led);
            }
        }


        #endregion



        public void Handle(RestartServerEvent message)
        {
            DisconnectFromHardware();
            _eventAggregator = null;
            _procController = null;
            _arduinoController = null;
            BackgroundVideo = null;
            MainScore = null;

            if (Configure(true))
            {
                ConnectToHardware();
                _eventAggregator.PublishOnUIThread(new ServerRestartedEvent());
            }
        }

        #region Configuration Related Methods

        public void Handle(RequestConfigEvent message)
        {
            var config = PopulateConfiguration();
            ServerBusController.SendConfigurationMessage(config);
        }

        public void Handle(ConfigureMachineEvent message)
        {
            UseHardware = message.UseHardware;
        }

        public void Handle(ConfigureSwitchEvent message)
        {
            UpdateConfig(message.Device, message.RemoveDevice);
            SaveConfigurationToFile();

            // Update Client with changes
            var config = PopulateConfiguration();
            ServerBusController.SendConfigurationMessage(config);
        }

        public void Handle(ConfigureCoilEvent message)
        {
            UpdateConfig(message.Device, message.RemoveDevice);
            SaveConfigurationToFile();

            // Update Client with changes
            var config = PopulateConfiguration();
            ServerBusController.SendConfigurationMessage(config);
        }

        public void Handle(ConfigureStepperMotorEvent message)
        {
            UpdateConfig(message.Device, message.RemoveDevice);
            SaveConfigurationToFile();

            // Update Client with changes
            var config = PopulateConfiguration();
            ServerBusController.SendConfigurationMessage(config);
        }

        public void Handle(ConfigureServoEvent message)
        {
            UpdateConfig(message.Device, message.RemoveDevice);
            SaveConfigurationToFile();

            // Update Client with changes
            var config = PopulateConfiguration();
            ServerBusController.SendConfigurationMessage(config);
        }

        public void Handle(ConfigureLedEvent message)
        {
            UpdateConfig(message.Device, message.RemoveDevice);
            SaveConfigurationToFile();

            // Update Client with changes
            var config = PopulateConfiguration();
            ServerBusController.SendConfigurationMessage(config);
        }

      

        private void UpdateConfig(Switch updatedSwitch, bool removeDevice)
        {
            if (updatedSwitch.Number == 0)
            {
                // Something has gone wrong. Should have generated a Number based on Address
                RsLogManager.GetCurrent.LogTestMessage("Invalid switch settings. Not saving to config.");
                return;
            }

            // Update or remove existing Switch
            if (Switches.ContainsKey(updatedSwitch.Number))
            {
                if (removeDevice)
                {
                    Switches.Remove(updatedSwitch.Number);
                    RsLogManager.GetCurrent.LogTestMessage("Removed switch " + updatedSwitch.Name + " from config.");
                }
                else
                {
                    Switches.Update(updatedSwitch.Number, updatedSwitch);
                    RsLogManager.GetCurrent.LogTestMessage("Updated switch " + updatedSwitch.Name + "in config.");
                }
            }
            else // Adding a new switch
            {
                Switches.Add(updatedSwitch.Number, updatedSwitch.Name, updatedSwitch);
                RsLogManager.GetCurrent.LogTestMessage("Added switch " + updatedSwitch.Name + "to config.");
            }
        }

        private void UpdateConfig(Coil updatedCoil, bool removeDevice)
        {
            if (updatedCoil.Number == 0)
            {
                // Something has gone wrong. Should have generated a Number based on Address
                RsLogManager.GetCurrent.LogTestMessage("Invalid coil settings. Not saving to config.");
                return;
            }

            // Update or remove existing coil
            if (Coils.ContainsKey(updatedCoil.Number))
            {
                if (removeDevice)
                {
                    Coils.Remove(updatedCoil.Number);
                    RsLogManager.GetCurrent.LogTestMessage("Removed coil " + updatedCoil.Name + " from config.");
                }
                else
                {
                    Coils.Update(updatedCoil.Number, updatedCoil);
                    RsLogManager.GetCurrent.LogTestMessage("Updated coil " + updatedCoil.Name + "in config.");
                }
            }
            else // Adding a new coil
            {
                Coils.Add(updatedCoil.Number, updatedCoil.Name, updatedCoil);
                RsLogManager.GetCurrent.LogTestMessage("Added coil " + updatedCoil.Name + "to config.");
            }
        }

        private void UpdateConfig(StepperMotor updatedStepperMotor, bool removeDevice)
        {
            if (updatedStepperMotor.Number == 0)
            {
                // Something has gone wrong. Should have generated a Number based on Address
                RsLogManager.GetCurrent.LogTestMessage("Invalid stepper motor settings. Not saving to config.");
                return;
            }

            // Update or remove existing stepperMotor
            if (StepperMotors.ContainsKey(updatedStepperMotor.Number))
            {
                if (removeDevice)
                {
                    StepperMotors.Remove(updatedStepperMotor.Number);
                    RsLogManager.GetCurrent.LogTestMessage("Removed stepper motor " + updatedStepperMotor.Name + " from config.");
                }
                else
                {
                    StepperMotors.Update(updatedStepperMotor.Number, updatedStepperMotor);
                    RsLogManager.GetCurrent.LogTestMessage("Updated stepper motor " + updatedStepperMotor.Name +
                                                           "in config.");
                }
            }
            else // Adding a new stepperMotor
            {
                StepperMotors.Add(updatedStepperMotor.Number, updatedStepperMotor.Name, updatedStepperMotor);
                RsLogManager.GetCurrent.LogTestMessage("Added stepper motor " + updatedStepperMotor.Name + "to config.");
            }
        }

        private void UpdateConfig(Servo updatedServo, bool removeDevice)
        {
            if (updatedServo.Number == 0)
            {
                // Something has gone wrong. Should have generated a Number based on Address
                RsLogManager.GetCurrent.LogTestMessage("Invalid servo settings. Not saving to config.");
                return;
            }

            // Update or remove existing servo
            if (Servos.ContainsKey(updatedServo.Number))
            {
                if (removeDevice)
                {
                    Servos.Remove(updatedServo.Number);
                    RsLogManager.GetCurrent.LogTestMessage("Removed servo " + updatedServo.Name + " from config.");
                }
                else
                {
                    Servos.Update(updatedServo.Number, updatedServo);
                    RsLogManager.GetCurrent.LogTestMessage("Updated servo " + updatedServo.Name + " in config.");
                }
            }
            else // Adding a new servo
            {
                Servos.Add(updatedServo.Number, updatedServo.Name, updatedServo);
                RsLogManager.GetCurrent.LogTestMessage("Added servo " + updatedServo.Name + " to config.");
            }
        }

        private void UpdateConfig(Led updatedLed, bool removeDevice)
        {
            if (updatedLed.Number == 0)
            {
                // Something has gone wrong. Should have generated a Number based on Address
                RsLogManager.GetCurrent.LogTestMessage("Invalid led settings. Not saving to config.");
                return;
            }

            // Update or remove existing led
            if (Leds.ContainsKey(updatedLed.Number))
            {
                if (removeDevice)
                {
                    Leds.Remove(updatedLed.Number);
                    RsLogManager.GetCurrent.LogTestMessage("Removed led " + updatedLed.Name + " from config.");
                }
                else
                {
                    Leds.Update(updatedLed.Number, updatedLed);
                    RsLogManager.GetCurrent.LogTestMessage("Updated led " + updatedLed.Name + "in config.");
                }
            }
            else // Adding a new led
            {
                Leds.Add(updatedLed.Number, updatedLed.Name, updatedLed);
                RsLogManager.GetCurrent.LogTestMessage("Added led " + updatedLed.Name + "to config.");
            }
        }


        public void SaveConfigurationToFile()
        {
            var filePath = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
            var config = PopulateConfiguration();
            config.ToFile(filePath + @"\testoutput.json");
        }


        private Configuration PopulateConfiguration()
        {
            var gameConfiguration = new Configuration();
            gameConfiguration.ImageSerialize();

            gameConfiguration.ServerName = ServerName;
            gameConfiguration.ServerIcon = ServerIcon;
            gameConfiguration.UseHardware = UseHardware;
            
            
            gameConfiguration.Switches = Switches.Values;
            gameConfiguration.Coils = Coils.Values;
            gameConfiguration.StepperMotors = StepperMotors.Values;
            gameConfiguration.Servos = Servos.Values;
            gameConfiguration.Leds = Leds.Values;

            return gameConfiguration;
        }

        #endregion


        #region Methods carried over from NetProcGame. May require rework

        public Attract Attract;
        public BaseGame BaseGameMode;
        public BallTrough BallTrough;
        private List<Player> _players;
        private double _ballEndTime;
        private double _ballStartTime;
        private int _currentPlayerIndex;
        private int _ball;
        private int _ballsPerGame;
        private List<Player> _oldPlayers;


        /// <summary>
        /// The ball time for the current player
        /// </summary>
        /// <returns>The ball time (in seconds) that the current ball has been in play</returns>
        public double GetBallTime()
        {
            return this._ballEndTime - this._ballStartTime;
        }

        /// <summary>
        /// The game time for the given player index
        /// </summary>
        /// <param name="player">The player index to calculate the game time for</param>
        /// <returns>The time in seconds the player has been playing the entire game</returns>
        public double GetGameTime(int player)
        {
            return this._players[player].GameTime;
        }

        /// <summary>
        /// Save the ball start time into local memory
        /// </summary>
        public void SaveBallStartTime()
        {
            this._ballStartTime = Time.GetTime();
        }

        /// <summary>
        /// Called by the implementor to notify the game that the first ball should be started.
        /// </summary>
        public void StartBall()
        {
            this.BallStarting();
        }

        /// <summary>
        /// Called by the game framework when a new ball is starting
        /// </summary>
        public virtual void BallStarting()
        {
            this.SaveBallStartTime();
            _modes.Add(BaseGameMode);
        }

        /// <summary>
        /// Called by the game framework when a new ball is starting which was the result of a stored extra ball.
        /// The default implementation calls ball_starting() which is not called by the framework in this case.
        /// </summary>
        public virtual void ShootAgain()
        {
            this.BallStarting();
        }

        /// <summary>
        /// Called by the game framework when the current ball has ended
        /// </summary>
        public virtual void BallEnded()
        {
            _modes.Remove(BaseGameMode);
        }

        /// <summary>
        /// Called by the implementor to notify the game that the current ball has ended
        /// </summary>
        public void EndBall()
        {
            this._ballEndTime = Time.GetTime();
            this.CurrentPlayer().GameTime += this.GetBallTime();
            this.BallEnded();

            if (this.CurrentPlayer().ExtraBalls > 0)
            {
                this.CurrentPlayer().ExtraBalls -= 1;
                this.ShootAgain();
                return;
            }

            if (this._currentPlayerIndex + 1 == this._players.Count)
            {
                this._ball += 1;
                this._currentPlayerIndex = 0;
            }
            else
            {
                this._currentPlayerIndex += 1;
            }

            if (this._ball > this._ballsPerGame)
            {
                this.EndGame();
            }
            else
            {
                this.StartBall();
            }

        }

        /// <summary>
        /// Called by the GameController when a new game is starting.
        /// </summary>
        public virtual void GameStarted()
        {
            this._ball = 1;
            this._players = new List<Player>();
            this._currentPlayerIndex = 0;
        }

        /// <summary>
        /// Called by the implementor to notify the game that the game has started.
        /// </summary>
        public virtual void StartGame()
        {
            this.GameStarted();
        }

        /// <summary>
        /// Called by the GameController when the current game has ended
        /// </summary>
        public virtual void GameEnded()
        {
            _modes.Add(Attract);
        }

        /// <summary>
        /// Called by the implementor to notify the game that the game as ended
        /// </summary>
        public void EndGame()
        {
            this.GameEnded();
            this._ball = 0;
        }

        /// <summary>
        /// Reset the game state to normal (like a slam tilt)
        /// </summary>
        public virtual void Reset()
        {
            _ball = 0;
            _oldPlayers.Clear();
            _oldPlayers.AddRange(_players);
            _players.Clear();
            _currentPlayerIndex = 0;
            _modes.Clear();
        }

        /// <summary>
        /// Creates a new player with a given name
        /// </summary>
        /// <param name="name">The name for the player to use, usually auto generated</param>
        /// <returns>A new player object</returns>
        public Player CreatePlayer(string name)
        {
            return new Player(name);
        }

        /// <summary>
        /// Adds a new player to 'Players' and auto-assigns a name
        /// </summary>
        /// <returns></returns>
        public virtual Player AddPlayer()
        {
            Player newPlayer = this.CreatePlayer("Player " + (_players.Count + 1).ToString());
            _players.Add(newPlayer);
            return newPlayer;
        }

        /// <summary>
        /// Returns the current 'Player' object according to the current_player_index value
        /// </summary>
        /// <returns></returns>
        public Player CurrentPlayer()
        {
            if (this._players.Count > this._currentPlayerIndex)
                return this._players[this._currentPlayerIndex];
            else
                return null;
        }


        #endregion

    }
}
