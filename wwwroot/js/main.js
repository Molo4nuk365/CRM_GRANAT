
//  КОНФИГУРАЦИЯ API
const API_BASE = 'http://localhost:5150/api';
let authToken = localStorage.getItem('granat_token') || null;
let currentUser = null;
let cart = [];

//  ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
function showToast(msg, isError = false) {
    const toast = document.getElementById('toastMsg');
    if (!toast) return;
    toast.textContent = msg;
    toast.style.background = isError ? '#d9534f' : 'var(--accent)';
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 2500);
}

function getAuthHeaders() {
    return authToken ? { 'Authorization': `Bearer ${authToken}` } : {};
}

// ЗАГРУЗКА ДАННЫХ С СЕРВЕРА 
async function loadProducts() {
    try {
        const res = await fetch(`${API_BASE}/products`);
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки товаров:', e);
        return [];
    }
}

async function loadMyOrders() {
    try {
        const res = await fetch(`${API_BASE}/orders/my`, { headers: getAuthHeaders() });
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки заказов:', e);
        return [];
    }
}

async function loadAllOrders() {
    try {
        const res = await fetch(`${API_BASE}/orders/all`, { headers: getAuthHeaders() });
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки всех заказов:', e);
        return [];
    }
}

async function loadJewelerOrders() {
    try {
        const res = await fetch(`${API_BASE}/orders/jeweler`, { headers: getAuthHeaders() });
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки заказов ювелира:', e);
        return [];
    }
}

async function loadMaterials() {
    try {
        const res = await fetch(`${API_BASE}/materials`, { headers: getAuthHeaders() });
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки материалов:', e);
        return [];
    }
}

async function loadUsers() {
    try {
        const res = await fetch(`${API_BASE}/users`, { headers: getAuthHeaders() });
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки пользователей:', e);
        return [];
    }
}

async function loadSalesHistory(from, to) {
    try {
        let url = `${API_BASE}/sales?`;
        if (from) url += `from=${from}&`;
        if (to) url += `to=${to}`;
        const res = await fetch(url, { headers: getAuthHeaders() });
        if (res.ok) return await res.json();
        return { orders: [], total: 0 };
    } catch (e) {
        console.error('Ошибка загрузки истории продаж:', e);
        return { orders: [], total: 0 };
    }
}

async function loadRepairOptions() {
    try {
        const res = await fetch(`${API_BASE}/repairoptions`);
        if (res.ok) return await res.json();
        return [];
    } catch (e) {
        console.error('Ошибка загрузки услуг ремонта:', e);
        return [];
    }
}

// АВТОРИЗАЦИЯ И РЕГИСТРАЦИЯ 
async function doLogin(login, password) {
    try {
        const res = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ login, password })
        });
        if (res.ok) {
            const user = await res.json();
            authToken = user.token;
            localStorage.setItem('granat_token', authToken);
            currentUser = {
                id: user.id,
                login: user.login,
                role: user.role,
                fullName: user.fullName,
                phone: user.phone || '',
                address: user.address || ''
            };
            sessionStorage.setItem('granat_user', JSON.stringify(currentUser));
            showToast(`Добро пожаловать, ${user.fullName}!`);
            renderApp();
        } else {
            const error = await res.json();
            showToast(error.message || 'Неверный логин или пароль', true);
        }
    } catch (e) {
        showToast('Ошибка соединения с сервером', true);
    }
}

