<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { api } from "../services/api";

const router = useRouter();

const username = ref("");
const password = ref("");
const errorMessage = ref("");

const login = async () => {
  errorMessage.value = "";

  try {
    const response = await api.post("/Auth/login", {
      username: username.value,
      password: password.value,
    });

    localStorage.setItem("token", response.data.token);

    window.location.href = "/dashboard";
  } catch (error) {
    errorMessage.value = "帳號或密碼錯誤";
  }
};
</script>

<template>
  <form @submit.prevent="login">
    <div class="login-page">
      <div class="login-card">
        <h1>Inventory System</h1>
        <p class="subtitle">庫存管理系統</p>

        <form @submit.prevent="login">
          <div class="form-group">
            <label>帳號</label>
            <input v-model="username" type="text" placeholder="請輸入帳號" />
          </div>

          <div class="form-group">
            <label>密碼</label>
            <input
              v-model="password"
              type="password"
              placeholder="請輸入密碼"
            />
          </div>

          <p v-if="errorMessage" class="error">
            {{ errorMessage }}
          </p>

          <button type="submit" class="login-btn">登入</button>
        </form>
      </div>
    </div>
  </form>
</template>

<style scoped>
.login-page {
  width: 100%;
  height: 100vh;

  display: flex;
  justify-content: center;
  align-items: center;

  background: #f5f7fa;
}

.login-card {
  width: 360px;

  padding: 40px;

  background: white;

  border-radius: 10px;

  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.login-card h1 {
  margin: 0;

  text-align: center;

  color: #2c3e50;
}

.subtitle {
  margin-top: 8px;
  margin-bottom: 30px;

  text-align: center;

  color: #7f8c8d;
}

.form-group {
  display: flex;
  flex-direction: column;

  margin-bottom: 20px;
}

.form-group label {
  margin-bottom: 6px;

  font-weight: bold;
  color: #34495e;
}

.form-group input {
  padding: 10px 12px;

  border: 1px solid #ddd;
  border-radius: 5px;

  font-size: 14px;

  box-sizing: border-box;
}

.form-group input:focus {
  outline: none;
  border-color: #3498db;
}

.login-btn {
  width: 100%;

  padding: 11px;

  border: none;
  border-radius: 5px;

  background: #3498db;
  color: white;

  font-size: 16px;

  cursor: pointer;
}

.login-btn:hover {
  background: #2980b9;
}

.error {
  margin-top: 0;
  margin-bottom: 15px;

  color: #e74c3c;

  font-size: 14px;
}
</style>
