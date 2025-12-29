namespace Managers
{
    public class DeskManager : Manager
    {
        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }
        
    }
}