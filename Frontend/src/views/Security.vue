<template>
  <AccountNav/>
  <div class="security-container">
  <div class="card">
    <h3>{{ $t('ChangePassword') }}</h3>
    <div v-if="changePasswordSuccess" class="success-alert">
      <div>
          <img src="/images/svg/alert-green-check.svg" alt="icon">
      </div>
      <div>
          <h3>{{ $t('PasswordChanged') }}</h3>
          <p>{{ $t('PasswordChangedSuccess') }}</p>
      </div>
    </div>
    <p v-if="passwordForm.errorSummary != ''" class="error-summary">{{passwordForm.errorSummary}}</p>
    <form @submit.prevent="changePassword" method="post" autocomplete="off">
      <label>{{ $t('OldPassword') }} <span class="error">{{passwordForm.errors.oldPassword}}</span></label>
      <div class="password-field">
        <input v-model="passwordForm.fields.oldPassword" :type="showOldPassword ? 'text' : 'password'" :placeholder="$t('OldPassword')">
        <img @click="showOldPassword = !showOldPassword" :src="'/images/svg/' + (showOldPassword ? 'hide' : 'show') + '.svg'">
      </div>

      <label>{{ $t('NewPassword') }} <span class="error">{{passwordForm.errors.newPassword}}</span></label>
      <div class="password-field">
        <input v-model="passwordForm.fields.newPassword" :type="showNewPassword ? 'text' : 'password'" :placeholder="$t('NewPassword')">
        <img @click="showNewPassword = !showNewPassword" :src="'/images/svg/' + (showNewPassword ? 'hide' : 'show') + '.svg'">
      </div>

      <label>{{ $t('ConfirmPassword') }} <span class="error">{{passwordForm.errors.confirmPassword}}</span></label>
      <div class="password-field">
        <input v-model="passwordForm.fields.confirmPassword" :type="showConfirmPassword ? 'text' : 'password'" :placeholder="$t('ConfirmPassword')">
        <img @click="showConfirmPassword = !showConfirmPassword" :src="'/images/svg/' + (showConfirmPassword ? 'hide' : 'show') + '.svg'">
      </div>

      <label v-if="authStore.tfaEnabled">{{ $t("TfaCode") }} <span class="error">{{passwordForm.errors.tfaCode}}</span></label>
      <input v-if="authStore.tfaEnabled" type="text" v-model="passwordFormTfaCode" autocomplete="off">

      <div class="justify-end">
        <button type="submit" class="primary">{{ $t('Save') }}</button>
      </div>
    </form>

  </div>

  <div class="card">
    <h3>{{ $t('TwoFactorAuth') }}</h3>
    <div v-if="toggleTfaSuccess" class="success-alert" style="padding: 0;">
      <p>{{ $t('TfaChangedSuccess') }}</p>
    </div>
    <p v-if="tfaForm.errorSummary != ''" class="error-summary">{{tfaForm.errorSummary}}</p>
    <form @submit.prevent="toggleTfa" method="post" autocomplete="off">
    <div class="tfa-container mb-10">
      <div v-if="!authStore.tfaEnabled" class="qrcode-container">
        <qrcode-vue :value="tfaUri" :size="240" level="H" />
      </div>
    <div>
      <div v-if="!authStore.tfaEnabled">
        <label>{{ $t('BackupCode') }}</label>
        <input v-model="tfaKey" type="text" disabled />
      </div>
      <div>
        <label>{{ $t('TfaCode') }} <span class="error">{{tfaForm.errors.tfaToken}}</span></label>
        <input v-model="tfaForm.fields.tfaToken" type="text" placeholder="">
      </div>
    </div>

  </div>
  <div class="justify-end mt-33">
    <button type="submit" class="primary">{{authStore.tfaEnabled ? i18n.global.t('DisableTfa') : i18n.global.t('EnableTfa') }}</button>
  </div>
  </form>
  </div>
</div>

</template>

<script setup>
import { ref, onMounted, reactive } from "vue";
import { z } from 'zod';
import QrcodeVue from 'qrcode.vue';
import zForm from '../utils/zform'
import AccountNav from '../components/AccountNav.vue';
import { useAuthStore } from "../stores/authStore";
import api from "../api/api";
import i18n from '../i18n'; 

