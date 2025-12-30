using System;
using System.Collections.Generic;
using UnityEngine;
using Input;

namespace Managers
{
    /// <summary>
    /// Interface for objects that want to receive input events
    /// </summary>
    public interface IInputListener
    {
        
        /// <summary>
        /// Called when a button input event occurs
        /// </summary>
        /// <param name="inputEvent">The input event that occurred</param>
        /// <param name="state">The state of the input (Started, Performed, Canceled)</param>
        public void OnInputEvent(InputEvents inputEvent, InputState state);
        
        /// <summary>
        /// Called when an axis value changes
        /// </summary>
        /// <param name="axis">The axis that changed</param>
        /// <param name="value">The new value of the axis</param>
        public void OnInputAxis(InputAxis axis, Vector2 value);
        
        /// <summary>
        /// Priority of this listener (higher priority listeners receive events first)
        /// </summary>
        int InputPriority { get; }
        
        /// <summary>
        /// Whether this listener should receive input events
        /// </summary>
        bool IsInputEnabled { get; }
    }
}