export default {
    props: {
        objects: {
            type: Array,
            required: true
        }
    },
    template: `
    <div class="fp-card-list" >
       <fpcard v-for="(item, index) in objects" :key="index" :data="item"></fpcard>
    </div>
  `
}
