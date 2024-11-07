import ts from 'typescript-eslint'
import svelte from 'eslint-plugin-svelte'
import svelte_config from './svelte.config.js'
import simpleImportSort from 'eslint-plugin-simple-import-sort'

/** @type {import("eslint").Linter.Config[]} */
export default [
  //importPlugin.flatConfigs.recommended,
  // ts
  ...ts.configs.recommendedTypeChecked,
  ...ts.configs.strictTypeChecked,
  ...ts.configs.stylisticTypeChecked,
  // ts config
  {
    plugins: {
      'simple-import-sort': simpleImportSort
    },
    rules: {
      'simple-import-sort/imports': 'error',
      'simple-import-sort/exports': 'error'
    }
  },
  {
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname
      }
    }
  },
  // other plugins
  // ...
  // svelte
  ...svelte.configs['flat/recommended'],
  // svelte config
  {
    rules: {
      // ...
    }
  },
  {
    files: ['**/*.svelte'],
    languageOptions: {
      parserOptions: {
        extraFileExtensions: ['.svelte'],
        parser: ts.parser,
        svelteConfig: svelte_config
      }
    }
  },
  {
    ignores: ['build/', 'dist/']
  }
]
