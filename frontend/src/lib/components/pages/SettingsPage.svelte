<script lang="ts">
  import SidebarNav from '$lib/components/SidebarNav.svelte'
  import { route } from '$lib/stores/routeStore'
  import ProfileView from './ProfileView.svelte'
  import AvatarPape from './AvatarPage.svelte'

  const sidebarNavItems = [
    {
      title: 'Profile',
      href: '/users/current/settings/profile'
    },
    {
      title: 'Notifications',
      href: '/users/current/settings/avatar'
    }
  ]
</script>

<div
  class="flex flex-col space-y-8 lg:flex-row lg:space-x-12 lg:space-y-0 lg:px-24"
>
  <aside class="-mx-4 lg:w-1/5">
    <SidebarNav items={sidebarNavItems} />
  </aside>
  <div class="w-full">
    {#if $route.path === sidebarNavItems[0].href}
      <ProfileView />
    {:else if $route.path === sidebarNavItems[1].href}
      <AvatarPape />
    {/if}
  </div>
</div>
