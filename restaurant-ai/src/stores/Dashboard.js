import { defineStore } from "pinia";
import { api } from "../services/api";

export const useDashboardStore = defineStore("dashboard", {
  state: () => ({
    summary: {},
    overview: null,
    loading: false,
  }),
  actions: {
    async loadDashboard() {
      try {
        const SummaryRes = await api.get("/Dashboard/summary");
        this.summary = SummaryRes.data;

        const OverviewRes = await api.get("/Dashboard/overview");
        this.overview = OverviewRes.data;
      } catch (error) {
        console.error(error);
      } finally {
        this.loading = false;
      }
    },
  },
});
