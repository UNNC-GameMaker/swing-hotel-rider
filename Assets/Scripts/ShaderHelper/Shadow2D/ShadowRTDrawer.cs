using System;
using System.Collections.Generic;
using UnityEngine;


public class ShadowRTDrawer : MonoBehaviour
{
    // 简单做：全局就用一个实例。如果你以后要多套，可以改成 List。
    public static ShadowRTDrawer Instance;

    [Header("View Settings")]
    public float viewWidth = 10f;
    public float viewHeight = 10f;
    public float nearPlane = -10f;
    public float farPlane = 10f;

    [Header("Render Settings")]
    public Material whiteMat;
    public List<ShadowObject> casters = new List<ShadowObject>();

    [Serializable]
    public class ShadowObject
    {
        public SpriteRenderer spriteRenderer;
        public float level;
    }
    public RenderTexture shadowRT;

    Dictionary<Sprite, Mesh> meshCache = new Dictionary<Sprite, Mesh>();

    [HideInInspector]
    public UnityEngine.Camera shadowCamera;

    void OnEnable()
    {
        Instance = this;

        shadowCamera = GetComponent<UnityEngine.Camera>();
        shadowCamera.orthographic = true;
        shadowCamera.enabled = false;   // 仍然不让它直接参与正常渲染

        EnsureRenderTexture();
    }

    void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    public void EnsureRenderTexture()
    {
        int w = Screen.width;
        int h = Screen.height;

        if (shadowRT != null && (shadowRT.width != w || shadowRT.height != h))
        {
            Debug.Log($"Ensure RT {w}x{h}");
            //shadowRT.Release();
            //shadowRT = null;
        }

        if (shadowRT == null)
        {
            shadowRT = new RenderTexture(w, h, 0, RenderTextureFormat.R8);
            shadowRT.name = "ShadowRT_2D_URP";
            shadowRT.Create();
        }
    }

    public void ConfigureCamera()
    {
        if (shadowCamera == null) return;

        shadowCamera.orthographic = true;
        shadowCamera.orthographicSize = viewHeight * 0.5f;
        shadowCamera.aspect = viewWidth / viewHeight;
        shadowCamera.nearClipPlane = nearPlane;
        shadowCamera.farClipPlane = farPlane;

        // 你原来就是用 camera 的世界矩阵来当“shadow view”，这里保持不变。
    }

    public Mesh GetMesh(Sprite sprite)
    {
        if (sprite == null)
            return null;

        if (meshCache.TryGetValue(sprite, out Mesh m))
            return m;

        m = new Mesh();
        var verts = sprite.vertices;
        var uv = sprite.uv;
        var tris = sprite.triangles;

        Vector3[] verts3D = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            verts3D[i] = verts[i];

        m.SetVertices(verts3D);
        m.SetUVs(0, new List<Vector2>(uv));

        // sprite.triangles 是 ushort[]，要转 int[]
        int[] trisInt = new int[tris.Length];
        for (int i = 0; i < tris.Length; i++)
            trisInt[i] = tris[i];

        m.SetTriangles(trisInt, 0);

        meshCache[sprite] = m;
        return m;
    }
}
