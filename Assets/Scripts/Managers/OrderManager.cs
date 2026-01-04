using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class OrderManager : Manager
    {
        public OrderList currentOrderList;

        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
            LoadOrderList();
        }

        private void LoadOrderList()
        {
            string chapter = GameManager.Instance.GetCurrentChapter();
            TextAsset jsonText = Resources.Load<TextAsset>($"ChapterData/OrderList/{chapter}");
            UnityEngine.Debug.Log($"[OrderManager] Loaded {jsonText.text}");
            
            if (jsonText == null)
            {
                UnityEngine.Debug.LogError($"{chapter} not exist");
                return;
            }

            OrderListJson json = JsonUtility.FromJson<OrderListJson>(jsonText.text);
            
            currentOrderList = new OrderList();
            currentOrderList.Orders = new Dictionary<string, float>();
            
            if (json != null && json.orders != null)
            {
                foreach(var item in json.orders)
                {
                    if (!currentOrderList.Orders.ContainsKey(item.name))
                    {
                        currentOrderList.Orders.Add(item.name, item.prob);
                    }
                }
                UnityEngine.Debug.Log($"[OrderManager] Loaded {currentOrderList.Orders.Count} orders for {chapter}");
            }
        }
        
        public string GetRandomOrder()
        {
            if (currentOrderList == null || currentOrderList.Orders == null || currentOrderList.Orders.Count == 0)
            {
                UnityEngine.Debug.LogWarning("No orders available.");
                return "";
            }

            float totalWeight = 0f;
            foreach (var weight in currentOrderList.Orders.Values)
            {
                totalWeight += weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var order in currentOrderList.Orders)
            {
                currentWeight += order.Value;
                if (randomValue <= currentWeight)
                {
                    return order.Key;
                }
            }

            return "";
        }
    }

    [Serializable]
    public class OrderList
    {
        public int chapter;
        // orders: order name, order probability
        public Dictionary<string, float> Orders;
    }

    [Serializable]
    public class OrderItemJson
    {
        public string name;
        public float prob;
    }

    [Serializable]
    public class OrderListJson
    {
        public List<OrderItemJson> orders;
    }
}