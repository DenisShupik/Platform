export function preventDefault<T extends Event, U>(fn: (this: U, event: T) => void) {
	return function (this: U, event: T): void {
		event.preventDefault()
		fn.call(this, event)
	}
}
