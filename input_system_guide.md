## How to use new input system

1. Implement IInputListener
2. register listener on whatever function you like
```csharp
        GameManager.Instance.GetManager<InputManager>().RegisterListener(this);
```
3. process input event in corresponding functions `OnInputAxis` and `OnInputEvent`

