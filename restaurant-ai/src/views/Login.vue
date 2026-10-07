<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { api } from "../services/api";

const router = useRouter();

const username = ref("");
const password = ref("");
const errorMessage = ref("");
const isLoading = ref(false);

const login = async () => {
  errorMessage.value = "";

  if (!username.value || !password.value) {
    errorMessage.value = "請輸入帳號與密碼";
    return;
  }

  try {
    isLoading.value = true;

    const response = await api.post("/Auth/login", {
      username: username.value,
      password: password.value,
    });

    localStorage.setItem("token", response.data.token);

    router.push("/dashboard");
  } catch (error) {
    console.error("登入失敗:", error);

    errorMessage.value = "帳號或密碼錯誤";
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <!-- Brand -->
      <div class="login-brand">
        <div class="brand-icon">IS</div>

        <h1>Inventory Management</h1>

        <p>餐廳庫存管理系統</p>
      </div>

      <!-- Login Form -->
      <form class="login-form" @submit.prevent="login">
        <!-- Username -->
        <div class="form-group">
          <label class="form-label" for="username"> 帳號 </label>

          <input
            id="username"
            v-model="username"
            class="form-input"
            type="text"
            placeholder="請輸入帳號"
            autocomplete="username"
          />
        </div>

        <!-- Password -->
        <div class="form-group">
          <label class="form-label" for="password"> 密碼 </label>

          <input
            id="password"
            v-model="password"
            class="form-input"
            type="password"
            placeholder="請輸入密碼"
            autocomplete="current-password"
          />
        </div>

        <!-- Error -->
        <div v-if="errorMessage" class="login-error">
          {{ errorMessage }}
        </div>

        <!-- Submit -->
        <button
          type="submit"
          class="btn btn-primary login-btn"
          :disabled="isLoading"
        >
          {{ isLoading ? "登入中..." : "登入" }}
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
/* ========================================
   Login Page
======================================== */

.login-page {
  min-height: 100vh;

  display: flex;
  align-items: center;
  justify-content: center;

  padding: var(--spacing-6);

  background: var(--color-bg);
}

/* ========================================
   Login Card
======================================== */

.login-card {
  width: 100%;
  max-width: 400px;

  padding: 40px;

  background: var(--color-surface);

  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);

  box-shadow: var(--shadow-lg);
}

/* ========================================
   Brand
======================================== */

.login-brand {
  margin-bottom: var(--spacing-8);

  text-align: center;
}

.brand-icon {
  width: 48px;
  height: 48px;

  display: flex;
  align-items: center;
  justify-content: center;

  margin: 0 auto var(--spacing-4);

  color: #ffffff;
  background: var(--color-primary);

  border-radius: var(--radius-md);

  font-size: 16px;
  font-weight: var(--font-weight-bold);
}

.login-brand h1 {
  margin: 0;

  color: var(--color-text-primary);

  font-size: 24px;
  font-weight: var(--font-weight-bold);
}

.login-brand p {
  margin: var(--spacing-2) 0 0;

  color: var(--color-text-secondary);

  font-size: 14px;
}

/* ========================================
   Form
======================================== */

.login-form {
  width: 100%;
}

.login-form .form-group {
  margin-bottom: var(--spacing-5);
}

/* ========================================
   Error
======================================== */

.login-error {
  margin-bottom: var(--spacing-4);

  padding: var(--spacing-3) var(--spacing-4);

  color: var(--color-danger);

  background: var(--color-danger-light);

  border: 1px solid #fecaca;
  border-radius: var(--radius-md);

  font-size: 14px;
}

/* ========================================
   Login Button
======================================== */

.login-btn {
  width: 100%;

  margin-top: var(--spacing-2);
}

.login-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* ========================================
   Responsive
======================================== */

@media (max-width: 480px) {
  .login-page {
    padding: var(--spacing-4);
  }

  .login-card {
    padding: 28px 24px;
  }

  .login-brand h1 {
    font-size: 21px;
  }
}
</style>
