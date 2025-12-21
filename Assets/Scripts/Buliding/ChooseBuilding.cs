using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class ChooseBuilding : MonoBehaviour
{
    BuildModeManager _manager;

    private GameObject _building;
    private bool _nowChoose;
    private Vector3 _originPos;
    [Tooltip("that name object attached on room")]
    private GameObject _name;
    
    public bool NowChoose{get{return _nowChoose;} set{_nowChoose = value; if (!_nowChoose){EndDrag();}} }

    void EndDrag()
    {
        if (!_building) _building = transform.GetChild(0).gameObject;
        
        BaseBuilding baseBuilding = _building.GetComponent<BaseBuilding>();
        Debug.Log(baseBuilding.CheckBuildable(false));
        if (baseBuilding.CheckBuildable(false))
        {
            baseBuilding.transform.SetParent(null);
            baseBuilding.Set();
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _manager = GameManager.Instance.GetManager<BuildModeManager>();
        _originPos =  transform.position;
    }

    void Update()
    {
        if(_nowChoose)
        {
            _name.SetActive(false);
        }
        else
        {
            _name.SetActive(true);
            MoveTo(_originPos);
        }
    }
    
    public void MoveTo(Vector2 position)
    {
        transform.position = Vector2.Lerp(transform.position, position, Time.deltaTime * 30);
    }
}
