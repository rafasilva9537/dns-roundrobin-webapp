import './style.css'
import api, { type LoggedUserResponse, type LoginResponse, type RegisterResponse } from './api'
import { AxiosError } from 'axios';

// Views
const loginView = document.getElementById('login-view')!;
const registerView = document.getElementById('register-view')!;
const profileView = document.getElementById('profile-view')!;

// Forms
const loginForm = document.getElementById('login-form') as HTMLFormElement;
const registerForm = document.getElementById('register-form') as HTMLFormElement;

// Inputs - Login
const loginUsernameInput = document.getElementById('login-username') as HTMLInputElement;
const loginPasswordInput = document.getElementById('login-password') as HTMLInputElement;
const loginErrorMsg = document.getElementById('login-error-msg')!;

// Inputs - Register
const regUsernameInput = document.getElementById('reg-username') as HTMLInputElement;
const regEmailInput = document.getElementById('reg-email') as HTMLInputElement;
const regPasswordInput = document.getElementById('reg-password') as HTMLInputElement;
const registerErrorMsg = document.getElementById('register-error-msg')!;

// Links
const linkToRegister = document.getElementById('link-to-register')!;
const linkToLogin = document.getElementById('link-to-login')!;

// Profile Elements
const pUsername = document.getElementById('p-username')!;
const pLoginTime = document.getElementById('p-login-time')!;
const pSession = document.getElementById('p-session')!;
const pServer = document.getElementById('p-server')!;
const btnRefresh = document.getElementById('btn-refresh')!;
const btnLogout = document.getElementById('btn-logout')!;


// State
const isAuthenticated = () => !!localStorage.getItem('accessToken');


// UI Functions
function updateUI() {
  if (isAuthenticated()) {
    showView('profile');
    fetchProfile();
  } else {
    showView('login');
  }
}

function showView(viewName: 'login' | 'register' | 'profile') {
    loginView.classList.add('hidden');
    registerView.classList.add('hidden');
    profileView.classList.add('hidden');
    
    // Reset forms/errors when switching
    loginErrorMsg.classList.add('hidden');
    registerErrorMsg.classList.add('hidden');

    if (viewName === 'login') {
        loginView.classList.remove('hidden');
        loginForm.reset();
    } else if (viewName === 'register') {
        registerView.classList.remove('hidden');
        registerForm.reset();
    } else {
        profileView.classList.remove('hidden');
    }
}

function showError(element: HTMLElement, message: string) {
  element.textContent = message;
  element.classList.remove('hidden');
}


// Handlers
async function handleLogin(e: Event) {
  e.preventDefault();
  const username = loginUsernameInput.value;
  const password = loginPasswordInput.value;

  try {
    const { data } = await api.post<LoginResponse>('/auth/login', { username, password });
    
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    
    updateUI();
  } catch (error) {
    const err = error as AxiosError<{detail?: string}>; 
    showError(loginErrorMsg, err.response?.data?.detail || 'Login failed. Check credentials.');
  }
}

async function handleRegister(e: Event) {
    e.preventDefault();
    const username = regUsernameInput.value;
    const email = regEmailInput.value;
    const password = regPasswordInput.value;
  
    try {
      const { data } = await api.post<RegisterResponse>('/auth/register', { username, email, password });
      
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      
      updateUI();
    } catch (error) {
      const err = error as AxiosError<{detail?: string}>; 
      // The backend might return validation errors, could be improved to show details
      showError(registerErrorMsg, err.response?.data?.detail || 'Registration failed. Try a different username.');
    }
  }

async function fetchProfile() {
  try {
    pServer.textContent = "Loading...";
    const { data } = await api.get<LoggedUserResponse>('/accounts/logged-user');
    
    pUsername.textContent = data.userName;
    pSession.textContent = data.sessionId;
    pServer.textContent = data.getHostName;
    
    // Format Date
    const date = new Date(data.loginDate);
    pLoginTime.textContent = date.toLocaleString('pt-BR');
    
  } catch (error) {
    console.error('Failed to fetch profile', error);
  }
}

async function handleLogout() {
  try {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      await api.post('/auth/logout', { refreshToken });
    }
  } finally {
    localStorage.clear();
    updateUI();
  }
}

// Event Listeners
loginForm.addEventListener('submit', handleLogin);
registerForm.addEventListener('submit', handleRegister);

linkToRegister.addEventListener('click', (e) => {
    e.preventDefault();
    showView('register');
});

linkToLogin.addEventListener('click', (e) => {
    e.preventDefault();
    showView('login');
});

btnRefresh.addEventListener('click', fetchProfile);
btnLogout.addEventListener('click', handleLogout);

updateUI();