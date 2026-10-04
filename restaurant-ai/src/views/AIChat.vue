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

  //先顯示使用者訊息
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
    console.log("AI response:", response.data);
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
  <div class="ai-chat">
    <div class="page-header">
      <h1>AI 庫存助理</h1>
      <p>可以詢問庫存、補貨與庫存預測相關問題</p>
    </div>
    <div class="chat-container">
      <div v-if="messages.length === 0" class="empty-state">
        <h2>👋 你好，我是庫存管理助理</h2>

        <p>我可以協助你查詢庫存、補貨與庫存預測。</p>

        <div class="quick-questions">
          <button @click="sendQuickQuestion('哪些食材需要補貨？')">
            哪些食材需要補貨？
          </button>

          <button @click="sendQuickQuestion('目前庫存狀況如何？')">
            目前庫存狀況如何？
          </button>

          <button @click="sendQuickQuestion('未來 7 天需要補多少貨？')">
            未來 7 天需要補多少貨？
          </button>

          <button @click="sendQuickQuestion('目前預估補貨總成本是多少？')">
            目前預估補貨總成本？
          </button>
        </div>
      </div>
      <div class="messages">
        <div
          v-for="(message, index) in messages"
          :key="index"
          class="message-row"
          :class="message.role"
        >
          <div
            v-if="message.role === 'assistant'"
            class="message markdown"
            v-html="renderMarkdown(message.content)"
          ></div>

          <div v-else class="message">
            {{ message.content }}
          </div>
        </div>

        <div v-if="loading" class="message-row assistant">
          <div class="message loading">
            AI 思考中<span class="dots">...</span>
          </div>
        </div>
      </div>
      <div class="input-area">
        <input
          v-model="inputMessage"
          type="text"
          placeholder="例如:那些食材需要補貨?"
          :disabled="loading"
          @keyup.enter="sendMessage"
        />

        <button
          @click="sendMessage"
          :disabled="loading || !inputMessage.trim()"
        >
          {{ loading ? "處理中" : "送出" }}
        </button>
      </div>
    </div>
  </div>
</template>
<style scoped>
.ai-chat {
  max-width: 1000px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h1 {
  margin-bottom: 8px;
}

.page-header p {
  color: #666;
  margin: 0;
}

.chat-container {
  height: calc(100vh - 180px);
  min-height: 500px;
  background: white;
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid #ddd;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.message-row {
  display: flex;
  margin-bottom: 15px;
}

.message-row.user {
  justify-content: flex-end;
}

.message-row.assistant {
  justify-content: flex-start;
}

.message {
  max-width: 70%;
  padding: 12px 16px;
  border-radius: 10px;
  line-height: 1.5;
}
.message-row.user .message {
  background: #3498db;
  color: white;
}

.message-row.assistant .message {
  background: #f1f1f1;
  color: #333;
}

.loading {
  color: #777;
}

.input-area {
  display: flex;
  gap: 10px;
  padding: 15px;
  border-top: 1px solid #ddd;
}

.input-area input {
  flex: 1;
  padding: 12px;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 16px;
}

.input-area button {
  padding: 0 20px;
  border: none;
  border-radius: 6px;
  background: #3498db;
  color: white;
  cursor: pointer;
}

.input-area button:disabled {
  background: #aaa;
  cursor: not-allowed;
}
.markdown {
  line-height: 1.5;
}

.markdown :deep(p) {
  margin: 0 0 8px;
  line-height: 1.5;
}

.markdown :deep(p:last-child) {
  margin-bottom: 0;
}

.markdown :deep(ul),
.markdown :deep(ol) {
  margin: 4px 0 8px;
  padding-left: 24px;
}

.markdown :deep(li) {
  margin: 0;
  padding: 0;
  line-height: 1.5;
}

.markdown :deep(li p) {
  margin: 0;
}

.markdown :deep(strong) {
  font-weight: 700;
}
.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #555;
}

.empty-state h2 {
  margin-bottom: 10px;
  color: #333;
}

.empty-state p {
  margin-bottom: 25px;
}

.quick-questions {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 10px;
}

.quick-questions button {
  padding: 10px 15px;
  border: 1px solid #ddd;
  border-radius: 20px;
  background: white;
  color: #555;
  cursor: pointer;
}

.quick-questions button:hover {
  background: #f5f5f5;
  border-color: #bbb;
}
.dots {
  display: inline-block;
  width: 20px;
  overflow: hidden;
  vertical-align: bottom;
  animation: loadingDots 1.2s steps(4, end) infinite;
}

@keyframes loadingDots {
  0% {
    width: 0;
  }

  33% {
    width: 7px;
  }

  66% {
    width: 14px;
  }

  100% {
    width: 20px;
  }
}
</style>
