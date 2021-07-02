<template>
    <div class="todo">
      <div class="title">详情</div>

      <div class="content">
        <div class="item" v-for="item in todos">
         <input class="checkbox" type="checkbox" v-model="item.done" @click="click_check(item)">
          <span class="item_text" :class="{'text_del': item.done}">{{item.text}}</span>
          <button class="delete" @click="delTodo(item)">X</button>
        </div>
      </div>

      <div class="content">
        <input class="input" v-model="todoText"/>
        <button class="add" @click="addTodo">add</button>
      </div>

    </div>
</template>

<script>
  import axios from 'axios';

    export default {
        name: "list",
      data() {
          return {
              todos: [{
                done: false,
                text: 'example todo'
              }],
            todoText: '',
            checkedId: 0,
          };
      },
      watch: {
          '$route.params.id'() {
            this.initTodo();
          },
          todos: function () {
            this.updateTodo();
          }
      },
      created() {
        this.initTodo();
      },
      methods: {
        getTodo() {
          return axios.get(process.env.BASE_API+'/api/todo/'+this.checkedId).then(res=>res.data)
        },
        updateTodo() {
          return axios.put(process.env.BASE_API+'/api/todo/'+this.checkedId, {todo:this.todos});
        },

        initTodo() {
          this.checkedId = this.$route.params.id;
          this.getTodo().then(res=>{
            this.todos = res.data.todos;
          });
        },

        addTodo() {
            this.todos.push({'done':false,'text':this.todoText});
            this.todoText = '';
        },
        delTodo(item) {
            this.todos.splice(this.todos.indexOf(item, 1));
        },

        click_check(item) {
          item.done = !item.done;
          this.todos.splice(this.todos.indexOf(item), 1, item);
        }
      }
    }
</script>

<style scoped>
  .title {
    color: coral;
    font-size: 22px;
    margin: 20px;
  }

  .content {
    width: 100%;
    margin: 10px 0;
    float: left;
  }

  .item {
    width: 90%;
    margin: 10px;
    height: 25px;
    font-size: 16px;
    float: left;
  }

  .checkbox {
    margin-left: 30px;
    width: 10%;
    float: left;
    font-size: 16px;
  }

  .item_text {
    width: 75%;
    text-align: left;
    float: left;
  }

  .text_del {
    color: darkgray;
    text-decoration: line-through;
  }

  .delete {
    background-color: transparent;
    border-radius: 12px;
    color: orangered;
    float: left;
    font-size: 16px;
  }

  .input {
    float: left;
    border: 0;
    background: darkgray;
    border-radius: 5px;
    margin-left: 93px;
    padding: 5px;
    font-size: 16px;
    width: 55%;
  }

  .add {
    margin: 0 40px 0 0;
    padding: 5px 10px;
    background: darkgray;
    border-radius: 5px;
    text-align: center;
    font-size: 16px;
    float: right;
  }
</style>
