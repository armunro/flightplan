<template>
  <div class="toast-container p-3 top-0 end-0">
    <TransitionGroup name="toast">
      <div v-for="toast in toasts" :key="toast.id" 
           class="toast show align-items-center border-0 mb-2" 
           :class="getToastClass(toast.type)"
           role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
          <div class="toast-body">
            <i :class="getIconClass(toast.type)" class="me-2"></i>
            {{ toast.message }}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" @click="removeToast(toast.id)"></button>
        </div>
      </div>
    </TransitionGroup>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted } from 'vue';

// Simple event bus for toasts
export const toastBus = ref([]);

export const showToast = (message, type = 'info', duration = 3000) => {
  const id = Date.now();
  const toast = { id, message, type };
  toastBus.value.push(toast);
  
  if (duration > 0) {
    setTimeout(() => {
      removeToast(id);
    }, duration);
  }
};

const removeToast = (id) => {
  toastBus.value = toastBus.value.filter(t => t.id !== id);
};

export default {
  name: 'Toast',
  setup() {
    const toasts = toastBus;

    const getToastClass = (type) => {
      switch (type) {
        case 'success': return 'bg-success text-white';
        case 'error': return 'bg-danger text-white';
        case 'warning': return 'bg-warning text-dark';
        case 'info':
        default: return 'bg-primary text-white';
      }
    };

    const getIconClass = (type) => {
      switch (type) {
        case 'success': return 'bi bi-check-circle-fill';
        case 'error': return 'bi bi-exclamation-octagon-fill';
        case 'warning': return 'bi bi-exclamation-triangle-fill';
        case 'info':
        default: return 'bi bi-info-circle-fill';
      }
    };

    return {
      toasts,
      removeToast,
      getToastClass,
      getIconClass
    };
  }
};
</script>

<style scoped>
.toast-container {
  position: fixed;
  z-index: 999999;
}

.toast {
  min-width: 250px;
  max-width: 350px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  border-radius: 4px;
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(30px);
}
</style>
