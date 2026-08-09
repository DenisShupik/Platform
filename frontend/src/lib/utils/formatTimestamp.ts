export function formatTimestamp(date: Date): string {
	return date.toLocaleString('en-US', {
		hour: '2-digit',
		minute: '2-digit',
		second: '2-digit',
		day: '2-digit',
		month: '2-digit',
		year: 'numeric'
	})
}

export function formatDate(date: Date): string {
	return date.toLocaleDateString('en-US', {
		day: '2-digit',
		month: '2-digit',
		year: 'numeric'
	})
}
