import { defineStore } from "pinia";
import { api } from "../services/api";

export const useInventoryStore = defineStore("inventory", {
  state: () => ({
    inventory: [],
    loading: false,
  }),
  actions: {
    async loadIngredients() {
      try {
        const res = await api.get("/Ingredients");
        this.inventory = res.data;

        for (const item of this.inventory) {
          await this.loadStock(item);
        }
      } catch (error) {
        console.error(error);
      }
    },
    async loadStock(item) {
      try {
        const stockRes = await api.get(`/Stock/stock/${item.id}`);
        item.stock = stockRes.data.stock;
      } catch (error) {
        console.error(error);
      }
    },
    async addIngredient(newIngredient) {
      try {
        await api.post("/Ingredients", newIngredient);
        await this.loadIngredients();
      } catch (error) {
        console.error(error);
      }
    },
    async deleteIngredient(id) {
      try {
        await api.delete(`/Ingredients/${id}`);
        await this.loadIngredients();
      } catch (error) {
        console.error(error);
      }
    },
    async addStock(item) {
      await api.post("/Stock", {
        ingredientId: item.id,
        type: "IN",
        quantity: Number(item.amount) || 0,
      });
      if (item) {
        await this.loadIngredients();
      }
    },
    async removeStock(item) {
      await api.post("/Stock", {
        ingredientId: item.id,
        type: "OUT",
        quantity: Number(item.amount) || 0,
      });
      if (item) {
        await this.loadIngredients();
      }
    },
  },
});
