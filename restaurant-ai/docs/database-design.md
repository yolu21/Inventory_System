# Database Design

## 1. Database Overview

本系統使用 SQL Server 作為主要資料庫，負責儲存使用者、食材、庫存異動、餐點、餐點食材配方及 Excel 匯入紀錄等資料。

後端透過 Entity Framework Core 的 `InventoryDbContext` 存取資料庫，並將資料模型對應至 SQL Server 資料表。

目前系統主要包含以下資料表：

- `Users`：使用者與角色資料
- `Ingredients`：食材基本資料
- `StockRecords`：庫存 IN / OUT 異動紀錄
- `Meals`：餐點資料
- `MealIngredients`：餐點與食材之間的 BOM 關係
- `ImportLog`：Excel 匯入紀錄

---

## 2. Entity Relationship

系統主要資料關係如下：

```text
┌──────────────┐
│    Users     │
├──────────────┤
│ id           │
│ UseName      │
│ PasswordHash │
│ Role         │
└──────────────┘


┌────────────────┐
│  Ingredients   │
├────────────────┤
│ Id             │
│ Name           │
│ Unit           │
│ UnitCost       │
│ MinimumStock   │
└────────────────┘
        │
        │ 1
        │
        ├───────────────< StockRecords
        │
        │
        └───────────────< MealIngredients
                                │
                                │
                                ▼
                         ┌─────────────┐
                         │    Meals    │
                         ├─────────────┤
                         │ Id          │
                         │ Name        │
                         └─────────────┘


┌────────────────┐
│   ImportLog    │
├────────────────┤
│ id             │
│ UserName       │
│ FileName       │
│ SuccessCount   │
│ FailureCount   │
│ TotalCount     │
│ ImportTime     │
│ IsSuccess      │
│ ErrMsg         │
└────────────────┘
```

### 主要關係

#### Ingredients → StockRecords

一個食材可以有多筆庫存異動紀錄。

```text
Ingredients 1 ──────── N StockRecords
```

`StockRecords.IngredientId` 為 Foreign Key，對應 `Ingredients.Id`。

例如：

```text
雞蛋

├── IN   100
├── OUT   20
├── IN    50
└── OUT   30
```

系統可以根據 IN / OUT 紀錄計算目前庫存。

---

#### Meals → MealIngredients → Ingredients

一個餐點可以包含多種食材，而一種食材也可以被不同餐點使用，因此 `Meals` 與 `Ingredients` 之間屬於多對多關係。

系統使用 `MealIngredients` 作為中間資料表：

```text
Meal 1 ──────── N MealIngredient N ──────── 1 Ingredients
```

`MealIngredients` 透過：

- `MealId` 對應餐點
- `IngredientId` 對應食材
- `Quantity` 記錄該餐點所需的食材數量

例如：

```text
蛋餅

├── 雞蛋  2 顆
├── 麵粉  50 g
└── 油    10 ml
```

此資料同時作為餐點出餐時的庫存扣料依據。

---

## 3. Table Design

### 3.1 Users

儲存系統使用者及其角色資訊。

| 欄位         | 型別          | 說明                              |
| ------------ | ------------- | --------------------------------- |
| id           | int           | 使用者識別碼，Primary Key         |
| UseName      | nvarchar(max) | 使用者名稱                        |
| PasswordHash | nvarchar(max) | 雜湊後的密碼                      |
| Role         | nvarchar(max) | 使用者角色，例如 `Admin` / `User` |

使用者登入後，系統會根據 `Role` 判斷其可使用的功能。

---

### 3.2 Ingredients

儲存食材基本資料，以及庫存預測所需的設定。

| 欄位         | 型別          | 說明                    |
| ------------ | ------------- | ----------------------- |
| Id           | int           | 食材識別碼，Primary Key |
| Name         | nvarchar(max) | 食材名稱                |
| Unit         | nvarchar(max) | 食材使用單位            |
| UnitCost     | decimal(18,2) | 每單位成本              |
| MinimumStock | decimal(18,2) | 最低庫存量              |

`UnitCost` 與 `MinimumStock` 使用 `decimal(18,2)`，以支援成本及庫存數量的精確計算。

---

### 3.3 StockRecords

儲存所有庫存異動紀錄。

