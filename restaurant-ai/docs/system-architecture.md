# System Architecture

## 1. Architecture Overview

本系統採用前後端分離架構，前端使用 Vue 3 建立使用者介面，後端使用 ASP.NET Core Web API 處理業務邏輯與資料存取，資料庫使用 SQL Server 儲存系統資料。

AI 庫存助理則透過 OpenAI Responses API 與後端 Inventory Tools 整合，讓 AI 負責自然語言理解與 Tool 選擇，實際庫存及預測資料則由後端既有 Business Logic 與資料庫提供。

整體架構如下：

```text
┌─────────────────────────────────────────────┐
│                  Frontend                   │
│                  Vue 3                      │
│                                             │
│  Views / Components / Pinia / Vue Router    │
│                    │                        │
│                  Axios                      │
└────────────────────┼────────────────────────┘
                     │ HTTP / REST API
                     ▼
┌─────────────────────────────────────────────┐
│                  Backend                    │
│              ASP.NET Core Web API           │
│                                             │
│  Controllers                                │
│      │                                      │
│      ├── Authentication / Authorization     │
│      │                                      │
│      ▼                                      │
│  Services                                   │
│      │                                      │
│      ├── Inventory Business Logic           │
│      ├── ForecastService                    │
│      ├── InventoryToolService               │
│      └── InventoryAgentService              │
│      │                                      │
│      ▼                                      │
│  Entity Framework Core                      │
└───────────────┬─────────────────────────────┘
                │
                ▼
┌─────────────────────────────┐
│         SQL Server          │
│                             │
│ Ingredients / StockRecords  │
│ Meals / BOM / Users / Logs  │
└─────────────────────────────┘
```

```
AI Assistant Flow

Vue AI Chat
      │
      ▼
AIController
      │
      ▼
InventoryAgentService
      │
      ▼
OpenAI Responses API
      │
      │ Tool Calling
      ▼
InventoryToolService
      │
      ├── GetLowStockItems()
      ├── GetInventoryForecast()
      └── GetInventorySummary()
      │
      ▼
ForecastService
      │
      ▼
InventoryDbContext / EF Core
      │
      ▼
SQL Server
```

---

## 2. Technology Stack

| Layer             | Technology                | Purpose                  |
| ----------------- | ------------------------- | ------------------------ |
| Frontend          | Vue 3                     | 建立使用者介面           |
| Frontend Build    | Vite                      | 前端開發與建置           |
| Routing           | Vue Router                | 頁面路由管理             |
| State Management  | Pinia                     | 管理前端共享狀態         |
| HTTP Client       | Axios                     | 呼叫後端 API             |
| Backend           | ASP.NET Core Web API      | 提供 REST API            |
| ORM               | Entity Framework Core     | 資料庫存取               |
| Database          | SQL Server                | 儲存系統資料             |
| Authentication    | JWT                       | 使用者身份驗證           |
| Password Security | BCrypt                    | 使用者密碼雜湊           |
| API Documentation | Swagger                   | API 測試與文件           |
| AI                | OpenAI Responses API      | AI Agent 與 Tool Calling |
| Excel             | SheetJS / ExcelDataReader | Excel 資料匯入與處理     |
| Charts            | Chart.js                  | Dashboard 圖表呈現       |

---

## 3. Frontend Architecture

前端採用 Vue 3 開發，依照頁面、元件、狀態及 API Service 分離不同責任。

目前主要結構如下：

```text
restaurant-ai/
├── src/
│   ├── components/
│   │
│   ├── views/
│   │   ├── AIChat.vue
│   │   ├── Dashboard.vue
│   │   ├── Forecast.vue
│   │   ├── History.vue
│   │   ├── Inventory.vue
│   │   ├── Login.vue
│   │   └── Meal.vue
│   │
│   ├── stores/
│   │   ├── Dashboard.js
│   │   ├── history.js
│   │   ├── inventory.js
│   │   └── Meal.js
│   │
│   ├── services/
│   │   └── api.js
│   │
│   ├── router/
│   │   └── index.js
│   │
│   ├── App.vue
│   ├── main.js
│   └── style.css
```

### 3.1 Views

`views` 負責主要頁面的呈現與使用者操作。

主要頁面包含：

- `Login.vue`：使用者登入
- `Dashboard.vue`：目前庫存及統計資訊
- `Inventory.vue`：食材與庫存管理
- `Forecast.vue`：庫存預測與補貨建議
- `History.vue`：歷史庫存及匯入記錄
- `Meal.vue`：餐點與 BOM 管理、出餐
- `AIChat.vue`：AI 庫存助理

