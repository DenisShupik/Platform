/**
 * `Object.entries` erases mapped and branded key types to `string`.
 *
 * API response records have already been validated by the generated client, so this helper only
 * retains their declared key type; it performs no runtime conversion or validation.
 */
type Entry<T extends object> = {
	[K in Extract<keyof T, string>]: [K, T[K]]
}[Extract<keyof T, string>]

export const typedEntries = <T extends object>(record: T): Entry<T>[] =>
	Object.entries(record) as Entry<T>[]
