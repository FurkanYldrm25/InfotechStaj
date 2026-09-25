// router.js — basit hash router (#/path/segment?query)

const routes = [];
let currentUnmount = null;

// register: pattern → 'jobs' | 'jobs/:id' | 'admin/categories'
export function register(pattern, handler, { auth = false, roles = null } = {}) {
    const parts = pattern.split('/').filter(Boolean);
    const paramNames = [];
    const regex = new RegExp('^#/' + parts.map(p => {
        if (p.startsWith(':')) { paramNames.push(p.slice(1)); return '([^/?]+)'; }
        return p.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }).join('/') + '/?(\\?.*)?$');
    routes.push({ pattern, regex, paramNames, handler, auth, roles });
}

export function go(path) {
    if (!path.startsWith('#/')) path = '#/' + path.replace(/^#?\/?/, '');
    if (location.hash === path) resolve();
    else location.hash = path;
}

export function currentPath() { return location.hash || '#/'; }

export function parseQuery() {
    const h = location.hash;
    const q = h.indexOf('?');
    if (q < 0) return {};
    const p = new URLSearchParams(h.slice(q + 1));
    const out = {};
    for (const [k, v] of p.entries()) out[k] = v;
    return out;
}

let authGate = null;
export function setAuthGate(fn) { authGate = fn; }

export async function resolve() {
    let hash = location.hash || '#/';
    if (hash === '#/' || hash === '#') hash = '#/dashboard';

    // Params
    for (const r of routes) {
        const m = hash.match(r.regex);
        if (!m) continue;
        const params = {};
        r.paramNames.forEach((n, i) => params[n] = decodeURIComponent(m[i + 1]));

        // Auth check
        if (authGate) {
            const gate = authGate(r);
            if (gate && gate.redirect) { location.hash = gate.redirect; return; }
        }

        // Unmount previous
        if (currentUnmount) { try { currentUnmount(); } catch { } currentUnmount = null; }

        try {
            const result = await r.handler({ params, query: parseQuery() });
            if (typeof result === 'function') currentUnmount = result;
        } catch (err) {
            console.error('Route hatası:', err);
        }
        return;
    }

    // Not found
    if (currentUnmount) { try { currentUnmount(); } catch { } currentUnmount = null; }
    document.getElementById('app').innerHTML = `
        <div class="auth-shell">
            <div class="card text-center" style="max-width:400px">
                <h2 class="mb-2">404</h2>
                <p class="text-muted mb-4">Aradığınız sayfa bulunamadı.</p>
                <a class="btn btn-primary" href="#/dashboard">Ana sayfaya dön</a>
            </div>
        </div>
    `;
}

export function start() {
    window.addEventListener('hashchange', resolve);
    resolve();
}