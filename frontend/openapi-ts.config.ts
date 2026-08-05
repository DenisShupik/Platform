import { defineConfig, type Plugins } from '@hey-api/openapi-ts'

const valueObjectExtension = 'x-value-object'

type TypeScriptResolvers = NonNullable<
	Plugins.HeyApiTypeScript.Types['Types']['config']['$resolvers']
>
type StringResolver = NonNullable<TypeScriptResolvers['string']>
type NumberResolver = NonNullable<TypeScriptResolvers['number']>

const brandedString: StringResolver = (context) => {
	const brand = context.schema[valueObjectExtension]
	if (typeof brand !== 'string') return

	return context.$.type.and(
		context.nodes.base(context),
		context.$.type
			.object()
			.prop('__brand', (property) => property.readonly().type(context.$.type.literal(brand)))
	)
}

const brandedNumber: NumberResolver = (context) => {
	const brand = context.schema[valueObjectExtension]
	if (typeof brand !== 'string') return

	return context.$.type.and(
		context.nodes.base(context),
		context.$.type
			.object()
			.prop('__brand', (property) => property.readonly().type(context.$.type.literal(brand)))
	)
}

export default defineConfig({
	input: 'http://localhost:8000/api/openapi.json',
	output: 'src/lib/utils/client',
	plugins: [
		{
			baseUrl: false, // [!code ++]
			name: '@hey-api/client-fetch'
		},
		'@hey-api/schemas',
		{
			dates: true,
			bigInt: true,
			name: '@hey-api/transformers'
		},
		{
			enums: 'typescript',
			name: '@hey-api/typescript',
			$resolvers: {
				number: brandedNumber,
				string: brandedString
			}
		},
		{
			name: '@hey-api/sdk',
			transformer: true
		},
		'valibot'
	]
})
