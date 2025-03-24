// 게임 UI 관리자
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject settingsPanel;
    public GameObject upgradePanel;

    public GameObject offlineProgressPanel;
    public TextMeshProUGUI offlineTimeText;
    public TextMeshProUGUI offlineGoldText;
    public TextMeshProUGUI offlineExpText;
    public TextMeshProUGUI offlineMonstersText;


    private void Start()
    {


        // 시작 시 모든 패널 숨기기
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

    public void ShowOfflineProgressResults(double hours, int gold, int exp, int monsters)
    {
        if (offlineProgressPanel != null)
        {
            offlineProgressPanel.SetActive(true);

            if (offlineTimeText != null)
                offlineTimeText.text = string.Format("{0:F1}시간", hours);

            if (offlineGoldText != null)
                offlineGoldText.text = gold.ToString();

            if (offlineExpText != null)
                offlineExpText.text = exp.ToString();

            if (offlineMonstersText != null)
                offlineMonstersText.text = monsters.ToString();
        }
    }

    public void CloseOfflineProgressPanel()
    {
        if (offlineProgressPanel != null)
            offlineProgressPanel.SetActive(false);
    }
}