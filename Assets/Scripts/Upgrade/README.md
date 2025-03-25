# 아이템 및 업그레이드 시스템 설정 안내서

이 안내서는 아이템, 버프, 상점, 업그레이드 시스템을 설정하고 사용하는 방법을 설명합니다.

## 1. 필요한 프리팹

### PlayerStats 설정

1. 플레이어 오브젝트에 `PlayerStats` 컴포넌트를 추가합니다.
2. 기본 스탯 값을 설정합니다 (체력, 공격력, 방어력 등).

### 아이템 효과 (버프) 시스템 설정

1. 버프 UI를 표시할 Canvas 생성:
   - 빈 게임 오브젝트를 만들고 "BuffSystem"이라 명명합니다.
   - Canvas 컴포넌트를 추가하고 `BuffUI` 컴포넌트를 추가합니다.
   - "BuffContainer" 자식 오브젝트를 만들고 HorizontalLayoutGroup 컴포넌트를 추가합니다.

2. 버프 아이콘 프리팹 생성:
   - "BuffIcon" 이름의 빈 게임 오브젝트를 생성합니다.
   - Image 컴포넌트와 `BuffTooltip` 컴포넌트를 추가합니다.
   - TextMeshProUGUI 자식 오브젝트를 추가하여 남은 시간을 표시합니다.
   - 툴팁 패널을 자식으로 추가하고 제목과 설명을 위한 TextMeshProUGUI를 추가합니다.
   - 프리팹으로 저장합니다.

### 상점 시스템 설정

1. 상점 UI 생성:
   - "ShopSystem" 빈 게임 오브젝트 생성
   - Canvas 컴포넌트와 `ShopManager` 컴포넌트 추가
   - "ShopPanel" 패널 추가 (초기에는 비활성화)
   - ShopPanel에 다음 요소들을 추가:
     - "Title" (TextMeshProUGUI): 상점 제목
     - "CloseButton" (Button): 상점 닫기 버튼
     - "ItemContainer" (ScrollRect): 상점 아이템 목록을 표시할 컨테이너
     - "DetailPanel" (Panel): 선택한 아이템 상세 정보
     - "GoldText" (TextMeshProUGUI): 현재 보유 골드 표시

2. 상점 아이템 프리팹 생성:
   - "ShopItem" 빈 게임 오브젝트 생성
   - `ShopItemUI` 컴포넌트 추가
   - 다음 UI 요소들을 추가:
     - "ItemIcon" (Image): 아이템 아이콘
     - "ItemName" (TextMeshProUGUI): 아이템 이름
     - "ItemPrice" (TextMeshProUGUI): 아이템 가격
   - 프리팹으로 저장

### 업그레이드 시스템 설정

1. 업그레이드 UI 생성:
   - "UpgradeSystem" 빈 게임 오브젝트 생성
   - Canvas 컴포넌트와 `UpgradeManager` 컴포넌트 추가
   - "UpgradePanel" 패널 추가 (초기에는 비활성화)
   - UpgradePanel에 다음 요소들을 추가:
     - "Title" (TextMeshProUGUI): 업그레이드 제목
     - "CloseButton" (Button): 패널 닫기 버튼
     - "ItemIcon" (Image): 아이템 아이콘
     - "ItemName" (TextMeshProUGUI): 아이템 이름과 레벨
     - "CurrentStats" (TextMeshProUGUI): 현재 아이템 스탯
     - "NextLevelStats" (TextMeshProUGUI): 다음 레벨 스탯
     - "UpgradeCost" (TextMeshProUGUI): 업그레이드 비용
     - "UpgradeButton" (Button): 업그레이드 버튼
     - "PlayerGold" (TextMeshProUGUI): 현재 보유 골드

2. 인벤토리 UI 수정:
   - 인벤토리 아이템 정보 패널에 "UpgradeButton" 버튼 추가
   - 버튼을 클릭하면 업그레이드 시스템 패널이 열림

## 2. 필요한 아이템 설정

### 소모품 아이템 (ConsumableSO) 설정

