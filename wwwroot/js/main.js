
// ПОЛНАЯ ЛОГИКА CRM: авторизация, корзина, заказы, ремонт,
// роли (admin, manager, jeweler, client), управление материалами,
// история продаж, тёмная/светлая тема.
// ============================================================

// ========== 1. ДАННЫЕ  ==========
let products = [
    { id: 1, name: "Кольцо «Гранатовый рассвет»", price: 18500, desc: "Серебро 925, гранат 0.8 карат", material: "Серебро 925, гранат", weight: "3.2 г", article: "GR-101", image: "images/кольцо.png" },
    { id: 2, name: "Серьги «Лунный свет»", price: 12400, desc: "Серебро 925, гранат 0.5 карат", material: "Серебро 925, гранат", weight: "4.5 г", article: "GR-102", image: "images/серьги.png" },
    { id: 3, name: "Подвеска «Капля росы»", price: 9800, desc: "Серебро 925, гранат 1 карат", material: "Серебро 925, гранат", weight: "1.8 г", article: "GR-103", image: "images/подвеска.jpg" },
    { id: 4, name: "Браслет «Золотая нить»", price: 23500, desc: "Серебро 925, гранат", material: "Серебро 925, гранат", weight: "6.2 г", article: "GR-104", image: "images/браслет.jpg" },
    { id: 5, name: "Брошь «Гранат»", price: 15900, desc: "Серебро 925, гранат 2 карат", material: "Серебро 925, гранат", weight: "5.1 г", article: "GR-105", image: "images/брошь.jpg" }
];

let repairOptions = [
    { id: 101, name: "Ремонт кольца", price: 3500, desc: "Пайка, полировка, изменение размера" },
    { id: 102, name: "Ремонт серёг", price: 2800, desc: "Замена замка, полировка" },
    { id: 103, name: "Ремонт цепочки", price: 2200, desc: "Пайка звеньев, замена замка" },
    { id: 104, name: "Чистка и полировка", price: 1200, desc: "Ультразвуковая чистка, полировка" }
];

let users = [
    { id: 1, login: 'admin', password: 'admin123', role: 'admin', fullName: 'Администратор', phone: '', address: '' },
    { id: 2, login: 'manager', password: 'manager123', role: 'manager', fullName: 'Анна Иванова', phone: '', address: '' },
    { id: 3, login: 'jeweler', password: 'jeweler123', role: 'jeweler', fullName: 'Сергей Петров', phone: '', address: '' },
    { id: 4, login: 'client', password: 'client123', role: 'client', fullName: 'Клиент Тестов', phone: '+7 (999) 123-4567', address: 'Москва' }
];

let materials = [
    { id: 1, name: 'Серебро 925', quantity: 980, unit: 'г', pricePerUnit: 80 },
    { id: 2, name: 'Гранат', quantity: 150, unit: 'кар', pricePerUnit: 1200 }
];

let orders = [
    { id: 1, clientId: 4, clientName: 'Клиент Тестов', type: 'product', productId: 1, status: 'completed', managerId: 2, jewelerId: 3, deadline: '2026-05-20', total: 18500, completedDate: '2026-05-18', paymentStatus: 'paid' },
    { id: 2, clientId: 4, clientName: 'Клиент Тестов', type: 'product', productId: 2, status: 'completed', managerId: 2, jewelerId: 3, deadline: '2026-05-25', total: 12400, completedDate: '2026-05-22', paymentStatus: 'paid' },
    { id: 3, clientId: 4, clientName: 'Клиент Тестов', type: 'repair', repairId: 101, status: 'in_progress', managerId: 2, jewelerId: 3, deadline: '2026-06-01', total: 3500, completedDate: null, paymentStatus: 'pending' }
];

let currentUser = null;
let cart = [];

// ========== 2. ЗАГРУЗКА / СОХРАНЕНИЕ В LOCALSTORAGE ==========
function loadData() {
    const u = localStorage.getItem('granat_users');
    const m = localStorage.getItem('granat_materials');
    const o = localStorage.getItem('granat_orders');
    if (u) users = JSON.parse(u);
    if (m) materials = JSON.parse(m);
    if (o) orders = JSON.parse(o);
}
function saveData() {
    localStorage.setItem('granat_users', JSON.stringify(users));
    localStorage.setItem('granat_materials', JSON.stringify(materials));
    localStorage.setItem('granat_orders', JSON.stringify(orders));
}
loadData();
function persist() { saveData(); renderApp(); }

