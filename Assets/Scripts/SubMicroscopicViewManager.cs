using UnityEngine;

public class SubMicroscopicViewManager : MonoBehaviour
{
    [SerializeField] private GameObject subMicroscopicPanel;
    [SerializeField] private GameObject instructionPanel;

    public void OpenSubMicroscopicView()
    {
        if (subMicroscopicPanel == null)
            return;

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        subMicroscopicPanel.SetActive(true);
        // Show the microscopic puzzle above the normal gameplay UI.
        subMicroscopicPanel.transform.SetAsLastSibling();
    }

    public void CloseSubMicroscopicView()
    {
        if (subMicroscopicPanel != null)
            subMicroscopicPanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(true);
    }
}
