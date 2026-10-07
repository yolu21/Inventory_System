<script setup>
import { ref, onMounted, computed } from "vue";
import { api } from "../services/api";
import { useInventoryStore } from "../stores/inventory";
import * as XLSX from "xlsx";

const inventoryStore = useInventoryStore();

/* ========================================
   Form
======================================== */

const newName = ref("");
const newUnit = ref("");
const newMinimumStock = ref("");
const unitCost = ref("");

/* ========================================
   Filter / Search
======================================== */

const keyword = ref("");
const stockFilter = ref("all");
const sortBy = ref("name");

/* ========================================
   Excel Import
======================================== */

const fileName = ref("");
const importData = ref([]);
const importErrors = ref([]);
const fileInput = ref(null);

/* ========================================
   Filtered Ingredients
======================================== */

const filteredIngredients = computed(() => {
  let list = [...inventoryStore.inventory];

  // Search
  if (keyword.value) {
    const searchKeyword = keyword.value.toLowerCase();

    list = list.filter((item) =>
      item.name.toLowerCase().includes(searchKeyword),
    );
  }

  // Stock filter
  if (stockFilter.value === "low") {
    list = list.filter((item) => item.stock < item.minimumStock);
  }

  if (stockFilter.value === "normal") {
    list = list.filter((item) => item.stock >= item.minimumStock);
  }

  // Sort
  switch (sortBy.value) {
    case "stock":
      list.sort((a, b) => b.stock - a.stock);
      break;

    case "name":
      list.sort((a, b) => a.name.localeCompare(b.name));
      break;
  }

  return list;
});

/* ========================================
   Ingredient
======================================== */

const addIngredient = async () => {
  if (!newName.value || !newUnit.value) {
    alert("請輸入食材名稱與單位");
    return;
  }

  await inventoryStore.addIngredient({
    name: newName.value,
    unit: newUnit.value,
    minimumStock: newMinimumStock.value || 0,
    unitCost: unitCost.value || 0,
  });

  newName.value = "";
  newUnit.value = "";
  newMinimumStock.value = "";
  unitCost.value = "";
};

const deleteIngredient = async (item) => {
  const confirmed = confirm(`確定要刪除「${item.name}」嗎？`);

  if (!confirmed) return;

  await inventoryStore.deleteIngredient(item.id);
};

const addStock = async (item) => {
  await inventoryStore.addStock(item);
};

const removeStock = async (item) => {
  await inventoryStore.removeStock(item);
};

/* ========================================
   Excel Export
======================================== */

const exportExcel = () => {
  const data = inventoryStore.inventory.map((item) => ({
    食材名稱: item.name,
    單位: item.unit,
    庫存: item.stock,
    最低庫存: item.minimumStock,
    單價: item.unitCost,
  }));

  const worksheet = XLSX.utils.json_to_sheet(data);

  const workbook = XLSX.utils.book_new();

  XLSX.utils.book_append_sheet(workbook, worksheet, "Inventory");

  XLSX.writeFile(workbook, "inventory.xlsx");
};

/* ========================================
   Excel Import
======================================== */

const importExcel = (event) => {
  const file = event.target.files[0];

  if (!file) return;

  fileName.value = file.name;

  const reader = new FileReader();

  reader.onload = (e) => {
    const data = new Uint8Array(e.target.result);

    const workbook = XLSX.read(data, {
      type: "array",
    });

    const sheet = workbook.Sheets[workbook.SheetNames[0]];

    const excelData = XLSX.utils.sheet_to_json(sheet);

    const json = excelData.map((item) => ({
      Name: item["食材名稱"]?.trim(),

      Unit: item["單位"]?.trim(),

      Stock: Number(item["庫存"]),

      MinimumStock: Number(item["最低庫存"] ?? 0),

      UnitCost: Number(item["單價"] ?? 0),
    }));

    const errors = validateImportData(json);

    importErrors.value = errors;

    if (errors.length > 0) {
      alert(`匯入失敗，請檢查以下錯誤:\n${errors.join("\n")}`);

      clearImport();
      return;
    }

    importData.value = json;

    alert("匯入成功，請確認資料後點擊儲存。");
  };

  reader.readAsArrayBuffer(file);
};

