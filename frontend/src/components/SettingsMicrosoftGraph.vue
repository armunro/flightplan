<template>
  <div class="card theme-card">
    <div class="card-body">
      <div class="mb-3">
        <label class="form-label theme-text">Tenant ID</label>
        <input v-model="microsoftGraph.tenantId" type="text" class="form-control theme-input">
      </div>
      <div class="mb-3">
        <label class="form-label theme-text">Client ID</label>
        <input v-model="microsoftGraph.clientId" type="text" class="form-control theme-input">
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
