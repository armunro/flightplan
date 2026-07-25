<template>
  <div class="card bg-dark text-light border-secondary">
    <div class="card-header border-secondary d-flex justify-content-between align-items-center">
      <h5 class="mb-0"><i class="bi bi-jira me-2"></i>Jira Configuration</h5>
    </div>
    <div class="card-body">
      <div class="row">
        <div class="col-md-12 mb-3">
          <label class="form-label">Jira URL</label>
          <input v-model="jira.url" type="text" class="form-control bg-dark text-light border-secondary" placeholder="https://your-domain.atlassian.net/">
        </div>
        <div class="col-md-6 mb-3">
          <label class="form-label">Username (Email)</label>
          <input v-model="jira.username" type="email" class="form-control bg-dark text-light border-secondary" placeholder="user@example.com">
        </div>
        <div class="col-md-6 mb-3">
          <label class="form-label">API Token</label>
          <input v-model="jira.apiToken" type="password" class="form-control bg-dark text-light border-secondary">
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  jira: {
    type: Object,
    required: true
  }
});
</script>
