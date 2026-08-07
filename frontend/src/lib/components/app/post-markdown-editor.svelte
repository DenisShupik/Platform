<script lang="ts">
	import type { Component, Snippet } from 'svelte'
	import { tick } from 'svelte'
	import { type TextAreaProps } from 'carta-md'
	import { createPostCarta } from '$lib/markdown/carta'
	import * as ButtonGroup from '$lib/components/ui/button-group'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Empty from '$lib/components/ui/empty'
	import * as ScrollArea from '$lib/components/ui/scroll-area'
	import * as Tabs from '$lib/components/ui/tabs'
	import { Textarea } from '$lib/components/ui/textarea'
	import * as Tooltip from '$lib/components/ui/tooltip'
	import PostMarkdown from './post-markdown.svelte'
	import BoldIcon from '@lucide/svelte/icons/bold'
	import CodeXmlIcon from '@lucide/svelte/icons/code-xml'
	import Heading2Icon from '@lucide/svelte/icons/heading-2'
	import ItalicIcon from '@lucide/svelte/icons/italic'
	import LinkIcon from '@lucide/svelte/icons/link'
	import ListChecksIcon from '@lucide/svelte/icons/list-checks'
	import ListIcon from '@lucide/svelte/icons/list'
	import ListOrderedIcon from '@lucide/svelte/icons/list-ordered'
	import TextQuoteIcon from '@lucide/svelte/icons/text-quote'
	import StrikethroughIcon from '@lucide/svelte/icons/strikethrough'

	let {
		value = $bindable(''),
		textarea,
		placeholder = 'Напишите сообщение в Markdown',
		footer
	}: {
		value?: string
		textarea: TextAreaProps
		placeholder?: string
		footer?: Snippet
	} = $props()

	type EditorTab = 'write' | 'preview'
	type ToolbarTool = {
		label: string
		icon: Component
		action: () => Promise<void>
	}

	const carta = createPostCarta()
	const isInvalid = $derived(
		textarea['aria-invalid'] === true || textarea['aria-invalid'] === 'true'
	)
	const isPreviewEmpty = $derived(value.trim().length === 0)

	let selectedTab = $state<EditorTab>('write')
	let textareaElement = $state<HTMLTextAreaElement | null>(null)

	function focusTextarea(selectionStart: number, selectionEnd = selectionStart) {
		void tick().then(() => {
			textareaElement?.focus()
			textareaElement?.setSelectionRange(selectionStart, selectionEnd)
		})
	}

	function replaceRange(
		start: number,
		end: number,
		replacement: string,
		nextSelectionStart: number,
		nextSelectionEnd = nextSelectionStart
	) {
		value = `${value.slice(0, start)}${replacement}${value.slice(end)}`
		focusTextarea(nextSelectionStart, nextSelectionEnd)
	}

	async function withTextarea(action: (input: HTMLTextAreaElement) => void) {
		selectedTab = 'write'
		await tick()

		if (textareaElement) {
			action(textareaElement)
		}
	}

	async function surroundSelection(opening: string, closing = opening) {
		await withTextarea((input) => {
			const start = input.selectionStart
			const end = input.selectionEnd
			const selected = value.slice(start, end)
			const before = value.slice(0, start)
			const after = value.slice(end)

			if (before.endsWith(opening) && after.startsWith(closing)) {
				replaceRange(
					start - opening.length,
					end + closing.length,
					selected,
					start - opening.length,
					end - opening.length
				)
				return
			}

			if (start === end) {
				replaceRange(start, end, `${opening}${closing}`, start + opening.length)
				return
			}

			replaceRange(
				start,
				end,
				`${opening}${selected}${closing}`,
				start + opening.length,
				end + opening.length
			)
		})
	}

	async function toggleLinePrefix(prefix: string) {
		await withTextarea((input) => {
			const start = input.selectionStart
			const end = input.selectionEnd
			const lineStart = value.lastIndexOf('\n', start - 1) + 1
			const lineEnd = value.indexOf('\n', end)
			const rangeEnd = lineEnd === -1 ? value.length : lineEnd
			const lines = value.slice(lineStart, rangeEnd).split('\n')
			const shouldRemove = lines.every((line) => line.startsWith(prefix))
			const replacement = lines
				.map((line) => (shouldRemove ? line.slice(prefix.length) : `${prefix}${line}`))
				.join('\n')

			replaceRange(lineStart, rangeEnd, replacement, lineStart, lineStart + replacement.length)
		})
	}

	async function toggleOrderedList() {
		await withTextarea((input) => {
			const start = input.selectionStart
			const end = input.selectionEnd
			const lineStart = value.lastIndexOf('\n', start - 1) + 1
			const lineEnd = value.indexOf('\n', end)
			const rangeEnd = lineEnd === -1 ? value.length : lineEnd
			const lines = value.slice(lineStart, rangeEnd).split('\n')
			const shouldRemove = lines.every((line) => /^\d+\. /.test(line))
			const replacement = lines
				.map((line, index) =>
					shouldRemove ? line.replace(/^\d+\. /, '') : `${index + 1}. ${line}`
				)
				.join('\n')

			replaceRange(lineStart, rangeEnd, replacement, lineStart, lineStart + replacement.length)
		})
	}

	async function insertLink() {
		await withTextarea((input) => {
			const start = input.selectionStart
			const end = input.selectionEnd
			const selected = value.slice(start, end) || 'текст ссылки'
			const url = 'https://'
			const replacement = `[${selected}](${url})`
			const urlStart = start + selected.length + 3

			replaceRange(start, end, replacement, urlStart, urlStart + url.length)
		})
	}

	function preserveSelection(event: MouseEvent) {
		event.preventDefault()
	}

	const toolbarGroups: { label: string; tools: ToolbarTool[] }[] = [
		{
			label: 'Форматирование текста',
			tools: [
				{ label: 'Подзаголовок', icon: Heading2Icon, action: () => toggleLinePrefix('## ') },
				{ label: 'Жирный', icon: BoldIcon, action: () => surroundSelection('**') },
				{ label: 'Курсив', icon: ItalicIcon, action: () => surroundSelection('*') },
				{ label: 'Зачёркнутый', icon: StrikethroughIcon, action: () => surroundSelection('~~') }
			]
		},
		{
			label: 'Блочные элементы',
			tools: [
				{ label: 'Цитата', icon: TextQuoteIcon, action: () => toggleLinePrefix('> ') },
				{ label: 'Код', icon: CodeXmlIcon, action: () => surroundSelection('`') },
				{ label: 'Ссылка', icon: LinkIcon, action: insertLink }
			]
		},
		{
			label: 'Списки',
			tools: [
				{ label: 'Маркированный список', icon: ListIcon, action: () => toggleLinePrefix('- ') },
				{ label: 'Нумерованный список', icon: ListOrderedIcon, action: toggleOrderedList },
				{ label: 'Список задач', icon: ListChecksIcon, action: () => toggleLinePrefix('- [ ] ') }
			]
		}
	]
