<script setup>
import { onMounted, computed, ref } from "vue";
import { useDashboardStore } from "../stores/dashboard";

import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from "chart.js";

import { Bar } from "vue-chartjs";

const dashboardStore = useDashboardStore();

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
);

/* ========================================
   Dashboard
======================================== */

onMounted(async () => {
  await dashboardStore.loadDashboard();
});

/* ========================================
   Month Navigation
======================================== */

const selectedYear = computed({
  get: () => dashboardStore.selectedYear,
  set: (value) => {
    dashboardStore.changeMonth(Number(value), dashboardStore.selectedMonth);
  },
});

const selectedMonth = computed({
  get: () => dashboardStore.selectedMonth,
  set: (value) => {
    dashboardStore.changeMonth(dashboardStore.selectedYear, Number(value));
  },
});

const previousMonth = async () => {
  await dashboardStore.previousMonth();
};

const nextMonth = async () => {
  await dashboardStore.nextMonth();
};

/* ========================================
   Month Display
======================================== */

const currentMonthLabel = computed(() => {
  return `${dashboardStore.selectedYear} 年 ${dashboardStore.selectedMonth} 月`;
});

/* ========================================
   Pagination
======================================== */

const currentPage = ref(1);
const pageSize = 20;

const inventoryItems = computed(() => {
  return dashboardStore.overview?.ingredients ?? [];
});

const totalPages = computed(() => {
  return Math.max(1, Math.ceil(inventoryItems.value.length / pageSize));
});

const paginatedInventoryItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize;

  return inventoryItems.value.slice(start, start + pageSize);
});

const goToPage = (page) => {
  if (page < 1 || page > totalPages.value) return;

  currentPage.value = page;
};

/* ========================================
   Stock Color
======================================== */

const STOCK_COLORS = {
  LOW: "#dc2626",
  WARNING: "#d97706",
  NORMAL: "#16a34a",
};

const getStockColor = (stock, minimumStock) => {
  if (stock < minimumStock) {
    return STOCK_COLORS.LOW;
  }

  if (stock < minimumStock * 1.5) {
    return STOCK_COLORS.WARNING;
  }

  return STOCK_COLORS.NORMAL;
};

/* ========================================
   Chart Data
======================================== */

/**
 * 所有食材目前庫存量
 */
const stockChartData = computed(() => {
  const ingredients = dashboardStore.overview?.ingredients ?? [];

  return {
    labels: ingredients.map((item) => item.name),

    datasets: [
      {
        label: "目前庫存",
        data: ingredients.map((item) => item.stock),

        backgroundColor: ingredients.map((item) =>
          getStockColor(item.stock, item.minimumStock),
        ),

        borderRadius: 6,
        borderSkipped: false,
      },
    ],
  };
});

/**
 * 低庫存食材
 */
const lowStockChartData = computed(() => {
  const ingredients =
    dashboardStore.overview?.ingredients?.filter(
      (item) => item.stock < item.minimumStock,
    ) ?? [];

  return {
    labels: ingredients.map((item) => item.name),

    datasets: [
      {
        label: "目前庫存",

        data: ingredients.map((item) => item.stock),

        backgroundColor: ingredients.map((item) =>
          getStockColor(item.stock, item.minimumStock),
        ),

        borderRadius: 6,
        borderSkipped: false,
      },
    ],
  };
});

/**
 * 選擇月份的進貨 / 出貨量
 */
const stockFlowChartData = computed(() => {
  const ingredients = dashboardStore.overview?.ingredients ?? [];

  return {
    labels: ingredients.map((item) => item.name),

    datasets: [
      {
        label: "進貨量",

        data: ingredients.map((item) => item.in),

        backgroundColor: "#2563eb",

        borderRadius: 6,
        borderSkipped: false,
      },

      {
        label: "出貨量",

        data: ingredients.map((item) => item.out),

        backgroundColor: "#94a3b8",

        borderRadius: 6,
        borderSkipped: false,
      },
    ],
  };
});

/* ========================================
   Chart Options
======================================== */

const baseChartOptions = {
  responsive: true,
  maintainAspectRatio: false,

  plugins: {
    legend: {
      position: "bottom",
    },
  },

  scales: {
    x: {
      grid: {
        display: false,
      },
    },

    y: {
      beginAtZero: true,
    },
  },
};

const stockChartOptions = {
  ...baseChartOptions,

  indexAxis: "y",

  plugins: {
    ...baseChartOptions.plugins,

    title: {
      display: true,
      text: "各食材目前庫存量",
    },
  },
};

const lowStockChartOptions = {
  ...baseChartOptions,

  indexAxis: "y",

  plugins: {
    ...baseChartOptions.plugins,

    legend: {
      display: false,
    },

    title: {
      display: true,
      text: "目前低庫存食材",
    },
  },
};

const stockFlowChartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,

  plugins: {
    legend: {
      position: "top",
    },
    title: {
      display: true,
      text: `${currentMonthLabel.value}進出貨量`,
    },
  },
}));

/* ========================================
   Computed Data
======================================== */

const lowStockItems = computed(() => {
  return dashboardStore.overview?.lowStock ?? [];
});

const topUsageItems = computed(() => {
  return dashboardStore.overview?.topUsage ?? [];
});

/* ========================================
   Pagination Watch
======================================== */

const inventoryPageInfo = computed(() => {
  if (inventoryItems.value.length === 0) {
    return "0 筆";
  }

  const start = (currentPage.value - 1) * pageSize + 1;

  const end = Math.min(
    currentPage.value * pageSize,
    inventoryItems.value.length,
  );

  return `${start}-${end} / 共 ${inventoryItems.value.length} 筆`;
});
</script>

<template>
  <div class="dashboard-page">
    <!-- ====================================
         Page Header
    ===================================== -->

    <header class="page-header dashboard-header">
      <div>
        <h1 class="page-title">Dashboard</h1>

        <p class="page-description">
          Restaurant inventory overview and stock status.
        </p>
      </div>

      <!-- Month Selector -->

      <div class="dashboard-period">
        <button
          class="btn btn-secondary"
          type="button"
          :disabled="dashboardStore.loading"
          @click="previousMonth"
        >
          ←
        </button>

        <select v-model="selectedYear" class="form-select period-select">
          <option
            v-for="year in [
              dashboardStore.selectedYear - 2,
              dashboardStore.selectedYear - 1,
              dashboardStore.selectedYear,
              dashboardStore.selectedYear + 1,
              dashboardStore.selectedYear + 2,
            ]"
            :key="year"
            :value="year"
          >
            {{ year }} 年
          </option>
        </select>

        <select v-model="selectedMonth" class="form-select period-select">
          <option v-for="month in 12" :key="month" :value="month">
            {{ month }} 月
          </option>
        </select>

        <button
          class="btn btn-secondary"
          type="button"
          :disabled="dashboardStore.loading"
          @click="nextMonth"
        >
          →
        </button>
      </div>
    </header>

    <!-- ====================================
         Selected Month
    ===================================== -->

    <div class="dashboard-period-label">
      <span class="badge badge-info">
        {{ currentMonthLabel }}
      </span>

      <span class="period-hint">
        進貨量、出貨量與使用量排行依選擇月份統計
      </span>
    </div>

    <!-- ====================================
         Summary Cards
    ===================================== -->

    <section class="stat-grid dashboard-stats">
      <!-- Total Ingredients -->

      <div class="stat-card">
        <div class="stat-label">食材總數</div>

        <div class="stat-value">
          {{ dashboardStore.summary.totalIngredients ?? 0 }}
        </div>
      </div>

      <!-- Monthly IN -->

      <div class="stat-card">
        <div class="stat-label">本月進貨量</div>

        <div class="stat-value">
          {{ dashboardStore.summary.monthlyIn ?? 0 }}
        </div>
      </div>

      <!-- Monthly OUT -->

      <div class="stat-card">
        <div class="stat-label">本月出貨量</div>

        <div class="stat-value">
          {{ dashboardStore.summary.monthlyOut ?? 0 }}
        </div>
      </div>

      <!-- Low Stock -->

      <div
        class="stat-card"
        :class="{
          'stat-card-warning': dashboardStore.summary.lowStockCount > 0,
        }"
      >
        <div class="stat-label">低庫存食材</div>

        <div class="stat-value">
          {{ dashboardStore.summary.lowStockCount ?? 0 }}
        </div>
      </div>
    </section>

    <!-- ====================================
         Main Dashboard Grid
    ===================================== -->

    <section class="dashboard-grid">
      <!-- Stock Overview -->

      <div class="card dashboard-card">
        <div class="card-header">
          <div>
            <h2 class="card-title">庫存概覽</h2>

            <p class="card-description">
              Current stock level of each ingredient.
            </p>
          </div>
        </div>

        <div class="card-body">
          <div
            v-if="dashboardStore.overview"
            class="chart-container chart-container-large"
          >
            <Bar :data="stockChartData" :options="stockChartOptions" />
          </div>

          <div v-else class="empty-state">暫無庫存資料</div>
        </div>
      </div>

      <!-- Low Stock -->

      <div class="card dashboard-card">
        <div class="card-header">
          <div>
            <h2 class="card-title">低庫存食材</h2>

            <p class="card-description">Ingredients that require attention.</p>
          </div>

          <span v-if="lowStockItems.length > 0" class="badge badge-danger">
            {{ lowStockItems.length }} 項
          </span>

          <span v-else class="badge badge-success"> 正常 </span>
        </div>

        <div class="card-body">
          <div v-if="lowStockItems.length > 0" class="low-stock-list">
            <div
              v-for="item in lowStockItems"
              :key="item.id"
              class="low-stock-item"
            >
              <div class="low-stock-info">
                <div class="low-stock-name">
                  {{ item.name }}
                </div>

                <div class="low-stock-stock">目前庫存 {{ item.stock }}</div>
              </div>

              <span class="badge badge-danger"> 低庫存 </span>
            </div>
          </div>

          <div v-else class="empty-state">
            <div class="empty-icon">✓</div>

            <div>目前沒有低庫存食材</div>
          </div>
        </div>
      </div>
    </section>

    <!-- ====================================
         Low Stock Chart
    ===================================== -->

    <section class="card dashboard-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">低庫存分析</h2>

          <p class="card-description">
            Current stock level of ingredients below the threshold.
          </p>
        </div>
      </div>

      <div class="card-body">
        <div
          v-if="lowStockItems.length > 0"
          class="chart-container chart-container-medium"
        >
          <Bar :data="lowStockChartData" :options="lowStockChartOptions" />
        </div>

        <div v-else class="empty-state">目前沒有低庫存資料</div>
      </div>
    </section>

    <!-- ====================================
         Stock Flow
    ===================================== -->

    <section class="card dashboard-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">{{ currentMonthLabel }}進出貨量</h2>

          <p class="card-description">
            Comparison between stock-in and stock-out quantities.
          </p>
        </div>
      </div>

      <div class="card-body">
        <div class="chart-container chart-container-large">
          <Bar :data="stockFlowChartData" :options="stockFlowChartOptions" />
        </div>
      </div>
    </section>

    <!-- ====================================
         Top Usage
    ===================================== -->

    <section class="card dashboard-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">{{ currentMonthLabel }}使用量最高食材</h2>

          <p class="card-description">
            Ingredients with the highest stock-out quantity.
          </p>
        </div>
      </div>

      <div class="card-body">
        <div v-if="topUsageItems.length > 0" class="usage-list">
          <div
            v-for="(item, index) in topUsageItems"
            :key="item.id"
            class="usage-item"
          >
            <div class="usage-rank">
              {{ index + 1 }}
            </div>

            <div class="usage-info">
              <div class="usage-name">
                {{ item.name }}
              </div>

              <div class="usage-bar">
                <div
                  class="usage-bar-fill"
                  :style="{
                    width: `${Math.min(
                      (item.out / (topUsageItems[0]?.out || 1)) * 100,
                      100,
                    )}%`,
                  }"
                />
              </div>
            </div>

            <div class="usage-value">
              {{ item.out }}
            </div>
          </div>
        </div>

        <div v-else class="empty-state">暫無使用量資料</div>
      </div>
    </section>

    <!-- ====================================
         Inventory Table
    ===================================== -->

    <section class="card dashboard-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">庫存明細</h2>

          <p class="card-description">
            {{ currentMonthLabel }}進出貨量與目前庫存。
          </p>
        </div>

        <div class="table-info">
          {{ inventoryPageInfo }}
        </div>
      </div>

      <div class="table-wrapper">
        <table v-if="dashboardStore.overview" class="table">
          <thead>
            <tr>
              <th>食材名稱</th>
              <th>{{ dashboardStore.selectedMonth }}月進貨量</th>
              <th>{{ dashboardStore.selectedMonth }}月出貨量</th>
              <th>目前庫存</th>
              <th>狀態</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="item in paginatedInventoryItems" :key="item.id">
              <td>
                <strong>
                  {{ item.name }}
                </strong>
              </td>

              <td>
                {{ item.in }}
              </td>

              <td>
                {{ item.out }}
              </td>

              <td>
                {{ item.stock }}
              </td>

              <td>
                <span
                  v-if="item.stock < item.minimumStock"
                  class="badge badge-danger"
                >
                  低庫存
                </span>

                <span
                  v-else-if="item.stock < item.minimumStock * 1.5"
                  class="badge badge-warning"
                >
                  注意
                </span>

                <span v-else class="badge badge-success"> 正常 </span>
              </td>
            </tr>
          </tbody>
        </table>

        <div v-else class="empty-state">暫無庫存資料</div>
      </div>

      <!-- Pagination -->

      <div v-if="totalPages > 1" class="pagination">
        <button
          class="btn btn-secondary"
          type="button"
          :disabled="currentPage === 1"
          @click="goToPage(currentPage - 1)"
        >
          上一頁
        </button>

        <div class="pagination-pages">
          <button
            v-for="page in totalPages"
            :key="page"
            class="pagination-page"
            :class="{
              active: currentPage === page,
            }"
            type="button"
            @click="goToPage(page)"
          >
            {{ page }}
          </button>
        </div>

        <button
          class="btn btn-secondary"
          type="button"
          :disabled="currentPage === totalPages"
          @click="goToPage(currentPage + 1)"
        >
          下一頁
        </button>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ========================================
   Dashboard
