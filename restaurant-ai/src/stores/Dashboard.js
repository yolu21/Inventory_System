import { defineStore } from "pinia";
import { api } from "../services/api";

export const useDashboardStore = defineStore("dashboard", {
  state: () => ({
    summary: {},
    overview: null,

    // 目前選擇的年月
    selectedYear: new Date().getFullYear(),
    selectedMonth: new Date().getMonth() + 1,

    loading: false,
  }),

  actions: {
    async loadDashboard(year = this.selectedYear, month = this.selectedMonth) {
      this.loading = true;

      try {
        const response = await api.get("/Dashboard/overview", {
          params: {
            year,
            month,
          },
        });

        this.overview = response.data;
        this.summary = response.data.summary;

        // 確認 Store 和後端目前選擇的年月一致
        this.selectedYear = response.data.selectedYear;
        this.selectedMonth = response.data.selectedMonth;
      } catch (error) {
        console.error("取得 Dashboard 資料失敗:", error);
      } finally {
        this.loading = false;
      }
    },

    // 切換月份
    async changeMonth(year, month) {
      this.selectedYear = year;
      this.selectedMonth = month;

      await this.loadDashboard(year, month);
    },

    // 上一個月
    async previousMonth() {
      let year = this.selectedYear;
      let month = this.selectedMonth - 1;

      if (month === 0) {
        month = 12;
        year--;
      }

      await this.changeMonth(year, month);
    },

    // 下一個月
    async nextMonth() {
      let year = this.selectedYear;
      let month = this.selectedMonth + 1;

      if (month === 13) {
        month = 1;
        year++;
      }

      await this.changeMonth(year, month);
    },
  },
});
