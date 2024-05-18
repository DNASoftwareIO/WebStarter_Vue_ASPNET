import { ref } from 'vue'
import { defineStore } from 'pinia'
import instance from '../api/axiosInstance';
import { jwtDecode } from "jwt-decode";

export const useAuthStore = defineStore('authStore', () => {
  const id = ref('');
  const userName = ref('');
  const dateJoined = ref('');
  const loggedIn = ref(false);
  const email = ref('');
  const emailConfirmed = ref(false);
  const tfaEnabled = ref(false);
  const sessionId = ref('');

  function login(data) {
    id.value = data.user.id,
    userName.value = data.user.userName;
    dateJoined.value = data.user.dateJoined;
    email.value = data.user.email;
    emailConfirmed.value = data.user.emailConfirmed;
    tfaEnabled.value = data.user.tfaEnabled;
    loggedIn.value = true;
    sessionId.value = data.sessionId;

    localStorage.setItem('authToken', data.jwt);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('sessionId', data.sessionId);
    localStorage.setItem('user', JSON.stringify(data.user));

    instance.defaults.headers.common['Authorization'] = `bearer ${data.jwt}`;
  }
  
  async function loginFromStorage() {
    var refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken != null && refreshToken != undefined) {
      var jwt = localStorage.getItem('authToken');
      if (jwt != null && jwt != undefined) {
        const decodedJwt = jwtDecode(jwt);
        if (Date.now() >= decodedJwt.exp * 1000) {
          try {
            const response = await instance({
              method: 'post',
              url: '/user/refresh-token',
              data: {
                refreshToken: refreshToken,
              }
            });

            if (response.status == 200) {
              localStorage.setItem('authToken', response.data.jwt);
              jwt = response.data.jwt;
            } else {
              logout();
              return;
            }
          } catch (error) {
            logout();
            return;
          }
        }

        instance.defaults.headers.common['Authorization'] = `bearer ${jwt}`;
        loggedIn.value = true;
        
        sessionId.value = localStorage.getItem('sessionId');
        var user = JSON.parse(localStorage.getItem('user'));
        id.value = user.id;
        userName.value = user.userName;
        dateJoined.value = user.dateJoined;
        email.value = user.email;
        emailConfirmed.value = user.emailConfirmed;
        tfaEnabled.value = user.tfaEnabled;
      }
    }
  }

  function logout() {
    id.value = '';
    userName.value = '';
    dateJoined.value = '';
    email.value = '';
    emailConfirmed.value = false;
    tfaEnabled.value = false;
    loggedIn.value = false;

    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');

    instance.defaults.headers.common['Authorization'] = '';
  }

  function toggleTfa() {
    tfaEnabled.value = !tfaEnabled.value;
    
    localStorage.setItem('user', JSON.stringify(
      {
        id: id.value,
        userName: userName.value,
        dateJoined: dateJoined.value,
        email: email.value,
        emailConfirmed: emailConfirmed.value,
        tfaEnabled: tfaEnabled.value
      }
    ));
  }

  return { id, userName, email, tfaEnabled, loggedIn, sessionId, login, loginFromStorage, logout, toggleTfa };
})