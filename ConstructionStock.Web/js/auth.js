async function login(username, password, role) {
    try {
        const response = await apiFetch('/auth/login', 'POST', { username, password, role });
        
        if (response.success) {
            const { token, role: returnedRole, siteId, userId, fullName, siteName } = response.data;
            
            localStorage.setItem('token', token);
            localStorage.setItem('role', returnedRole);
            localStorage.setItem('siteId', siteId);
            localStorage.setItem('userId', userId);
            localStorage.setItem('fullName', fullName);
            localStorage.setItem('siteName', siteName);
            
            return { success: true, role: returnedRole };
        }
        return { success: false, message: response.message };
    } catch (error) {
        return { success: false, message: error.message };
    }
}

function logout() {
    localStorage.clear();
    window.location.href = 'index.html';
}

function getCurrentUser() {
    return {
        fullName: localStorage.getItem('fullName'),
        role: localStorage.getItem('role'),
        siteName: localStorage.getItem('siteName')
    };
}
