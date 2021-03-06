/* eslint-disable no-unused-vars */
import Vue from 'vue'
import App from './App'
import store from './store/store'
import router from './router'
import { sync } from 'vuex-router-sync'
import semantic from 'semantic'

sync(store, router)

/* eslint-disable no-new */
new Vue({
  router,
  store,
  ...App
}).$mount('#app')
