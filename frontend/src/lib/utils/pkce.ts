const charset =
  'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~'
const charsetLength = charset.length

export function base64UrlEncode(array: ArrayBuffer): string {
  return window
    .btoa(
      String.fromCharCode.apply(
        null,
        new Uint8Array(array) as unknown as number[]
      )
    )
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '')
}

export function randomString(length: number): string {
  const bytes = new Uint8Array(length)
  window.crypto.getRandomValues(bytes)
  let result = ''
  for (let i = 0; i < length; ++i) {
    result += charset[bytes[i] % charsetLength]
  }
  return result
}
