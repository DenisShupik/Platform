import tsParser from '@typescript-eslint/parser'
import svelte from 'eslint-plugin-svelte'
import { defineConfig, globalIgnores } from 'eslint/config'

// Oxlint owns JavaScript/TypeScript linting. ESLint remains only for the
// Svelte template AST, which Oxlint does not support yet.
export default defineConfig(
	globalIgnores([
		// Generated or overwritten by the shadcn-svelte CLI.
		'src/lib/components/ui/**',
		'src/lib/hooks/is-mobile.svelte.ts'
	]),
	svelte.configs.recommended,
	svelte.configs.prettier,
	{
		files: ['**/*.svelte', '**/*.svelte.ts', '**/*.svelte.js'],

		languageOptions: {
			parserOptions: {
				extraFileExtensions: ['.svelte'],
				parser: tsParser
			}
		}
	}
)
