document.addEventListener('DOMContentLoaded', async () => {
    await loadUsers();
    updateAlertBadge();
});

// Admin functions migrated to admin.js

function renderUsers(users) {
    const tbody = document.getElementById('usersTableBody');
    const role = localStorage.getItem('role');
    tbody.innerHTML = '';

    users.forEach(u => {
        const tr = document.createElement('tr');
        const roleClass = u.role === 'StockManager' ? 'badge-info' : 'badge-secondary';
        const statusClass = u.isActive ? 'badge-success' : 'badge-danger';
        
        let actions = '';

        tr.innerHTML = `
            <td><strong>${u.fullName}</strong></td>
            <td>${u.username}</td>
            <td><span class="badge ${roleClass}">${u.role}</span></td>
            <td><span class="badge ${statusClass}">${u.isActive ? 'Active' : 'Inactive'}</span></td>
            <td>${actions}</td>
        `;
        tbody.appendChild(tr);
    });
}

async function updateAlertBadge() {
    try {
        const response = await apiFetch('/alerts');
        if (response.success && response.data.length > 0) {
            const badge = document.getElementById('alertCount');
            if (badge) {
                badge.textContent = response.data.length;
                badge.style.display = 'inline-flex';
            }
        }
    } catch (error) {}
}