======================================== */

.dashboard-page {
  width: 100%;
}

.dashboard-stats {
  margin-bottom: var(--spacing-6);
}

/* ========================================
   Dashboard Header
======================================== */

.dashboard-header {
  align-items: flex-start;
}

.dashboard-period {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.period-select {
  min-width: 110px;
}

/* ========================================
   Period Label
======================================== */

.dashboard-period-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
  margin-bottom: var(--spacing-5);
}

.period-hint {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

/* ========================================
   Stat Card
======================================== */

.stat-card-warning {
  border-color: #fed7aa;
  background: #fffaf5;
}

/* ========================================
   Dashboard Grid
======================================== */

.dashboard-grid {
  display: grid;
  grid-template-columns:
    minmax(0, 2fr)
    minmax(320px, 1fr);

  gap: var(--spacing-5);
  margin-bottom: var(--spacing-5);
}

.dashboard-card {
  margin-bottom: var(--spacing-5);
}

.dashboard-card .card-header {
  min-height: 78px;
}

.card-description {
  margin: 4px 0 0;
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

/* ========================================
   Chart
======================================== */

.chart-container {
  position: relative;
  width: 100%;
}

.chart-container-large {
  height: 380px;
}

.chart-container-medium {
  height: 300px;
}

/* ========================================
   Low Stock
======================================== */

.low-stock-list {
  display: flex;
  flex-direction: column;
}

.low-stock-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-3);
  padding: var(--spacing-4) 0;
  border-bottom: 1px solid var(--color-border-light);
}

