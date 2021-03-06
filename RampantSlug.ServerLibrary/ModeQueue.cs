﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RampantSlug.ServerLibrary.Hardware.Proc;

namespace RampantSlug.ServerLibrary
{
    public class ModeQueue
    {
        protected GameController _game;
        protected List<Mode> _modes;
        protected object _mode_lock_obj = new object();
        public ModeQueue(GameController game)
        {
            _game = game;
            _modes = new List<Mode>();
        }

        public void Add(Mode mode)
        {
            if (_modes.Contains(mode))
                throw new Exception("Attempted to add mode " + mode.ToString() + ", already in mode queue.");

            lock (_mode_lock_obj)
            {
                _modes.Add(mode);
            }
            //self.modes.sort(lambda x, y: y.priority - x.priority)
            _modes.Sort();
            mode.mode_started();

            if (mode == _modes[0])
                mode.mode_topmost();
        }

        public void Remove(Mode mode)
        {
            mode.mode_stopped();
            lock (_mode_lock_obj)
            {
                _modes.Remove(mode);
            }

            if (_modes.Count > 0)
                _modes[0].mode_topmost();
        }

        public void handle_event(Event evt)
        {
            Mode[] modes = new Mode[_modes.Count()];
            _modes.CopyTo(modes);
            for (int i = 0; i < modes.Length; i++)
            {
                bool handled = modes[i].handle_event(evt);
                if (handled)
                    break;
            }
        }

        public void Clear()
        {
            _modes.Clear();
        }

        public void tick()
        {
            Mode[] modes;
            lock (_mode_lock_obj)
            {
                modes = new Mode[_modes.Count()];
                _modes.CopyTo(modes);
            }
            for (int i = 0; i < modes.Length; i++)
            {
                modes[i].dispatch_delayed();
                modes[i].mode_tick();
            }
        }

        public List<Mode> Modes
        {
            get { return _modes; }
        }
    }
}

