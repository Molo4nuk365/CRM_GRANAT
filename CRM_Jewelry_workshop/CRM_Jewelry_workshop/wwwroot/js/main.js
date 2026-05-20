// Глобальные переменные 
let currentUser = null;         // { id, fullName, role, token }
let cart = [];                  // { type, id, quantity }
let products = [];              // загружаются с API
let repairOptions = [];         // загружаются с API
let sortField = 'name';        // 'name' или 'price'
let sortOrder = 'asc';         // 'asc' или 'desc'

//  DOM элементы 
const themeToggle = document.getElementById('themeToggle');
const profileAvatar = document.getElementById('profileAvatar');
const cartBtn = document.getElementById('cartBtn');
const cartCountSpan = document.getElementById('cartCount');
const dynamicContent = document.getElementById('dynamicContent');
const toastMsg = document.getElementById('toastMsg');

// Утилиты
function showToast(msg, isError = false) {
    toastMsg.textContent = msg;
    toastMsg.style.background = isError ? '#d9534f' : 'var(--accent)';
    toastMsg.classList.add('show');
    setTimeout(() => toastMsg.classList.remove('show'), 2500);
}

// Сохранение/загрузка корзины
function saveCart() {
    localStorage.setItem('granat_cart', JSON.stringify(cart));
}
function loadCart() {
    const saved = localStorage.getItem('granat_cart');
    if (saved) cart = JSON.parse(saved);
    updateCartBadge();
}
function updateCartBadge() {
    const count = cart.reduce((s, i) => s + (i.quantity || 1), 0);
    cartCountSpan.innerText = count;
}

// API вызовы с токеном 
async function apiFetch(url, options = {})
{
    const headers = { 'Content-Type': 'application/json' };
    if (currentUser && currentUser.token) {
        headers['Authorization'] = `Bearer ${currentUser.token}`;
    }
    const res = await fetch(url, { ...options, headers });
    if (res.status === 401)
    {
        logout();
        showToast('Сессия истекла, войдите снова', true);
        throw new Error('Unauthorized');
    }
    if (!res.ok) {
        const err = await res.text();
        throw new Error(err);
    }
    return res.json();
}

// Загрузка продуктов и ремонтов с бэкенда
async function loadProductsFromAPI() {
    try {
        products = await apiFetch('/api/products');
    } catch (e) {
        console.warn('Ошибка загрузки продуктов, использую заглушку:', e);
        products = [ { productId: 1, name: "Кольцо «Гранатовый рассвет»", price: 18500, description: "Серебро 925, гранат 0.8 карат", material: "Серебро 925, гранат", weight: "3.2 г", article: "GR-101", imageUrl: "/images/кольцо.png"},
            { productId: 2, name: "Серьги «Лунный свет»", price: 12400, description: "Серебро 925, гранат 0.5 карат", material: "Серебро 925, гранат", weight: "4.5 г", article: "GR-102", imageUrl: "/images/серьги.png" },
            { productId: 3, name: "Подвеска «Капля росы»", price: 9800, description: "Серебро 925, гранат 2 карат", material: "Серебро 925, гранат", weight: "1.8 г", article: "GR-103", imageUrl: "/images/подвеска.jpg" },
            { productId: 4, name: "Браслет «Серебряная нить»", price: 23500, description: "Серебро 925", material: "Серебро 925", weight: "6.2 г", article: "GR-104", imageUrl: "/images/браслет.png" },
            { productId: 5, name: "Брошь «Гранат»", price: 15900, description: "Серебро 925, гранат 2 карат", material: "Серебро 925, гранат", weight: "5.1 г", article: "GR-105", imageUrl: "/images/брошь.jpg" } ];
    }
}
async function loadRepairsFromAPI() {
    try {
        repairOptions = await apiFetch('/api/repairoptions');
    } catch (e)
    {
        repairOptions = [ { id: 101, name: "Ремонт кольца", price: 3500, desc: "Пайка, полировка, изменение размера" },
            { id: 102, name: "Ремонт серёг", price: 2800, desc: "Замена замка, полировка" },
            { id: 103, name: "Ремонт цепочки", price: 2200, desc: "Пайка звеньев, замена замка" },
            { id: 104, name: "Чистка и полировка", price: 1200, desc: "Ультразвуковая чистка, полировка" } ];
    }
}

