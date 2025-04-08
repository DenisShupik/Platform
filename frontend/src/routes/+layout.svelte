<script lang="ts">
	import { AppHeader } from '$lib/components/app'
	import '../app.css'
	import { ModeWatcher } from 'mode-watcher'
	import type { LayoutProps } from './$types'
	import { authStore } from '$lib/client/auth-state.svelte'
	let { children }: LayoutProps = $props()
</script>

<ModeWatcher />
<div class="bg-background relative flex min-h-screen flex-col">
	<AppHeader />
	{@render children?.()}
</div>
