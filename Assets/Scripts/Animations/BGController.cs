using UnityEngine;

namespace Animations
{
    public class BGController : MonoBehaviour
    {
        public Transform Cam;

        [Header("Movement Settings")]
        public float factorX;
        public float factorY;

        public float offsetX;
        public float offsetY;

        [Header("Mouse Settings")]
        // 当没有Cam时，鼠标移动的灵敏度
        public float mouseStrength = 0.05f; 

        void Update()
        {
            // ---------------------------------------------------------
            // 情况 A: Cam 存在 (完全保持你原本的逻辑)
            // ---------------------------------------------------------
            if (Cam != null)
            {
                // 这两行代码的运算逻辑和你原来完全一致
                transform.localPosition = new Vector3(
                    Cam.position.x * factorX + offsetX, 
                    Cam.position.y * factorY + offsetY, 
                    0
                );
            }
            // ---------------------------------------------------------
            // 情况 B: Cam 不存在 (Cam 为空时启用鼠标逻辑)
            // ---------------------------------------------------------
            else
            {
                // 算出鼠标相对于屏幕中心的距离
                float mouseDeltaX = UnityEngine.Input.mousePosition.x - (Screen.width / 2f);
                float mouseDeltaY = UnityEngine.Input.mousePosition.y - (Screen.height / 2f);

                // 应用相同的 factor 和 offset，保持手感一致性
                // 乘以 mouseStrength 是为了把像素单位缩小到合适的世界单位
                transform.localPosition = new Vector3(
                    mouseDeltaX * mouseStrength * factorX * 0.02f + offsetX, 
                    mouseDeltaY * mouseStrength * factorY * 0.02f + offsetY, 
                    0
                );
            }
        }
    }
}

