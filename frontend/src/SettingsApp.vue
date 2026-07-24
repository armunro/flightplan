<template>
  <div class="vh-100 d-flex flex-row overflow-hidden app-root">
    <Navbar />
    <div class="flex-grow-1 overflow-auto bg-darker main-wrapper">
      <Settings />
    </div>
  </div>
</template>

<script setup>
import Navbar from './components/Navbar.vue';
import Settings from './components/Settings.vue';
</script>

<style>
.app-root {
  background-color: var(--bg-darker);
}

.main-wrapper {
  background-color: var(--bg-darker);
}

body {
  background-color: var(--bg-darker) !important;
  color: var(--text-primary) !important;
  font-family: 'Noto Sans', sans-serif !important;
  height: 100vh;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.form-control::placeholder {
  color: var(--text-muted) !important;
  opacity: 0.6 !important;
}

label, .form-label {
  color: var(--text-primary) !important;
  opacity: 0.9 !important;
}

/* Custom Scrollbar */
::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}
::-webkit-scrollbar-track {
  background: var(--bg-darker);
}
::-webkit-scrollbar-thumb {
  background: var(--border-primary);
  border-radius: 5px;
  border: 2px solid var(--bg-darker);
}
::-webkit-scrollbar-thumb:hover {
  background: var(--text-muted);
}
</style>
