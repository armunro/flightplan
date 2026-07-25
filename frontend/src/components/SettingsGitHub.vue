<template>
  <div class="card bg-dark text-light border-secondary">
    <div class="card-header border-secondary">
      <h5 class="mb-0"><i class="bi bi-github me-2"></i>GitHub Configuration</h5>
    </div>
    <div class="card-body">
      <div class="mb-3">
        <label class="form-label">Organization</label>
        <input v-model="gitHub.organization" type="text" class="form-control bg-dark text-light border-secondary" placeholder="your-org">
      </div>
      <div class="mb-3">
        <label class="form-label">Username</label>
        <input v-model="gitHub.username" type="text" class="form-control bg-dark text-light border-secondary" placeholder="your-username">
      </div>
      <div class="mb-3">
        <label class="form-label">Access Token</label>
        <input v-model="gitHub.accessToken" type="password" class="form-control bg-dark text-light border-secondary">
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  gitHub: {
    type: Object,
    required: true
  }
});
</script>
