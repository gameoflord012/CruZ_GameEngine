﻿using System.Collections.Generic;
using System.Windows.Forms;

namespace CruZ.Editor.Services
{
    enum InvalidatedEvents
    {
        EntityComponentChanged,
        SelectingEntityChanged
    }

    class InvalidatedService
    {
        public static void Register(Control controls, params InvalidatedEvents[] invalidatedEvents)
        {
            foreach (var invalidatedEvent in invalidatedEvents)
            {
                GetRegisters(invalidatedEvent).Remove(controls);
                GetRegisters(invalidatedEvent).Add(controls);
            }
        }

        public static void SendInvalidated(InvalidatedEvents invalidatedEvent)
        {
            foreach (var control in GetRegisters(invalidatedEvent))
            {
                control.Refresh();
            }
        }

        static List<Control> GetRegisters(InvalidatedEvents invalidatedEvent)
        {
            if (!registers.ContainsKey(invalidatedEvent))
                registers[invalidatedEvent] = [];

            return registers[invalidatedEvent];
        }

        static Dictionary<InvalidatedEvents, List<Control>> registers = [];
    }
}