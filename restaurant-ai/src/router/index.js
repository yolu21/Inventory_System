import { createRouter, createWebHistory } from "vue-router";
import { jwtDecode } from "jwt-decode";
import Dashboard from "../views/Dashboard.vue";
import Inventory from "../views/Inventory.vue";
import History from "../views/History.vue";
import Meal from "../views/Meal.vue";
import Login from "../views/Login.vue";
import Forecast from "../views/Forecast.vue";
import AIChat from "../views/AIChat.vue";
const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/login",
    name: "Login",
    component: Login,
  },
  {
    path: "/dashboard",
    name: "Dashboard",
    component: Dashboard,
    meta: {
      requireAuth: true, //需要登入才能進入頁面
    },
  },
  {
    path: "/inventory",
    name: "Inventory",
    component: Inventory,
    meta: {
      requireAuth: true,
    },
  },
  {
    path: "/history",
    name: "History",
    component: History,
    meta: {
      requireAuth: true,
      requireAdmin: true,
    },
  },
  {
    path: "/meal",
    name: "Meal",
    component: Meal,
    meta: {
      requireAuth: true,
    },
  },
  {
    //預測庫存
    path: "/forecast",
    name: "Forecast",
    component: Forecast,
    meta: {
      requireAuth: true,
    },
  },
  {
    path: "/ai-chat",
    name: "AIChat",
    component: AIChat,
    meta: { requiresAuth: true },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// 登入驗證：未登入不可進入需要登入的頁面
router.beforeEach((to) => {
  const token = localStorage.getItem("token");
  if (to.meta.requireAuth && !token) {
    return "/login";
  }
  if (to.meta.requireAdmin) {
    //驗證JWT Role
    if (!token) {
      return "/login";
    }

    try {
      const user = jwtDecode(token);

      const role =
        user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

      // 不是 Admin
      if (role !== "Admin") {
        return "/dashboard";
      }
    } catch (error) {
      // Token 無法解析
      localStorage.removeItem("token");
      return "/login";
    }
  }
  return true;
});
export default router;
