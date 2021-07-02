<template>
  <div class="menus">

    <div class="menu" v-for="item in todoList" @click="checkTodo(item.id)" :class="{'hight': item.id === checkedId}">
      <span hidden="hidden">{{item.id}}</span>
      <!-- <span class="lock">{{item.lock}}</span> -->
      <span class="lock">a</span>
      <span class="title">{{item.title}}</span>
      <!-- <span class="count">{{item.todos.length}}</span> -->
      <button class="delete" @click="delTodo(item)">X</button>
    </div>

    <div class="menu">
      <input class="input" v-model="todoName"/>
      <button class="add" @click="addTodo">add</button>
    </div>
  </div>
</template>

<script>
  import axios from 'axios';
  import uuidv4 from 'uuid/v4';

  export default {
    name: "Menus",
    data() {
      return {
        todoList: [{
          'id': 0,
          'lock': 'N',
          'title': 'example',
          'todos': []
        }],
        checkedId: 0,
        todoName: 'new todo'
      };
    },
    watch: {
      checkedId: function (newId, oldId) {
        this.$router.push({name: 'todo', params: {id: newId}});
      }
    },
    created() {
      this.getTodoList().then(res => {
        this.todoList = res.data;
        this.checkedId = res.data[0].id;/*默认选中第一项*/
        })
    },
    methods: {

      getTodoList() {
        return axios.get(process.env.BASE_API+'/api/todoList').then(res => res.data);
      },

      addTodo() {
        const todo = {id:uuidv4(), lock:'N', title:this.todoName, todos:[]};
        this.todoList.push(todo);
        axios.post(process.env.BASE_API+'/api/todo', {todo:todo});
      },
      delTodo(item) {
        this.todoList.splice(this.todoList.indexOf(item), 1);
        axios.delete(process.env.BASE_API+'/api/todo/'+item.id);
      },

      checkTodo(id) {
        this.checkedId = id;
      },
    }
  }
</script>

<style scoped>
  .menus {
    margin-top: 20px;
  }

  .menu {
    margin: 10px 10px;
    width: 95%;
    font-size: 18px;
    float: left;
  }

  .hight {
    font-size: 20px;
    background-color: lightgoldenrodyellow;
  }

  .lock {
    color: chocolate;
    width: 50px;
    float: left;
  }

  .title {
    color: cornflowerblue;
    text-align: left;
    width: 120px;
    float: left;
  }

  .count {
    color: blueviolet;
    text-align: left;
    width: 30px;
    float: left;
  }

  .delete {
    background-color: transparent;
    border-radius: 12px;
    margin-top: 3px;
    color: orangered;
    float: right;
    font-size: 12px;
  }

  .input {
    float: left;
    border: 0;
    background: lightblue;
    border-radius: 5px;
    margin-left: 18px;
    padding: 3px;
    font-size: 16px;
    width: 40%;
  }

  .add {
    margin: 0 40px 0 0;
    padding: 3px 10px;
    background: lightblue;
    border-radius: 5px;
    text-align: center;
    font-size: 16px;
    float: right;
  }
</style>
