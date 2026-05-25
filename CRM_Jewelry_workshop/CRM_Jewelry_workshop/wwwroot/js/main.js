// ГЛОБАЛЬНЫЕ ПЕРЕМЕННЫЕ 
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

const inputModal = new bootstrap.Modal(document.getElementById('inputModal'));
const inputModalField = document.getElementById('inputModalField');
let inputModalResolve = null;

document.getElementById('inputModalConfirm').onclick = () => {
    if (inputModalResolve) inputModalResolve(inputModalField.value);
    inputModal.hide();
};
document.getElementById('inputModal').addEventListener('hidden.bs.modal', () => {
    if (inputModalResolve) inputModalResolve(null);
});

window.showInputDialog = function (placeholder, title = 'Ювелирная мастерская Гранат') {
    return new Promise((resolve) => {
        inputModalResolve = resolve;
        document.querySelector('#inputModal .modal-title').innerText = title;
        inputModalField.placeholder = placeholder;
        inputModalField.value = '';
        inputModal.show();
    });
};

// УТИЛИТЫ
window.showToast = function (msg, isError = false) {
    toastMsg.textContent = msg;
    toastMsg.style.background = isError ? '#d9534f' : 'var(--accent)';
    toastMsg.classList.add('show');
    setTimeout(() => toastMsg.classList.remove('show'), 2500);
};

function saveCart() { localStorage.setItem('granat_cart', JSON.stringify(cart)); }
function loadCart() { const saved = localStorage.getItem('granat_cart'); if (saved) cart = JSON.parse(saved); updateCartBadge(); }
function updateCartBadge() { const count = cart.reduce((s, i) => s + (i.quantity || 1), 0); cartCountSpan.innerText = count; }

// API 
async function apiFetch(url, options = {}, silent = false) {
    const headers = {};
    if (currentUser?.token) headers['Authorization'] = `Bearer ${currentUser.token}`;
    if (options.body !== undefined && options.body !== null) {
        headers['Content-Type'] = 'application/json';
    }

    console.log(` ${options.method || 'GET'} ${url}`, options.body ? JSON.parse(options.body) : '');

    const res = await fetch(url, { ...options, headers });

    if (res.status === 401) {
        if (!silent && currentUser) {
            logout();
            window.showToast('Сессия истекла', true);
        }
        const err = await res.text();
        console.error(` 401 от ${url}:`, err);
        throw new Error(err || 'Unauthorized');
    }
    if (!res.ok) {
        const errText = await res.text();
        console.error(`Ошибка ${res.status} от ${url}:`, errText);
        throw new Error(errText || 'Ошибка запроса');
    }
    const data = await res.json();
    console.log(`Ответ от ${url}:`, data);
    return data;
}

