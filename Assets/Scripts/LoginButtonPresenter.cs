using UnityEngine;
using UnityEngine.UI;

public class LoginButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        ClientRunner.Login();
    }
}
