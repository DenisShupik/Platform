import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  client: '@hey-api/client-fetch',
  input: 'https://localhost:8000/swagger/YARP/swagger.json',
  output: 'src/lib/utils/client'
})
