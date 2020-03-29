using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuPresenter : MonoBehaviour
{
    [SerializeField]
    private Button menuButton;

    [SerializeField]
    private RectTransform listTransform;
    
    [SerializeField]
    private Vector3 listOpenPosition;
    
    [SerializeField]
    private Vector3 listClosedPosition;

    [SerializeField]
    private float listTweenDuration;

    private bool menuOpened;

    void Awake()
    {
        menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    private void OnMenuButtonClicked()
    {
        menuOpened = !menuOpened;

        DOTween.Kill(listTransform);

        if (menuOpened)
        {
            listTransform.DOLocalMove(listOpenPosition, listTweenDuration);
        }
        else
        {
            listTransform.DOLocalMove(listClosedPosition, listTweenDuration);
        }
    }
}
