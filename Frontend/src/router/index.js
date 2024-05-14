import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/authStore";
import { useModalStore } from "../stores/modalStore";

import Home from "../views/Home.vue";
import ResetPassword from "../views/ResetPassword.vue";
import ConfirmEmail from "../views/ConfirmEmail.vue";
import Sessions from "../views/Sessions.vue";
import Security from "../views/Security.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: Home,
    },
    {
      path: "/reset-password",
      name: "reset-password",
      component: ResetPassword,
    },
    {
      path: "/confirm-email",
      name: "confirm-email",
      component: ConfirmEmail,
    },
    {
      path: "/sessions",
      name: "sessoins",
      component: Sessions,
      meta: { requiresAuth: true }
    },
    {
      path: "/security",
      name: "security",
      component: Security,
      meta: { requiresAuth: true }
    },
  ],
  linkActiveClass: "active",
  // linkExactActiveClass: "exact-active",
});

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  const modalStore = useModalStore();

  if (to.meta.requiresAuth) {
    if (authStore.loggedIn) {
      next();
    } else {
      await authStore.loginFromStorage();
      if (authStore.loggedIn) {
        next();
      }
      else {
        next('/');
        modalStore.openLoginModal();
      }
    }
  } else if ((to.name === 'login' || to.name === 'forgot-password' || to.name === 'reset-password') && authStore.loggedIn) {
    next('/');
  } else {
    next();
  }
});

export default router;
