<template>
  <div class="container">
    <div class="todo_menu">
      <Menus></Menus>
    </div>
    <div class="todo_list">
      <Todo></Todo>
    </div>
  </div>
</template>

<script>
  import Menus from './Menus';
  import Todo from './Todo';
    export default {
        name: "Layouts",
      components: {
          Menus,
          Todo
        },
    }
</script>

<style scoped>
  .container {
    width: 850px;
    height: 600px;
    margin-left: auto;
    margin-right: auto;
    vertical-align: top;
  }
  .todo_menu {
    background: lightcyan;
    width: 240px;
    height: inherit;
    display: inline-block;
    vertical-align: top;
  }

  .todo_list {
    background: lightgray;
    width: 560px;
    height: inherit;
    display: inline-block;
    vertical-align: top;
  }
</style>
