<script setup>
import { ref, computed, onMounted } from "vue";
import { api } from "../services/api";

const forecasts = ref([]);
const loading = ref(false);

// 是否只顯示需要補貨的食材
const showNeedReplenishment = ref(false);

// 排序方式
const sortBy = ref("default");

// 預設分析條件
const usageDays = ref(14);
const forecastDays = ref(7);

/* ========================================
   Summary
======================================== */

// 食材總數
const totalIngredients = computed(() => {
  return forecasts.value.length;
});

// 需要補貨食材數量
const needReplenishment = computed(() => {
  return forecasts.value.filter(
    (item) => Number(item.suggestedPurchase || 0) > 0,
  ).length;
});

// 預估補貨總成本
const totalEstimatedCost = computed(() => {
  return forecasts.value.reduce(
    (sum, item) => sum + Number(item.estimatedCost || 0),
    0,
  );
});

/* ========================================
   Filter / Sort
======================================== */

const filteredForecasts = computed(() => {
  let result = [...forecasts.value];

  // 篩選
  if (showNeedReplenishment.value) {
    result = result.filter((item) => Number(item.suggestedPurchase || 0) > 0);
  }

  // 排序
  if (sortBy.value === "purchase-desc") {
    result.sort(
      (a, b) =>
        Number(b.suggestedPurchase || 0) - Number(a.suggestedPurchase || 0),
    );
  }

  if (sortBy.value === "cost-desc") {
    result.sort(
      (a, b) => Number(b.estimatedCost || 0) - Number(a.estimatedCost || 0),
    );
  }

  if (sortBy.value === "stock-asc") {
    result.sort(
      (a, b) => Number(a.currentStock || 0) - Number(b.currentStock || 0),
    );
  }

  return result;
});

/* ========================================
   Load Forecast
======================================== */

const loadForecast = async () => {
  loading.value = true;

  try {
    const res = await api.get("/Forecast", {
      params: {
        usageDays: usageDays.value,
        forecastDays: forecastDays.value,
      },
    });

    forecasts.value = res.data;
  } catch (error) {
    console.error("取得庫存預測失敗:", error);

    alert("取得庫存預測失敗");
  } finally {
    loading.value = false;
  }
};

/* ========================================
   Status
======================================== */

const getStatus = (item) => {
  if (item.suggestedPurchase > 0) {
    return "需要補貨";
  }

  if (item.currentStock <= item.minimumStock) {
    return "接近最低庫存";
  }

  return "庫存充足";
};

/* ========================================
   Lifecycle
======================================== */

onMounted(() => {
  loadForecast();
});
</script>

