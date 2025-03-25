# 인벤토리 시스템 설정 안내서

이 안내서는 인벤토리 시스템을 설정하고 사용하는 방법을 설명합니다.

## 1. 프리팹 생성 방법

### 인벤토리 프리팹 생성하기

1. 빈 게임 오브젝트를 생성하고 "InventorySystem"이라 명명합니다.
2. InventorySystem 오브젝트에 `InventoryManager` 컴포넌트를 추가합니다.
3. InventorySystem 아래에 "Inventory" 자식 오브젝트를 생성하고 `Inventory` 컴포넌트를 추가합니다.
4. InventorySystem 아래에 "InventoryUI" 자식 오브젝트를 생성합니다.
   - InventoryUI 오브젝트에 Canvas 컴포넌트를 추가하고 `InventoryUI` 컴포넌트를 추가합니다.
   - Canvas Scaler 컴포넌트를 추가하고 UI Scale Mode를 "Scale With Screen Size"로 설정합니다.

### 인벤토리 UI 설정하기

1. InventoryUI 아래에 "InventoryPanel" 오브젝트를 생성하고 Panel 컴포넌트를 추가합니다.
   - Anchor Preset을 전체화면으로 설정합니다.
   - 이 패널은 인벤토리가 열릴 때 활성화됩니다.

2. InventoryPanel 아래에 다음과 같은 요소들을 추가합니다:
   - "Title" (TextMeshProUGUI): 인벤토리 제목
   - "SortButton" (Button): 아이템 정렬 버튼
   - "CloseButton" (Button): 인벤토리 닫기 버튼
   - "ItemSlotsPanel" (Panel): 아이템 슬롯을 포함할 패널
   - "ItemInfoPanel" (Panel): 선택된 아이템 정보를 표시할 패널

3. ItemSlotsPanel 아래에 GridLayoutGroup 컴포넌트를 추가하고 다음과 같이 설정합니다:
   - Cell Size: 60x60
   - Spacing: X=10, Y=10
   - Constraint: Fixed Column Count = 5 (가로 5개)

4. ItemInfoPanel에 다음과 같은 요소들을 추가합니다:
   - "ItemIcon" (Image): 아이템 아이콘
   - "ItemName" (TextMeshProUGUI): 아이템 이름
   - "ItemDescription" (TextMeshProUGUI): 아이템 설명
   - "ItemStats" (TextMeshProUGUI): 아이템 스탯 정보
   - "UseButton" (Button): 아이템 사용 버튼
   - "DropButton" (Button): 아이템 버리기 버튼

### 아이템 슬롯 프리팹 만들기

1. "ItemSlot" 이름의 빈 게임 오브젝트를 생성합니다.
2. `ItemSlotUI` 컴포넌트를 추가합니다.
3. 버튼 컴포넌트를 추가합니다.
4. 다음 UI 요소를 자식으로 추가합니다:
   - "Background" (Image): 슬롯 배경
   - "Highlight" (Image): 선택 시 강조 표시 (초기에는 비활성화)
   - "ItemImage" (Image): 아이템 이미지 (초기에는 비활성화)
   - "AmountText" (TextMeshProUGUI): 아이템 개수 (초기에는 비활성화)
5. 프리팹으로 저장합니다.

### 인벤토리 버튼 설정하기

1. 인벤토리를 열기 위한 버튼 생성:
   - UI > Button을 생성하고 "InventoryButton"이라 명명합니다.
   - 버튼 이미지와 텍스트를 적절히 설정합니다 (예: 가방 아이콘).
   - 생성한 버튼에 `InventoryButton` 컴포넌트를 추가합니다.

2. 인벤토리 패널의 닫기 버튼:
   - InventoryPanel 내에 "CloseButton"이라는 이름의 버튼이 있어야 합니다.
   - 이 버튼은 자동으로 인벤토리 패널을 닫는 기능에 연결됩니다.

## 2. 인벤토리 매니저 설정

1. 인벤토리 매니저 컴포넌트에서 다음 필드를 설정합니다:
   - Inventory: 인벤토리 컴포넌트 참조
   - InventoryUI: 인벤토리UI 컴포넌트 참조
   - Toggle Inventory Key: 인벤토리 열기/닫기 키 (기본 "I")
   - Debug Items: 테스트용 아이템 (ScriptableObject 배열)

## 3. 시스템 사용 방법

### 인벤토리에 아이템 추가하기

```csharp
// 아이템 1개 추가
InventoryManager.instance.AddItem(itemSO);

// 여러 개의 아이템 추가
InventoryManager.instance.AddItem(itemSO, 5);
```

### 인벤토리에서 아이템 제거하기

```csharp
// 아이템 1개 제거
InventoryManager.instance.RemoveItem(itemSO);

// 여러 개의 아이템 제거
InventoryManager.instance.RemoveItem(itemSO, 3);
```

### 인벤토리에 아이템 존재 여부 확인하기

```csharp
if (InventoryManager.instance.HasItem(itemSO))
{
    // 아이템이 존재할 때 로직
}
```

### 인벤토리 UI 열기/닫기

```csharp
// 코드로 인벤토리 열기/닫기 토글
InventoryManager.instance.ToggleInventory();
```

## 4. ItemSO (아이템 ScriptableObject) 만들기

1. 프로젝트 창에서 우클릭 > Create > Game > Items > Base Item 또는 파생 아이템 중 하나를 선택합니다.
2. 생성된 ScriptableObject에서 다음 정보를 설정합니다:
   - Item ID: 고유 아이템 ID
   - Item Name: 아이템 이름
   - Description: 아이템 설명
   - Icon: 아이템 아이콘 
   - Item Type: 아이템 유형 (무기, 방어구, 소모품 등)
   - Rarity: 아이템 희귀도
   - Buy/Sell Price: 상점 판매/구매 가격

## 5. 아이템 종류별 설정

1. 무기 (WeaponSO)
   - 추가 필드: 공격력, 공격 속도, 등

2. 방어구 (ArmorSO)
   - 추가 필드: 방어력, 체력 보너스, 등

3. 소모품 (ConsumableSO)
   - 추가 필드: 회복량, 지속시간, 등 