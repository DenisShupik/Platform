<script lang="ts">
	import { resetMode, setMode } from 'mode-watcher'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu/index.js'
	import { buttonVariants } from '$lib/components/ui/button/index.js'
	import IconMoonFilled from '~icons/tabler/moon-filled'
	import IconSunFilled from '~icons/tabler/sun-filled'
	import * as m from '$lib/paraglide/messages'
</script>

<DropdownMenu.Root>
	<DropdownMenu.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon' })}>
		<IconSunFilled
			class="size-6 scale-100 rotate-0 text-primary opacity-100 transition-[transform,opacity] duration-200 dark:scale-0 dark:-rotate-90 dark:opacity-0"
		/>
		<IconMoonFilled
			class="absolute size-6 scale-0 rotate-90 text-primary opacity-0 transition-[transform,opacity] duration-200 dark:scale-100 dark:rotate-0 dark:opacity-100"
		/>
		<span class="sr-only">{m.theme_toggle()}</span>
	</DropdownMenu.Trigger>
	<DropdownMenu.Content align="end">
		<DropdownMenu.Item onclick={() => setMode('light')}>{m.theme_light()}</DropdownMenu.Item>
		<DropdownMenu.Item onclick={() => setMode('dark')}>{m.theme_dark()}</DropdownMenu.Item>
		<DropdownMenu.Item onclick={() => resetMode()}>{m.theme_system()}</DropdownMenu.Item>
	</DropdownMenu.Content>
</DropdownMenu.Root>
