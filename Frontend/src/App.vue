<template>
  <div class="nav">
    <router-link to="/">Logo</router-link>
    <div v-if="!authStore.loggedIn" class="buttons-container">
      <button @click="modalStore.openLoginModal" class="secondary">{{ $t('Login') }}</button>
      <button @click="modalStore.openRegisterModal" class="primary">{{ $t('Register') }}</button>
    </div>
    <accountDropdown v-if="authStore.loggedIn" />
  </div>
  
  <div class="main-container">
    <RouterView />
  </div>
  <Modal />
</template>


<script setup>
import { ref, onMounted } from 'vue';
import Modal from './components/Modal.vue';
import accountDropdown from './components/AccountDropdown.vue';
import { useModalStore } from "./stores/modalStore";
import { useAuthStore } from "./stores/authStore";

const modalStore = useModalStore();
const authStore = useAuthStore();

onMounted(() => {
  if(!authStore.loggedIn)
    authStore.loginFromStorage();
  }
);

</script>

<style scoped>
.main-container {
  margin: 0 auto;
  max-width: 1104px !important;
}

.nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.25rem 0.25rem;
  height: 3.75rem;
  background-color: var(--secondary-bg-color);
}

.nav a {
  font-size: 1.5rem;
  padding-left: 1.5rem;
}

.nav .buttons-container {
  width: 180px;
  display: flex;
  justify-content: space-around;
}

</style>
