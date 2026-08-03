<template>
  <div :class="['vh-100 d-flex flex-row overflow-hidden app-root', themeClass]">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <div id="app-content" class="links-app-container flex-grow-1">
        <div v-if="loading" class="p-4">Loading bookmarks...</div>
        <template v-else>
          <!-- Sidebar -->
        <div class="links-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center">
            <h5 v-if="!sidebarCollapsed" class="theme-text">Categories</h5>
            <div v-if="!sidebarCollapsed" class="d-flex align-items-center gap-1 ms-auto">
              <button class="btn-icon theme-text" @click="isEditingCategories = !isEditingCategories" :title="isEditingCategories ? 'Save Categories' : 'Edit Categories'">
                <i class="bi" :class="isEditingCategories ? 'bi-check-lg text-success' : 'bi-pencil-square'"></i>
              </button>
            </div>
            <div v-else class="mx-auto theme-text">
              <i class="bi bi-link-45deg"></i>
            </div>
          </div>
            <div class="category-list">
              <CategoryTree 
                v-for="category in categories" 
                :key="category.id" 
                :category="category" 
                :selected-id="selectedCategoryId" 
                :collapsed="sidebarCollapsed"
                :is-editing="isEditingCategories"
                @select="selectedCategoryId = $event"
                @add-sub="addSubcategory"
                @edit-cat="editCategory"
              />
            </div>
        <div class="sidebar-footer" :class="{ 'collapsed': sidebarCollapsed }">
          <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
            <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
          </button>
        </div>
          </div>

          <!-- Sidebar Resizer -->
          <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startResize"></div>

          <!-- Main Content -->
          <div class="main-content">
            <div class="links-container">
              <div class="controls-bar theme-border">
                <div class="category-title-area d-flex align-items-center gap-3">
                  <template v-if="selectedCategory">
                    <ColorPicker v-model="selectedCategory.color" size="sm" />
                    <h2 class="mb-0 text-truncate theme-text" style="max-width: 300px;">{{ selectedCategory.name }}</h2>
                    <button class="btn btn-sm btn-link text-info p-0" @click="editCategory(selectedCategory)" title="Edit Category">
                      <i class="bi bi-pencil"></i>
                    </button>
                  </template>
                  <h2 v-else class="theme-text">Links & Bookmarks</h2>
                </div>

                <div class="d-flex align-items-center gap-2 flex-grow-1 justify-content-end">
                  <div class="d-flex align-items-center gap-3 theme-bg-dark rounded-pill px-3 py-1 theme-border border">
                    <div class="search-box position-relative" style="width: 200px;">
                      <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-2 text-info opacity-75 x-small"></i>
                      <input v-model="searchQuery" class="form-control form-control-sm bg-transparent border-0 theme-text ps-4 search-input" placeholder="Search..." />
                      <button v-if="searchQuery" class="btn btn-link btn-sm position-absolute top-50 end-0 translate-middle-y me-0 p-0 theme-text-muted" @click="searchQuery = ''">
                        <i class="bi bi-x-circle-fill"></i>
                      </button>
                    </div>

                    <div class="vr h-50 my-auto theme-border opacity-25"></div>

                    <div class="form-check form-switch mb-0 d-flex align-items-center gap-2 ps-0">
                      <input class="form-check-input ms-0 mt-0" type="checkbox" role="switch" id="showChildrenToggle" v-model="showChildren">
                      <label class="form-check-label small theme-text text-nowrap" for="showChildrenToggle">Show Children</label>
                    </div>
                  </div>

                  <div class="btn-group btn-group-sm gap-2">
                    <button class="btn theme-btn-outline btn-sm" @click="$refs.fileInput.click()" title="Import Bookmark File">
                      <i class="bi bi-file-earmark-arrow-up me-1"></i>Import
                    </button>
                    <button class="btn btn-primary btn-sm px-3" @click="saveBookmarks" :disabled="saving" title="Save Changes">
                      <span v-if="saving" class="spinner-border spinner-border-sm"></span>
                      <i v-else class="bi bi-save me-1"></i>Save
                    </button>
                  </div>
                  <input type="file" ref="fileInput" class="d-none" accept=".html" @change="handleFileUpload" />
                </div>
              </div>

              <div v-if="selectedCategory" class="tiles-grid p-4">
                <div v-for="(link, lIdx) in allLinksInSelected" :key="link.id" class="link-tile-wrapper">
                  <div class="link-tile card theme-card h-100 shadow-sm">
                    <div class="card-body d-flex flex-column p-3">
                      <div class="d-flex justify-content-between align-items-start mb-2">
                        <img v-if="link.url" :src="getFavicon(link.url)" class="favicon-large" @error="handleIconError" />
                        <div v-else class="favicon-placeholder theme-text"><i class="bi bi-link-45deg"></i></div>
                        <div class="tile-actions">
                          <button class="btn btn-sm btn-link text-info p-0 me-2" @click="editLink(link.id)" title="Edit Link">
                            <i class="bi bi-pencil"></i>
                          </button>
                          <button class="btn btn-sm btn-link text-danger p-0" @click="removeLink(link.id)" title="Remove Link">
                            <i class="bi bi-x-lg"></i>
                          </button>
                        </div>
                      </div>
                      <a :href="link.url" target="_blank" class="tile-title text-decoration-none text-info fw-bold mb-1">
                        {{ link.title || 'Untitled' }}
                      </a>
                      <div class="tile-description small text-truncate-2" v-if="link.description">
                        {{ link.description }}
                      </div>
                      <div class="mt-auto pt-2 tile-url text-truncate">
                        {{ link.url }}
                      </div>
                    </div>
                  </div>
                </div>
                <!-- Add Link Tile -->
                <div class="link-tile-wrapper">
                  <div class="link-tile card theme-card border-dashed h-100 shadow-sm add-tile" @click="addLink">
                    <div class="card-body d-flex flex-column align-items-center justify-content-center text-info">
                      <i class="bi bi-plus-lg fs-2 mb-2"></i>
                      <span class="fw-bold">Add Link</span>
                    </div>
                  </div>
                </div>
              </div>
              <div v-else class="empty-state p-4 text-center">
                <i class="bi bi-bookmark-star display-4 text-muted mb-3"></i>
                <p class="lead fs-7" style="font-size: 0.85rem;">Select a category from the sidebar or add a new one.</p>
                <button class="btn btn-outline-primary mt-2" @click="isEditingCategories = true; addCategory()">
                  <i class="bi bi-folder-plus me-1"></i>Add Category
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>

      <!-- Edit Link Modal -->
      <div v-if="editingLink" class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);" @keydown.esc="editingLink = null">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content theme-modal">
            <div class="modal-header theme-modal-header">
              <h5 class="modal-title">{{ isNewLink ? 'Add Link' : 'Edit Link' }}</h5>
              <button type="button" class="btn-close theme-btn-close" @click="editingLink = null"></button>
            </div>
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label theme-text text-info small fw-bold">URL</label>
                <input v-model="tempLink.url" class="form-control theme-input" placeholder="https://..." @blur="autoPopulateTitle" />
              </div>
              <div class="mb-3">
                <label class="form-label theme-text text-info small fw-bold">Title</label>
                <input v-model="tempLink.title" class="form-control theme-input" placeholder="Site Name" />
              </div>
              <div class="mb-3">
                <label class="form-label theme-text text-info small fw-bold">Description</label>
                <textarea v-model="tempLink.description" class="form-control theme-input" rows="2" placeholder="Brief description..."></textarea>
              </div>
            </div>
            <div class="modal-footer theme-modal-footer">
              <button type="button" class="btn btn-secondary" @click="editingLink = null">Cancel</button>
              <button type="button" class="btn btn-primary" @click="saveLinkEdit">Confirm</button>
            </div>
          </div>
        </div>
      </div>
      <!-- Edit Category Modal -->
      <div v-if="editingCategory" class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);" @keydown.esc="editingCategory = null">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content theme-modal">
            <div class="modal-header theme-modal-header">
              <h5 class="modal-title">Edit Category</h5>
              <button type="button" class="btn-close theme-btn-close" @click="editingCategory = null"></button>
            </div>
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label theme-text text-info small fw-bold">Name</label>
                <input v-model="tempCategory.name" class="form-control theme-input" placeholder="Category Name" />
              </div>
              <div class="mb-3">
                <label class="form-label theme-text text-info small fw-bold">Color</label>
                <ColorPicker v-model="tempCategory.color" show-text size="lg" palette-placement="top-start" />
              </div>
            </div>
            <div class="modal-footer theme-modal-footer justify-content-between">
              <button type="button" class="btn btn-outline-danger" @click="removeCategory(tempCategory.id)">
                <i class="bi bi-trash me-1"></i>Delete
              </button>
              <div>
                <button type="button" class="btn btn-secondary me-2" @click="editingCategory = null">Cancel</button>
                <button type="button" class="btn btn-primary" @click="saveCategoryEdit">Save</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import Navbar from './components/Navbar.vue';
