<script lang="ts">
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { ErrorState } from '$lib/components/app'
	import * as m from '$lib/paraglide/messages'

	const cancelled = $derived(page.url.searchParams.get('error') === 'access_denied')
	const title = $derived(cancelled ? m.auth_cancelled_title() : m.auth_error_title())
	const description = $derived(
		cancelled ? m.auth_cancelled_description() : m.auth_error_description()
	)
</script>

<svelte:head>
	<title>{title} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<ErrorState {title} {description} />
