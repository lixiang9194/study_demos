#include <stdio.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <strings.h>
#include <stdlib.h>
#include <unistd.h>

#define SERVER_PORT 10086
#define MAX_CONN 80
#define MAX_MSG_LEN 10240


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

    int connfd;
    struct sockaddr_in client_addr;
    socklen_t client_len = sizeof(client_addr);
    connfd = accept(listen_fd, (struct sockaddr*) &client_addr, &client_len);
    if (connfd < 0) {
        printf("accept failed!\n");exit(0);
    }
    printf("accept connection: %d\n", connfd);

    char rcv_buf[MAX_MSG_LEN], send_buf[MAX_MSG_LEN];
    int count = 0;

    for (;;) {
        int n = read(connfd, rcv_buf, MAX_MSG_LEN);
        if (n < 0) {
            printf("connection error!\n");exit(0);
        } else if (n == 0) {
            printf("client closed!\n");exit(0);
        }

        rcv_buf[n] = 0;
        printf("received %d bytes: %s\n", n, rcv_buf);
        count++;

        sleep(5);

        bzero(send_buf, MAX_MSG_LEN);
        sprintf(send_buf, "Hi, %s", rcv_buf);

        int wn = send(connfd, send_buf, strlen(send_buf), 0);
        if (wn < 0) {
            printf("send data failed!\n");exit(0);
        }
        printf("send %d bytes\n", wn);
    }

}