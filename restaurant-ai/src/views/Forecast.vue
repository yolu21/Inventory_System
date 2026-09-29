<script setup>
import { ref, computed, onMounted } from "vue";
import { api } from "../services/api";

const forecasts = ref([]);
const loading = ref(false);

//是否只顯示需要補貨的食材
const showNeedReplenishment = ref(false);

//排序方式
const sortBy = ref("default");

// 預設分析條件
const usageDays = ref(14); //使用最近 14 天資料
const forecastDays = ref(7); //預測未來 7 天
//食材總數
const totalIngredients = computed(() => {
  return forecasts.value.length;
});

//需要補貨食材數量
const needReplenishment = computed(() => {
  return forecasts.value.filter((item) => item.suggestedPurchase > 0).length;
});

//預估補貨總成本
const totalEstimatedCost = computed(() => {
  return forecasts.value.reduce((sum, item) => sum + item.estimatedCost, 0);
});

const filteredForecasts = computed(() => {
  let result = [...forecasts.value];

  //篩選
  if (showNeedReplenishment.value) {
    result = result.filter((item) => Number(item.suggestedPurchase || 0) > 0);
  }

  //排序
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
const getStatus = (item) => {
  if (item.suggestedPurchase > 0) {
    return "需要補貨";
  }

  if (item.currentStock <= item.minimumStock) {
    return "接近最低庫存";
  }
  return "庫存充足";
};
onMounted(() => {
  loadForecast();
});
</script>
<template>
  <div class="page">
    <h1>📊 庫存預測</h1>
    <div class="forecast-settings">
      <div class="setting">
        <label>使用歷史資料</label>

        <input v-model.number="usageDays" type="number" min="1" />

        <span>天</span>
      </div>
      <div class="setting">
        <label>預測未來需求</label>
        <input v-model.number="forecastDays" type="number" min="1" />

        <span>天</span>
      </div>
      <button @click="loadForecast">🔄 重新分析</button>
    </div>
    <div class="summary-cards">
      <div class="summary-card">
        <div class="summary-icon">📦</div>
        <div>
          <p class="summary-label">食材總數</p>
          <h2>{{ totalIngredients }}</h2>
        </div>
      </div>
      <div class="summary-card">
        <div class="summary-icon">🔴</div>
        <div>
          <p class="summary-label">需要補貨</p>
          <h2>{{ needReplenishment }}</h2>
        </div>
      </div>

      <div class="summary-card">
        <div class="summary-icon">💰</div>
        <div>
          <p class="summary-label">預估補貨總成本</p>
          <h2>${{ totalEstimatedCost.toFixed(2) }}</h2>
        </div>
      </div>
    </div>
    <p class="forecast-info">
      使用最近 {{ usageDays }} 天的庫存使用紀錄， 預測未來
      {{ forecastDays }} 天的需求。
    </p>
    <div class="forecast-toolbar">
      <div class="forecast-filter">
        <button
          :class="{ active: !showNeedReplenishment }"
          @click="showNeedReplenishment = false"
        >
          全部
        </button>
        <button
          :class="{ active: showNeedReplenishment }"
          @click="showNeedReplenishment = true"
        >
          🔴 需要補貨
        </button>
      </div>
      <div class="forecast-sort">
        <label>排序：</label>

        <select v-model="sortBy">
          <option value="default">預設順序</option>
          <option value="purchase-desc">建議補貨量：高 → 低</option>
          <option value="cost-desc">預估成本：高 → 低</option>
          <option value="stock-asc">目前庫存：低 → 高</option>
        </select>
      </div>
    </div>
    <p v-if="loading">分析庫存中...</p>

    <table v-else>
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
          <td>
            {{ item.ingredientName }}
          </td>

          <td>{{ item.currentStock }} {{ item.unit }}</td>

          <td>{{ item.minimumStock }} {{ item.unit }}</td>

          <td>
            {{ item.averageDailyUsage.toFixed(2) }}
            {{ item.unit }}/天
          </td>

          <td>
            {{ item.forecastDemand.toFixed(2) }}
            {{ item.unit }}
          </td>
          <td>
            <strong v-if="item.suggestedPurchase > 0">
              {{ item.suggestedPurchase.toFixed(2) }}
              {{ item.unit }}
            </strong>
            <span v-else> 0 </span>
          </td>
          <td>${{ item.estimatedCost.toFixed(2) }}</td>
          <td>
            <span v-if="item.suggestedPurchase > 0" class="danger">
              🔴 {{ getStatus(item) }}
            </span>

            <span
              v-else-if="item.currentStock <= item.minimumStock"
              class="warning"
            >
              🟡 {{ getStatus(item) }}
            </span>

            <span v-else class="normal"> 🟢 {{ getStatus(item) }} </span>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<style scoped>
.forecast-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 20px 0;
}

.forecast-filter {
  display: flex;
  gap: 8px;
}

.forecast-filter button {
  padding: 8px 16px;
  border: 1px solid #ddd;
  background: white;
  border-radius: 6px;
  cursor: pointer;
}

.forecast-filter button.active {
  background: #333;
  color: white;
  border-color: #333;
}

.forecast-sort {
  display: flex;
  align-items: center;
  gap: 8px;
}

.forecast-sort select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
}
.forecast-settings {
  display: flex;
  align-items: center;
  gap: 20px;
  margin: 20px 0 10px;
  padding: 16px;
  background: #f8f8f8;
  border-radius: 8px;
}

.setting {
  display: flex;
  align-items: center;
  gap: 8px;
}

.setting label {
  font-weight: bold;
}

.setting input {
  width: 80px;
  padding: 8px 10px;
  border: 1px solid #ddd;
  border-radius: 6px;
  margin: 0;
}

.forecast-settings button {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 12px;
  border-bottom: 1px solid #ddd;
  text-align: left;
}

.danger {
  color: #d32f2f;
  font-weight: bold;
}

.warning {
  color: #f57c00;
  font-weight: bold;
}

.normal {
  color: #388e3c;
  font-weight: bold;
}
.summary-cards {
  display: flex;
  gap: 20px;
  margin: 20px 0;
}

.summary-card {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: white;
  border: 1px solid #e5e5e5;
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.summary-icon {
  font-size: 32px;
}

.summary-label {
  margin: 0 0 5px;
  color: #666;
  font-size: 14px;
}

.summary-card h2 {
  margin: 0;
  font-size: 28px;
}
</style>
