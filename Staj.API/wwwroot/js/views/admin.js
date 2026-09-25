// views/admin.js — kategori ve skill yönetimi (admin)

import { esc, toast, openModal, confirmModal, icons, formatDate } from '../utils.js';

// ==================== Kategoriler ====================
export async function renderAdminCategories(ctx) {
    const { renderView, api } = ctx;
    renderView('Kategoriler', `
        <div class="page-header">
            <div>
                <h1>Kategoriler</h1>
                <div class="page-sub">Sistemin iş kategorilerini yönet.</div>
            </div>
            <button class="btn btn-primary" id="add-btn">${icons.plus} Yeni kategori</button>
        </div>
        <div id="cat-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);
    document.getElementById('add-btn').addEventListener('click', () => showCategoryModal(ctx, null));
    await loadCategories(ctx);
}

async function loadCategories(ctx) {
    const { api } = ctx;
    const list = document.getElementById('cat-list');
    try {
        const r = await api.getCategories();
        const cats = r.data || [];
        if (cats.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">Kategori yok</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="card tight">
                <table class="table hoverable">
                    <thead>
                        <tr><th>Ad</th><th>Slug</th><th>Skill</th><th>Aktif</th><th>Tarih</th><th></th></tr>
                    </thead>
                    <tbody>
                        ${cats.map(c => `
                            <tr data-id="${c.id}">
                                <td class="font-medium">${esc(c.name)}</td>
                                <td class="text-muted">${esc(c.slug)}</td>
                                <td>${c.skillCount}</td>
                                <td>${c.isActive ? '<span class="badge badge-success">Aktif</span>' : '<span class="badge badge-muted">Pasif</span>'}</td>
                                <td class="text-muted">${formatDate(c.createdAt)}</td>
                                <td class="text-right">
                                    <button class="btn btn-ghost btn-sm edit">${icons.edit}</button>
                                    <button class="btn btn-ghost btn-sm skills-btn">Skills</button>
                                    <button class="btn btn-ghost btn-sm del">${icons.trash}</button>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
        list.querySelectorAll('.edit').forEach(b => b.addEventListener('click', (e) => {
            const id = e.target.closest('tr').dataset.id;
            const cat = cats.find(x => x.id === id);
            showCategoryModal(ctx, cat);
        }));
        list.querySelectorAll('.del').forEach(b => b.addEventListener('click', async (e) => {
            const id = e.target.closest('tr').dataset.id;
            if (!await confirmModal('Kategoriyi silmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'Sil' })) return;
            try { await api.deleteCategory(id); toast('Silindi.', 'success'); loadCategories(ctx); }
            catch (ex) { toast(ex?.message || 'Hata', 'error'); }
        }));
        list.querySelectorAll('.skills-btn').forEach(b => b.addEventListener('click', async (e) => {
            const id = e.target.closest('tr').dataset.id;
            showCategorySkillsModal(ctx, id);
        }));
    } catch (e) {
        list.innerHTML = `<div class="card text-danger">${esc(e?.message || 'Hata')}</div>`;
    }
}

function showCategoryModal(ctx, cat) {
    const { api } = ctx;
    const isEdit = !!cat;
    const modal = openModal({
        title: isEdit ? 'Kategoriyi düzenle' : 'Yeni kategori',
        body: `
            <form id="cat-form">
                <div class="form-group">
                    <label class="form-label required">Ad</label>
                    <input class="input" name="name" required maxlength="100" value="${esc(cat?.name || '')}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Açıklama</label>
                    <textarea class="textarea" name="description" rows="3" maxlength="500">${esc(cat?.description || '')}</textarea>
                </div>
                ${isEdit ? `
                    <div class="form-group">
                        <label class="checkbox"><input type="checkbox" name="isActive" ${cat.isActive ? 'checked' : ''}/> Aktif</label>
                    </div>
                ` : ''}
            </form>
        `,
        footer: `<button class="btn btn-secondary" data-act="cancel">Vazgeç</button><button class="btn btn-primary" data-act="submit">Kaydet</button>`
    });
    modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
    modal.footer.querySelector('[data-act=submit]').onclick = async () => {
        const fd = new FormData(modal.body.querySelector('form'));
        const payload = {
            name: fd.get('name').trim(),
            description: fd.get('description') || null,
            ...(isEdit ? { isActive: fd.get('isActive') === 'on' } : {})
        };
        try {
            if (isEdit) await api.updateCategory(cat.id, payload);
            else await api.createCategory(payload);
            modal.close();
            toast('Kaydedildi.', 'success');
            loadCategories(ctx);
        } catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    };
}

async function showCategorySkillsModal(ctx, catId) {
    const { api } = ctx;
    const [detail, allSkills] = await Promise.all([api.getCategory(catId), api.getSkills()]);
    const cat = detail.data;
    const has = new Set((cat.skills || []).map(s => s.id));
    const skills = allSkills.data || [];
    const modal = openModal({
        title: `"${cat.name}" kategorisi — Yetkinlikler`,
        wide: true,
        body: `
            <div class="text-muted mb-3">Bu kategoriye ait yetkinlikleri seç.</div>
            <div class="chip-list" id="cat-skills">
                ${skills.map(s => `<span class="chip ${has.has(s.id) ? 'selected' : ''}" data-id="${s.id}">${esc(s.name)}</span>`).join('')}
            </div>
        `,
        footer: `<button class="btn btn-secondary" data-act="close">Kapat</button>`
    });
    modal.footer.querySelector('[data-act=close]').onclick = modal.close;
    modal.body.querySelectorAll('.chip').forEach(chip => {
        chip.addEventListener('click', async () => {
            const skillId = chip.dataset.id;
            const wasSelected = chip.classList.contains('selected');
            chip.classList.toggle('selected');
            try {
                if (wasSelected) await api.removeSkillFromCategory(catId, skillId);
                else await api.assignSkillToCategory(catId, skillId);
            } catch (ex) {
                chip.classList.toggle('selected'); // rollback
                toast(ex?.message || 'Hata', 'error');
            }
        });
    });
}

// ==================== Skills ====================
export async function renderAdminSkills(ctx) {
    const { renderView, api } = ctx;
    renderView('Yetkinlikler', `
        <div class="page-header">
            <div>
                <h1>Yetkinlikler</h1>
                <div class="page-sub">Sistemin skill/teknoloji havuzunu yönet.</div>
            </div>
            <button class="btn btn-primary" id="add-btn">${icons.plus} Yeni yetkinlik</button>
        </div>
        <div id="skill-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);
    document.getElementById('add-btn').addEventListener('click', () => showSkillModal(ctx, null));
    await loadSkills(ctx);
}

async function loadSkills(ctx) {
    const { api } = ctx;
    const list = document.getElementById('skill-list');
    try {
        const [r, cats] = await Promise.all([api.getSkills(), api.getCategories()]);
        const items = r.data || [];
        if (items.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">Yetkinlik yok</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="card tight">
                <table class="table hoverable">
                    <thead><tr><th>Ad</th><th>Slug</th><th>Kategori sayısı</th><th>Aktif</th><th></th></tr></thead>
                    <tbody>
                        ${items.map(s => `
                            <tr data-id="${s.id}">
                                <td class="font-medium">${esc(s.name)}</td>
                                <td class="text-muted">${esc(s.slug)}</td>
                                <td>${s.categoryCount}</td>
                                <td>${s.isActive ? '<span class="badge badge-success">Aktif</span>' : '<span class="badge badge-muted">Pasif</span>'}</td>
                                <td class="text-right">
                                    <button class="btn btn-ghost btn-sm edit">${icons.edit}</button>
                                    <button class="btn btn-ghost btn-sm del">${icons.trash}</button>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
        list.querySelectorAll('.edit').forEach(b => b.addEventListener('click', (e) => {
            const id = e.target.closest('tr').dataset.id;
            const s = items.find(x => x.id === id);
            showSkillModal(ctx, s, cats.data || []);
        }));
        list.querySelectorAll('.del').forEach(b => b.addEventListener('click', async (e) => {
            const id = e.target.closest('tr').dataset.id;
            if (!await confirmModal('Yetkinliği silmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'Sil' })) return;
            try { await api.deleteSkill(id); toast('Silindi.', 'success'); loadSkills(ctx); }
            catch (ex) { toast(ex?.message || 'Hata', 'error'); }
        }));
    } catch (e) {
        list.innerHTML = `<div class="card text-danger">${esc(e?.message || 'Hata')}</div>`;
    }
}

function showSkillModal(ctx, skill, allCats) {
    const { api } = ctx;
    const isEdit = !!skill;
    // Kategoriler yüklü değilse yükle
    const catsPromise = allCats ? Promise.resolve(allCats) : api.getCategories().then(r => r.data || []);
    catsPromise.then(cats => {
        const modal = openModal({
            title: isEdit ? 'Yetkinliği düzenle' : 'Yeni yetkinlik',
            body: `
                <form id="skill-form">
                    <div class="form-group">
                        <label class="form-label required">Ad</label>
                        <input class="input" name="name" required maxlength="100" value="${esc(skill?.name || '')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Açıklama</label>
                        <textarea class="textarea" name="description" rows="3" maxlength="500">${esc(skill?.description || '')}</textarea>
                    </div>
                    ${!isEdit ? `
                        <div class="form-group">
                            <label class="form-label">Kategoriler</label>
                            <div class="chip-list" id="cat-picker">
                                ${cats.map(c => `<span class="chip" data-id="${c.id}">${esc(c.name)}</span>`).join('')}
                            </div>
                        </div>
                    ` : `
                        <div class="form-group">
                            <label class="checkbox"><input type="checkbox" name="isActive" ${skill.isActive ? 'checked' : ''}/> Aktif</label>
                        </div>
                    `}
                </form>
            `,
            footer: `<button class="btn btn-secondary" data-act="cancel">Vazgeç</button><button class="btn btn-primary" data-act="submit">Kaydet</button>`
        });
        modal.body.querySelectorAll('#cat-picker .chip').forEach(c =>
            c.addEventListener('click', () => c.classList.toggle('selected'))
        );
        modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
        modal.footer.querySelector('[data-act=submit]').onclick = async () => {
            const fd = new FormData(modal.body.querySelector('form'));
            const payload = {
                name: fd.get('name').trim(),
                description: fd.get('description') || null,
                ...(isEdit
                    ? { isActive: fd.get('isActive') === 'on' }
                    : { categoryIds: [...modal.body.querySelectorAll('#cat-picker .chip.selected')].map(c => c.dataset.id) }
                )
            };
            try {
                if (isEdit) await api.updateSkill(skill.id, payload);
                else await api.createSkill(payload);
                modal.close();
                toast('Kaydedildi.', 'success');
                loadSkills(ctx);
            } catch (ex) { toast(ex?.message || 'Hata', 'error'); }
        };
    });
}