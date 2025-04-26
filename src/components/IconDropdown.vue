<template>
    <div class="dropdown">
      <button
          class="btn btn-outline-secondary dropdown-toggle w-100 text-start"
          type="button"
          data-bs-toggle="dropdown"
          aria-expanded="false"
      >
        <i v-if="modelValue" :class="modelValue + ' me-2'"></i>
        {{ displayName || 'Choose an icon' }}
      </button>
      <ul class="dropdown-menu w-100" style="max-height: 300px; overflow-y: auto;">
        <li class="px-3 py-2">
          <input
              v-model="search"
              type="text"
              class="form-control"
              placeholder="Search icons..."
              @click.stop
          />
        </li>
        <li v-for="icon in filteredIcons" :key="icon">
          <button
              class="dropdown-item d-flex align-items-center gap-2"
              @click="selectIcon(icon)"
          >
            <i :class="`fa fa-${icon}`"></i> {{ icon }}
          </button>
        </li>
        <li v-if="filteredIcons.length === 0" class="dropdown-item text-muted">
          No matching icons
        </li>
      </ul>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { defineProps, defineEmits } from 'vue'

const props = defineProps({
  modelValue: {
    name: String,
    default: null
  }
})

const emit = defineEmits(['update:modelValue'])

const search = ref('')

const icons = [
  'address-book', 'anchor', 'bell', 'bolt', 'camera', 'cloud',
  'coffee', 'cog', 'comment', 'compass', 'download', 'edit',
  'envelope', 'eye', 'file', 'flag', 'folder', 'gift', 'globe',
  'heart', 'home', 'image', 'key', 'lightbulb', 'lock', 'map',
  'paper-plane', 'phone', 'question', 'search', 'shield-alt',
  'shopping-cart', 'star', 'sync', 'thumbs-up', 'trash', 'upload',
  'user', 'video', 'wifi', 'wrench'
]

const filteredIcons = computed(() =>
    icons.filter(icon => icon.includes(search.value.toLowerCase()))
)

const displayName = computed(() => {
  return props.modelValue?.replace(/^fa fa-/, '') || ''
})

function selectIcon(icon) {
  emit('update:modelValue', `fa fa-${icon}`)
}
</script>

<style scoped>
.dropdown-menu input.form-control {
  margin-bottom: 0.5rem;
}
</style>
