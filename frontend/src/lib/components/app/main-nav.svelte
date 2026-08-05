<script lang="ts">
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { cn } from '$lib/utils.js'
	import IconMessageCircleFilled from '~icons/tabler/message-circle-filled'
	import { resolve } from '$app/paths'
</script>

<div class="hidden items-center gap-6 md:flex">
	<a href={resolve('/')} class="flex items-center gap-2">
		<IconMessageCircleFilled class="size-6" />
		<span class="hidden font-bold xl:inline-block">
			{PUBLIC_APP_NAME}
		</span>
	</a>
	<nav class="flex items-center gap-6 text-sm" aria-label="Основная навигация">
		<a
			href={resolve('/')}
			class={cn(
				'transition-colors hover:text-foreground/80',
				page.url.pathname === '/' ? 'text-foreground' : 'text-foreground/60'
			)}
			>Forums
		</a>
	</nav>
</div>
