<script setup>
import { computed } from "vue";
import { useRouter, useRoute } from "vue-router";
import { jwtDecode } from "jwt-decode";

const router = useRouter();
const route = useRoute();

const getRole = () => {
  const token = localStorage.getItem("token");

  if (!token) return null;

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
  <!-- Login page -->
  <template v-if="route.path === '/login'">
    <router-view />
  </template>

  <!-- Main application -->
  <div v-else class="app-layout">
    <!-- Sidebar -->
    <aside class="sidebar">
      <!-- Brand -->
      <div class="sidebar-brand">
        <div class="brand-icon">🍴</div>

        <div class="brand-text">
          <div class="brand-title">Inventory</div>

          <div class="brand-subtitle">Management</div>
        </div>
      </div>

      <!-- Navigation -->
      <nav class="sidebar-nav">
        <!-- Overview -->
        <div class="nav-section">
          <div class="nav-section-title">OVERVIEW</div>

          <router-link
            to="/dashboard"
            class="nav-item"
            active-class="nav-item-active"
          >
            <span class="nav-icon">▣</span>
            <span>目前庫存</span>
          </router-link>
        </div>

        <!-- Management -->
        <div class="nav-section">
          <div class="nav-section-title">MANAGEMENT</div>

          <router-link
            v-if="isAdmin"
            to="/inventory"
            class="nav-item"
            active-class="nav-item-active"
          >
            <span class="nav-icon">▤</span>
            <span>庫存管理</span>
          </router-link>

          <router-link
            to="/meal"
            class="nav-item"
            active-class="nav-item-active"
          >
            <span class="nav-icon">◈</span>
            <span>餐點管理</span>
          </router-link>

          <router-link
            v-if="isAdmin"
            to="/history"
            class="nav-item"
            active-class="nav-item-active"
          >
            <span class="nav-icon">◷</span>
            <span>歷史記錄</span>
          </router-link>
        </div>

        <!-- Analytics -->
        <div class="nav-section">
          <div class="nav-section-title">ANALYTICS</div>

          <router-link
            to="/forecast"
            class="nav-item"
            active-class="nav-item-active"
          >
            <span class="nav-icon">↗</span>
            <span>庫存預測</span>
          </router-link>
        </div>

        <!-- AI -->
        <div class="nav-section">
          <div class="nav-section-title">AI</div>

          <router-link
            to="/ai-chat"
            class="nav-item nav-item-ai"
            active-class="nav-item-active"
          >
            <span class="nav-icon">✦</span>
            <span>AI 庫存助理</span>
          </router-link>
        </div>
      </nav>

      <!-- Sidebar Footer -->
      <div class="sidebar-footer">
        <div class="user-info">
          <div class="user-avatar">
            {{ isAdmin ? "A" : "U" }}
          </div>

          <div class="user-details">
            <div class="user-name">
              {{ isAdmin ? "Administrator" : "User" }}
            </div>

            <div class="user-role">
              {{ isAdmin ? "Admin" : "User" }}
            </div>
          </div>
        </div>

        <button class="logout-button" type="button" @click="logout">
          <span>↪</span>
          <span>登出</span>
        </button>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="app-content">
      <div class="page-container">
        <router-view />
      </div>
    </main>
  </div>
</template>

<style scoped>
/* ========================================
   Sidebar
======================================== */

.sidebar {
  position: fixed;
  top: 0;
  left: 0;

  display: flex;
  flex-direction: column;

  width: var(--sidebar-width);
  height: 100vh;

  padding: var(--spacing-5) var(--spacing-3);

  color: var(--color-sidebar-text);
  background: var(--color-sidebar);

  z-index: 100;
}

/* ========================================
   Brand
======================================== */

.sidebar-brand {
  display: flex;
  align-items: center;

  gap: var(--spacing-3);

  padding: var(--spacing-2) var(--spacing-3);
  margin-bottom: var(--spacing-6);
}

.brand-icon {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 38px;
  height: 38px;

  border-radius: var(--radius-md);

  background: var(--color-primary);
  color: white;

  font-size: 18px;
}

.brand-title {
  color: var(--color-text-inverse);

  font-size: var(--font-size-md);
  font-weight: var(--font-weight-bold);
  line-height: 1.2;
}

.brand-subtitle {
  margin-top: 2px;

  color: #94a3b8;

  font-size: var(--font-size-xs);
}

/* ========================================
   Navigation
======================================== */

.sidebar-nav {
  flex: 1;

  overflow-y: auto;

  padding-right: 2px;
}

.nav-section {
  margin-bottom: var(--spacing-5);
}

.nav-section-title {
  padding: 0 var(--spacing-3);
  margin-bottom: var(--spacing-2);

  color: #64748b;

  font-size: 11px;
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.08em;
}

.nav-item {
  display: flex;
  align-items: center;

  gap: var(--spacing-3);

  min-height: 42px;

  padding: 0 var(--spacing-3);

  margin-bottom: 2px;

  color: var(--color-sidebar-text);

  border-radius: var(--radius-md);

  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);

  transition:
    color var(--transition-fast),
    background var(--transition-fast);
}

.nav-item:hover {
  color: var(--color-sidebar-text-active);
  background: var(--color-sidebar-hover);
}

.nav-item-active {
  color: var(--color-sidebar-text-active);
  background: var(--color-sidebar-active);
}

.nav-icon {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 20px;

  color: #94a3b8;

  font-size: 15px;
}

.nav-item-active .nav-icon {
  color: #bfdbfe;
}

.nav-item-ai .nav-icon {
  color: #a5b4fc;
}

/* ========================================
   Sidebar Footer
======================================== */

.sidebar-footer {
  padding-top: var(--spacing-4);

  border-top: 1px solid #1e293b;
}

.user-info {
  display: flex;
  align-items: center;

  gap: var(--spacing-3);

  padding: var(--spacing-2) var(--spacing-3);
  margin-bottom: var(--spacing-2);
}

.user-avatar {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 34px;
  height: 34px;

  flex-shrink: 0;

  border-radius: var(--radius-full);

  color: white;
  background: #334155;

  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
}

.user-details {
  min-width: 0;
}

.user-name {
  overflow: hidden;

  color: #e2e8f0;

  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);

  white-space: nowrap;
  text-overflow: ellipsis;
}

.user-role {
  margin-top: 2px;

  color: #64748b;

  font-size: var(--font-size-xs);
}

.logout-button {
  display: flex;
  align-items: center;

  width: 100%;
  min-height: 40px;

  gap: var(--spacing-3);

  padding: 0 var(--spacing-3);

  color: #94a3b8;
  background: transparent;

  border: none;
  border-radius: var(--radius-md);

  font-size: var(--font-size-sm);

  transition:
    color var(--transition-fast),
    background var(--transition-fast);
}

.logout-button:hover {
  color: #fca5a5;
  background: rgba(220, 38, 38, 0.1);
}

/* ========================================
   Responsive
======================================== */

@media (max-width: 768px) {
  .sidebar {
    position: static;

    width: 100%;
    height: auto;

    padding: var(--spacing-3);
  }

  .sidebar-nav {
    max-height: none;
  }

  .sidebar-footer {
    margin-top: var(--spacing-4);
  }
}
</style>
