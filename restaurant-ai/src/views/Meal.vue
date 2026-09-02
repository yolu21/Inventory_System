<script setup>
import { ref, onMounted, computed } from "vue";
import { api } from "../services/api";

const meals = ref([]);
const newMealName = ref("");
const loading = ref(false);

const expandedMealId = ref(null);
const mealBom = ref({});
const loadingBom = ref(false);

const ingredients = ref([]); // 所有食材列表
const selectedIngredientId = ref(""); // 選擇的食材ID
const bomQuantity = ref(""); // 每份用量

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
//刪除餐點
const deleteMeal = async (mealId) => {
  if (!confirm("確定要刪除這個餐點嗎？")) {
    return;
  }

  try {
    await api.delete(`/Meal/${mealId}`);

    alert("餐點刪除成功");

    await loadMeals(); // 重新載入餐點列表
  } catch (error) {
    console.error("Failed to delete meal:", error);
    alert(`刪除餐點失敗:\n ${error.response?.data?.message || error.message}`);
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
const loadIngredients = async () => {
  try {
    const response = await api.get("/Ingredients");
    ingredients.value = response.data;
  } catch (error) {
    console.error("Failed to load ingredients:", error);
    alert(
      `取得食材列表失敗:\n ${error.response?.data?.message || error.message}`,
    );
  }
};
//新增餐點Bom
const addBom = async (mealId) => {
  if (!selectedIngredientId.value) {
    alert("請選擇食材");
    return;
  }
  if (
    !bomQuantity.value ||
    isNaN(bomQuantity.value) ||
    bomQuantity.value <= 0
  ) {
    alert("請輸入大於 0 的用量");
    return;
  }

  try {
    await api.post(`/Meal/${mealId}/ingredients`, {
      ingredientId: Number(selectedIngredientId.value),
      quantity: Number(bomQuantity.value),
    });

    alert("BOM新增成功");
    // 重新載入餐點BOM
    // 重新取得這個餐點的 BOM
    const res = await api.get(`/Meal/${mealId}`);

    mealBom.value[mealId] = res.data;

    //清空輸入
    selectedIngredientId.value = "";
    bomQuantity.value = "";
  } catch (error) {
    console.error("Failed to add BOM:", error);
    alert(`新增BOM失敗:\n ${error.response?.data?.message || error.message}`);
  }
};
//更新餐點Bom
const updateBom = async (mealId, bom) => {
  if (!bom.quantity || isNaN(bom.quantity) || bom.quantity <= 0) {
    alert("請輸入大於 0 的用量");
    return;
  }

  try {
    await api.put(`/Meal/${mealId}/ingredients/${bom.ingredientId}`, {
      quantity: Number(bom.quantity),
    });

    alert("BOM 修改成功");
    // 重新取得這個餐點的 BOM
    const res = await api.get(`/Meal/${mealId}`);
    mealBom.value[mealId] = res.data;
  } catch (error) {
    console.error("Failed to update BOM:", error);
    alert(`更新BOM失敗:\n ${error.response?.data?.message || error.message}`);
  }
};
//刪除餐點Bom
const deleteBom = async (mealId, ingredientId) => {
  if (!confirm("確定要刪除這個BOM嗎？")) {
    return;
  }

  try {
    await api.delete(`/Meal/${mealId}/ingredients/${ingredientId}`);
    alert("BOM刪除成功");
    // 重新取得這個餐點的 BOM
    const res = await api.get(`/Meal/${mealId}`);
    mealBom.value[mealId] = res.data;
  } catch (error) {
    console.error("Failed to delete BOM:", error);
    alert(`刪除BOM失敗:\n ${error.response?.data?.message || error.message}`);
  }
};
onMounted(async () => {
  await loadMeals();
  await loadIngredients();
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
      <button @click="deleteMeal(meal.id)" class="delete-btn">刪除</button>
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
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="bom in mealBom[meal.id].ingredients"
            :key="bom.ingredientId"
            class="bom-item"
          >
            <td>{{ bom.name }}</td>
            <td>{{ bom.unit }}</td>
            <td>
              <input type="number" v-model="bom.quantity" min="0" step="0.01" />
            </td>

            <td>
              <button @click="updateBom(meal.id, bom)" class="update-btn">
                更新
              </button>
              <button
                @click="deleteBom(meal.id, bom.ingredientId)"
                class="delete-btn"
              >
                刪除
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <!-- 新增BOM區塊 -->
      <div class="add-bom">
        <h5>新增BOM</h5>
        <select v-model="selectedIngredientId">
          <option value="">選擇食材</option>
          <option
            v-for="ingredient in ingredients"
            :key="ingredient.id"
            :value="ingredient.id"
          >
            {{ ingredient.name }} ({{ ingredient.unit }})
          </option>
        </select>
        <input
          v-model="bomQuantity"
          type="number"
          placeholder="每份用量"
          min="0"
          step="0.01"
        />
        <button @click="addBom(meal.id)">新增BOM</button>
      </div>
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

.meal-header button {
  margin-left: auto;
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

.update-btn {
  margin-right: 8px;
  padding: 6px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  background-color: #cfefff;
}
.update-btn:hover {
  background-color: #78c2ff;
}

.delete-btn {
  background-color: #e78b94;
  color: black;
  border: none;
  border-radius: 4px;
}

.delete-btn:hover {
  background-color: #ff7878;
}

.add-bom {
  margin-top: 20px;
  padding-top: 15px;
  border-top: 1px solid #ddd;

  display: flex;
  align-items: center;
  gap: 10px;
}

.add-bom select,
.add-bom input {
  padding: 8px;
}

.add-bom input {
  width: 120px;
}
.delete-btn {
  background-color: #e78b94;
  color: black;
  border: none;
  border-radius: 4px;
}

.delete-btn:hover {
  background-color: #c82333;
}
</style>
