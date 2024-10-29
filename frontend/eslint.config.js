import ts from "typescript-eslint";
import svelte from "eslint-plugin-svelte";
import svelte_config from "./svelte.config.js";

/** @type {import("eslint").Linter.Config[]} */
export default [
  // ts
  ...ts.configs.recommendedTypeChecked,
  ...ts.configs.strictTypeChecked,
  ...ts.configs.stylisticTypeChecked,
  // ts config
  {
    rules: {
      // ...
    },
  },
  {
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
  // other plugins
  // ...
  // svelte
  ...svelte.configs["flat/recommended"],
  // svelte config
  {
    rules: {
      // ...
    },
  },
  {
    files: ["**/*.svelte"],
    languageOptions: {
      parserOptions: {
        extraFileExtensions: [".svelte"],
        parser: ts.parser,
        svelteConfig: svelte_config,
      },
    },
  },
  {
    ignores: ["build/", "dist/"],
  },
];