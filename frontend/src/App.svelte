<script lang="ts">
  import { onMount } from 'svelte'
  import keycloak from '$lib//keycloak'
  import { post } from '$lib/post'
  import {
    ChevronUp,
    MessageSquare,
    CircleUser,
    Search,
    Menu
  } from 'lucide-svelte'
  import './app.css'
  import * as Sheet from '$lib/components/ui/sheet'
  import { Button } from '$lib/components/ui/button'
  import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
  import { Input } from '$lib/components/ui/input'
  import * as Collapsible from '$lib/components/ui/collapsible'
  import AddSectionDialog from '$lib/components/AddSectionDialog.svelte'

  let authenticated: boolean | undefined = false
  let username: string

  onMount(async () => {
    try {
      await keycloak.init({
        onLoad: 'login-required'
      })
      authenticated = keycloak.authenticated
      username = keycloak.tokenParsed?.preferred_username || ''
    } catch (error) {
      console.error('Ошибка авторизации:', error)
    }
  })
</script>

{#if authenticated}
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
          <MessageSquare class="h-6 w-6" />
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
        <Sheet.Trigger asChild let:builder>
          <Button
            variant="outline"
            size="icon"
            class="shrink-0 md:hidden"
            builders={[builder]}
          >
            <Menu class="h-5 w-5" />
            <span class="sr-only">Toggle navigation menu</span>
          </Button>
        </Sheet.Trigger>
        <Sheet.Content side="left">
          <nav class="grid gap-6 text-lg font-medium">
            <a href="##" class="flex items-center gap-2 text-lg font-semibold">
              <MessageSquare class="h-6 w-6" />
              <span class="sr-only">Acme Inc</span>
            </a>
            <a href="##" class="hover:text-foreground">Разделы</a>
          </nav>
        </Sheet.Content>
      </Sheet.Root>
      <div class="flex w-full items-center gap-4 md:ml-auto md:gap-2 lg:gap-4">
        <div class="ml-auto"><AddSectionDialog></AddSectionDialog></div>
        <form class="flex-1 sm:flex-initial">
          <div class="relative">
            <Search
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
          <DropdownMenu.Trigger asChild let:builder>
            <Button
              builders={[builder]}
              variant="secondary"
              size="icon"
              class="rounded-full"
            >
              <CircleUser class="h-5 w-5" />
              <span class="sr-only">Toggle user menu</span>
            </Button>
          </DropdownMenu.Trigger>
          <DropdownMenu.Content align="end">
            <DropdownMenu.Label>My Account</DropdownMenu.Label>
            <DropdownMenu.Separator />
            <DropdownMenu.Item>Settings</DropdownMenu.Item>
            <DropdownMenu.Item>Support</DropdownMenu.Item>
            <DropdownMenu.Separator />
            <DropdownMenu.Item>Logout</DropdownMenu.Item>
          </DropdownMenu.Content>
        </DropdownMenu.Root>
      </div>
    </header>
    <main class="container">
      <Collapsible.Root class="w-full space-y-2">
        <div class="flex items-center justify-between space-x-4 px-4">
          <h4 class="text-sm font-semibold">
            @huntabyte starred 3 repositories
          </h4>
          <Collapsible.Trigger asChild let:builder>
            <Button
              builders={[builder]}
              variant="ghost"
              size="sm"
              class="w-9 p-0"
            >
              <ChevronUp class="h-4 w-4" />
              <span class="sr-only">Toggle</span>
            </Button>
          </Collapsible.Trigger>
        </div>
        <div class="rounded-md border px-4 py-3 font-mono text-sm">
          @huntabyte/bits-ui
        </div>
        <Collapsible.Content class="space-y-2">
          <div class="rounded-md border px-4 py-3 font-mono text-sm">
            @melt-ui/melt-ui
          </div>
          <div class="rounded-md border px-4 py-3 font-mono text-sm">
            @sveltejs/svelte
          </div>
        </Collapsible.Content>
      </Collapsible.Root>
    </main>
  </div>
{/if}