// ЗАГРУЗКА ДАННЫХ 
async function loadProductsFromAPI() {
    try {
        const data = await apiFetch('/api/products', {}, true);
        products = data.map(p => ({ ...p, id: p.productId }));
    } catch (e) {
        console.warn('Если не удалось загрузить данные с API , то испольем заглушку', e);
        products = [
            { productId: 1, name: "Кольцо «Гранатовый рассвет»", price: 18500, description: "Серебро 925, гранат 0.8 карат", metal: "Серебро 925", weight: "3.2", article: "GR-101", imageUrl: "/images/кольцо.png" },
            { productId: 2, name: "Серьги «Лунный свет»", price: 12400, description: "Серебро 925, гранат", metal: "Серебро 925", weight: "4.5", article: "GR-102", imageUrl: "/images/серьги.png" },
            { productId: 3, name: "Подвеска «Капля росы»", price: 9800, description: "Серебро 925, гранат 2 карат", metal: "Серебро 925", weight: "1.8", article: "GR-103", imageUrl: "/images/подвеска.jpg" },
            { productId: 4, name: "Браслет «Серебряная нить»", price: 23500, description: "Серебро 925", metal: "Серебро 925", weight: "6.2", article: "GR-104", imageUrl: "/images/браслет.png" },
            { productId: 5, name: "Брошь «Гранат»", price: 15900, description: "Серебро 925, гранат 2 карат", metal: "Серебро 925", weight: "5.1", article: "GR-105", imageUrl: "/images/брошь.jpg" }
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

// ТЁМНАЯ ТЕМА 
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

// АВАТАР И ВЫХОД 
function updateProfileAvatar() {
    if (!currentUser) {
        profileAvatar.innerHTML = `<i class="fas fa-user-circle"></i>`;
        profileAvatar.classList.add('default-icon');
        profileAvatar.onclick = () => showAuthModal();
        if (historyBtn) historyBtn.classList.add('d-none');
        if (cartBtn) cartBtn.style.display = 'none';
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
        if (cartBtn) cartBtn.style.display = 'inline-flex';
    } else {
        if (historyBtn) historyBtn.classList.add('d-none');
        if (cartBtn) cartBtn.style.display = 'none';
    }
}
function logout() {
    currentUser = null;
    localStorage.removeItem('token');
    sessionStorage.removeItem('granat_user');
    cart = []; saveCart(); updateCartBadge();
    isOnMainSite = false;
    renderApp();
    window.showToast('Вы вышли');
}

// АВТОРИЗАЦИЯ 
function showAuthModal() {
    const container = document.getElementById('authFormContainer');
    container.innerHTML = `
        <ul class="nav nav-tabs justify-content-center border-0 mb-3" id="authTab">
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
                <button id="doRegBtn" class="btn-primary w-100">Зарегистрироваться</button>
            </div>
        </div>
    `;
    const modal = new bootstrap.Modal(document.getElementById('authModal'));
    modal.show();
    document.getElementById('doLoginBtn')?.addEventListener('click', async () => {
        const login = document.getElementById('authLogin').value.trim();
        const pwd = document.getElementById('authPassword').value.trim();
        if (!login || !pwd) return window.showToast('Введите логин и пароль', true);
        console.log(`🔐 Пытаюсь войти: логин="${login}", пароль="${pwd}"`);
        try {
            const res = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ login, password: pwd })
            });
            if (!res.ok) {
                const errData = await res.json();
                console.error('Ошибка входа:', errData);
                throw new Error(errData.message || 'Неверный логин или пароль');
            }
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
            window.showToast(`Добро пожаловать, ${data.fullName}!`);
            renderApp();
        } catch (e) { window.showToast(e.message, true); }
    });
    document.getElementById('doRegBtn')?.addEventListener('click', async () => {
        const name = document.getElementById('regName').value.trim();
        const login = document.getElementById('regLogin').value.trim();
        const pwd = document.getElementById('regPassword').value.trim();
        const email = document.getElementById('regEmail').value.trim();
        const phone = document.getElementById('regPhone').value.trim();
        if (!name || !login || !pwd) return window.showToast('Имя, логин и пароль обязательны', true);
        console.log(`Регистрация: логин="${login}", пароль="${pwd}", имя="${name}"`);
        try {
            const res = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ login, password: pwd, fullName: name, email, phone })
            });
            if (!res.ok) {
                const errData = await res.json();
                console.error('Ошибка регистрации:', errData);
                throw new Error(errData.message || 'Ошибка регистрации');
            }
            window.showToast('Регистрация успешна, войдите');
            document.querySelector('#authTab button[data-bs-target="#loginTab"]')?.click();
        } catch (e) { window.showToast(e.message, true); }
    });
}

