<template>
  <div>
    <p class="error-summary">{{form.errorSummary}}</p>
    <form @submit.prevent="login" method="post" autocomplete="off" class="flex-col">
      <label>{{ $t('Username') }} <span class="error">{{form.errors.username}}</span></label>
      <input v-model="form.fields.username" type="text" :placeholder="$t('Username')">
      
      <label>{{ $t('Password') }} <span class="error">{{form.errors.password}}</span></label>
      <div class="password-field">
        <input v-model="form.fields.password" :type="passwordInputType" :placeholder="$t('Password')">
        <img v-if="showPassword" @click="toggleShowPassword" src="/images/svg/hide.svg" alt="">
        <img v-else @click="toggleShowPassword" src="/images/svg/show.svg" alt="">
      </div>

      <label v-if="mfaRequired">{{ $t("2FACode") }} <span class="error">{{form.errors.mfaCode}}</span></label>
      <input v-if="mfaRequired" type="text" v-model="mfaCode" autocomplete="off">
        
      <label class="terms-label">{{ $t('loginForm.terms') }} <router-link @click="modalStore.closeModal()" to="/terms">{{ $t('TermsofService') }}</router-link></label>
      
      <button class="primary modal-button" type="submit">{{ $t('Login') }}</button>
      
      <p class="link-button"><a @click="modalStore.openForgotPasswordModal">{{ $t('ForgotPassword') }}?</a></p>
      <p class="link-button">{{ $t('DontHaveAccount') }} <a @click="modalStore.openRegisterModal">{{ $t('SignUp') }}</a></p>

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
  password: z.string().min(8),
  // mfaCode: z.string().min(6),
});

var form = reactive(new zForm(formSchema));
const showPassword = ref(false);
const passwordInputType = ref('password');
const mfaRequired = ref(false);
const mfaCode = ref('');

function toggleShowPassword() {
  showPassword.value = !showPassword.value;
  passwordInputType.value = passwordInputType.value === 'password' ? 'text' : 'password';
}

async function login() {
  form.validate();

  if(!form.isValid)
    return;  

  try {
    const response = await api.auth.login(form.fields.username, form.fields.password, mfaCode.value);
    authStore.login(response.data);
    modalStore.closeModal();
  } catch (error) {
    if(error.response) {
      if(error.response.data == 'tfa required') {
        mfaRequired.value = true;
        form.errorSummary = '2fa code required';
      }
      else {
        form.errorSummary = error.response.data;
      }
    } else {
      form.errorSummary = i18n.global.t('ErrorTryAgainText');
    }
  }
}

</script>