</script>

{#snippet ToolbarButton(tool: ToolbarTool)}
	{@const Icon = tool.icon}
	<Tooltip.Root>
		<Tooltip.Trigger>
			{#snippet child({ props })}
				<Button
					{...props}
					aria-label={tool.label}
					size="icon-sm"
					variant="outline"
					onclick={() => tool.action()}
					onmousedown={preserveSelection}
				>
					<Icon />
				</Button>
			{/snippet}
		</Tooltip.Trigger>
		<Tooltip.Content>{tool.label}</Tooltip.Content>
	</Tooltip.Root>
{/snippet}

<Tabs.Root bind:value={selectedTab} aria-label="Режим редактора">
	<Card.Root class="gap-0 py-0" data-invalid={isInvalid ? 'true' : undefined} size="sm">
		<Card.Header class="flex flex-col gap-2 px-3 py-3">
			<Card.Title class="sr-only">Редактор сообщения</Card.Title>
			<div class="flex min-w-0 flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
				<Tabs.List>
					<Tabs.Trigger value="write">Написать</Tabs.Trigger>
					<Tabs.Trigger value="preview">Предпросмотр</Tabs.Trigger>
				</Tabs.List>
				<div
					class="flex w-full min-w-0 justify-end-safe gap-2 overflow-x-auto pb-1 sm:ml-auto sm:w-auto sm:pb-0"
				>
					{#each toolbarGroups as group (group.label)}
						<ButtonGroup.Root aria-label={group.label} class="shrink-0">
							{#each group.tools as tool (tool.label)}
								{@render ToolbarButton(tool)}
							{/each}
						</ButtonGroup.Root>
					{/each}
				</div>
			</div>
		</Card.Header>
		<Card.Content class="px-3 pb-3">
			<Tabs.Content value="write">
				<Textarea
					{...textarea}
					bind:ref={textareaElement}
					bind:value
					class="field-sizing-fixed max-h-96 min-h-64 resize-y text-base leading-7 md:text-base"
					{placeholder}
				/>
			</Tabs.Content>
			<Tabs.Content value="preview">
				{#if isPreviewEmpty}
					<Empty.Root class="h-64 p-6">
						<Empty.Header>
							<Empty.Description>Предпросмотр сообщения появится здесь.</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				{:else}
					<ScrollArea.Root class="h-64 overflow-hidden rounded-md border">
						<PostMarkdown
							class="min-h-full px-2.5 py-2 text-base leading-7 md:text-base [&>:first-child>:first-child]:mt-0 [&>:first-child>:last-child]:mb-0"
							html={carta.renderSSR(value)}
						/>
					</ScrollArea.Root>
				{/if}
			</Tabs.Content>
		</Card.Content>
		{#if footer}
			<Card.Footer
				class="flex-col items-stretch gap-3 border-t px-3 py-3 [--card-spacing:--spacing(3)] sm:flex-row sm:items-center"
			>
				{@render footer()}
			</Card.Footer>
		{/if}
	</Card.Root>
</Tabs.Root>
