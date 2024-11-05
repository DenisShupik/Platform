<script lang="ts">
  import { Separator } from '$lib/components/ui/separator'
  import { avatarUrl } from '$lib/env'
  import { currentUserStore } from '$lib/stores/currentUserStore'
  import { Button } from '../ui/button'
  import { Input } from '../ui/input'
  import { Label } from '../ui/label'
</script>

<div class="space-y-6">
  <div>
    <h3 class="text-lg font-medium">Профиль</h3>
    <p class="text-muted-foreground text-sm">
      Так Вас будут видеть другие пользователи на сайте
    </p>
  </div>
  <Separator />

  <form class="space-y-8">
    <div class="flex justify-center items-center w-full h-full">
      <img
        src="{avatarUrl}{$currentUserStore?.userId}.jpg"
        alt={$currentUserStore?.username}
        class="max-w-[128px] max-h-[128px] w-32 h-32 object-contain shadow-sm border rounded-lg"
      />
    </div>
  </form>
</div>
