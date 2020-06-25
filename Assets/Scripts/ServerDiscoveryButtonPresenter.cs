using UnityEngine;
using UnityEngine.UI;

public class ServerDiscoveryButtonPresenter : MonoBehaviour
{
    public Button button;
    public Text text;
    public Image searchingImage;
    public RectTransform searchingRectTransform;
    public float searchingImageRotationSpeed;
    
    public void Toggle(bool serverDiscoveryRunning)
    {
        button.interactable = serverDiscoveryRunning == false;
        searchingRectTransform.localRotation = Quaternion.identity;

        text.enabled = serverDiscoveryRunning == false;
        searchingImage.enabled = serverDiscoveryRunning;
    }

    private void Update()
    {
        if (searchingImage.enabled)
        {
            searchingRectTransform.Rotate(Vector3.back, Time.deltaTime * searchingImageRotationSpeed);
        }
    }
}