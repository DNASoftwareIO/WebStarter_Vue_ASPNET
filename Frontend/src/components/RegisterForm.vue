<template>
  <div>
    <p class="error-summary">{{form.errorSummary}}</p>
    <form @submit.prevent="register" method="post" autocomplete="off" class="flex-col">
      <label>{{ $t('Username') }} <span class="error">{{form.errors.username}}</span></label>
      <input  v-model="form.fields.username" type="text":placeholder="$t('Username')">
      
      <label>{{ $t('EmailAddress') }} <span class="error">{{form.errors.email}}</span></label>
      <input v-model="form.fields.email" type="text" :placeholder="$t('EmailAddress')">
      
      <label>{{ $t('Password') }} <span class="error">{{form.errors.password}}</span></label>
      <div class="password-field">
        <input v-model="form.fields.password" :type="showPassword ? 'text' : 'password'" :placeholder="$t('Password')">
        <img @click="showPassword = !showPassword" :src="'/images/svg/' + (showPassword ? 'hide' : 'show') + '.svg'">
      </div>
      
      <label>{{ $t('PromoCode') }} <span class="error">{{form.errors.promoCode}}</span></label>
      <input v-model="form.fields.promoCode" type="text" :placeholder="$t('PromoCodePlaceholder')">
      
      <span class="error">{{form.errors.termsAgreed}}</span>
      <label class="terms-label">
        <input type="checkbox" v-model="form.fields.termsAgreed">
        {{ $t('reisterForm.terms') }} 
        <router-link @click="modalStore.closeModal()" to="/terms">{{ $t('GeneralTermsandConditions') }}</router-link>.
      </label>
      
      <button class="primary modal-button" type="submit">{{ $t('SignUp')}}</button>

      <p class="link-button">{{ $t('AlreadyHaveAccount') }} <a @click="modalStore.openLoginModal">{{ $t('Login2') }}</a></p>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { z } from 'zod';
import zForm from '../utils/zform'
import { useAuthStore } from "../stores/authStore";
import { useModalStore } from "../stores/modalStore";
import api from "../api/api";
import i18n from '../i18n'; 

const authStore = useAuthStore();
const modalStore = useModalStore();

const formSchema = z.object({
  username: z.string().min(3),
  email: z.string().min(5).includes('@'),
  password: z.string().min(8),
  promoCode: z.string().optional(),
  termsAgreed: z.boolean()
});

var form = reactive(new zForm(formSchema));
const showPassword = ref(false);

async function register() {
  form.validate();

  if(!form.isValid)
    return;  
  
  try {
    const response = await api.auth.register(form.fields.username, form.fields.password, form.fields.email);
      authStore.login(response.data);
      modalStore.closeModal();
  } catch (error) {
    if(error.response) {
      form.errorSummary = error.response.data;
    } else {
      form.errorSummary = i18n.global.t('ErrorTryAgainText');
    }
  }
}

</script>

