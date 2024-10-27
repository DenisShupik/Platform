import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';
import path from 'path';
import fs from 'fs';

export default defineConfig({
    server: {
        https: {
            key: fs.readFileSync('../service.key'),
            cert: fs.readFileSync('../service.crt'),
        },
    },
    plugins: [
        svelte({
            /* plugin options */
        }),
    ],
    resolve: {
        alias: {
            $lib: path.resolve("./src/lib"),
        },
    },
});