import ColorPicker from './components/ColorPicker.vue';
import { fetchSettings } from './js/dashboard-api';

import CategoryTree from './components/CategoryTree.vue';

const categories = ref([]);
const theme = ref('Cosmic');
const themeClass = computed(() => `theme-${theme.value.toLowerCase()}`);
const loading = ref(true);
const saving = ref(false);
const editingLink = ref(null);
const editingCategory = ref(null);
const isEditingCategories = ref(false);
const tempLink = ref({});
const tempCategory = ref({});
const isNewLink = ref(false);
const fileInput = ref(null);
const sidebarCollapsed = ref(false);
const selectedCategoryId = ref(null);
const showChildren = ref(true);
const searchQuery = ref('');
const sidebarWidth = ref(260);
const isResizing = ref(false);
let startX = 0;
let startWidth = 0;

let activeLinkUniqueId = null;
let activeLinkIdx = -1;

const loadSetting = (key, defaultValue) => {
  const val = localStorage.getItem(key);
  if (!val) return defaultValue;
  try {
    return JSON.parse(val);
  } catch (e) {
    return defaultValue;
  }
};

const onKeyDown = (e) => {
  if (e.key === 'Escape') {
    if (editingLink.value) editingLink.value = null;
    else if (editingCategory.value) editingCategory.value = null;
  }
};

