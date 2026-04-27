<script lang="ts">
	import { resolve } from '$app/paths'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import type { CategoryDto } from '$lib/utils/client'

	let {
		category
	}: {
		category: Pick<CategoryDto, 'categoryId' | 'title'>
	} = $props()
</script>

<Breadcrumb.Item>
	<a
		href={resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId: category.categoryId })}
		>{category.title}</a
	>
</Breadcrumb.Item>
