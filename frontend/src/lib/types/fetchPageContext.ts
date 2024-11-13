export type FetchPageContext =
  | {
      abortController: AbortController
      pageId: number
    }
  | undefined
