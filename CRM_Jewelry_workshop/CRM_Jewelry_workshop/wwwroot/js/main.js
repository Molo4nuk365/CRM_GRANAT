// ======================== ГЛОБАЛЬНЫЕ ПЕРЕМЕННЫЕ ========================
let currentUser = null;
let cart = [];
let products = [];
let repairOptions = [];
let sortField = 'price';
let sortOrder = 'asc';
let isOnMainSite = false;

const profileAvatar = document.getElementById('profileAvatar');
const cartBtn = document.getElementById('cartBtn');
const cartCountSpan = document.getElementById('cartCount');
const dynamicContent = document.getElementById('dynamicContent');
const toastMsg = document.getElementById('toastMsg');
const heroBlock = document.querySelector('.hero-fullscreen');
const scrollBtn = document.getElementById('scrollToCatalog');
const historyBtn = document.getElementById('historyBtn');

function showToast(msg, isError = false) {
    toastMsg.textContent = msg;
    toastMsg.style.background = isError ? '#d9534f' : 'var(--accent)';
    toastMsg.classList.add('show');
    setTimeout(() => toastMsg.classList.remove('show'), 2500);
}
function saveCart() { localStorage.setItem('granat_cart', JSON.stringify(cart)); }
function loadCart() { const saved = localStorage.getItem('granat_cart'); if (saved) cart = JSON.parse(saved); updateCartBadge(); }
function updateCartBadge() { const count = cart.reduce((s, i) => s + (i.quantity || 1), 0); cartCountSpan.innerText = count; }

// API (относительные пути – будут работать при открытии через бэкенд)
async function apiFetch(url, options = {}) {
    const headers = { 'Content-Type': 'application/json' };
    if (currentUser?.token) headers['Authorization'] = `Bearer ${currentUser.token}`;
    const res = await fetch(url, { ...options, headers });
    if (res.status === 401) { logout(); showToast('Сессия истекла', true); throw new Error('Unauthorized'); }
    if (!res.ok) { const err = await res.text(); throw new Error(err || 'Ошибка запроса'); }
    return res.json();
}

// Загрузка товаров
async function loadProductsFromAPI() {
    try {
        products = await apiFetch('/api/products');
        products = products.map(p => ({ ...p, id: p.productId }));
    } catch (e) {
        console.warn('Заглушка товаров');
        products = [
            { id: 1, productId: 1, name: "Кольцо «Гранатовый рассвет»", price: 18500, description: "Серебро 925, гранат 0.8 карат", material: "Серебро 925, гранат", weight: "3.2 г", article: "GR-101", imageUrl: "images/кольцо.png" },
            { id: 2, productId: 2, name: "Серьги «Лунный свет»", price: 12400, description: "Серебро 925, гранат", material: "Серебро 925, гранат", weight: "4.5 г", article: "GR-102", imageUrl: "images/серьги.png" },
            { id: 3, productId: 3, name: "Подвеска «Капля росы»", price: 9800, description: "Серебро 925, гранат", material: "Серебро 925, гранат", weight: "1.8 г", article: "GR-103", imageUrl: "images/подвеска.jpg" },
            { id: 4, productId: 4, name: "Браслет «Серебряная нить»", price: 23500, description: "Серебро 925", material: "Серебро 925", weight: "6.2 г", article: "GR-104", imageUrl: "images/браслет.png" },
            { id: 5, productId: 5, name: "Брошь «Гранат»", price: 15900, description: "Серебро 925, гранат", material: "Серебро 925, гранат", weight: "5.1 г", article: "GR-105", imageUrl: "images/брошь.jpg" }
        ];
    }
}
async function loadRepairsFromAPI() {
    repairOptions = [
        { id: 101, name: "Ремонт кольца", price: 3500, desc: "Пайка, полировка, изменение размера" },
        { id: 102, name: "Ремонт серёг", price: 2800, desc: "Замена замка, полировка" },
        { id: 103, name: "Ремонт цепочки", price: 2200, desc: "Пайка звеньев, замена замка" },
        { id: 104, name: "Чистка и полировка", price: 1200, desc: "Ультразвуковая чистка, полировка" }
    ];
}

// Тёмная тема
const themeToggle = document.getElementById('themeToggle');
if (localStorage.getItem('theme') === 'dark') document.body.classList.add('dark-theme');
if (themeToggle) {
    themeToggle.addEventListener('click', () => {
        document.body.classList.toggle('dark-theme');
        localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
        const icon = themeToggle.querySelector('i');
        if (icon) icon.className = document.body.classList.contains('dark-theme') ? 'fas fa-sun' : 'fas fa-moon';
    });
}