async function doRegister(name, login, password, phone, address) {
    try {
        const res = await fetch(`${API_BASE}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fullName: name, login, password, phone, address })
        });
        if (res.ok) {
            showToast('Регистрация успешна! Теперь войдите.');
            const loginTabBtn = document.querySelector('#authTab button[data-bs-target="#loginTab"]');
            if (loginTabBtn) new bootstrap.Tab(loginTabBtn).show();
        } else {
            const error = await res.json();
            showToast(error.message || 'Ошибка регистрации', true);
        }
    } catch (e) {
        showToast('Ошибка соединения с сервером', true);
    }
}

function showAuthModal() {
    const container = document.getElementById('authFormContainer');
    if (!container) return;
    container.innerHTML = `
        <div class="text-center mb-3">
            <i class="fas fa-user-circle" style="font-size: 3rem; color: var(--accent);"></i>
        </div>
        <ul class="nav nav-tabs justify-content-center border-0" id="authTab" role="tablist">
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
                <input id="regPhone" class="form-control mb-2" placeholder="Телефон">
                <input id="regAddress" class="form-control mb-2" placeholder="Адрес доставки">
                <button id="doRegBtn" class="btn-primary w-100">Зарегистрироваться</button>
            </div>
        </div>
    `;
    const modal = new bootstrap.Modal(document.getElementById('authModal'));
    modal.show();

    document.getElementById('doLoginBtn')?.addEventListener('click', () => {
        const login = document.getElementById('authLogin')?.value.trim() || '';
        const pwd = document.getElementById('authPassword')?.value.trim() || '';
        if (!login || !pwd) { showToast('Введите логин и пароль', true); return; }
        doLogin(login, pwd);
        modal.hide();
    });

    document.getElementById('doRegBtn')?.addEventListener('click', () => {
        const name = document.getElementById('regName')?.value.trim() || '';
        const login = document.getElementById('regLogin')?.value.trim() || '';
        const pwd = document.getElementById('regPassword')?.value.trim() || '';
        const phone = document.getElementById('regPhone')?.value.trim() || '';
        const address = document.getElementById('regAddress')?.value.trim() || '';
        if (!name || !login || !pwd) { showToast('Имя, логин и пароль обязательны', true); return; }
        doRegister(name, login, pwd, phone, address);
        modal.hide();
    });
}

function logout() {
    currentUser = null;
    authToken = null;
    localStorage.removeItem('granat_token');
    sessionStorage.removeItem('granat_user');
    cart = [];
    localStorage.removeItem('granat_cart');
    updateCartBadge();
    showToast('Вы вышли');
    renderApp();
}

// КОРЗИНА ==========
function addToCart(productId) {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); showAuthModal(); return; }
    const existing = cart.find(item => item.type === 'product' && item.productId === productId);
    if (existing) existing.quantity = (existing.quantity || 1) + 1;
    else cart.push({ type: 'product', productId, quantity: 1 });
    saveCart(); updateCartBadge(); showToast('Товар добавлен в корзину');
}

function addRepairToCart(repairId) {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); showAuthModal(); return; }
    if (!cart.find(item => item.type === 'repair' && item.repairId === repairId))
        cart.push({ type: 'repair', repairId, quantity: 1 });
    saveCart(); updateCartBadge(); showToast('Услуга ремонта добавлена');
}

function removeFromCart(index) {
    cart.splice(index, 1);
    saveCart(); updateCartBadge();
    if (document.getElementById('cartModal')?.classList.contains('show')) renderCartModal();
    showToast('Удалено');
}

function saveCart() { localStorage.setItem('granat_cart', JSON.stringify(cart)); }
function loadCart() { const saved = localStorage.getItem('granat_cart'); if (saved) cart = JSON.parse(saved); else cart = []; }
function updateCartBadge() {
    const count = cart.reduce((s, i) => s + (i.quantity || 1), 0);
    const badge = document.getElementById('cartCount');
    if (badge) badge.innerText = count.toString();
}

async function renderCartModal() {
    const modalBody = document.getElementById('cartModalBody');
    if (!modalBody) return;
    if (cart.length === 0) {
        modalBody.innerHTML = `<p>Корзина пуста</p><button class="btn-primary" data-bs-dismiss="modal">Закрыть</button>`;
        return;
    }
    const productsData = await loadProducts();
    const repairsData = await loadRepairOptions();
    let total = 0;
    let itemsHtml = `<div class="list-group">`;
    cart.forEach((item, idx) => {
        let name = '', price = 0;
        if (item.type === 'product') {
            const prod = productsData.find(p => p.id === item.productId);
            if (prod) { name = prod.name; price = prod.price; }
        } else {
            const rep = repairsData.find(r => r.id === item.repairId);
            if (rep) { name = rep.name; price = rep.price; }
        }
        const itemTotal = price * (item.quantity || 1);
        total += itemTotal;
        itemsHtml += `<div class="list-group-item d-flex justify-content-between align-items-center">${name} x ${item.quantity || 1} = ${itemTotal.toLocaleString()}₽ <button class="btn-sm btn-outline-danger" onclick="removeFromCart(${idx})">Удалить</button></div>`;
    });
    itemsHtml += `</div><hr><div class="fw-bold">Итого: ${total.toLocaleString()} ₽</div>`;
    itemsHtml += `<button id="checkoutBtn" class="btn-primary mt-3 w-100">Оформить заказ</button>`;
    modalBody.innerHTML = itemsHtml;
    document.getElementById('checkoutBtn')?.addEventListener('click', () => checkoutOrder());
}

async function checkoutOrder() {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); return; }
    if (cart.length === 0) { showToast('Корзина пуста'); return; }
    const items = cart.map(item => ({
        type: item.type,
        id: item.type === 'product' ? item.productId : item.repairId,
        quantity: item.quantity || 1
    }));
    try {
        const res = await fetch(`${API_BASE}/orders/create`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', ...getAuthHeaders() },
            body: JSON.stringify({ items })
        });
        if (res.ok) {
            cart = []; saveCart(); updateCartBadge();
            showToast('Заказ оформлен! Менеджер свяжется.');
            renderClientDashboard();
        } else {
            showToast('Ошибка оформления заказа', true);
        }
    } catch (e) {
        showToast('Ошибка соединения с сервером', true);
    }
}
 //УПРАВЛЕНИЕ ЗАКАЗАМИ
async function changeOrderStatus(orderId, newStatus) {
    try {
        const res = await fetch(`${API_BASE}/orders/${orderId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', ...getAuthHeaders() },
            body: JSON.stringify({ status: newStatus })
        });
        if (res.ok) {
            showToast('Статус заказа обновлён');
            renderApp();
        } else {
            showToast('Ошибка обновления статуса', true);
        }
    } catch (e) {
        showToast('Ошибка соединения с сервером', true);
    }
}

