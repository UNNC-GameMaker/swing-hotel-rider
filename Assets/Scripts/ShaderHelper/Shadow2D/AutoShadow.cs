using UnityEngine;

namespace ShaderHelper.Shadow2D
{
    public class AutoShadow : MonoBehaviour
    {
        public float ShadowLevel = 1f;
        private ShadowRTDrawer shadowRTDrawer;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            if (shadowRTDrawer == null)
                shadowRTDrawer = ShadowRTDrawer.Instance;

            if (shadowRTDrawer == null)
                return;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            foreach (var sr in shadowRTDrawer.casters)
                if (sr != null && sr.spriteRenderer == spriteRenderer)
                    return;

            shadowRTDrawer.casters.Add(new ShadowRTDrawer.ShadowObject
            {
                spriteRenderer = spriteRenderer,
                level = ShadowLevel
            });
        }

        private void OnDisable()
        {
            foreach (var sr in shadowRTDrawer.casters.ToArray())
                if (sr != null && sr.spriteRenderer == spriteRenderer)
                {
                    shadowRTDrawer.casters.Remove(sr);
                    break;
                }
        }
    }
}