using UnityEngine;


    public class UIManager : MonoBehaviour
    {
        
        public GameObject gridInventoryPrefab;
        
        public GameObject GenerateInventoryUI(Inventory inventory)
        {
            GameObject go = Instantiate(gridInventoryPrefab, transform);
            GridInventoryUI gridInventoryUI = go.GetComponent<GridInventoryUI>();
            gridInventoryUI.BindInventory(inventory);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(inventory.sizeX*32, inventory.sizeY*32);
            return go;
        }
        
        public GameObject OpenInventoryUITrifold(Inventory inventory)
        {
            GameObject go = GenerateInventoryUI(inventory);
            go.SetActive(true);
            return go;
        }
    }
