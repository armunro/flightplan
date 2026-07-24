<template>
  <div class="cron-explainer p-2 mt-1 rounded bg-darker border border-secondary">
    <div v-if="description" class="d-flex align-items-start gap-2">
      <i class="bi bi-info-circle text-info mt-1"></i>
      <div>
        <div class="text-light small fw-bold">Schedule Description:</div>
        <div class="text-info small">{{ description }}</div>
      </div>
    </div>
    <div v-if="error" class="d-flex align-items-start gap-2 mt-1">
      <i class="bi bi-exclamation-triangle mt-1" :class="isCriticalError ? 'text-danger' : 'text-warning'"></i>
      <div>
        <div class="small fw-bold" :class="isCriticalError ? 'text-danger' : 'text-warning'">
          {{ isCriticalError ? 'Invalid Expression:' : 'Note:' }}
        </div>
        <div class="small" :class="isCriticalError ? 'text-danger' : 'text-warning'">{{ error }}</div>
      </div>
    </div>
    
    <div class="mt-2 pt-2 border-top border-secondary">
      <div class="text-light x-small">
        Quartz Format: <code class="text-info">sec min hour dom month dow [year]</code>
      </div>
      <div class="text-light x-small mt-1">
        Example: <code class="text-accent cursor-pointer" @click="emit('update:modelValue', '0 0 9 ? * MON-FRI')">0 0 9 ? * MON-FRI</code> <span class="text-muted">(9:00 AM Mon-Fri)</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';
import cronstrue from 'cronstrue';

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
});

const emit = defineEmits(['update:modelValue']);

const description = ref('');
const error = ref('');
const isCriticalError = ref(false);

const updateDescription = (cron) => {
  if (!cron || cron.trim() === '') {
    description.value = '';
    error.value = '';
    isCriticalError.value = false;
    return;
  }

  try {
    // Basic Quartz validation/normalization check
    const parts = cron.trim().split(/\s+/);
    if (parts.length < 6) {
      throw new Error('Quartz cron expressions must have at least 6 parts (seconds, minutes, hours, day-of-month, month, day-of-week)');
    }

    // Normalize: If parts[3] is * and parts[5] is *, Quartz will fail. 
    // Usually one should be ? if the other is *.
    let warning = '';
    if (parts[3] === '*' && parts[5] === '*') {
      warning = 'Quartz requires either day-of-month or day-of-week to be "?" instead of "*"';
    }

    description.value = cronstrue.toString(cron, { 
      use24HourTimeFormat: true,
      throwExceptionOnParseError: true
    });
    
    if (warning) {
      error.value = warning;
      isCriticalError.value = false;
    } else {
      error.value = '';
      isCriticalError.value = false;
    }
  } catch (e) {
    description.value = '';
    error.value = e.message || 'Invalid cron expression';
    isCriticalError.value = true;
  }
};

watch(() => props.modelValue, (newVal) => {
  updateDescription(newVal);
});

onMounted(() => {
  updateDescription(props.modelValue);
});
</script>

<style scoped>
.bg-darker {
  background-color: rgba(0, 0, 0, 0.3);
}
.x-small {
  font-size: 0.75rem;
}
.cursor-pointer {
  cursor: pointer;
}
</style>
