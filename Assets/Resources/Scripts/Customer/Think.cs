using System.Collections;
using System.Collections.Generic;
using Camera;
using UnityEngine;
using UnityEngine.Rendering;

public class Think : MonoBehaviour
{
    // Responsible for that think bubble on customer's head

    [SerializeField] private SpriteRenderer target;
    [SerializeField] private SpriteRenderer bubble;

    [SerializeField] private KeepInCam keepInCam;


    [SerializeField] private SortingGroup sortingGroup;

    private string nowThink;

    public void StartThink(string text,bool important,int sorting=0,Color color=default)
    {
        if(color!=default){
            bubble.color=color;
        }else{
            bubble.color=Color.white;
        }
        target.transform.parent.gameObject.SetActive(true);

        if(important){
            keepInCam.keepInView=true;
        }else{
            keepInCam.keepInView=false;
        }

        sortingGroup.sortingOrder=sorting;

        if(nowThink==text){
            return;
        }

        nowThink=text;

        target.sprite = LoadSprite("" + text, "Think");
    }

    public void StopThink()
    {
        target.transform.parent.gameObject.SetActive(false);
    }
    
    public static Sprite LoadSprite(string filePath, string sliceName)
    {
        Sprite[] slices = Resources.LoadAll<Sprite>(filePath);

        if (slices == null || slices.Length == 0)
        {
            Debug.LogError($"SpriteLoaderNoCache: 未能加载 Resources/{filePath}");
            return null;
        }
        
        foreach (var s in slices)
        {
            if (s.name == sliceName)
                return s;
        }

        Debug.LogError($"SpriteLoaderNoCache: 在 {filePath} 中未找到切片 \"{sliceName}\"");
        return null;
    }
}