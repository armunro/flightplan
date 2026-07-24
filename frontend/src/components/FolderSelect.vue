<template>
  <select :value="currentFolderId" @change="onChange" class="form-select form-select-sm bg-dark border-secondary text-light">
    <option value="">Select Folder...</option>
    <option v-for="folder in folderTree" :key="folder.id" :value="folder.id">
      {{ getIndentedName(folder) }}
    </option>
  </select>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  folders: {
    type: Array,
    default: () => []
  },
  folderPreferences: {
    type: Object,
    default: () => ({})
  }
});

const emit = defineEmits(['update:modelValue']);

const folderTree = computed(() => {
  if (!props.folders || props.folders.length === 0) {
    return [];
  }
  const result = [];
  const map = {};
  
  props.folders.forEach(f => {
    map[f.id] = { 
      ...f, 
      children: [],
      displayName: props.folderPreferences?.[f.id]?.customName || f.displayName,
      order: props.folderPreferences?.[f.id]?.order ?? 999
    };
  });
  
  const roots = [];
  props.folders.forEach(f => {
    const node = map[f.id];
    if (f.parentFolderId && map[f.parentFolderId]) {
      map[f.parentFolderId].children.push(node);
    } else {
      roots.push(node);
    }
  });
  
  const sortFolders = (a, b) => {
    if (a.order !== 999 || b.order !== 999) {
      return a.order - b.order;
    }
    const folderOrder = ['inbox', 'archive', 'sentitems', 'drafts', 'deleteditems', 'junkemail'];
    const idxA = folderOrder.indexOf(a.displayName.toLowerCase().replace(' ', ''));
    const idxB = folderOrder.indexOf(b.displayName.toLowerCase().replace(' ', ''));
    if (idxA !== -1 && idxB !== -1) return idxA - idxB;
    if (idxA !== -1) return -1;
    if (idxB !== -1) return 1;
    return a.displayName.localeCompare(b.displayName);
  };

  const processLevel = (nodes, level = 0, path = '') => {
    const sortedNodes = [...nodes].sort(sortFolders);
    sortedNodes.forEach(node => {
      const fullPath = path ? `${path}\\${node.displayName}` : node.displayName;
      result.push({ ...node, level, fullPath });
      if (node.children && node.children.length > 0) {
        processLevel(node.children, level + 1, fullPath);
      }
    });
  };
  
  processLevel(roots);
  return result;
});

const currentFolderId = computed(() => {
  if (!props.modelValue) return '';
  
  // Try to find by ID first
  const byId = folderTree.value.find(f => f.id === props.modelValue);
  if (byId) return byId.id;
  
  // Try to find by path
  const byPath = folderTree.value.find(f => f.fullPath === props.modelValue);
  if (byPath) return byPath.id;

  // Try well-known names if it's a simple string like "Inbox"
  const wellKnown = folderTree.value.find(f => f.displayName.toLowerCase() === props.modelValue.toLowerCase());
  if (wellKnown) return wellKnown.id;
  
  return '';
});

const onChange = (event) => {
  const folderId = event.target.value;
  const folder = folderTree.value.find(f => f.id === folderId);
  if (folder) {
    emit('update:modelValue', folder.fullPath);
  } else {
    emit('update:modelValue', '');
  }
};

const getIndentedName = (folder) => {
  const indentation = '\u00A0'.repeat((folder.level || 0) * 4);
  return indentation + folder.displayName;
};
</script>