<template>
  <div class="forecast-page">
    <!-- ====================================
         Page Header
    ===================================== -->

    <header class="page-header">
      <div>
        <h1 class="page-title">庫存預測</h1>

        <p class="page-description">
          根據歷史庫存使用量，預測未來需求與建議補貨量。
        </p>
      </div>
    </header>

    <!-- ====================================
         Forecast Settings
    ===================================== -->

    <section class="card settings-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">預測條件</h2>

          <p class="card-description">調整分析期間後重新計算庫存預測。</p>
        </div>
      </div>

      <div class="card-body">
        <div class="forecast-settings">
          <!-- Usage Days -->

          <div class="setting">
            <label class="form-label" for="usage-days"> 使用歷史資料 </label>

            <div class="setting-input">
              <input
                id="usage-days"
                v-model.number="usageDays"
                class="form-input"
                type="number"
                min="1"
              />

              <span class="setting-unit"> 天 </span>
            </div>
          </div>

          <!-- Forecast Days -->

          <div class="setting">
            <label class="form-label" for="forecast-days"> 預測未來需求 </label>

            <div class="setting-input">
              <input
                id="forecast-days"
                v-model.number="forecastDays"
                class="form-input"
                type="number"
                min="1"
              />

              <span class="setting-unit"> 天 </span>
            </div>
          </div>

          <!-- Analyze -->

          <button
            class="btn btn-primary analyze-btn"
            type="button"
            :disabled="loading"
            @click="loadForecast"
          >
            {{ loading ? "分析中..." : "重新分析" }}
          </button>
        </div>
      </div>
    </section>

    <!-- ====================================
         Summary Cards
    ===================================== -->

    <section class="summary-grid">
      <!-- Total Ingredients -->

      <div class="stat-card">
        <div class="stat-icon stat-icon-info">📦</div>

        <div class="stat-content">
          <p class="stat-label">食材總數</p>

          <p class="stat-value">
            {{ totalIngredients }}
          </p>
        </div>
      </div>

      <!-- Need Replenishment -->

      <div class="stat-card">
        <div class="stat-icon stat-icon-danger">!</div>

        <div class="stat-content">
          <p class="stat-label">需要補貨</p>

          <p class="stat-value">
            {{ needReplenishment }}
          </p>
        </div>
      </div>

      <!-- Estimated Cost -->

      <div class="stat-card">
        <div class="stat-icon stat-icon-success">$</div>

        <div class="stat-content">
          <p class="stat-label">預估補貨總成本</p>

          <p class="stat-value">${{ totalEstimatedCost.toFixed(2) }}</p>
        </div>
      </div>
    </section>

    <!-- ====================================
         Forecast Info
    ===================================== -->

    <div class="forecast-info">
      <span class="info-icon"> ℹ </span>

      <span>
        使用最近
        <strong>{{ usageDays }}</strong>
        天的庫存使用紀錄， 預測未來
        <strong>{{ forecastDays }}</strong>
        天的需求。
      </span>
    </div>

    <!-- ====================================
         Toolbar
    ===================================== -->

    <section class="card toolbar-card">
      <div class="toolbar">
        <!-- Filter -->

        <div class="forecast-filter">
          <button
            class="filter-button"
            :class="{
              active: !showNeedReplenishment,
            }"
            type="button"
            @click="showNeedReplenishment = false"
          >
            全部
          </button>

          <button
            class="filter-button"
            :class="{
              active: showNeedReplenishment,
            }"
            type="button"
            @click="showNeedReplenishment = true"
          >
            需要補貨
          </button>
        </div>

        <!-- Sort -->

        <div class="forecast-sort">
          <label class="sort-label" for="forecast-sort"> 排序 </label>

          <select
            id="forecast-sort"
            v-model="sortBy"
            class="form-select sort-select"
          >
            <option value="default">預設順序</option>

            <option value="purchase-desc">建議補貨量：高 → 低</option>

            <option value="cost-desc">預估成本：高 → 低</option>

            <option value="stock-asc">目前庫存：低 → 高</option>
          </select>
        </div>
      </div>
    </section>

    <!-- ====================================
         Forecast Table
    ===================================== -->

    <section class="card forecast-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">預測結果</h2>

          <p class="card-description">
            共 {{ filteredForecasts.length }} 項食材
          </p>
        </div>
      </div>

      <!-- Loading -->

      <div v-if="loading" class="loading-state">
        <div class="loading-spinner"></div>

        <p>正在分析庫存資料...</p>
      </div>

      <!-- Table -->

      <div v-else class="table-wrapper">
        <table class="table">
          <thead>
            <tr>
              <th>食材</th>
              <th>目前庫存</th>
              <th>最低庫存</th>
              <th>平均每日使用</th>
              <th>預測需求</th>
              <th>建議補貨</th>
              <th>預估成本</th>
              <th>狀態</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="item in filteredForecasts" :key="item.ingredientId">
              <!-- Ingredient -->

              <td>
                <strong>
                  {{ item.ingredientName }}
                </strong>
              </td>

              <!-- Current Stock -->

              <td>
                {{ item.currentStock }}
                {{ item.unit }}
              </td>

              <!-- Minimum Stock -->

              <td>
                {{ item.minimumStock }}
                {{ item.unit }}
              </td>

              <!-- Average Daily Usage -->

              <td>
                {{ Number(item.averageDailyUsage || 0).toFixed(2) }}
                {{ item.unit }}/天
              </td>

              <!-- Forecast Demand -->

              <td>
                {{ Number(item.forecastDemand || 0).toFixed(2) }}
                {{ item.unit }}
              </td>

              <!-- Suggested Purchase -->

              <td>
                <strong
                  v-if="Number(item.suggestedPurchase || 0) > 0"
                  class="purchase-value"
                >
                  {{ Number(item.suggestedPurchase || 0).toFixed(2) }}
                  {{ item.unit }}
                </strong>

                <span v-else> 0 </span>
              </td>

              <!-- Estimated Cost -->

              <td>${{ Number(item.estimatedCost || 0).toFixed(2) }}</td>

              <!-- Status -->

              <td>
                <span
                  v-if="Number(item.suggestedPurchase || 0) > 0"
                  class="badge badge-danger"
                >
                  需要補貨
                </span>

                <span
                  v-else-if="item.currentStock <= item.minimumStock"
                  class="badge badge-warning"
                >
                  接近最低庫存
                </span>

                <span v-else class="badge badge-success"> 庫存充足 </span>
              </td>
            </tr>

            <!-- Empty -->

            <tr v-if="filteredForecasts.length === 0">
              <td colspan="8" class="empty-table">
                <div class="empty-icon">📦</div>

                <div class="empty-title">沒有符合條件的資料</div>

                <div class="empty-description">
                  目前沒有符合篩選條件的庫存預測資料。
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ========================================
   Page