/* ========================================
   Save Excel
======================================== */

const saveExcel = async () => {
  if (importData.value.length === 0) {
    alert("沒有匯入資料，請先匯入 Excel。");

    return;
  }

  try {
    const res = await api.post("/Import", {
      FileName: fileName.value,
      Data: importData.value,
    });

    alert(
      `匯入成功。

新增食材: ${res.data.newIngredients} 筆
新增庫存: ${res.data.stockRecords} 筆`,
    );

    await inventoryStore.loadIngredients();

    clearImport();
  } catch (error) {
    console.error("匯入失敗:", error);

    alert(`匯入失敗\n${error.response?.data?.error || error.message}`);
  }
};

/* ========================================
   Clear Import
======================================== */

const clearImport = () => {
  importData.value = [];
  importErrors.value = [];
  fileName.value = "";

  if (fileInput.value) {
    fileInput.value.value = null;
  }
};

/* ========================================
   Validate Excel
======================================== */

const validateImportData = (data) => {
  const errors = [];

  data.forEach((item, index) => {
    // Excel 第一列為標題，因此資料從第 2 列開始
    const row = index + 2;

    if (!item.Name) {
      errors.push(`第 ${row} 列未填寫食材名稱`);
    }

    if (!item.Unit) {
      errors.push(`第 ${row} 列未填寫單位`);
    }

    if (item.Stock === undefined || item.Stock === "") {
      errors.push(`第 ${row} 列 ${item.Name} 未填寫進貨數量`);
    } else if (isNaN(item.Stock)) {
      errors.push(`第 ${row} 列 ${item.Name} 進貨數量必須為數字`);
    } else if (item.Stock <= 0) {
      errors.push(`第 ${row} 列 ${item.Name} 進貨數量必須大於 0`);
    }
  });

  return errors;
};

/* ========================================
   Lifecycle
======================================== */

onMounted(async () => {
  await inventoryStore.loadIngredients();
});
</script>