// ========================= КОРЗИНА =========================
function modifyCart(type, id, delta) {
    if (!currentUser || currentUser.role !== 'client') { window.showToast('Войдите как клиент', true); showAuthModal(); return; }
    const idx = cart.findIndex(i => i.type === type && i.id === id);
    if (idx !== -1) {
        let newQty = cart[idx].quantity + delta;
        if (newQty <= 0) cart.splice(idx, 1);
        else cart[idx].quantity = newQty;
    } else if (delta > 0) cart.push({ type, id, quantity: 1 });
    saveCart(); updateCartBadge(); window.showToast(delta > 0 ? 'Добавлено' : 'Убрано');
    if (document.getElementById('cartModal').classList.contains('show')) renderCartModal();
}
window.addToCart = function (productId) { modifyCart('product', productId, 1); };
window.addRepairToCart = function (repairId) { modifyCart('repair', repairId, 1); };
window.removeFromCart = function (index) { cart.splice(index, 1); saveCart(); updateCartBadge(); if (document.getElementById('cartModal').classList.contains('show')) renderCartModal(); window.showToast('Удалено'); };
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
                <button class="qty-btn btn-outline" onclick="window.modifyCart('${item.type}', ${realId}, -1)">-</button>
                <span>${qty}</span>
                <button class="qty-btn btn-outline" onclick="window.modifyCart('${item.type}', ${realId}, 1)">+</button>
                <button class="btn-sm btn-outline-danger" onclick="window.removeFromCart(${idx})"><i class="fas fa-trash-alt"></i> Удалить</button>
            </div>
        </div>`;
    });
    itemsHtml += `</div><hr><div class="fw-bold fs-5">Итого: ${total.toLocaleString()} ₽</div><button id="checkoutBtn" class="btn-primary mt-4 w-100">Оформить заказ</button>`;
    modalBody.innerHTML = itemsHtml;
    document.getElementById('checkoutBtn')?.addEventListener('click', checkoutOrder);
}
async function checkoutOrder() {
    if (!currentUser || currentUser.role !== 'client') { window.showToast('Войдите как клиент', true); return; }
    if (cart.length === 0) { window.showToast('Корзина пуста'); return; }
    const items = cart.map(i => ({ type: i.type, id: i.id, quantity: i.quantity || 1 }));
    try {
        const result = await apiFetch('/api/orders/create', { method: 'POST', body: JSON.stringify({ items }) });
        cart = []; saveCart(); updateCartBadge();
        const modal = bootstrap.Modal.getInstance(document.getElementById('cartModal'));
        if (modal) modal.hide();
        window.showToast(`Заказ №${result.orderId} создан! Ожидайте подтверждения менеджера.`);
        renderClientCatalog();
    } catch (e) { window.showToast('Ошибка: ' + e.message, true); }
}
window.modifyCart = modifyCart;

// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ДЛЯ СТАТУСОВ 
function getStatusClass(statusName) {
    const map = {
        'Новый': 'new',
        'Принят': 'accepted',
        'В работе': 'in_progress',
        'Готов': 'ready',
        'Завершён': 'completed',
        'Отменён': 'cancelled'
    };
    return map[statusName] || '';
}

// ИСТОРИЯ ЗАКАЗОВ (клиент) 
async function showOrderHistory() {
    if (!currentUser || currentUser.role !== 'client') {
        window.showToast('Только для клиентов', true);
        return;
    }
    let orders = [];
    try {
        orders = await apiFetch('/api/orders');
    } catch (e) {
        window.showToast('Не удалось загрузить заказы', true);
        return;
    }
    if (orders.length === 0) {
        document.getElementById('historyModalBody').innerHTML = '<p class="text-center">У вас пока нет заказов</p>';
        new bootstrap.Modal(document.getElementById('historyModal')).show();
        return;
    }
    let html = `<div class="table-wrapper"><table class="table"><thead>
         <th>ID</th><th>Статус</th><th>Сумма</th><th>Дата</th><th>Срок</th><th>Менеджер</th><th>Ювелир</th>
     </thead><tbody>`;
    orders.forEach(o => {
        const statusName = o.status || o.statusOrder?.name || '—';
        const deadline = o.deadline ? new Date(o.deadline).toLocaleDateString() : '—';
        html += `<tr>
            <td>${o.orderId}</td>
            <td><span class="status-badge status-${getStatusClass(statusName)}">${statusName}</span></td>
            <td>${o.totalCost}₽</td>
            <td>${new Date(o.createDate).toLocaleDateString()}</td>
            <td>${deadline}</td>
            <td>${o.managerName || '—'}</td>
            <td>${o.jewelerName || '—'}</td>
        </tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('historyModalBody').innerHTML = html;
    new bootstrap.Modal(document.getElementById('historyModal')).show();
}

