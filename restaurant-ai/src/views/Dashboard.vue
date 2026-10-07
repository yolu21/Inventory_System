<script setup>
import { onMounted, computed } from "vue";

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

onMounted(async () => {
  await dashboardStore.loadDashboard();
});

/* ========================================
   Stock Color
======================================== */

const STOCK_COLORS = {
  LOW: "#dc2626",
  WARNING: "#d97706",
  NORMAL: "#16a34a",
};

const getStockColor = (stock) => {
  if (stock < 10) {
    return STOCK_COLORS.LOW;
  }

  if (stock < 30) {
    return STOCK_COLORS.WARNING;
  }

  return STOCK_COLORS.NORMAL;
};

/* ========================================
   Chart Data
======================================== */

/**
 * 所有食材庫存量
 */
const stockChartData = computed(() => {
  const ingredients = dashboardStore.overview?.ingredients ?? [];

  return {
    labels: ingredients.map((item) => item.name),

    datasets: [
      {
        label: "目前庫存",

        data: ingredients.map((item) => item.stock),

        backgroundColor: ingredients.map((item) => getStockColor(item.stock)),

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

        backgroundColor: ingredients.map((item) => getStockColor(item.stock)),

        borderRadius: 6,
        borderSkipped: false,
      },
    ],
  };
});

/**
 * 進貨 / 出貨量
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

const stockFlowChartOptions = {
  ...baseChartOptions,

  plugins: {
    ...baseChartOptions.plugins,

    title: {
      display: true,
      text: "食材進出貨量",
    },
  },
};

/* ========================================
   Computed Data
======================================== */

const lowStockItems = computed(() => {
  return dashboardStore.overview?.lowStock ?? [];
});

const topUsageItems = computed(() => {
  return dashboardStore.overview?.topUsage ?? [];
});
</script>

<template>
  <div class="dashboard-page">
    <!-- ====================================
         Page Header
    ===================================== -->

    <header class="page-header">
      <div>
        <h1 class="page-title">Dashboard</h1>

        <p class="page-description">
          Restaurant inventory overview and stock status.
        </p>
      </div>
    </header>

    <!-- ====================================
         Summary Cards
    ===================================== -->

    <section class="stat-grid dashboard-stats">
      <!-- Total Ingredients -->
      <div class="stat-card">
        <div class="stat-label">食材總數</div>

        <div class="stat-value">
          {{ dashboardStore.summary.totalIngredients }}
        </div>
      </div>

      <!-- Total IN -->
      <div class="stat-card">
        <div class="stat-label">總進貨量</div>

        <div class="stat-value">
          {{ dashboardStore.summary.totalIn }}
        </div>
      </div>

      <!-- Total OUT -->
      <div class="stat-card">
        <div class="stat-label">總出貨量</div>

        <div class="stat-value">
          {{ dashboardStore.summary.totalOut }}
        </div>
      </div>

      <!-- Low Stock -->
      <div
        class="stat-card"
        :class="{
          'stat-card-warning': dashboardStore.summary.lowStockIngredients > 0,
        }"
      >
        <div class="stat-label">低庫存食材</div>

        <div class="stat-value">
          {{ dashboardStore.summary.lowStockIngredients }}
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
          <h2 class="card-title">庫存進出量</h2>

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
          <h2 class="card-title">使用量最高食材</h2>

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

          <p class="card-description">Detailed stock movement by ingredient.</p>
        </div>
      </div>

      <div class="table-wrapper">
        <table v-if="dashboardStore.overview" class="table">
          <thead>
            <tr>
              <th>食材名稱</th>
              <th>進貨量</th>
              <th>出貨量</th>
              <th>目前庫存</th>
              <th>狀態</th>
            </tr>
          </thead>

          <tbody>
            <tr
              v-for="item in dashboardStore.overview.ingredients || []"
              :key="item.id"
            >
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
                  v-else-if="item.stock < item.maximumStock + 5"
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
   Responsive
======================================== */

@media (max-width: 1000px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .chart-container-large {
    height: 300px;
  }

  .chart-container-medium {
    height: 250px;
  }
}
</style>
