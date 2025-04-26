<template>
  <div :class="containerClass">
    <!-- Tabs -->
    <ul class="nav nav-tabs" :class="navClass" v-if="isTabTopOrLeft">
      <li class="nav-item fp-tab" v-for="(page, index) in pages" :key="index">
        <button
            class="nav-link"
            :class="{ active: activeTab === index }"
            @click="activeTab = index"
        >
          {{ page.title }}
        </button>
      </li>
    </ul>

    <!-- Content -->
    <div class="tab-content fp-tab-content p-3 border bg-dark text-light" :class="contentClass">
      <component :is="pages[activeTab].component" v-bind="pages[activeTab].props" />
    </div>

    <!-- Tabs (bottom/right) -->
    <ul class="nav nav-tabs" :class="navClass" v-if="!isTabTopOrLeft">
      <li class="nav-item fp-tab" v-for="(page, index) in pages" :key="index">
        <button
            class="nav-link"
            :class="{ active: activeTab === index }"
            @click="activeTab = index"
        >
          {{ page.title }}
        </button>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, computed, defineProps } from 'vue'

const props = defineProps({
  pages: {
    name: Array,
    required: true,
    // Format: [{ title: 'Tab Name', component: SFC, props: PROPS }]
  },
  tabPosition: {
    name: String,
    default: 'top',
    validator: val => ['top', 'bottom', 'left', 'right'].includes(val),
  }
})

const activeTab = ref(0)

const isTabTopOrLeft = computed(() =>
    props.tabPosition === 'top' || props.tabPosition === 'left'
)

const containerClass = computed(() => {
  if (props.tabPosition === 'left') return 'd-flex flex-row'
  if (props.tabPosition === 'right') return 'd-flex flex-row-reverse'
  return ''
})

const navClass = computed(() => {
  if (props.tabPosition === 'left' || props.tabPosition === 'right') {
    return 'flex-column vertical-tabs me-3'
  }
  if (props.tabPosition === 'bottom') {
    return 'mt-3'
  }
  return ''
})

const contentClass = computed(() => {
  if (props.tabPosition === 'left' || props.tabPosition === 'right') {
    return 'flex-fill'
  }
  return ''
})


</script>

<style scoped>
.nav-tabs .nav-link {
  cursor: pointer;
}

.vertical-tabs .nav-link {
  text-align: left;
  white-space: nowrap;
  border-radius: 0;
}

.vertical-tabs .nav-link.active {
  background-color: #343a40;
  color: #fff;
}
/* Tabs and Pages*/
.nav-item.fp-tab {}
.nav-item.fp-tab button{border-radius: 0 !important;
  padding: 1rem 2rem;
}
.tab-content.fp-tab-content {border-radius: 0 !important; padding:0 !important;}

</style>
