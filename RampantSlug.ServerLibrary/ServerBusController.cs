﻿using MassTransit;
using RampantSlug.Common;
using RampantSlug.Common.Devices;
using RampantSlug.ServerLibrary.ContractImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RampantSlug.Common.Logging;

namespace RampantSlug.ServerLibrary
{
    public class ServerBusController: IServerBusController
    {
        private IServiceBus _bus;


        public void Start() 
        {
            _bus = BusInitializer.CreateBus("TestServer", x => 
            {
                x.Subscribe(subs =>
                {
                    subs.Consumer<ConfigureDeviceMessageConsumer>().Transient();
                    subs.Consumer<RequestConfigMessageConsumer>().Transient();
                    subs.Consumer<CommandDeviceMessageConsumer>().Transient();
                    subs.Consumer<ConfigureMachineMessageConsumer>().Transient();
                    subs.Consumer<RestartServerMessageConsumer>().Transient();
                });
            });
          
        }

        public void SendLogMessage(LogEventType eventType, OriginatorType originator, string originatorName, string status, string information) 
        {
            var message = new LogMessage()
            {
                Timestamp = DateTime.Now,
                EventType = eventType,
                OriginatorType = originator,
                OriginatorName = originatorName,
                Status = status,
                Information = information
            };
            _bus.Publish<LogMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void SendConfigurationMessage(Configuration configuration)
        {
            var message = new ConfigMessage() { MachineConfiguration = configuration, Timestamp = DateTime.Now };
            _bus.Publish<ConfigMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void SendUpdateDeviceMessage(Switch device)
        {
            var message = new UpdateSwitchMessage() { Device = device, Timestamp = DateTime.Now };
            _bus.Publish<UpdateSwitchMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void SendUpdateDeviceMessage(Coil device)
        {
            var message = new UpdateCoilMessage() { Device = device, Timestamp = DateTime.Now };
            _bus.Publish<UpdateCoilMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void SendUpdateDeviceMessage(StepperMotor device)
        {
            var message = new UpdateStepperMotorMessage() { Device = device, Timestamp = DateTime.Now };
            _bus.Publish<UpdateStepperMotorMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void SendUpdateDeviceMessage(Servo device)
        {
            var message = new UpdateServoMessage() { Device = device, Timestamp = DateTime.Now };
            _bus.Publish<UpdateServoMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void SendUpdateDeviceMessage(Led device)
        {
            var message = new UpdateLedMessage() { Device = device, Timestamp = DateTime.Now };
            _bus.Publish<UpdateLedMessage>(message, x => { x.SetDeliveryMode(MassTransit.DeliveryMode.InMemory); });
        }

        public void Stop()
        {
            _bus.Dispose();
        }
    }
}
