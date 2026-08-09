<script lang="ts">
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { ErrorState } from '$lib/components/app'
	import * as m from '$lib/paraglide/messages'

	const title = $derived(
		page.status === 404
			? m.error_page_not_found()
			: page.status === 401
				? m.error_authentication_required()
				: m.error_something_wrong()
	)
	const description = $derived(
		page.status === 401 ? m.error_unauthorized() : m.error_request_failed()
	)
</script>

<svelte:head>
	<title>{title} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<ErrorState {title} {description} status={page.status} />
