import Vue from 'vue'
import Router from 'vue-router'
import HelloWorld from '@/components/HelloWorld'
import Layouts from '@/components/Layouts'
import Todo from '@/components/Todo'

Vue.use(Router);

export default new Router({
  routes: [
    /*{
      path: '/hello',
      name: 'HelloWorld',
      component: HelloWorld
    },*/
    {
      path: '/',
      name: 'layouts',
      component: Layouts,
      children: [{
        path: '/todo/:id',
        name: 'todo',
        component: Todo
      }]
    }
  ]
})
