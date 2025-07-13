<script lang="ts">
	import { resetMode, setMode } from 'mode-watcher'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu/index.js'
	import { buttonVariants } from '$lib/components/ui/button/index.js'
	import { IconMoonFilled, IconSunFilled } from '@tabler/icons-svelte'
</script>

<DropdownMenu.Root>
	<DropdownMenu.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon' })}>
		<IconSunFilled
			class="text-primary transition-all! size-6 rotate-0 scale-100 dark:-rotate-90 dark:scale-0"
		/>
		<IconMoonFilled
			class="text-primary transition-all! absolute size-6 rotate-90 scale-0 dark:rotate-0 dark:scale-100"
		/>
		<span class="sr-only">Toggle theme</span>
	</DropdownMenu.Trigger>
	<DropdownMenu.Content align="end">
		<DropdownMenu.Item onclick={() => setMode('light')}>Light</DropdownMenu.Item>
		<DropdownMenu.Item onclick={() => setMode('dark')}>Dark</DropdownMenu.Item>
		<DropdownMenu.Item onclick={() => resetMode()}>System</DropdownMenu.Item>
	</DropdownMenu.Content>
</DropdownMenu.Root>
