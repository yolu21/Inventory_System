# AI 餐飲庫存管理系統

結合庫存管理、進出貨紀錄、營運數據分析與 AI 輔助功能的全端 Web 應用程式。透過集中管理食材庫存與相關紀錄，協助使用者掌握庫存狀態，並探索以數據輔助餐飲營運決策的可能性。

> 專案性質：個人全端開發作品
> 開發目標：實作前後端分離架構，整合資料庫、API、圖表與 AI 輔助功能。

## 專案展示

- 線上 Demo：[待補]
- 系統操作影片：[待補]
- 專案文件：請參閱下方「文件」章節。

### 系統畫面

<!-- 後續可加入 Dashboard、庫存管理、庫存紀錄及 AI 功能的畫面截圖 -->

| Dashboard            | 庫存管理           |
| -------------------- | ------------------ |
| 待補：Dashboard 截圖 | 待補：庫存管理截圖 |

| 庫存紀錄           | AI 輔助功能              |
| ------------------ | ------------------------ |
| 待補：庫存紀錄截圖 | 待補：AI Chat 或預測畫面 |

## 功能介紹

### 1. 庫存管理

- 食材資料查詢與管理。
- 新增及刪除食材。
- 查看食材目前庫存量。
- 透過進貨與出貨紀錄追蹤庫存變化。

### 2. 庫存異動紀錄

- 記錄進貨（IN）與出貨（OUT）。
- 查詢庫存異動歷史。
- 依異動資料掌握食材使用情況。

### 3. Dashboard 與數據視覺化

- 顯示食材總數及進出貨相關統計。
- 提醒低庫存食材。
- 以圖表呈現庫存及使用狀況。

### 4. AI 與營運分析

- AI 對話功能。
- 食材使用量與需求預測相關功能。
- 以營運數據分析輔助庫存管理。

> 請依照目前實際完成的功能調整以上項目，並清楚區分已完成與開發中功能。

## 技術架構

| 類別          | 技術                               |
| ------------- | ---------------------------------- |
| 前端          | Vue 3、Vite、JavaScript            |
| 前端狀態管理  | Pinia                              |
| 前端路由      | Vue Router                         |
| HTTP API 串接 | Axios                              |
| 資料視覺化    | Chart.js、vue-chartjs              |
| 後端          | C#、ASP.NET Core Web API（.NET 8） |
| ORM           | Entity Framework Core              |
| 資料庫        | Microsoft SQL Server               |
| API 測試      | Swagger / OpenAPI                  |
| 版本控制      | Git、GitHub                        |

## 系統架構

本系統採用前後端分離架構：

1. 使用者透過 Vue 前端操作系統。
2. 前端透過 HTTP API 向 ASP.NET Core 後端發送請求。
3. 後端透過 Entity Framework Core 存取 SQL Server。
4. 後端將處理結果以 API Response 回傳前端。
5. 前端依據回傳資料更新畫面及圖表。

```text
User
  |
  v
Vue 3 Frontend
  |
  | HTTP / REST API
  v
ASP.NET Core Web API
  |
  | Entity Framework Core
  v
SQL Server
```

## 專案結構

```text
Inventory_System/
├── README.md
├── .gitignore
├── InventorySys/
│   ├── InventorySys.slnx
│   └── InventorySys/
│       ├── Controllers/
│       ├── DTOs/
│       ├── Data/
│       ├── Migrations/
│       ├── Models/
│       ├── Services/
│       └── InventorySys.csproj
└── restaurant-ai/
    ├── package.json
    ├── src/
    │   ├── services/
    │   ├── stores/
    │   └── views/
    └── docs/
```

## 本機開發環境

### 前置需求

- Node.js（請依實際 Vite 版本確認支援的版本）
- .NET 8 SDK
- SQL Server LocalDB 或相容的 SQL Server 執行環境
- Git

### 1. 取得專案

```bash
git clone https://github.com/yolu21/Inventory_System.git
cd Inventory_System
```

### 2. 設定後端

請先確認後端專案路徑：

```text
InventorySys/InventorySys/InventorySys.csproj
```

在 `InventorySys/InventorySys/appsettings.json` 設定適用於本機的資料庫連線資訊，並確認 JWT 金鑰使用安全的設定來源，不要將金鑰提交至 Git。

設定 JWT 金鑰的本機 User Secrets：

```bash
dotnet user-secrets set "Jwt:Key" "<your-development-key>" --project InventorySys/InventorySys/InventorySys.csproj
```

建立或更新資料庫的方式，請依專案目前的 Entity Framework Core migrations 設定執行。

啟動後端：

```bash
dotnet run --project InventorySys/InventorySys/InventorySys.csproj
```

請依啟動時顯示的網址開啟 API 或 Swagger（若已啟用）。

### 3. 設定前端

```bash
cd restaurant-ai
npm install
npm run dev
```

依照 Vite 顯示的網址開啟前端。

> 注意：實際資料庫初始化方式、API 網址、環境變數與登入設定，請以目前專案設定為準。

## 專案文件

目前文件位於 `restaurant-ai/docs/`：

- `requirements.md`：需求規格。
- `database-design.md`：資料庫設計。
- `system-architecture.md`：系統架構。
- `ai-agent.md`：AI Agent 相關設計。
- `development-notes.md`：開發紀錄與問題處理。

## 開發過程中的重點

本專案除了實作前端畫面，也涵蓋以下全端開發工作：

- 前後端 API 設計與串接。
- 資料庫模型與資料存取。
- 庫存異動及統計資料處理。
- 前端路由、狀態管理與圖表呈現。
- 整理需求、資料庫與系統架構文件。

## 後續規劃

- 改善不同螢幕尺寸下的操作體驗。
- 完善錯誤處理與資料驗證。
- 持續改善 AI 分析與預測功能。
- 補充測試、部署與使用說明。

## License

目前尚未指定授權條款。若計畫公開原始碼供他人使用，可依需求選擇合適的開源授權。