async function completeOrder(orderId) {
    await changeOrderStatus(orderId, 'completed');
}

async function payOrder(orderId) {
    try {
        const res = await fetch(`${API_BASE}/orders/${orderId}/pay`, {
            method: 'PUT',
            headers: getAuthHeaders()
        });
        if (res.ok) {
            showToast('Заказ оплачен');
            renderClientDashboard();
        } else {
            showToast('Ошибка оплаты', true);
        }
    } catch (e) {
        showToast('Ошибка соединения с сервером', true);
    }
}

//  ГЛОБАЛЬНЫЕ ПЕРЕМЕННЫЕ
let productsData = [];
let repairOptionsData = [];

//  МОДАЛЬНОЕ ОКНО ТОВАРА
function showProductModal(id) {
    const p = productsData.find(p => p.id === id);
    if (!p) return;
    const body = document.getElementById('productModalBody');
    if (!body) return;
    body.innerHTML = `
        <div class="text-center"><img src="${p.imageUrl || 'images/placeholder.jpg'}" class="img-fluid mb-3" style="max-height:250px;object-fit:cover;border-radius:20px;" onerror="this.src='https://placehold.co/400x300?text=${p.name}'"></div>
        <h4>${p.name}</h4>
        <p>${p.description}</p>
        <hr><div class="row mb-2"><div class="col-6"><strong>Материал:</strong></div><div class="col-6">${p.material}</div></div>
        <div class="row mb-2"><div class="col-6"><strong>Вес:</strong></div><div class="col-6">${p.weight}</div></div>
        <div class="row mb-3"><div class="col-6"><strong>Артикул:</strong></div><div class="col-6">${p.article}</div></div>
       <div class="d-flex justify-content-between align-items-center"><span class="h4" style="color:var(--accent);">${p.price.toLocaleString()} ₽</span><button class="btn-primary" onclick="addToCart(${p.id}); bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();">В корзину</button></div>
    `;
    new bootstrap.Modal(document.getElementById('productModal')).show();
}

