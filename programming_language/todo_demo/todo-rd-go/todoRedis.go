package main

import (
	"github.com/go-redis/redis"
	"encoding/json"
)

func getTodoListFromRedis() []Todo {
	client := getRedisClient()
	defer client.Close()

	val, err := client.Get(TODO_REDIS_KEY).Result()
	if err != nil {
		data := make([]Todo, 0)
		details := make([]TodoDetail, 0)
		details = append(details, TodoDetail{false, "todo-detail"})
		data = append(data, Todo{"example-id", "N", "todo-title", details})
		return data
	}

	todoList := make([]Todo, 5)
	err = json.Unmarshal([]byte(val), &todoList)
	if err != nil {
		panic(err)
	}

	return todoList
}

func setTodoListToRedis(todoList []Todo) {
	client := getRedisClient()
	defer client.Close()
	data, err := json.Marshal(todoList)
	if err != nil {
		panic(err)
	}
	err = client.Set(TODO_REDIS_KEY, data, 60*60*24*365*10E9).Err()
	if err != nil {
		panic(err)
	}
}

func getRedisClient() *redis.Client {
	client := redis.NewClient(&redis.Options{
		Addr:     REDIS_HOST + ":" + REDIS_PORT,
		Password: REDIS_PASS,
		DB:       REDIS_DB,
	})
	return client
}
