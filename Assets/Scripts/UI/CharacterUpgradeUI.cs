using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject upgradePanel;
    public Image characterIcon;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterLevelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI upgradeCostText;
    public Button upgradeButton;

    private CharacterManager.CharacterData currentCharacter;

    private void Start()
    {
        // 업그레이드 버튼 이벤트 등록
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeCharacter);

        // 처음에는 패널 숨기기
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public void ShowCharacterDetails(CharacterManager.CharacterData character)
    {
        if (character == null || upgradePanel == null)
            return;

        currentCharacter = character;

        // 패널 표시
        upgradePanel.SetActive(true);

        // 캐릭터 정보 업데이트
        if (characterIcon != null)
            characterIcon.sprite = character.icon;

        if (characterNameText != null)
            characterNameText.text = character.name;

        if (characterLevelText != null)
            characterLevelText.text = "Lv. " + character.level;

        // 스탯 정보 업데이트
        if (healthText != null)
            healthText.text = "체력: " + character.GetCurrentHealth().ToString("F0");

        if (attackText != null)
            attackText.text = "공격력: " + character.GetCurrentAttack().ToString("F1");

        if (defenseText != null)
            defenseText.text = "방어력: " + character.GetCurrentDefense().ToString("F1");

        if (speedText != null)
            speedText.text = "속도: " + character.GetCurrentSpeed().ToString("F2");

        // 업그레이드 비용
        int upgradeCost = character.GetUpgradeCost();
        if (upgradeCostText != null)
            upgradeCostText.text = "비용: " + upgradeCost + " 골드";

        // 업그레이드 버튼 활성화 여부
        if (upgradeButton != null)
            upgradeButton.interactable = CurrencyManager.instance.gold >= upgradeCost;
    }

    public void UpgradeCharacter()
    {
        if (currentCharacter == null)
            return;

        int upgradeCost = currentCharacter.GetUpgradeCost();

        // 비용 지불 가능한지 확인
        if (CurrencyManager.instance.SpendGold(upgradeCost))
        {
            // 레벨 증가
            currentCharacter.level++;

            // 캐릭터 매니저 업데이트
            CharacterManager.instance.UpdateCharacter(currentCharacter);

            // UI 다시 표시
            ShowCharacterDetails(currentCharacter);

            // 효과 또는 알림
            Debug.Log(currentCharacter.name + " 캐릭터가 레벨 " + currentCharacter.level + "로 강화되었습니다!");
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            // 골드 부족 알림 표시
        }
    }

    public void ClosePanel()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
}