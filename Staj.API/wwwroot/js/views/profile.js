// views/profile.js — kendi profilim (temel bilgi + freelancer/client + portfolio)

import {
    esc, initials, fullName, toast, openModal, confirmModal, icons,
    ContactPreference, ContactPreferenceOptions
} from '../utils.js';

export async function renderMyProfile(ctx) {
    const { renderView, api, auth } = ctx;
    const user = auth.getUser();
    const isFreelancer = auth.isFreelancer();
    const isClient = auth.isClient();

    renderView('Profilim', `
        <div class="page-header">
            <div>
                <h1>Profilim</h1>
                <div class="page-sub">Hesap bilgilerin ve rol profilleriniz.</div>
            </div>
        </div>
        <div id="profile-tabs" class="tabs">
            <button class="tab active" data-t="basic">Temel bilgiler</button>
            ${isFreelancer ? '<button class="tab" data-t="freelancer">Freelancer profili</button>' : ''}
            ${isFreelancer ? '<button class="tab" data-t="portfolio">Portfolio</button>' : ''}
            ${isFreelancer ? '<button class="tab" data-t="skills">Yetkinliklerim</button>' : ''}
            ${isClient ? '<button class="tab" data-t="client">İşveren profili</button>' : ''}
        </div>
        <div id="profile-body"></div>
    `);

    const tabs = document.querySelectorAll('#profile-tabs .tab');
    const body = document.getElementById('profile-body');

    async function loadTab(t) {
        tabs.forEach(x => x.classList.toggle('active', x.dataset.t === t));
        body.innerHTML = `<div class="loading-box"><div class="spinner"></div></div>`;
        try {
            if (t === 'basic') await renderBasic(body, ctx);
            else if (t === 'freelancer') await renderFreelancerTab(body, ctx);
            else if (t === 'portfolio') await renderPortfolioTab(body, ctx);
            else if (t === 'skills') await renderSkillsTab(body, ctx);
            else if (t === 'client') await renderClientTab(body, ctx);
        } catch (e) {
            body.innerHTML = `<div class="card text-danger">${esc(e?.message || 'Yüklenemedi')}</div>`;
        }
    }
    tabs.forEach(x => x.addEventListener('click', () => loadTab(x.dataset.t)));
    loadTab('basic');
}

// ---------- Temel bilgiler ----------
async function renderBasic(body, { api, auth }) {
    const me = (await api.me()).data;
    body.innerHTML = `
        <div class="card" style="max-width:640px">
            <div class="flex items-center gap-4 mb-4">
                <div class="avatar avatar-xl">${esc(initials(me.firstName, me.lastName))}</div>
                <div>
                    <div class="font-semibold text-lg">${esc(fullName(me.firstName, me.lastName))}</div>
                    <div class="text-muted">${esc(me.email)}</div>
                    <div class="mt-2 flex gap-1">
                        ${(me.roles || []).map(r => `<span class="badge badge-primary">${esc(r)}</span>`).join('')}
                    </div>
                </div>
            </div>
            <form id="basic-form">
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label required">Ad</label>
                        <input class="input" name="firstName" required value="${esc(me.firstName || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label required">Soyad</label>
                        <input class="input" name="lastName" required value="${esc(me.lastName || '')}" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="form-label">Profil fotoğrafı URL'i</label>
                    <input class="input" name="imageUrl" value="${esc(me.profileImageUrl || '')}" placeholder="https://..." />
                </div>
                <div class="flex justify-end">
                    <button class="btn btn-primary" type="submit">Kaydet</button>
                </div>
            </form>
        </div>
    `;
    body.querySelector('#basic-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData(e.target);
        try {
            await api.updateBasicInfo(fd.get('firstName').trim(), fd.get('lastName').trim());
            if (fd.get('imageUrl') !== me.profileImageUrl) {
                await api.updateProfileImage(fd.get('imageUrl') || null);
            }
            // Auth cache güncelle
            const a = auth.getAuth();
            if (a) { a.firstName = fd.get('firstName'); a.lastName = fd.get('lastName'); auth.setAuth(a); }
            toast('Bilgilerin kaydedildi.', 'success');
        } catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    });
}

