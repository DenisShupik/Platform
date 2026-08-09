<script lang="ts">
	import LanguagesIcon from '@lucide/svelte/icons/languages'
	import { Button } from '$lib/components/ui/button'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
	import { authClient } from '$lib/client'
	import { getLocale, setLocale } from '$lib/paraglide/runtime'
	import * as m from '$lib/paraglide/messages'
	import { supportedLocales, type SupportedLocale } from '$lib/i18n'
	import { changeCurrentUserLocale, Locale } from '$lib/utils/client'
	import { withApiLocale } from '$lib/client/api-options'

	const session = authClient.useSession()
	const currentLocale = getLocale()
	let isChanging = $state(false)
	let isOpen = $state(false)
	let changeError = $state<string>()

	const apiLocales = {
		en: Locale.EN,
		ru: Locale.RU
	} satisfies Record<SupportedLocale, Locale>
	const apiLocale = (locale: SupportedLocale): Locale => apiLocales[locale]
	async function updateIdentityLocale(locale: SupportedLocale) {
		await changeCurrentUserLocale<true>(
			withApiLocale({
				body: { locale: apiLocale(locale) },
				throwOnError: true
			})
		)
	}

	async function selectLocale(locale: SupportedLocale) {
		if (locale === currentLocale || isChanging) return
		isChanging = true
		changeError = undefined
		try {
			if ($session.data) {
				await updateIdentityLocale(locale)
			}
			await setLocale(locale)
		} catch (error) {
			console.error('Failed to change locale:', error)
			changeError = m.language_change_failed()
			isOpen = true
			isChanging = false
		}
	}
</script>

<DropdownMenu.Root bind:open={isOpen}>
	<DropdownMenu.Trigger>
		{#snippet child({ props })}
			<Button {...props} variant="ghost" size="icon" disabled={isChanging}>
				<LanguagesIcon />
				<span class="sr-only">{m.language_menu()}</span>
			</Button>
		{/snippet}
	</DropdownMenu.Trigger>
	<DropdownMenu.Content align="end">
		<DropdownMenu.Label>{m.language_menu()}</DropdownMenu.Label>
		<DropdownMenu.Separator />
		<DropdownMenu.RadioGroup value={currentLocale}>
			<DropdownMenu.Group>
				{#each supportedLocales as locale (locale)}
					<DropdownMenu.RadioItem value={locale} onclick={() => selectLocale(locale)}>
						<span lang={locale}
							>{locale === 'en' ? m.language_english() : m.language_russian()}</span
						>
					</DropdownMenu.RadioItem>
				{/each}
			</DropdownMenu.Group>
		</DropdownMenu.RadioGroup>
		{#if changeError}
			<DropdownMenu.Separator />
			<p class="max-w-56 px-2 py-1.5 text-sm text-destructive" role="alert">{changeError}</p>
		{/if}
	</DropdownMenu.Content>
</DropdownMenu.Root>
