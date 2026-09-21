using UnityEngine;

public class SubMicroscopicViewManager : MonoBehaviour
{
    [SerializeField] private GameObject subMicroscopicPanel;

    public void OpenSubMicroscopicView()
    {
        if (subMicroscopicPanel != null)
        {
            subMicroscopicPanel.SetActive(true);
        }
    }

    public void CloseSubMicroscopicView()
    {
        if (subMicroscopicPanel != null)
        {
            subMicroscopicPanel.SetActive(false);
        }
    }
}