<template>
  <div v-if="!linkSent">
    <div class="flex-col-centerv mb-1">
    <h3>{{ $t('ResetPassword') }}</h3>
    <p>{{ $t('ResetPasswordDesc') }}</p>
    </div>
    <p class="error-summary">{{form.errorSummary}}</p>
    <form @submit.prevent="sendLink" method="post" autocomplete="off" class="flex-col">
      <label>{{ $t('Email') }} <span class="error">{{form.errors.email}}</span></label>
      <input v-model="form.fields.email" type="text" :placeholder="$t('Email')">
      
      <button class="primary modal-button" type="submit">{{ $t('Send Link') }}</button>
    </form>
  </div>
  <div v-if="linkSent" class="success-alert">
    <div>
        <img src="/images/svg/alert-green-check.svg">
    </div>
    <div>
        <h3>{{ $t('AlmostThere') }}...</h3>
        <p>{{ $t('ResetLinkSentText') }}</p>
    </div>
  </div>
  <p class="link-button"><a @click="modalStore.openLoginModal">{{ $t('BackToLogin') }}</a></p>

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
const linkSent = ref(false);

const formSchema = z.object({
  email: z.string().min(5).includes('@'),
});

var form = reactive(new zForm(formSchema));

async function sendLink() {
  form.validate();

  if(!form.isValid)
    return;  

  try {
    const response = await api.auth.forgotPassword(form.fields.email);
    linkSent.value = true;
  } catch (error) {
    if(error.response) {
      form.errorSummary = error.response.data;
    } else {
      form.errorSummary = i18n.global.t('ErrorTryAgainText');
    }
  }
}

</script>
