<template>
    <ul class="nav nav-tabs">
      <li class="nav-item" v-for="(page, index) in pages" :key="index">
        <button
            class="nav-link"
            :class="{ active: activeTab === index }"
            @click="activeTab = index"
        >
          {{ page.title }}
        </button>
      </li>
    </ul>

    <div class="tab-content p-3 border rounded bg-dark text-light">
      <component :is="pages[activeTab].component" />
    </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  pages: {
    type: Array,
    required: true,
    // Expected format: [{ title: 'Tab Name', component: SFC }]
  }
})

const activeTab = ref(0)
</script>

<style scoped>
.nav-tabs .nav-link {
  cursor: pointer;
}
</style>