<template>
  <div class="inventory-page">
    <!-- ====================================
         Page Header
    ===================================== -->

    <header class="page-header">
      <div>
        <h1 class="page-title">Inventory Management</h1>

        <p class="page-description">
          Manage ingredients, stock levels and inventory data.
        </p>
      </div>

      <div class="page-actions">
        <button
          class="btn btn-secondary"
          type="button"
          @click="fileInput?.click()"
        >
          Import Excel
        </button>

        <button class="btn btn-secondary" type="button" @click="exportExcel">
          Export Excel
        </button>
      </div>
    </header>

    <!-- ====================================
         Add Ingredient
    ===================================== -->

    <section class="card add-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">新增食材</h2>

          <p class="card-description">Add a new ingredient to the inventory.</p>
        </div>
      </div>

      <div class="card-body">
        <div class="ingredient-form">
          <div class="form-group">
            <label class="form-label"> 食材名稱 </label>

            <input
              v-model="newName"
              class="form-input"
              placeholder="例如：番茄"
            />
          </div>

          <div class="form-group">
            <label class="form-label"> 單位 </label>

            <input
              v-model="newUnit"
              class="form-input"
              placeholder="例如：kg"
            />
          </div>

          <div class="form-group">
            <label class="form-label"> 最低庫存 </label>

            <input
              v-model="newMinimumStock"
              class="form-input"
              type="number"
              min="0"
              placeholder="0"
            />
          </div>

          <div class="form-group">
            <label class="form-label"> 單價 </label>

            <input
              v-model="unitCost"
              class="form-input"
              type="number"
              min="0"
              placeholder="0"
            />
          </div>

          <div class="form-group add-button-group">
            <label class="form-label"> &nbsp; </label>

            <button
              class="btn btn-primary"
              type="button"
              @click="addIngredient"
            >
              + 新增食材
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- ====================================
         Search / Filter
    ===================================== -->

    <section class="card filter-card">
      <div class="card-body">
        <div class="filter-row">
          <div class="search-wrapper">
            <span class="search-icon"> 🔍 </span>

            <input
              v-model="keyword"
              class="form-input search-input"
              placeholder="搜尋食材名稱..."
            />
          </div>

          <select v-model="stockFilter" class="form-select filter-select">
            <option value="all">全部庫存</option>

            <option value="low">低庫存</option>

            <option value="normal">正常庫存</option>
          </select>

          <select v-model="sortBy" class="form-select filter-select">
            <option value="name">依名稱排序</option>

            <option value="stock">依庫存排序</option>
          </select>
        </div>
      </div>
    </section>

    <!-- ====================================
         Excel Import Preview
    ===================================== -->

    <section
      v-if="importData.length > 0 || importErrors.length > 0"
      class="card import-card"
    >
      <div class="card-header">
        <div>
          <h2 class="card-title">Excel 匯入</h2>

          <p class="card-description">確認資料後再儲存至資料庫。</p>
        </div>

        <button class="btn btn-ghost" type="button" @click="clearImport">
          清除
        </button>
      </div>

      <!-- Errors -->

      <div v-if="importErrors.length > 0" class="import-errors">
        <div class="error-title">匯入資料有錯誤</div>

        <ul>
          <li v-for="(error, index) in importErrors" :key="index">
            {{ error }}
          </li>
        </ul>
      </div>

      <!-- Selected File -->

      <div v-if="fileName" class="selected-file">
        <span> 📄 {{ fileName }} </span>

        <span> {{ importData.length }} 筆資料 </span>
      </div>

      <!-- Preview -->

      <div v-if="importData.length > 0" class="table-wrapper">
        <table class="table">
          <thead>
            <tr>
              <th>食材名稱</th>
              <th>單位</th>
              <th>庫存</th>
              <th>最低庫存</th>
              <th>單價</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="(item, index) in importData" :key="index">
              <td>
                {{ item.Name }}
              </td>

              <td>
                {{ item.Unit }}
              </td>

              <td>
                {{ item.Stock }}
              </td>

              <td>
                {{ item.MinimumStock }}
              </td>

              <td>
                {{ item.UnitCost }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Import Actions -->

      <div v-if="importData.length > 0" class="import-footer">
        <button class="btn btn-secondary" type="button" @click="clearImport">
          取消
        </button>

        <button class="btn btn-primary" type="button" @click="saveExcel">
          儲存匯入資料
        </button>
      </div>
    </section>

    <!-- ====================================
         Inventory Table
    ===================================== -->

    <section class="card inventory-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">食材庫存</h2>

          <p class="card-description">
            {{ filteredIngredients.length }} 項食材
          </p>
        </div>
      </div>

      <div class="table-wrapper">
        <table class="table">
          <thead>
            <tr>
              <th>食材名稱</th>
              <th>單位</th>
              <th>目前庫存</th>
              <th>最低庫存</th>
              <th>單價</th>
              <th>庫存異動</th>
              <th>操作</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="item in filteredIngredients" :key="item.id">
              <!-- Name -->

              <td>
                <strong>
                  {{ item.name }}
                </strong>
              </td>

              <!-- Unit -->

              <td>
                {{ item.unit }}
              </td>

              <!-- Stock -->

              <td>
                <div class="stock-cell">
                  <strong>
                    {{ item.stock }}
                  </strong>

                  <span
                    v-if="item.stock < item.minimumStock"
                    class="badge badge-danger"
                  >
                    低庫存
                  </span>

                  <span v-else class="badge badge-success"> 正常 </span>
                </div>
              </td>

              <!-- Minimum Stock -->

              <td>
                {{ item.minimumStock }}
                {{ item.unit }}
              </td>

              <!-- Unit Cost -->

              <td>${{ item.unitCost }} / {{ item.unit }}</td>

              <!-- Stock IN / OUT -->

              <td>
                <div class="stock-action">
                  <input
                    v-model="item.amount"
                    class="form-input quantity-input"
                    type="number"
                    min="0"
                    placeholder="數量"
                  />

                  <button
                    class="btn btn-stock-in"
                    type="button"
                    @click="addStock(item)"
                  >
                    + IN
                  </button>

                  <button
                    class="btn btn-stock-out"
                    type="button"
                    @click="removeStock(item)"
                  >
                    - OUT
                  </button>
                </div>
              </td>

              <!-- Delete -->

              <td>
                <button
                  class="btn btn-danger"
                  type="button"
                  @click="deleteIngredient(item)"
                >
                  刪除
                </button>
              </td>
            </tr>

            <!-- Empty -->

            <tr v-if="filteredIngredients.length === 0">
              <td colspan="7" class="empty-table">沒有符合條件的食材</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- Hidden File Input -->

    <input
      ref="fileInput"
      type="file"
      accept=".xlsx, .xls"
      class="hidden-file-input"
      @change="importExcel"
    />
  </div>
</template>

<style scoped>
/* ========================================
   Page
======================================== */

.inventory-page {
  width: 100%;
}

/* ========================================
   Header
======================================== */

.page-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
}

