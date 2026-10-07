<script setup>
import { ref, onMounted } from "vue";
import { useMealStore } from "../stores/meal";
import { storeToRefs } from "pinia";
import { jwtDecode } from "jwt-decode";

const mealStore = useMealStore();

const { meals, mealBom, ingredients, loading, loadingBom } =
  storeToRefs(mealStore);

/* ========================================
   User / Permission
======================================== */

const token = localStorage.getItem("token");

const user = token ? jwtDecode(token) : null;

const roleClaim =
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

const isAdmin = user?.[roleClaim] === "Admin";

/* ========================================
   Meal
======================================== */

const newMealName = ref("");

const expandedMealId = ref(null);

/* ========================================
   BOM
======================================== */

const selectedIngredientId = ref("");

const bomQuantity = ref("");

/* ========================================
   Serve Meal
======================================== */

const serveMealId = ref("");

const serveQuantity = ref(1);

/* ========================================
   Create Meal
======================================== */

const createMeal = async () => {
  if (!newMealName.value.trim()) {
    alert("請輸入餐點名稱");
    return;
  }

  await mealStore.createMeal(newMealName.value.trim());

  newMealName.value = "";
};

/* ========================================
   Delete Meal
======================================== */

const deleteMeal = async (mealId) => {
  const meal = meals.value.find((item) => item.id === mealId);

  const mealName = meal?.name || "此餐點";

  const confirmed = confirm(`確定要刪除「${mealName}」嗎？`);

  if (!confirmed) {
    return;
  }

  await mealStore.deleteMeal(mealId);

  if (expandedMealId.value === mealId) {
    expandedMealId.value = null;
  }
};

/* ========================================
   Toggle Meal
======================================== */

const toggleMeal = async (mealId) => {
  if (expandedMealId.value === mealId) {
    expandedMealId.value = null;
    return;
  }

  expandedMealId.value = mealId;

  await mealStore.loadMealBom(mealId);
};

/* ========================================
   Add BOM
======================================== */

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

  await mealStore.addBom(mealId, selectedIngredientId.value, bomQuantity.value);

  selectedIngredientId.value = "";
  bomQuantity.value = "";
};

/* ========================================
   Update BOM
======================================== */

const updateBom = async (mealId, bom) => {
  if (!bom.quantity || isNaN(bom.quantity) || bom.quantity <= 0) {
    alert("請輸入大於 0 的用量");
    return;
  }

  await mealStore.updateMealBom(mealId, bom.ingredientId, bom.quantity);

  alert("BOM 修改成功");

  await mealStore.loadMealBom(mealId);
};

/* ========================================
   Delete BOM
======================================== */

const deleteBom = async (mealId, ingredientId) => {
  const confirmed = confirm("確定要刪除這個 BOM 嗎？");

  if (!confirmed) {
    return;
  }

  await mealStore.deleteMealBom(mealId, ingredientId);
};

/* ========================================
   Serve Meal
======================================== */

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

/* ========================================
   Lifecycle
======================================== */

onMounted(async () => {
  await mealStore.loadMeals();
  await mealStore.loadIngredients();
});
</script>

