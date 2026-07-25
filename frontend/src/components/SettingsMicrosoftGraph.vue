<template>
  <div class="card bg-dark text-light border-secondary">
    <div class="card-body">
      <div class="mb-3">
        <label class="form-label">Tenant ID</label>
        <input v-model="microsoftGraph.tenantId" type="text" class="form-control bg-dark text-light border-secondary">
      </div>
      <div class="mb-3">
        <label class="form-label">Client ID</label>
        <input v-model="microsoftGraph.clientId" type="text" class="form-control bg-dark text-light border-secondary">
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  microsoftGraph: {
    type: Object,
    required: true
  }
});
</script>
