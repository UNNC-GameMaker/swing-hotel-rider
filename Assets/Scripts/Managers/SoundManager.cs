using System;

namespace Managers
{
    public class SoundManager : Manager
    {
        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }

        public static void Play(String ogg)
        {
            // TODO add this later
        }
    }
}