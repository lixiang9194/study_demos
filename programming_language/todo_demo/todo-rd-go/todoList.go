package main

type TodoDetail struct {
	Done bool `json:"done"`
	Text string `json:"text"`
}

type Todo struct {
	Id string `json:"id"`
	Lock string `json:"lock"`
	Title string `json:"title"`
	Todos []TodoDetail `json:"todos"`
}
