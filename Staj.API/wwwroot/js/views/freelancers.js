// views/freelancers.js — freelancer arama ve detay

import {
    esc, initials, fullName, stars, formatDate, formatDateTime,
    renderPagination, bindPagination, openModal, toast, icons
} from '../utils.js';

// ==================== Freelancer arama ====================
export async function renderFreelancers(ctx, { query }) {
    const { renderView, api, router } = ctx;

    renderView('Freelancerlar', `
        <div class="page-header">
            <div>
                <h1>Freelancerlar</h1>
                <div class="page-sub">Yetkinlik, konum veya ücret aralığına göre mühendis bul.</div>
            </div>
        </div>
        <div class="filter-bar">
            <input class="input" id="q" placeholder="🔍 İsim, unvan..." value="${esc(query.keyword || '')}" style="min-width:220px" />
            <select class="select" id="cat"><option value="">Tüm kategoriler</option></select>
            <input class="input" id="minR" type="number" placeholder="Min ücret" value="${esc(query.minHourlyRate || '')}" style="max-width:130px" />
            <input class="input" id="maxR" type="number" placeholder="Max ücret" value="${esc(query.maxHourlyRate || '')}" style="max-width:130px" />
            <input class="input" id="city" placeholder="Şehir" value="${esc(query.city || '')}" style="max-width:140px" />
            <label class="checkbox"><input type="checkbox" id="avail" ${query.isAvailable === 'true' ? 'checked' : ''}/> Sadece müsait olanlar</label>
            <button class="btn btn-primary btn-sm" id="apply-filter">Uygula</button>
            <button class="btn btn-ghost btn-sm" id="clear-filter">Temizle</button>
        </div>
        <div id="fl-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);

    try {
        const cats = await api.getCategories();
        const sel = document.getElementById('cat');
        (cats.data || []).forEach(c => {
            const o = document.createElement('option');
            o.value = c.id; o.textContent = c.name;
            if (query.categoryId === c.id) o.selected = true;
            sel.appendChild(o);
        });
    } catch { }

    const apply = () => {
        const q = {};
        const kv = document.getElementById('q').value.trim();
        const cat = document.getElementById('cat').value;
        const minR = document.getElementById('minR').value;
        const maxR = document.getElementById('maxR').value;
        const city = document.getElementById('city').value.trim();
        const avail = document.getElementById('avail').checked;
        if (kv) q.keyword = kv;
        if (cat) q.categoryId = cat;
        if (minR) q.minHourlyRate = minR;
        if (maxR) q.maxHourlyRate = maxR;
        if (city) q.city = city;
        if (avail) q.isAvailable = 'true';
        location.hash = '#/freelancers' + (Object.keys(q).length ? '?' + new URLSearchParams(q).toString() : '');
    };
    document.getElementById('apply-filter').onclick = apply;
    document.getElementById('q').addEventListener('keydown', e => { if (e.key === 'Enter') apply(); });
    document.getElementById('clear-filter').onclick = () => { location.hash = '#/freelancers'; };

    const list = document.getElementById('fl-list');
    try {
        const params = {
            page: parseInt(query.page || '1'), pageSize: 12,
            keyword: query.keyword, categoryId: query.categoryId,
            minHourlyRate: query.minHourlyRate, maxHourlyRate: query.maxHourlyRate,
            city: query.city, isAvailable: query.isAvailable
        };
        const r = await api.searchFreelancers(params);
        const p = r.data;
        if (!p || !p.items || p.items.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">Freelancer bulunamadı</div><div>Filtreleri değiştirmeyi dene.</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="grid grid-3">
                ${p.items.map(f => freelancerCard(f)).join('')}
            </div>
            ${renderPagination(p)}
        `;
        list.querySelectorAll('.freelancer-card').forEach(c => {
            c.addEventListener('click', () => router.go('/freelancers/' + c.dataset.id));
        });
        bindPagination(list, page => {
            const q = { ...query, page };
            location.hash = '#/freelancers?' + new URLSearchParams(q).toString();
        });
    } catch (e) {
        list.innerHTML = `<div class="card text-danger">Yüklenemedi: ${esc(e?.message || '')}</div>`;
    }
}

