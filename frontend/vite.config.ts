import { defineConfig } from 'vitest/config'
import { paraglideVitePlugin } from '@inlang/paraglide-js'
import devtoolsJson from 'vite-plugin-devtools-json'
import tailwindcss from '@tailwindcss/vite'
import { sveltekit } from '@sveltejs/kit/vite'
import adapter from '@sveltejs/adapter-node'
import Icons from 'unplugin-icons/vite'

export default defineConfig({
	define: { __ENABLE_CARTA_SSR_HIGHLIGHTER__: false },
	plugins: [
		tailwindcss(),
		sveltekit({
			adapter: adapter(),
			alias: { '@/*': './path/to/lib/*' },
			experimental: { remoteFunctions: true },
			compilerOptions: {
				experimental: { async: true },
				runes: ({ filename }) =>
					filename.split(/[/\\]/).includes('node_modules') ? undefined : true
			}
		}),
		Icons({ compiler: 'svelte' }),
		devtoolsJson(),
		paraglideVitePlugin({
			project: './project.inlang',
			outdir: './src/lib/paraglide',
			emitTsDeclarations: true,
			strategy: ['cookie', 'preferredLanguage', 'baseLocale'],
			routeStrategies: [{ match: '/api/auth/:path(.*)?', exclude: true }]
		})
	],
	server: { allowedHosts: ['forum-node.ru'] },
	build: { target: 'esnext' },
	test: {
		expect: { requireAssertions: true },
		projects: [
			{
				extends: './vite.config.ts',
				test: {
					name: 'server',
					environment: 'node',
					include: ['src/**/*.{test,spec}.{js,ts}'],
					exclude: ['src/**/*.svelte.{test,spec}.{js,ts}']
				}
			}
		]
	}
})
