import { resolve } from 'path'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  define: {
    __VUE_PROD_DEVTOOLS__: true
  },
  build: {
    outDir: '../wwwroot/assets',
    emptyOutDir: true,
    assetsDir: '.',
    manifest: true,
    rollupOptions: {
      input: {
        dashboard: resolve(__dirname, 'dashboard.html'),
        settings: resolve(__dirname, 'settings.html'),
        tasks: resolve(__dirname, 'tasks.html'),
        email: resolve(__dirname, 'email.html'),
        calendar: resolve(__dirname, 'calendar.html'),
        notepad: resolve(__dirname, 'notepad.html'),
        github: resolve(__dirname, 'github.html'),
        links: resolve(__dirname, 'links.html'),
        jira: resolve(__dirname, 'jira.html'),
        debug: resolve(__dirname, 'debug.html'),
        'scheduled-tasks': resolve(__dirname, 'scheduled-tasks.html'),
        alarms: resolve(__dirname, 'alarms.html'),
      },
    },
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5000'
    }
  }
})