const authStore = useAuthStore();

const passwordFormSchema = z.object({
  oldPassword: z.string().min(8),
  newPassword: z.string().min(8),
  confirmPassword: z.string().min(8),
});

const tfaFormSchema = z.object({
  tfaToken: z.string().min(6),
});

var tfaForm = reactive(new zForm(tfaFormSchema));
var passwordForm = reactive(new zForm(passwordFormSchema));
const showOldPassword = ref(false);
const showNewPassword = ref(false);
const showConfirmPassword = ref(false);
const changePasswordSuccess = ref(false);
const tfaKey = ref('');
const tfaUri = ref('');
const passwordFormTfaCode = ref(''); // TODO can we handle this with zod where it's only required if authStore.tfaEnabled is true?
const toggleTfaSuccess = ref(false);

onMounted(async () => {
  if (!authStore.tfaEnabled) {
    try {
    const response = await api.auth.getTfaCode();
    tfaKey.value = response.data.tfaKey;
    tfaUri.value = response.data.uri;
    tfaForm.fields.tfaToken = '';
    }
    catch (error) {
    if(error.response) {
      if(error.response.data == 'Cant get key if Tfa enabled.') {
        tfaKey.value = '';
        tfaUri.value = '';
        tfaForm.fields.tfaToken = '';
        authStore.toggleTfa();
      } else {
        tfaForm.errorSummary = error.response.data;
      }
    } else {
      tfaForm.errorSummary = i18n.global.t('ErrorTryAgainText');
    }
  }
  }
});

const toggleTfa = async () => {
  tfaForm.validate();
  if(!tfaForm.isValid)
    return;  
  
  try {
    await api.auth.toggleTfa(tfaForm.fields.tfaToken);
    tfaKey.value = '';
    tfaUri.value = '';
    tfaForm.fields.tfaToken = '';
    authStore.toggleTfa();
    toggleTfaSuccess.value = true;
    
    // TODO return this with the first request
    if (!authStore.tfaEnabled) {
      const response = await api.auth.getTfaCode();
      tfaKey.value = response.data.tfaKey;
      tfaUri.value = response.data.uri;
    }
  } catch (error) {
    if(error.response) {
      tfaForm.errorSummary = error.response.data;
    } else {
      tfaForm.errorSummary = i18n.global.t('ErrorTryAgainText');
    }
  }
}

const changePassword = async () => {
  passwordForm.validate();

  if(!passwordForm.isValid)
    return;  

  // TODO this should be handled by zod, but the refine feature loses the shape object used by the zForm class 
  if(passwordForm.fields.newPassword != passwordForm.fields.confirmPassword) {
    passwordForm.errorSummary = i18n.global.t('PasswordsDontMatch');
    return;
  }
  
  try {
    await api.auth.changePassword(passwordForm.fields.oldPassword, passwordForm.fields.newPassword, passwordFormTfaCode.value);
    changePasswordSuccess.value = true;
    passwordForm.reset();
    passwordFormTfaCode.value = '';
  } catch (error) {
    if(error.response) {
      passwordForm.errorSummary = error.response.data;
    } else {
      passwordForm.errorSummary = i18n.global.t('ErrorTryAgainText');
    }    
  }
};


</script>

<style scoped>
.qrcode-container {
  display: flex; 
  justify-content: center; 
  align-items: center; 
  background: #fff; /* changing from white can make the qr code scanning not work */
  height: 15.65rem; 
  width: 15.65rem;
  border-radius: 10px;

  margin: 0 auto;
  margin-bottom: 20px;
}

.security-container {
  display: flex;
  flex-direction: column;
  gap: 1.8rem;
}

.tfa-container {
  display: flex;
  flex-direction: column;
}

@media screen and (min-width: 64em) {
  .security-container {
    display: grid;
    grid-template-columns: 1fr 1fr;
    column-gap: 1.5rem;
  }

  .tfa-container {
    display: grid;
    grid-template-columns: 1fr 2fr;
    column-gap: 1.5rem;
  }

  .qrcode-container {
    margin-bottom: 0px;
  }
}




</style>