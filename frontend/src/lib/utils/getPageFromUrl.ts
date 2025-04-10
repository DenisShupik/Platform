export function getPageFromUrl(url: URL): bigint {
	try {
		const parsed = BigInt(url.searchParams.get('page'))
		return parsed > 0n ? parsed : 1n
	} catch {
		return 1n
	}
}
