<script lang="ts">
  import ModeratorToolBar from '$lib/components/ModeratorToolBar.svelte'
  import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'
  import { IconSearch, IconUserCircle } from '@tabler/icons-svelte'
  import { Button } from '$lib/components/ui/button'
  import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
  import { Input } from '$lib/components/ui/input'
  import { authStore } from '$lib/stores/authStore'
  import { MainNav, MobileNav, ModeToggle } from '$lib/components'
  import * as Avatar from '$lib/components/ui/avatar'
</script>

<header
  class="border-border/40 bg-background/95 supports-[backdrop-filter]:bg-background/60 sticky top-0 z-50 w-full border-b backdrop-blur"
>
  <div class="container flex h-14 max-w-screen-2xl items-center">
    <MainNav />
    <MobileNav />
    <div
      class="flex flex-1 items-center justify-between gap-x-2 md:gap-x-4 md:justify-end"
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
                variant="outline"
                size="icon"
                class="relative size-8 rounded-full"
              >
                {#if $authStore != null}
                  <Avatar.Root class="size-8">
                    <Avatar.Image src={$authStore.avatarUrl} alt="@shadcn" />
                    <Avatar.Fallback>{$authStore.username}</Avatar.Fallback>
                  </Avatar.Root>
                {:else}
                  <IconUserCircle />
                {/if}
                <span class="sr-only">Toggle user menu</span>
              </Button>
            {/snippet}
          </DropdownMenu.Trigger>
          <DropdownMenu.Content align="end">
            <DropdownMenu.Group>
              <DropdownMenu.GroupHeading
                ><div class="flex flex-col space-y-1">
                  <p class="text-sm font-medium leading-none">
                    {$authStore?.username}
                  </p>
                  <p class="text-muted-foreground text-xs leading-none">
                    {$authStore?.email}
                  </p>
                </div></DropdownMenu.GroupHeading
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
