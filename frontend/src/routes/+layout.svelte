<script lang="ts">
  import '../app.css'
  import * as Sheet from '$lib/components/ui/sheet'
  import { Button, buttonVariants } from '$lib/components/ui/button'
  import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
  import { Input } from '$lib/components/ui/input'
  import { ModeWatcher, setMode, resetMode } from 'mode-watcher'
  import {
    IconSun,
    IconMoon,
    IconSearch,
    IconMenu2,
    IconMessage,
    IconUserCircle
  } from '@tabler/icons-svelte'
  import { initAuthCodeFlow, authStore } from '$lib/stores/authStore'
  import { page } from '$app/stores'
  import ModeratorToolBar from '$lib/components/ModeratorToolBar.svelte'

  let { children } = $props()

  $effect(() => {
    if ($page.route.id !== '/auth/callback' && $authStore == null)
      initAuthCodeFlow()
  })
</script>

<ModeWatcher />
{#if $page.route.id !== '/auth/callback' && $authStore != null}
  <div class="flex min-h-screen w-full flex-col">
    <header
      class="bg-background sticky top-0 flex h-16 items-center gap-4 border-b px-4 md:px-6"
    >
      <nav
        class="hidden flex-col gap-6 text-lg font-medium md:flex md:flex-row md:items-center md:gap-5 md:text-sm lg:gap-6"
      >
        <a
          href="##"
          class="flex items-center gap-2 text-lg font-semibold md:text-base"
        >
          <IconMessage class="h-6 w-6" />
          <span class="sr-only">Acme Inc</span>
        </a>
        <a
          href="#Sections"
          class="text-foreground hover:text-foreground transition-colors"
        >
          Разделы
        </a>
      </nav>
      <Sheet.Root>
        <Sheet.Trigger>
          {#snippet child({ props })}
            <Button
              {...props}
              variant="outline"
              size="icon"
              class="shrink-0 md:hidden"
            >
              <IconMenu2 class="size-5" />
              <span class="sr-only">Toggle navigation menu</span>
            </Button>
          {/snippet}
        </Sheet.Trigger>
        <Sheet.Content side="left">
          <nav class="grid gap-6 text-lg font-medium">
            <a href="##" class="flex items-center gap-2 text-lg font-semibold">
              <IconMessage class="h-6 w-6" />
              <span class="sr-only">Acme Inc</span>
            </a>
            <a href="##" class="hover:text-foreground">Разделы</a>
          </nav>
        </Sheet.Content>
      </Sheet.Root>
      <div class="flex w-full items-center gap-4 md:ml-auto md:gap-2 lg:gap-4">
        <div class="ml-auto"><ModeratorToolBar /></div>
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
                >{$authStore.username}</DropdownMenu.GroupHeading
              >
              <DropdownMenu.Separator />
              <DropdownMenu.Item></DropdownMenu.Item>
              <DropdownMenu.Separator />
              <DropdownMenu.Item>Выйти</DropdownMenu.Item>
            </DropdownMenu.Group>
          </DropdownMenu.Content>
        </DropdownMenu.Root>

        <DropdownMenu.Root>
          <DropdownMenu.Trigger
            class={buttonVariants({ variant: 'outline', size: 'icon' })}
          >
            <IconSun
              class="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0"
            />
            <IconMoon
              class="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100"
            />
            <span class="sr-only">Toggle theme</span>
          </DropdownMenu.Trigger>
          <DropdownMenu.Content align="end">
            <DropdownMenu.Item onclick={() => setMode('light')}
              >Light</DropdownMenu.Item
            >
            <DropdownMenu.Item onclick={() => setMode('dark')}
              >Dark</DropdownMenu.Item
            >
            <DropdownMenu.Item onclick={() => resetMode()}
              >System</DropdownMenu.Item
            >
          </DropdownMenu.Content>
        </DropdownMenu.Root>
      </div>
    </header>
    <main class="container pt-8">
      {@render children()}
    </main>
  </div>
{:else if $page.route.id == '/auth/callback' && $authStore == null}
  {@render children()}
{/if}
