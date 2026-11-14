import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  resolve: {
    alias: {
      '@proxy': fileURLToPath(new URL('./src/app/proxy', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
  },
});
