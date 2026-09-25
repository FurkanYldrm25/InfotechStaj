// views/auth.js — login ve register ekranları

import { esc, h, toast } from '../utils.js';

export function renderLogin(mount, { auth, router, api }) {
    mount.innerHTML = `
        <div class="auth-shell">
            <div class="auth-card">
                <div class="auth-logo">
                    <div class="logo-mark">S</div>
                    <div>Staj</div>
                </div>
                <div class="auth-title">Tekrar hoş geldin</div>
                <div class="auth-sub">Devam etmek için hesabına giriş yap.</div>

                <div class="form-alert hidden" id="err"></div>

                <form id="login-form">
                    <div class="form-group">
                        <label class="form-label required">E-posta</label>
                        <input class="input" type="email" name="email" required autocomplete="email" placeholder="ornek@staj.local" />
                    </div>
                    <div class="form-group">
                        <label class="form-label required">Şifre</label>
                        <input class="input" type="password" name="password" required autocomplete="current-password" placeholder="••••••••" />
                    </div>
                    <button class="btn btn-primary btn-block btn-lg" type="submit" id="submit-btn">Giriş yap</button>
                </form>

                <div class="auth-footer">
                    Hesabın yok mu? <a href="#/register">Kayıt ol</a>
                </div>
            </div>
        </div>
    `;

    const form = mount.querySelector('#login-form');
    const err = mount.querySelector('#err');
    const btn = mount.querySelector('#submit-btn');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        err.classList.add('hidden');
        btn.disabled = true;
        btn.textContent = 'Giriş yapılıyor...';
        try {
            const fd = new FormData(form);
            const r = await api.login(fd.get('email').trim(), fd.get('password'));
            if (r.success && r.data) {
                auth.setAuth(r.data);
                window.dispatchEvent(new Event('staj-auth-changed'));
                router.go('/dashboard');
            } else {
                err.textContent = r.message || 'Giriş başarısız.';
                err.classList.remove('hidden');
            }
        } catch (ex) {
            err.textContent = ex?.message || 'Giriş başarısız.';
            err.classList.remove('hidden');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Giriş yap';
        }
    });
}

export function renderRegister(mount, { auth, router, api }) {
    mount.innerHTML = `
        <div class="auth-shell">
            <div class="auth-card">
                <div class="auth-logo">
                    <div class="logo-mark">S</div>
                    <div>Staj</div>
                </div>
                <div class="auth-title">Hesap oluştur</div>
                <div class="auth-sub">Platforma katıl, seni tanıyalım.</div>

                <div class="form-alert hidden" id="err"></div>

                <form id="reg-form">
                    <div class="form-group">
                        <label class="form-label required">Ne olarak katılıyorsun?</label>
                        <div class="role-picker">
                            <div class="role-option" data-role="Freelancer">Freelancer<br><span class="text-sm text-muted">İş almak istiyorum</span></div>
                            <div class="role-option" data-role="Client">İşveren<br><span class="text-sm text-muted">İş açmak istiyorum</span></div>
                        </div>
                        <input type="hidden" name="role" required />
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label class="form-label required">Ad</label>
                            <input class="input" name="firstName" required minlength="2" />
                        </div>
                        <div class="form-group">
                            <label class="form-label required">Soyad</label>
                            <input class="input" name="lastName" required minlength="2" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label required">E-posta</label>
                        <input class="input" type="email" name="email" required autocomplete="email" />
                    </div>
                    <div class="form-group">
                        <label class="form-label required">Şifre</label>
                        <input class="input" type="password" name="password" required minlength="6" autocomplete="new-password" />
                        <div class="form-hint">En az 6 karakter.</div>
                    </div>
                    <button class="btn btn-primary btn-block btn-lg" type="submit" id="submit-btn">Kayıt ol</button>
                </form>

                <div class="auth-footer">
                    Zaten hesabın var mı? <a href="#/login">Giriş yap</a>
                </div>
            </div>
        </div>
    `;

    const form = mount.querySelector('#reg-form');
    const err = mount.querySelector('#err');
    const roleInput = form.querySelector('input[name=role]');
    mount.querySelectorAll('.role-option').forEach(opt => {
        opt.addEventListener('click', () => {
            mount.querySelectorAll('.role-option').forEach(o => o.classList.remove('selected'));
            opt.classList.add('selected');
            roleInput.value = opt.dataset.role;
        });
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        err.classList.add('hidden');
        if (!roleInput.value) {
            err.textContent = 'Lütfen bir rol seç.';
            err.classList.remove('hidden');
            return;
        }
        const btn = form.querySelector('#submit-btn');
        btn.disabled = true;
        btn.textContent = 'Kayıt oluşturuluyor...';
        try {
            const fd = new FormData(form);
            const payload = {
                firstName: fd.get('firstName').trim(),
                lastName: fd.get('lastName').trim(),
                email: fd.get('email').trim(),
                password: fd.get('password'),
                role: roleInput.value
            };
            const r = await api.register(payload);
            if (r.success) {
                // Auto-login
                const login = await api.login(payload.email, payload.password);
                if (login.success && login.data) {
                    auth.setAuth(login.data);
                    window.dispatchEvent(new Event('staj-auth-changed'));
                    router.go('/dashboard');
                    toast('Hoş geldin! Hesabın oluşturuldu.', 'success');
                } else {
                    router.go('/login');
                }
            } else {
                err.innerHTML = (r.message || 'Kayıt başarısız.') + (r.errors?.length ? '<br>' + r.errors.map(esc).join('<br>') : '');
                err.classList.remove('hidden');
            }
        } catch (ex) {
            err.innerHTML = (ex?.message || 'Kayıt başarısız.') + (ex?.errors?.length ? '<br>' + ex.errors.map(esc).join('<br>') : '');
            err.classList.remove('hidden');
        } finally {
            const btn = form.querySelector('#submit-btn');
            if (btn) { btn.disabled = false; btn.textContent = 'Kayıt ol'; }
        }
    });
}