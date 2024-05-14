<template>
    <div class="card flex-col-centerv mt-33">
      <h3 :class="showSuccess ? 'success' : 'error'">{{ showSuccess ? i18n.global.t("EmailAddressConfirmed") : i18n.global.t("ErrorConfirmingEmailAddress") }}</h3>
      <p v-if="!showSuccess">{{ $t("EmailErrorDetail") }}</p>
      <router-link to="/">{{ $t("BackToHome") }}</router-link>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute } from "vue-router";
import api from "../api/api";
import i18n from '../i18n'; 

const route = useRoute(); 
const showSuccess = ref(false);
const showError = ref(false);

onMounted(async () => {
  if(route.query.userId === undefined || route.query.token === undefined) {
      return;
  }
      
  try {
    await api.auth.confirmEmail(route.query.userId, route.query.token);
    showSuccess.value = true;
  } catch {
    
  }
});

</script>