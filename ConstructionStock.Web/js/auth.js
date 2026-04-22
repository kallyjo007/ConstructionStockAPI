async function login(username, password) {
    try {
        const response = await apiFetch('/auth/login', 'POST', { username, password });
        
        if (response.success) {
            const { token, role, siteId, userId, fullName, siteName } = response.data;
            
            localStorage.setItem('token', token);
            localStorage.setItem('role', role);
            localStorage.setItem('siteId', siteId);
            localStorage.setItem('userId', userId);
            localStorage.setItem('fullName', fullName);
            localStorage.setItem('siteName', siteName);
            
            return { success: true, role };
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
