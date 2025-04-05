export default {
    props: {
        data: {
            type: Object,
            required: true
        }
    },
    template: `

    <div class="task-card">
          {{ data.Key }} FROM {{ data.Source }}<br>
                    S {{ data.TargetStart }} -> E {{ data.TargetEnd }}  
    </div>
  `
}
