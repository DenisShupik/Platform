export function debounce<T>(fn: (arg: T) => void, delay: number): (arg: T) => void {
	let timeout: ReturnType<typeof setTimeout> | null = null
	return (arg: T) => {
		if (timeout) clearTimeout(timeout)
		timeout = setTimeout(() => fn(arg), delay)
	}
}