### 3.2 Components

`components` 用於存放可重複使用的 Vue 元件，避免將所有 UI 與操作邏輯集中於單一頁面。

### 3.3 Pinia Stores

Pinia 用於管理需要在不同元件或頁面之間共享的資料。

目前依照功能拆分 Store，例如：

- Dashboard
- Inventory
- History
- Meal

藉此將部分資料狀態與頁面呈現分離。

### 3.4 Vue Router

Vue Router 負責前端頁面路由管理，並搭配登入狀態與角色權限限制頁面存取。

主要路由包含：

```text
/login
/dashboard
/inventory
/forecast
/history
/meal
/ai-chat
```

Admin 限定頁面則由角色進行存取控制。

---

## 4. Backend Architecture

後端使用 ASP.NET Core Web API，採 Controller + Service 的分層方式設計。

主要結構如下：

```text
InventorySys/
├── Controllers/
│   ├── AIController.cs
│   ├── AuthController.cs
│   ├── DashboardController.cs
│   ├── ForecastController.cs
│   ├── ImportController.cs
│   ├── IngredientController.cs
│   ├── MealController.cs
│   ├── StockController.cs
│   └── ToolController.cs
│
├── Data/
│   └── InventoryDbContext.cs
│
├── DTOs/
│
├── Models/
│
├── Services/
│   ├── ForecastService.cs
│   ├── InventoryAgentService.cs
│   └── InventoryToolService.cs
│
├── Migrations/
│
└── Program.cs
```

### 4.1 Controllers

Controller 負責接收 HTTP Request、驗證基本輸入及呼叫對應的 Service。

主要 Controller 與功能：

| Controller           | Responsibility          |
| -------------------- | ----------------------- |
| AuthController       | 登入及身份驗證          |
| IngredientController | 食材資料管理            |
| StockController      | 庫存 IN / OUT           |
| MealController       | 餐點與 BOM、出餐        |
| DashboardController  | Dashboard 資料          |
| ForecastController   | 庫存預測                |
| ImportController     | Excel 匯入              |
| AIController         | AI Agent 對話           |
| ToolController       | Inventory Tool 相關 API |

Controller 本身不負責複雜的庫存計算，而是將相關邏輯交由 Service 處理。

---

## 5. Business Logic Layer

系統將主要 Business Logic 放在 Service Layer，避免將複雜邏輯直接寫在 Controller 中。

### 5.1 ForecastService

`ForecastService` 負責庫存預測及補貨計算。

主要處理：

1. 取得目前庫存
2. 取得指定期間內的 OUT 使用量
3. 計算平均每日使用量
4. 計算預測需求量
5. 計算建議補貨量
6. 計算預估補貨成本

計算流程：

```text
Stock Records
      │
      ▼
計算目前庫存
      │
      ▼
取得歷史 OUT 使用量
      │
      ▼
平均每日使用量
      │
      ▼
預測需求量
      │
      ▼
建議補貨量
      │
      ▼
預估補貨成本
```

庫存預測由後端程式依照既定 Business Rules 計算，而不是交由 AI 自行推算。

---

## 6. Inventory Tool Layer

AI 庫存助理並不直接存取資料庫，而是透過 `InventoryToolService` 提供標準化的資料取得方式。

目前提供主要 Inventory Tools：

```text
InventoryToolService
│
├── GetLowStockItems()
│
├── GetInventoryForecast()
│
└── GetInventorySummary()
```

各 Tool 的責任如下：

| Tool                 | Purpose                |
| -------------------- | ---------------------- |
| GetLowStockItems     | 取得需要補貨的食材     |
| GetInventoryForecast | 取得庫存預測及補貨建議 |
| GetInventorySummary  | 取得整體庫存摘要       |

Tool 會呼叫既有的 `ForecastService`，因此 AI 所取得的庫存資料與一般庫存預測頁面使用相同的 Business Logic。

這可以避免 AI 與系統本身存在兩套不同的庫存計算方式。

---

## 7. AI Agent Architecture

AI 庫存助理採用 Tool Calling 架構。

AI 本身不直接計算或推測庫存數值，而是根據使用者問題選擇適合的 Inventory Tool。

完整流程如下：