function freelancerCard(f) {
    const rate = f.hourlyRateMin || f.hourlyRateMax
        ? `${f.currency || '₺'}${(f.hourlyRateMin || f.hourlyRateMax).toLocaleString('tr-TR')}${f.hourlyRateMin && f.hourlyRateMax ? ' - ' + (f.currency || '₺') + f.hourlyRateMax.toLocaleString('tr-TR') : ''} /sa`
        : 'Ücret belirtilmemiş';
    return `
        <div class="freelancer-card" data-id="${f.id}">
            <div class="flex items-center gap-3 mb-3">
                <div class="avatar avatar-lg">${esc(initials(f.firstName, f.lastName))}</div>
                <div class="grow">
                    <div class="font-semibold">${esc(fullName(f.firstName, f.lastName))}</div>
                    <div class="text-muted text-sm truncate">${esc(f.title || 'Freelancer')}</div>
                </div>
                ${f.isAvailable ? '<span class="badge badge-success">Müsait</span>' : '<span class="badge badge-muted">Meşgul</span>'}
            </div>
            <div class="chip-list mb-3">
                ${(f.skills || []).slice(0, 5).map(s => `<span class="chip">${esc(s)}</span>`).join('')}
                ${(f.skills || []).length > 5 ? `<span class="chip">+${f.skills.length - 5}</span>` : ''}
            </div>
            <div class="text-sm text-muted flex items-center gap-3">
                <span>${esc(rate)}</span>
                ${f.city ? `<span>· ${esc(f.city)}</span>` : ''}
                ${f.experienceYears ? `<span>· ${f.experienceYears} yıl</span>` : ''}
            </div>
        </div>
    `;
}

// ==================== Freelancer detay ====================
export async function renderFreelancerDetail(ctx, { params }) {
    const { renderView, api, router, auth } = ctx;
    renderView('Freelancer', `<div class="loading-box"><div class="spinner"></div></div>`);
    const view = document.getElementById('view');

    let f;
    try {
        const r = await api.getFreelancer(params.id);
        f = r.data;
    } catch (e) {
        view.innerHTML = `<div class="card text-danger">Freelancer bulunamadı: ${esc(e?.message || '')}</div>`;
        return;
    }

    const me = auth.getUser();
    const isSelf = me && me.userId === f.userId;
    const canOffer = auth.isClient() && !isSelf;
    const canMessage = !isSelf;

    view.innerHTML = `
        <div class="page-header">
            <div>
                <button class="btn btn-ghost btn-sm" data-nav="/freelancers">${icons.back} Listeye dön</button>
            </div>
            <div class="flex gap-2">
                ${canMessage ? `<button class="btn btn-secondary" id="msg-btn">${icons.message} Mesaj gönder</button>` : ''}
                ${canOffer ? `<button class="btn btn-primary" id="offer-btn">Doğrudan teklif gönder</button>` : ''}
            </div>
        </div>

        <div class="grid grid-sidebar">
            <div>
                <div class="card">
                    <div class="flex items-center gap-4 mb-4">
                        <div class="avatar avatar-xl">${esc(initials(f.firstName, f.lastName))}</div>
                        <div class="grow">
                            <h1>${esc(fullName(f.firstName, f.lastName))}</h1>
                            <div class="text-muted">${esc(f.title || 'Freelancer')}</div>
                            <div class="flex items-center gap-3 mt-2">
                                <span class="rating-summary"><span class="stars">${stars(f.averageRating)}</span> ${(f.averageRating || 0).toFixed(1)} <span class="text-muted">(${f.reviewCount} yorum)</span></span>
                                ${f.isAvailable ? '<span class="badge badge-success">Müsait</span>' : '<span class="badge badge-muted">Meşgul</span>'}
                            </div>
                        </div>
                    </div>
                    ${f.bio ? `<div class="mb-3" style="white-space:pre-wrap">${esc(f.bio)}</div>` : ''}
                </div>

                <div class="card mt-4">
                    <div class="section-title">Yetkinlikler</div>
                    <div class="chip-list">
                        ${(f.skills || []).map(s => `<span class="chip selected">${esc(s.name)}${s.proficiencyLevel ? ` <span class="text-soft">· ${s.proficiencyLevel}/5</span>` : ''}</span>`).join('') || '<span class="text-muted">Belirtilmemiş</span>'}
                    </div>
                </div>

                <div class="card mt-4">
                    <div class="section-title">Portfolio</div>
                    ${(f.portfolioItems || []).length ? `
                        <div class="portfolio-grid">
                            ${f.portfolioItems.map(p => `
                                <div class="portfolio-card">
                                    ${p.imageUrl ? `<div class="thumb"><img src="${esc(p.imageUrl)}" alt="${esc(p.title)}" onerror="this.style.display='none'"/></div>` : ''}
                                    <div class="body">
                                        <div class="font-semibold">${esc(p.title)}</div>
                                        ${p.description ? `<div class="text-muted text-sm line-clamp-3 mt-1">${esc(p.description)}</div>` : ''}
                                        ${p.projectUrl ? `<a class="text-sm mt-2 inline-flex" href="${esc(p.projectUrl)}" target="_blank" rel="noopener">Projeye git →</a>` : ''}
                                    </div>
                                </div>
                            `).join('')}
                        </div>
                    ` : '<div class="text-muted">Portfolio kalemi yok.</div>'}
                </div>

                <div class="card mt-4">
                    <div class="section-title">Son değerlendirmeler</div>
                    ${(f.recentReviews || []).length ? f.recentReviews.map(r => `
                        <div class="mb-3 pb-3" style="border-bottom:1px solid var(--border)">
                            <div class="flex items-center gap-2 mb-1">
                                <div class="avatar">${esc(initials(r.authorFirstName, r.authorLastName))}</div>
                                <div class="grow">
                                    <div class="font-medium">${esc(fullName(r.authorFirstName, r.authorLastName))}</div>
                                    <div class="text-muted text-sm">${formatDate(r.createdAt)}</div>
                                </div>
                                <span class="stars">${stars(r.rating)}</span>
                            </div>
                            <div class="text-muted" style="white-space:pre-wrap">${esc(r.comment)}</div>
                        </div>
                    `).join('') : '<div class="text-muted">Henüz değerlendirme yok.</div>'}
                </div>
            </div>

            <aside>
                <div class="card">
                    <div class="section-title">Ücret</div>
                    <div class="text-xl font-semibold">
                        ${f.hourlyRateMin || f.hourlyRateMax
            ? `${f.currency || '₺'}${(f.hourlyRateMin || f.hourlyRateMax).toLocaleString('tr-TR')}${f.hourlyRateMin && f.hourlyRateMax ? ' - ' + (f.currency || '₺') + f.hourlyRateMax.toLocaleString('tr-TR') : ''} /sa`
            : 'Belirtilmemiş'}
                    </div>
                    ${f.experienceYears ? `<div class="text-muted mt-2">Deneyim: <b>${f.experienceYears} yıl</b></div>` : ''}
                    ${(f.city || f.country) ? `<div class="text-muted mt-1">Konum: <b>${esc([f.city, f.country].filter(Boolean).join(', '))}</b></div>` : ''}
                </div>

                ${(f.linkedInUrl || f.gitHubUrl || f.websiteUrl || f.cvUrl) ? `
                    <div class="card mt-4">
                        <div class="section-title">Bağlantılar</div>
                        <div class="flex flex-col gap-2">
                            ${f.websiteUrl ? `<a href="${esc(f.websiteUrl)}" target="_blank" rel="noopener">🌐 Website</a>` : ''}
                            ${f.linkedInUrl ? `<a href="${esc(f.linkedInUrl)}" target="_blank" rel="noopener">💼 LinkedIn</a>` : ''}
                            ${f.gitHubUrl ? `<a href="${esc(f.gitHubUrl)}" target="_blank" rel="noopener">🐙 GitHub</a>` : ''}
                            ${f.cvUrl ? `<a href="${esc(f.cvUrl)}" target="_blank" rel="noopener">📄 CV</a>` : ''}
                        </div>
                    </div>
                ` : ''}
            </aside>
        </div>
    `;

    view.querySelector('#msg-btn')?.addEventListener('click', async () => {
        try {
            const r = await api.startConversation(f.userId);
            router.go('/messages/' + r.data);
        } catch (e) { toast(e?.message || 'Sohbet başlatılamadı.', 'error'); }
    });

    view.querySelector('#offer-btn')?.addEventListener('click', () => showOfferModal(ctx, f));
}