// ---------- Freelancer profili ----------
async function renderFreelancerTab(body, { api }) {
    let f = null;
    try { f = (await api.getMyFreelancer()).data; } catch { f = null; }
    body.innerHTML = `
        <div class="card" style="max-width:820px">
            <form id="fl-form">
                <div class="form-group">
                    <label class="form-label">Unvan</label>
                    <input class="input" name="title" maxlength="150" value="${esc(f?.title || '')}" placeholder="Örn: Kıdemli Backend Geliştirici" />
                </div>
                <div class="form-group">
                    <label class="form-label">Biyografi</label>
                    <textarea class="textarea" name="bio" rows="5" maxlength="2000">${esc(f?.bio || '')}</textarea>
                </div>
                <div class="form-row-3">
                    <div class="form-group">
                        <label class="form-label">Deneyim (yıl)</label>
                        <input class="input" name="experienceYears" type="number" min="0" max="60" value="${f?.experienceYears ?? ''}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Min ücret</label>
                        <input class="input" name="hourlyRateMin" type="number" step="0.01" value="${f?.hourlyRateMin ?? ''}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Max ücret</label>
                        <input class="input" name="hourlyRateMax" type="number" step="0.01" value="${f?.hourlyRateMax ?? ''}" />
                    </div>
                </div>
                <div class="form-row-3">
                    <div class="form-group">
                        <label class="form-label">Para birimi</label>
                        <input class="input" name="currency" maxlength="3" value="${esc(f?.currency || 'TRY')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Ülke</label>
                        <input class="input" name="country" value="${esc(f?.country || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Şehir</label>
                        <input class="input" name="city" value="${esc(f?.city || '')}" />
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">LinkedIn URL</label>
                        <input class="input" name="linkedInUrl" type="url" value="${esc(f?.linkedInUrl || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">GitHub URL</label>
                        <input class="input" name="gitHubUrl" type="url" value="${esc(f?.gitHubUrl || '')}" />
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Website URL</label>
                        <input class="input" name="websiteUrl" type="url" value="${esc(f?.websiteUrl || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">CV URL</label>
                        <input class="input" name="cvUrl" type="url" value="${esc(f?.cvUrl || '')}" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="checkbox"><input type="checkbox" name="isAvailable" ${f?.isAvailable !== false ? 'checked' : ''}/> Şu anda yeni işlere açığım</label>
                </div>
                <div class="flex justify-end">
                    <button class="btn btn-primary" type="submit">Kaydet</button>
                </div>
            </form>
        </div>
    `;
    body.querySelector('#fl-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData(e.target);
        const payload = {
            title: fd.get('title') || null,
            bio: fd.get('bio') || null,
            experienceYears: fd.get('experienceYears') ? parseInt(fd.get('experienceYears')) : null,
            hourlyRateMin: fd.get('hourlyRateMin') ? parseFloat(fd.get('hourlyRateMin')) : null,
            hourlyRateMax: fd.get('hourlyRateMax') ? parseFloat(fd.get('hourlyRateMax')) : null,
            currency: fd.get('currency') || null,
            country: fd.get('country') || null,
            city: fd.get('city') || null,
            cvUrl: fd.get('cvUrl') || null,
            linkedInUrl: fd.get('linkedInUrl') || null,
            gitHubUrl: fd.get('gitHubUrl') || null,
            websiteUrl: fd.get('websiteUrl') || null,
            isAvailable: fd.get('isAvailable') === 'on'
        };
        try {
            await api.upsertFreelancer(payload);
            toast('Freelancer profilin kaydedildi.', 'success');
        } catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    });
}

