<template>
  <div v-if="isOpen" class="modal-overlay" @click="close">
    <div class="help-modal theme-card border-primary" @click.stop>
      <div class="modal-header theme-border">
        <h5 class="theme-text mb-0">Keyboard Shortcuts</h5>
        <button class="close-btn theme-text-muted" @click="close">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>
      <div class="modal-body p-4">
        <div class="row g-4">
          <div class="col-md-6">
            <h6 class="theme-text-muted mb-3 text-uppercase small fw-bold">Navigation</h6>
            <div class="shortcut-list">
              <div class="shortcut-item">
                <span class="key">Alt + D</span>
                <span class="action theme-text">Dashboard</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + T</span>
                <span class="action theme-text">Tasks</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + J</span>
                <span class="action theme-text">Jira</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + G</span>
                <span class="action theme-text">GitHub</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + S</span>
                <span class="action theme-text">Schedules</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + E</span>
                <span class="action theme-text">Email</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + C</span>
                <span class="action theme-text">Calendar</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + L</span>
                <span class="action theme-text">Links</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + N</span>
                <span class="action theme-text">Notepad</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + B</span>
                <span class="action theme-text">Debug</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + ,</span>
                <span class="action theme-text">Settings</span>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <h6 class="theme-text-muted mb-3 text-uppercase small fw-bold">General</h6>
            <div class="shortcut-list">
              <div class="shortcut-item">
                <span class="key">Alt + /</span>
                <span class="action theme-text">Open Help</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Alt + A</span>
                <span class="action theme-text">Quick Add Task</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Esc</span>
                <span class="action theme-text">Close Modal</span>
              </div>
            </div>
            
            <h6 class="theme-text-muted mt-4 mb-3 text-uppercase small fw-bold">Task Editing</h6>
            <div class="shortcut-list">
              <div class="shortcut-item">
                <span class="key">Enter</span>
                <span class="action theme-text">Save & New Sibling</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Tab</span>
                <span class="action theme-text">Indent (Subtask)</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Shift + Tab</span>
                <span class="action theme-text">Outdent (Promote)</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Esc</span>
                <span class="action theme-text">Cancel Edit</span>
              </div>
              <div class="shortcut-item">
                <span class="key">Backspace*</span>
                <span class="action theme-text">Delete Empty Task</span>
              </div>
            </div>

            <h6 class="theme-text-muted mt-4 mb-3 text-uppercase small fw-bold">Task Drag & Drop</h6>
            <div class="shortcut-list">
              <div class="shortcut-item">
                <span class="key">Ctrl + Drag</span>
                <span class="action theme-text">Copy Task</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits, onMounted, onBeforeUnmount } from 'vue';

const props = defineProps({
  isOpen: Boolean
});

const emit = defineEmits(['close']);

const close = () => {
  emit('close');
};

const handleKeyDown = (e) => {
  if (e.key === 'Escape' && props.isOpen) {
    close();
  }
};

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
});

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeyDown);
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
  backdrop-filter: blur(2px);
}

.help-modal {
  width: 95%;
  max-width: 800px;
  max-height: 90vh;
  background-color: var(--bg-dark);
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
  overflow: hidden;
  border: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
}

.modal-body {
  overflow-y: auto;
}

.modal-header {
  padding: 1rem 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--border-primary);
  background-color: rgba(255, 255, 255, 0.02);
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  transition: color 0.2s;
}

.close-btn:hover {
  color: var(--text-primary) !important;
}

.shortcut-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.shortcut-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 4px 0;
}

.key {
  background-color: rgba(255, 255, 255, 0.1);
  color: var(--accent-blue);
  padding: 2px 8px;
  border-radius: 4px;
  font-family: monospace;
  font-size: 0.9rem;
  min-width: 80px;
  text-align: center;
  border: 1px solid rgba(88, 166, 255, 0.2);
}

.action {
  font-size: 0.9rem;
}

.theme-card {
  background-color: var(--bg-dark);
}

.theme-text {
  color: var(--text-primary);
}

.theme-text-muted {
  color: var(--text-muted);
}

.theme-border {
  border-color: var(--border-primary);
}
</style>
