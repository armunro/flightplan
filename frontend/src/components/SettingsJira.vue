<template>
  <div class="card theme-card">
    <div class="card-body">
      <div class="row">
        <div class="col-md-12 mb-3">
          <label class="form-label theme-text">Jira URL</label>
          <input v-model="jira.url" type="text" class="form-control theme-input" placeholder="https://your-domain.atlassian.net/">
        </div>
        <div class="col-md-6 mb-3">
          <label class="form-label theme-text">Username (Email)</label>
          <input v-model="jira.username" type="email" class="form-control theme-input" placeholder="user@example.com">
        </div>
        <div class="col-md-6 mb-3">
          <label class="form-label theme-text">API Token</label>
          <input v-model="jira.apiToken" type="password" class="form-control theme-input">
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
