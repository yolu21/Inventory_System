import { createRouter, createWebHistory } from "vue-router";

import Dashboard from "../views/Dashboard.vue";
import Inventory from "../views/Inventory.vue";
import History from "../views/History.vue";
import Meal from "../views/Meal.vue";
import Login from "../views/Login.vue";
const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/dashboard",
    name: "Dashboard",
    component: Dashboard,
  },
  {
    path: "/inventory",
    name: "Inventory",
    component: Inventory,
  },
  {
    path: "/history",
    name: "History",
    component: History,
  },
  {
    path: "/meal",
    name: "Meal",
    component: Meal,
  },
  {
    path: "/login",
    name: "Login",
    component: Login,
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
