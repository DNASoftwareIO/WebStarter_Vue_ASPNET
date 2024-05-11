<template>
  <div class="relative mr-6" v-click-outside="closeDropdown">
          <div @click="toggleAccountDropdown"  class="avatar account-avatar sm">
            <img src="/images/avatar.jpg">
          </div>
          <div :class="{ open: showAccountDropdown }" class="account-dropdown">
            <div class="user-details">
              <div class="avatar account-avatar lg">
                <img src="/images/avatar.jpg">
              </div>
              <div class="text-container">
                <p class="name">{{authStore.userName}}</p>
                <p class="email" >{{authStore.email ? authStore.email : $t('NoEmailFound')}}</p>
              </div>
            </div>
            <div class="links-section">
              <router-link @click="toggleAccountDropdown" to="/sessions">
                {{ $t('Account') }}
              </router-link>
            </div>
            <div class="links-section">
              <a @click.prevent="logout" href="#" >
                {{ $t('Logout') }}
              </a>
            </div>
          </div>
          </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '../api/api';
import router from "../router/index";
import { useAuthStore } from "../stores/authStore";

const authStore = useAuthStore();

const showAccountDropdown = ref(false);

function closeDropdown()
{
  if(showAccountDropdown.value === true) 
  {
    showAccountDropdown.value=false;
  }
}

function toggleAccountDropdown() {
  showAccountDropdown.value = !showAccountDropdown.value;
}

async function logout() {
  await api.auth.endSession(authStore.sessionId);
  authStore.logout();
  toggleAccountDropdown();
  router.push("/login");
}
</script>

<style scoped>
.account-dropdown {
  display: none;
  width: 18.5rem;
  padding: 0 1.5rem;
  position: absolute;
  right: -1rem;
  top: 3.125rem;
  border-radius: 8px;
  background: var(--secondary-bg-color);
  box-shadow: 1px 1px 8px 0px var(--black);
  z-index: 100;
}

.open {
  display: block;
}

.user-details {
  padding: 1.5rem 0;
  display: flex;
  column-gap: 1rem;
}

.img-container img {
  width: 100%;
}

.text-container {
  display: flex;
  flex-direction: column;
  row-gap: 0.5rem;
}

.name {
  font-size: 1.125rem;
  font-weight: 500;
}

.email {
  color: var(--chevron);
}

.links-section {
  border-top: 1px solid var(--input-border-color);
  padding: 0.5rem;
}

a {
  padding: 0.7rem 0;
  text-decoration: none;
  display: flex;
  column-gap: 0.5rem;
  color: var(--primary-text-color);
}

a:hover {
  color: var(--primary-color);
}

</style>