// ТАБЛИЦА ЗАКАЗОВ ДЛЯ ПАНЕЛЕЙ 
async function renderOrdersTable(orders) {
    if (!orders || !orders.length) return '<div class="text-center">Нет заказов</div>';
    let html = `<div class="table-wrapper"><table class="table"><thead>
         <th>ID</th><th>Клиент</th><th>Статус</th><th>Сумма</th><th>Срок</th><th>Действие</th>
     </thead><tbody>`;
    for (let o of orders) {
        const clientName = o.clientName || 'Не указан';
        const statusName = o.status || o.statusOrder?.name || '—';
        const deadline = o.deadline ? new Date(o.deadline).toLocaleDateString() : '—';
        let actions = '';

        if ((currentUser.role === 'manager' || currentUser.role === 'admin') && statusName === 'Новый') {
            actions += `<button class="btn-sm btn-outline me-1" onclick="window.acceptOrder(${o.orderId})">Принять</button>`;
        }
        if ((currentUser.role === 'manager' || currentUser.role === 'admin') && (statusName === 'Новый' || statusName === 'Принят')) {
            actions += `<button class="btn-sm btn-outline me-1" onclick="window.assignJeweler(${o.orderId})">Назначить ювелира</button>`;
        }
        if (currentUser.role === 'jeweler' && statusName === 'Принят') {
            actions += `<button class="btn-sm btn-primary me-1" onclick="window.takeOrder(${o.orderId})">Взять в работу</button>`;
        }
        if (currentUser.role === 'jeweler' && statusName === 'В работе') {
            actions += `<button class="btn-sm btn-outline me-1" onclick="window.readyOrder(${o.orderId})">Готов</button>`;
        }
        if ((currentUser.role === 'manager' || currentUser.role === 'admin') && statusName === 'Готов') {
            actions += `<button class="btn-sm btn-primary me-1" onclick="window.completeOrder(${o.orderId})">Завершить</button>`;
        }
        if (currentUser.role === 'jeweler' && statusName === 'Готов') {
            actions += `<button class="btn-sm btn-primary me-1" onclick="window.completeOrder(${o.orderId})">Завершить</button>`;
        }
        if ((currentUser.role === 'manager' || currentUser.role === 'admin') && statusName !== 'Завершён' && statusName !== 'Отменён') {
            actions += `<button class="btn-sm btn-outline-danger me-1" onclick="window.cancelOrder(${o.orderId})">Отменить</button>`;
        }
        if (currentUser.role === 'manager' || currentUser.role === 'admin') {
            actions += `<button class="btn-sm btn-outline me-1" onclick="window.setDeadline(${o.orderId})">Срок</button>`;
        }

        if (!actions) actions = '—';

        html += `<tr>
            <td>${o.orderId}</td>
            <td>${clientName}</td>
            <td><span class="status-badge status-${getStatusClass(statusName)}">${statusName}</span></td>
            <td>${o.totalCost}₽</td>
            <td>${deadline}</td>
            <td>${actions}</td>
        </tr>`;
    }
    html += `</tbody></table></div>`;
    return html;
}

// ГЛОБАЛЬНЫЕ ФУНКЦИИ ДЕЙСТВИЙ ДЛЯ СТАТУСОВ ЗАКАЗА 
window.acceptOrder = async function (orderId) {
    try { await apiFetch(`/api/orders/${orderId}/accept`, { method: 'PUT' }); window.showToast('Заказ принят'); renderApp(); }
    catch (e) { window.showToast('Ошибка: ' + e.message, true); }
};

window.takeOrder = async function (orderId) {
    try { await apiFetch(`/api/orders/${orderId}/take`, { method: 'PUT' }); window.showToast('Заказ взят в работу'); renderApp(); }
    catch (e) { window.showToast('Ошибка: ' + e.message, true); }
};

window.readyOrder = async function (orderId) {
    try { await apiFetch(`/api/orders/${orderId}/ready`, { method: 'PUT' }); window.showToast('Заказ отмечен как готовый'); renderApp(); }
    catch (e) { window.showToast('Ошибка: ' + e.message, true); }
};

window.completeOrder = async function (orderId) {
    try { await apiFetch(`/api/orders/${orderId}/complete`, { method: 'PUT' }); window.showToast('Заказ завершён'); renderApp(); }
    catch (e) { window.showToast('Ошибка: ' + e.message, true); }
};

window.cancelOrder = async function (orderId) {
    try {
        await apiFetch(`/api/orders/${orderId}/cancel`, { method: 'PUT' });
        window.showToast('Заказ отменён');
        renderApp();
    } catch (e) {
        window.showToast('Ошибка отмены: ' + e.message, true);
    }
};

window.assignJeweler = async function (orderId) {
    let users = [];
    try { users = await apiFetch('/api/users'); } catch (e) { window.showToast('Ошибка загрузки пользователей', true); return; }
    const jewelers = users.filter(u => u.roleName === 'jeweler' || u.role === 'jeweler');
    if (!jewelers.length) { window.showToast('Нет доступных ювелиров', true); return; }

    const select = document.getElementById('jewelerSelect');
    select.innerHTML = jewelers.map(j => `<option value="${j.userId}">${j.fullName} (ID: ${j.userId})</option>`).join('');
    const modal = new bootstrap.Modal(document.getElementById('jewelerSelectModal'));
    modal.show();

    const confirmBtn = document.getElementById('confirmJewelerBtn');
    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    newConfirmBtn.addEventListener('click', async () => {
        const jewelerId = parseInt(select.value);
        modal.hide();
        try {
            await apiFetch(`/api/orders/${orderId}/assignJeweler`, {
                method: 'PUT',
                body: JSON.stringify(jewelerId)
            });
            window.showToast('Ювелир назначен');
            renderApp();
        } catch (e) {
            window.showToast('Ошибка: ' + e.message, true);
        }
    });
};

