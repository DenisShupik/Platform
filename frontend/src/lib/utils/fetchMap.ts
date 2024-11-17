import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

export class FetchMap<K, V> extends SvelteMap<K, V> {
  private loader: DataLoader<K, V>

  constructor(batchFn: DataLoader.BatchLoadFn<K, V>) {
    super()
    this.loader = new DataLoader(batchFn, { maxBatchSize: 100, cache: false })
  }

  get(key: K): V | undefined {
    const value = super.get(key)
    if (value === undefined) {
      this.loader
        .load(key)
        .then((v) => this.set(key, v))
        .catch((e: unknown) => {
          console.error(e)
        })
    }
    return value
  }
}
