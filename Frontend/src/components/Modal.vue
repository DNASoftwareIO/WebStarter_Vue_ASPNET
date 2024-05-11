<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="store.component != null" @click.self="store.closeModal" class="modal-mask" aria-modal="true" role="dialog" tabindex="-1">
        <div class="modal-container">
          <div @click="store.closeModal" class="modal-close-button">X</div>
          <component :is="store.component" v-bind="store.props" />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { onMounted, onUnmounted } from "vue";
import { useModalStore } from "../stores/modalStore";

const store = useModalStore();

function keydownListener(event) {
  if (event.key === "Escape") store.closeModal();
}

onMounted(() => {
  document.addEventListener("keydown", keydownListener);
});

onUnmounted(() => {
  document.removeEventListener("keydown", keydownListener);
});

</script>

<style>
.modal-mask {
  position: fixed;
  z-index: 9998;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: var(--modal-mask);
  display: flex;
  transition: opacity 0.3s ease;
  backdrop-filter: saturate(180%) blur(15px);
}

.modal-container {
  color: var(--primary-text-color);
  width: 600px; 
  margin: auto;
  padding: 1.25rem 1.875rem;
  background-color: var(--secondary-bg-color);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.33);
  transition: all 0.3s ease;
  height: 100%;
}

.modal-close-button {
  cursor: pointer;
  display: flex;
  justify-content: end;
}

/* transition animations */
.modal-enter-from {
  opacity: 0;
}

.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  -webkit-transform: scale(1.1);
  transform: scale(1.1);
}

@media only screen and (min-width: 765px) {
  .modal-container {
    border-radius: 20px;
    height: auto;
  }
}
</style>