| 欄位         | 型別          | 說明                        |
| ------------ | ------------- | --------------------------- |
| id           | int           | 庫存異動識別碼，Primary Key |
| IngredientId | int           | 對應食材的 Foreign Key      |
| Type         | nvarchar(max) | 庫存異動類型，`IN` / `OUT`  |
| Quantity     | decimal(18,2) | 異動數量                    |
| Date         | datetime2     | 異動時間                    |

`StockRecords.IngredientId` 為 Foreign Key，對應 `Ingredients.Id`。

```text
StockRecords.IngredientId
          │
          ▼
Ingredients.Id
```

一個食材可以對應多筆庫存異動紀錄，因此兩者為：

```text
Ingredients 1 ──────── N StockRecords
```

庫存不直接儲存於 `Ingredients` 表中，而是根據 `StockRecords` 的異動資料計算目前庫存。

計算方式：

```text
目前庫存 = 所有 IN 數量 - 所有 OUT 數量
```

餐點出餐時產生的食材扣料會以 `OUT` 紀錄保存。

---

### 3.4 Meals

儲存餐點基本資料。

| 欄位 | 型別          | 說明                    |
| ---- | ------------- | ----------------------- |
| Id   | int           | 餐點識別碼，Primary Key |
| Name | nvarchar(max) | 餐點名稱                |

餐點本身只保存基本資訊，所需食材及數量則由 `MealIngredients` 管理。

---

### 3.5 MealIngredients

儲存餐點與食材之間的 BOM 關係。

| 欄位         | 型別          | 說明                    |
| ------------ | ------------- | ----------------------- |
| Id           | int           | BOM 識別碼，Primary Key |
| MealId       | int           | 對應餐點的 Foreign Key  |
| IngredientId | int           | 對應食材的 Foreign Key  |
| Quantity     | decimal(18,2) | 該餐點所需的食材數量    |

例如：

```text
MealId = 1
IngredientId = 2
Quantity = 2
```

代表某餐點需要 2 單位的指定食材。

系統在執行出餐時，會依照 `MealIngredients` 的資料計算需要扣除的食材數量。

---

### 3.6 ImportLog

儲存 Excel 食材匯入的執行結果。

| 欄位         | 型別          | 說明                        |
| ------------ | ------------- | --------------------------- |
| id           | int           | 匯入紀錄識別碼，Primary Key |
| UserName     | nvarchar(max) | 執行匯入的使用者            |
| FileName     | nvarchar(max) | 匯入檔案名稱                |
| SuccessCount | int           | 成功筆數                    |
| FailureCount | int           | 失敗筆數                    |
| TotalCount   | int           | 總筆數                      |
| ImportTime   | datetime2     | 匯入時間                    |
| IsSuccess    | bit           | 是否成功                    |
| ErrMsg       | nvarchar(max) | 錯誤訊息                    |

此資料表用於保留 Excel 匯入的歷程及執行結果，方便管理者查看匯入狀況。

目前 `ImportLog` 以 `UserName` 保存執行匯入的使用者名稱，未另外建立與 `Users` 的 Foreign Key 關係。

---

## 4. Database Relationships

### 4.1 Ingredients 與 StockRecords

`StockRecords.IngredientId` 對應 `Ingredients.Id`，並透過 Foreign Key 建立資料關係。

```text
Ingredients
     │
     │ 1 : N
     ▼
StockRecords
```

一個食材可以產生多筆庫存異動。

---

### 4.2 Meals 與 Ingredients

`Meals` 與 `Ingredients` 之間透過 `MealIngredients` 建立多對多關係。

```text
Meals
  │
  │ 1 : N
  ▼
MealIngredients
  ▲
  │ N : 1
  │
Ingredients
```

因此：

```text
Meal N ─────── N Ingredients
        │
        ▼
 MealIngredients
```

`MealIngredients.Quantity` 用來記錄每個餐點所需的食材數量。

---

### 4.3 Entity Framework Core Relationship

系統在 `InventoryDbContext` 中設定資料表之間的 Foreign Key 關係。

`StockRecords` 與 `Ingredients`：

```csharp
modelBuilder.Entity<StockRecord>()
    .HasOne(x => x.Ingredient)
    .WithMany()
    .HasForeignKey(x => x.IngredientId);
```

`MealIngredients` 與 `Meals`：

```csharp
modelBuilder.Entity<MealIngredient>()
    .HasOne<Meal>()
    .WithMany()
    .HasForeignKey(x => x.MealId);
```