// Аватар и выход
function updateProfileAvatar() {
    if (!currentUser) {
        profileAvatar.innerHTML = `<i class="fas fa-user-circle"></i>`;
        profileAvatar.classList.add('default-icon');
        profileAvatar.onclick = () => showAuthModal();
        if (historyBtn) historyBtn.classList.add('d-none');
        return;
    }
    let iconHtml = '';
    switch (currentUser.role) {
        case 'admin': iconHtml = '<i class="fas fa-shield-haltered"></i>'; break;
        case 'manager': iconHtml = '<i class="fas fa-briefcase"></i>'; break;
        case 'jeweler': iconHtml = '<i class="fas fa-gem"></i>'; break;
        default: iconHtml = '<i class="fas fa-user"></i>';
    }
    profileAvatar.innerHTML = iconHtml;
    profileAvatar.classList.remove('default-icon');
    profileAvatar.onclick = logout;
    if (currentUser.role === 'client') {
        if (historyBtn) historyBtn.classList.remove('d-none');
    } else {
        if (historyBtn) historyBtn.classList.add('d-none');
    }
}
function logout() {
    currentUser = null;
    localStorage.removeItem('token');
    sessionStorage.removeItem('granat_user');
    cart = []; saveCart(); updateCartBadge();
    isOnMainSite = false;
    renderApp();
    showToast('Вы вышли');
}

// Авторизация
function showAuthModal() {
    const container = document.getElementById('authFormContainer');
    container.innerHTML = `
        <ul class="nav nav-tabs justify-content-center border-0 mb-3" id="authTab" role="tablist">
            <li class="nav-item"><button class="nav-link active" data-bs-toggle="tab" data-bs-target="#loginTab">Вход</button></li>
            <li class="nav-item"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#registerTab">Регистрация</button></li>
        </ul>
        <div class="tab-content mt-3">
            <div class="tab-pane active" id="loginTab">
                <input id="authLogin" class="form-control mb-2" placeholder="Логин">
                <input id="authPassword" type="password" class="form-control mb-2" placeholder="Пароль">
                <button id="doLoginBtn" class="btn-primary w-100">Войти</button>
            </div>
            <div class="tab-pane" id="registerTab">
                <input id="regName" class="form-control mb-2" placeholder="Имя и фамилия">
                <input id="regLogin" class="form-control mb-2" placeholder="Логин">
                <input id="regPassword" type="password" class="form-control mb-2" placeholder="Пароль">
                <input id="regEmail" type="email" class="form-control mb-2" placeholder="Электронная почта">
                <input id="regPhone" class="form-control mb-2" placeholder="Телефон">
                <input id="regAddress" class="form-control mb-2" placeholder="Адрес">
                <button id="doRegBtn" class="btn-primary w-100">Зарегистрироваться</button>
            </div>
        </div>
    `;
    const modal = new bootstrap.Modal(document.getElementById('authModal'));
    modal.show();
    document.getElementById('doLoginBtn')?.addEventListener('click', async () => {
        const login = document.getElementById('authLogin').value.trim();
        const pwd = document.getElementById('authPassword').value.trim();
        if (!login || !pwd) return showToast('Введите логин и пароль', true);
        try {
            const res = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ login, password: pwd })
            });
            if (!res.ok) throw new Error('Неверный логин или пароль');
            const data = await res.json();
            currentUser = {
                id: data.userId,
                fullName: data.fullName,
                role: data.roleName,
                token: data.token
            };
            localStorage.setItem('token', data.token);
            sessionStorage.setItem('granat_user', JSON.stringify(currentUser));
            await loadProductsFromAPI();
            await loadRepairsFromAPI();
            modal.hide();
            showToast(`Добро пожаловать, ${data.fullName}!`);
            renderApp();
        } catch (e) { showToast(e.message, true); }
    });
    document.getElementById('doRegBtn')?.addEventListener('click', async () => {
        const name = document.getElementById('regName').value.trim();
        const login = document.getElementById('regLogin').value.trim();
        const pwd = document.getElementById('regPassword').value.trim();
        const email = document.getElementById('regEmail').value.trim();
        const phone = document.getElementById('regPhone').value.trim();
        const address = document.getElementById('regAddress').value.trim();
        if (!name || !login || !pwd) return showToast('Имя, логин и пароль обязательны', true);
        try {
            await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ login, password: pwd, fullName: name, email, phone, address })
            });
            showToast('Регистрация успешна, войдите');
            document.querySelector('#authTab button[data-bs-target="#loginTab"]')?.click();
        } catch (e) { showToast('Ошибка регистрации', true); }
    });
}

