<script setup>
import { ref, onMounted, computed } from "vue";
import { api } from "../services/api";
import { useMealStore } from "../stores/meal";
import { storeToRefs } from "pinia";

const mealStore = useMealStore();
const { meals, mealBom, ingredients, loading, loadingBom } =
  storeToRefs(mealStore);
const newMealName = ref("");
const expandedMealId = ref(null);

const selectedIngredientId = ref(""); // 選擇的食材ID
const bomQuantity = ref(""); // 每份用量

const serveMealId = ref(""); // 選擇的餐點ID
const serveQuantity = ref(1); // 出餐份數

//新增餐點
const createMeal = async () => {
  if (!newMealName.value.trim()) {
    alert("請輸入餐點名稱");
    return;
  }
  await mealStore.createMeal(newMealName.value.trim());
  loading.value = false;
};
//刪除餐點
const deleteMeal = async (mealId) => {
  await mealStore.deleteMeal(mealId);
};
//點擊餐點
const toggleMeal = async (mealId) => {
  if (expandedMealId.value === mealId) {
    expandedMealId.value = null; // 收起
    return;
  }
  expandedMealId.value = mealId; // 展開
  await mealStore.loadMealBom(mealId); // 取得餐點BOM
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

  await mealStore.addMealBom(
    mealId,
    selectedIngredientId.value,
    bomQuantity.value,
  );

  selectedIngredientId.value = ""; // 清空選擇的食材
  bomQuantity.value = ""; // 清空用量輸入
};
//更新餐點Bom
const updateBom = async (mealId, bom) => {
  if (!bom.quantity || isNaN(bom.quantity) || bom.quantity <= 0) {
    alert("請輸入大於 0 的用量");
    return;
  }

  await mealStore.updateMealBom(mealId, bom.ingredientId, bom.quantity);
  alert("BOM 修改成功");
  await mealStore.loadMealBom(mealId); // 重新取得餐點BOM
};
//刪除餐點Bom
const deleteBom = async (mealId, ingredientId) => {
  if (!confirm("確定要刪除這個BOM嗎？")) {
    return;
  }
  await mealStore.deleteMealBom(mealId, ingredientId);
};
// 出餐
const serveMeal = async () => {
  if (!serveMealId.value) {
    alert("請選擇餐點");
    return;
  }
  if (
    !serveQuantity.value ||
    isNaN(serveQuantity.value) ||
    serveQuantity.value <= 0
  ) {
    alert("請輸入大於 0 的份數");
    return;
  }
  const success = await mealStore.serveMeal(
    serveMealId.value,
    serveQuantity.value,
  );
  if (success) {
    serveMealId.value = "";
    serveQuantity.value = 1;
  }
};

