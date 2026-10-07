<script setup>
import { ref } from "vue";
import { api } from "../services/api";
import { marked } from "marked";

const inputMessage = ref("");
const messages = ref([]);
const loading = ref(false);

const renderMarkdown = (content) => {
  return marked.parse(content);
};

const sendMessage = async () => {
  const message = String(inputMessage.value || "").trim();

  if (!message || loading.value) {
    return;
  }

  // 先顯示使用者訊息
  messages.value.push({
    role: "user",
    content: message,
  });

  inputMessage.value = "";
  loading.value = true;

  try {
    const response = await api.post("/AI/Chat", {
      message: message,
    });

    messages.value.push({
      role: "assistant",
      content: response.data,
    });
  } catch (error) {
    console.error("AI Chat Error", error);

    messages.value.push({
      role: "assistant",
      content: "AI 回覆失敗，請稍後再試。",
    });
  } finally {
    loading.value = false;
  }
};

const sendQuickQuestion = (question) => {
  inputMessage.value = question;
  sendMessage();
};
</script>

<template>
  <div class="ai-chat-page">
    <!-- Page Header -->
    <div class="page-header">
      <div>
        <h1 class="page-title">AI 庫存助理</h1>
        <p class="page-description">
          詢問庫存、補貨與庫存預測，快速取得系統分析結果。
        </p>
      </div>
    </div>

    <!-- Chat Card -->
    <div class="chat-card">
      <!-- Chat Header -->
      <div class="chat-header">
        <div class="assistant-info">
          <div class="assistant-avatar">✦</div>

          <div>
            <div class="assistant-name">Inventory Assistant</div>
            <div class="assistant-status">
              <span class="status-dot"></span>
              AI 庫存助理
            </div>
          </div>
        </div>
      </div>

      <!-- Messages -->
      <div class="messages">
        <!-- Empty State -->
        <div v-if="messages.length === 0" class="empty-chat">
          <div class="empty-icon">✦</div>

          <h2>今天需要查詢什麼？</h2>

          <p>我可以協助你查詢目前庫存、需要補貨的食材， 以及未來的庫存預測。</p>

          <div class="quick-questions">
            <button
              type="button"
              class="quick-question"
              @click="sendQuickQuestion('哪些食材需要補貨？')"
            >
              <span>📦</span>
              哪些食材需要補貨？
            </button>

            <button
              type="button"
              class="quick-question"
              @click="sendQuickQuestion('目前庫存狀況如何？')"
            >
              <span>📊</span>
              目前庫存狀況如何？
            </button>

            <button
              type="button"
              class="quick-question"
              @click="sendQuickQuestion('未來 7 天需要補多少貨？')"
            >
              <span>📈</span>
              未來 7 天需要補多少貨？
            </button>

            <button
              type="button"
              class="quick-question"
              @click="sendQuickQuestion('目前預估補貨總成本是多少？')"
            >
              <span>💰</span>
              目前預估補貨總成本？
            </button>
          </div>
        </div>

        <!-- Message List -->
        <div
          v-for="(message, index) in messages"
          :key="index"
          class="message-row"
          :class="message.role"
        >
          <!-- Assistant -->
          <template v-if="message.role === 'assistant'">
            <div class="message-avatar">✦</div>

            <div class="message assistant-message markdown">
              <div v-html="renderMarkdown(message.content)"></div>
            </div>
          </template>

          <!-- User -->
          <div v-else class="message user-message">
            {{ message.content }}
          </div>
        </div>

        <!-- Loading -->
        <div v-if="loading" class="message-row assistant">
          <div class="message-avatar">✦</div>

          <div class="message assistant-message loading-message">
            <span class="loading-text">AI 正在分析</span>

            <span class="loading-dots">
              <span></span>
              <span></span>
              <span></span>
            </span>
          </div>
        </div>
      </div>

      <!-- Input -->
      <div class="input-area">
        <input
          v-model="inputMessage"
          type="text"
          class="chat-input"
          placeholder="例如：哪些食材需要補貨？"
          :disabled="loading"
          @keyup.enter="sendMessage"
        />

        <button
          type="button"
          class="btn btn-primary send-button"
          :disabled="loading || !inputMessage.trim()"
          @click="sendMessage"
        >
          {{ loading ? "處理中..." : "送出" }}
        </button>
      </div>

      <div class="chat-hint">AI 會透過系統提供的庫存資料進行回答。</div>
    </div>
  </div>
</template>

<style scoped>
.ai-chat-page {
  width: 100%;
  max-width: 1100px;
  margin: 0 auto;
}

/* =========================
   Chat Card
========================= */

.chat-card {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 190px);
  min-height: 560px;

  overflow: hidden;

  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

/* =========================
   Chat Header
========================= */

.chat-header {
  display: flex;
  align-items: center;

  padding: var(--spacing-4) var(--spacing-5);

  border-bottom: 1px solid var(--color-border);
}

.assistant-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
}

.assistant-avatar,
.message-avatar {
  display: flex;
  align-items: center;
  justify-content: center;

  flex-shrink: 0;

  width: 36px;
  height: 36px;

  color: white;

  background: var(--color-primary);
  border-radius: var(--radius-md);
}

.assistant-name {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text);
}

