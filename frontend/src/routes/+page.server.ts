import { getForums, getForumsCount } from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url }) => {
	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10
	return {
		forumsCount: (await getForumsCount<true>()).data,
		forums: (
			await getForums<true>({
				query: {
					cursor: (currentPage - 1n) * BigInt(perPage),
					limit: perPage,
					sort: '-latestPost'
				}
			})
		).data
	}
}
