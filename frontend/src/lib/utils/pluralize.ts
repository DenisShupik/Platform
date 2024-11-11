export function pluralize(count: number, wordForms: [string, string]): string {
  const [singular, plural] = wordForms
  return count === 1 ? singular : plural
}