<template>
  <div class="meal-page">
    <!-- ====================================
         Page Header
    ===================================== -->

    <header class="page-header">
      <div>
        <h1 class="page-title">餐點管理</h1>

        <p class="page-description">管理餐點、食材 BOM 與出餐庫存扣除。</p>
      </div>
    </header>

    <!-- ====================================
         Serve Meal
    ===================================== -->

    <section class="card serve-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">出餐</h2>

          <p class="card-description">
            選擇餐點與出餐份數，系統會依 BOM 自動扣除庫存。
          </p>
        </div>
      </div>

      <div class="card-body">
        <div class="serve-form">
          <!-- Meal -->

          <div class="form-group">
            <label class="form-label" for="serve-meal"> 餐點 </label>

            <select
              id="serve-meal"
              v-model="serveMealId"
              class="form-select"
              @change="mealStore.loadMealBom(serveMealId)"
            >
              <option value="">選擇餐點</option>

              <option v-for="meal in meals" :key="meal.id" :value="meal.id">
                {{ meal.name }}
              </option>
            </select>
          </div>

          <!-- Quantity -->

          <div class="form-group">
            <label class="form-label" for="serve-quantity"> 出餐份數 </label>

            <input
              id="serve-quantity"
              v-model.number="serveQuantity"
              class="form-input"
              type="number"
              min="1"
            />
          </div>

          <!-- Submit -->

          <div class="form-group serve-action">
            <label class="form-label"> &nbsp; </label>

            <button
              class="btn btn-primary"
              type="button"
              :disabled="loading"
              @click="serveMeal"
            >
              {{ loading ? "處理中..." : "確認出餐" }}
            </button>
          </div>
        </div>

        <!-- Serve Preview -->

        <div
          v-if="
            serveMealId &&
            mealBom[serveMealId] &&
            mealBom[serveMealId].ingredients.length > 0
          "
          class="serve-preview"
        >
          <div class="preview-header">
            <div>
              <h3 class="preview-title">本次預計消耗</h3>

              <p class="preview-description">
                根據目前餐點 BOM 與出餐份數計算。
              </p>
            </div>
          </div>

          <div class="table-wrapper">
            <table class="table">
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
                  <td>
                    <strong>
                      {{ bom.name }}
                    </strong>
                  </td>

                  <td>
                    {{ bom.quantity }}
                    {{ bom.unit }}
                  </td>

                  <td>
                    {{ serveQuantity }}
                  </td>

                  <td>
                    <strong>
                      {{ (bom.quantity * serveQuantity).toFixed(2) }}
                      {{ bom.unit }}
                    </strong>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </section>

    <!-- ====================================
         Create Meal
    ===================================== -->

    <section v-if="isAdmin" class="card create-meal-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">新增餐點</h2>

          <p class="card-description">
            建立新的餐點後，可以進一步設定食材 BOM。
          </p>
        </div>
      </div>

      <div class="card-body">
        <form class="create-meal-form" @submit.prevent="createMeal">
          <input
            v-model="newMealName"
            class="form-input"
            placeholder="輸入餐點名稱"
          />

          <button class="btn btn-primary" type="submit" :disabled="loading">
            新增餐點
          </button>
        </form>
      </div>
    </section>

    <!-- ====================================
         Meal List
    ===================================== -->

    <section class="card meal-list-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">餐點列表</h2>

          <p class="card-description">共 {{ meals.length }} 道餐點</p>
        </div>
      </div>

      <!-- Loading -->

      <div v-if="loading" class="loading-state">
        <div class="loading-spinner"></div>

        <p>載入餐點資料中...</p>
      </div>

      <!-- Empty -->

      <div v-else-if="meals.length === 0" class="empty-state">
        <div class="empty-icon">🍽️</div>

        <div class="empty-title">目前尚未建立餐點</div>

        <div class="empty-description">
          建立餐點後，可以設定對應的食材 BOM。
        </div>
      </div>

      <!-- Meal List -->

      <div v-else class="meal-list">
        <div v-for="meal in meals" :key="meal.id" class="meal-item">
          <!-- Meal Header -->

          <div
            class="meal-header"
            :class="{
              expanded: expandedMealId === meal.id,
            }"
            @click="toggleMeal(meal.id)"
          >
            <div class="meal-title">
              <span class="meal-icon"> 🍽️ </span>

              <strong>
                {{ meal.name }}
              </strong>
            </div>

            <div class="meal-actions">
              <button
                v-if="isAdmin"
                class="btn btn-danger btn-sm"
                type="button"
                @click.stop="deleteMeal(meal.id)"
              >
                刪除
              </button>

              <span class="expand-icon">
                {{ expandedMealId === meal.id ? "▼" : "▶" }}
              </span>
            </div>
          </div>

          <!-- BOM -->

          <div v-if="expandedMealId === meal.id" class="bom-section">
            <!-- Add BOM -->

            <div v-if="isAdmin" class="add-bom">
              <div class="add-bom-title">新增 BOM</div>

              <div class="add-bom-form">
                <select v-model="selectedIngredientId" class="form-select">
                  <option value="">選擇食材</option>

                  <option
                    v-for="ingredient in ingredients"
                    :key="ingredient.id"
                    :value="ingredient.id"
                  >
                    {{ ingredient.name }}
                    ({{ ingredient.unit }})
                  </option>
                </select>

                <input
                  v-model="bomQuantity"
                  class="form-input bom-quantity-input"
                  type="number"
                  placeholder="每份用量"
                  min="0"
                  step="0.01"
                />

                <button
                  class="btn btn-primary"
                  type="button"
                  @click="addBom(meal.id)"
                >
                  新增 BOM
                </button>
              </div>
            </div>

            <!-- Loading BOM -->

            <div v-if="loadingBom" class="bom-loading">
              <div class="loading-spinner"></div>

              <span> 載入 BOM 中... </span>
            </div>

            <!-- Empty BOM -->

            <div
              v-else-if="
                !mealBom[meal.id] || mealBom[meal.id].ingredients.length === 0
              "
              class="bom-empty"
            >
              <div class="empty-icon">📋</div>

              <div class="empty-title">此餐點尚未設定 BOM</div>

              <div class="empty-description">
                {{
                  isAdmin
                    ? "請新增食材與每份用量。"
                    : "目前沒有可查看的 BOM 資料。"
                }}
              </div>
            </div>

            <!-- BOM Table -->

            <div v-else class="table-wrapper">
              <table class="table">
                <thead>
                  <tr>
                    <th>食材名稱</th>
                    <th>單位</th>
                    <th>每份用量</th>

                    <th v-if="isAdmin">操作</th>
                  </tr>
                </thead>

                <tbody>
                  <tr
                    v-for="bom in mealBom[meal.id].ingredients"
                    :key="bom.ingredientId"
                  >
                    <!-- Ingredient -->

                    <td>
                      <strong>
                        {{ bom.name }}
                      </strong>
                    </td>

                    <!-- Unit -->

                    <td>
                      {{ bom.unit }}
                    </td>

                    <!-- Quantity -->

                    <td>
                      <input
                        v-if="isAdmin"
                        v-model="bom.quantity"
                        class="form-input bom-edit-input"
                        type="number"
                        min="0"
                        step="0.01"
                      />

                      <span v-else>
                        {{ Number(bom.quantity).toFixed(2) }}
                      </span>
                    </td>

                    <!-- Actions -->

                    <td v-if="isAdmin">
                      <div class="bom-actions">
                        <button
                          class="btn btn-secondary btn-sm"
                          type="button"
                          @click="updateBom(meal.id, bom)"
                        >
                          更新
                        </button>

                        <button
                          class="btn btn-danger btn-sm"
                          type="button"
                          @click="deleteBom(meal.id, bom.ingredientId)"
                        >
                          刪除
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ========================================
   Page
