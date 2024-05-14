import instance from './axiosInstance';

const login = async (username, password, tfaCode) => {
  return await instance.post('/user/login', {
    username,
    password,
    tfaCode
  });
}

const register = async (username, password, email) => {
  return await instance.post('/user/register', {
    username,
    password,
    email
  });
}

const getSessions = async () => {
  return await instance.get('/user/get-sessions');
}

const endSession = async (id) => {
  return await instance.post('/user/logout', {
    id
  });
}

const enAlldSessions = async () => {
  return await instance.post('/user/end-all-sessions');
}

const changePassword = async (oldPassword, newPassword, tfaCode) => {
  return await instance.post('/user/change-password', {
    oldPassword,
    newPassword,
    tfaCode
  });
}

const getTfaCode = async () => {
  return await instance.get('/user/get-tfa-key');
}

const toggleTfa = async (tfaCode) => {
  return await instance.post('/user/toggle-tfa', {
    tfaCode
  });
}

const forgotPassword = async (email) => {
  return await instance.post('/user/forgot-password', {
    email,
  });
}

const resetPassword = async (email, token, password) => {
  return await instance.post('/user/reset-password', {
    email,
    token, 
    password
  });
}

const confirmEmail = async (userId, token) => {
  return await instance.post('/user/confirm-email', {
    userId,
    token, 
  });
}

export default {
  login,
  register,
  getSessions,
  endSession,
  enAlldSessions,
  getTfaCode,
  toggleTfa,
  changePassword,
  forgotPassword,
  resetPassword,
  confirmEmail
}