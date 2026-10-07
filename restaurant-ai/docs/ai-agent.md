# AI Agent Design

## 1. AI Feature Overview

本系統提供「AI 庫存助理」，使用者可以透過自然語言詢問目前庫存、低庫存食材、庫存摘要及庫存預測等資訊。

AI 不直接存取資料庫，也不負責庫存數值計算，而是透過 Tool Calling 取得後端提供的實際資料，再整理成自然語言回答。

---

## 2. Inventory Tools

目前 AI Agent 提供三個 Inventory Tools：

| Tool                     | 用途                   |
| ------------------------ | ---------------------- |
| `GetLowStockItems()`     | 取得目前需要補貨的食材 |
| `GetInventoryForecast()` | 取得庫存預測及補貨結果 |
| `GetInventorySummary()`  | 取得整體庫存摘要       |

AI 會根據使用者問題選擇適合的 Tool。

例如：

```text
使用者：
「哪些食材需要補貨？」
        ↓
AI 選擇 GetLowStockItems()
        ↓
InventoryToolService
        ↓
ForecastService
        ↓
取得實際庫存資料
        ↓
Tool Result
        ↓
AI 產生自然語言回答
```

---

## 3. AI 與 Business Logic 的分工

本系統將 AI 與庫存計算邏輯分開。

### AI 負責

- 理解使用者問題
- 選擇適合的 Tool
- 將 Tool Result 整理成自然語言

### Backend 負責

- 查詢實際庫存資料
- 計算目前庫存
- 計算平均每日使用量
- 計算預測需求量
- 計算建議補貨量
- 計算預估補貨成本

其中庫存預測由 `ForecastService` 負責，AI 不直接計算庫存數值。

---

## 4. Design Decisions

### AI 不直接存取資料庫

AI 只能透過系統提供的 Inventory Tools 取得資料：

```text
Vue AI Chat
      ↓
AIController
      ↓
InventoryAgentService
      ↓
OpenAI Responses API
      ↓
InventoryToolService
      ↓
ForecastService
      ↓
InventoryDbContext / EF Core
      ↓
SQL Server
```

這樣可以限制 AI 可取得的資料範圍，並避免 AI 直接存取或操作資料庫。

### AI 不負責實際庫存計算

庫存及補貨數值由 Backend Business Logic 計算，再交由 AI 轉換成自然語言。

因此即使未來更換 AI 模型，原本的庫存計算邏輯仍可以繼續使用。

---

## Summary

本系統採用「AI + Tool Calling + Backend Business Logic」的方式實作 AI 庫存助理。

AI 負責自然語言理解與 Tool 選擇，Backend 負責取得及計算實際庫存資料，讓 AI 回覆能夠基於系統實際資料，而不是自行推測庫存數值。
