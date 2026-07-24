<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content bulk-date-dialog">
      <div class="modal-header">
        <h3>Bulk Update</h3>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>
      <div class="modal-body">
        <p class="mb-3 text-muted">Updating {{ taskIds.length }} selected tasks.</p>
        
        <div class="form-group mb-3">
          <label class="form-check-label d-flex align-items-center mb-1">
            <input type="checkbox" v-model="updateStart" class="form-check-input me-2">
            Start Date & Time
          </label>
          <date-time-selector 
            v-model="start" 
            placeholder="e.g. tomorrow 10am"
            :disabled="!updateStart"
          />
        </div>

        <div class="form-group mb-3">
          <label class="form-check-label d-flex align-items-center mb-1">
            <input type="checkbox" v-model="updateEnd" class="form-check-input me-2">
            End Date & Time
          </label>
          <date-time-selector 
            v-model="end" 
            placeholder="e.g. Next Week"
            :disabled="!updateEnd"
          />
        </div>

        <div class="form-group mb-3">
          <label class="form-check-label d-flex align-items-center mb-1">
            <input type="checkbox" v-model="updateEstimate" class="form-check-input me-2">
            Estimate (minutes)
          </label>
          <input type="number" v-model.number="estimate" class="form-control" placeholder="e.g. 60" min="0" :disabled="!updateEstimate">
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" @click="$emit('close')">Cancel</button>
        <button class="btn-primary" @click="onApply">Apply to {{ taskIds.length }} Tasks</button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue';
import DateTimeSelector from './DateTimeSelector.vue';

export default {
  name: 'BulkDateDialog',
  components: {
    DateTimeSelector
  },
  props: {
    taskIds: { type: Array, required: true }
  },
  emits: ['close', 'apply'],
  setup(props, { emit }) {
    const start = ref(null);
    const end = ref(null);
    const estimate = ref(null);

    const updateStart = ref(false);
    const updateEnd = ref(false);
    const updateEstimate = ref(false);

    const onApply = () => {
      const payload = {};
      if (updateStart.value) payload.start = start.value;
      if (updateEnd.value) payload.end = end.value;
      if (updateEstimate.value) payload.estimateMinutes = estimate.value;

      emit('apply', payload);
    };

    return {
      start,
      end,
      estimate,
      updateStart,
      updateEnd,
      updateEstimate,
      onApply
    };
  }
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
}

.modal-content.bulk-date-dialog {
  background: var(--bg-darker);
  border: 1px solid var(--border-primary);
  border-radius: 8px;
  width: 400px;
  max-width: 90vw;
  box-shadow: 0 10px 25px rgba(0,0,0,0.5);
  display: flex;
  flex-direction: column;
}

.modal-header {
  padding: 16px;
  border-bottom: 1px solid var(--border-primary);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.2rem;
  color: var(--text-primary);
}

.close-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  font-size: 1.5rem;
  cursor: pointer;
}

.modal-body {
  padding: 20px;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  color: var(--text-muted);
}

.form-control {
  background: var(--bg-dark);
  border: 1px solid var(--border-primary);
  color: var(--text-primary);
  border-radius: 4px;
}

.form-control:focus {
  background: var(--bg-dark);
  color: var(--text-primary);
  border-color: var(--accent-blue);
  box-shadow: none;
}

.modal-footer {
  padding: 16px;
  border-top: 1px solid var(--border-primary);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-primary {
  background: var(--accent-blue);
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-secondary {
  background: var(--bg-card);
  color: var(--text-primary);
  border: 1px solid var(--border-primary);
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}
</style>