======================================== */

.forecast-page {
  width: 100%;
}

/* ========================================
   Settings
======================================== */

.settings-card {
  margin-bottom: var(--spacing-5);
}

.forecast-settings {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-6);

  flex-wrap: wrap;
}

.setting {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-3);
}

.setting .form-label {
  margin-bottom: 0;
  white-space: nowrap;
}

.setting-input {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.setting-input .form-input {
  width: 80px;
}

.setting-unit {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.analyze-btn {
  min-width: 110px;
}

/* ========================================
   Summary
======================================== */

.summary-grid {
  display: grid;

  grid-template-columns: repeat(3, minmax(0, 1fr));

  gap: var(--spacing-4);

  margin-bottom: var(--spacing-5);
}

.stat-card {
  display: flex;
  align-items: center;

  gap: var(--spacing-4);

  padding: var(--spacing-5);

  background: var(--color-surface);

  border: 1px solid var(--color-border);

  border-radius: var(--radius-lg);

  box-shadow: var(--shadow-sm);
}

.stat-icon {
  width: 48px;
  height: 48px;

  display: flex;
  align-items: center;
  justify-content: center;

  flex-shrink: 0;

  border-radius: var(--radius-md);

  font-size: 20px;
  font-weight: var(--font-weight-bold);
}

.stat-icon-info {
  color: var(--color-info);
  background: var(--color-info-light);
}

.stat-icon-danger {
  color: var(--color-danger);
  background: var(--color-danger-light);
}

.stat-icon-success {
  color: var(--color-success);
  background: var(--color-success-light);
}

.stat-content {
  min-width: 0;
}

.stat-label {
  margin: 0 0 var(--spacing-1);

  color: var(--color-text-secondary);

  font-size: var(--font-size-sm);
}

.stat-value {
  margin: 0;

  color: var(--color-text-primary);

  font-size: 24px;
  font-weight: var(--font-weight-bold);
}

/* ========================================
   Forecast Info
======================================== */

.forecast-info {
  display: flex;
  align-items: center;

  gap: var(--spacing-2);

  margin-bottom: var(--spacing-5);

  padding: var(--spacing-3) var(--spacing-4);

  color: var(--color-info);

  background: var(--color-info-light);

  border-radius: var(--radius-md);

  font-size: var(--font-size-sm);
}

.info-icon {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 20px;
  height: 20px;

  border-radius: 50%;

  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.forecast-info strong {
  font-weight: var(--font-weight-semibold);
}

/* ========================================
   Toolbar
======================================== */

.toolbar-card {
  margin-bottom: var(--spacing-4);
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;

  gap: var(--spacing-2);
}

.forecast-filter {
  display: flex;
  gap: var(--spacing-2);
}

.filter-button {
  padding: 12px 16px;
  color: var(--color-text-secondary);
  background: var(--color-surface);

  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);

  font-size: var(--font-size-sm);

  cursor: pointer;

  transition:
    background var(--transition-fast),
    color var(--transition-fast),
    border-color var(--transition-fast);
}

.filter-button:hover {
  color: var(--color-text-primary);
  background: var(--color-bg);
}

.filter-button.active {
  color: var(--color-primary);

  background: var(--color-primary-light);

  border-color: var(--color-primary);
}

.forecast-sort {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.sort-label {
  color: var(--color-text-secondary);

  font-size: var(--font-size-sm);

  white-space: nowrap;
}

.sort-select {
  min-width: 190px;
}

/* ========================================
   Table
======================================== */

.forecast-card {
  overflow: hidden;
}

.purchase-value {
  color: var(--color-danger);
}

.empty-table {
  padding: 56px 20px !important;

  text-align: center;
}

.empty-icon {
  margin-bottom: var(--spacing-3);

  font-size: 32px;
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
   Responsive
======================================== */

@media (max-width: 1000px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .forecast-settings {
    align-items: stretch;
  }

  .setting {
    align-items: center;
  }
}

@media (max-width: 768px) {
  .toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .forecast-filter {
    width: 100%;
  }

  .filter-button {
    flex: 1;
  }

  .forecast-sort {
    width: 100%;
  }

  .sort-select {
    flex: 1;
  }

  .forecast-settings {
    flex-direction: column;
    align-items: stretch;
    gap: var(--spacing-4);
  }

  .setting {
    justify-content: space-between;
  }

  .analyze-btn {
    width: 100%;
  }
}
</style>
