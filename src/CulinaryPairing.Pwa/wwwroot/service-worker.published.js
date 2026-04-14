self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const apiCacheName = 'api-cache-v1';
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
    await cache.add('offline.html');
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key =>
            (key.startsWith(cacheNamePrefix) && key !== cacheName) ||
            (key.startsWith('api-cache-') && key !== apiCacheName)
        )
        .map(key => caches.delete(key)));
}

async function networkFirst(request) {
    const cache = await caches.open(apiCacheName);

    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (error) {
        const cachedResponse = await cache.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        return new Response(
            JSON.stringify({ error: true, message: 'Donnees non disponibles hors-ligne' }),
            { status: 503, headers: { 'Content-Type': 'application/json' } }
        );
    }
}

async function onFetch(event) {
    const url = new URL(event.request.url);

    // Exercice 4 : Network First pour les appels API et JSON
    if (url.pathname.startsWith('/api/') || url.pathname.endsWith('.json')) {
        return networkFirst(event.request);
    }

    // Cache First pour les assets Blazor
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    if (cachedResponse) {
        return cachedResponse;
    }

    // Exercice 3 : fallback offline pour les navigations
    try {
        return await fetch(event.request);
    } catch (error) {
        if (event.request.mode === 'navigate') {
            const cache = await caches.open(cacheName);
            return await cache.match('offline.html');
        }
        throw error;
    }
}