// ========== ДАШБОРД АДМИНИСТРАТОРА ==========
async function renderAdminDashboard() {
    const orders = await loadAllOrders();
    const completed = orders.filter(o => o.status === 'completed').length;
    const revenue = orders.filter(o => o.status === 'completed').reduce((s, o) => s + o.total, 0);
    let html = `<div class="dashboard"><h2>Админ панель</h2><div class="stats-grid"><div class="stat-card animate-on-scroll"><h3>${orders.length}</h3><p>Заказов</p></div><div class="stat-card animate-on-scroll"><h3>${completed}</h3><p>Завершено</p></div><div class="stat-card animate-on-scroll"><h3>${revenue.toLocaleString()} ₽</h3><p>Выручка</p></div></div><div><button id="adminOrdersBtn" class="btn-primary">Заказы</button> <button id="adminProductsBtn" class="btn-primary">Товары</button> <button id="adminMaterialsBtn" class="btn-outline">Материалы</button> <button id="adminSalesBtn" class="btn-outline">История продаж</button> <button id="adminUsersBtn" class="btn-outline">Пользователи</button> <button id="adminBackCatalog" class="btn-outline">На сайт</button></div><div id="adminContent"></div></div>`;
    const app = document.getElementById('app');
    if (app) app.innerHTML = html;
    document.getElementById('adminOrdersBtn')?.addEventListener('click', () => renderOrdersTable(orders));
    document.getElementById('adminProductsBtn')?.addEventListener('click', () => renderProductsTableForAdmin());
    document.getElementById('adminMaterialsBtn')?.addEventListener('click', () => renderMaterialsTable());
    document.getElementById('adminSalesBtn')?.addEventListener('click', () => renderSalesHistory());
    document.getElementById('adminUsersBtn')?.addEventListener('click', () => renderUsersTable());
    document.getElementById('adminBackCatalog')?.addEventListener('click', () => { currentUser = null; sessionStorage.removeItem('granat_user'); renderApp(); });
    renderOrdersTable(orders);
}

function renderOrdersTable(orders) {
    const container = document.getElementById('adminContent');
    if (!container) return;
    let html = `<h3>Заказы</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Клиент</th><th>Тип</th><th>Статус</th><th>Сумма</th><th>Оплата</th><th>Действие</th></tr></thead><tbody>${orders.map(o => `<tr>
        <td>${o.id}</td>
        <td>${o.clientName}</td>
        <td>${o.type}</td>
        <td><span class="status-badge status-${o.status}">${o.status}</span></td>
        <td>${o.total}₽</td>
        <td>${o.paymentStatus}</td>
        <td><button class="btn-sm btn-outline" onclick="window.changeOrderStatus(${o.id}, prompt('Новый статус'))">Статус</button></td>
    </tr>`).join('')}</tbody></table></div>`;
    container.innerHTML = html;
}

function renderProductsTableForAdmin() {
    const container = document.getElementById('adminContent');
    if (!container) return;
    let html = `<h3>Товары</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>Изображение</th><th>Название</th><th>Цена</th><th>Артикул</th><th>Материал</th></tr></thead><tbody>${productsData.map(p => `<tr>
        <td><img src="${p.imageUrl || 'images/placeholder.jpg'}" style="width:50px;height:50px;object-fit:cover;border-radius:12px;"></td>
        <td>${p.name}</td>
        <td>${p.price.toLocaleString()} ₽</td>
        <td>${p.article}</td>
        <td>${p.material}</td>
    </tr>`).join('')}</tbody></td></div>`;
    container.innerHTML = html;
}

async function renderMaterialsTable() {
    const container = document.getElementById('adminContent');
    if (!container) return;
    const mats = await loadMaterials();
    let html = `<h3>Материалы</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>Материал</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>${mats.map(m => `<tr>
        <td>${m.name}</td>
        <td>${m.quantity} ${m.unit}</td>
        <td>${m.pricePerUnit}₽</td>
    </tr>`).join('')}</tbody></table></div>`;
    container.innerHTML = html;
}

