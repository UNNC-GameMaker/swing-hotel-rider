namespace Managers
{
    public class SoundManger : Manager
    {
        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }
    }
}