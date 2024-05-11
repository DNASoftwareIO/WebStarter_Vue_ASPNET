import { createApp } from 'vue'
import { createPinia } from "pinia";
import i18n from './i18n'; 
import './css/style.css'
import App from './App.vue'
import router from './router'

const pinia = createPinia();

var app = createApp(App);
app.use(router).use(pinia).use(i18n).mount('#app');

// uses Daggerhart's answer for to handle click outside events
//https://stackoverflow.com/questions/60144575/how-do-you-handle-click-outside-of-element-properly-in-vuejs
app.directive('click-outside', {
  beforeMount: function (element, binding) {
      //  check that click was outside the el and it's children
      element.clickOutsideEvent = function (event) {

          // and if it did, call method provided in attribute value
          if (!(element === event.target || element.contains(event.target))) {
    binding.value(event);
          }
      };

      document.body.addEventListener('click', element.clickOutsideEvent)
  },
  unmounted: function (element) {
    document.body.removeEventListener('click', element.clickOutsideEvent)
  }
});
