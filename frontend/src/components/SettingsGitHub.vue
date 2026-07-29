<template>
  <div class="card theme-card">
    <div class="card-body">
      <div class="mb-3">
        <label class="form-label theme-text">Organization</label>
        <input v-model="gitHub.organization" type="text" class="form-control theme-input" placeholder="your-org">
      </div>
      <div class="mb-3">
        <label class="form-label theme-text">Username</label>
        <input v-model="gitHub.username" type="text" class="form-control theme-input" placeholder="your-username">
      </div>
      <div class="mb-3">
        <label class="form-label theme-text">Access Token</label>
        <input v-model="gitHub.accessToken" type="password" class="form-control theme-input">
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
