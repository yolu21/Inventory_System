<script setup>
import { ref, onMounted } from "vue";
import { api } from "../services/api";

const summary = ref({});
const overview = ref(null);

const loadsummary = async () => {
  try {
    const res = await api.get("/Dashboard/summary");
    summary.value = res.data;
  } catch (error) {
    console.error(error);
  }
};

const loadOverview = async () => {
  try {
    const res = await api.get("/Dashboard/overview");
    overview.value = res.data;
  } catch (error) {
    console.error(error);
  }
};

onMounted(async () => {
  await loadsummary();
  await loadOverview();
});
</script>
<template>
  <div class="page">
    <h1>📊 Dashboard</h1>
    <!--所有庫存量-->
    <div class="cards">
      <div class="card">
        <h3>食材總數</h3>
        <p>{{ summary.totalIngredients }}</p>
      </div>
      <div class="card">
        <h3>總進貨量</h3>
        <p>{{ summary.totalIn }}</p>
      </div>
      <div class="card">
        <h3>總出貨量</h3>
        <p>{{ summary.totalOut }}</p>
      </div>
      <div class="card warning">
        <h3>低庫存食材</h3>
        <p>{{ summary.lowStockIngredients }}</p>
      </div>
    </div>
    <hr />

    <!--各食材庫存) -->
    <h2>📦庫存概覽</h2>

    <table v-if="overview">
      <thead>
        <tr>
          <th>食材名稱</th>
          <th>進貨量</th>
          <th>出貨量</th>
          <th>庫存</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in overview?.ingredients || []" :key="item.id">
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

    <!--低庫存食材-->
    <h2>⚠️ 低庫存食材</h2>
    <ul>
      <li v-for="item in overview?.lowStock || []" :key="item.id">
        {{ item.name }} - 庫存: {{ item.stock }}
      </li>
    </ul>

    <hr />
    <!--最多用量-->

    <h2>最多用量食材</h2>
    <ol>
      <li v-for="item in overview?.topUsage || []" :key="item.id">
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
</style>