function showOfferModal(ctx, f) {
    const { api, router } = ctx;
    const modal = openModal({
        title: 'Doğrudan teklif gönder',
        wide: true,
        body: `
            <div class="text-muted mb-3">${esc(fullName(f.firstName, f.lastName))} adlı freelancer'a doğrudan teklif göndereceksin.</div>
            <form id="offer-form">
                <div class="form-group">
                    <label class="form-label required">Teklif metni</label>
                    <textarea class="textarea" name="coverMessage" rows="6" required placeholder="İşin kapsamı, beklentiler ve şartlar..."></textarea>
                </div>
                <div class="form-row-3">
                    <div class="form-group">
                        <label class="form-label">Önerdiğin ücret</label>
                        <input class="input" name="proposedRate" type="number" step="0.01" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Para birimi</label>
                        <input class="input" name="currency" maxlength="3" value="TRY" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Süre (gün)</label>
                        <input class="input" name="proposedDurationDays" type="number" />
                    </div>
                </div>
            </form>
        `,
        footer: `
            <button class="btn btn-secondary" data-act="cancel">Vazgeç</button>
            <button class="btn btn-primary" data-act="submit">Gönder</button>
        `
    });
    modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
    modal.footer.querySelector('[data-act=submit]').onclick = async () => {
        const form = modal.body.querySelector('form');
        const fd = new FormData(form);
        const payload = {
            freelancerProfileId: f.id,
            coverMessage: fd.get('coverMessage'),
            proposedRate: fd.get('proposedRate') ? parseFloat(fd.get('proposedRate')) : null,
            proposedDurationDays: fd.get('proposedDurationDays') ? parseInt(fd.get('proposedDurationDays')) : null,
            currency: fd.get('currency') || null
        };
        try {
            await api.submitDirectOffer(payload);
            modal.close();
            toast('Teklif gönderildi.', 'success');
            router.go('/proposals');
        } catch (e) {
            toast(e?.message || 'Teklif gönderilemedi.', 'error');
        }
    };
}