onMounted(async () => {
  window.addEventListener('keydown', onKeyDown);
  sidebarCollapsed.value = loadSetting('linksSidebarCollapsed', false);
  selectedCategoryId.value = loadSetting('linksSelectedCategoryId', null);
  showChildren.value = loadSetting('linksShowChildren', true);
  sidebarWidth.value = loadSetting('linksSidebarWidth', 260);
  await fetchBookmarks();
});

onUnmounted(() => {
  window.removeEventListener('keydown', onKeyDown);
});

const sidebarStyle = computed(() => {
  if (sidebarCollapsed.value) return {};
  return { 
    width: sidebarWidth.value + 'px',
    transition: isResizing.value ? 'none' : 'width 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
  };
});

const selectedCategory = computed(() => {
  if (!selectedCategoryId.value) return null;
  return findCategoryById(categories.value, selectedCategoryId.value);
});

const allLinksInSelected = computed(() => {
  if (!selectedCategory.value) return [];
  
  const collect = (cat) => {
    let links = [...(cat.bookmarks || [])];
    if (showChildren.value && cat.subcategories) {
      for (const sub of cat.subcategories) {
        links = links.concat(collect(sub));
      }
    }
    return links;
  };
  
  let allLinks = collect(selectedCategory.value);

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    allLinks = allLinks.filter(l => 
      (l.title && l.title.toLowerCase().includes(query)) ||
      (l.url && l.url.toLowerCase().includes(query)) ||
      (l.description && l.description.toLowerCase().includes(query))
    );
  }

  return allLinks;
});

const selectedCategoryIdx = computed(() => {
  if (!selectedCategoryId.value) return -1;
  return categories.value.findIndex(c => c.id === selectedCategoryId.value);
});

watch(selectedCategoryId, (newId) => {
  if (newId) {
    localStorage.setItem('linksSelectedCategoryId', JSON.stringify(newId));
  }
});

watch(sidebarCollapsed, (newVal) => {
  localStorage.setItem('linksSidebarCollapsed', JSON.stringify(newVal));
});

