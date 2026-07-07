using Unity.VisualScripting;
using UnityEngine;


    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        public GameObject gridInventoryPrefab;
        public GameObject trifoldInventoryPrefab;
        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }
        
        public GameObject GenerateInventoryUI(Inventory inventory)
        {
            GameObject go = Instantiate(gridInventoryPrefab, transform);
            GridInventoryUI gridInventoryUI = go.GetComponent<GridInventoryUI>();
            gridInventoryUI.BindInventory(inventory);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(inventory.sizeX*32, inventory.sizeY*32);
            return go;
        }
        
        [DoNotSerialize]
        InventoryTrifold currentTrifoldInventoryUI=null;
        
        public void OpenInventoryUITrifold(Inventory inventory)
        {
            if( currentTrifoldInventoryUI != null)
            {
               CloseInventoryUITrifold();
            }
            else
            {
                GameObject go = Instantiate(trifoldInventoryPrefab, transform.parent);
                InventoryTrifold trifoldInventoryUI = go.GetComponent<InventoryTrifold>();
                trifoldInventoryUI.BindInventory(inventory);
                currentTrifoldInventoryUI = trifoldInventoryUI;
            }
            
        }
        public void CloseInventoryUITrifold()
        {
            if( currentTrifoldInventoryUI != null)
            {
                Destroy(currentTrifoldInventoryUI.gameObject);
                currentTrifoldInventoryUI = null;
            }
        }
    }
