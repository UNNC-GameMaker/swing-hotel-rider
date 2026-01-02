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
            target.transform.parent.gameObject.SetActive(true);

            if (important)
                keepInCam.keepInView = true;
            else
                keepInCam.keepInView = false;

            sortingGroup.sortingOrder = sorting;

            if (nowThink == text) return;

            nowThink = text;

            target.sprite = GameManager.Instance.GetManager<TextureManager>().GetSprite("Food/" + nowThink);
        }

        public void StopThink()
        {
            target.transform.parent.gameObject.SetActive(false);
        }
        
    }
}