//  Тёмная тема 
if (localStorage.getItem('theme') === 'dark') document.body.classList.add('dark-theme');
themeToggle.addEventListener('click', () => {
    document.body.classList.toggle('dark-theme');
    localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
});

// Аватар и выход 
function updateProfileAvatar() {
    if (!currentUser) {
        profileAvatar.innerHTML = `<i class="fas fa-user-circle" style="font-size: 1.6rem; color: var(--text-secondary);"></i>`;
        profileAvatar.classList.add('default-icon');
        profileAvatar.onclick = () => showAuthModal();
        return;
    }
    let iconPath = '';
    switch (currentUser.role) {
        case 'admin': iconPath = '/icons/admin.png'; break;
        case 'manager': iconPath = '/icons/manager.png'; break;
        case 'jeweler': iconPath = '/icons/jeweler.png'; break;
        default: iconPath = '/icons/client.png';
    }
    profileAvatar.innerHTML = `<img src="${iconPath}" alt="${currentUser.role}" onerror="this.src='https://placehold.co/42x42'">`;
    profileAvatar.classList.remove('default-icon');
    profileAvatar.onclick = logout;
}
function logout() {
    currentUser = null;
    localStorage.removeItem('token');
    sessionStorage.removeItem('granat_user');
    cart = []; saveCart(); updateCartBadge();
    renderApp();
    showToast('Вы вышли');
}