// Корзина
function modifyCart(type, id, delta) {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); showAuthModal(); return; }
    const idx = cart.findIndex(i => i.type === type && i.id === id);
    if (idx !== -1) {
        let newQty = cart[idx].quantity + delta;
        if (newQty <= 0) cart.splice(idx, 1);
        else cart[idx].quantity = newQty;
    } else if (delta > 0) cart.push({ type, id, quantity: 1 });
    saveCart(); updateCartBadge(); showToast(delta > 0 ? 'Добавлено' : 'Убрано');
    if (document.getElementById('cartModal').classList.contains('show')) renderCartModal();
}
function addToCart(productId) { modifyCart('product', productId, 1); }
function addRepairToCart(repairId) { modifyCart('repair', repairId, 1); }
function removeFromCart(index) { cart.splice(index, 1); saveCart(); updateCartBadge(); if (document.getElementById('cartModal').classList.contains('show')) renderCartModal(); showToast('Удалено'); }
function renderCartModal() {
    const modalBody = document.getElementById('cartModalBody');
    if (!modalBody) return;
    if (cart.length === 0) { modalBody.innerHTML = `<p class="text-center">Корзина пуста</p><button class="btn-primary w-100" data-bs-dismiss="modal">Закрыть</button>`; return; }
    let total = 0, itemsHtml = `<div class="list-group gap-2">`;
    cart.forEach((item, idx) => {
        let name, price, realId;
        if (item.type === 'product') {
            const p = products.find(pr => pr.productId === item.id);
            if (!p) return;
            name = p.name; price = p.price; realId = p.productId;
        } else {
            const r = repairOptions.find(rr => rr.id === item.id);
            if (!r) return;
            name = r.name; price = r.price; realId = r.id;
        }
        const qty = item.quantity || 1;
        total += price * qty;
        itemsHtml += `<div class="list-group-item glass-card d-flex flex-wrap justify-content-between align-items-center">
            <div><strong>${name}</strong><br><small>${price.toLocaleString()}₽ × ${qty}</small></div>
            <div class="cart-quantity">
                <button class="qty-btn btn-outline" onclick="modifyCart('${item.type}', ${realId}, -1)">-</button>
                <span>${qty}</span>
                <button class="qty-btn btn-outline" onclick="modifyCart('${item.type}', ${realId}, 1)">+</button>
                <button class="btn-sm btn-outline-danger" onclick="removeFromCart(${idx})"><i class="fas fa-trash-alt"></i> Удалить</button>
            </div>
        </div>`;
    });
    itemsHtml += `</div><hr><div class="fw-bold fs-5">Итого: ${total.toLocaleString()} ₽</div><button id="checkoutBtn" class="btn-primary mt-4 w-100">Оформить заказ</button>`;
    modalBody.innerHTML = itemsHtml;
    document.getElementById('checkoutBtn')?.addEventListener('click', checkoutOrder);
}
async function checkoutOrder() {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); return; }
    if (cart.length === 0) { showToast('Корзина пуста'); return; }
    const items = cart.map(i => ({ type: i.type, id: i.id, quantity: i.quantity || 1 }));
    try {
        await apiFetch('/api/orders/create', { method: 'POST', body: JSON.stringify({ items }) });
        cart = []; saveCart(); updateCartBadge();
        const modal = bootstrap.Modal.getInstance(document.getElementById('cartModal'));
        if (modal) modal.hide();
        showToast('Заказ успешно оформлен!');
        renderClientCatalog();
    } catch (e) { showToast('Ошибка: ' + e.message, true); }
}

