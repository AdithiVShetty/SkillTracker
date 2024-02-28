function login() {
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    const loginUser = {
        EmailId: email,
        Password: password
    };

    fetch('https://localhost:44309/api/User/postloginuser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(loginUser),
    })
        .then(response => response.json())
        .then(data => handleLoginResponse(data))
        .catch(error => console.error('Error submitting login form:', error));
}

function handleLoginResponse(data) {
    if (data && data.length > 0) {
        // Admin logged in
        window.location.href = 'admin-dashboard.html';
    } else if (data) {
        // User logged in
        window.location.href = 'user-dashboard.html';
    } else {
        alert('Invalid EmailId or Password');
    }
}