async function renderSalesHistory() {
    const container = document.getElementById('adminContent');
    if (!container) return;
    let html = `<h3>История продаж</h3><div class="filter-bar"><input type="date" id="salesFrom"><input type="date" id="salesTo"><button id="applySalesFilter" class="btn-primary">Фильтр</button></div><div id="salesTable"></div>`;
    container.innerHTML = html;
    const renderFiltered = async () => {
        const from = document.getElementById('salesFrom')?.value || '';
        const to = document.getElementById('salesTo')?.value || '';
        const data = await loadSalesHistory(from, to);
        let table = `<div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Клиент</th><th>Сумма</th><th>Дата</th></tr></thead><tbody>${data.orders.map((o) => `<tr>
            <td>${o.id}</td>
            <td>${o.clientName}</td>
            <td>${o.total}₽</td>
            <td>${o.completedDate || '—'}</td>
        </tr>`).join('')}</tbody></table></div><p>Итого: ${data.total.toLocaleString()}₽</p>`;
        const salesTable = document.getElementById('salesTable');
        if (salesTable) salesTable.innerHTML = table;
    };
    document.getElementById('applySalesFilter')?.addEventListener('click', renderFiltered);
    renderFiltered();
}

async function renderUsersTable() {
    const container = document.getElementById('adminContent');
    if (!container) return;
    const usersList = await loadUsers();
    let html = `<h3>Пользователи</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead></tr><th>ID</th><th>Логин</th><th>Имя</th><th>Роль</th></thead><tbody>${usersList.map(u => `<tr>
        <td>${u.id}</td>
        <td>${u.login}</td>
        <td>${u.fullName}</td>
        <td>${u.role}</td>
    </tr>`).join('')}</tbody></table></div>`;
    container.innerHTML = html;
}

// ========== ДАШБОРД МЕНЕДЖЕРА ==========
async function renderManagerDashboard() {
    const orders = await loadAllOrders();
    let html = `<div class="dashboard"><h2>Панель менеджера</h2><div><button id="managerMaterialsBtn" class="btn-outline">Материалы</button> <button id="managerBackCatalog" class="btn-outline">На сайт</button></div><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Клиент</th><th>Тип</th><th>Статус</th><th>Сумма</th><th>Действие</th></tr></thead><tbody>${orders.map(o => `<tr>
        <td>${o.id}</td>
        <td>${o.clientName}</td>
        <td>${o.type}</td>
        <td><span class="status-badge status-${o.status}">${o.status}</span></td>
        <td>${o.total}₽</td>
        <td><button class="btn-sm btn-outline" onclick="window.changeOrderStatus(${o.id}, prompt('Новый статус'))">Изменить статус</button></td>
    </tr>`).join('')}</tbody></table></div></div>`;
    const app = document.getElementById('app');
    if (app) app.innerHTML = html;
    document.getElementById('managerMaterialsBtn')?.addEventListener('click', async () => {
        const mats = await loadMaterials();
        if (app) app.innerHTML = `<div class="dashboard"><h2>Материалы</h2><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>Материал</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>${mats.map(m => `<td>
            <td>${m.name}</td>
            <td>${m.quantity} ${m.unit}</td>
            <td>${m.pricePerUnit}₽</td>
        </table>`).join('')}</tbody></table></div><button class="btn-outline mt-2" onclick="renderManagerDashboard()">← Назад</button></div>`;
    });
    document.getElementById('managerBackCatalog')?.addEventListener('click', () => { currentUser = null; sessionStorage.removeItem('granat_user'); renderApp(); });
}

// ========== ДАШБОРД ЮВЕЛИРА ==========
async function renderJewelerDashboard() {
    const orders = await loadJewelerOrders();
    let html = `<div class="dashboard"><h2>Мои заказы (ювелир)</h2><div><button id="jewelerMaterialsBtn" class="btn-outline">Материалы</button> <button id="jewelerBackCatalog" class="btn-outline">На сайт</button></div><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Клиент</th><th>Тип</th><th>Статус</th><th>Сумма</th><th>Действие</th></tr></thead><tbody>${orders.map(o => `<td>
        <td>${o.id}</td>
        <td>${o.clientName}</td>
        <td>${o.type}</td>
        <td><span class="status-badge status-${o.status}">${o.status}</span></td>
        <td>${o.total}₽</td>
        <td>${o.status !== 'completed' ? `<button class="btn-sm btn-primary" onclick="window.completeOrder(${o.id})">Завершить</button>` : '—'}</td>
    </tr>`).join('')}</tbody><table></div></div>`;
    const app = document.getElementById('app');
    if (app) app.innerHTML = html;
    document.getElementById('jewelerMaterialsBtn')?.addEventListener('click', async () => {
        const mats = await loadMaterials();
        if (app) app.innerHTML = `<div class="dashboard"><h2>Материалы</h2><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>Материал</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>${mats.map(m => `<tr>
            <td>${m.name}</td>
            <td>${m.quantity} ${m.unit}</td>
            <td>${m.pricePerUnit}₽</td>
        </tr>`).join('')}</tbody></table></div><button class="btn-outline mt-2" onclick="renderJewelerDashboard()">← Назад</button></div>`;
    });
    document.getElementById('jewelerBackCatalog')?.addEventListener('click', () => { currentUser = null; sessionStorage.removeItem('granat_user'); renderApp(); });
}

