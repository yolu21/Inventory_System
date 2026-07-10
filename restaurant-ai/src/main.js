import { createApp } from "vue";
//import "./style.css";
import App from "./App.vue";
import router from "./router";
import "./styles/global.css";

// createApp(App).use(router).mount("#app");
//20260710 改為用pinia管理狀態
import { createPinia } from "pinia";
const app = createApp(App);
app.use(createPinia());
app.use(router);
app.mount("#app");