`MealIngredients` 與 `Ingredients`：

```csharp
modelBuilder.Entity<MealIngredient>()
    .HasOne<Ingredients>()
    .WithMany()
    .HasForeignKey(x => x.IngredientId);
```

因此主要 Foreign Key 關係如下：

```text
StockRecords.IngredientId
        ↓
Ingredients.Id


MealIngredients.MealId
        ↓
Meals.Id


MealIngredients.IngredientId
        ↓
Ingredients.Id
```

其中 `StockRecord` 使用 `Ingredient` Navigation Property：

```csharp
public int IngredientId { get; set; }

public Ingredients Ingredient { get; set; } = null!;
```

這讓 Entity Framework Core 可以建立 `StockRecord` 與 `Ingredients` 之間的關聯。

---

## 5. Inventory Data Flow

資料庫中的庫存資料主要透過 `StockRecords` 保存異動歷程。

### 進貨

```text
使用者進貨
    ↓
StockRecord
Type = IN
    ↓
增加庫存
```

### 出餐

```text
使用者執行出餐
    ↓
MealIngredients
    ↓
取得餐點所需食材及數量
    ↓
產生 StockRecord
Type = OUT
    ↓
扣除庫存
```

### 庫存預測

```text
Ingredients
   │
   ├── MinimumStock
   └── UnitCost
        │
        ▼
StockRecords
   │
   └── 歷史 OUT 紀錄
        │
        ▼
ForecastService
        │
        ├── 平均每日使用量
        ├── 預測需求量
        ├── 建議補貨量
        └── 預估補貨成本
```

因此資料庫不需要額外儲存固定的「目前庫存」或「預測結果」，系統可以根據目前的庫存異動及食材設定即時計算。

---

## 6. Database Design Decisions

### 6.1 以庫存異動紀錄作為庫存資料來源

系統沒有直接在 `Ingredients` 中儲存目前庫存數量，而是透過 `StockRecords` 保存每一次 IN / OUT 異動。

這樣可以保留完整的庫存異動歷程，也能作為歷史使用量及庫存預測的資料來源。

---

### 6.2 使用 MealIngredients 管理 BOM

餐點與食材之間不是直接儲存多個食材欄位，而是透過 `MealIngredients` 建立關聯。

此設計可以讓不同餐點使用不同食材及數量，並在出餐時依照 BOM 自動計算需要扣除的庫存。

---

### 6.3 使用 decimal 儲存庫存及成本數值

食材成本、最低庫存量、庫存異動數量及 BOM 數量皆可能包含小數，因此相關欄位使用 `decimal(18,2)` 儲存，以避免使用浮點數造成計算精度問題。

---

### 6.4 預測結果由 Business Logic 即時計算

庫存預測結果並未直接儲存在資料庫中。

系統由 `ForecastService` 根據：

- 歷史庫存 OUT 紀錄
- 目前庫存
- 最低庫存量
- 單位成本
- 使用量期間
- 預測期間

即時計算平均每日使用量、預測需求量、建議補貨量及預估成本。

這樣可以避免資料庫中保存過期的預測結果，並讓相同輸入條件產生一致的計算結果。

---

## 7. Summary

本系統的資料庫設計以「食材、庫存異動及餐點 BOM」為核心。

`Ingredients` 儲存食材及庫存設定，`StockRecords` 保存實際庫存異動，`Meals` 與 `MealIngredients` 建立餐點與食材之間的 BOM 關係，而 `ImportLog` 則保存 Excel 匯入歷程。

整體資料流可簡化為：

```text
                 ┌──────────────┐
                 │ Ingredients  │
                 └──────┬───────┘
                        │
             ┌──────────┴──────────┐
             │                     │
             ▼                     ▼
      ┌─────────────┐     ┌─────────────────┐
      │ StockRecords│     │ MealIngredients │
      └──────┬──────┘     └────────┬────────┘
             │                     │
             │                     ▼
             │                 ┌─────────┐
             │                 │  Meals  │
             │                 └─────────┘
             │
             ▼
      ┌───────────────┐
      │ ForecastService│
      └───────┬───────┘
              │
              ▼
      Inventory Forecast
```

透過 Entity Framework Core 的 `InventoryDbContext`，後端 Business Logic 可以存取上述資料，並支援庫存計算、餐點出餐扣料、Excel 匯入及庫存預測等功能。