// // ДАШБОРД КЛИЕНТА 
async function renderClientDashboard() {
    const myOrders = await loadMyOrders();
    let ordersHtml = `<h3>Мои заказы</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Тип</th><th>Наименование</th><th>Статус</th><th>Сумма</th><th>Оплата</th><th>Действие</th></tr></thead><tbody>`;
    for (const o of myOrders) {
        let name = '';
        if (o.type === 'product') {
            const p = productsData.find(p => p.id === o.productId);
            name = p ? p.name : 'Товар';
        } else {
            const r = repairOptionsData.find(r => r.id === o.repairId);
            name = r ? r.name : 'Ремонт';
        }
        ordersHtml += `<tr>
            <td>${o.id}</td>
            <td>${o.type === 'product' ? 'Изделие' : 'Ремонт'}</td>
            <td>${name}</td>
            <td><span class="status-badge status-${o.status}">${o.status}</span></td>
            <td>${o.total}₽</td>
            <td>${o.paymentStatus === 'paid' ? 'Оплачен' : 'Не оплачен'}</td>
            <td>${o.paymentStatus !== 'paid' ? `<button class="btn-sm btn-primary" onclick="payOrder(${o.id})">Оплатить</button>` : '—'}</td>
        </tr>`;
    }
    ordersHtml += `</tbody></table></div>`;
    const app = document.getElementById('app');
    if (app) app.innerHTML = `<div class="dashboard"><h2>Личный кабинет</h2><div class="mb-3"><strong>${currentUser.fullName}</strong><br>${currentUser.phone} ${currentUser.address}</div><div class="mb-4"><button class="btn-primary" id="showRepairBtn">➕ Заказать ремонт</button> <button class="btn-primary" id="showCatalogBtn">🛍️ Каталог изделий</button></div>${ordersHtml}</div>`;
    document.getElementById('showRepairBtn')?.addEventListener('click', () => showRepairCatalog());
    document.getElementById('showCatalogBtn')?.addEventListener('click', () => showProductCatalog());
}

function showProductCatalog() {
    const app = document.getElementById('app');
    if (!app) return;
    app.innerHTML = `<div class="dashboard"><h2>Каталог изделий</h2><div class="products-grid">${productsData.map(p => `<div class="product-card animate-on-scroll" data-id="${p.id}"><img src="${p.imageUrl || 'images/placeholder.jpg'}" class="product-img" onerror="this.src='https://placehold.co/300x300?text=${p.name}'"><div class="product-info"><div class="product-name">${p.name}</div><div class="product-price">${p.price.toLocaleString()} ₽</div><button class="btn-sm btn-primary mt-2" onclick="addToCart(${p.id})">В корзину</button></div></div>`).join('')}</div><button class="btn-outline mt-3" onclick="renderClientDashboard()">← Назад</button></div>`;
    document.querySelectorAll('.product-card').forEach(card => {
        const id = parseInt(card.getAttribute('data-id') || '0');
        card.addEventListener('click', (e) => {
            if (!(e.target as HTMLElement).classList.contains('btn-primary')) showProductModal(id);
        });
    });
    initScrollAnimation();
}