watch(showChildren, (newVal) => {
  localStorage.setItem('linksShowChildren', JSON.stringify(newVal));
});

watch(sidebarWidth, (newVal) => {
  localStorage.setItem('linksSidebarWidth', JSON.stringify(newVal));
});

const startResize = (e) => {
  isResizing.value = true;
  startX = e.clientX;
  startWidth = sidebarWidth.value;
  
  document.addEventListener('mousemove', doResize);
  document.addEventListener('mouseup', stopResize);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
  
  e.preventDefault();
  e.stopPropagation();
};

const doResize = (e) => {
  if (!isResizing.value) return;
  const delta = e.clientX - startX;
  const newWidth = startWidth + delta;
  if (newWidth > 150 && newWidth < 600) {
    sidebarWidth.value = newWidth;
  }
};

const stopResize = () => {
  isResizing.value = false;
  document.removeEventListener('mousemove', doResize);
  document.removeEventListener('mouseup', stopResize);
  document.body.style.cursor = '';
  document.body.style.userSelect = '';
};

const fetchBookmarks = async () => {
  loading.value = true;
  try {
    const [bookmarksResponse, settings] = await Promise.all([
      fetch('/api/bookmarks'),
      fetchSettings()
    ]);
    
    if (settings) {
      theme.value = settings.theme || 'Cosmic';
    }

    if (bookmarksResponse.ok) {
      const data = await bookmarksResponse.json();
      
      const ensureIds = (cats) => {
        return cats.map(c => ({
          ...c,
          id: c.id || crypto.randomUUID(),
          bookmarks: c.bookmarks || [],
          subcategories: c.subcategories ? ensureIds(c.subcategories) : []
        }));
      };

      categories.value = ensureIds(data);
      
      if (categories.value.length > 0) {
        if (!selectedCategoryId.value || !findCategoryById(categories.value, selectedCategoryId.value)) {
          selectedCategoryId.value = categories.value[0].id;
        }
      }
    }
  } catch (e) {
    console.error('Error fetching bookmarks:', e);
  } finally {
    loading.value = false;
  }
};

const findCategoryById = (cats, id) => {
  for (const cat of cats) {
    if (cat.id === id) return cat;
    if (cat.subcategories) {
      const found = findCategoryById(cat.subcategories, id);
      if (found) return found;
    }
  }
  return null;
};

const findParentCategory = (cats, childId) => {
  for (const cat of cats) {
    if (cat.subcategories && cat.subcategories.some(sub => sub.id === childId)) {
      return cat;
    }
    if (cat.subcategories) {
      const found = findParentCategory(cat.subcategories, childId);
      if (found) return found;
    }
  }
  return null;
};

const saveBookmarks = async () => {
  saving.value = true;
  try {
    const response = await fetch('/api/bookmarks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(categories.value)
    });
    if (response.ok) {
      // Show success toast or feedback
    }
  } catch (e) {
    console.error('Error saving bookmarks:', e);
  } finally {
    saving.value = false;
  }
};

const addCategory = () => {
  const newId = crypto.randomUUID();
  categories.value.push({
    id: newId,
    name: 'New Category',
    color: '#58a6ff',
    bookmarks: [],
    subcategories: []
  });
  selectedCategoryId.value = newId;
};

const addSubcategory = (parentId) => {
  const parent = findCategoryById(categories.value, parentId);
  if (parent) {
    const newId = crypto.randomUUID();
    if (!parent.subcategories) parent.subcategories = [];
    parent.subcategories.push({
      id: newId,
      name: 'New Subcategory',
      color: parent.color || '#58a6ff',
      bookmarks: [],
      subcategories: []
    });
    selectedCategoryId.value = newId;
    
    // Open edit modal for the new subcategory
    editCategory(parent.subcategories[parent.subcategories.length - 1]);
  }
};

const editCategory = (category) => {
  tempCategory.value = JSON.parse(JSON.stringify(category));
  editingCategory.value = true;
};

const saveCategoryEdit = () => {
  const cat = findCategoryById(categories.value, tempCategory.value.id);
  if (cat) {
    cat.name = tempCategory.value.name;
    cat.color = tempCategory.value.color;
  }
  editingCategory.value = null;
};