// История заказов (клиент)
async function showOrderHistory() {
    if (!currentUser || currentUser.role !== 'client') { showToast('Только для клиентов', true); return; }
    let orders = [];
    try {
        orders = await apiFetch('/api/orders/my');
    } catch (e) {
        showToast('Не удалось загрузить заказы', true);
        return;
    }
    if (orders.length === 0) {
        document.getElementById('historyModalBody').innerHTML = '<p class="text-center">У вас пока нет заказов</p>';
        new bootstrap.Modal(document.getElementById('historyModal')).show();
        return;
    }
    let html = `<div class="table-wrapper"><table class="table"><thead><tr><th>ID</th><th>Тип</th><th>Наименование</th><th>Статус</th><th>Сумма</th><th>Дата</th><th>Срок</th></tr></thead><tbody>`;
    for (let o of orders) {
        let name = '';
        let type = o.type === 'product' ? 'Товар' : 'Ремонт';
        if (o.type === 'product') {
            const prod = products.find(p => p.productId === o.productId);
            name = prod ? prod.name : 'Товар';
        } else {
            const rep = repairOptions.find(r => r.id === o.repairId);
            name = rep ? rep.name : 'Ремонт';
        }
        const deadline = o.deadline ? new Date(o.deadline).toLocaleDateString() : '—';
        html += `<tr>
            <td>${o.orderId}</td>
            <td>${type}</td>
            <td>${name}</td>
            <td><span class="status-badge status-${o.statusOrder?.name || 'new'}">${o.statusOrder?.name || 'new'}</span></td>
            <td>${o.totalCost}₽</td>
            <td>${new Date(o.createDate).toLocaleDateString()}</td>
            <td>${deadline}</td>
        </tr>`;
    }
    html += `</tbody></table></div>`;
    document.getElementById('historyModalBody').innerHTML = html;
    new bootstrap.Modal(document.getElementById('historyModal')).show();
}

// Сортировка и каталог
function sortProducts() {
    const sorted = [...products];
    if (sortField === 'name') sorted.sort((a, b) => sortOrder === 'asc' ? a.name.localeCompare(b.name) : b.name.localeCompare(a.name));
    else sorted.sort((a, b) => sortOrder === 'asc' ? a.price - b.price : b.price - a.price);
    return sorted;
}
function renderClientCatalog() {
    const sorted = sortProducts();
    let html = `<h2 class="catalog-title">✨ Каталог ювелирных украшений</h2>`;
    if (currentUser && currentUser.role === 'client') {
        html += `<div class="action-bar">
            <button id="repairActionBtn" class="btn-outline glass-btn">🔧 Ремонт изделия</button>
            <button id="orderActionBtn" class="btn-outline glass-btn">📦 Сделать заказ</button>
        </div>`;
    } else if (!currentUser) {
        html += `<div class="action-bar">
            <button id="repairActionBtn" class="btn-outline glass-btn">🔧 Ремонт изделия (только для авторизованных)</button>
            <button id="orderActionBtn" class="btn-outline glass-btn">📦 Сделать заказ (только для авторизованных)</button>
        </div>`;
    }
    if (isOnMainSite && currentUser && currentUser.role !== 'client') {
        html += `<div class="text-start mb-3"><button id="backToPanelFromMain" class="btn-back-panel"><i class="fas fa-arrow-left"></i> Вернуться на панель</button></div>`;
    }
    html += `<div class="products-grid" id="catalogGrid">`;
    sorted.forEach(p => {
        const materialText = p.material ? p.material : (p.description ? p.description.split(',')[0] : 'Серебро 925');
        const weightText = p.weight ? p.weight : '—';
        html += `<div class="product-card animate-on-scroll" data-id="${p.productId}">
            <img src="${p.imageUrl}" class="product-img" onerror="this.src='https://placehold.co/300x300'">
            <div class="product-info">
                <div class="fw-semibold">${p.name}</div>
                <div class="product-price">${p.price.toLocaleString()} ₽</div>
                <div class="product-details">${materialText} • ${weightText}</div>
                <button class="btn-primary mt-2" onclick="event.stopPropagation(); addToCart(${p.productId})">В корзину</button>
            </div>
        </div>`;
    });
    html += `</div>`;
    dynamicContent.innerHTML = html;
    document.querySelectorAll('.product-card').forEach(card => {
        card.addEventListener('click', (e) => {
            if (!e.target.classList.contains('btn-primary')) showProductModal(parseInt(card.dataset.id));
        });
    });
    const repairBtn = document.getElementById('repairActionBtn');
    if (repairBtn) {
        repairBtn.addEventListener('click', () => {
            if (!currentUser || currentUser.role !== 'client') { showToast('Только для авторизованных клиентов', true); showAuthModal(); return; }
            showRepairCatalog();
        });
    }
    const orderBtn = document.getElementById('orderActionBtn');
    if (orderBtn) {
        orderBtn.addEventListener('click', () => {
            if (!currentUser || currentUser.role !== 'client') { showToast('Только для авторизованных клиентов', true); showAuthModal(); return; }
            showToast('Выберите товар и добавьте в корзину');
        });
    }
    const backBtn = document.getElementById('backToPanelFromMain');
    if (backBtn) {
        backBtn.addEventListener('click', () => {
            isOnMainSite = false;
            if (currentUser.role === 'admin') renderAdminDashboard();
            else if (currentUser.role === 'manager') renderManagerDashboard();
            else if (currentUser.role === 'jeweler') renderJewelerDashboard();
        });
    }
    initScrollAnimation();
}
function showRepairCatalog() {
    let html = `<div class="dashboard"><h2>🛠️ Услуги ремонта</h2><div class="list-group gap-2">`;
    repairOptions.forEach(r => {
        html += `<div class="list-group-item glass-card d-flex justify-content-between align-items-center">
            <div><strong>${r.name}</strong><br>${r.desc}<br>${r.price}₽</div>
            <button class="btn-primary" onclick="addRepairToCart(${r.id}); renderClientCatalog();">В корзину</button>
        </div>`;
    });
    html += `</div><button class="btn-outline mt-3" onclick="renderClientCatalog()">← Назад в каталог</button></div>`;
    dynamicContent.innerHTML = html;
    initScrollAnimation();
}
function showProductModal(id) {
    const p = products.find(p => p.productId === id);
    if (!p) return;
    const body = document.getElementById('productModalBody');
    body.innerHTML = `
        <div class="text-center"><img src="${p.imageUrl}" style="max-height:200px" class="mb-3 rounded"></div>
        <h4>${p.name}</h4><p>${p.description}</p><hr>
        <div><strong>Материал:</strong> ${p.material || '—'}</div>
        <div><strong>Вес:</strong> ${p.weight || '—'}</div>
        <div><strong>Артикул:</strong> ${p.article || '—'}</div>
        <div class="d-flex justify-content-between mt-3">
            <span class="h4" style="color:var(--accent)">${p.price.toLocaleString()}₽</span>
            <button class="btn-primary" onclick="addToCart(${p.productId}); bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();">В корзину</button>
        </div>
    `;
    new bootstrap.Modal(document.getElementById('productModal')).show();
}