function showRepairCatalog() {
    const app = document.getElementById('app');
    if (!app) return;
    app.innerHTML = `<div class="dashboard"><h2>Услуги ремонта</h2><div class="list-group">${repairOptionsData.map(r => `<div class="list-group-item animate-on-scroll"><div><strong>${r.name}</strong><br>${r.description}<br>Цена: ${r.price}₽</div><button class="btn-sm btn-primary mt-2" onclick="addRepairToCart(${r.id})">В корзину</button></div>`).join('')}</div><button class="btn-outline mt-3" onclick="renderClientDashboard()">← Назад</button></div>`;
    initScrollAnimation();
}

// //АНИМАЦИЯ ПРИ СКРОЛЛЕ
function initScrollAnimation() {
    const animatedElements = document.querySelectorAll('.animate-on-scroll');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => { if (entry.isIntersecting) { entry.target.classList.add('animated'); observer.unobserve(entry.target); } });
    }, { threshold: 0.1 });
    animatedElements.forEach(el => observer.observe(el));
}

// // АВАТАР ПОЛЬЗОВАТЕЛЯ
function updateProfileAvatar() {
    const avatarDiv = document.getElementById('profileAvatar');
    if (!avatarDiv) return;
    if (!currentUser) {
        avatarDiv.innerHTML = `<i class="fas fa-user-circle"></i>`;
        avatarDiv.onclick = () => showAuthModal();
        return;
    }
    let iconPath = '';
    switch (currentUser.role) {
        case 'admin': iconPath = 'icons/admin.png'; break;
        case 'manager': iconPath = 'icons/manager.png'; break;
        case 'jeweler': iconPath = 'icons/jeweler.png'; break;
        default: iconPath = 'icons/client.png';
    }
    avatarDiv.innerHTML = `<img src="${iconPath}" alt="${currentUser.role}" onerror="this.src='https://placehold.co/42x42'">`;
    avatarDiv.onclick = logout;
}

// ===== ТЕМА ==========
const themeToggle = document.getElementById('themeToggle');
if (localStorage.getItem('theme') === 'dark') document.body.classList.add('dark-theme');
themeToggle.addEventListener('click', () => {
    document.body.classList.toggle('dark-theme');
    localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
});

// // ОСНОВНОЙ РЕНДЕР 
async function renderApp() {
    const saved = sessionStorage.getItem('granat_user');
    if (saved && !currentUser) {
        currentUser = JSON.parse(saved);
        authToken = localStorage.getItem('granat_token');
    }
    updateProfileAvatar();

    // Загружаем данные
    productsData = await loadProducts();
    repairOptionsData = await loadRepairOptions();

    if (!currentUser) {
        // Неавторизованный пользователь – показываем каталог
        const app = document.getElementById('app');
        if (app) {
            app.innerHTML = `
                <div class="products-grid" id="catalogGrid">
                    ${productsData.map(p => `<div class="product-card animate-on-scroll" data-id="${p.id}"><img src="${p.imageUrl || 'images/placeholder.jpg'}" class="product-img" onerror="this.src='https://placehold.co/300x300?text=${p.name}'"><div class="product-info"><div class="product-name">${p.name}</div><div class="product-price">${p.price.toLocaleString()} ₽</div></div></div>`).join('')}
                </div>
            `;
            document.querySelectorAll('.product-card').forEach(card => {
                card.addEventListener('click', () => showProductModal(parseInt(card.getAttribute('data-id') || '0')));
            });
            document.getElementById('scrollToCatalog')?.addEventListener('click', () => {
                const catalog = document.getElementById('catalogGrid');
                if (catalog) catalog.scrollIntoView({ behavior: 'smooth' });
            });
        }
        initScrollAnimation();
        return;
    }

    //Авторизованный пользователь – показываем дашборд в зависимости от роли
    if (currentUser.role === 'admin') await renderAdminDashboard();
    else if (currentUser.role === 'manager') await renderManagerDashboard();
    else if (currentUser.role === 'jeweler') await renderJewelerDashboard();
    else await renderClientDashboard();
    initScrollAnimation();
}

// //ЗАПУСК
loadCart();
updateCartBadge();
renderApp();

// Глобальные функции для вызова из HTML
window.addToCart = addToCart;
window.addRepairToCart = addRepairToCart;
window.removeFromCart = removeFromCart;
window.changeOrderStatus = changeOrderStatus;
window.completeOrder = completeOrder;
window.payOrder = payOrder;