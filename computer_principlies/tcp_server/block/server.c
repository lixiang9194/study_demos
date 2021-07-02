#include <stdio.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>

#define SERVER_PORT 10086
#define MAX_CONN 128
#define MAX_MSG_LEN 1024

int handle(int conn_fd);


int main(int argc, char **argv) {
    int listen_fd;
    listen_fd = socket(AF_INET, SOCK_STREAM, 0);

    struct sockaddr_in server_addr;
    bzero(&server_addr, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(SERVER_PORT);

    int on = 1;
    setsockopt(listen_fd, SOL_SOCKET, SO_REUSEADDR, &on, sizeof(on));
    int resb = bind(listen_fd, (struct sockaddr*) &server_addr, sizeof(server_addr));
    if (resb < 0) {
        printf("bind error!\n");exit(0);
    }

    int res2 = listen(listen_fd, MAX_CONN);
    if (res2 < 0) {
        printf("listen failed!\n");exit(0);
    }
    printf("start server on port: %d\n", SERVER_PORT);

    int conn_fd;
    struct sockaddr_in client_addr;
    socklen_t client_len = sizeof(client_addr);

    while(1) {
        //接收客户端连接
        bzero(&client_addr, client_len);
        conn_fd = accept(listen_fd, (struct sockaddr*) &client_addr, &client_len);
        if (conn_fd < 0) {
            printf("accept failed!\n");exit(0);
        }
        //printf("accept connection: %d\n", conn_fd);

        //处理客户端请求
        handle(conn_fd);
    }
}


int handle(int conn_fd) {
    char rcv_buf[MAX_MSG_LEN], send_buf[MAX_MSG_LEN];

    for (;;) {
        bzero(rcv_buf, MAX_MSG_LEN);
        int rcv_len = read(conn_fd, rcv_buf, MAX_MSG_LEN);
        if (rcv_len < 0) {
            printf("client %d error!\n", conn_fd);
            return -1;
        } else if (rcv_len == 0) {
            //printf("client closed!\n");
            return 0;
        }
        rcv_buf[rcv_len] = 0;
        //printf("client: %d msg: %s \n", conn_fd, rcv_buf);

        bzero(send_buf, MAX_MSG_LEN);
        //PING 命令
        if (strncmp(rcv_buf, "PING", 4) == 0) {
            sprintf(send_buf, "PONG\n");
        } else {
            sprintf(send_buf, "error, command not support!\n");
        }

        int wn = send(conn_fd, send_buf, strlen(send_buf), 0);
        if (wn < 0) {
            printf("send data failed!\n");exit(0);
        }
        //printf("send %d bytes\n", wn);
    }
}