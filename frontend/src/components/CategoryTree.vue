<template>
  <div class="category-tree-node">
    <div class="category-item"
         :class="{ active: selectedId === category.id, 'editing-mode': isEditing }"
         :style="collapsed ? { justifyContent: 'center', paddingLeft: 0, paddingRight: 0 } : { paddingLeft: (0.75 + (depth || 0) * 1.5) + 'rem' }"
         @click="isEditing ? null : $emit('select', category.id)"
         :title="collapsed ? category.name : ''">
      <div v-if="!collapsed && hasChildren" class="expand-toggle me-1" @click.stop="isExpanded = !isExpanded">
        <i class="bi" :class="isExpanded ? 'bi-chevron-down' : 'bi-chevron-right'"></i>
      </div>
      <div v-else-if="!collapsed" class="expand-spacer me-1"></div>
      
      <div class="category-icon-wrapper" :style="category.color ? { color: category.color } : {}" :class="{ 'me-0': collapsed }">
        <i class="bi" :class="getCategoryIcon(category)"></i>
      </div>
      <span v-if="!collapsed" class="category-name text-truncate">{{ category.name }}</span>
      <span v-if="!collapsed" class="category-link-count ms-2">{{ category.bookmarks?.length || 0 }}</span>
      
      <div v-if="!collapsed && isEditing" class="category-actions ms-auto d-flex gap-1">
        <button class="btn btn-sm btn-link p-0 text-info" @click.stop="$emit('edit-cat', category)" title="Edit Category">
          <i class="bi bi-pencil"></i>
        </button>
        <button class="btn btn-sm btn-link p-0 text-info" @click.stop="$emit('add-sub', category.id)" title="Add Subcategory">
          <i class="bi bi-plus"></i>
        </button>
      </div>
    </div>
    <div v-if="isExpanded || collapsed" class="subcategories">
      <CategoryTree 
        v-for="sub in category.subcategories" 
        :key="sub.id" 
        :category="sub" 
        :selected-id="selectedId" 
        :collapsed="collapsed"
        :depth="(depth || 0) + 1"
        :is-editing="isEditing"
        @select="$emit('select', $event)"
        @add-sub="$emit('add-sub', $event)"
        @edit-cat="$emit('edit-cat', $event)"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
const props = defineProps(['category', 'selectedId', 'collapsed', 'depth', 'isEditing']);
defineEmits(['select', 'add-sub', 'edit-cat']);

const isExpanded = ref(true);
const hasChildren = computed(() => props.category.subcategories && props.category.subcategories.length > 0);

const getCategoryIcon = (category) => {
  const name = category.name.toLowerCase();
  if (name.includes('favorite') || name.includes('star')) return 'bi-star-fill';
  if (name.includes('work')) return 'bi-briefcase';
  if (name.includes('personal')) return 'bi-person';
  if (name.includes('social')) return 'bi-people';
  if (name.includes('news')) return 'bi-newspaper';
  if (name.includes('video')) return 'bi-play-circle';
  if (name.includes('music')) return 'bi-music-note-beamed';
  if (name.includes('dev') || name.includes('code')) return 'bi-code-slash';
  if (name.includes('shop')) return 'bi-cart';
  if (name.includes('travel')) return 'bi-airplane';
  return 'bi-folder';
};
</script>

<script>
export default {
  name: 'CategoryTree'
}
</script>

<style scoped>
.category-tree-node {
  width: 100%;
}

.subcategories {
  width: 100%;
}

.category-item {
  position: relative;
  padding: 0.75rem 0.5rem 0.75rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s;
  border-left: 3px solid transparent;
  min-width: 0;
  overflow: hidden;
  user-select: none;
}

.category-item:hover {
  background-color: var(--bg-card);
}

.category-item.active {
  background-color: rgba(88, 166, 255, 0.1);
  border-left-color: var(--accent-blue);
}

.category-item.editing-mode {
  cursor: default;
}

.expand-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  color: var(--text-muted);
  font-size: 0.7rem;
}

.expand-toggle:hover {
  color: var(--accent-blue);
}

.expand-spacer {
  width: 16px;
}

.category-icon-wrapper {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  font-size: 1.1rem;
  color: var(--accent-blue);
  flex-shrink: 0;
  transition: color 0.2s;
}

.category-name {
  flex-grow: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.9rem;
}

.category-link-count {
  font-size: 0.8rem;
  color: var(--text-muted);
  background-color: var(--bg-darker);
  padding: 1px 6px;
  border-radius: 10px;
}

.category-actions {
  opacity: 0;
  transition: opacity 0.2s;
  pointer-events: none;
}

.category-item:hover .category-actions,
.category-item.editing-mode .category-actions {
  opacity: 1;
  pointer-events: auto;
}
</style>