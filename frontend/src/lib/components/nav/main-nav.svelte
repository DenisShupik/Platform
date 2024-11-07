<script lang="ts">
  import { goto } from '$app/navigation'
  import { page } from '$app/stores'
  import { siteConfig } from '$lib/config/site.js'
  import { preventDefault } from '$lib/utils/preventDefault'
  import { cn } from '$lib/utils.js'
  import { IconMessageCircleFilled } from '@tabler/icons-svelte'
</script>

<div class="mr-4 hidden md:flex">
  <a
    href="/"
    class="mr-6 flex items-center space-x-2"
    onclick={preventDefault(() => goto('/'))}
  >
    <IconMessageCircleFilled class="h-6 w-6" />
    <span class="hidden font-bold xl:inline-block">
      {siteConfig.name}
    </span>
  </a>
  <nav class="flex items-center gap-6 text-sm">
    <a
      href="/"
      class={cn(
        'hover:text-foreground/80 transition-colors',
        $page.url.pathname === '/' ? 'text-foreground' : 'text-foreground/60'
      )}
      onclick={preventDefault(() => goto('/'))}
      >Sections
    </a>
  </nav>
</div>
