# Development Notes

本文件記錄本系統開發過程中的實作思考、遇到的問題，以及開發過程中對技術與系統設計的理解。

---

## 1. 庫存與出餐扣料設計

### Design Considerations

餐點出餐時可能會同時使用多種食材，因此在設計出餐功能時，需要先確認相關食材的庫存是否足夠。

如果其中一項食材庫存不足，則不應允許出餐，避免只扣除部分食材而造成庫存資料不一致。

### Implementation

出餐前會依照餐點的 BOM（Bill of Materials）計算所需食材及數量，並確認各食材目前庫存是否足夠。

若任一食材不足，則阻止出餐並提示使用者。

若所有食材庫存皆足夠，才建立對應的 OUT 庫存異動紀錄。

此外，出餐涉及多筆庫存異動，因此使用 Database Transaction，確保整個操作成功或全部取消。

### Learning

這次實作讓我了解到，除了功能本身可以正常執行之外，也需要考慮操作失敗時資料是否會維持一致。

---

## 2. 庫存預測：從 AI 分析改為後端計算

### Initial Approach

最初規劃庫存預測功能時，曾考慮讓 AI 分析歷史庫存資料並產生預測結果。

### Problem

實際分析需求後發現，目前的預測邏輯可以透過既有的庫存異動資料直接計算，例如：

- 計算指定期間的 OUT 數量
- 計算平均每日使用量
- 計算未來期間的預測需求
- 根據目前庫存與最低庫存量計算建議補貨量

這些都是具有明確計算規則的數值，因此不需要由 AI 進行推測。

### Final Design

最後將庫存預測邏輯集中在 Backend 的 `ForecastService` 中。

基本流程為：

```text
StockRecords
      ↓
計算指定期間 OUT 數量
      ↓
平均每日使用量
      ↓
預測需求量
      ↓
建議補貨量
      ↓
預估補貨成本
```

AI 則透過 Inventory Tools 取得已計算好的結果，再將結果整理成自然語言回答。

### Learning

這次調整讓我了解到，AI 不一定適合負責所有分析工作。

對於具有明確 Business Rules 的數值計算，使用程式邏輯處理會比較容易維持一致性及可預期性，而 AI 更適合處理自然語言理解與互動。

---

## 3. Entity 與 DTO 的區分

### Initial Approach

一開始設計 API 時，沒有特別區分 Entity 與 DTO，認為資料可以直接使用 Model 傳遞。

隨著功能增加，發現不同 API 需要的資料內容並不完全相同，因此開始將 API 回傳資料拆分成不同 DTO。

### Final Design

目前會依照 API 的用途建立不同 DTO，例如：

```text
Entity
  ↓
Business Logic
  ↓
DTO
  ↓
API Response
```

例如庫存預測相關資料使用 `InventoryForecastDto`，庫存摘要則使用 `InventorySummaryDto`。

### Learning

這次實作讓我了解到，Database Entity 與 API Response 的用途不同。

Entity 主要描述資料庫中的資料結構，而 DTO 可以依照 API 實際需求決定要回傳哪些資料，避免所有 API 都直接使用相同的資料結構。

---

## 4. Entity Framework Core 與 LINQ

### Development Experience

這是第一次較完整地使用 Entity Framework Core，因此一開始對資料查詢的執行方式並不熟悉。

EF Core 可以透過 `DbContext` 與 Entity 操作資料庫，並使用 LINQ 進行資料查詢。

例如：

```csharp
var ingredients = await _context.Ingredients
    .Where(x => x.MinimumStock > 0)
    .ToListAsync();
```

這種寫法與 SQL 的查詢概念相似，但實際上是使用 C# / LINQ 的語法來描述查詢。

### Problem

開發過程中也遇到部分 LINQ 查詢無法直接被 EF Core 轉換成 SQL 的情況。

這讓我了解到：

> LINQ 雖然可以用類似 SQL 的方式思考資料查詢，但並不是所有 C# / LINQ 寫法都能直接轉換成資料庫可以執行的 SQL。

因此在使用 EF Core 時，需要同時理解 LINQ 的寫法以及 EF Core 如何將查詢轉換成 SQL。

### Learning

透過這次專案，我開始理解：

```text
C# / LINQ
    ↓
Entity Framework Core
    ↓
SQL
    ↓
Database
```

也更清楚 `DbContext` 在資料存取層中所扮演的角色。

---

## 5. Foreign Key 與資料表關聯

### Initial Approach

一開始設計 `StockRecord` 時，只在資料表中保存：

```csharp
public int IngredientId { get; set; }
```

當時認為只要透過 `IngredientId` 對應 `Ingredients.Id` 即可。

### Problem

後來在學習資料庫關聯時，才了解到：

「程式中保存一個可以對應的 ID」與「資料庫正式建立 Foreign Key 關係」是不同的概念。

因此後來將 `StockRecord` 與 `Ingredients` 建立正式的 Foreign Key relationship。

### Final Design

```text
Ingredients
    │
    │ Id
    ▼
StockRecords
 IngredientId
```

並在 EF Core 中設定：

```csharp
modelBuilder.Entity<StockRecord>()
    .HasOne(x => x.Ingredient)
    .WithMany()
    .HasForeignKey(x => x.IngredientId);
```

### Learning

這次實作讓我開始理解 Foreign Key 的用途，以及資料庫不只是單純存放資料，也可以透過關聯限制資料之間的關係。

---

## 6. Vue 資料綁定與 Template Rendering

### Development Experience

在開發 Vue 前端時，一開始比較著重於如何將 API 取得的資料顯示在畫面上。

基本流程為：

```text
API
 ↓
JavaScript Data
 ↓
Vue Template
 ↓
UI
```

例如從 API 取得食材資料後，可以透過 `v-for` 將陣列中的資料產生為表格：

```vue
<tr v-for="item in ingredients" :key="item.id">
    <td>{{ item.name }}</td>
    <td>{{ item.stock }}</td>
</tr>
```

其中 `v-for` 用來重複產生畫面元素，而 `{{ }}` 則用來將 JavaScript 中的資料顯示在 Template 中。

### Learning

透過這次開發，我逐漸理解 Vue 的資料綁定方式，以及前端資料與畫面之間的關係。

相較於直接操作 DOM，Vue 可以讓資料狀態與畫面保持關聯，當資料改變時，畫面也可以跟著更新。

---

## 7. Development Summary

這次專案開發過程中，除了完成實際功能，也逐漸理解前後端系統中不同層次的責任。

目前對整體系統的理解為：

```text
Vue
 ↓
API
 ↓
Controller
 ↓
Business Logic / Service
 ↓
EF Core / DbContext
 ↓
SQL Server
```

而 AI 功能則建立在既有 Business Logic 之上：

```text
User Question
      ↓
AI
      ↓
Tool Calling
      ↓
Backend Service
      ↓
Actual Data
      ↓
AI Natural Language Response
```

透過這次專案，我也逐漸了解到，開發不只是將功能完成，也需要考慮資料一致性、程式責任分離、資料結構設計，以及不同技術之間如何協作。
