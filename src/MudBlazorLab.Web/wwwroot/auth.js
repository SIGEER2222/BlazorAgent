window.authLogin = async function (username, password) {
  try {
    const res = await fetch('/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Username: username, Password: password }),
      credentials: 'include'
    });
    return res.ok;
  } catch (e) {
    return false;
  }
}

window.authLogout = async function () {
  try {
    const res = await fetch('/auth/logout', {
      method: 'POST',
      credentials: 'include'
    });
    return res.ok;
  } catch (e) {
    return false;
  }
}
