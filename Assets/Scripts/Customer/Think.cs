using Camera;
using Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Customer
{
    public class Think : MonoBehaviour
    {
        // Responsible for that think bubble on customer's head

        [SerializeField] private SpriteRenderer target;
        [SerializeField] private SpriteRenderer bubble;

        [SerializeField] private KeepInCam keepInCam;


        [SerializeField] private SortingGroup sortingGroup;

        private string nowThink;

        public void StartThink(string text, bool important, int sorting = 0, Color color = default)
        {
            
            if (color != default)
                bubble.color = color;
            else
                bubble.color = Color.white;

            if (bubble != null) bubble.gameObject.SetActive(true);

            // Prevent deactivating the Customer if target is a direct child
            if (target.transform.parent != transform)
            {
                target.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                target.gameObject.SetActive(true);
            }

            if (important)
                keepInCam.keepInView = true;
            else
                keepInCam.keepInView = false;

            sortingGroup.sortingOrder = sorting;

            if (nowThink == text) return;

            nowThink = text;

            // this is the sprite in the bubble, so in waiting state it should show what customer ordered
            // in other states it shows current status
            target.sprite = GameManager.Instance.GetManager<TextureManager>().GetSprite(nowThink);
        }

        public void StopThink()
        {
            if (target != null && target.transform.parent != null)
            {
                // Prevent deactivating the Customer if target is a direct child
                if (target.transform.parent != transform)
                {
                    // UnityEngine.Debug.Log($"[Think] StopThink deactivating parent: {target.transform.parent.gameObject.name}");
                    target.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    // UnityEngine.Debug.Log($"[Think] StopThink deactivating target and bubble directly");
                    target.gameObject.SetActive(false);
                    if (bubble != null) bubble.gameObject.SetActive(false);
                }
            }
        }
        
    }
}