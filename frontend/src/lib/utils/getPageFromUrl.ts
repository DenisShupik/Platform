export function getPageFromUrl(url: URL): number {
	const raw = url.searchParams.get('page');
	if (!raw) return 1;

	const parsed = Number.parseInt(raw, 10);
	if (Number.isNaN(parsed) || parsed < 1) return 1;

	return parsed;
}