// ========== 3. ТЕМА (СВЕТЛАЯ / ТЁМНАЯ) ==========
const themeToggle = document.getElementById('themeToggle');
if (localStorage.getItem('theme') === 'dark') document.body.classList.add('dark-theme');
themeToggle.addEventListener('click', () => {
    document.body.classList.toggle('dark-theme');
    localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
});

function showToast(msg, isError = false) {
    const toast = document.getElementById('toastMsg');
    toast.textContent = msg;
    toast.style.background = isError ? '#d9534f' : 'var(--accent)';
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 2500);
}

// ========== 4. КАРУСЕЛЬ ТОВАРОВ ==========
function renderCarousel() {
    const carouselInner = document.getElementById('carouselInner');
    if (!carouselInner) return;
    carouselInner.innerHTML = products.map((p, idx) => `
        <div class="carousel-item ${idx === 0 ? 'active' : ''}">
            <img src="${p.image}" class="d-block w-100 carousel-img" alt="${p.name}" onerror="this.src='https://placehold.co/600x400?text=${p.name}'">
            <div class="carousel-caption d-none d-md-block">
                <h5>${p.name}</h5>
                <p>${p.price.toLocaleString()} ₽</p>
                <button class="btn btn-sm btn-primary" onclick="quickAddToCart(${p.id})">В корзину</button>
            </div>
        </div>
    `).join('');
}
window.quickAddToCart = function (productId) { addToCart(productId); };

