using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CollapsibleGroup : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform headerRectTransform;
    [SerializeField] private RectTransform foldoutArrowRectTransform;
    [SerializeField] private bool collapsed;

    private RectTransform rectTransform;
    private Vector2 originalSizeDelta;
    private float headerHeight;
    private float totalHeight;

    private const float tweenDuration = 0.3f;
    private const Ease tweenEase = Ease.InSine;
    
    private static readonly Vector3 foldoutArrowCollapsedRotation = new Vector3(0,0,-90);
    private static readonly Vector3 foldoutArrowExpandedRotation = new Vector3(0,0,-180);
    
    // Start is called before the first frame update
    private void Start()
    {
        Init();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Init()
    {
        rectTransform = transform as RectTransform;
        originalSizeDelta = rectTransform.sizeDelta;
        totalHeight = UtilityMethods.GetTotalHeightOfChildren(rectTransform);
        headerHeight = headerRectTransform.sizeDelta.y;
    }

    private void OnButtonClicked()
    {
        DOTween.Kill(rectTransform);
        DOTween.Kill(foldoutArrowRectTransform);

        if (collapsed)
        {
            rectTransform.DOSizeDelta(new Vector2(originalSizeDelta.x, totalHeight), tweenDuration).SetEase(tweenEase).SetUpdate(true);
            foldoutArrowRectTransform.DOLocalRotate(foldoutArrowExpandedRotation, tweenDuration).SetEase(tweenEase).SetUpdate(true);
        }
        else
        {
            rectTransform.DOSizeDelta(new Vector2(originalSizeDelta.x, headerHeight), tweenDuration).SetEase(tweenEase).SetUpdate(true);
            foldoutArrowRectTransform.DOLocalRotate(foldoutArrowCollapsedRotation, tweenDuration).SetEase(tweenEase).SetUpdate(true);
        }

        collapsed = !collapsed;
    }

    public void CollapseImmediately()
    {
        Init();
        DOTween.Kill(rectTransform);
        DOTween.Kill(foldoutArrowRectTransform);
        rectTransform.sizeDelta = new Vector2(originalSizeDelta.x, headerHeight);
        foldoutArrowRectTransform.localRotation = Quaternion.Euler(foldoutArrowCollapsedRotation);
        collapsed = true;
    }

    public void ExpandImmediately()
    {
        Init();
        DOTween.Kill(rectTransform);
        DOTween.Kill(foldoutArrowRectTransform);
        rectTransform.sizeDelta = new Vector2(originalSizeDelta.x, totalHeight);
        foldoutArrowRectTransform.localRotation = Quaternion.Euler(foldoutArrowExpandedRotation);
        collapsed = false;
    }
}
