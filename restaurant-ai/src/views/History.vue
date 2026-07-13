<script setup>
import { ref, onMounted, computed } from "vue";
import { useHistoryStore } from "../stores/history";

const historyStore = useHistoryStore();

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("zh-TW");
  //日期+時間寫法
  //return new Date(date).toLocaleString("zh-TW");
};

const keyword = ref("");
const typeFilter = ref("all");
const sortBy = ref("name");
const filteredIngredients = computed(() => {
  let list = [...historyStore.history];

  //搜尋
  if (keyword.value) {
    list = list.filter((item) =>
      item.ingredientName.toLowerCase().includes(keyword.value.toLowerCase()),
    );
  }

  //篩選
  if (typeFilter.value === "IN") {
    list = list.filter((item) => item.type === "IN");
  }

  if (typeFilter.value === "OUT") {
    list = list.filter((item) => item.type === "OUT");
  }

  //排序
  switch (sortBy.value) {
    case "quantity":
      list.sort((a, b) => b.quantity - a.quantity);
      break;

    case "name":
      list.sort((a, b) => a.ingredientName.localeCompare(b.ingredientName));
      break;

    case "date":
      list.sort((a, b) => new Date(b.date) - new Date(a.date));
      break;
  }

  return list;
});
onMounted(async () => {
  await historyStore.loadHistory();
});
</script>
<template>
  <div class="page">
    <h1>📜 歷史紀錄</h1>
    <div class="toolbar">
      <input v-model="keyword" placeholder="🔍 Search Ingredient" />

      <select v-model="typeFilter">
        <option value="all">All</option>
        <option value="IN">IN</option>
        <option value="OUT">OUT</option>
      </select>

      <select v-model="sortBy">
        <option value="name">Name</option>
        <option value="quantity">Quantity</option>
        <option value="date">Date descending</option>
      </select>
    </div>
    <table v-if="historyStore.history.length > 0">
      <thead>
        <tr>
          <th>日期</th>
          <th>食材名稱</th>
          <th>類型</th>
          <th>數量</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="record in filteredIngredients" :key="record.id">
          <td>{{ formatDate(record.date) }}</td>
          <td>{{ record.ingredientName }}</td>
          <td>
            <span :class="record.type === 'IN' ? 'in-tag' : 'out-tag'">
              {{ record.type }}
            </span>
          </td>
          <td>{{ record.quantity }}</td>
        </tr>
      </tbody>
    </table>
    <p v-else>目前沒有歷史紀錄。</p>
  </div>
</template>
<style scoped>
.in-tag {
  color: #16a34a;
  font-weight: bold;
}

.out-tag {
  color: #dc2626;
  font-weight: bold;
}

.toolbar {
  display: flex;
  gap: 12px;
  margin: 20px 0;
  flex-wrap: wrap;
}

input,
select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
}
</style>
