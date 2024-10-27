import sveltePlugin from 'eslint-plugin-svelte';
import svelteParser from "svelte-eslint-parser";
import tseslint from "typescript-eslint";
import svelteConfig from './svelte.config.js';
export default [
  ...tseslint.configs.recommended,
  ...sveltePlugin.configs['flat/recommended'],
  {
    rules: {
      // override/add rules settings here, such as:
      // 'svelte/rule-name': 'error'
    }
  },
  {
    files: ["**/*.svelte", "*.svelte"],
    languageOptions: {
      parser: svelteParser,
      parserOptions: {
        parser: {
          ts: "@typescript-eslint/parser",
        },
        extraFileExtensions: [".svelte"],
      },
    }
  },
];