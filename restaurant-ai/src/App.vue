<script setup>
import { useRouter, useRoute } from "vue-router";
import { jwtDecode } from "jwt-decode";
import { computed } from "vue";

const router = useRouter();
const route = useRoute();

const getRole = () => {
  const token = localStorage.getItem("token");

  if (!token) {
    return null;
  }

  try {
    const user = jwtDecode(token);

    return user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  } catch {
    return null;
  }
};

const isAdmin = computed(() => getRole() === "Admin");
const logout = () => {
  localStorage.removeItem("token");
  router.push("/login");
};
</script>

<template>
  <!-- 登入頁 -->
  <template v-if="route.path === '/login'">
    <router-view />
  </template>

  <!-- 登入後:顯示主要系統畫面 -->
  <div v-else class="layout">
    <!--左側選單-->
    <aside class="sidebar">
      <div class="logo">Inventory System</div>
      <!--HTML5測欄用法-->
      <nav class="menu">
        <router-link to="/dashboard">庫存管理</router-link>
        <router-link v-if="isAdmin" to="/inventory">新增庫存</router-link>
        <router-link v-if="isAdmin" to="/history">歷史記錄</router-link>
        <router-link to="/meal">餐點管理</router-link>
      </nav>
      <button class="logout-btn" @click="logout">登出</button>
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
} /* Sidebar */
.sidebar {
  width: 200px;
  height: 100vh;
  flex-shrink: 0;
  background: #2c3e50;
  color: white;
  padding: 20px;
  display: flex;
  flex-direction: column;
} /* Logo */
.logo {
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 30px;
} /* 選單 */
.menu {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.menu a {
  display: block;
  color: white;
  text-decoration: none;
  padding: 10px;
  border-radius: 5px;
}
.menu a:hover {
  background: #34495e;
} /* 登出按鈕 */
.logout-btn {
  margin-top: auto;
  padding: 10px;
  border: none;
  border-radius: 5px;
  background: #e74c3c;
  color: white;
  cursor: pointer;
}
.logout-btn:hover {
  background: #c0392b;
} /* Content */
.content {
  flex: 1;
  height: 100vh;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 20px;
  background: #f5f5f5;
}
</style>
