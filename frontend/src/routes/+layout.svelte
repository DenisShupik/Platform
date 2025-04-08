<script lang="ts">
	import { AppHeader } from '$lib/components/app'
	import '../app.css'
	import { ModeWatcher } from 'mode-watcher'
	import type { LayoutProps } from './$types'
	import { authState } from '$lib/client/auth-state.svelte'
	let { children }: LayoutProps = $props()

	let intervalId: number | undefined = undefined

	$effect(() => {
		if (authState.keycloak.authenticated) {
			intervalId = setInterval(() => {
				authState.keycloak.updateToken(30).catch(() => {
					console.error('Не удалось обновить токен')
				})
			}, 3000)
		} else {
			clearInterval(intervalId)
			intervalId = undefined
		}
	})
</script>

<ModeWatcher />
<div class="bg-background relative flex min-h-screen flex-col">
	<AppHeader />
	{@render children?.()}
</div>