// ========== 5. КОРЗИНА ==========
function addToCart(productId) {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); showAuthModal(); return; }
    const existing = cart.find(item => item.type === 'product' && item.productId === productId);
    if (existing) existing.quantity = (existing.quantity || 1) + 1;
    else cart.push({ type: 'product', productId, quantity: 1, repairId: null });
    saveCart(); updateCartBadge(); showToast('Товар добавлен в корзину');
}
function addRepairToCart(repairId) {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); showAuthModal(); return; }
    if (!cart.find(item => item.type === 'repair' && item.repairId === repairId))
        cart.push({ type: 'repair', repairId, quantity: 1, productId: null });
    saveCart(); updateCartBadge(); showToast('Услуга ремонта добавлена');
}
function removeFromCart(index) { cart.splice(index, 1); saveCart(); updateCartBadge(); if (document.getElementById('cartModal').classList.contains('show')) renderCartModal(); showToast('Удалено'); }
function saveCart() { localStorage.setItem('granat_cart', JSON.stringify(cart)); }
function loadCart() { const saved = localStorage.getItem('granat_cart'); if (saved) cart = JSON.parse(saved); else cart = []; }
function updateCartBadge() { const count = cart.reduce((s, i) => s + (i.quantity || 1), 0); document.getElementById('cartCount').innerText = count; }
function renderCartModal() {
    const modalBody = document.getElementById('cartModalBody');
    if (!modalBody) return;
    if (cart.length === 0) { modalBody.innerHTML = `<p>Корзина пуста</p><button class="btn-primary" data-bs-dismiss="modal">Закрыть</button>`; return; }
    let total = 0;
    let itemsHtml = `<div class="list-group">`;
    cart.forEach((item, idx) => {
        let name, price;
        if (item.type === 'product') {
            const prod = products.find(p => p.id === item.productId);
            if (!prod) return;
            name = prod.name;
            price = prod.price;
            total += price * (item.quantity || 1);
            itemsHtml += `<div class="list-group-item d-flex justify-content-between align-items-center">${name} x ${item.quantity || 1} = ${(price * (item.quantity || 1)).toLocaleString()}₽ <button class="btn-sm btn-outline-danger" onclick="removeFromCart(${idx})">Удалить</button></div>`;
        } else {
            const rep = repairOptions.find(r => r.id === item.repairId);
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
window.removeFromCart = removeFromCart;
window.addRepairToCart = addRepairToCart;

function checkoutOrder() {
    if (!currentUser || currentUser.role !== 'client') { showToast('Войдите как клиент', true); return; }
    if (cart.length === 0) { showToast('Корзина пуста'); return; }
    for (let item of cart) {
        const newId = orders.length + 1;
        if (item.type === 'product') {
            const prod = products.find(p => p.id === item.productId);
            orders.push({
                id: newId, clientId: currentUser.id, clientName: currentUser.fullName, type: 'product',
                productId: item.productId, repairId: null, status: 'new', managerId: null, jewelerId: null,
                deadline: new Date(Date.now() + 14 * 86400000).toISOString().slice(0, 10),
                total: prod.price * (item.quantity || 1), completedDate: null, paymentStatus: 'pending'
            });
        } else {
            const rep = repairOptions.find(r => r.id === item.repairId);
            orders.push({
                id: newId, clientId: currentUser.id, clientName: currentUser.fullName, type: 'repair',
                productId: null, repairId: item.repairId, status: 'new', managerId: null, jewelerId: null,
                deadline: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10),
                total: rep.price, completedDate: null, paymentStatus: 'pending'
            });
        }
    }
    cart = []; saveCart(); updateCartBadge(); persist();
    const modalEl = document.getElementById('cartModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
    showToast('Заказ оформлен! Менеджер свяжется.');
    renderClientDashboard();
}

// ========== 6. АВТОРИЗАЦИЯ И РЕГИСТРАЦИЯ ==========
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
                <div class="small text-muted mt-2 text-center">Тест: client / client123</div>
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
        const login = document.getElementById('authLogin').value.trim();
        const pwd = document.getElementById('authPassword').value.trim();
        if (!login || !pwd) { showToast('Введите логин и пароль', true); return; }
        const user = users.find(u => u.login === login && u.password === pwd);
        if (user) {
            currentUser = { id: user.id, login: user.login, role: user.role, fullName: user.fullName, phone: user.phone || '', address: user.address || '' };
            sessionStorage.setItem('granat_user', JSON.stringify(currentUser));
            modal.hide();
            showToast(`Добро пожаловать, ${user.fullName}!`);
            renderApp();
        } else showToast('Неверный логин или пароль', true);
    });
    document.getElementById('doRegBtn')?.addEventListener('click', () => {
        const name = document.getElementById('regName').value.trim();
        const login = document.getElementById('regLogin').value.trim();
        const pwd = document.getElementById('regPassword').value.trim();
        const phone = document.getElementById('regPhone').value.trim();
        const address = document.getElementById('regAddress').value.trim();
        if (!name || !login || !pwd) { showToast('Имя, логин и пароль обязательны', true); return; }
        if (users.find(u => u.login === login)) { showToast('Логин уже занят', true); return; }
        const newId = users.length + 1;
        users.push({ id: newId, login, password: pwd, role: 'client', fullName: name, phone, address });
        persist();
        showToast('Регистрация успешна! Теперь войдите.');
        const loginTabBtn = document.querySelector('#authTab button[data-bs-target="#loginTab"]');
        if (loginTabBtn) new bootstrap.Tab(loginTabBtn).show();
        document.getElementById('regName').value = '';
        document.getElementById('regLogin').value = '';
        document.getElementById('regPassword').value = '';
        document.getElementById('regPhone').value = '';
        document.getElementById('regAddress').value = '';
    });
}
function logout() { currentUser = null; sessionStorage.removeItem('granat_user'); cart = []; saveCart(); updateCartBadge(); renderApp(); showToast('Вы вышли'); }

// ========== 7. АВАТАР ПОЛЬЗОВАТЕЛЯ ==========
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

// ========== 8. АНИМАЦИЯ ПРИ СКРОЛЛЕ ==========
function initScrollAnimation() {
    const animatedElements = document.querySelectorAll('.animate-on-scroll');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => { if (entry.isIntersecting) { entry.target.classList.add('animated'); observer.unobserve(entry.target); } });
    }, { threshold: 0.1 });
    animatedElements.forEach(el => observer.observe(el));
}

