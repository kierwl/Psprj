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
        // ���׷��̵� ��ư �̺�Ʈ ���
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeCharacter);

        // ó������ �г� �����
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public void ShowCharacterDetails(CharacterManager.CharacterData character)
    {
        if (character == null || upgradePanel == null)
            return;

        currentCharacter = character;

        // �г� ǥ��
        upgradePanel.SetActive(true);

        // ĳ���� ���� ������Ʈ
        if (characterIcon != null)
            characterIcon.sprite = character.icon;

        if (characterNameText != null)
            characterNameText.text = character.name;

        if (characterLevelText != null)
            characterLevelText.text = "Lv. " + character.level;

        // ���� ���� ������Ʈ
        if (healthText != null)
            healthText.text = "ü��: " + character.GetCurrentHealth().ToString("F0");

        if (attackText != null)
            attackText.text = "���ݷ�: " + character.GetCurrentAttack().ToString("F1");

        if (defenseText != null)
            defenseText.text = "����: " + character.GetCurrentDefense().ToString("F1");

        if (speedText != null)
            speedText.text = "�ӵ�: " + character.GetCurrentSpeed().ToString("F2");

        // ���׷��̵� ���
        int upgradeCost = character.GetUpgradeCost();
        if (upgradeCostText != null)
            upgradeCostText.text = "���: " + upgradeCost + " ���";

        // ���׷��̵� ��ư Ȱ��ȭ ����
        if (upgradeButton != null)
            upgradeButton.interactable = CurrencyManager.instance.gold >= upgradeCost;
    }

    public void UpgradeCharacter()
    {
        if (currentCharacter == null)
            return;

        int upgradeCost = currentCharacter.GetUpgradeCost();

        // ��� ���� �������� Ȯ��
        if (CurrencyManager.instance.SpendGold(upgradeCost))
        {
            // ���� ����
            currentCharacter.level++;

            // ĳ���� �Ŵ��� ������Ʈ
            CharacterManager.instance.UpdateCharacter(currentCharacter);

            // UI �ٽ� ǥ��
            ShowCharacterDetails(currentCharacter);

            // ȿ�� �Ǵ� �˸�
            Debug.Log(currentCharacter.name + " ĳ���Ͱ� ���� " + currentCharacter.level + "�� ��ȭ�Ǿ����ϴ�!");
        }
        else
        {
            Debug.Log("��尡 �����մϴ�!");
            // ��� ���� �˸� ǥ��
        }
    }

    public void ClosePanel()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
}