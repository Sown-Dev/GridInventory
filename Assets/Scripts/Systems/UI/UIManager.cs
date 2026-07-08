using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        public GameObject gridInventoryPrefab;
        public GameObject trifoldInventoryPrefab;
        
        
        public Transform trifoldParent;
        //trifold stuff
        public UITrifold tri1;
        public UITrifold tri2;
        public UITrifold tri3;
        
        public PlayerInventoryTrifold playerInventoryTrifold;
        public PlayerEquipmentTrifold playerEquipmentTrifold;
        
        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
            }

            tri1 = null;
            tri2 = null;
            tri3 = null;

        }
        
        //tab toggle
        
        private void Update()
        {
            //BindInventory();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TabToggle();
            }
        }

        public void TabToggle()
        {
            //if ui open, close all, else open inventory
            if( IsAnyTrifoldOpen())
            {
                CloseAllTrifolds();
            }
            else
            {
                //open inventory
                OpenTrifold(playerInventoryTrifold, 2);
                OpenTrifold(playerEquipmentTrifold, 1);
            }
        }
        
        public void OpenTrifold(UITrifold trifold, int tri=1)
        {
            if( tri > 3 || tri < 1)
            {
                Debug.LogError("Invalid trifold number: " + tri);
                return;
            }
            
            if( tri == 1)
            {
                tri1 = trifold;
            }
            else if( tri == 2)
            {
                tri2 = trifold;
            }
            else if( tri == 3)
            {
                tri3 = trifold;
            }
            trifold.Open();
        }

        private bool IsAnyTrifoldOpen()
        {
            return tri1 != null || tri2 != null || tri3 != null;
        }
        
        public void CloseAllTrifolds()
        {
            if( tri1 != null)
            {
               CloseTrifold(ref tri1);
            }
            if( tri2 != null)
            {
                CloseTrifold(ref tri2);
            }
            if( tri3 != null)
            {
                CloseTrifold(ref tri3);
            }
        }
        private void CloseTrifold(ref UITrifold trifold)
        {
            trifold.Close();
            trifold = null;
        }
       
        
        //utility
        
        public void OpenInventoryTrifold(Inventory inventory)
        {
            OpenInventoryUITrifold(inventory);
        }
        
        
        
        //useless bs
        
        public GameObject GenerateInventoryUI(Inventory inventory)
        {
            GameObject go = Instantiate(gridInventoryPrefab, transform);
            GridInventoryUI gridInventoryUI = go.GetComponent<GridInventoryUI>();
            gridInventoryUI.BindInventory(inventory);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(inventory.sizeX*32, inventory.sizeY*32);
            return go;
        }
        
        
        public void OpenInventoryUITrifold(Inventory inventory)
        {
            if (tri3 != null)
            {
                CloseTrifold(ref tri3);
            }
            else
            {
                
                //todo: workaround for now but prob want a seperate function to open inventory instead of simulating tab press
                TabToggle();
                GameObject go = Instantiate(trifoldInventoryPrefab, trifoldParent);
                InventoryTrifold trifoldInventoryUI = go.GetComponent<InventoryTrifold>();
                trifoldInventoryUI.BindInventory(inventory);
                OpenTrifold(trifoldInventoryUI, 3);

            }
        }
        
    }
