using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; }

    [Header("Setup")]
    public GameObject healthBarUIPrefab; // prefab with HealthBarUI, Image fill, CanvasGroup - NO Canvas component

    private Canvas sharedCanvas;
    private readonly Stack<HealthBarUI> pool = new Stack<HealthBarUI>();

    private void Awake()
    {
        Instance = this;
        sharedCanvas = GetComponent<Canvas>();
    }

    public HealthBarUI Get()
    {
        HealthBarUI bar;

        if (pool.Count > 0)
        {
            bar = pool.Pop();
            bar.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = Instantiate(healthBarUIPrefab, sharedCanvas.transform);
            bar = go.GetComponent<HealthBarUI>();
        }

        return bar;
    }

    public void Release(HealthBarUI bar)
    {
        if (bar == null) return;
        bar.gameObject.SetActive(false);
        pool.Push(bar);
    }
}