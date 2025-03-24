// ���� UI ������
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject settingsPanel;
    public GameObject upgradePanel;

    private void Start()
    {
        // ���� �� ��� �г� �����
        CloseAllPanels();
    }

    public void OpenInventory()
    {
        CloseAllPanels();
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    public void OpenShop()
    {
        CloseAllPanels();
        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OpenUpgrades()
    {
        CloseAllPanels();
        if (upgradePanel != null)
            upgradePanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
}