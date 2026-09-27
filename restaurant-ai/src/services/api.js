import axios from "axios";

export const api = axios.create({
  baseURL: "http://localhost:5299",
});

//每次送出 API 前，自動加入 JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
