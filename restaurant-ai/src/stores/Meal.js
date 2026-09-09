import { ref } from "vue";
import { defineStore } from "pinia";
import { api } from "../services/api";

export const useMealStore = defineStore("meal", () => {
  // =========================
  // State
  // =========================

  const meals = ref([]);
  const mealBom = ref({});
  const ingredients = ref([]);

  const loading = ref(false);
  const loadingBom = ref(false);

  // =========================
  // 取得餐點列表
  // =========================

  const loadMeals = async () => {
    try {
      loading.value = true;

      const response = await api.get("/Meal");

      meals.value = response.data;
    } catch (error) {
      console.error("Failed to load meals:", error);

      alert("取得餐點失敗，請稍後再試。");
    } finally {
      loading.value = false;
    }
  };

  // =========================
  // 新增餐點
  // =========================

  const createMeal = async (name) => {
    if (!name.trim()) {
      alert("請輸入餐點名稱");
      return;
    }

    try {
      loading.value = true;

      const response = await api.post("/Meal", {
        name: name.trim(),
      });

      alert(`餐點 "${response.data.name}" 新增成功`);

      await loadMeals();
    } catch (error) {
      console.error("Failed to create meal:", error);

      alert(
        `新增餐點失敗:\n ${error.response?.data?.message || error.message}`,
      );
    } finally {
      loading.value = false;
    }
  };

  // =========================
  // 刪除餐點
  // =========================

  const deleteMeal = async (mealId) => {
    if (!confirm("確定要刪除這個餐點嗎？")) {
      return;
    }

    try {
      await api.delete(`/Meal/${mealId}`);

      alert("餐點刪除成功");

      await loadMeals();
    } catch (error) {
      console.error("Failed to delete meal:", error);

      alert(
        `刪除餐點失敗:\n ${error.response?.data?.message || error.message}`,
      );
    }
  };

  // =========================
  // 取得餐點 BOM
  // =========================

  const loadMealBom = async (mealId) => {
    if (mealBom.value[mealId]) {
      return;
    }

    try {
      loadingBom.value = true;

      const response = await api.get(`/Meal/${mealId}`);

      mealBom.value[mealId] = response.data;
    } catch (error) {
      console.error("Failed to load meal BOM:", error);

      alert(
        `取得餐點BOM失敗:\n ${error.response?.data?.message || error.message}`,
      );
    } finally {
      loadingBom.value = false;
    }
  };

  // =========================
  // 取得食材
  // =========================

  const loadIngredients = async () => {
    try {
      const response = await api.get("/Ingredients");

      ingredients.value = response.data;
    } catch (error) {
      console.error("Failed to load ingredients:", error);

      alert(
        `取得食材列表失敗:\n ${error.response?.data?.message || error.message}`,
      );
    }
  };

  // =========================
  // 新增 BOM
  // =========================

  const addBom = async (mealId, ingredientId, quantity) => {
    try {
      await api.post(`/Meal/${mealId}/ingredients`, {
        ingredientId: Number(ingredientId),
        quantity: Number(quantity),
      });

      alert("BOM新增成功");

      // 重新取得 BOM
      const response = await api.get(`/Meal/${mealId}`);

      mealBom.value[mealId] = response.data;
    } catch (error) {
      console.error("Failed to add BOM:", error);

      alert(`新增BOM失敗:\n ${error.response?.data?.message || error.message}`);
    }
  };

  // =========================
  // 修改 BOM
  // =========================

  const updateBom = async (mealId, ingredientId, quantity) => {
    try {
      await api.put(`/Meal/${mealId}/ingredients/${ingredientId}`, {
        quantity: Number(quantity),
      });

      alert("BOM 修改成功");

      const response = await api.get(`/Meal/${mealId}`);

      mealBom.value[mealId] = response.data;
    } catch (error) {
      console.error("Failed to update BOM:", error);

      alert(`更新BOM失敗:\n ${error.response?.data?.message || error.message}`);
    }
  };

  // =========================
  // 刪除 BOM
  // =========================

  const deleteBom = async (mealId, ingredientId) => {
    if (!confirm("確定要刪除這個BOM嗎？")) {
      return;
    }

    try {
      await api.delete(`/Meal/${mealId}/ingredients/${ingredientId}`);

      alert("BOM刪除成功");

      const response = await api.get(`/Meal/${mealId}`);

      mealBom.value[mealId] = response.data;
    } catch (error) {
      console.error("Failed to delete BOM:", error);

      alert(`刪除BOM失敗:\n ${error.response?.data?.message || error.message}`);
    }
  };

  // =========================
  // 出餐
  // =========================

  const serveMeal = async (mealId, quantity) => {
    try {
      const response = await api.post(`/Meal/${mealId}/serve`, {
        quantity: Number(quantity),
      });

      alert(response.data.message || "出餐成功");

      return true;
    } catch (error) {
      console.error("Failed to serve meal:", error);

      alert(`出餐失敗:\n ${error.response?.data?.message || error.message}`);

      return false;
    }
  };

  return {
    // state
    meals,
    mealBom,
    ingredients,
    loading,
    loadingBom,

    // methods
    loadMeals,
    createMeal,
    deleteMeal,
    loadMealBom,
    loadIngredients,
    addBom,
    updateBom,
    deleteBom,
    serveMeal,
  };
});
