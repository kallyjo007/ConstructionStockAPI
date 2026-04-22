document.addEventListener('DOMContentLoaded', async () => {
    await loadSuppliers();
    updateAlertBadge();
});

async function loadSuppliers() {
    try {
        const response = await apiFetch('/suppliers');
        if (response.success) {
            renderSuppliers(response.data);
        }
    } catch (error) {
        console.error('Failed to load suppliers:', error);
    }
}

function renderSuppliers(suppliers) {
    const tbody = document.getElementById('suppliersTableBody');
    tbody.innerHTML = '';

    suppliers.forEach(s => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><strong>${s.supplierName}</strong></td>
            <td>${s.contactPerson || '-'}</td>
            <td>${s.phone || '-'}</td>
            <td>${s.email || '-'}</td>
            <td>
                <button onclick="editSupplier(${JSON.stringify(s).replace(/"/g, '&quot;')})" class="btn btn-secondary" style="padding: 0.25rem 0.5rem; font-size: 0.75rem;">Edit</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function openSupplierModal(s = null) {
    const modal = document.getElementById('supplierModal');
    const form = document.getElementById('supplierForm');
    
    form.reset();
    modal.style.display = 'flex';

    if (s) {
        document.getElementById('modalTitle').textContent = 'Edit Supplier';
        document.getElementById('editSupplierId').value = s.supplierId;
        document.getElementById('supplierName').value = s.supplierName;
        document.getElementById('contactPerson').value = s.contactPerson || '';
        document.getElementById('phone').value = s.phone || '';
        document.getElementById('email').value = s.email || '';
    } else {
        document.getElementById('modalTitle').textContent = 'Add New Supplier';
        document.getElementById('editSupplierId').value = '';
    }
}

function closeSupplierModal() {
    document.getElementById('supplierModal').style.display = 'none';
}

document.getElementById('supplierForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('editSupplierId').value;
    
    const data = {
        supplierName: document.getElementById('supplierName').value,
        contactPerson: document.getElementById('contactPerson').value,
        phone: document.getElementById('phone').value,
        email: document.getElementById('email').value
    };

    try {
        let response;
        if (id) {
            response = await apiFetch(`/suppliers/${id}`, 'PUT', data);
        } else {
            response = await apiFetch('/suppliers', 'POST', data);
        }

        if (response.success) {
            closeSupplierModal();
            await loadSuppliers();
        }
    } catch (error) {
        alert(error.message);
    }
});

function editSupplier(s) {
    openSupplierModal(s);
}

async function updateAlertBadge() {
    try {
        const response = await apiFetch('/alerts');
        if (response.success && response.data.length > 0) {
            const badge = document.getElementById('alertCount');
            badge.textContent = response.data.length;
            badge.style.display = 'inline-flex';
        }
    } catch (error) {}
}