1. 프로젝트 창에서 우클릭 > Create > Game > Items > Consumable 선택
2. 다음 필드를 설정:
   - 기본 아이템 정보 (이름, 설명, 아이콘 등)
   - healthRestore: 체력 회복량
   - manaRestore: 마나 회복량
   - hasAttackBoost: 공격력 증가 여부
   - attackBoostValue: 공격력 증가량 (기본값: 10%)
   - attackBoostDuration: 공격력 증가 지속시간 (기본값: 30초)
   - hasDefenseBoost: 방어력 증가 여부
   - defenseBoostValue: 방어력 증가량 (기본값: 10%)
   - defenseBoostDuration: 방어력 증가 지속시간 (기본값: 30초)
   - hasSpeedBoost: 이동속도 증가 여부
   - speedBoostValue: 이동속도 증가량 (기본값: 15%)
   - speedBoostDuration: 이동속도 증가 지속시간 (기본값: 20초)

### 무기 아이템 (WeaponSO) 설정

1. 프로젝트 창에서 우클릭 > Create > Game > Items > Weapon 선택
2. 기본 아이템 정보와 함께 다음 필드를 설정:
   - attackDamage: 기본 공격력
   - attackSpeed: 공격 속도
   - criticalChance: 치명타 확률

### 방어구 아이템 (ArmorSO) 설정

1. 프로젝트 창에서 우클릭 > Create > Game > Items > Armor 선택
2. 기본 아이템 정보와 함께 다음 필드를 설정:
   - defense: 방어력
   - healthBonus: 체력 보너스
   - elementalResistance: 원소 저항력

## 3. 시스템 사용 방법

### 상점 시스템 사용

```csharp
// 상점 열기
ShopManager.instance.OpenShop();

// 상점 닫기
ShopManager.instance.CloseShop();
```

### 업그레이드 시스템 사용

```csharp
// 아이템 업그레이드 패널 열기
UpgradeManager.instance.OpenUpgradePanel(itemSO);

// 아이템의 현재 레벨 가져오기
int level = UpgradeManager.instance.GetItemLevel(itemSO);

// 아이템의 현재 스탯 가져오기
float stat = UpgradeManager.instance.GetItemStat(itemSO);
```

### 버프 시스템 사용

```csharp
// 버프 생성 및 적용
PlayerStats.BuffData buff = PlayerStats.CreateBuff(
    "buff_id",       // 버프 ID
    "버프 이름",      // 버프 표시 이름
    PlayerStats.StatType.Attack,  // 스탯 유형
    0.1f,            // 증가량 (10%)
    30f,             // 지속시간 (30초)
    buffIcon,        // 버프 아이콘
    true             // 퍼센트 증가 여부
);

// 버프 적용
PlayerStats.instance.ApplyBuff(buff);
```

### 골드 관리

```csharp
// 골드 추가
PlayerStats.instance.AddGold(100);

// 골드 차감 (성공 여부 반환)
bool success = PlayerStats.instance.SpendGold(50);

// 현재 골드 확인
int currentGold = PlayerStats.instance.GetGold();
```

## 4. 아이템 사용 예시

### 체력 포션 사용 예시

체력 포션을 사용하면 즉시 체력이 회복됩니다:

1. ConsumableSO를 생성하고 healthRestore 값을 설정 (예: 30)
2. Use 메서드가 호출되면 PlayerStats.RestoreHealth 메서드를 호출하여 체력 회복

### 공격력 스크롤 사용 예시

공격력 스크롤을 사용하면 30초 동안 공격력이 10% 증가합니다:

1. ConsumableSO를 생성하고 hasAttackBoost = true, attackBoostValue = 0.1 (10%), attackBoostDuration = 30 (30초) 설정
2. Use 메서드가 호출되면 PlayerStats.ApplyBuff 메서드를 호출하여 공격력 버프 적용
3. 버프는 30초 동안 지속되고 자동으로 제거됨

### 아이템 업그레이드 예시

무기를 업그레이드하면 공격력이 10%씩 증가합니다:

1. UpgradeManager.upgradeableItems 배열에 업그레이드 가능한 아이템 추가
2. 업그레이드 진행 시 아이템 레벨 증가, 비용 차감, 공격력 증가 