======================================== */

.meal-page {
  width: 100%;
}

/* ========================================
   Serve
======================================== */

.serve-card {
  margin-bottom: var(--spacing-5);
}

.serve-form {
  display: grid;

  grid-template-columns:
    2fr
    1fr
    auto;

  gap: var(--spacing-4);

  align-items: end;
}

.serve-action {
  min-width: 120px;
}

.serve-action .btn {
  width: 100%;
  height: 45px;
}

.serve-preview {
  margin-top: var(--spacing-6);

  padding-top: var(--spacing-5);

  border-top: 1px solid var(--color-border);
}

.preview-header {
  margin-bottom: var(--spacing-4);
}

.preview-title {
  margin: 0;

  color: var(--color-text-primary);

  font-size: 16px;
  font-weight: var(--font-weight-semibold);
}

.preview-description {
  margin: var(--spacing-1) 0 0;

  color: var(--color-text-secondary);

  font-size: var(--font-size-sm);
}

/* ========================================
   Create Meal
======================================== */

.create-meal-card {
  margin-bottom: var(--spacing-5);
}

.create-meal-form {
  display: flex;
  align-items: center;

  gap: var(--spacing-3);
}

.create-meal-form .form-input {
  flex: 1;
  max-width: 400px;
}

/* ========================================
   Meal List
======================================== */

.meal-list-card {
  overflow: hidden;
}

.meal-list {
  border-top: 1px solid var(--color-border);
}

.meal-item {
  border-bottom: 1px solid var(--color-border);
}

.meal-item:last-child {
  border-bottom: none;
}

/* ========================================
   Meal Header
======================================== */

.meal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;

  min-height: 64px;

  padding: var(--spacing-4) var(--spacing-6);

  background: var(--color-surface);

  cursor: pointer;

  transition: background var(--transition-fast);
}

.meal-header:hover {
  background: var(--color-bg);
}

.meal-header.expanded {
  background: var(--color-primary-light);

  border-bottom: 1px solid var(--color-border);
}

