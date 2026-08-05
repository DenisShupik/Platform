<script lang="ts">
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
</script>

<svelte:head>
	<title>Ошибка — {PUBLIC_APP_NAME}</title>
</svelte:head>

<h1 class="text-center">{page.error?.message}</h1>
