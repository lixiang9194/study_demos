package main

import (
	"github.com/gin-gonic/gin"
	"net/http"
)

func main() {
	gin.SetMode(GIN_MODE)
	engine := gin.Default()
	engine.GET("/hello", WebRoot)

	//route for vue-todos
	engine.GET("/api/todoList", getTodoList)
	engine.POST("/api/todo", addTodo)
	engine.DELETE("/api/todo/:id", delTodo)
	engine.GET("/api/todo/:id", getTodo)
	engine.PUT("/api/todo/:id", updateTodo)

	engine.Run(LISTEN_ADDR)
}

func WebRoot(context *gin.Context) {
	context.String(http.StatusOK, "hello, world")
}
