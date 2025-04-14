import { defaultPlugins } from '@hey-api/openapi-ts'

export default {
	input: 'http://localhost:8000/swagger/gateway/swagger.json',
	output: 'src/lib/utils/client',
	experimentalParser: true,
	plugins: [
		...defaultPlugins,
		{
			baseUrl: false, // [!code ++]
			name: '@hey-api/client-fetch'
		},
		'@hey-api/schemas',
		{
			dates: true,
			name: '@hey-api/transformers'
		},
		{
			exportInlineEnums: true,
			name: '@hey-api/typescript'
		},
		{
			name: '@hey-api/sdk',
			transformer: true
		},
		'zod'
	]
}