// ========== 9. ОСНОВНОЙ РЕНДЕР ==========
function renderApp() {
    const saved = sessionStorage.getItem('granat_user');
    if (saved && !currentUser) currentUser = JSON.parse(saved);
    updateProfileAvatar();
    if (!currentUser) {
        renderCarousel();
        document.getElementById('dynamicContent').innerHTML = `
            <div class="hero-section animate-on-scroll">
                <h1>Изделия из серебра с гранатом</h1>
                <p>Уникальные украшения ручной работы</p>
                <button class="btn-primary" id="scrollToCatalog">Смотреть коллекцию</button>
            </div>
            <div class="products-grid" id="catalogGrid">
                ${products.map(p => `<div class="product-card animate-on-scroll" data-id="${p.id}"><img src="${p.image}" class="product-img" onerror="this.src='https://placehold.co/300x300?text=${p.name}'"><div class="product-info"><div class="product-name">${p.name}</div><div class="product-price">${p.price.toLocaleString()} ₽</div></div></div>`).join('')}
            </div>
        `;
        document.querySelectorAll('.product-card').forEach(card => card.addEventListener('click', () => showProductModal(parseInt(card.dataset.id))));
        document.getElementById('scrollToCatalog')?.addEventListener('click', () => document.getElementById('catalogGrid').scrollIntoView({ behavior: 'smooth' }));
        initScrollAnimation();
        return;
    }
    if (currentUser.role === 'admin') renderAdminDashboard();
    else if (currentUser.role === 'manager') renderManagerDashboard();
    else if (currentUser.role === 'jeweler') renderJewelerDashboard();
    else renderClientDashboard();
    initScrollAnimation();
}

// ========== 10. МОДАЛЬНОЕ ОКНО ТОВАРА ==========
function showProductModal(id) {
    const p = products.find(p => p.id === id);
    const body = document.getElementById('productModalBody');
    body.innerHTML = `
        <div class="text-center"><img src="${p.image}" class="img-fluid mb-3" style="max-height:250px;object-fit:cover;border-radius:20px;" onerror="this.src='https://placehold.co/400x300?text=${p.name}'"></div>
        <h4>${p.name}</h4>
        <p>${p.desc}</p>
        <hr><div class="row mb-2"><div class="col-6"><strong>Материал:</strong></div><div class="col-6">${p.material}</div></div>
        <div class="row mb-2"><div class="col-6"><strong>Вес:</strong></div><div class="col-6">${p.weight}</div></div>
        <div class="row mb-3"><div class="col-6"><strong>Артикул:</strong></div><div class="col-6">${p.article}</div></div>
        <div class="d-flex justify-content-between align-items-center"><span class="h4" style="color:var(--accent);">${p.price.toLocaleString()} ₽</span><button class="btn-primary" onclick="addToCart(${p.id}); bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();">В корзину</button></div>
    `;
    new bootstrap.Modal(document.getElementById('productModal')).show();
}

// ========== 11. КЛИЕНТСКАЯ ПАНЕЛЬ ==========
function renderClientDashboard() {
    const myOrders = orders.filter(o => o.clientId === currentUser.id);
    let ordersHtml = `<h3>Мои заказы</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Тип</th><th>Наименование</th><th>Статус</th><th>Сумма</th><th>Оплата</th><th>Действие</th></tr></thead><tbody>`;
    myOrders.forEach(o => {
        let name = '';
        if (o.type === 'product') { const p = products.find(p => p.id === o.productId); name = p ? p.name : 'Товар'; }
        else { const r = repairOptions.find(r => r.id === o.repairId); name = r ? r.name : 'Ремонт'; }
        ordersHtml += `<tr>
            <td>${o.id}</td>
            <td>${o.type === 'product' ? 'Изделие' : 'Ремонт'}</td>
            <td>${name}</td>
            <td><span class="status-badge status-${o.status}">${o.status}</span></td>
            <td>${o.total}₽</td>
            <td>${o.paymentStatus === 'paid' ? 'Оплачен' : 'Не оплачен'}</td>
            <td>${o.paymentStatus !== 'paid' ? `<button class="btn-sm btn-primary" onclick="payOrder(${o.id})">Оплатить</button>` : '—'}</td>
        </tr>`;
    });
    ordersHtml += `</tbody></table></div>`;
    document.getElementById('dynamicContent').innerHTML = `<div class="dashboard"><h2>Личный кабинет</h2><div class="mb-3"><strong>${currentUser.fullName}</strong><br>${currentUser.phone || ''} ${currentUser.address || ''}</div><div class="mb-4"><button class="btn-primary" id="showRepairBtn">➕ Заказать ремонт</button> <button class="btn-primary" id="showCatalogBtn">🛍️ Каталог изделий</button></div>${ordersHtml}</div>`;
    document.getElementById('showRepairBtn')?.addEventListener('click', () => showRepairCatalog());
    document.getElementById('showCatalogBtn')?.addEventListener('click', () => showProductCatalog());
}
function showProductCatalog() {
    document.getElementById('dynamicContent').innerHTML = `<div class="dashboard"><h2>Каталог изделий</h2><div class="products-grid">${products.map(p => `<div class="product-card animate-on-scroll" data-id="${p.id}"><img src="${p.image}" class="product-img" onerror="this.src='https://placehold.co/300x300?text=${p.name}'"><div class="product-info"><div class="product-name">${p.name}</div><div class="product-price">${p.price.toLocaleString()} ₽</div><button class="btn-sm btn-primary mt-2" onclick="addToCart(${p.id})">В корзину</button></div></div>`).join('')}</div><button class="btn-outline mt-3" onclick="renderClientDashboard()">← Назад</button></div>`;
    document.querySelectorAll('.product-card').forEach(card => { const id = parseInt(card.dataset.id); card.addEventListener('click', (e) => { if (!e.target.classList.contains('btn-primary')) showProductModal(id); }); });
    initScrollAnimation();
}
function showRepairCatalog() {
    document.getElementById('dynamicContent').innerHTML = `<div class="dashboard"><h2>Услуги ремонта</h2><div class="list-group">${repairOptions.map(r => `<div class="list-group-item animate-on-scroll"><div><strong>${r.name}</strong><br>${r.desc}<br>Цена: ${r.price}₽</div><button class="btn-sm btn-primary mt-2" onclick="addRepairToCart(${r.id})">В корзину</button></div>`).join('')}</div><button class="btn-outline mt-3" onclick="renderClientDashboard()">← Назад</button></div>`;
    initScrollAnimation();
}
window.payOrder = function (orderId) { const order = orders.find(o => o.id === orderId); if (order) { order.paymentStatus = 'paid'; persist(); showToast('Заказ оплачен'); renderClientDashboard(); } };