// Авторизация
function showAuthModal() {
    const container = document.getElementById('authFormContainer');
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

    document.getElementById('doLoginBtn')?.addEventListener('click', async () =>
    {
        const login = document.getElementById('authLogin').value.trim();
        const pwd = document.getElementById('authPassword').value.trim();
        if (!login || !pwd)
        {
            showToast('Введите логин и пароль', true);
            return;
        }
        try {
            const res = await fetch('/api/auth/login',
                {
                method: 'POST',
                    headers:
                    {
                        'Content-Type': 'application/json'
                    },
                body: JSON.stringify({ login, password: pwd })
            });
            if (!res.ok) throw new Error('Неверный логин или пароль');
            const data = await res.json();
            currentUser =
            {
                id: data.userId,
                fullName: data.fullName,
                role: data.roleName,
                token: data.token
            };
            localStorage.setItem('token', data.token);
            sessionStorage.setItem('granat_user', JSON.stringify(currentUser));
            modal.hide();
            showToast(`Добро пожаловать, ${data.fullName}!`);
            await loadProductsFromAPI();
            await loadRepairsFromAPI();
            renderApp();
        } catch (err) {
            showToast(err.message, true);
        }
    });

    document.getElementById('doRegBtn')?.addEventListener('click', async () =>
    {
        const name = document.getElementById('regName').value.trim();
        const login = document.getElementById('regLogin').value.trim();
        const pwd = document.getElementById('regPassword').value.trim();
        const phone = document.getElementById('regPhone').value.trim();
        const address = document.getElementById('regAddress').value.trim();
        if (!name || !login || !pwd) {
            showToast('Имя, логин и пароль обязательны', true); return;
        }
        try {
            const res = await fetch('/api/auth/register',
                {
                method: 'POST',
                headers:
                    {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(
                        {
                    login, password: pwd, fullName: name, phone, address, email: ""
                })
            });
            if (!res.ok) {
                const text = await res.text();
                throw new Error(text || 'Ошибка регистрации');
            }
            showToast('Регистрация успешна! Теперь войдите.');
            const loginTabBtn = document.querySelector('#authTab button[data-bs-target="#loginTab"]');
            if (loginTabBtn) new bootstrap.Tab(loginTabBtn).show();
        } catch (err) {
            showToast(err.message, true);
        }
    });
}

// Сортировка продуктов
function sortProducts() {
    const sorted = [...products];
    if (sortField === 'name')
    {
        sorted.sort((a, b) => sortOrder === 'asc' ? a.name.localeCompare(b.name) : b.name.localeCompare(a.name));

    } else if (sortField === 'price')
    {
        sorted.sort((a, b) => sortOrder === 'asc' ? a.price - b.price : b.price - a.price);
    }
    return sorted;
}

function renderSortButtons()
{
    return `
        <div class="sort-bar">
            <button class="sort-btn ${sortField === 'name' && sortOrder === 'asc' ? 'active' : ''}" data-sort="name_asc">Название А→Я</button>
            <button class="sort-btn ${sortField === 'name' && sortOrder === 'desc' ? 'active' : ''}" data-sort="name_desc">Название Я→А</button>
            <button class="sort-btn ${sortField === 'price' && sortOrder === 'asc' ? 'active' : ''}" data-sort="price_asc">Цена ↑</button>
            <button class="sort-btn ${sortField === 'price' && sortOrder === 'desc' ? 'active' : ''}" data-sort="price_desc">Цена ↓</button>
        </div>
    `;
}

// Корзина
function addToCart(productId) {
    if (!currentUser || currentUser.role !== 'client')
    {
        showToast('Войдите как клиент', true);
        showAuthModal();
        return;
    }
    const existing = cart.find(item => item.type === 'product' && item.id === productId);
    if (existing) existing.quantity = (existing.quantity || 1) + 1;
    else cart.push(
        {
            type: 'product', id: productId, quantity: 1
        });
    saveCart();
    updateCartBadge();
    showToast('Товар добавлен в корзину');
}
function addRepairToCart(repairId) {
    if (!currentUser || currentUser.role !== 'client') {
        showToast('Войдите как клиент', true);
        showAuthModal();
        return;
    }
    if (!cart.find(item => item.type === 'repair' && item.id === repairId))
        cart.push({ type: 'repair', id: repairId, quantity: 1 });
    saveCart();
    updateCartBadge();
    showToast('Услуга ремонта добавлена');
}
function removeFromCart(index)
{
    cart.splice(index, 1);
    saveCart();
    updateCartBadge();
    if (document.getElementById('cartModal').classList.contains('show')) renderCartModal();
    showToast('Удалено');
}
function renderCartModal()

{
    const modalBody = document.getElementById('cartModalBody');
    if (!modalBody) return;
    if (cart.length === 0) {
        modalBody.innerHTML = `<p>Корзина пуста</p><button class="btn-primary" data-bs-dismiss="modal">Закрыть</button>`;
        return;
    }
    let total = 0;
    let itemsHtml = `<div class="list-group">`;
    cart.forEach((item, idx) => {
        let name, price;
        if (item.type === 'product') {
            const prod = products.find(p => p.productId === item.id);
            if (!prod) return;
            name = prod.name;
            price = prod.price;
            total += price * (item.quantity || 1);
            itemsHtml += `<div class="list-group-item d-flex justify-content-between align-items-center">${name} x ${item.quantity || 1} = ${(price * (item.quantity || 1)).toLocaleString()}₽ <button class="btn-sm btn-outline-danger" onclick="removeFromCart(${idx})">Удалить</button></div>`;
        } else {
            const rep = repairOptions.find(r => r.id === item.id);
            if (!rep) return;
            name = rep.name;
            price = rep.price;
            total += price;
            itemsHtml += `<div class="list-group-item d-flex justify-content-between align-items-center">${name} = ${price.toLocaleString()}₽ <button class="btn-sm btn-outline-danger" onclick="removeFromCart(${idx})">Удалить</button></div>`;
        }
    });
    itemsHtml += `</div><hr><div class="fw-bold">Итого: ${total.toLocaleString()} ₽</div>`;
    itemsHtml += `<button id="checkoutBtn" class="btn-primary mt-3 w-100">Оформить заказ</button>`;
    modalBody.innerHTML = itemsHtml;
    document.getElementById('checkoutBtn')?.addEventListener('click', () => checkoutOrder());
}
async function checkoutOrder() {
    if (!currentUser || currentUser.role !== 'client')
    {
        showToast('Войдите как клиент', true);
        return;
    }
    if (cart.length === 0) {
        showToast('Корзина пуста');
        return;
    }
    const items = cart.map(item => (
        {
            type: item.type, id: item.id, quantity: item.quantity || 1
        } ));
    try {
        await apiFetch('/api/orders/create',
        {
            method: 'POST',
            body: JSON.stringify({ items })
        });
        cart = [];
        saveCart();
        updateCartBadge();
        const modalEl = document.getElementById('cartModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();

        showToast('Заказ оформлен! Менеджер свяжется.');

        if (currentUser.role === 'client') renderClientDashboard();

    } catch (err)
    {
        showToast('Ошибка оформления: ' + err.message, true);
    }
}

//  Отображение каталога / ремонтов / дашбордов 
function renderClientCatalog() {
    const sortedProducts = sortProducts();
    let html = `<h2>Каталог изделий</h2>${renderSortButtons()}<div class="products-grid" id="catalogGrid">`;
    sortedProducts.forEach(p =>
    {
        html += `<div class="product-card animate-on-scroll" data-id="${p.productId}">
            <img src="${p.imageUrl}" class="product-img" onerror="this.src='https://placehold.co/300x300?text=${p.name}'">
            <div class="product-info">
                <div class="fw-semibold">${p.name}</div>
                <div class="product-price">${p.price.toLocaleString()} ₽</div>
                <button class="btn-sm btn-primary mt-2" onclick="addToCart(${p.productId})">В корзину</button>
            </div>
        </div>`;
    });
    html += `</div><button class="btn-outline mt-3" id="showRepairBtnClient">🛠️ Заказать ремонт</button>`;
    dynamicContent.innerHTML = html;
    document.querySelectorAll('.product-card').forEach(card =>
    {
        const id = parseInt(card.dataset.id);
        card.addEventListener('click', (e) =>
        {
            if (!e.target.classList.contains('btn-primary')) showProductModal(id);
        });
    });
    const btn = document.getElementById('showRepairBtnClient');
    if (btn) btn.onclick = () => showRepairCatalog();
    document.querySelectorAll('.sort-btn').forEach(btn =>
    {
        btn.addEventListener('click', (e) =>
        {
            const val = btn.dataset.sort;
            if (val === 'name_asc') { sortField = 'name'; sortOrder = 'asc'; }
            else if (val === 'name_desc') { sortField = 'name'; sortOrder = 'desc'; }
            else if (val === 'price_asc') { sortField = 'price'; sortOrder = 'asc'; }
            else if (val === 'price_desc') { sortField = 'price'; sortOrder = 'desc'; }
            renderClientCatalog();
        });
    });
    initScrollAnimation();
}

function showRepairCatalog() {
    let html = `<h2>Услуги ремонта</h2><div class="list-group">`;
    repairOptions.forEach(r => {
        html += `<div class="list-group-item animate-on-scroll">
            <div><strong>${r.name}</strong><br>${r.desc}<br>Цена: ${r.price}₽</div>
            <button class="btn-sm btn-primary mt-2" onclick="addRepairToCart(${r.id})">В корзину</button>
        </div>`;
    });
    html += `</div><button class="btn-outline mt-3" onclick="renderClientCatalog()">← Назад в каталог</button>`;
    dynamicContent.innerHTML = html;
    initScrollAnimation();
}
function showProductModal(id) {
    const p = products.find(p => p.productId === id);
    if (!p) return;
    const body = document.getElementById('productModalBody');
    body.innerHTML = `
        <div class="text-center"><img src="${p.imageUrl}" class="img-fluid mb-3" style="max-height:250px;object-fit:cover;border-radius:20px;" onerror="this.src='https://placehold.co/400x300?text=${p.name}'"></div>
        <h4>${p.name}</h4>
        <p>${p.description}</p>
        <hr><div class="row mb-2"><div class="col-6"><strong>Материал:</strong></div><div class="col-6">${p.material}</div></div>
        <div class="row mb-2"><div class="col-6"><strong>Вес:</strong></div><div class="col-6">${p.weight}</div></div>
        <div class="row mb-3"><div class="col-6"><strong>Артикул:</strong></div><div class="col-6">${p.article}</div></div>
        <div class="d-flex justify-content-between align-items-center"><span class="h4" style="color:var(--accent);">${p.price.toLocaleString()} ₽</span><button class="btn-primary" onclick="addToCart(${p.productId}); bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();">В корзину</button></div>
    `;
    new bootstrap.Modal(document.getElementById('productModal')).show();
}

// Дашборды (это панель данных по ролям) 
async function renderAdminDashboard()
{
    // Получаем данные с API (заказы, пользователи, материалы)
    let orders = [], users = [], materials = [];
    try
    {
        orders = await apiFetch('/api/orders/all');
        users = await apiFetch('/api/users');
        materials = await apiFetch('/api/materials');
    } catch (e) {
        console.warn(e);
    }
    const totalOrders = orders.length;
    const completed = orders.filter(o => o.statusOrder?.name === 'completed').length;
    const revenue = orders.filter(o => o.statusOrder?.name === 'completed').reduce((s, o) => s + o.totalCost, 0);
    let html = `<div class="dashboard"><h2>Админ панель</h2>
        <div class="stats-grid d-flex gap-3 mb-3"><div class="stat-card"><h3>${totalOrders}</h3><p>Заказов</p></div><div class="stat-card"><h3>${completed}</h3><p>Завершено</p></div><div class="stat-card"><h3>${revenue.toLocaleString()} ₽</h3><p>Выручка</p></div></div>
        <button id="adminOrdersBtn" class="btn-primary">Заказы</button>
        <button id="adminProductsBtn" class="btn-primary">Товары</button>
        <button id="adminMaterialsBtn" class="btn-outline">Материалы</button>
        <button id="adminUsersBtn" class="btn-outline">Пользователи</button>
        <button id="adminBackCatalog" class="btn-outline">На сайт</button>
        <div id="adminContent"></div>
    </div>`;

    dynamicContent.innerHTML = html;
    document.getElementById('adminOrdersBtn').onclick = () => renderOrdersTable(orders);
    document.getElementById('adminProductsBtn').onclick = () => renderProductsTable();
    document.getElementById('adminMaterialsBtn').onclick = () => renderMaterialsTable(materials);
    document.getElementById('adminUsersBtn').onclick = () => renderUsersTable(users);
    document.getElementById('adminBackCatalog').onclick = () => renderClientCatalog();
    renderOrdersTable(orders);
}
function renderOrdersTable(orders)
{
    let html = `<h3>Заказы</h3><div class="table-wrapper"><table class="table"><thead><tr><th>ID</th><th>Клиент</th><th>Статус</th><th>Сумма</th><th>Действие</th></tr></thead><tbody>`;
    orders.forEach(o => {
        html += `<tr><td>${o.orderId}</td><td>${o.client?.fullName || ''}</td><td><span class="status-badge status-${o.statusOrder?.name}">${o.statusOrder?.name}</span></td><td>${o.totalCost}₽</td><td><button class="btn-sm btn-outline" onclick="window.changeOrderStatus(${o.orderId})">Статус</button></td></tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
}
window.changeOrderStatus = async function (orderId) {
    let newStatus = prompt('Новый статус (new, in_progress, completed, cancelled)');
    if (newStatus) {
        await apiFetch(`/api/orders/${orderId}/status`, { method: 'PUT', body: JSON.stringify({ status: newStatus }) });
        showToast('Статус обновлён');
        renderApp();
    }
};
function renderProductsTable() {
    let html = `<h3>Товары</h3><div class="table-wrapper"><table class="table"><thead><tr><th>Название</th><th>Цена</th><th>Артикул</th></tr></thead><tbody>`;
    products.forEach(p => {
        html += `<tr><td>${p.name}</td><td>${p.price}₽</td><td>${p.article}</td></tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
}
function renderMaterialsTable(materials) {
    let html = `<h3>Материалы</h3><div class="table-wrapper"><table class="table"><thead><tr><th>Наименование</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>`;
    materials.forEach(m => {
        html += `<tr><td>${m.name}</td><td>${m.quantityInStock} ${m.unit}</td><td>${m.pricePerUnit}₽</td></tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
}
function renderUsersTable(users) {
    let html = `<h3>Пользователи</h3><div class="table-wrapper"><table class="table"><thead><tr><th>Логин</th><th>Имя</th><th>Роль</th></tr></thead><tbody>`;
    users.forEach(u => {
        html += `<tr><td>${u.login}</td><td>${u.fullName}</td><td>${u.roleName}</td></tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
}
function renderManagerDashboard() { /* аналогично админской, но без управления пользователями */ }
function renderJewelerDashboard() { /* список заказов ювелира */ }
function renderClientDashboard() {
    // показываем последние заказы клиента
    renderClientCatalog();
}

// Главный рендер (преобразование кода)
async function renderApp() {
    // Если нет текущего пользователя, пытаемся восстановить из sessionStorage
    const saved = sessionStorage.getItem('granat_user');
    if (saved && !currentUser) {
        currentUser = JSON.parse(saved);
        await loadProductsFromAPI();
        await loadRepairsFromAPI();
    }
    updateProfileAvatar();
    if (!currentUser)
    {
        dynamicContent.innerHTML = `<div class="products-grid" id="catalogGrid">${sortProducts().map(p => `<div class="product-card animate-on-scroll" data-id="${p.productId}"><img src="${p.imageUrl}" class="product-img" onerror="this.src='https://placehold.co/300x300?text=${p.name}'"><div class="product-info"><div class="fw-semibold">${p.name}</div><div class="product-price">${p.price.toLocaleString()} ₽</div></div></div>`).join('')}</div>`;
        document.querySelectorAll('.product-card').forEach(card => card.addEventListener('click', () => showProductModal(parseInt(card.dataset.id))));
        document.getElementById('scrollToCatalog')?.addEventListener('click', () => document.getElementById('catalogGrid').scrollIntoView({ behavior: 'smooth' }));
        initScrollAnimation();
        return;
    }
    switch (currentUser.role)
    {
        case 'admin': await renderAdminDashboard(); break;
        case 'manager': await renderManagerDashboard(); break;
        case 'jeweler': await renderJewelerDashboard(); break;
        default: renderClientDashboard();
    }
    initScrollAnimation();
}

function initScrollAnimation()
{
    const animated = document.querySelectorAll('.animate-on-scroll');
    const observer = new IntersectionObserver((entries) =>
    {
        entries.forEach(e =>
        {
            if (e.isIntersecting)
            {
                e.target.classList.add('animated'); observer.unobserve(e.target);
            }
        });
    },

        {
            threshold: 0.1
        });
    animated.forEach(el => observer.observe(el));
}

//  Старт приложения 
(async function init()
{
    await loadProductsFromAPI();
    await loadRepairsFromAPI();
    loadCart();
    updateCartBadge();
    await renderApp();
    cartBtn.addEventListener('click', () =>

    {
        renderCartModal();
        new bootstrap.Modal(document.getElementById('cartModal')).show();
    });
    window.addToCart = addToCart;
    window.addRepairToCart = addRepairToCart;
    window.removeFromCart = removeFromCart;
}) ();