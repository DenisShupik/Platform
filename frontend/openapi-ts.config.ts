import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  client: '@hey-api/client-fetch',
  input: 'https://localhost:8010/swagger/v1/swagger.json',
  output: 'src/lib/utils/client'
})
