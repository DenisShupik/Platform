export function convertToWebp(file: File): Promise<Blob> {
  return new Promise((resolve, reject) => {
    const img = new Image()
    const url = URL.createObjectURL(file)

    img.onload = () => {
      const canvas = document.createElement('canvas')
      canvas.width = img.width
      canvas.height = img.height

      const ctx = canvas.getContext('2d')
      if (ctx) {
        ctx.drawImage(img, 0, 0)
        canvas.toBlob(
          (blob) => {
            if (blob) {
              resolve(blob)
            } else {
              reject(new Error('Ошибка при создании Blob'))
            }
          },
          'image/webp',
          0.8
        )
      } else {
        reject(new Error('Ошибка получения контекста канваса'))
      }

      URL.revokeObjectURL(url)
    }

    img.onerror = () => {
      reject(new Error('Ошибка загрузки изображения'))
    }

    img.src = url
  })
}
