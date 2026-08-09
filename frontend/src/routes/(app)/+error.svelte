<script lang="ts">
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { ErrorState } from '$lib/components/app'

	const title = $derived(page.status === 404 ? 'Page not found' : 'Something went wrong')
	const description = $derived(
		page.error?.message ?? 'The request could not be completed. Please try again.'
	)
</script>

<svelte:head>
	<title>{title} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<ErrorState {title} {description} status={page.status} />