.assistant-status {
  display: flex;
  align-items: center;
  gap: 6px;

  margin-top: 2px;

  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.status-dot {
  width: 7px;
  height: 7px;

  background: var(--color-success);
  border-radius: 50%;
}

/* =========================
   Messages
========================= */

.messages {
  flex: 1;

  overflow-y: auto;

  padding: var(--spacing-6);
}

.message-row {
  display: flex;
  align-items: flex-start;

  margin-bottom: var(--spacing-5);
}

.message-row.user {
  justify-content: flex-end;
}

.message-row.assistant {
  justify-content: flex-start;
  gap: var(--spacing-3);
}

.message {
  max-width: 72%;

  padding: 12px 16px;

  line-height: 1.6;
  font-size: var(--font-size-sm);
  border-radius: var(--radius-md);
}

.user-message {
  color: white;
  background: var(--color-primary);
  border-bottom-right-radius: 4px;
}

.assistant-message {
  color: var(--color-text);
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-bottom-left-radius: 4px;
}

.message-avatar {
  width: 32px;
  height: 32px;

  font-size: 14px;
  border-radius: 50%;
}

/* =========================
   Markdown
========================= */

.markdown {
  line-height: 1.6;
}

.markdown :deep(p) {
  margin: 0 0 10px;
}

.markdown :deep(p:last-child) {
  margin-bottom: 0;
}

.markdown :deep(ul),
.markdown :deep(ol) {
  margin: 6px 0 10px;
  padding-left: 22px;
}

.markdown :deep(li) {
  margin: 2px 0;
}

.markdown :deep(strong) {
  font-weight: var(--font-weight-semibold);
}

.markdown :deep(code) {
  padding: 2px 5px;

  font-size: 0.9em;

  background: #e5e7eb;
  border-radius: 4px;
}

/* =========================
   Empty State
========================= */

.empty-chat {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  min-height: 100%;

  padding: 40px 20px;

  text-align: center;
}

.empty-icon {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 52px;
  height: 52px;

  margin-bottom: var(--spacing-4);

  color: var(--color-primary);
  background: var(--color-primary-light);
  border-radius: var(--radius-lg);

  font-size: 24px;
}

.empty-chat h2 {
  margin: 0 0 var(--spacing-2);

  font-size: 20px;
  font-weight: var(--font-weight-semibold);
  color: var(--color-text);
}

.empty-chat p {
  max-width: 500px;

  margin: 0 0 var(--spacing-6);

  line-height: 1.6;
  color: var(--color-text-secondary);
}

.quick-questions {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;

  max-width: 760px;

  gap: var(--spacing-3);
}

.quick-question {
  display: flex;
  align-items: center;
  gap: 8px;

  padding: 10px 14px;

  color: var(--color-text-secondary);
  background: var(--color-surface);

  border: 1px solid var(--color-border);
  border-radius: 999px;

  cursor: pointer;

  transition:
    background var(--transition-fast),
    border-color var(--transition-fast),
    color var(--transition-fast);
}

.quick-question:hover {
  color: var(--color-primary);

  background: var(--color-primary-light);
  border-color: #bfdbfe;
}

/* =========================
   Loading
========================= */

.loading-message {
  display: flex;
  align-items: center;
  gap: 8px;

  color: var(--color-text-secondary);
}

.loading-dots {
  display: inline-flex;
  gap: 3px;
}

.loading-dots span {
  width: 4px;
  height: 4px;

  background: var(--color-text-secondary);
  border-radius: 50%;

  animation: loadingDot 1.2s infinite ease-in-out;
}

.loading-dots span:nth-child(2) {
  animation-delay: 0.15s;
}

.loading-dots span:nth-child(3) {
  animation-delay: 0.3s;
}

@keyframes loadingDot {
  0%,
  60%,
  100% {
    opacity: 0.3;
    transform: translateY(0);
  }

  30% {
    opacity: 1;
    transform: translateY(-3px);
  }
}

/* =========================
   Input
========================= */

.input-area {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);

  padding: var(--spacing-4) var(--spacing-5);

  border-top: 1px solid var(--color-border);
}

.chat-input {
  flex: 1;

  min-width: 0;

  padding: 11px 14px;

  color: var(--color-text);
  background: var(--color-surface);

  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);

  font-size: var(--font-size-sm);

  transition:
    border-color var(--transition-fast),
    box-shadow var(--transition-fast);
}

.chat-input:focus {
  outline: none;

  border-color: var(--color-primary);

  box-shadow: 0 0 0 3px var(--color-primary-light);
}

.chat-input:disabled {
  background: var(--color-bg);
  cursor: not-allowed;
}

.send-button {
  flex-shrink: 0;
  min-width: 76px;
}

/* =========================
   Hint
========================= */

.chat-hint {
  padding: 0 var(--spacing-5) var(--spacing-3);

  text-align: center;

  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

/* =========================
   Responsive
========================= */

@media (max-width: 768px) {
  .chat-card {
    height: calc(100vh - 150px);
    min-height: 500px;
  }

  .messages {
    padding: var(--spacing-4);
  }

  .message {
    max-width: 85%;
  }

  .input-area {
    padding: var(--spacing-3);
  }

  .chat-hint {
    display: none;
  }

  .quick-questions {
    flex-direction: column;
    width: 100%;
  }

  .quick-question {
    justify-content: center;
  }
}
</style>
