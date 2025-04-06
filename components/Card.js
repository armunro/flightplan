export default {
    props: {
        data: {
            type: Object,
            required: true
        }
    },
    template: `

    <div class="task-card">
          <span class="fp-ref">{{ data.Key }}</span> FROM <span class="fp-source">{{data.Source }}</span><br>
                    <span class="fp-label">START: </span> {{ data.TargetStart }}<br>
                    <span class="fp-label">END:</span> {{ data.TargetEnd }}  
    </div>
  `
}
