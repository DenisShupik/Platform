import { defineConfig, type Plugins } from '@hey-api/openapi-ts'

const valueObjectExtension = 'x-value-object'

type TypeScriptResolvers = NonNullable<
	Plugins.HeyApiTypeScript.Types['Types']['config']['$resolvers']
>
type StringResolver = NonNullable<TypeScriptResolvers['string']>
type NumberResolver = NonNullable<TypeScriptResolvers['number']>
type ValibotStringResolver = Plugins.Valibot.Resolvers['string']
type ValibotObjectResolver = Plugins.Valibot.Resolvers['object']

const brandedString: StringResolver = (context) => {
	const brand = context.schema[valueObjectExtension]
	if (typeof brand === 'string') {
		return context.$.type.and(
			context.nodes.base(context),
			context.$.type
				.object()
				.prop('__brand', (property) => property.readonly().type(context.$.type.literal(brand)))
		)
	}

	if (context.schema.format === 'date' || context.schema.format === 'date-time') {
		return context.$.type('Date')
	}
}

const brandedNumber: NumberResolver = (context) => {
	const brand = context.schema[valueObjectExtension]
	if (typeof brand === 'string') {
		return context.$.type.and(
			context.nodes.base(context),
			context.$.type
				.object()
				.prop('__brand', (property) => property.readonly().type(context.$.type.literal(brand)))
		)
	}

	if (context.schema.type === 'integer' && context.schema.format === 'int64') {
		return context.$.type('bigint')
	}
}

const dateString: ValibotStringResolver = (context) => {
	if (context.schema.format !== 'date' && context.schema.format !== 'date-time') return

	context.pipes.push(context.pipes.current, context.nodes.base(context))
	context.pipes.push(context.pipes.current, context.nodes.format(context))
	context.pipes.push(
		context.pipes.current,
		context
			.$(context.plugin.imports.v)
			.attr('transform')
			.call(context.$.func().param('value').do(context.$.new('Date').arg('value').return()))
	)

	return context.pipes.current
}

const dictionaryObject: ValibotObjectResolver = (context) => {
	const valueSchema = context.schema.additionalProperties
	if (!valueSchema || typeof valueSchema === 'boolean' || valueSchema.type) return

	const valueResult = context.walk(valueSchema, {
		path: context.path,
		plugin: context.plugin
	})
	context._childResults.push(valueResult)

	let keyNode = context.$(context.plugin.imports.v).attr('string').call()
	const keySchema = context.schema.propertyNames
	if (keySchema && typeof keySchema !== 'boolean') {
		const keyResult = context.walk(keySchema, {
			path: context.path,
			plugin: context.plugin
		})
		context._childResults.push(keyResult)
		keyNode = context.pipes.toNode(context.applyModifiers(keyResult).pipes, context.plugin)
	}

	const valueNode = context.pipes.toNode(context.applyModifiers(valueResult).pipes, context.plugin)

	return context.$(context.plugin.imports.v).attr('record').call(keyNode, valueNode)
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
			enums: 'typescript',
			name: '@hey-api/typescript',
			$resolvers: {
				number: brandedNumber,
				string: brandedString
			}
		},
		{
			name: '@hey-api/sdk',
			transformer: 'valibot'
		},
		{
			name: 'valibot',
			$resolvers: {
				object: dictionaryObject,
				string: dateString
			}
		}
	]
})
