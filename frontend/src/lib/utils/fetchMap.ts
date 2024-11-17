import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

export class FetchMap<K, V> extends SvelteMap<K, V | null> {
  private loader: DataLoader<K, V>

  constructor(batchFn: DataLoader.BatchLoadFn<K, V>) {
    super()
    // Инициализация DataLoader с переданной BatchFn
    this.loader = new DataLoader(batchFn)
  }

  // Переопределяем метод get
  get(key: K): V | null | undefined {
    const value = super.get(key)
    // Добавляем дополнительные действия, например, возвращаем значение по умолчанию
    if (value === undefined) {
      this.loader.load(key).then((v) => this.set(key, v ?? null))
    }
    return value
  }
}
