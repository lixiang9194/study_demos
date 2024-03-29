#include <stdio.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/poll.h>
#include <fcntl.h>
#include <errno.h>

#define SERVER_PORT 10086
#define MAX_CONN 128
#define MAX_MSG_LEN 1024
#define MAX_PATH_LEN 128


int handle_msg(int conn_fd, char* rcv_buf, int rcv_len);
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
    fcntl(listen_fd, F_SETFL, fcntl(listen_fd, F_GETFL, 0) | O_NONBLOCK);
    int resb = bind(listen_fd, (struct sockaddr*) &server_addr, sizeof(server_addr));
    if (resb < 0) {
        printf("bind error!\n");exit(0);
    }

    int res2 = listen(listen_fd, MAX_CONN);
    if (res2 < 0) {
        printf("listen failed!\n");exit(0);
    }
    printf("start server on port: %d\n", SERVER_PORT);


    struct pollfd events[MAX_CONN];
    int i;
    for (i = 0; i < MAX_CONN; i++)
    {
        events[i].fd = -1;
    }
    events[0].fd = listen_fd;
    events[0].events = POLLRDNORM;


    char rcv_buf[MAX_MSG_LEN];
    char* send_buf;
    while (1) {
        int n = poll(events, MAX_CONN, -1);
        if (n <= 0) {
            printf("poll failed\n");exit(0);
        }

        //处理连接
        if (events[0].revents & POLLRDNORM)
        {
            struct sockaddr_in client_addr;
            socklen_t client_len = sizeof(client_addr);
            errno = 0;
            int conn_fd = accept(listen_fd, (struct sockaddr*) &client_addr, &client_len);

            if (conn_fd < 0) {
                //io not ready
                if (errno == EAGAIN) continue;
                printf("accept failed!\n");exit(0);
            }

            //printf("accept connection: %d\n", conn_fd);

            //将连接加入 poll 集合
            int i;
            for (i = 1; i < MAX_CONN; i++)
            {
                if (events[i].fd < 0)
                {
                    events[i].fd = conn_fd;
                    events[i].events = POLLRDNORM;
                    break;
                }
            }
            if(i >= MAX_CONN) {
                printf("connection has full, error!");
                close(conn_fd);
            }
        }

        //轮询所有连接获取输入
        int i;
        for (i = 1; i < MAX_CONN; i++)
        {
            int conn_fd = events[i].fd;
            if (conn_fd < 0) continue;
            if (events[i].revents & POLLRDNORM)
            {
                errno = 0;
                bzero(rcv_buf, MAX_MSG_LEN);
                int rcv_len = read(conn_fd, rcv_buf, MAX_MSG_LEN);
                if (rcv_len < 0) {
                    //io not ready
                    if (errno == EAGAIN) {
                        continue;
                    }
                    printf("client %d error!\n", conn_fd);
                    continue;
                } else if (rcv_len == 0) {
                    //关闭连接并清除相关信息
                    //printf("client %d closed!\n", conn_fd);
                    events[i].fd = -1;
                    close(conn_fd);
                    continue;
                }

                //处理消息
                handle_msg(conn_fd, rcv_buf, rcv_len);
            }
        }
    }
}


//处理客户端消息
int handle_msg(int conn_fd, char* rcv_buf, int rcv_len) {
    char* send_buf;
    rcv_buf[rcv_len] = 0;
    //printf("client: %d msg: %s \n", conn_fd, rcv_buf);

    //PING 命令
    if (strncmp(rcv_buf, "PING", 4) == 0) {
        char res_buf[8];
        sprintf(res_buf, "PONG\n");
        send_buf = res_buf;

    //pwd 命令
    } else if (strncmp(rcv_buf, "pwd", 3) == 0) {
        char pwd_buf[MAX_PATH_LEN];
        send_buf = getcwd(pwd_buf, MAX_PATH_LEN);

    //ls 命令
    } else if (strncmp(rcv_buf, "ls", 2) == 0) {
        send_buf = run_cmd(rcv_buf);

    // cd 命令
    } else if (strncmp(rcv_buf, "cd", 2) == 0) {
        char target_dir[MAX_PATH_LEN];
        bzero(target_dir, MAX_PATH_LEN);
        memcpy(target_dir, rcv_buf + 3, rcv_len - 3);
        int rc = chdir(target_dir);
        //chdir 成功不返回信息
        if (rc == 0) {
            return 0;
        }

        //chdir 出错则返回错误信息
        char err_str[16];
        sprintf(err_str, "chdir failed!\n");
        send_buf = err_str;

    //其它命令不支持
    } else {
        char err_str[32];
        sprintf(err_str, "error, command not support!\n");
        send_buf = err_str;
    }

    //向客户端发送消息
    int wn = send(conn_fd, send_buf, strlen(send_buf), 0);
    if (wn < 0) {
        printf("send data failed!\n");
        return -1;
    }
    return wn;
}


//通过popen执行命令
char* run_cmd(char* cmd) {
    char* res = malloc(MAX_MSG_LEN);
    bzero(res, MAX_MSG_LEN);
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