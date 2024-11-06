<script lang="ts">
  import { exchange } from '$lib/stores/authStore'
  import { goto } from '$app/navigation';
  exchange().then(() => {
    goto('/')
  })
</script>
