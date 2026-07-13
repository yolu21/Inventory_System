import { defineStore } from "pinia";
import { api } from "../services/api";

export const useHistoryStore = defineStore("history", {
  state: () => ({
    history: [],
    loading: false,
  }),
  actions: {
    async loadHistory() {
      try {
        this.loading = true;
        const res = await api.get("/Stock");
        this.history = res.data;
      } catch (error) {
        console.error(error);
      } finally {
        this.loading = false;
      }
    },
  },
});
