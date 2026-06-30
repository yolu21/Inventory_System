<script setup>
import { ref, onMounted } from "vue";
import { api } from "../services/api";

const ingredients = ref([]);
const newName = ref("");
const newUnit = ref("");

const loadIngredients = async () => {
  try {
    const res = await api.get("/Ingredients");
    ingredients.value = res.data;

    for (const item of ingredients.value) {
      await loadStock(item);
    }
  } catch (error) {
    console.error(error);
  }
};

const loadStock = async (item) => {
  try {
    const stockRes = await api.get(`/Stock/stock/${item.id}`);
    item.stock = stockRes.data.stock;
  } catch (error) {
    console.error(error);
  }
};

const addIngredient = async () => {
  if (!newName.value || !newUnit.value) return;
  try {
    await api.post("/Ingredients", {
      name: newName.value,
      unit: newUnit.value,
    });
    newName.value = "";
    newUnit.value = "";
    await loadIngredients();
  } catch (error) {
    console.error(error);
  }
};

const deleteIngredient = async (item) => {
  try {
    await api.delete(`/Ingredients/${item.id}`);
    await loadIngredients();
  } catch (error) {
    console.error(error);
  }
};
const addStock = async (item) => {
  await api.post("/Stock", {
    ingredientId: item.id,
    type: "IN",
    quantity: Number(item.amount) || 0,
  });
  if (item) {
    await loadStock(item);
  }
};

const removeStock = async (item) => {
  await api.post("/Stock", {
    ingredientId: item.id,
    type: "OUT",
    quantity: Number(item.amount) || 0,
  });
  if (item) {
    await loadStock(item);
  }
};
onMounted(() => {
  loadIngredients();
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
        <tr v-for="item in ingredients" :key="item.id">
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
<style scoped></style>
