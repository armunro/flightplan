import { createApp } from 'vue'
import '../global.css'
import '../tasks-style.css'
import App from '../TasksApp.vue'

const app = createApp(App)

// Custom directive for focusing the priority select
app.directive('focus', {
  mounted(el) {
    el.focus();
  }
});

app.mount('#app')