// ---------- Portfolio ----------
async function renderPortfolioTab(body, { api }) {
    const f = (await api.getMyFreelancer()).data;
    const items = f?.portfolioItems || [];
    body.innerHTML = `
        <div class="flex justify-between items-center mb-4">
            <div class="text-muted">${items.length} portfolio kalemi</div>
            <button class="btn btn-primary" id="add-btn">${icons.plus} Yeni kalem</button>
        </div>
        <div class="portfolio-grid">
            ${items.map(p => `
                <div class="portfolio-card" data-id="${p.id}">
                    ${p.imageUrl ? `<div class="thumb"><img src="${esc(p.imageUrl)}" onerror="this.style.display='none'"/></div>` : ''}
                    <div class="body">
                        <div class="font-semibold">${esc(p.title)}</div>
                        ${p.description ? `<div class="text-muted text-sm line-clamp-3 mt-1">${esc(p.description)}</div>` : ''}
                        ${p.projectUrl ? `<a class="text-sm mt-2 inline-flex" href="${esc(p.projectUrl)}" target="_blank" rel="noopener">Projeye git →</a>` : ''}
                        <div class="flex justify-end gap-2 mt-3">
                            <button class="btn btn-ghost btn-sm edit-btn">${icons.edit}</button>
                            <button class="btn btn-ghost btn-sm del-btn">${icons.trash}</button>
                        </div>
                    </div>
                </div>
            `).join('')}
        </div>
        ${!items.length ? '<div class="card"><div class="card-empty"><div class="empty-title">Kalem yok</div><div>İlk portfolio öğeni ekle.</div></div></div>' : ''}
    `;
    body.querySelector('#add-btn').addEventListener('click', () => showPortfolioModal(body, api, null));
    body.querySelectorAll('.edit-btn').forEach(b => b.addEventListener('click', (e) => {
        const id = e.target.closest('.portfolio-card').dataset.id;
        const item = items.find(x => x.id === id);
        showPortfolioModal(body, api, item);
    }));
    body.querySelectorAll('.del-btn').forEach(b => b.addEventListener('click', async (e) => {
        const id = e.target.closest('.portfolio-card').dataset.id;
        if (!await confirmModal('Bu kalemi silmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'Sil' })) return;
        try { await api.deletePortfolio(id); toast('Silindi.', 'success'); renderPortfolioTab(body, { api }); }
        catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    }));
}

function showPortfolioModal(body, api, item) {
    const isEdit = !!item;
    const modal = openModal({
        title: isEdit ? 'Portfolio kalemini düzenle' : 'Yeni portfolio kalemi',
        body: `
            <form id="pf-form">
                <div class="form-group">
                    <label class="form-label required">Başlık</label>
                    <input class="input" name="title" maxlength="200" required value="${esc(item?.title || '')}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Açıklama</label>
                    <textarea class="textarea" name="description" rows="4" maxlength="1500">${esc(item?.description || '')}</textarea>
                </div>
                <div class="form-group">
                    <label class="form-label">Proje URL'i</label>
                    <input class="input" name="projectUrl" type="url" value="${esc(item?.projectUrl || '')}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Görsel URL'i</label>
                    <input class="input" name="imageUrl" type="url" value="${esc(item?.imageUrl || '')}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Sıra</label>
                    <input class="input" name="displayOrder" type="number" min="0" value="${item?.displayOrder ?? 0}" />
                </div>
            </form>
        `,
        footer: `<button class="btn btn-secondary" data-act="cancel">Vazgeç</button><button class="btn btn-primary" data-act="submit">Kaydet</button>`
    });
    modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
    modal.footer.querySelector('[data-act=submit]').onclick = async () => {
        const fd = new FormData(modal.body.querySelector('form'));
        const payload = {
            title: fd.get('title').trim(),
            description: fd.get('description') || null,
            projectUrl: fd.get('projectUrl') || null,
            imageUrl: fd.get('imageUrl') || null,
            displayOrder: parseInt(fd.get('displayOrder') || '0')
        };
        try {
            if (isEdit) await api.updatePortfolio(item.id, payload);
            else await api.addPortfolio(payload);
            modal.close();
            toast('Kaydedildi.', 'success');
            renderPortfolioTab(body, { api });
        } catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    };
}

// ---------- Skills ----------
async function renderSkillsTab(body, { api }) {
    const [me, allSkills] = await Promise.all([api.getMyFreelancer(), api.getSkills()]);
    const mySkills = new Map((me.data?.skills || []).map(s => [s.skillId, s.proficiencyLevel]));
    const skills = allSkills.data || [];

    body.innerHTML = `
        <div class="card">
            <div class="card-title mb-2">Yetkinliklerin</div>
            <div class="text-muted mb-3">İşe alım eşleşmesi için sahip olduğun teknolojileri seç.</div>
            <div class="chip-list" id="skill-picker">
                ${skills.map(s => `
                    <span class="chip ${mySkills.has(s.id) ? 'selected' : ''}" data-id="${s.id}">${esc(s.name)}</span>
                `).join('')}
            </div>
            <div class="flex justify-end mt-4">
                <button class="btn btn-primary" id="save-skills">Kaydet</button>
            </div>
        </div>
    `;
    const picker = body.querySelector('#skill-picker');
    picker.querySelectorAll('.chip').forEach(c => c.addEventListener('click', () => c.classList.toggle('selected')));
    body.querySelector('#save-skills').addEventListener('click', async () => {
        const selected = [...picker.querySelectorAll('.chip.selected')].map(c => ({ skillId: c.dataset.id, proficiencyLevel: null }));
        try { await api.setMySkills(selected); toast('Yetkinliklerin güncellendi.', 'success'); }
        catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    });
}

// ---------- Client profili ----------
async function renderClientTab(body, { api }) {
    let c = null;
    try { c = (await api.getMyClient()).data; } catch { c = null; }
    body.innerHTML = `
        <div class="card" style="max-width:820px">
            <form id="c-form">
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Firma adı</label>
                        <input class="input" name="companyName" maxlength="200" value="${esc(c?.companyName || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Sektör</label>
                        <input class="input" name="industry" maxlength="150" value="${esc(c?.industry || '')}" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="form-label">Hakkında</label>
                    <textarea class="textarea" name="about" rows="5" maxlength="2000">${esc(c?.about || '')}</textarea>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Website</label>
                        <input class="input" name="websiteUrl" type="url" value="${esc(c?.websiteUrl || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">İletişim telefonu</label>
                        <input class="input" name="contactPhone" value="${esc(c?.contactPhone || '')}" />
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Ülke</label>
                        <input class="input" name="country" value="${esc(c?.country || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Şehir</label>
                        <input class="input" name="city" value="${esc(c?.city || '')}" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="form-label">İletişim tercihi</label>
                    <select class="select" name="contactPreference">
                        ${ContactPreferenceOptions.map(o => `<option value="${o.value}" ${c?.contactPreference === o.value ? 'selected' : ''}>${o.label}</option>`).join('')}
                    </select>
                </div>
                <div class="flex justify-end">
                    <button class="btn btn-primary" type="submit">Kaydet</button>
                </div>
            </form>
        </div>
    `;
    body.querySelector('#c-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData(e.target);
        const payload = {
            companyName: fd.get('companyName') || null,
            industry: fd.get('industry') || null,
            about: fd.get('about') || null,
            websiteUrl: fd.get('websiteUrl') || null,
            country: fd.get('country') || null,
            city: fd.get('city') || null,
            contactPreference: parseInt(fd.get('contactPreference') || '1'),
            contactPhone: fd.get('contactPhone') || null
        };
        try { await api.upsertClient(payload); toast('İşveren profilin kaydedildi.', 'success'); }
        catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    });
}