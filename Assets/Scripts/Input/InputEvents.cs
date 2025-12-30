namespace Input
{
    /// <summary>
    /// Defines all possible input events in the game
    /// Return one of the InputState
    /// </summary>
    public enum InputEvents
    {
        // Movement
        Jump,
        Crouch,
        
        // Actions
        Grab,
        Release,
        Interact,
        
        // Building
        ChooseBuilding,
        PlaceBuilding,
        CancelBuilding,
        
        
        // UI
        Pause,
        Cancel,
        Submit,
        Exit,
        
        // Debug
        DebugToggle,
        ResetLevel
    }
    
    /// <summary>
    /// Input state for analog inputs (axes, mouse position, etc.)
    /// Return Vector 2
    /// </summary>
    public enum InputAxis
    {
        MoveAxis,
        MouseAxis,
        MouseScrollWheel,
        CameraAxis,
    }
    
    /// <summary>
    /// Defines the state of an input event
    /// </summary>
    public enum InputState
    {
        Started,    // Button just pressed
        Performed,  // Button held down
        Canceled    // Button released
    }
}