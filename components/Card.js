export default {
    props: {
        data: {
            type: Object,
            required: true
        }
    },
    template: `

    <div class="task-card">
          <h5>{{ data.Source }} -> {{ data.Key }}</h5>
    </div>
  `
}