window.setDeadline = async function (orderId) {
    const deadlineStr = await window.showInputDialog('Введите дату (ГГГГ-ММ-ДД)', 'Установка срока');
    if (!deadlineStr) return;
    if (!/^\d{4}-\d{2}-\d{2}$/.test(deadlineStr)) {
        window.showToast('Формат даты: ГГГГ-ММ-ДД', true);
        return;
    }
    try {
        await apiFetch(`/api/orders/${orderId}/setDeadline`, {
            method: 'PUT',
            body: JSON.stringify(deadlineStr)
        });
        window.showToast('Срок установлен');
        renderApp();
    } catch (e) {
        window.showToast('Ошибка: ' + e.message, true);
    }
};

window.deleteUser = async function (id) {
    if (id === currentUser.id) return window.showToast('Нельзя удалить себя', true);
    if (!confirm('Вы уверены?')) return;
    try {
        await apiFetch(`/api/users/${id}`, { method: 'DELETE' });
        window.showToast('Пользователь удалён');
        renderAdminDashboard();
    } catch (e) { window.showToast('Ошибка удаления: ' + e.message, true); }
};

window.deleteMaterial = async function (id) {
    if (!confirm('Удалить материал?')) return;
    try {
        await apiFetch(`/api/materials/${id}`, { method: 'DELETE' });
        window.showToast('Материал удалён');
        renderAdminDashboard();
    } catch (e) { window.showToast('Ошибка: ' + e.message, true); }
};

