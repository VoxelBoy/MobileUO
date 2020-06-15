using UnityEngine;
using UnityEngine.UI;

public class VersionTextPresenter : MonoBehaviour
{
    [SerializeField]
    private Text text;
    
    private void Awake()
    {
        text.text = "Version: " + Application.version;
    }
}
