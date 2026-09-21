<script setup>
import { ref } from "vue";
import { api } from "../services/api";

const username = ref("");
const password = ref("");
const errorMessage = ref("");

const login = async () => {
  try {
    const response = await api.post("/Auth/login", {
      username: username.value,
      password: password.value,
    });

    localStorage.setItem("token", response.data.token); //把JWT token存到瀏覽器
    console.log("登入成功");
    console.log(response.data.token);
  } catch (error) {
    errorMessage.value = "帳號或密碼錯誤";
  }
};
</script>
<template>
  <div>
    <h1>登入</h1>
    <input v-model="username" type="text" placeholder="帳號" />

    <input v-model="password" type="password" placeholder="密碼" />

    <button @click="login">登入</button>
    <p v-if="errorMessage">
      {{ errorMessage }}
    </p>
  </div>
</template>
<style scoped></style>
