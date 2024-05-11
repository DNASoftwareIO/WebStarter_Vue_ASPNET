<template>
  <section class="hero hero-account">
    <h1>{{ $t('Account') }}</h1>
  </section>
  <div class="justify-start account-nav">
    <router-link to="/security" class="tab">{{ $t('Security') }}</router-link>
    <router-link to="/sessions" class="tab" >{{ $t('Sessions') }}</router-link>
  </div>
</template>

<style scoped>
.account-nav {
  gap: 1rem;
  padding: 0.5rem 0.5rem;
  display: flex;
  overflow: auto;
}

.tab {
  display: flex;
  font-size: 1rem;
  color: var(--primary-text-color);
  height: 2rem;
  align-items: center;
  cursor: pointer; 
  padding: 0px 1.1rem;
  align-self: last baseline;
  white-space: nowrap;
}

.active {
  border-bottom: 2px solid var(--primary-color);
}

</style>