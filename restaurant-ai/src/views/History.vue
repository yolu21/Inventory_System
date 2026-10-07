<script setup>
import { ref, onMounted, computed } from "vue";
import { useHistoryStore } from "../stores/history";

const historyStore = useHistoryStore();

/* ========================================
   Date
======================================== */

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("zh-TW");

  // 如果之後需要顯示日期 + 時間：
  // return new Date(date).toLocaleString("zh-TW");
};

/* ========================================
   Search / Filter / Sort
======================================== */

const keyword = ref("");
const typeFilter = ref("all");
const sortBy = ref("name");

const filteredHistory = computed(() => {
  let list = [...historyStore.history];

  // 搜尋
  if (keyword.value) {
    const searchKeyword = keyword.value.toLowerCase();

    list = list.filter((item) =>
      item.ingredientName.toLowerCase().includes(searchKeyword),
    );
  }

  // 篩選
  if (typeFilter.value === "IN") {
    list = list.filter((item) => item.type === "IN");
  }

  if (typeFilter.value === "OUT") {
    list = list.filter((item) => item.type === "OUT");
  }

  // 排序
  switch (sortBy.value) {
    case "quantity":
      list.sort((a, b) => Number(b.quantity || 0) - Number(a.quantity || 0));
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

/* ========================================
   Lifecycle
======================================== */

onMounted(async () => {
  await historyStore.loadHistory();
});
</script>

<template>
  <div class="history-page">
    <!-- ====================================
         Page Header
    ===================================== -->

    <header class="page-header">
      <div>
        <h1 class="page-title">歷史紀錄</h1>

        <p class="page-description">查看食材庫存的進貨與出庫異動紀錄。</p>
      </div>
    </header>

    <!-- ====================================
         Filter Toolbar
    ===================================== -->

    <section class="card filter-card">
      <div class="card-body">
        <div class="filter-row">
          <!-- Search -->

          <div class="search-wrapper">
            <span class="search-icon"> 🔍 </span>

            <input
              v-model="keyword"
              class="form-input search-input"
              type="text"
              placeholder="搜尋食材名稱..."
            />
          </div>

          <!-- Type -->

          <select v-model="typeFilter" class="form-select filter-select">
            <option value="all">全部異動</option>

            <option value="IN">進貨 IN</option>

            <option value="OUT">出庫 OUT</option>
          </select>

          <!-- Sort -->

          <select
            v-model="sortBy"
            class="form-select filter-select sort-select"
          >
            <option value="name">依食材名稱</option>

            <option value="quantity">依數量：高 → 低</option>

            <option value="date">依日期：新 → 舊</option>
          </select>
        </div>
      </div>
    </section>

    <!-- ====================================
         History Table
    ===================================== -->

    <section class="card history-card">
      <div class="card-header">
        <div>
          <h2 class="card-title">庫存異動紀錄</h2>

          <p class="card-description">共 {{ filteredHistory.length }} 筆紀錄</p>
        </div>
      </div>

      <!-- Has History -->

      <div v-if="historyStore.history.length > 0" class="table-wrapper">
        <table class="table">
          <thead>
            <tr>
              <th>日期</th>
              <th>食材名稱</th>
              <th>類型</th>
              <th>數量</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="record in filteredHistory" :key="record.id">
              <!-- Date -->

              <td>
                {{ formatDate(record.date) }}
              </td>

              <!-- Ingredient -->

              <td>
                <strong>
                  {{ record.ingredientName }}
                </strong>
              </td>

              <!-- Type -->

              <td>
                <span v-if="record.type === 'IN'" class="badge badge-success">
                  IN
                </span>

                <span v-else class="badge badge-danger"> OUT </span>
              </td>

              <!-- Quantity -->

              <td>
                {{ record.quantity }}
              </td>
            </tr>

            <!-- No Search Result -->

            <tr v-if="filteredHistory.length === 0">
              <td colspan="4" class="empty-table">
                <div class="empty-icon">🔍</div>

                <div class="empty-title">找不到符合條件的紀錄</div>

                <div class="empty-description">
                  請嘗試調整搜尋條件或篩選條件。
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- No History -->

      <div v-else class="empty-state">
        <div class="empty-icon">📜</div>

        <div class="empty-title">目前沒有歷史紀錄</div>

        <div class="empty-description">
          當食材進貨或出庫後，相關紀錄會顯示在這裡。
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ========================================
   Page
======================================== */

.history-page {
  width: 100%;
}

/* ========================================
   Filter
======================================== */

.filter-card {
  margin-bottom: var(--spacing-5);
}

.filter-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
}

.search-wrapper {
  position: relative;
  flex: 1;
}

.search-icon {
  position: absolute;

  left: 12px;
  top: 50%;

  transform: translateY(-50%);

  pointer-events: none;
}

.search-input {
  padding-left: 38px;
}

.filter-select {
  width: 170px;
}

.sort-select {
  width: 190px;
}

/* ========================================
   History Table
======================================== */

.history-card {
  overflow: hidden;
}

.empty-table {
  padding: 56px 20px !important;

  text-align: center;
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
  .filter-row {
    flex-wrap: wrap;
  }

  .search-wrapper {
    flex-basis: 100%;
  }
}

@media (max-width: 768px) {
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-select,
  .sort-select {
    width: 100%;
  }
}
</style>