// Таблица заказов (общая)
async function renderOrdersTable(orders) {
    if (!orders || !orders.length) return '<div class="text-center">Нет заказов</div>';
    let html = `<div class="table-wrapper"><table class="table"><thead><tr><th>ID</th><th>Клиент</th><th>Статус</th><th>Сумма</th><th>Срок</th><th>Действие</th><tr></thead><tbody>`;
    orders.forEach(o => {
        const clientName = o.client?.fullName || o.clientName || 'Не указан';
        const statusName = o.statusOrder?.name || o.status || 'new';
        const deadline = o.deadline ? new Date(o.deadline).toLocaleDateString() : '—';
        html += `<tr>
            <td>${o.orderId}</td>
            <td>${clientName}</td>
            <td><span class="status-badge status-${statusName}">${statusName}</span></td>
            <td>${o.totalCost}₽</td>
            <td>${deadline}</td>
            <td>`;
        if ((currentUser.role === 'manager' || currentUser.role === 'admin') && statusName === 'new') {
            html += `<button class="btn-sm btn-outline" onclick="window.acceptOrder(${o.orderId})">Принять</button> `;
        }
        if (currentUser.role === 'admin' || currentUser.role === 'manager' || currentUser.role === 'jeweler') {
            html += `<button class="btn-sm btn-outline" onclick="window.changeOrderStatus(${o.orderId})">Статус</button>`;
        }
        html += `</div>`;
    });
    html += `</tbody></table></div>`;
    return html;
}
window.changeOrderStatus = async function (orderId) {
    let newStatus = prompt('Новый статус (new, in_progress, completed, cancelled)');
    if (newStatus) {
        await apiFetch(`/api/orders/${orderId}/status`, { method: 'PUT', body: JSON.stringify({ status: newStatus }) });
        showToast('Статус обновлён');
        renderApp();
    }
};
window.acceptOrder = async function (orderId) {
    const jewelerId = prompt('Введите ID ювелира (например, 3 – jeweler)');
    if (!jewelerId) return;
    try {
        await apiFetch(`/api/orders/${orderId}/assignJeweler`, { method: 'PUT', body: JSON.stringify(parseInt(jewelerId)) });
        await apiFetch(`/api/orders/${orderId}/status`, { method: 'PUT', body: JSON.stringify({ status: 'in_progress' }) });
        showToast('Заказ принят, назначен ювелир');
        renderApp();
    } catch (e) { showToast('Ошибка: ' + e.message, true); }
};
window.setDeadline = async function (orderId) {
    const deadlineStr = prompt('Введите дату завершения (ГГГГ-ММ-ДД)');
    if (!deadlineStr) return;
    const deadline = new Date(deadlineStr);
    if (isNaN(deadline.getTime())) { showToast('Неверный формат даты', true); return; }
    await apiFetch(`/api/orders/${orderId}/setDeadline`, { method: 'PUT', body: JSON.stringify(deadline) });
    showToast('Срок указан');
    renderApp();
};
window.completeOrder = async function (orderId) {
    await apiFetch(`/api/orders/${orderId}/status`, { method: 'PUT', body: JSON.stringify({ status: 'completed' }) });
    showToast('Заказ завершён');
    renderApp();
};

