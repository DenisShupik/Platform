export function getPageFromUrl(url: URL): number {
  const parsed = parseInt(url.searchParams.get('page'))
  return !isNaN(parsed) ? parsed : 1
}
