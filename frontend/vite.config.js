import { defineConfig } from 'vite'
import { sveltekit } from '@sveltejs/kit/vite'
import path from 'path'
import fs from 'fs'

export default defineConfig({
  server: {
    https: {
      key: fs.readFileSync('../service.key'),
      cert: fs.readFileSync('../service.crt')
    }
  },
  plugins: [sveltekit()],
  resolve: {
    alias: {
      $lib: path.resolve('./src/lib')
    }
  },
  build: {
    target: 'esnext'
  }
})
