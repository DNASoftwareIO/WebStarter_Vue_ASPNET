import { ref, shallowRef } from 'vue';
import { defineStore } from "pinia";

import RegisterForm from "../components/RegisterForm.vue";
import LoginForm from "../components/LoginForm.vue";
import ForgotPasswordForm from "../components/ForgotPasswordForm.vue";

export const useModalStore = defineStore("modal-store", () => {
  const component = shallowRef(null);
  const modalProps = ref({});

  function openModal(newComponent, props = {}) {
    component.value = newComponent;
    modalProps.value = props;

    document.body.style.overflow = "hidden";
  }

  function closeModal() {
    component.value = null;
    modalProps.value = {};

    document.body.style.overflow = "auto";
  }

  function openRegisterModal() {
    openModal(RegisterForm);
  }

  function openLoginModal() {
    openModal(LoginForm);
  }

  function openForgotPasswordModal() {
    openModal(ForgotPasswordForm);
  }

  return { component, modalProps, closeModal, openRegisterModal, openLoginModal, openForgotPasswordModal }
});
