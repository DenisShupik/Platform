<script lang="ts">
  import ModeratorToolBar from '$lib/components/ModeratorToolBar.svelte'
  import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'
  import { IconSearch, IconUserCircle } from '@tabler/icons-svelte'
  import { Button } from '$lib/components/ui/button'
  import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
  import { Input } from '$lib/components/ui/input'
  import { authStore } from '$lib/stores/authStore'
  import { MainNav, MobileNav, ModeToggle } from '$lib/components'
</script>

<header
  class="border-border/40 bg-background/95 supports-[backdrop-filter]:bg-background/60 sticky top-0 z-50 w-full border-b backdrop-blur"
>
  <div class="container flex h-14 max-w-screen-2xl items-center">
    <MainNav />
    <MobileNav />
    <div
      class="flex flex-1 items-center justify-between space-x-4 md:justify-end"
    >
      <div class="ml-auto mr-2"><ModeratorToolBar /></div>
      <form class="flex-1 sm:flex-initial">
        <div class="relative">
          <IconSearch
            class="text-muted-foreground absolute left-2.5 top-2.5 h-4 w-4"
          />
          <Input
            type="search"
            placeholder="Поиск..."
            class="pl-8 sm:w-[300px] md:w-[200px] lg:w-[300px]"
          />
        </div>
      </form>
      <nav class="flex items-center gap-x-2">
        <DropdownMenu.Root>
          <DropdownMenu.Trigger>
            {#snippet child({ props })}
              <Button
                {...props}
                variant="secondary"
                size="icon"
                class="rounded-full"
              >
                <IconUserCircle class="size-5" />
                <span class="sr-only">Toggle user menu</span>
              </Button>
            {/snippet}
          </DropdownMenu.Trigger>
          <DropdownMenu.Content align="end">
            <DropdownMenu.Group>
              <DropdownMenu.GroupHeading
                >{$authStore?.username}</DropdownMenu.GroupHeading
              >
              <DropdownMenu.Separator />
              <DropdownMenu.Item>
                <RouteLink
                  link="/settings/profile"
                  title="Settings"
                /></DropdownMenu.Item
              >
              <DropdownMenu.Separator />
              <DropdownMenu.Item>Logout</DropdownMenu.Item>
            </DropdownMenu.Group>
          </DropdownMenu.Content>
        </DropdownMenu.Root>
        <ModeToggle />
      </nav>
    </div>
  </div>
</header>