.meal-title {
  display: flex;
  align-items: center;

  gap: var(--spacing-3);

  min-width: 0;
}

.meal-title strong {
  color: var(--color-text-primary);

  font-size: 16px;
}

.meal-icon {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 36px;
  height: 36px;

  flex-shrink: 0;

  background: var(--color-bg);

  border-radius: var(--radius-md);

  font-size: 18px;
}

.meal-actions {
  display: flex;
  align-items: center;

  gap: var(--spacing-3);
}

.expand-icon {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 24px;

  color: var(--color-text-secondary);

  font-size: 12px;
}

/* ========================================
   BOM Section
======================================== */

.bom-section {
  padding: var(--spacing-6);

  background: var(--color-bg);
}

/* ========================================
   Add BOM
======================================== */

.add-bom {
  margin-bottom: var(--spacing-5);

  padding: var(--spacing-5);

  background: var(--color-surface);

  border: 1px solid var(--color-border);

  border-radius: var(--radius-md);
}

.add-bom-title {
  margin-bottom: var(--spacing-3);

  color: var(--color-text-primary);

  font-weight: var(--font-weight-semibold);
}

.add-bom-form {
  display: grid;

  grid-template-columns:
    minmax(200px, 1fr)
    150px
    auto;

  gap: var(--spacing-3);

  align-items: center;
}

.bom-quantity-input {
  width: 100%;
}

/* ========================================
   BOM Table
======================================== */

.bom-section .table-wrapper {
  background: var(--color-surface);

  border: 1px solid var(--color-border);

  border-radius: var(--radius-md);

  overflow: hidden;
}

.bom-edit-input {
  width: 120px;
}

.bom-actions {
  display: flex;
  align-items: center;

  gap: var(--spacing-2);
}

/* ========================================
   BOM Loading / Empty
======================================== */

.bom-loading {
  min-height: 160px;

  display: flex;
  align-items: center;
  justify-content: center;

  gap: var(--spacing-3);

  color: var(--color-text-secondary);
}

.bom-loading .loading-spinner {
  width: 24px;
  height: 24px;
}

.bom-empty {
  min-height: 180px;

  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  padding: var(--spacing-6);

  text-align: center;

  background: var(--color-surface);

  border: 1px solid var(--color-border);

  border-radius: var(--radius-md);
}

/* ========================================
   Loading
======================================== */

.loading-state {
  min-height: 280px;

  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  gap: var(--spacing-4);

  color: var(--color-text-secondary);
}

.loading-state p {
  margin: 0;
}

.loading-spinner {
  width: 32px;
  height: 32px;

  border: 3px solid var(--color-border);

  border-top-color: var(--color-primary);

  border-radius: 50%;

  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* ========================================
   Button
======================================== */

.btn-sm {
  padding: 6px 10px;

  font-size: 12px;
}

/* ========================================
   Empty State
======================================== */

.empty-state {
  min-height: 280px;

  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  padding: var(--spacing-8);

  text-align: center;
}

.empty-icon {
  margin-bottom: var(--spacing-3);

  font-size: 36px;
}

.empty-title {
  margin-bottom: var(--spacing-1);

  color: var(--color-text-primary);

  font-weight: var(--font-weight-semibold);
}

.empty-description {
  color: var(--color-text-secondary);

  font-size: var(--font-size-sm);
}

/* ========================================
   Responsive
======================================== */

@media (max-width: 900px) {
  .serve-form {
    grid-template-columns: 1fr 1fr;
  }

  .serve-action {
    grid-column: span 2;
  }

  .add-bom-form {
    grid-template-columns: 1fr 140px;
  }

  .add-bom-form .btn {
    grid-column: span 2;
  }
}

@media (max-width: 768px) {
  .serve-form {
    grid-template-columns: 1fr;
  }

  .serve-action {
    grid-column: auto;
  }

  .create-meal-form {
    flex-direction: column;
    align-items: stretch;
  }

  .create-meal-form .form-input {
    max-width: none;
  }

  .add-bom-form {
    grid-template-columns: 1fr;
  }

  .add-bom-form .btn {
    grid-column: auto;
  }

  .meal-header {
    padding: var(--spacing-4);
  }

  .meal-actions {
    gap: var(--spacing-2);
  }

  .bom-section {
    padding: var(--spacing-4);
  }

  .bom-actions {
    flex-wrap: wrap;
  }
}
</style>
