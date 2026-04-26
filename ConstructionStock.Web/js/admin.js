document.addEventListener('DOMContentLoaded', async () => {
    // Setup initial date for report
    const dateInput = document.getElementById('adminReportDate');
    const today = new Date().toISOString().split('T')[0];
    dateInput.value = today;
    dateInput.addEventListener('change', (e) => loadReport(e.target.value));

    // Load initial data
    await loadSites();
    await loadUsers();
    await loadSuppliers();
    await loadReport(today);

    // Bind forms
    bindCreateSite();
    bindCreateUser();
    bindCreateSupplier();
});

function showSection(sectionId, linkEl) {
    document.querySelectorAll('.admin-section').forEach(el => el.classList.remove('active'));
    document.getElementById(sectionId).classList.add('active');

    document.querySelectorAll('.nav-links a').forEach(el => el.classList.remove('active'));
    linkEl.classList.add('active');
}

// =======================
//   USERS & SITES
// =======================
async function loadSites() {
    try {
        const response = await apiFetch('/sites');
        if (response.success) {
            const select = document.getElementById('userSite');
            select.innerHTML = '<option value="">Select Site</option>';
            response.data.forEach(site => {
                const option = document.createElement('option');
                option.value = site.siteId;
                option.textContent = site.siteName;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Failed to load sites:', error);
    }
}

async function loadUsers() {
    try {
        const response = await apiFetch('/users');
        if (response.success) {
            const tbody = document.getElementById('adminUsersTableBody');
            tbody.innerHTML = '';
            response.data.forEach(u => {
                const tr = document.createElement('tr');
                const btnStatus = u.isActive ? 'Deactivate' : 'Activate';
                tr.innerHTML = `
                    <td><strong>${u.fullName}</strong></td>
                    <td>${u.username}</td>
                    <td><span class="badge ${u.isActive ? 'badge-success' : 'badge-danger'}">${u.role}</span></td>
                    <td>${u.isActive ? 'Active' : 'Inactive'}</td>
                    <td>
                        <button class="btn btn-sm btn-outline" onclick="toggleUserStatus(${u.userId}, ${u.isActive})">${btnStatus}</button>
                        <button class="btn btn-sm" onclick="showAssignSite(${u.userId})">Assign Site</button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (error) {
        console.error('Failed to load users:', error);
    }
}

function bindCreateSite() {
    document.getElementById('createSiteForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            siteName: document.getElementById('newSiteName').value.trim(),
            location: document.getElementById('newSiteLocation').value.trim()
        };
        const msg = document.getElementById('createSiteMessage');
        try {
            const res = await apiFetch('/sites', 'POST', data);
            msg.style.display = 'block';
            if (res.success) {
                msg.style.color = 'var(--success-color)';
                msg.textContent = 'Site created.';
                document.getElementById('createSiteForm').reset();
                await loadSites();
            } else {
                msg.style.color = 'var(--danger-color)';
                msg.textContent = res.message || 'Failed to create site.';
            }
        } catch (err) {
            msg.style.color = 'var(--danger-color)';
            msg.textContent = err.message;
            msg.style.display = 'block';
        }
    });
}

function bindCreateUser() {
    document.getElementById('createUserForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            fullName: document.getElementById('fullName').value.trim(),
            username: document.getElementById('username').value.trim(),
            password: document.getElementById('password').value,
            role: document.getElementById('userRole').value,
            siteId: parseInt(document.getElementById('userSite').value)
        };
        const msg = document.getElementById('createUserMessage');
        try {
            const res = await apiFetch('/users', 'POST', data);
            msg.style.display = 'block';
            if (res.success) {
                msg.style.color = 'var(--success-color)';
                msg.textContent = 'User created successfully.';
                document.getElementById('createUserForm').reset();
                await loadUsers();
            } else {
                msg.style.color = 'var(--danger-color)';
                msg.textContent = res.message || 'Failed to create user.';
            }
        } catch (err) {
            msg.style.color = 'var(--danger-color)';
            msg.textContent = err.message;
            msg.style.display = 'block';
        }
    });
}

async function toggleUserStatus(userId, currentStatus) {
    if (confirm(`Are you sure you want to ${currentStatus ? 'deactivate' : 'activate'} this user?`)) {
        await apiFetch(`/sites/users/${userId}/status`, 'PUT', { isActive: !currentStatus });
        await loadUsers();
    }
}

async function showAssignSite(userId) {
    const siteId = prompt('Enter new Site ID for user:');
    if (siteId) {
        await apiFetch(`/sites/users/${userId}/site`, 'PUT', { siteId: parseInt(siteId) });
        await loadUsers();
    }
}

// =======================
//   SUPPLIERS
// =======================
async function loadSuppliers() {
    try {
        const response = await apiFetch('/suppliers');
        if (response.success) {
            const tbody = document.getElementById('adminSuppliersTableBody');
            tbody.innerHTML = '';
            response.data.forEach(s => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>#${s.supplierId}</td>
                    <td><strong>${s.supplierName}</strong></td>
                    <td>${s.contactInfo || '-'}</td>
                    <td><span class="badge ${s.isActive ? 'badge-success' : 'badge-danger'}">${s.isActive ? 'Active' : 'Inactive'}</span></td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (error) {
        console.error('Failed to load suppliers:', error);
    }
}

function bindCreateSupplier() {
    document.getElementById('supplierForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            supplierName: document.getElementById('supplierName').value.trim(),
            contactInfo: document.getElementById('contactInfo').value.trim()
        };
        const msg = document.getElementById('supplierMessage');
        try {
            const res = await apiFetch('/suppliers', 'POST', data);
            msg.style.display = 'block';
            if (res.success) {
                msg.style.color = 'var(--success-color)';
                msg.textContent = 'Supplier registered successfully.';
                document.getElementById('supplierForm').reset();
                await loadSuppliers();
            } else {
                msg.style.color = 'var(--danger-color)';
                msg.textContent = res.message || 'Failed to register supplier.';
            }
        } catch (error) {
            msg.style.color = 'var(--danger-color)';
            msg.textContent = error.message;
        }
    });
}

// =======================
//   REPORTS
// =======================
async function loadReport(date) {
    try {
        const response = await apiFetch(`/reports/admin?date=${date}`);
        if (response.success) {
            const data = response.data;
            document.getElementById('activeSitesCount').textContent = data.activeSitesCount;
            document.getElementById('activeUsersCount').textContent = data.activeUsersCount;

            const tSites = document.getElementById('activeSitesTableBody');
            tSites.innerHTML = '';
            data.activeSites.forEach(s => {
                tSites.innerHTML += `<tr><td>${s.siteName}</td><td>${s.location || '-'}</td><td>${s.transactionCount}</td></tr>`;
            });

            const tUsers = document.getElementById('activeUsersTableBody');
            tUsers.innerHTML = '';
            data.activeUsers.forEach(u => {
                tUsers.innerHTML += `<tr><td>${u.fullName}</td><td><span class="badge badge-info">${u.role}</span></td><td>${u.siteName}</td><td>${u.transactionCount}</td></tr>`;
            });
        }
    } catch (error) {
        console.error('Failed to load admin report:', error);
    }
}
