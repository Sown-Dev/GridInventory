using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance;

        [Header("Tooltip Fields")]
        [SerializeField]
        private TMP_Text title;

        [SerializeField] private RectTransform rt;

        [Header("Dynamic Components")]
        [Tooltip("Must have a VerticalLayoutGroup. Body/Image/Stat prefabs get instantiated as children here.")]
        [SerializeField] private RectTransform componentContainer;
        [Tooltip("Plain prefab: RectTransform + Image, nothing else.")]
        [SerializeField] private Image imageComponentPrefab;
        [Tooltip("Plain prefab: RectTransform + TMP_Text, nothing else.")]
        [SerializeField] private TMP_Text bodyComponentPrefab;

        [SerializeField] private CanvasGroup TTCG;
        [SerializeField] private CanvasGroup tooltipCG;



        [Header("Layout")][SerializeField] private Vector2 offset;
        [SerializeField] private int width = 228;
        private Vector2 screenDimensions = new Vector2(960, 540);

        [Header("Fade Settings")]
        [SerializeField]
        private AnimationCurve easeIn = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField] private AnimationCurve easeOut = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float fadeDuration = 0.2f;

        private GameObject tooltipCaller;
        private Coroutine fadeRoutine;
        private bool TTmouseOver;

        private void Awake()
        {
            Instance = this;
            tooltipCG.alpha = 0;
            tooltipCG.blocksRaycasts = false;
        }

        public bool hadGO;
        private void Update()
        {
            if (tooltipCaller != null && !tooltipCaller.activeSelf)
            {
                Hide();
            }

            if (tooltipCaller == null && hadGO)
            {
                Hide();
                hadGO = false;
            }

            if (tooltipCaller != null)
            {
                hadGO = true;
            }
        }

        private void OnShow(Vector2 position, bool useOffset, bool useWorldSpace = false,
     bool edgeDetect = true)
        {



            Vector2 screenPos = position;
            if (useWorldSpace)
            {
                Vector3 worldPos = new Vector3(position.x, position.y, 0f);
                screenPos = Camera.main.WorldToScreenPoint(worldPos);
            }

                TTCG.alpha = 1;
            


            bool hitsEdge = false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rt.parent, screenPos, null,
                out var canvasPos);

            if (edgeDetect)
            {
                // Apply offset AFTER getting the position but BEFORE edge detection
                Vector2 posWithOffset = canvasPos;
                if (useOffset) posWithOffset += offset;

                float halfW = screenDimensions.x * 0.5f;
                float halfH = screenDimensions.y * 0.5f;
                float tooltipW = width;
                float tooltipH = rt.sizeDelta.y;
                float margin = 16f;

                // Adjust for pivot point
                float pivotOffsetX = tooltipW * rt.pivot.x;
                float pivotOffsetY = tooltipH * rt.pivot.y;

                // Right edge - check if right side of tooltip goes past right boundary
                if (posWithOffset.x - pivotOffsetX + tooltipW > halfW)
                {
                    posWithOffset.x = halfW - tooltipW + pivotOffsetX;
                    hitsEdge = true;
                }

                // Left edge - check if left side of tooltip goes past left boundary
                if (posWithOffset.x - pivotOffsetX < -halfW)
                {
                    posWithOffset.x = -halfW + pivotOffsetX;
                    hitsEdge = true;
                }

                // Bottom edge - use old logic that worked
                if (posWithOffset.y - tooltipH - margin < -halfH)
                {
                    hitsEdge = true;
                    posWithOffset.y = -halfH + (tooltipH * 2f);
                }

                // Top edge
                if (posWithOffset.y - pivotOffsetY + tooltipH + margin > halfH)
                {
                    hitsEdge = true;
                    posWithOffset.y = halfH - tooltipH + pivotOffsetY - margin;
                }

                canvasPos = posWithOffset;
            }
            else if (useOffset)
            {
                canvasPos += offset;
            }

            // Debug.Log("showing tooltip. hit edge: " + hitsEdge);

            rt.anchoredPosition = canvasPos;

            ClearComponents();
        }

        /// <summary>
        /// Destroys every instantiated component (body, image, stat, etc.) currently
        /// sitting in the vertical layout container. Anything tagged DoNotKill survives,
        /// same exclusion rule the old statHolder clear used.
        /// </summary>
        private void ClearComponents()
        {
            foreach (Transform c in componentContainer)
            {
                if (!c.gameObject.CompareTag("DoNotKill"))
                    Destroy(c.gameObject);
            }
        }

        /// <summary>
        /// Instantiates the image prefab into the layout if a sprite is supplied.
        /// No-op if iconSprite is null, so tooltips without an icon don't get an empty slot.
        /// </summary>
        private void AddImageComponent(Sprite iconSprite)
        {
            if (iconSprite == null || imageComponentPrefab == null) return;

            var img = Instantiate(imageComponentPrefab, componentContainer);
            img.sprite = iconSprite;
            img.SetNativeSize();
        }

        /// <summary>
        /// Instantiates the body text prefab into the layout if non-empty text is supplied.
        /// No-op on null/empty text so tooltips without a body don't get an empty slot.
        /// </summary>
        private void AddBodyComponent(string bodyText)
        {
            if (string.IsNullOrEmpty(bodyText) || bodyComponentPrefab == null) return;

            var txt = Instantiate(bodyComponentPrefab, componentContainer);
            txt.text = bodyText;
        }

        /// <summary>
        /// Legacy entry point kept for any external callers that used to set the icon directly.
        /// Internally just routes through the same component path as everything else now.
        /// </summary>
        public void SetImage(Sprite iconSprite)
        {
            AddImageComponent(iconSprite);
        }

        public void ShowTooltip(IToolTippable tooltip, Vector2 position, GameObject caller, bool useOffset = true,
            bool useWorldSpace = false)
        {
            tooltipCaller = caller;
            title.text = tooltip.name;

            OnShow(position, useOffset, useWorldSpace);

            AddImageComponent(tooltip.icon);
            AddBodyComponent(tooltip.description);

            StartFade(0f, 1f, easeIn);
        }


        /*public void ShowStatusEffect(StatusEffectInstance status, Vector2 position, GameObject caller,
            bool useOffset = true, bool useWorldSpace = false)
        {

            tooltipCaller = caller;
            title.text = status.data.displayName;
            SetImage(status.data.icon);

            OnShow(position, useOffset, useWorldSpace);

            foreach (var stat in status.GetStatModifications().stats)
            {
                var go = Instantiate(statUIPrefab, statHolder);
                go.GetComponent<StatsUI>().Setup(stat);
            }

            StartFade(0f, 1f, easeIn);
        }*/

        /*public void ShowSimplified(string text, Vector2 position, GameObject caller, bool useOffset = false,
            bool useWorldSpace = false)
        {

            int lineCount = text.Split('\n').Length;

            tooltipCaller = caller;
            simpleText.SetText(text);

            OnShow(position, useOffset, useWorldSpace, simple: true, edgeDetect: false);


            StartFade(0f, 1f, easeIn);

            LayoutRebuilder.ForceRebuildLayoutImmediate(simpleText.transform.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(simpleRT);
        }*/

        /*public void ShowUpgrade(Upgrade upgrade, Vector2 position, GameObject caller, bool useOffset = true,
            bool useWorldSpace = false)
        {
            tooltipCaller = caller;
            title.text = upgrade.Name;
            SetImage(upgrade.Icon);

            OnShow(position, useOffset, useWorldSpace);

            if (upgrade.description.IsNullOrWhiteSpace())
            {
                foreach (var stat in upgrade.st.stats)
                {
                    var go = Instantiate(statUIPrefab, statHolder);
                    go.GetComponent<StatsUI>().Setup(stat);
                }
            }
            else
            {
                description.text = upgrade.description;
            }


            StartFade(0f, 1f, easeIn);
        }*/

        // NOTE: this was previously broken in the source file — it referenced an "upgrade"
        // variable that doesn't exist in this method, and called .Icon / .description / .st.stats
        // which don't match IToolTippable's shape. Rewritten below assuming ItemData exposes
        // Name, icon, and description (same shape as IToolTippable). Update the field names
        // here if your actual ItemData differs.
        public void ShowItem(ItemData item, Vector2 position, GameObject caller, bool useOffset = false,
            bool useWorldSpace = false)
        {

            tooltipCaller = caller;
            title.text = item.GetName(); 

            OnShow(position, useOffset, useWorldSpace);

            
            // for each item component

            if (item.HasComponent<DurabilityItemComponent>())
            {
                
                AddBodyComponent($"Durability: {item.GetComponent<DurabilityItemComponent>().durability}/{item.GetComponent<DurabilityItemComponent>().maxDurability}");
            }
            
            if( item.HasComponent<EquipmentItemComponent>())
            {
                string stats = item.GetComponent<EquipmentItemComponent>().GetDefinition<EquipmentComponentDefinition>().stats.ToString();
                
                AddBodyComponent($"Stats: \n<color=green>{stats}</color>\nType: {item.GetComponent<EquipmentItemComponent>().GetDefinition<EquipmentComponentDefinition>().equipmentType}");
            }
            
            //description last:
            AddBodyComponent($"<color=grey>{item.GetDescription()}</color>");


            StartFade(0f, 1f, easeIn);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(componentContainer.transform.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }


        public void Hide()
        {
            // tooltipCaller = null;
            StartFade(tooltipCG.alpha, 0f, easeOut);

        }

        public void OnTTEnter()
        {
            TTmouseOver = true;
        }

        public void OnTTExit()
        {
            TTmouseOver = false;
            Hide();
        }

        private void StartFade(float from, float to, AnimationCurve curve)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeCoroutine(from, to, curve));
        }

        private IEnumerator FadeCoroutine(float from, float to, AnimationCurve curve)
        {
            float t = 0f;
            // Set blocksRaycasts immediately based on target alpha
            tooltipCG.blocksRaycasts = to > 0.5f;



            while (t < fadeDuration)
            {
                float norm = t / fadeDuration;
                tooltipCG.alpha = Mathf.LerpUnclamped(from, to, curve.Evaluate(norm));
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Ensure final values are set
            tooltipCG.alpha = to;
            tooltipCG.blocksRaycasts = to > 0.5f;

            // Clear the routine reference when done
            fadeRoutine = null;
        }
    }

    public interface IToolTippable
    {
        string name { get; }
        string description { get; }
        Sprite icon { get; }
    }
}