export function getDeclension(
  count: number,
  forms: [string, string, string]
): string {
  const mod10 = count % 10
  const mod100 = count % 100

  if (mod10 === 1 && mod100 !== 11) {
    return forms[0]
  } else if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) {
    return forms[1]
  } else {
    return forms[2]
  }
}
