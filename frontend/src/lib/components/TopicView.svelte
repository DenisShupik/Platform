<script lang="ts">
  import type { Topic } from '$lib/types/Topic'
  let { topic }: { topic: Topic } = $props()
</script>

<div>{topic.title}</div>
