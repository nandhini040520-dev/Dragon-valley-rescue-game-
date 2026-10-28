using System;
using UnityEngine;
namespace DragonValley
{
    public enum MechanismState { OFF, POWERED, BLOCKED, PARTIAL, MISALIGNED, ACTIVE, COMPLETED, FAILED }
    public enum MechanismKind { Gate, Water, Bridge, Lift, Signal, Garden, Restoration }
    public enum MechanismAction { Crystal, Install, Power, Route, Gear, Pin, Unlock, Drive, Confirm, Clue, Blocker, Valve, Direction, Floor, NodeA, NodeB, Beacon, Temporary, Discovery }

    // Each link owns its input, constraint and output. Changes propagate downstream
    // immediately; the actuator integrates that output over time in the controller.
    public sealed class MechanismComponent
    {
        public readonly string Name;
        public bool Output { get; private set; }
        public bool Enabled=true;
        public MechanismComponent Input { get; private set; }
        public event Action<bool> Changed;
        public MechanismComponent(string name) { Name=name; }
        public void Connect(MechanismComponent input)
        {
            Input=input; input.Changed+=Receive; Evaluate();
        }
        void Receive(bool unused) { Evaluate(); }
        public void Configure(bool enabled) { Enabled=enabled; Evaluate(); }
        public void Evaluate()
        {
            bool next=Enabled && (Input==null || Input.Output);
            if(next==Output) return;
            Output=next; Changed?.Invoke(Output);
        }
    }
}
