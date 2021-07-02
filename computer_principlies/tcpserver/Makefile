all : block select poll epoll


block : bin/block_server bin/block_client
bin/block_server : block/server.c
	mkdir -p bin
	gcc block/server.c -o bin/block_server
bin/block_client : block/client.c
	mkdir -p bin
	gcc block/client.c -o bin/block_client

select : bin/select_server bin/select_client
bin/select_server : select/server.c
	mkdir -p bin
	gcc select/server.c -o bin/select_server
bin/select_client : select/client.c
	mkdir -p bin
	gcc select/client.c -o bin/select_client

poll : bin/poll_server bin/poll_client
bin/poll_server : poll/server.c
	mkdir -p bin
	gcc poll/server.c -o bin/poll_server
bin/poll_client : poll/client.c
	mkdir -p bin
	gcc poll/client.c -o bin/poll_client

epoll : bin/epoll_server bin/epoll_client
bin/epoll_server : epoll/server.c
	mkdir -p bin
	gcc epoll/server.c -o bin/epoll_server
bin/epoll_client : epoll/client.c
	mkdir -p bin
	gcc epoll/client.c -o bin/epoll_client


.PYTHON : clean
clean :
	rm -rf bin/