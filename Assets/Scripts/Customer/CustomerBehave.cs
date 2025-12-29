using UnityEngine;
using Managers;

namespace Customer
{
    public class CustomerBehave : MonoBehaviour
    {
        private DeskManager _deskManager;
        private BuildingGridManager _buildingGridManager;
        private int CurrentLevel => Mathf.FloorToInt(transform.position.y / _buildingGridManager.GridSize.y);

        public void Awake()
        {
            _deskManager = GameManager.Instance.GetManager<DeskManager>();
            _buildingGridManager = GameManager.Instance.GetManager<BuildingGridManager>();
        }
        
        
    }


}