.low-stock-item:last-child {
  border-bottom: none;
}

.low-stock-name {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
}

.low-stock-stock {
  margin-top: 3px;
  color: var(--color-text-secondary);
  font-size: var(--font-size-xs);
}

/* ========================================
   Empty State
======================================== */

.empty-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  margin-bottom: var(--spacing-3);
  border-radius: var(--radius-full);
  color: var(--color-success);
  background: var(--color-success-light);
  font-weight: var(--font-weight-bold);
}

/* ========================================
   Usage List
======================================== */

.usage-list {
  display: flex;
  flex-direction: column;
}

.usage-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-4);
  padding: var(--spacing-4) 0;
  border-bottom: 1px solid var(--color-border-light);
}

.usage-item:last-child {
  border-bottom: none;
}

.usage-rank {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  flex-shrink: 0;
  border-radius: var(--radius-full);
  color: var(--color-primary);
  background: var(--color-primary-light);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-bold);
}

.usage-info {
  flex: 1;
  min-width: 0;
}

.usage-name {
  margin-bottom: var(--spacing-2);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
}

.usage-bar {
  width: 100%;
  height: 6px;
  overflow: hidden;
  border-radius: var(--radius-full);
  background: var(--color-border-light);
}

.usage-bar-fill {
  height: 100%;
  border-radius: inherit;
  background: var(--color-primary);
}

.usage-value {
  min-width: 48px;
  text-align: right;
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
}

/* ========================================
   Table Info
======================================== */

.table-info {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

/* ========================================
   Pagination
======================================== */

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-3);
  padding: var(--spacing-4);
  border-top: 1px solid var(--color-border-light);
}

.pagination-pages {
  display: flex;
  align-items: center;
  gap: 4px;
}

.pagination-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 34px;
  height: 34px;
  padding: 0 8px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  background: var(--color-surface);
  cursor: pointer;
  transition: var(--transition-fast);
}

.pagination-page:hover {
  color: var(--color-primary);
  border-color: var(--color-primary);
}

.pagination-page.active {
  color: #fff;
  border-color: var(--color-primary);
  background: var(--color-primary);
}

/* ========================================
   Responsive
======================================== */

@media (max-width: 1000px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }

  .dashboard-header {
    flex-direction: column;
  }
}

@media (max-width: 768px) {
  .dashboard-period {
    width: 100%;
  }

  .period-select {
    flex: 1;
    min-width: 0;
  }

  .dashboard-period-label {
    align-items: flex-start;
    flex-direction: column;
  }

  .chart-container-large {
    height: 300px;
  }

  .chart-container-medium {
    height: 250px;
  }

  .pagination {
    flex-wrap: wrap;
  }

  .pagination-pages {
    order: -1;
    width: 100%;
    justify-content: center;
  }
}
</style>