const removeCategory = (id) => {
  if (confirm('Are you sure you want to remove this category and all its links?')) {
    const parent = findParentCategory(categories.value, id);
    if (parent) {
      const idx = parent.subcategories.findIndex(c => c.id === id);
      parent.subcategories.splice(idx, 1);
    } else {
      const idx = categories.value.findIndex(c => c.id === id);
      if (idx !== -1) {
        categories.value.splice(idx, 1);
      }
    }
    
    if (selectedCategoryId.value === id) {
      selectedCategoryId.value = categories.value.length > 0 ? categories.value[0].id : null;
    }
    editingCategory.value = null;
  }
};

const addLink = () => {
  if (!selectedCategory.value) return;
  isNewLink.value = true;
  tempLink.value = { title: '', url: '', description: '', id: crypto.randomUUID(), createdAt: new Date() };
  editingLink.value = true;
};

const editLink = (linkId) => {
  const link = allLinksInSelected.value.find(l => l.id === linkId);
  if (link) {
    activeLinkUniqueId = linkId;
    isNewLink.value = false;
    tempLink.value = { ...link };
    editingLink.value = true;
  }
};

const saveLinkEdit = () => {
  if (isNewLink.value) {
    selectedCategory.value.bookmarks.push({ ...tempLink.value });
  } else {
    // Find where the link is
    const findAndReplace = (cats) => {
      for (const cat of cats) {
        const idx = cat.bookmarks.findIndex(l => l.id === activeLinkUniqueId);
        if (idx !== -1) {
          cat.bookmarks[idx] = { ...tempLink.value };
          return true;
        }
        if (cat.subcategories && findAndReplace(cat.subcategories)) return true;
      }
      return false;
    };
    findAndReplace(categories.value);
  }
  editingLink.value = null;
};

const removeLink = (linkId) => {
  const findAndRemove = (cats) => {
    for (const cat of cats) {
      const idx = cat.bookmarks.findIndex(l => l.id === linkId);
      if (idx !== -1) {
        cat.bookmarks.splice(idx, 1);
        return true;
      }
      if (cat.subcategories && findAndRemove(cat.subcategories)) return true;
    }
    return false;
  };
  findAndRemove(categories.value);
};

const getFavicon = (url) => {
  try {
    const domain = new URL(url).hostname;
    return `https://www.google.com/s2/favicons?domain=${domain}&sz=32`;
  } catch (e) {
    return '';
  }
};

const handleIconError = (e) => {
  e.target.style.display = 'none';
};

const handleFileUpload = (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    const content = e.target.result;
    parseBookmarksHtml(content);
  };
  reader.readAsText(file);
  
  // Reset input
  event.target.value = '';
};

const parseBookmarksHtml = (html) => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(html, 'text/html');
  
  const results = [];
  
  const parseDL = (dlElement, parentCategory = null) => {
    const children = dlElement.children;
    for (let i = 0; i < children.length; i++) {
      const child = children[i];
      if (child.tagName === 'DT') {
        const h3 = child.querySelector('h3');
        const a = child.querySelector('a');
        
        if (h3) {
          const folderName = h3.textContent;
          const newCategory = {
            id: crypto.randomUUID(),
            name: folderName,
            color: '#58a6ff',
            bookmarks: [],
            subcategories: []
          };
          
          if (parentCategory) {
            parentCategory.subcategories.push(newCategory);
          } else {
            results.push(newCategory);
          }
          
          const nextDl = child.querySelector('dl') || (child.nextElementSibling?.tagName === 'DL' ? child.nextElementSibling : null);
          if (nextDl) {
            parseDL(nextDl, newCategory);
          }
        } else if (a) {
          const link = {
            id: crypto.randomUUID(),
            title: a.textContent,
            url: a.getAttribute('href'),
            description: a.getAttribute('description') || '',
            createdAt: new Date()
          };
          
          if (parentCategory) {
            parentCategory.bookmarks.push(link);
          } else {
            let importedCat = results.find(c => c.name === 'Imported');
            if (!importedCat) {
              importedCat = { id: crypto.randomUUID(), name: 'Imported', color: '#58a6ff', bookmarks: [], subcategories: [] };
              results.push(importedCat);
            }
            importedCat.bookmarks.push(link);
          }
        }
      }
    }
  };

  const rootDl = doc.querySelector('dl');
  if (rootDl) {
    parseDL(rootDl);
  } else {
    const links = doc.querySelectorAll('a');
    if (links.length > 0) {
      const importedCat = { id: crypto.randomUUID(), name: 'Imported', color: '#58a6ff', bookmarks: [], subcategories: [] };
      links.forEach(a => {
        importedCat.bookmarks.push({
          id: crypto.randomUUID(),
          title: a.textContent,
          url: a.getAttribute('href'),
          description: a.getAttribute('description') || '',
          createdAt: new Date()
        });
      });
      results.push(importedCat);
    }
  }

  if (results.length > 0) {
    const hasContent = (cat) => {
      return cat.bookmarks.length > 0 || (cat.subcategories && cat.subcategories.some(sub => hasContent(sub)));
    };
    const validResults = results.filter(c => hasContent(c));
    if (validResults.length > 0) {
      categories.value = [...categories.value, ...validResults];
    }
  }
};

