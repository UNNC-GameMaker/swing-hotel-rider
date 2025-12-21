using Managers;
using UnityEngine;

public class ChooseBuilding : MonoBehaviour
{
    private GameObject _building;
    private BuildModeManager _manager;

    [Tooltip("that name object attached on room")]
    private GameObject _name;

    private bool _nowChoose;
    private Vector3 _originPos;

    public bool NowChoose
    {
        get => _nowChoose;
        set
        {
            _nowChoose = value;
            if (!_nowChoose) EndDrag();
        }
    }

    private void Start()
    {
        _manager = GameManager.Instance.GetManager<BuildModeManager>();
        _originPos = transform.position;
    }

    private void Update()
    {
        if (_nowChoose)
        {
            _name.SetActive(false);
        }
        else
        {
            _name.SetActive(true);
            MoveTo(_originPos);
        }
    }

    private void EndDrag()
    {
        if (!_building) _building = transform.GetChild(0).gameObject;

        var baseBuilding = _building.GetComponent<BaseBuilding>();
        Debug.Log(baseBuilding.CheckBuildable(false));
        if (baseBuilding.CheckBuildable(false))
        {
            baseBuilding.transform.SetParent(null);
            baseBuilding.Set();
            Destroy(gameObject);
        }
    }

    public void MoveTo(Vector2 position)
    {
        transform.position = Vector2.Lerp(transform.position, position, Time.deltaTime * 30);
    }
}