using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShadow : MonoBehaviour
{
    ShadowRTDrawer shadowRTDrawer;
    SpriteRenderer spriteRenderer;

    public float ShadowLevel = 1f;

    void OnEnable()
    {
        if (shadowRTDrawer == null)
            shadowRTDrawer = ShadowRTDrawer.Instance;

        if (shadowRTDrawer == null)
            return;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        foreach (var sr in shadowRTDrawer.casters)
        {
            if (sr != null && sr.spriteRenderer == spriteRenderer)
                return;
        }

        shadowRTDrawer.casters.Add(new ShadowRTDrawer.ShadowObject()
        {
            spriteRenderer = spriteRenderer,
            level = ShadowLevel,
        });
    }

    void Start()
    {
        OnEnable();
    }

    void OnDisable()
    {
        foreach (var sr in shadowRTDrawer.casters.ToArray())
        {
            if (sr != null && sr.spriteRenderer == spriteRenderer)
            {
                shadowRTDrawer.casters.Remove(sr);
                break;
            }
        }
    }
}
