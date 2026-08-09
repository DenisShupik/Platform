<script lang="ts">
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { ErrorState } from '$lib/components/app'
	import * as m from '$lib/paraglide/messages'
</script>

<svelte:head>
	<title>{m.thread_not_found()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<ErrorState
	title={m.thread_not_found()}
	description={m.thread_not_found_description()}
	status={404}
/>
