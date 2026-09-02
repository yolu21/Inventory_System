<script setup></script>

<template>
  <div class="layout">
    <!--左側選單-->
    <aside class="sidebar">
      <!--HTML5測欄用法-->
      <router-link to="/">首頁</router-link>
      <router-link to="/dashboard">庫存管理</router-link>
      <router-link to="/inventory">新增庫存</router-link>
      <router-link to="/history">歷史記錄</router-link>
      <router-link to="/meal">餐點管理</router-link>
    </aside>

    <!--右側內容-->
    <main class="content">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.layout {
  display: flex;
  width: 100%;
  height: 100vh;
}

/* sidebar */
.sidebar {
  width: 200px;
  height: 100vh;
  flex-shrink: 0; /* 防止縮小 */
  background: #2c3e50;
  color: white;
  padding: 20px;
  display: flex;
  flex-direction: column; /* 讓選單垂直排列 */
  gap: 10px;
}

.sidebar a {
  display: block;
  color: white;
  text-decoration: none;
  padding: 8px;
  border-radius: 5px;
}

.sidebar a:hover {
  background: #34495e;
}

/* content */
.content {
  flex: 1;
  height: 100vh;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 20px;
  background: #f5f5f5;
}
</style>
