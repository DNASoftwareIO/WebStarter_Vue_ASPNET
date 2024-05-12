<template>
    <div class="reset-password-container card">
      <h4>{{ $t('ResetYourPassword') }}</h4>
      <div v-if="!passwordReset">
        <p>{{ $t('EnterNewPassDesc') }}.</p>
        <p v-if="form.errorSummary != ''" class="error-summary">{{form.errorSummary}}</p>
        
        <form @submit.prevent="resetPassword" method="post" autocomplete="off" class="flex-col">
          <label>{{ $t('NewPassword') }} <span class="error">{{form.errors.newPassword}}</span></label>
          <div class="password-field">
            <input v-model="form.fields.newPassword" :type="showNewPassword ? 'text' : 'password'" :placeholder="$t('NewPassword')">
            <img @click="showNewPassword = !showNewPassword" :src="'/images/svg/' + (showNewPassword ? 'hide' : 'show') + '.svg'">
          </div>

          <label>{{ $t('ConfirmPassword') }} <span class="error">{{form.errors.confirmPassword}}</span></label>
          <div class="password-field">
            <input v-model="form.fields.confirmPassword" :type="showConfirmPassword ? 'text' : 'password'" :placeholder="$t('ConfirmPassword')">
            <img @click="showConfirmPassword = !showConfirmPassword" :src="'/images/svg/' + (showConfirmPassword ? 'hide' : 'show') + '.svg'">
          </div>

          <button class="primary" type="submit">{{ $t('ResetPassword') }}</button>
        </form>
      </div>
      <div v-if="passwordReset" class="success-alert">
        <div>
            <img src="/images/svg/alert-green-check.svg">
        </div>
        <div>
            <h3>{{ $t('PasswordReset') }}</h3>
            <p>{{ $t('PasswordResetSuccess') }}</p>
        </div>
      </div>
      <div class="back-to-login">
        <img src="/images/svg/chevron-left.svg" alt="chevron-left">
        <router-link to="/">
          {{ $t('BackToHome') }}
        </router-link>
      </div>
      
    </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { useRoute } from "vue-router";
import { z } from 'zod';
import zForm from '../utils/zform'
import api from "../api/api";
import i18n from '../i18n'; 

const route = useRoute(); 

const passwordReset = ref(false);

const formSchema = z.object({
  newPassword: z.string().min(8),
  confirmPassword: z.string().min(8),
});

var form = reactive(new zForm(formSchema));
const showNewPassword = ref(false);
const showConfirmPassword = ref(false);

async function resetPassword() {
  form.validate();

  if(!form.isValid)
    return;  
  
  // TODO this should be handled by zod, but the refine feature loses the shape object used by the zForm class 
  if(form.fields.newPassword != form.fields.confirmPassword) {
    form.errorSummary = i18n.global.t('PasswordsDontMatch');
    return;
  }

  try {
    await api.auth.resetPassword(route.query.email, route.query.token, form.fields.newPassword);
    passwordReset.value = true;
  } catch (error) {
    if(error.response) {
      form.errorSummary = error.response.data;
    } else {
      form.errorSummary = i18n.global.t('ErrorTryAgainText');
    }
  }
}

</script>

<style scoped>
.reset-password-container {
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 2rem 4rem;

  margin: 0 auto;
  margin-top: 1.125rem;
  width: 40.5rem; 

}

h4 {
  font-size: 2rem;
  margin: 0 0 1rem 0;
  font-weight: 500;
}

p {
  margin: 0 0 2rem 0;
  color: var(--chevron);
}

a {
  display: flex;
  justify-content: end;
  margin-bottom: 3rem;
  font-size: 1.125rem;
  font-weight: 500;
}

.back-to-login {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-top: 1.5rem;
  column-gap: 0.375rem;
 } 
 
 .back-to-login a {
    margin-bottom: 0px;
  }
</style>