// Админ-панель
async function renderAdminDashboard() {
    let orders = [], users = [], materials = [];
    try { orders = await apiFetch('/api/orders/all'); } catch (e) { console.warn(e); }
    try { users = await apiFetch('/api/users'); } catch (e) { console.warn(e); }
    try { materials = await apiFetch('/api/materials'); } catch (e) { console.warn(e); }
    const total = orders.length, completed = orders.filter(o => o.statusOrder?.name === 'completed').length;
    const revenue = orders.filter(o => o.statusOrder?.name === 'completed').reduce((s, o) => s + o.totalCost, 0);
    let html = `<div class="dashboard">
        <h2 class="mb-3">🔧 Админ панель</h2>
        <div class="stats-grid">
            <div class="stat-card"><h3>${total}</h3><p>Заказов</p></div>
            <div class="stat-card"><h3>${completed}</h3><p>Завершено</p></div>
            <div class="stat-card"><h3>${revenue.toLocaleString()} ₽</h3><p>Выручка</p></div>
        </div>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <button id="adminOrdersBtn" class="btn-primary glass-btn">Заказы</button>
            <button id="adminProductsBtn" class="btn-primary glass-btn">Товары</button>
            <button id="adminMaterialsBtn" class="btn-outline glass-btn">Материалы</button>
            <button id="adminUsersBtn" class="btn-outline glass-btn">Пользователи</button>
            <button id="adminBackCatalog" class="btn-outline glass-btn">На сайт</button>
        </div>
        <div id="adminContent"></div>
    </div>`;
    dynamicContent.innerHTML = html;
    document.getElementById('adminOrdersBtn').onclick = async () => {
        document.getElementById('adminContent').innerHTML = await renderOrdersTable(orders);
    };
    document.getElementById('adminProductsBtn').onclick = () => renderProductsTable();
    document.getElementById('adminMaterialsBtn').onclick = () => renderMaterialsTable(materials);
    document.getElementById('adminUsersBtn').onclick = () => renderUsersTable(users);
    document.getElementById('adminBackCatalog').onclick = () => { isOnMainSite = true; renderClientCatalog(); };
    document.getElementById('adminContent').innerHTML = await renderOrdersTable(orders);
}
function renderProductsTable() {
    if (!products.length) { document.getElementById('adminContent').innerHTML = '<p class="text-center">Товары не загружены</p>'; return; }
    let html = `<h3>Товары</h3>
        <div class="table-wrapper">
            <table class="table">
                <thead><tr><th>Название</th><th>Цена</th><th>Артикул</th><th>Материал</th><th>Вес (г)</th></tr></thead>
                <tbody>${products.map(p => `<tr><td>${p.name}<td>${p.price.toLocaleString()} ₽</td><td>${p.article || '—'}</td><td>${p.material || '—'}</td><td>${p.weight || '—'}</td></tr>`).join('')}</tbody>
            </table>
        </div>`;
    document.getElementById('adminContent').innerHTML = html;
}
async function renderUsersTable(users) {
    let html = `<h3>Пользователи</h3>
        <button id="addUserBtn" class="btn-primary mb-2">+ Сотрудник</button>
        <div class="table-wrapper">
            <table class="table">
                <thead><tr><th>ID</th><th>Логин</th><th>Имя</th><th>Роль</th><th>Действие</th></tr></thead>
                <tbody>${users.map(u => `<tr><td>${u.userId}</td><td>${u.login}</td><td>${u.fullName}</td><td>${u.roleName}</td><td>${u.userId !== currentUser.id ? `<button class="btn-sm btn-outline" onclick="window.deleteUser(${u.userId})">Удалить</button>` : '—'} </tr>`).join('')}</tbody>
            </table>
        </div>`;
    document.getElementById('adminContent').innerHTML = html;
    document.getElementById('addUserBtn')?.addEventListener('click', async () => {
        const login = prompt('Логин сотрудника');
        if (login) {
            const pwd = prompt('Пароль');
            const name = prompt('Полное имя');
            const role = prompt('Роль (manager/jeweler)');
            if (login && pwd && name && role && (role === 'manager' || role === 'jeweler')) {
                try {
                    await apiFetch('/api/auth/register', { method: 'POST', body: JSON.stringify({ login, password: pwd, fullName: name, phone: '', email: '' }) });
                    showToast('Сотрудник добавлен');
                    renderAdminDashboard();
                } catch (e) { showToast('Ошибка добавления', true); }
            } else showToast('Некорректная роль', true);
        }
    });
}
window.deleteUser = async function (id) {
    if (id === currentUser.id) return showToast('Нельзя удалить себя', true);
    if (!confirm('Вы уверены?')) return;
    try {
        await apiFetch(`/api/users/${id}`, { method: 'DELETE' });
        showToast('Пользователь удалён');
        renderAdminDashboard();
    } catch (e) { showToast('Ошибка удаления: ' + e.message, true); }
};
async function renderMaterialsTable(materials) {
    let html = `<h3>Материалы</h3>
        <button id="addMaterialBtn" class="btn-primary mb-2">+ Добавить материал</button>
        <div class="table-wrapper">
            <table class="table">
                <thead><tr><th>Наименование</th><th>Количество</th><th>Цена/ед</th><th>Действие</th></tr></thead>
                <tbody>${materials.map(m => `<tr><td>${m.name}</td><td>${m.quantityInStock} ${m.unit}</td><td>${m.pricePerUnit}₽</td><td><button class="btn-sm btn-outline" onclick="window.deleteMaterial(${m.materialId})">Удалить</button></td></tr>`).join('')}</tbody>
            </table>
        </div>`;
    document.getElementById('adminContent').innerHTML = html;
    document.getElementById('addMaterialBtn')?.addEventListener('click', showAddMaterialForm);
}
function showAddMaterialForm() {
    const name = prompt('Название материала');
    if (!name) return;
    const unit = prompt('Единица измерения (г, кар, шт)');
    const price = parseFloat(prompt('Цена за единицу'));
    const quantity = parseFloat(prompt('Количество на складе'));
    if (isNaN(price) || isNaN(quantity)) return showToast('Цена и количество должны быть числами', true);
    apiFetch('/api/materials', { method: 'POST', body: JSON.stringify({ name, unit, pricePerUnit: price, quantityInStock: quantity, description: '' }) })
        .then(() => { showToast('Материал добавлен'); renderAdminDashboard(); })
        .catch(e => showToast('Ошибка: ' + e.message, true));
}
window.deleteMaterial = async function (id) {
    if (!confirm('Удалить материал?')) return;
    try {
        const response = await fetch(`/api/materials/${id}`, { method: 'DELETE', headers: { 'Authorization': `Bearer ${currentUser.token}` } });
        if (!response.ok) throw new Error('Ошибка удаления');
        showToast('Материал удалён');
        renderAdminDashboard();
    } catch (e) { showToast('Ошибка: ' + e.message, true); }
};

