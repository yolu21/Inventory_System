<script setup>
import { ref, onMounted, computed } from "vue";
import { api } from "../services/api";
import { useInventoryStore } from "../stores/inventory";
import * as XLSX from "xlsx";
//const ingredients = ref([]);
const inventoryStore = useInventoryStore();
const newName = ref("");
const newUnit = ref("");

const keyword = ref("");
const stockFilter = ref("all");
const sortBy = ref("name");

const importData = ref([]); //匯入 Excel 資料
const importErrors = ref([]); //匯入錯誤訊息
const fileInput = ref(null); //檔案輸入框的引用

const filteredIngredients = computed(() => {
  let list = [...inventoryStore.inventory];

  //搜尋
  if (keyword.value) {
    list = list.filter((item) =>
      item.name.toLowerCase().includes(keyword.value.toLowerCase()),
    );
  }

  //篩選
  if (stockFilter.value === "low") {
    list = list.filter((item) => item.stock < 10);
  }

  if (stockFilter.value === "normal") {
    list = list.filter((item) => item.stock >= 10);
  }

  //排序
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

const addIngredient = async () => {
  if (!newName.value || !newUnit.value) return;

  await inventoryStore.addIngredient({
    name: newName.value,
    unit: newUnit.value,
  });

  newName.value = "";
  newUnit.value = "";
};

const deleteIngredient = async (item) => {
  await inventoryStore.deleteIngredient(item.id);
};

const addStock = async (item) => {
  await inventoryStore.addStock(item);
};

const removeStock = async (item) => {
  await inventoryStore.removeStock(item);
};

//匯出 Excel
const exportExcel = () => {
  const data = inventoryStore.inventory.map((item) => ({
    食材名稱: item.name,
    單位: item.unit,
    庫存: item.stock,
  }));

  const worksheet = XLSX.utils.json_to_sheet(data);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "Inventory");
  XLSX.writeFile(workbook, "inventory.xlsx");
};

//匯入 Excel
const importExcel = (event) => {
  const file = event.target.files[0];

  if (!file) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    const data = new Uint8Array(e.target.result);

    const workbook = XLSX.read(data, { type: "array" });

    const sheet = workbook.Sheets[workbook.SheetNames[0]];

    //const json = XLSX.utils.sheet_to_json(sheet);
    const excelData = XLSX.utils.sheet_to_json(sheet);

    const json = excelData.map((item) => ({
      Name: item["食材名稱"],
      Unit: item["單位"],
      Stock: item["庫存"],
    }));
    const errors = validateImportData(json);
    importErrors.value = errors;

    if (errors.length > 0) {
      alert(`匯入失敗，請檢查以下錯誤:\n${errors.join("\n")}`);
      clearImport();
      return;
    } else {
      importData.value = json;
      alert("匯入成功，請點擊儲存以更新資料庫。");
    }

    console.log(json);
  };

  reader.readAsArrayBuffer(file);
};

//儲存 Excel
const saveExcel = async () => {
  if (importData.value.length === 0) {
    alert("沒有匯入資料，請先匯入 Excel。");
    return;
  }
  console.log(importData.value);
  try {
    const res = await api.post("/Import", importData.value, {
      headers: {
        "Content-Type": "application/json",
      },
    });
    alert(`匯入成功。
    新增食材: ${res.data.newIngredients} 筆
    新增庫存: ${res.data.stockRecords} 筆
    `);
    await inventoryStore.loadIngredients(); //重新載入食材資料
    clearImport(); //清除匯入資料
  } catch (error) {
    console.error("匯入失敗:", error);
    alert(`匯入失敗。
    錯誤訊息: ${error.response.data.error}`);
  }
};

//清除匯入資料
const clearImport = () => {
  importData.value = [];
  if (fileInput.value) {
    fileInput.value.value = null; //清除檔案輸入框的值
  }
};

const validateImportData = (data) => {
  const errors = [];
  data.forEach((item, index) => {
    const row = index + 1; // Excel 的列號從 1 開始，且第一列是標題，所以要加 2
    if (!item["Name"]) {
      errors.push(`第 ${row} 列未填寫食材名稱`);
    }

    if (!item["Unit"]) {
      errors.push(`第 ${row} 列未填寫單位`);
    }

    if (item["Stock"] === undefined || item["Stock"] === "") {
      errors.push(`第 ${row} 列 ${item["Name"]} 未填寫進貨數量`);
    } else if (isNaN(item["Stock"])) {
      errors.push(`第 ${row} 列 ${item["Name"]} 進貨數量必須為數字`);
    } else if (item["Stock"] <= 0) {
      errors.push(`第 ${row} 列 ${item["Name"]}   進貨數量不能小於 0`);
    }
  });
  return errors;
};
onMounted(async () => {
  await inventoryStore.loadIngredients();
});
</script>

<template>
  <div class="page">
    <h1>🍴 Restaurant Inventory System</h1>
    <div class="toolbar">
      <input v-model="newName" placeholder="Ingredient Name" />
      <input v-model="newUnit" placeholder="Unit" />
      <button @click="addIngredient">➕ Add Ingredient</button>
    </div>
    <div class="toolbar">
      <input v-model="keyword" placeholder="🔍 Search Ingredient" />

      <select v-model="stockFilter">
        <option value="all">All stock</option>
        <option value="low">Low stock</option>
        <option value="normal">Normal stock</option>
      </select>

      <select v-model="sortBy">
        <option value="name">Name</option>
        <option value="stock">Stock</option>
      </select>
    </div>
    <div class="toolbar">
      <input
        ref="fileInput"
        type="file"
        accept=".xlsx, .xls"
        @change="importExcel"
      />
      <button @click="saveExcel">Save</button>
      <button @click="clearImport">Clear</button>
      <button @click="exportExcel">📥 Download Excel</button>
    </div>
    <div v-if="importData.length > 0" class="preview">
      <h2>📋 Excel 匯入預覽</h2>
      <p>共 {{ importData.length }} 筆資料</p>
      <table>
        <thead>
          <tr>
            <th>食材名稱</th>
            <th>單位</th>
            <th>庫存</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(item, index) in importData" :key="index">
            <td>{{ item["Name"] }}</td>
            <td>{{ item["Unit"] }}</td>
            <td>{{ item["Stock"] }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <table>
      <thead>
        <tr>
          <th>Name</th>
          <th>Unit</th>
          <th>Stock</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in filteredIngredients" :key="item.id">
          <td>{{ item.name }}</td>
          <td>{{ item.unit }}</td>
          <td>
            {{ item.stock }}
            <span v-if="item.stock < 10" class="warning"> (Low Stock) </span>
          </td>
          <td>
            <input type="number" v-model="item.amount" placeholder="Qty" />
            <button @click="addStock(item)">➕ IN</button>
            <button @click="removeStock(item)">➖ OUT</button>
            <button @click="deleteIngredient(item)">❌ Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<style scoped>
.toolbar {
  display: flex;
  gap: 12px;
  margin: 20px 0;
  flex-wrap: wrap;
}

input,
select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  margin-right: 8px;
}
</style>
