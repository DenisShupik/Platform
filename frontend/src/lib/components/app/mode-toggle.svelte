<script lang="ts">
	import { resetMode, setMode } from 'mode-watcher'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu/index.js'
	import { buttonVariants } from '$lib/components/ui/button/index.js'
	import IconMoonFilled from '~icons/tabler/moon-filled'
	import IconSunFilled from '~icons/tabler/sun-filled'
</script>

<DropdownMenu.Root>
	<DropdownMenu.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon' })}>
		<IconSunFilled
			class="size-6 scale-100 rotate-0 text-primary transition-all! dark:scale-0 dark:-rotate-90"
		/>
		<IconMoonFilled
			class="absolute size-6 scale-0 rotate-90 text-primary transition-all! dark:scale-100 dark:rotate-0"
		/>
		<span class="sr-only">Toggle theme</span>
	</DropdownMenu.Trigger>
	<DropdownMenu.Content align="end">
		<DropdownMenu.Item onclick={() => setMode('light')}>Light</DropdownMenu.Item>
		<DropdownMenu.Item onclick={() => setMode('dark')}>Dark</DropdownMenu.Item>
		<DropdownMenu.Item onclick={() => resetMode()}>System</DropdownMenu.Item>
	</DropdownMenu.Content>
</DropdownMenu.Root>