// АДМИН-ПАНЕЛЬ ПОСЛЕ ВХОДА
async function renderAdminDashboard() {
    let orders = [], users = [], materials = [];
    try { orders = await apiFetch('/api/orders'); } catch (e) { console.warn(e); }
    try { users = await apiFetch('/api/users'); } catch (e) { console.warn(e); }
    try { materials = await apiFetch('/api/materials'); } catch (e) { console.warn(e); }

    const total = orders.length;
    const completed = orders.filter(o => (o.status || o.statusOrder?.name) === 'Завершён').length;
    const revenue = orders.filter(o => (o.status || o.statusOrder?.name) === 'Завершён').reduce((s, o) => s + o.totalCost, 0);

    let html = `<div class="dashboard">
        <h2 class="mb-3">Админ панель</h2>
        <div class="stats-grid">
            <div class="stat-card"><h3>${total}</h3><p>Заказов</p></div>
            <div class="stat-card"><h3>${completed}</h3><p>Завершено</p></div>
            <div class="stat-card"><h3>${revenue.toLocaleString()} ₽</h3><p>Выручка</p></div>
        </div>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <button id="adminRefreshStats" class="btn-outline glass-btn"><i class="fas fa-sync-alt"></i> Обновить статистику</button>
            <button id="adminOrdersBtn" class="btn-primary glass-btn">Заказы</button>
            <button id="adminProductsBtn" class="btn-primary glass-btn">Товары</button>
            <button id="adminMaterialsBtn" class="btn-outline glass-btn">Материалы</button>
            <button id="adminUsersBtn" class="btn-outline glass-btn">Пользователи</button>
            <button id="adminBackCatalog" class="btn-outline glass-btn">На сайт</button>
        </div>
        <div id="adminContent"></div>
    </div>`;
    dynamicContent.innerHTML = html;

    const refreshStats = async () => {
        try {
            const newOrders = await apiFetch('/api/orders');
            const newCompleted = newOrders.filter(o => (o.status || o.statusOrder?.name) === 'Завершён').length;
            const newRevenue = newOrders.filter(o => (o.status || o.statusOrder?.name) === 'Завершён').reduce((s, o) => s + o.totalCost, 0);
            document.querySelector('.stats-grid .stat-card:first-child h3').innerText = newOrders.length;
            document.querySelector('.stats-grid .stat-card:nth-child(2) h3').innerText = newCompleted;
            document.querySelector('.stats-grid .stat-card:last-child h3').innerText = newRevenue.toLocaleString() + ' ₽';
            window.showToast('Статистика обновлена');
        } catch (e) { window.showToast('Ошибка обновления', true); }
    };
    document.getElementById('adminRefreshStats').onclick = refreshStats;
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
    let html = `<h3>Товары</h3><div class="table-wrapper"><table class="table"><thead>
         <th>Название</th><th>Цена</th><th>Металл</th><th>Камень</th><th>Вес (г)</th><th>Артикул</th>
     </thead><tbody>`;
    products.forEach(p => {
        html += `<tr>
            <td>${p.name}</td>
            <td>${p.price.toLocaleString()} ₽</td>
            <td>${p.metal || '—'}</td>
            <td>${p.stone || '—'}</td>
            <td>${p.weight || '—'}</td>
            <td>${p.article || '—'}</td>
        </tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
}
async function renderUsersTable(users) {
    let html = `<h3>Пользователи</h3>
        <button id="addUserBtn" class="btn-primary mb-2">+ Добавить сотрудника</button>
        <div class="table-wrapper"><table class="table"><thead>
         <th>ID</th><th>Логин</th><th>Имя</th><th>Роль</th><th>Действие</th>
     </thead><tbody>`;
    users.forEach(u => {
        html += `<tr>
            <td>${u.userId}</td>
            <td>${u.login}</td>
            <td>${u.fullName}</td>
            <td>${u.roleName}</td>
            <td>${u.userId !== currentUser.id ? `<button class="btn-sm btn-outline" onclick="window.deleteUser(${u.userId})">Удалить</button>` : '—'}</td>
        </tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
    document.getElementById('addUserBtn')?.addEventListener('click', async () => {
        const login = await window.showInputDialog('Логин сотрудника', 'Ювелирная мастерская Гранат');
        if (login) {
            const pwd = await window.showInputDialog('Пароль', 'Ювелирная мастерская Гранат');
            const name = await window.showInputDialog('Полное имя', 'Ювелирная мастерская Гранат');
            const role = await window.showInputDialog('Роль (manager/jeweler)', 'Ювелирная мастерская Гранат');
            if (login && pwd && name && role && (role === 'manager' || role === 'jeweler')) {
                try {
                    await apiFetch('/api/auth/register', { method: 'POST', body: JSON.stringify({ login, password: pwd, fullName: name, phone: '', email: '' }) });
                    window.showToast('Сотрудник добавлен');
                    renderAdminDashboard();
                } catch (e) { window.showToast('Ошибка добавления', true); }
            } else window.showToast('Некорректная роль', true);
        }
    });
}
async function renderMaterialsTable(materials) {
    let html = `<h3>Материалы</h3>
        <button id="addMaterialBtn" class="btn-primary mb-2">+ Добавить материал</button>
        <div class="table-wrapper"><table class="table"><thead>
         <th>Наименование</th><th>Количество</th><th>Цена/ед</th><th>Действие</th>
     </thead><tbody>`;
    materials.forEach(m => {
        html += `<tr>
            <td>${m.name}</td>
            <td>${m.quantityInStock} ${m.unit}</td>
            <td>${m.pricePerUnit}₽</td>
            <td><button class="btn-sm btn-outline" onclick="window.deleteMaterial(${m.materialId})">Удалить</button></td>
        </tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
    document.getElementById('addMaterialBtn')?.addEventListener('click', showAddMaterialForm);
}
function showAddMaterialForm() {
    const modalHtml = `
    <div class="modal fade" id="addMaterialModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Добавить материал</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Название материала</label>
              <input type="text" class="form-control" id="matName" placeholder="например: Серебро 925">
            </div>
            <div class="mb-3">
              <label class="form-label">Единица измерения</label>
              <input type="text" class="form-control" id="matUnit" placeholder="г, кар, шт">
            </div>
            <div class="mb-3">
              <label class="form-label">Цена за единицу</label>
              <input type="number" step="any" class="form-control" id="matPrice" placeholder="например: 120 или 3500.50">
            </div>
            <div class="mb-3">
              <label class="form-label">Количество на складе</label>
              <input type="number" step="any" class="form-control" id="matQty" placeholder="например: 1000">
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Отмена</button>
            <button type="button" class="btn btn-primary" id="saveMaterialBtn">Добавить</button>
          </div>
        </div>
      </div>
    </div>`;

    const oldModal = document.getElementById('addMaterialModal');
    if (oldModal) oldModal.remove();

    document.body.insertAdjacentHTML('beforeend', modalHtml);

    const modalElement = document.getElementById('addMaterialModal');
    const modal = new bootstrap.Modal(modalElement);
    modal.show();

    document.getElementById('saveMaterialBtn').addEventListener('click', async () => {
        const name = document.getElementById('matName').value.trim();
        const unit = document.getElementById('matUnit').value.trim();
        const price = parseFloat(document.getElementById('matPrice').value);
        const quantity = parseFloat(document.getElementById('matQty').value);

        if (!name) { window.showToast('Введите название материала', true); return; }
        if (!unit) { window.showToast('Введите единицу измерения', true); return; }
        if (isNaN(price)) { window.showToast('Цена должна быть числом', true); return; }
        if (isNaN(quantity)) { window.showToast('Количество должно быть числом', true); return; }

        try {
            await apiFetch('/api/materials', {
                method: 'POST',
                body: JSON.stringify({
                    name: name,
                    unit: unit,
                    pricePerUnit: price,
                    quantityInStock: quantity,
                    description: ''
                })
            });
            modal.hide();
            window.showToast('Материал добавлен');
            modalElement.addEventListener('hidden.bs.modal', () => modalElement.remove());
            renderAdminDashboard();
        } catch (e) {
            window.showToast('Ошибка добавления материала: ' + e.message, true);
        }
    });

    modalElement.addEventListener('hidden.bs.modal', () => {
        modalElement.remove();
    });
}

// МЕНЕДЖЕР-ПАНЕЛЬ ПОСЛЕ ВХОДА 
async function renderManagerDashboard() {
    let orders = [], materials = [];
    try { orders = await apiFetch('/api/orders'); } catch (e) { console.warn(e); }
    try { materials = await apiFetch('/api/materials'); } catch (e) { console.warn(e); }
    let html = `<div class="dashboard">
        <h2 class="mb-3">Панель менеджера</h2>
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
        document.getElementById('managerContent').innerHTML = `<div class="table-wrapper"><table class="table"><thead>
             <th>Наименование</th><th>Количество</th><th>Цена/ед</th>
         </thead><tbody>${materials.map(m => `<tr><td>${m.name}</td><td>${m.quantityInStock} ${m.unit}</td><td>${m.pricePerUnit}₽</td></tr>`).join('')}</tbody></table></div><button class="btn-outline mt-2" onclick="renderManagerDashboard()">← Назад</button>`;
    };
    document.getElementById('managerBackCatalog').onclick = () => { isOnMainSite = true; renderClientCatalog(); };
    document.getElementById('managerContent').innerHTML = await renderOrdersTable(orders);
}

// ЮВЕЛИР-ПАНЕЛЬ ПОСЛЕ ВХОДА
async function renderJewelerDashboard() {
    let orders = [], materials = [];
    try { orders = await apiFetch('/api/orders'); } catch (e) { console.warn(e); }
    try { materials = await apiFetch('/api/materials'); } catch (e) { console.warn(e); }
    let html = `<div class="dashboard">
        <h2 class="mb-3"> Панель ювелира</h2>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <button id="jewelerOrdersBtn" class="btn-primary glass-btn">Мои заказы</button>
            <button id="jewelerMaterialsBtn" class="btn-outline glass-btn">Материалы</button>
            <button id="jewelerBackCatalog" class="btn-outline glass-btn">На сайт</button>
        </div>
        <div id="jewelerContent"></div>
    </div>`;
    dynamicContent.innerHTML = html;
    const renderOrders = async () => {
        const myOrders = orders.filter(o => o.jewelerId === currentUser.id);
        document.getElementById('jewelerContent').innerHTML = await renderOrdersTable(myOrders);
    };
    document.getElementById('jewelerOrdersBtn').onclick = renderOrders;
    document.getElementById('jewelerMaterialsBtn').onclick = () => {
        document.getElementById('jewelerContent').innerHTML = `<div class="table-wrapper"><table class="table"><thead>
             <th>Наименование</th><th>Количество</th><th>Цена/ед</th>
         </thead><tbody>${materials.map(m => `<tr><td>${m.name}</td><td>${m.quantityInStock} ${m.unit}</td><td>${m.pricePerUnit}₽</td></tr>`).join('')}</tbody></table></div><button class="btn-outline mt-2" onclick="renderJewelerDashboard()">← Назад</button>`;
    };
    document.getElementById('jewelerBackCatalog').onclick = () => { isOnMainSite = true; renderClientCatalog(); };
    renderOrders();
}

// КАТАЛОГ (ГЛАВНАЯ СТРАНИЦА) 
function renderClientCatalog() {
    const sorted = [...products];
    if (sortField === 'name') sorted.sort((a, b) => sortOrder === 'asc' ? a.name.localeCompare(b.name) : b.name.localeCompare(a.name));
    else sorted.sort((a, b) => sortOrder === 'asc' ? a.price - b.price : b.price - a.price);

    let html = `<h2 class="catalog-title"> Каталог ювелирных украшений</h2>`;
    if (currentUser && currentUser.role === 'client') {
        html += `<div class="action-bar">
            <button id="repairActionBtn" class="btn-outline glass-btn"> Услуги ремонта</button>
        </div>`;
    } else if (!currentUser) {
        html += `<div class="action-bar">
            <p class="text-muted">Для покупки войдите как клиент</p>
        </div>`;
    }
    if (isOnMainSite && currentUser && currentUser.role !== 'client') {
        html += `<div class="text-start mb-3"><button id="backToPanelFromMain" class="btn-back-panel"><i class="fas fa-arrow-left"></i> Вернуться на панель</button></div>`;
    }
    html += `<div class="products-grid" id="catalogGrid">`;
    sorted.forEach(p => {
        const materialText = p.metal ? p.metal : 'Серебро 925';
        const weightText = p.weight ? `${p.weight} г` : '—';
        html += `<div class="product-card animate-on-scroll" data-id="${p.productId}">
            <img src="${p.imageUrl}" class="product-img" onerror="this.src='https://placehold.co/300x300'">
            <div class="product-info">
                <div class="fw-semibold">${p.name}</div>
                <div class="product-price">${p.price.toLocaleString()} ₽</div>
                <div class="product-details">${materialText} • ${weightText}</div>
                ${currentUser?.role === 'client' ? `<button class="btn-primary mt-2" onclick="event.stopPropagation(); window.addToCart(${p.productId})">В корзину</button>` : ''}
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
    if (repairBtn) repairBtn.addEventListener('click', () => {
        if (!currentUser || currentUser.role !== 'client') { window.showToast('Только для клиентов', true); showAuthModal(); return; }
        showRepairCatalog();
    });
    const backBtn = document.getElementById('backToPanelFromMain');
    if (backBtn) backBtn.addEventListener('click', () => {
        isOnMainSite = false;
        if (currentUser.role === 'admin') renderAdminDashboard();
        else if (currentUser.role === 'manager') renderManagerDashboard();
        else if (currentUser.role === 'jeweler') renderJewelerDashboard();
    });
    initScrollAnimation();
}
function showRepairCatalog() {
    let html = `<div class="dashboard"><h2>Услуги ремонта</h2><div class="list-group gap-2">`;
    repairOptions.forEach(r => {
        html += `<div class="list-group-item glass-card d-flex justify-content-between align-items-center">
            <div><strong>${r.name}</strong><br>${r.desc}<br>${r.price}₽</div>
            <button class="btn-primary" onclick="window.addRepairToCart(${r.id}); renderClientCatalog();">В корзину</button>
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
        <div><strong>Металл:</strong> ${p.metal || '—'}</div>
        <div><strong>Камень:</strong> ${p.stone || '—'}</div>
        <div><strong>Вес:</strong> ${p.weight ? p.weight + ' г' : '—'}</div>
        <div><strong>Артикул:</strong> ${p.article || '—'}</div>
        <div class="d-flex justify-content-between mt-3">
            <span class="h4" style="color:var(--accent)">${p.price.toLocaleString()}₽</span>
            ${currentUser?.role === 'client' ? `<button class="btn-primary" onclick="window.addToCart(${p.productId}); bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();">В корзину</button>` : ''}
        </div>
    `;
    new bootstrap.Modal(document.getElementById('productModal')).show();
}

// ГЛАВНЫЙ РЕНДЕР (ВЫЗЫВАЕТСЯ ПРИ ОБНОВЛЕНИИ)
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

// АНИМАЦИЯ
function initScrollAnimation() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(e => { if (e.isIntersecting) { e.target.classList.add('animated'); observer.unobserve(e.target); } });
    }, { threshold: 0.1 });
    document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
}

// ЗАПУСК 
(async function () {
    await loadProductsFromAPI();
    await loadRepairsFromAPI();
    loadCart();
    updateCartBadge();
    await renderApp();
    cartBtn.addEventListener('click', () => { renderCartModal(); new bootstrap.Modal(document.getElementById('cartModal')).show(); });
    if (historyBtn) historyBtn.addEventListener('click', showOrderHistory);
})();