
    using UnityEngine;

    [RequireComponent(typeof(RectTransform)), RequireComponent(typeof(CanvasGroup))]
    public class UITrifold: MonoBehaviour
    {
        public RectTransform RectTransform;
        
        public CanvasGroup myCanvasGroup;
        public bool KillToggle = false;
        public bool StartOpen = true;

        public virtual void Start()
        {
            if (StartOpen)
            {
                Open();
            }
            else
            {
                Close();
            }
            
            if( myCanvasGroup == null)
            {
                myCanvasGroup = GetComponent<CanvasGroup>();
            }
            if( RectTransform == null)
            {
                RectTransform = GetComponent<RectTransform>();
            }
        }
        
        public virtual void Open()
        {
            if (myCanvasGroup != null)
            {
                myCanvasGroup.alpha = 1f;
                myCanvasGroup.interactable = true;
                myCanvasGroup.blocksRaycasts = true;
            }
        }

        public virtual void Close()
        {
            if (KillToggle)
            {
                Destroy(gameObject);
            }
            else
            {
                if( myCanvasGroup != null)
                {
                    myCanvasGroup.alpha = 0f;
                    myCanvasGroup.interactable = false;
                    myCanvasGroup.blocksRaycasts = false;
                }
            }
        }
    }