onMounted(async () => {
  await mealStore.loadMeals(); //取得餐點列表
  await mealStore.loadIngredients(); //取得食材列表
});
</script>
<template>
  <div class="page">
    <h1>🍽️ 餐點管理</h1>
    <div class="serve-section">
      <h3>出餐</h3>
      <select
        v-model="serveMealId"
        @change="mealStore.loadMealBom(serveMealId)"
      >
        <option value="">選擇餐點</option>
        <option v-for="meal in meals" :key="meal.id" :value="meal.id">
          {{ meal.name }}
        </option>
      </select>
      <input v-model="serveQuantity" type="number" placeholder="份數" min="1" />
      <button class="create-btn" @click="serveMeal" :disabled="loading">
        {{ loading ? "處理中..." : "確認出餐" }}
      </button>

      <div
        v-if="
          serveMealId &&
          mealBom[serveMealId] &&
          mealBom[serveMealId].ingredients.length > 0
        "
        class="serve-preview"
      >
        <h4>本次預計消耗</h4>
        <table>
          <thead>
            <tr>
              <th>食材名稱</th>
              <th>每份用量</th>
              <th>出餐數量</th>
              <th>預計消耗</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="bom in mealBom[serveMealId].ingredients"
              :key="bom.ingredientId"
            >
              <td>{{ bom.name }}</td>
              <td>{{ bom.quantity }} {{ bom.unit }}</td>
              <td>{{ serveQuantity }}</td>
              <td>{{ bom.quantity * serveQuantity }} {{ bom.unit }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    <hr />
    <!-- 新增餐點區塊  -->
    <div class="create-meal">
      <input
        v-model="newMealName"
        placeholder="輸入餐點名稱"
        @keyup.enter="createMeal"
      />
      <button @click="createMeal" :disabled="loading">新增餐點</button>
    </div>

    <!-- 餐點列表區塊 -->
    <h2>📋 餐點列表</h2>
    <P v-if="loading">載入中...</P>
    <div v-else-if="meals.length === 0">目前尚未建立餐點</div>
    <div v-for="meal in meals" :key="meal.id" class="meal-item">
      <!-- 餐點標題  -->
      <div class="meal-header" @click="toggleMeal(meal.id)">
        <strong>{{ meal.name }}</strong>

        <!-- .stop 點擊按鈕時，不繼續觸發父層 click-->
        <button @click.stop="deleteMeal(meal.id)" class="delete-btn">
          刪除
        </button>
        <span>
          {{ expandedMealId === meal.id ? "▼" : "▶" }}
        </span>
      </div>
      <!-- 餐點BOM內容 -->
      <div v-if="expandedMealId === meal.id" class="bom">
        <!-- 新增BOM區塊 -->
        <div class="add-bom">
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
          <button class="create-btn" @click="addBom(meal.id)">新增BOM</button>
        </div>
        <p v-if="loadingBom">載入BOM中...</p>
        <div
          v-else-if="
            !mealBom[meal.id] || mealBom[meal.id].ingredients.length === 0
          "
        >
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
                <input
                  type="number"
                  v-model="bom.quantity"
                  min="0"
                  step="0.01"
                />
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
      </div>
    </div>
  </div>
</template>
<style scoped>
input,
select {
  padding: 10px 12px;
  margin-right: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}
.create-meal {
  display: flex;
  gap: 10px;
  margin: 20px 0;
}
.create-meal button {
  padding: 8px 24px;
  border: none;
  border-radius: 8px;
  background: #7bcd98;
  font-size: 15px;
  cursor: pointer;
}

.meal-item {
  border: 1px solid #ddd;
  border-radius: 8px;
  margin-bottom: 10px;
  overflow: hidden;
}

.meal-header {
  padding: 15px 18px;
  cursor: pointer;
  background: #f5f5f5;
  display: flex;
  align-items: center;
}

.meal-header:hover {
  background: #eaeaea;
}

.meal-header strong {
  font-size: 16px;
}

.meal-header button {
  margin-right: 10px;
  margin-left: auto;
}

.meal-header span {
  width: 20px;
  text-align: center;
  margin-right: 0;
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
  margin: 10px 10px 20px 10px;
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
.create-btn {
  padding: 8px 24px;
  border: none;
  border-radius: 8px;
  background: #7bcd98;
  font-size: 15px;
  cursor: pointer;
}
.serve-section {
  margin-bottom: 30px;
  padding: 24px;
  border-radius: 12px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.serve-section h3 {
  margin-top: 0;
  margin-bottom: 20px;
}

.serve-section select {
  min-width: 180px;
}

.serve-section input {
  width: 100px;
}

.serve-preview {
  margin-top: 20px;
  border-top: 1px solid #eee;
}

.serve-preview h4 {
  margin-bottom: 12px;
}

.serve-preview table {
  width: 100%;
  border-collapse: collapse;
}

.serve-preview th,
.serve-preview td {
  padding: 10px;
  border-bottom: 1px solid #eee;
  text-align: left;
}

.serve-preview th {
  background: #f8f9fa;
}

.serve-btn:hover {
  background: #23c862;
}

.serve-btn:disabled {
  background: #aaa;
  cursor: not-allowed;
}
</style>
