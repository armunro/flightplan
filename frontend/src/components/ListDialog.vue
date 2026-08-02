<template>
  <div class="modal-overlay">
    <div class="modal-content list-dialog theme-bg-dark theme-border">
      <div class="modal-header">
        <h3>{{ isNew ? 'Add List' : 'Edit List' }}</h3>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>
      <div class="modal-body">
        <div class="form-group mb-3">
          <label class="form-label theme-text">List Name</label>
          <input v-model="form.name" type="text" class="form-control theme-bg-dark theme-text theme-border" placeholder="Enter list name" ref="nameInput">
        </div>
        
        <div class="form-group mb-3">
          <label class="form-label theme-text">Icon (Bootstrap Icon class)</label>
          <div class="input-group">
            <span class="input-group-text theme-bg-dark theme-text theme-border preview-icon-box">
              <i :class="[displayIconClass]" :style="{ color: form.color }"></i>
            </span>
            <input v-model="form.icon" type="text" class="form-control theme-bg-dark theme-text theme-border" placeholder="e.g. bi-list-task">
          </div>
          <small class="text-muted">Use any <a href="https://icons.getbootstrap.com/" target="_blank" class="text-info">Bootstrap Icon</a> class name.</small>
        </div>

        <div class="form-group mb-3">
          <label class="form-label theme-text">List Color</label>
          <ColorPicker v-model="form.color" show-text />
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-secondary" @click="$emit('close')">Cancel</button>
        <button class="btn btn-primary" @click="onSave" :disabled="!form.name">Save</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import ColorPicker from './ColorPicker.vue';

const props = defineProps({
  list: {
    type: Object,
    default: () => ({ name: '', icon: '', color: '' })
  },
  isNew: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['close', 'save']);

const form = ref({
  name: '',
  icon: '',
  color: ''
});

const nameInput = ref(null);

const displayIconClass = computed(() => {
  const icon = form.value.icon || 'bi-list-task';
  return icon.startsWith('bi-') ? `bi ${icon}` : `bi bi-${icon}`;
});

onMounted(() => {
  if (props.list) {
    form.value = { 
      name: props.list.name || '',
      icon: props.list.icon || '',
      color: props.list.color || ''
    };
  }
  nameInput.value?.focus();
});

const onSave = () => {
  emit('save', { ...form.value });
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10001;
}

.modal-content {
  width: 100%;
  max-width: 450px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
}

.modal-header {
  padding: 15px 20px;
  border-bottom: 1px solid var(--border-primary);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body {
  padding: 20px;
}

.modal-footer {
  padding: 15px 20px;
  border-top: 1px solid var(--border-primary);
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: var(--text-muted);
}

.preview-icon-box {
  width: 40px;
  display: flex;
  justify-content: center;
}
</style>
