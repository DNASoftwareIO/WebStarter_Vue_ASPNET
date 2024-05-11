<template>
  <AccountNav/>
  <div class="card justify-between-centerv mb-10">
    <p class="font-xl">{{ $t('LogoutAllSessions') }}</p>
    <button @click.prevent="logOutAll" class="primary logout-all-button">{{ $t('Logout2') }}</button>
  </div>
  <div class="card hide-mobile">
    <table>
      <thead>
        <tr>
          <th>{{ $t('Date') }}</th>
          <th>{{ $t('IpAddress') }}</th>
          <th>{{ $t('Device') }}</th>
          <th>{{ $t('Country') }}</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(session, index) in sessions" :key="index">
          <td>{{ formatDate(session.date) }}</td>
          <td>{{ session.ipAddress }}</td>
          <td>{{ formatDevice(session.device) }}</td>
          <td>{{ session.country }}</td>
          <td>
            <button @click.prevent="logoutSession(session.id)" class="primary xs" :disabled="authStore.sessionId === session.id">{{authStore.sessionId === session.id ? i18n.global.t('Current') : i18n.global.t('Logout') }}</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <h3 class="hide-desktop mb-1" style="padding-left: 12px;">Sessions</h3>
  <div v-for="(session, index) in sessions" :key="index" class="card hide-desktop mb-1">
    <table>
      <tbody>
        <tr>
          <th width="50%">{{ $t('Date') }}</th>
          <td width="50%">{{ formatDate(session.date) }}</td>
        </tr>
        <tr>
          <th>{{ $t('IpAddress') }}</th>
          <td>{{ session.ipAddress }}</td>
        </tr>
        <tr>          
          <th>{{ $t('Device') }}</th>
          <td>{{ formatDevice(session.device) }}</td>
        </tr>
        <tr>
          <th>{{ $t('Country') }}</th>
          <td>{{ session.country }}</td>
        </tr>
        <tr>
          <td></td>
          <td>
            <button @click.prevent="logoutSession(session.id)" class="primary xs" :disabled="authStore.sessionId === session.id">{{authStore.sessionId === session.id ? i18n.global.t('Current') : i18n.global.t('Logout') }}</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { UAParser } from 'ua-parser-js';
import api from '../api/api';
import { useAuthStore } from "../stores/authStore";
import AccountNav from '../components/AccountNav.vue';
import router from "../router/index";
import i18n from '../i18n'; 
import { formatDate } from '../utils/formatters'


const authStore = useAuthStore();
const sessions = ref([]);

onMounted(async () => {
  await getSessions();
});

async function getSessions() {
  const response = await api.auth.getSessions();
  sessions.value = response.data.sessions;
};

function formatDevice(u) {
  const { browser, os } = UAParser(u);
  return browser.name + ' ' + browser.version + ' ' + i18n.global.t('on') + ' ' + os.name + ' ' + os.version; 
}

async function logoutSession(id) {
  await api.auth.endSession(id);
  
  sessions.value = sessions.value.filter(s => s.id == id);
}

async function logOutAll() {
  await api.auth.enAlldSessions();
  authStore.logout();
  router.push("/");
}
</script>

<style scoped>
.logout-all-button {
  padding: 8px 2.5rem;
  font-size: 1rem;
}

</style>