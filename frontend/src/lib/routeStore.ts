// router.ts
import { writable, type Writable } from 'svelte/store';

export const enum RouteKey {
  Sections,
  Category
}

interface Route {
  key: RouteKey;
  path: string;
  params?: Record<string, string>;
}

interface RoutePattern {
  key: RouteKey;
  pattern: RegExp;
  paramKeys?: string[];
}

const routes: RoutePattern[] = [];

export function addRoute(key: RouteKey, pathPattern: string) {
  const paramKeys: string[] = [];
  const pattern = new RegExp(
    `^${pathPattern.replace(/:([a-zA-Z0-9_]+)/g, (_, key) => {
      paramKeys.push(key);
      return "([^/]+)";
    })}$`
  );
  routes.push({ key, pattern, paramKeys });
}

addRoute(RouteKey.Sections, '/')
addRoute(RouteKey.Category, '/categories/:categoryId')

export const route: Writable<Route> = writable();

export function navigate(path: string): void {
  window.history.pushState({}, "", path);
  matchRoute(path);
}

export function initRouter(): () => void {
  const handlePopstate = () => matchRoute(window.location.pathname);
  window.addEventListener('popstate', handlePopstate);

  matchRoute(window.location.pathname);

  return () => window.removeEventListener('popstate', handlePopstate);
}

function matchRoute(path: string) {
  for (const { key, pattern, paramKeys } of routes) {
    const match = path.match(pattern);
    if (match) {
      const paramsObj: Record<string, string> = {};
      paramKeys?.forEach((key, index) => {
        paramsObj[key] = match[index + 1];
      });
      route.set({ key, path, params: paramsObj });
      return;
    }
  }
  route.set({ key: RouteKey.Category, path: "/" });
}