// Менеджер-панель
async function renderManagerDashboard() {
    let orders = [], materials = [];
    try { orders = await apiFetch('/api/orders/all'); } catch (e) { console.warn(e); }
    try { materials = await apiFetch('/api/materials'); } catch (e) { console.warn(e); }
    let html = `<div class="dashboard">
        <h2 class="mb-3">📦 Панель менеджера</h2>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <button id="managerOrdersBtn" class="btn-primary glass-btn">Все заказы</button>
            <button id="managerMaterialsBtn" class="btn-outline glass-btn">Материалы</button>
            <button id="managerBackCatalog" class="btn-outline glass-btn">На сайт</button>
        </div>
        <div id="managerContent"></div>
    </div>`;
    dynamicContent.innerHTML = html;
    document.getElementById('managerOrdersBtn').onclick = async () => {
        document.getElementById('managerContent').innerHTML = await renderOrdersTable(orders);
    };
    document.getElementById('managerMaterialsBtn').onclick = () => {
        document.getElementById('managerContent').innerHTML = `<div class="table-wrapper"><table class="table"><thead><tr><th>Наименование</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>${materials.map(m => `<tr><td>${m.name}</td><td>${m.quantityInStock} ${m.unit}</td><td>${m.pricePerUnit}₽</td></tr>`).join('')}</tbody></table></div><button class="btn-outline mt-2" onclick="renderManagerDashboard()">← Назад</button>`;
    };
    document.getElementById('managerBackCatalog').onclick = () => { isOnMainSite = true; renderClientCatalog(); };
    document.getElementById('managerContent').innerHTML = await renderOrdersTable(orders);
}