const autoPopulateTitle = () => {
  if (tempLink.value.url && !tempLink.value.title) {
    try {
      const url = new URL(tempLink.value.url);
      let host = url.hostname.replace('www.', '');
      const parts = host.split('.');
      if (parts.length > 0) {
        tempLink.value.title = parts[0].charAt(0).toUpperCase() + parts[0].slice(1);
      }
    } catch (e) {}
  }
};
</script>

<style>
.form-control::placeholder {
  color: #aab2bb !important;
  opacity: 0.6 !important;
}

label, .form-label {
  color: var(--text-primary) !important;
  opacity: 0.9 !important;
}
</style>

<style scoped>
.links-app-container {
  display: flex;
  height: 100%;
  background-color: var(--bg-darker);
  color: var(--text-primary);
}

.app-root {
  background-color: var(--bg-darker);
}

.main-wrapper {
  background-color: var(--bg-darker);
}

.links-sidebar {
  width: 230px;
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  background-color: var(--bg-dark);
  overflow: hidden;
  flex-shrink: 0;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.links-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header {
  /* height and other properties moved to global.css */
}

.sidebar-footer {
  /* properties moved to global.css */
}

.sidebar-toggle {
  /* properties moved to global.css */
}

.category-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.category-link-count {
  font-size: 0.8rem;
  color: var(--text-muted);
  background-color: var(--bg-darker);
  padding: 2px 6px;
  border-radius: 10px;
}

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.links-container {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.search-input::placeholder {
  color: #aab2bb;
  opacity: 0.8;
  font-size: 0.85rem;
}

.x-small {
  font-size: 0.75rem;
}

.bg-darker {
  background-color: var(--bg-darker) !important;
}

.category-title-area h2 {
  outline: none;
}

.tiles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1.5rem;
  overflow-y: auto;
}

.link-tile {
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
  min-height: 140px;
}

.link-tile:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.3) !important;
}

.tile-actions {
  opacity: 0;
  transition: opacity 0.2s;
}

.link-tile:hover .tile-actions {
  opacity: 1;
}

.favicon-large {
  width: 32px;
  height: 32px;
  border-radius: 4px;
}

.favicon-placeholder {
  width: 32px;
  height: 32px;
  background-color: var(--bg-card);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
}

.tile-title {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  font-size: var(--fs-base);
  line-height: 1.2;
}

.tile-description {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  color: #aab2bb !important;
  font-size: var(--fs-xs);
}

.tile-url {
  font-size: var(--fs-xs);
  color: var(--text-primary) !important;
  opacity: 0.7;
}

.x-small {
  font-size: 0.75rem;
}

.text-truncate-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.border-dashed {
  border-style: dashed !important;
}

.add-tile:hover {
  background-color: rgba(255,255,255,0.05) !important;
  border-color: var(--accent-blue) !important;
  color: var(--accent-blue) !important;
}

.add-tile:hover .text-info {
  color: var(--accent-blue) !important;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
}

.bg-darker {
  background-color: #0d1117 !important;
}

.modal-body .bg-darker {
  background-color: #161b22 !important;
}

.modal-content {
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
}
</style>
