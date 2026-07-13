import { createRouter, createWebHistory } from "vue-router";

import Dashboard from "../views/Dashboard.vue";
import Inventory from "../views/Inventory.vue";
import History from "../views/History.vue";
const routes = [
  {
    path: "/",
    redirect: "/dashboard",
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
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
