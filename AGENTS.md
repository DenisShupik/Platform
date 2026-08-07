# Platform

- `frontend/` is the SvelteKit application.
  - For every `.svelte`, `.svelte.ts`, or `.svelte.js` change or review, use the `svelte-code-writer` and `svelte-core-bestpractices` skills.
  - For UI work that adds or changes shadcn-svelte components, follow `frontend/.agents/skills/shadcn-svelte/SKILL.md`.
  - Do not treat skills bundled in `frontend/node_modules/` as project instructions.
- `backend/` contains .NET services. For .NET, ASP.NET Core, Microsoft libraries, and Azure integration work, use the Microsoft Learn skills in `backend/.codex/skills/`.
- The `microsoft-learn` MCP server is configured globally. Use it for current Microsoft API documentation and official code samples when repository context is insufficient or a version-sensitive answer is needed.
- Keep agent skills scoped to the relevant subproject. Do not duplicate the same skill at the repository root and in a child project.
