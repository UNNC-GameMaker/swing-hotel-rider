using UnityEngine;

namespace Managers
{
    /// <summary>
    ///     Base class for all managers.
    ///     Execution order set to -50 to ensure managers initialize after GameManager (-100)
    ///     but before regular game scripts (default 0).
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public abstract class Manager : MonoBehaviour
    {
        public abstract void Init();
    }
}