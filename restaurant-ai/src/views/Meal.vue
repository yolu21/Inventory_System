<script setup>
import { ref, onMounted, computed } from "vue";
import { api } from "../services/api";

const meals = ref([]);
const newMealName = ref("");
const loading = ref(false);

const expandedMealId = ref(null);
const mealBom = ref({});
const loadingBom = ref(false);

// 取得餐點列表
const loadMeals = async () => {
  try {
    loading.value = true;
    const response = await api.get("/Meal");
    meals.value = response.data;
  } catch (error) {
    console.error("Failed to load meals:", error);
    alert("取得餐點失敗，請稍後再試。");
  } finally {
    loading.value = false;
  }
};

//新增餐點
const createMeal = async () => {
  if (!newMealName.value.trim()) {
    alert("請輸入餐點名稱");
    return;
  }

  try {
    loading.value = true;
    const response = await api.post("/Meal", {
      name: newMealName.value.trim(),
    });

    alert(`餐點 "${response.data.name}" 新增成功`);
    newMealName.value = "";

    await loadMeals(); // 重新載入餐點列表
  } catch (error) {
    console.error("Failed to create meal:", error);
    alert(`新增餐點失敗:\n ${error.response?.data?.message || error.message}`);
  } finally {
    loading.value = false;
  }
};

//點擊餐點
const toggleMeal = async (mealId) => {
  if (expandedMealId.value === mealId) {
    expandedMealId.value = null; // 收起
    return;
  }
  expandedMealId.value = mealId; // 展開
  if (mealBom.value[mealId]) {
    // 如果已經有資料，直接展開，不用重新呼叫API
    return;
  }
  try {
    loadingBom.value = true;
    const response = await api.get(`/Meal/${mealId}`);
    mealBom.value[mealId] = response.data; // 儲存餐點BOM資料
  } catch (error) {
    console.error("Failed to load meal BOM:", error);
    alert(
      `取得餐點BOM失敗:\n ${error.response?.data?.message || error.message}`,
    );
  } finally {
    loadingBom.value = false;
  }
};

onMounted(() => {
  loadMeals();
});
</script>
<template>
  <div class="page">
    <h1>🍽️ 餐點管理</h1>

    <!-- 新增餐點區塊  -->
    <div class="create-meal"></div>
    <input
      v-model="newMealName"
      placeholder="輸入餐點名稱"
      @keyup.enter="createMeal"
    />
    <button @click="createMeal" :disabled="loading">➕ 新增餐點</button>
  </div>
  <hr />
  <!-- 餐點列表區塊 -->
  <h2>📋 餐點列表</h2>
  <P v-if="loading">載入中...</P>
  <div v-else-if="meals.length === 0">目前尚未建立餐點</div>
  <div v-for="meal in meals" :key="meal.id" class="meal-item">
    <!-- 餐點標題  -->
    <div class="meal-header" @click="toggleMeal(meal.id)">
      <strong>{{ meal.name }}</strong>
      <span>
        {{ expandedMealId === meal.id ? "▼" : "▶" }}
      </span>
    </div>
    <!-- 餐點BOM內容 -->
    <div v-if="expandedMealId === meal.id" class="bom">
      <p v-if="loadingBom">載入BOM中...</p>
      <div v-else-if="!mealBom[meal.id] || mealBom[meal.id].length === 0">
        此餐點尚未設定BOM
      </div>
      <table v-else>
        <thead>
          <tr>
            <th>食材名稱</th>
            <th>單位</th>
            <th>每份用量</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="ingredient in mealBom[meal.id].ingredients"
            :key="ingredient.ingredientId"
          >
            <td>{{ ingredient.name }}</td>
            <td>{{ ingredient.unit }}</td>
            <td>{{ ingredient.quantity }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<style scoped>
.create-meal {
  display: flex;
  gap: 10px;
  margin: 20px 0;
}

.create-meal input {
  padding: 8px 12px;
  width: 250px;
}

.create-meal button {
  padding: 8px 16px;
}

.meal-item {
  border: 1px solid #ddd;
  border-radius: 8px;
  margin-bottom: 10px;
  overflow: hidden;
}

.meal-header {
  padding: 15px;
  cursor: pointer;
  background: #f5f5f5;
  display: flex;
  gap: 10px;
  align-items: center;
}

.meal-header:hover {
  background: #eaeaea;
}

.bom {
  padding: 15px 20px;
  background: white;
}

.bom table {
  width: 100%;
  border-collapse: collapse;
}

.bom th,
.bom td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
}
</style>