// ========== 12. АДМИНИСТРАТОР (сортировка товаров, история продаж) ==========
let productsSortField = 'name';
let productsSortOrder = 'asc';

function renderProductsTableForAdmin() {
    let sortedProducts = [...products];
    if (productsSortField === 'name') {
        sortedProducts.sort((a, b) => productsSortOrder === 'asc' ? a.name.localeCompare(b.name) : b.name.localeCompare(a.name));
    } else if (productsSortField === 'price') {
        sortedProducts.sort((a, b) => productsSortOrder === 'asc' ? a.price - b.price : b.price - a.price);
    }
    let html = `<h3>Управление товарами</h3><div class="d-flex gap-2 mb-3 flex-wrap"><button id="sortNameAsc" class="btn-outline">Название А→Я</button><button id="sortNameDesc" class="btn-outline">Название Я→А</button><button id="sortPriceAsc" class="btn-outline">Цена ↑</button><button id="sortPriceDesc" class="btn-outline">Цена ↓</button></div><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>Изображение</th><th>Название</th><th>Цена</th><th>Артикул</th><th>Материал</th><tr></thead><tbody>${sortedProducts.map(p => `<tr>
        <td><img src="${p.image}" style="width:50px;height:50px;object-fit:cover;border-radius:12px;" onerror="this.src='https://placehold.co/50x50'"></td>
        <td>${p.name}</td>
        <td>${p.price.toLocaleString()} ₽</td><td>${p.article}</td>
        <td>${p.material}</td>
    </tr>`).join('')}</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
    document.getElementById('sortNameAsc')?.addEventListener('click', () => { productsSortField = 'name'; productsSortOrder = 'asc'; renderProductsTableForAdmin(); });
    document.getElementById('sortNameDesc')?.addEventListener('click', () => { productsSortField = 'name'; productsSortOrder = 'desc'; renderProductsTableForAdmin(); });
    document.getElementById('sortPriceAsc')?.addEventListener('click', () => { productsSortField = 'price'; productsSortOrder = 'asc'; renderProductsTableForAdmin(); });
    document.getElementById('sortPriceDesc')?.addEventListener('click', () => { productsSortField = 'price'; productsSortOrder = 'desc'; renderProductsTableForAdmin(); });
}

function renderAdminDashboard() {
    const totalOrders = orders.length, completed = orders.filter(o => o.status === 'completed').length, revenue = orders.filter(o => o.status === 'completed').reduce((s, o) => s + o.total, 0);
    let html = `<div class="dashboard"><h2>Админ панель</h2><div class="stats-grid"><div class="stat-card animate-on-scroll"><h3>${totalOrders}</h3><p>Заказов</p></div><div class="stat-card animate-on-scroll"><h3>${completed}</h3><p>Завершено</p></div><div class="stat-card animate-on-scroll"><h3>${revenue.toLocaleString()} ₽</h3><p>Выручка</p></div></div><div><button id="adminOrdersBtn" class="btn-primary">Заказы</button> <button id="adminProductsBtn" class="btn-primary">Товары</button> <button id="adminMaterialsBtn" class="btn-outline">Материалы</button> <button id="adminSalesBtn" class="btn-outline">История продаж</button> <button id="adminUsersBtn" class="btn-outline">Пользователи</button> <button id="adminBackCatalog" class="btn-outline">На сайт</button></div><div id="adminContent"></div></div>`;
    document.getElementById('app').innerHTML = html;
    document.getElementById('adminOrdersBtn').onclick = () => renderOrdersTable();
    document.getElementById('adminProductsBtn').onclick = () => renderProductsTableForAdmin();
    document.getElementById('adminMaterialsBtn').onclick = () => renderMaterialsTable();
    document.getElementById('adminSalesBtn').onclick = () => renderSalesHistory();
    document.getElementById('adminUsersBtn').onclick = () => renderUsersTable();
    document.getElementById('adminBackCatalog').onclick = () => { currentUser = null; sessionStorage.removeItem('granat_user'); cart = []; saveCart(); renderApp(); };
    renderOrdersTable();
}
function renderOrdersTable() {
    let html = `<h3>Заказы</h3><button id="createOrderAdmin" class="btn-primary mb-2">+ Новый заказ</button><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Клиент</th><th>Тип</th><th>Статус</th><th>Сумма</th><th>Оплата</th><th>Действие</th></tr></thead><tbody>${orders.map(o => `<tr>
        <td>${o.id}</td>
        <td>${o.clientName}</td>
        <td>${o.type}</td>
        <td><span class="status-badge status-${o.status}">${o.status}</span></td>
        <td>${o.total}₽</td><td>${o.paymentStatus}</td>
        <td><button class="btn-sm btn-outline" onclick="window.changeOrderStatus(${o.id})">Статус</button></td>
    </tr>`).join('')}</tbody></table></div>`;
    document.getElementById('adminContent').innerHTML = html;
    document.getElementById('createOrderAdmin')?.addEventListener('click', () => showCreateOrderForm());
}
function renderMaterialsTable() {
    document.getElementById('adminContent').innerHTML = `<h3>Материалы</h3><div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>Материал</th><th>Количество</th><th>Цена/ед</th></tr></thead><tbody>${materials.map(m => `<tr>
        <td>${m.name}</td>
        <td>${m.quantity} ${m.unit}</td>
        <td>${m.pricePerUnit}₽</td>
    </tr>`).join('')}</tbody></table></div>`;
}
function renderSalesHistory() {
    let completed = orders.filter(o => o.status === 'completed');
    let html = `<h3>История продаж</h3><div class="filter-bar"><input type="date" id="salesFrom"><input type="date" id="salesTo"><button id="applySalesFilter" class="btn-primary">Фильтр</button></div><div id="salesTable"></div>`;
    document.getElementById('adminContent').innerHTML = html;
    function renderFiltered() {
        const from = document.getElementById('salesFrom').value, to = document.getElementById('salesTo').value;
        let filtered = completed;
        if (from) filtered = filtered.filter(o => o.completedDate >= from);
        if (to) filtered = filtered.filter(o => o.completedDate <= to);
        let table = `<div class="table-wrapper"><table class="animate-on-scroll"><thead><tr><th>ID</th><th>Клиент</th><th>Сумма</th><th>Дата</th></tr></thead><tbody>${filtered.map(o => `<tr>
            <td>${o.id}</td>
            <td>${o.clientName}</td>
            <td>${o.total}₽</td><td>${o.completedDate}</td>
        </tr>`).join('')}</tbody></table></div><p>Итого: ${filtered.reduce((s, o) => s + o.total, 0).toLocaleString()}₽</p>`;
        document.getElementById('salesTable').innerHTML = table;
    }
    document.getElementById('applySalesFilter').on 