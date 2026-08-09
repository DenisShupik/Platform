<script lang="ts">
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { ErrorState } from '$lib/components/app'
</script>

<svelte:head>
	<title>Thread not found — {PUBLIC_APP_NAME}</title>
</svelte:head>

<ErrorState
	title="Thread not found"
	description="The thread may have been removed, or you may not have permission to view it."
	status={404}
/>