```text
User
 │
 ▼
Vue AIChat.vue
 │
 ▼
AIController
 │
 ▼
InventoryAgentService
 │
 ▼
OpenAI Responses API
 │
 ├── 判斷使用者問題
 │
 └── 選擇 Inventory Tool
 │
 ▼
InventoryToolService
 │
 ▼
ForecastService / Database
 │
 ▼
Tool Result
 │
 ▼
OpenAI Responses API
 │
 ▼
自然語言回答
 │
 ▼
Vue AIChat.vue
```

例如使用者詢問：

> 「哪些食材需要補貨？」

AI 會根據問題選擇：

```text
get_low_stock_items
```

後端取得實際資料後，再將 Tool Result 提供給 AI 產生自然語言回答。

因此 AI 的角色主要為：

- 理解使用者問題
- 選擇適當 Tool
- 整理 Tool 回傳資料
- 產生自然語言回答

而不是：

- 自行產生庫存數量
- 自行計算補貨數量
- 自行推測資料庫內容

---

## 8. Authentication & Authorization

系統使用 JWT 作為身份驗證機制。

登入流程：

```text
User
 │
 ▼
Login.vue
 │
 ▼
Auth API
 │
 ▼
驗證帳號密碼
 │
 ▼
產生 JWT Token
 │
 ▼
Frontend
 │
 ▼
Local Storage
 │
 ▼
後續 API Request
 │
 ▼
JWT 驗證
```

Token 中包含使用者角色資訊，後端根據角色判斷 API 是否允許存取。

目前主要角色：

```text
Admin
 ├── Inventory Management
 ├── History
 ├── Meal Management
 └── 其他一般功能

User
 ├── Dashboard
 ├── Forecast
 ├── Meal Serving
 └── AI Assistant
```

前端會根據角色隱藏不具權限的功能入口，後端 API 則負責實際權限驗證。

---

## 9. Data Access

資料庫存取使用 Entity Framework Core。

主要流程：

```text
Controller
    ↓
Service
    ↓
InventoryDbContext
    ↓
Entity Framework Core
    ↓
SQL Server
```

`InventoryDbContext` 負責管理 Entity 與資料庫之間的對應關係。

主要資料包含：

- User
- Ingredient
- StockRecord
- Meal
- MealIngredient / BOM
- ImportLog

實際資料表與 Entity 關係另於 `database-design.md` 說明。

---

## 10. Meal Serving Transaction

餐點出餐可能同時影響多項食材庫存，因此系統使用 Database Transaction 確保資料一致性。

流程如下：

```text
使用者選擇餐點
      ↓
輸入出餐數量
      ↓
取得 BOM
      ↓
計算各食材需求量
      ↓
檢查庫存
      ↓
┌───────────────────┐
│ 庫存是否足夠？     │
└─────────┬─────────┘
          │
     ┌────┴────┐
     │         │
    否         是
     │         │
     ▼         ▼
  阻止出餐    建立 OUT Records
                │
                ▼
          Transaction Commit
```

如果其中任一項庫存異動失敗，整筆出餐操作應回滾，避免產生部分庫存更新。

---

## 11. Architecture Design Decisions

### 11.1 AI 不負責庫存預測

原始規劃曾考慮使用 AI 分析歷史資料並進行庫存預測。

實作過程中發現庫存預測屬於明確且可定義的 Business Rules，因此改由 `ForecastService` 負責計算。

這樣可以：

- 確保計算結果一致
- 避免 AI 產生不確定數值
- 方便測試與驗證
- 讓 AI 專注於自然語言互動

### 11.2 AI 透過 Tools 取得資料

AI 不直接存取資料庫，而是透過 Inventory Tools 取得資料。

這樣可以：

- 限制 AI 可使用的資料範圍
- 避免 AI 直接操作資料庫
- 重複使用既有 Business Logic
- 確保 AI 回覆使用系統實際資料

### 11.3 Controller 與 Business Logic 分離

Controller 主要負責 API Request / Response，複雜邏輯則集中於 Service。

例如庫存預測由 `ForecastService` 處理，而不是直接寫在 `ForecastController` 中。

此設計可降低程式碼耦合，並提升後續維護及測試的便利性。

---

## 12. Summary

本系統採用 Vue 3 + ASP.NET Core Web API + SQL Server 的前後端分離架構，並透過 Service Layer 集中處理主要 Business Logic。

AI 庫存助理則以 Tool Calling 方式整合既有庫存功能，使 AI 負責自然語言互動，而實際庫存及預測資料仍由後端 Business Logic 與資料庫提供。

整體設計的核心原則為：

> **UI 負責互動、API 負責系統介接、Service 負責 Business Logic、Database 負責資料保存、AI 負責自然語言互動與 Tool 選擇。**
