#include <stdio.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <strings.h>
#include <unistd.h>

#define MAX_MSG_LEN 10240


int main(int argc, char **argv) {
    if (argc != 3) {
        printf("usage: client <ip> <port>\n");exit(0);
    }

    int socket_fd;
    socket_fd = socket(AF_INET, SOCK_STREAM, 0);

    struct sockaddr_in server_addr;
    bzero(&server_addr, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = inet_addr(argv[1]);
    server_addr.sin_port = htons(atoi(argv[2]));

    socklen_t server_len = sizeof(server_addr);
    int resc = connect(socket_fd, (struct sockaddr *) &server_addr, server_len);
    if (resc < 0) {
        printf("connect failed!");exit(0);
    }
    printf("connect to server %s:%s success!\n", argv[1], argv[2]);

    char rcv_buf[MAX_MSG_LEN];
    while (1) {
        bzero(rcv_buf, MAX_MSG_LEN);
        int rn = read(0, rcv_buf, MAX_MSG_LEN);
        if (rn < 0) {
            printf("read stdout error!\n");exit(0);
        }

        int sn = send(socket_fd, rcv_buf, rn, 0);
        if (sn < 0) {
            printf("send data failed\n");exit(0);
        }

        bzero(rcv_buf, MAX_MSG_LEN);
        int rrn = read(socket_fd, rcv_buf, MAX_MSG_LEN);
        if (rrn < 0) {
            printf("read socket error!\n");exit(0);
        }

        printf("receive message from server: %s\n", rcv_buf);
    }
}

