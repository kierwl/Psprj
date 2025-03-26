# 프로젝트 안내서

이 안내서는 아이템, 버프, 상점, 업그레이드 시스템을 설정하고 사용하는 방법을 설명합니다.

## 1. 필요한 프리팹

### PlayerStats 설정

1. 플레이어 오브젝트에 `PlayerStats` 컴포넌트를 추가합니다.
2. 기본 스탯 값을 설정합니다 (체력, 공격력, 방어력 등).

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

## 3. 스테이지 매니저 설정

### 스테이지 매니저 설정

1. 스테이지 매니저 오브젝트 생성:
   - "StageManager" 빈 게임 오브젝트 생성
   - `StageManager` 컴포넌트 추가

2. 스테이지 정보 설정:
   - 각 스테이지에 대한 정보를 담는 데이터 구조를 정의합니다 (예: 스테이지 번호, 적의 종류, 보상 등).
   - 스테이지 전환 로직을 구현하여 플레이어가 스테이지를 클리어할 때 다음 스테이지로 이동할 수 있도록 합니다.

3. 스테이지 시작 및 종료 처리:
   - 스테이지 시작 시 적을 생성하고, 플레이어에게 목표를 안내합니다.
   - 스테이지 종료 시 결과를 표시하고, 보상을 지급합니다.

## 4. 시스템 사용 방법

### 업그레이드 시스템 사용

```csharp
// 아이템 업그레이드 패널 열기
UpgradeManager.instance.OpenUpgradePanel(itemSO);

// 아이템의 현재 레벨 가져오기
int level = UpgradeManager.instance.GetItemLevel(itemSO);

// 아이템의 현재 스탯 가져오기
float stat = UpgradeManager.instance.GetItemStat(itemSO);
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

### 아이템 업그레이드 예시

무기를 업그레이드하면 공격력이 10%씩 증가합니다:

1. UpgradeManager.upgradeableItems 배열에 업그레이드 가능한 아이템 추가
2. 업그레이드 진행 시 아이템 레벨 증가, 비용 차감, 공격력 증가 

