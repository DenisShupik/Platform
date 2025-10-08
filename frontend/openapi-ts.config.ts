export default {
	input: 'http://localhost:8000/api/openapi.json',
	output: 'src/lib/utils/client',
	experimentalParser: true,
	plugins: [
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
			enums: 'typescript',
			name: '@hey-api/typescript'
		},
		{
			name: '@hey-api/sdk',
			transformer: true
		},
		'valibot'
	]
}
