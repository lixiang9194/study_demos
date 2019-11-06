#include <stdio.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <strings.h>
#include <stdlib.h>
#include <unistd.h>

#define SERVER_PORT 10086
#define MAX_CONN 80
#define MAX_MSG_LEN 10240

int handle(int conn_fd);
char* run_cmd(char* cmd);

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

    while (1) {
        int conn_fd;
        struct sockaddr_in client_addr;
        socklen_t client_len = sizeof(client_addr);
        conn_fd = accept(listen_fd, (struct sockaddr*) &client_addr, &client_len);
        if (conn_fd < 0) {
            printf("accept failed!\n");exit(0);
        }
        printf("accept connection: %d\n", conn_fd);

        handle(conn_fd);
        close(conn_fd);
    }
}


int handle(int conn_fd) {
    char rcv_buf[MAX_MSG_LEN];
    char* send_buf;
    while (1) {
        int rcv_len = read(conn_fd, rcv_buf, MAX_MSG_LEN);
        if (rcv_len < 0) {
            printf("connection error!\n");
            return -1;
        } else if (rcv_len == 0) {
            printf("client closed!\n");
            return 0;
        }

        rcv_buf[rcv_len] = 0;
        printf("client: %d msg: %s \n", conn_fd, rcv_buf);

        //执行命令
        if (strcmp(rcv_buf, "pwd") == 0) {
            char pwd_buf[256];
            send_buf = getcwd(pwd_buf, 256);
        } else if (strncmp(rcv_buf, "ls", 2) == 0) {
            send_buf = run_cmd(rcv_buf);
        } else if (strncmp(rcv_buf, "cd", 2) == 0) {
            char target_dir[128];
            bzero(target_dir, sizeof(target_dir));
            memcpy(target_dir, rcv_buf + 3, rcv_len - 3);
            int rc = chdir(target_dir);
            if (rc == 0) {
                continue;
            }

            char err_str[32];
            sprintf(err_str, "chdir failed!\n");
            send_buf = err_str;
        } else {
            char err_str[32];
            sprintf(err_str, "error, command not support!\n");
            send_buf = err_str;
        }

        int wn = send(conn_fd, send_buf, strlen(send_buf), 0);
        if (wn < 0) {
            printf("send data failed!\n");
            return -1;
        }
    }
}

char* run_cmd(char* cmd) {
    char* res = malloc(10240);
    bzero(res, sizeof(*res));
    char buf[256];

    FILE* cmd_fp = popen(cmd, "r");
    if (!cmd_fp) {
        sprintf(res, "error execute %s!\n", cmd);
        return res;
    }

    char* res_head =  res;
    while (fgets(buf, 256, cmd_fp) != NULL) {
        memcpy(res, buf, strlen(buf));
        res += strlen(buf);
    }
    pclose(cmd_fp);

    return res_head;
}