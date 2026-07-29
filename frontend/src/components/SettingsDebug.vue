<template>
  <div class="card theme-card">
    <div class="card-body">
      <h5 class="card-title mb-4 theme-text">Diagnostics & Debugging</h5>
      <div class="row">
        <div class="col-md-12">
          <div class="form-check form-switch custom-switch theme-text">
            <input class="form-check-input" type="checkbox" id="demoModeSwitch" v-model="debug.demoMode">
            <label class="form-check-label ms-2" for="demoModeSwitch">
              <strong>Enable Demo Mode</strong>
            </label>
            <div class="theme-text-muted small mt-1">
              When enabled, the application will use mock data instead of connecting to real APIs (Jira, GitHub, etc.). 
              Any changes made in this mode are volatile and will be lost when the application is restarted.
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  debug: {
    type: Object,
    required: true
  }
});
</script>

<style scoped>
.custom-switch .form-check-input {
  cursor: pointer;
  width: 3rem;
  height: 1.5rem;
}

.custom-switch .form-check-label {
  cursor: pointer;
  padding-top: 0.2rem;
}
</style>
