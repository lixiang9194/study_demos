package main

import (
	"github.com/gin-gonic/gin"
	"net/http"
)

func getTodoList(context *gin.Context) {
	todoList := getTodoListFromRedis()
	context.JSON(http.StatusOK, gin.H{"data": todoList})
}

func addTodo(context *gin.Context) {
	todoList := getTodoListFromRedis()
	todo := make(map[string]Todo)
	context.BindJSON(&todo)
	todoList = append(todoList, todo["todo"])
	setTodoListToRedis(todoList)
	context.JSON(http.StatusOK, gin.H{"data":"created success"})
}

func delTodo(context *gin.Context) {
	todoList := getTodoListFromRedis()
	id := context.Param("id")
	newTodoList := todoList[:0]
	for _, todo := range todoList {
		if todo.Id != id {
			newTodoList = append(newTodoList, todo)
		}
	}
	setTodoListToRedis(newTodoList)
	context.JSON(http.StatusOK, gin.H{"data":"deleted success"})
}

func getTodo(context *gin.Context) {
	todoList := getTodoListFromRedis()
	id := context.Param("id")
	var result Todo
	for _, todo := range todoList {
		if todo.Id == id {
			result = todo
		}
	}
	context.JSON(http.StatusOK, gin.H{"data": result})
}

func updateTodo(context *gin.Context) {
	todoList := getTodoListFromRedis()
	id := context.Param("id")
	newTodo := make(map[string][]TodoDetail)
	context.BindJSON(&newTodo)
	for i, todo := range todoList {
		if todo.Id == id {
			todoList[i].Todos = newTodo["todo"]
		}
	}
	setTodoListToRedis(todoList)
	context.JSON(http.StatusOK, gin.H{"data":"update success"})
}
