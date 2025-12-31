using UnityEngine;

namespace GameObjects
{
    /// <summary>
    ///     Base class for all grabbable objects that the player can interact with
    /// </summary>
    public abstract class Grabbable : MonoBehaviour
    {
        public Rigidbody2D rb;

        /// <summary>
        ///     Called when the object is grabbed by the player
        /// </summary>
        public abstract void OnGrab();

        /// <summary>
        ///     Called when the object is released by the player
        /// </summary>
        public abstract void OnRelease();
    }
}