// Ювелир-панель
async function renderJewelerDashboard() {
    let orders = [], materials = [];
    try { orders = await apiFetch('/api/orders/jeweler'); } catch (e) { console.warn(e); }
    try { materials = await apiFetch('/api/materials'); } catch (e) { console.warn(e); materials = []; }
    let html = `<div class="dashboard">
        <h2 class="mb-3">💎 Панель ювелира</h2>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <button id="jewelerOrdersBtn" class="btn-primary glass-btn">Мои заказы</button>
            <button id="jewelerMaterialsBtn" class="btn-outline glass-btn">Материалы</button>
            <button id="jewelerBackCatalog" class="btn-outline glass-btn">На сайт</button>
        </div>
        <div id="jewelerContent"></div>
    </div>`;
    dynamicContent.innerHTML = html;
    const renderOrders = () => {
        if (orders.length === 0) { document.getElementById('jewelerContent').innerHTML = '<p class="text-center">Нет активных заказов</p>'; return; }
        let table = `<div class="table-wrapper"><table class="table"><thead><td><th>ID</th><th>Клиент</th><th>Статус</th><th>Сумма</th><th>Срок</th><th>Действие</th></tr></thead><tbody>`;
        orders.forEach(o => {
            const deadline = o.deadline ? new Date(o.deadline).toLocaleDateString() : '—';
            table += `<tr>
                <td>${o.orderId}</td>
                <td>${o.client?.fullName || ''}</td>
                <td><span class="status-badge status-${o.statusOrder?.name}">${o.statusOrder?.name}</span></td>
                <td>${o.totalCost}₽</td><td>${deadline}</td>
                <td>`;
            if (o.statusOrder?.name !== 'completed') {
                table += `<button class="btn-sm btn-primary" onclick="window.setDeadline(${o.orderId})">Указать срок</button> `;
                table += `<button class="btn-sm btn-primary" onclick="window.completeOrder(${o.orderId})">Завершить</button>`;
            } else { table += `✓`; }
            table += `</tr>`;
        });
        table += `</tbody></table></div>`;
        document.getElementById('jewelerContent').innerHTML = table;
    };
    document.getElementById('jewelerOrdersBtn').onclick = renderOrders;
    document.getElementById('jewelerMaterialsBtn').onclick = () => {
        document.getElementById('jewelerContent').innerHTML = `<div class="table-wrapper"><table class="table"><thead><tr><th>Наименование</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>${materials.map(m => `<tr><td>${m.name}</td><td>${m.quantityInStock} ${m.unit}</td><td>${m.pricePerUnit}₽</td></tr>`).join('')}</tbody></table></div><button class="btn-outline mt-2" onclick="renderJewelerDashboard()">← Назад</button>`;
    };
    document.getElementById('jewelerBackCatalog').onclick = () => { isOnMainSite = true; renderClientCatalog(); };
    renderOrders();
}

// Главный рендер
async function renderApp() {
    const saved = sessionStorage.getItem('granat_user');
    if (saved && !currentUser) {
        currentUser = JSON.parse(saved);
        await loadProductsFromAPI();
        await loadRepairsFromAPI();
    }
    updateProfileAvatar();
    if (!currentUser || currentUser.role === 'client') {
        if (heroBlock) heroBlock.style.display = 'flex';
        renderClientCatalog();
        if (scrollBtn) scrollBtn.addEventListener('click', () => document.getElementById('catalogGrid')?.scrollIntoView({ behavior: 'smooth' }));
        isOnMainSite = false;
        initScrollAnimation();
        return;
    }
    if (heroBlock) heroBlock.style.display = 'none';
    if (currentUser.role === 'admin') await renderAdminDashboard();
    else if (currentUser.role === 'manager') await renderManagerDashboard();
    else if (currentUser.role === 'jeweler') await renderJewelerDashboard();
    initScrollAnimation();
}

function initScrollAnimation() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(e => { if (e.isIntersecting) { e.target.classList.add('animated'); observer.unobserve(e.target); } });
    }, { threshold: 0.1 });
    document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
}

// Запуск
(async function () {
    await loadProductsFromAPI();
    await loadRepairsFromAPI();
    loadCart();
    updateCartBadge();
    await renderApp();
    cartBtn.addEventListener('click', () => { renderCartModal(); new bootstrap.Modal(document.getElementById('cartModal')).show(); });
    if (historyBtn) historyBtn.addEventListener('click', showOrderHistory);
    window.addToCart = addToCart;
    window.addRepairToCart = addRepairToCart;
    window.removeFromCart = removeFromCart;
    window.modifyCart = modifyCart;
})();