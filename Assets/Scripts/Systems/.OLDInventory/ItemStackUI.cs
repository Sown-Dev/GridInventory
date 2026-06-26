using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.Inventory
{
    public class ItemStackUI: MonoBehaviour,  IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public Image icon;
        public Image bg;
        public TMP_Text stackSize;

        public RectTransform rt;


        public GridUI myGrid;
        //dragging stuff:
        private bool allowDrag = true;
        private bool isDragging = false;

        private Vector3 awakePos;
        private Vector3 targetPos;

        public ItemStack myStack;
    
        
        protected Material mat;

        private void Awake()
        {
            mat = new Material( icon.material);
            icon.material = mat;
            mat.SetFloat("_PixelsPerUnit", icon.sprite.texture.width);

            mat.SetColor("_OutlineColor", Color.white);

        }

        public void Init(ItemStack itemStack, GridUI gridUI)
        {
            myGrid = gridUI;
        
    
            myStack = itemStack; 
            icon.sprite = itemStack.myItem.Icon;
            

            rt.sizeDelta = new Vector2(ItemRegistry.GridSize*itemStack.Width(),ItemRegistry.GridSize*itemStack.Height());
            icon.rectTransform.sizeDelta = rt.sizeDelta;
            bg.rectTransform.sizeDelta = rt.sizeDelta;

            awakePos = rt.anchoredPosition;
            
            mat.SetFloat("_PixelsPerUnit", icon.sprite.texture.width);

            
        }

        private Vector3 iconTargetPos;
        private void Update(){
        

            if (!isDragging){
                rt.anchoredPosition = Vector3.Lerp(rt.anchoredPosition, awakePos, Time.unscaledDeltaTime * 32);
            }
            else{
                transform.position = Vector3Int.RoundToInt(Vector3.Lerp(transform.position, targetPos, Time.unscaledDeltaTime * 32));
            }
            
            icon.rectTransform.anchoredPosition = Vector3.Lerp(icon.rectTransform.anchoredPosition,  iconTargetPos, Time.unscaledDeltaTime * 24);
            
        stackSize.text = myStack.Quantity >1 ? myStack.Quantity.ToString() : "";
        }

        public void OnDrag(PointerEventData eventData){
            if (!allowDrag|| !isDragging) return;


            targetPos = eventData.position;
           // myImg.raycastTarget = false;
           
           
        }
    
        
        (int,int) stackPos=(0,0);
        public void OnBeginDrag(PointerEventData eventData){
            //temp
        
            if (!allowDrag) return;

            transform.SetParent(ItemRegistry.instance.onTop, false);
            transform.position = eventData.position;

            isDragging = true;
            
            stackPos = (myStack.position.x, myStack.position.y);
            
            myGrid.myGrid.RemoveItem( myStack.position.x ,myStack.position.y);

            
            OnPointerExit(eventData);
            
        }

        public void OnEndDrag(PointerEventData eventData){
            if (!allowDrag) return;
        
          
        
            transform.SetParent(myGrid.transform, true);
            transform.localScale = Vector3.one;
            isDragging = false;



            if (!myGrid.OnDrop(eventData))
            {
                myGrid.myGrid.PlaceItem(myStack, myStack.position.x, myStack.position.y);
            }
            myGrid.ClearHighlight();
        }


        private bool mouseOver;

        public void OnPointerEnter(PointerEventData eventData){

            if (!isDragging)
            {
                mouseOver = true;
                //selection
                iconTargetPos = new Vector3(0, 1, 0);
                mat.SetFloat("_OutlineThickness", 1);

            }
            //Debug.Log($"mover {myStack.myItem.name}, pos {myStack.position.x}, {myStack.position.y}");

        }

        public void OnPointerExit(PointerEventData eventData){
            iconTargetPos= new Vector3(0,-1,0);
            mat.SetFloat("_OutlineThickness", 0);

            mouseOver = false;
          
        }
    }
}