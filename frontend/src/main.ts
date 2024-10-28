import { mount } from 'svelte'
import App from './App.svelte'
import { initRouter } from '$lib/routeStore';

initRouter();

const app = mount(App, {
  target: document.getElementById('app')!,
})

export default app
