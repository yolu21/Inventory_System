<script setup>
import { ref, onMounted, computed } from "vue";
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

//=================
// 庫存量 Chart
//=================
const stockChartData = computed(() => {
  const ingredients = dashboardStore.overview?.ingredients ?? [];

  return {
    labels: ingredients.map((item) => item.name),
    datasets: [
      {
        label: "庫存量",
        backgroundColor: "#42A5F5",
        data: ingredients.map((item) => item.stock),
      },
    ],
  };
});

const stockChartOptions = {
  responsive: true,
  indexAxis: "y", // 設定為水平條形圖
  maintainAspectRatio: false, // 設定為 false 以允許自動調整高度，依照CSS樣式設定高度
  plugins: {
    legend: {
      position: "bottom",
    },
    title: {
      display: true,
      text: "各食材庫存量",
    },
  },
};
</script>
<template>
  <div class="page">
    <h1>📊 Dashboard</h1>
    <!--所有庫存量-->
    <div class="cards">
      <div class="card">
        <h3>食材總數</h3>
        <p>{{ dashboardStore.summary.totalIngredients }}</p>
      </div>
      <div class="card">
        <h3>總進貨量</h3>
        <p>{{ dashboardStore.summary.totalIn }}</p>
      </div>
      <div class="card">
        <h3>總出貨量</h3>
        <p>{{ dashboardStore.summary.totalOut }}</p>
      </div>
      <div class="card warning">
        <h3>低庫存食材</h3>
        <p>{{ dashboardStore.summary.lowStockIngredients }}</p>
      </div>
    </div>
    <hr />

    <!--各食材庫存) -->
    <h2>📦庫存概覽</h2>

    <table v-if="dashboardStore.overview">
      <thead>
        <tr>
          <th>食材名稱</th>
          <th>進貨量</th>
          <th>出貨量</th>
          <th>庫存</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="item in dashboardStore.overview?.ingredients || []"
          :key="item.id"
        >
          <td>{{ item.name }}</td>
          <td>{{ item.in }}</td>
          <td>{{ item.out }}</td>
          <td>
            {{ item.stock }}
            <span v-if="item.stock < 10" class="warning"> ⚠️(低庫存) </span>
          </td>
        </tr>
      </tbody>
    </table>
    <h2>📊 庫存圖表</h2>
    <div class="chart-container">
      <Bar :data="stockChartData" :options="stockChartOptions" />
    </div>
    <!--低庫存食材-->
    <h2>⚠️ 低庫存食材</h2>
    <ul>
      <li
        v-for="item in dashboardStore.overview?.lowStock || []"
        :key="item.id"
      >
        {{ item.name }} - 庫存: {{ item.stock }}
      </li>
    </ul>

    <hr />
    <!--最多用量-->

    <h2>最多用量食材</h2>
    <ol>
      <li
        v-for="item in dashboardStore.overview?.topUsage || []"
        :key="item.id"
      >
        {{ item.name }} - 用量: {{ item.out }}
      </li>
    </ol>
  </div>
</template>
<style scoped>
.cards {
  display: grid;
  gap: 15px;
  grid-template-columns: repeat(4, 1fr);
  margin-bottom: 20px;
}
.card {
  background: #f5f5f5;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
}
.card p {
  font-size: 24px;
  font-weight: bold;
  margin: 0;
}
.chart-container {
  width: 100%;
  max-width: 800px;
  height: 400px;
  margin: 0 auto;
}
</style>