/* ========================================
   Add Ingredient
======================================== */

.add-card {
  margin-bottom: var(--spacing-5);
}

.ingredient-form {
  display: grid;

  grid-template-columns:
    2fr
    1fr
    1fr
    1fr
    auto;

  gap: var(--spacing-4);

  align-items: end;
}

/* Button is aligned with the input fields */

.add-button-group {
  display: flex;
  flex-direction: column;

  transform: translateY(-1px);
}

.add-button-group .btn {
  height: 45px;
}

/* ========================================
   Filter
======================================== */

.filter-card {
  margin-bottom: var(--spacing-5);
}

.filter-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
}

.search-wrapper {
  position: relative;
  flex: 1;
}

.search-icon {
  position: absolute;

  left: 12px;
  top: 50%;

  transform: translateY(-50%);

  pointer-events: none;
}

.search-input {
  padding-left: 38px;
}

.filter-select {
  width: 160px;
}

/* ========================================
   Import
======================================== */

.import-card {
  margin-bottom: var(--spacing-5);
}

.import-errors {
  margin: var(--spacing-5) var(--spacing-6);

  padding: var(--spacing-4);

  border: 1px solid #fecaca;
  border-radius: var(--radius-md);

  color: var(--color-danger);
  background: var(--color-danger-light);
}

.error-title {
  margin-bottom: var(--spacing-2);

  font-weight: var(--font-weight-semibold);
}

.import-errors ul {
  margin: 0;
  padding-left: 20px;
}

.selected-file {
  display: flex;
  align-items: center;
  justify-content: space-between;

  margin: 0 var(--spacing-6) var(--spacing-4);

  padding: var(--spacing-3) var(--spacing-4);

  color: var(--color-text-secondary);

  background: var(--color-bg);

  border-radius: var(--radius-md);
}

.import-footer {
  display: flex;
  justify-content: flex-end;

  gap: var(--spacing-3);

  padding: var(--spacing-5) var(--spacing-6);

  border-top: 1px solid var(--color-border);
}

/* ========================================
   Inventory Table
======================================== */

.inventory-card {
  overflow: hidden;
}

.stock-cell {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.stock-action {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.quantity-input {
  width: 90px;
}

.btn-stock-in {
  color: var(--color-success);
  background: var(--color-success-light);
}

.btn-stock-in:hover {
  background: #dcfce7;
}

.btn-stock-out {
  color: var(--color-warning);
  background: var(--color-warning-light);
}

.btn-stock-out:hover {
  background: #fef3c7;
}

.empty-table {
  padding: 40px !important;

  color: var(--color-text-secondary);

  text-align: center;
}

/* ========================================
   File Input
======================================== */

.hidden-file-input {
  display: none;
}

/* ========================================
   Responsive
======================================== */

@media (max-width: 1100px) {
  .ingredient-form {
    grid-template-columns: repeat(2, 1fr);
  }

  .filter-row {
    flex-wrap: wrap;
  }

  .search-wrapper {
    flex-basis: 100%;
  }
}

@media (max-width: 768px) {
  .page-actions {
    width: 100%;
  }

  .page-actions .btn {
    flex: 1;
  }

  .ingredient-form {
    grid-template-columns: 1fr;
  }

  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-select {
    width: 100%;
  }

  .stock-action {
    flex-wrap: wrap;
  }
}
</style>
