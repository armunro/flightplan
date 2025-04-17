export default {
    props: {
        waypoints: {
            type: Array,
            required: true
        }
    },
    template: `
    <div class="fp-card-list" >
       <fpcard v-for="(wp, index) in waypoints" :key="index" :data="wp"></fpcard>
    </div>
  `
}
