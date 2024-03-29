#include <stdio.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <string.h>
#include <unistd.h>
#include <sys/poll.h>

#define MAX_MSG_LEN 1024

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

    struct pollfd events_read[2];
    events_read[0].fd = 0;
    events_read[0].events = POLLRDNORM;
    events_read[1].fd = socket_fd;
    events_read[1].events = POLLRDNORM;


    char rcv_buf[MAX_MSG_LEN], send_buf[MAX_MSG_LEN];

    while (1) {
        int rc = poll(events_read, 2, -1);
        if (rc <= 0) {
            printf("poll failed\n");exit(0);
        }

        //处理标准输入
        if (events_read[0].revents & POLLRDNORM) {
            bzero(send_buf, MAX_MSG_LEN);
            int rn = read(0, send_buf, MAX_MSG_LEN);
            if (rn < 0) {
                printf("read stdout error!\n");exit(0);
            }
            if (send_buf[rn-1] == '\n') {
                send_buf[rn-1] = 0;
            }

            if (strcmp(send_buf, "quit") == 0) {
                close(socket_fd);
                return 0;
            }

            int sn = send(socket_fd, send_buf, rn, 0);
            if (sn < 0) {
                printf("send data failed\n");exit(0);
            }
        }

        //处理 socket 读事件
        if (events_read[1].revents & POLLRDNORM) {
            bzero(rcv_buf, MAX_MSG_LEN);
            int rrn = read(socket_fd, rcv_buf, MAX_MSG_LEN);
            if (rrn < 0) {
                printf("read socket error!\n");exit(0);
            }else if (rrn == 0) {
                printf("server closed!\n");exit(0);
            }

            rcv_buf[rrn] = 0;
            printf("%s\n", rcv_buf);
        }
    }
}

