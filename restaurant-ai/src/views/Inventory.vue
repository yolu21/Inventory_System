<script setup>
import { ref, onMounted, computed } from "vue";
import { useInventoryStore } from "../stores/inventory";
//const ingredients = ref([]);
const inventoryStore = useInventoryStore();
const newName = ref("");
const newUnit = ref("");

const keyword = ref("");
const stockFilter = ref("all");
const sortBy = ref("name");

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
onMounted(async () => {
  await inventoryStore.loadIngredients();
});
</script>

<template>
  <div class="page">
    <h1>🍴 Restaurant Inventory System</h1>
    <div class="add-section">
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

.toolbar input,
.toolbar select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
}
</style>
