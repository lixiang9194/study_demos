#include <stdio.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <strings.h>
#include <stdlib.h>
#include <unistd.h>

#define SERVER_PORT 10086
#define MAX_CONN 1024
#define MAX_MSG_LEN 10240

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
    int resb = bind(listen_fd, (struct sockaddr*) &server_addr, sizeof(server_addr));
    if (resb < 0) {
        printf("bind error!\n");exit(0);
    }

    int res2 = listen(listen_fd, MAX_CONN);
    if (res2 < 0) {
        printf("listen failed!\n");exit(0);
    }
    printf("start server on port: %d\n", SERVER_PORT);

    fd_set fds_read, fds_read_mask;
    FD_ZERO(&fds_read);
    FD_SET(0, &fds_read);
    FD_SET(listen_fd, &fds_read);

    while (1) {
        fds_read_mask = fds_read;
        int n = select(FD_SETSIZE, &fds_read_mask, NULL, NULL, NULL);
        if (n <= 0) {
            printf("select failed\n");exit(0);
        }

        //处理标准输入
        if (FD_ISSET(0, &fds_read_mask)) {
            //
        }

        //处理连接
        if (FD_ISSET(listen_fd, &fds_read_mask))
        {
            struct sockaddr_in client_addr;
            socklen_t client_len = sizeof(client_addr);
            int conn_fd = accept(listen_fd, (struct sockaddr*) &client_addr, &client_len);
            if (conn_fd < 0) {
                printf("accept failed!\n");exit(0);
            }
            printf("accept connection: %d\n", conn_fd);

            //将连接加入 select 集合
            FD_SET(conn_fd, &fds_read);
        }

        //轮询所有连接获取输入
        char rcv_buf[MAX_MSG_LEN];
        char* send_buf;
        for (int i = listen_fd+1; i < MAX_CONN; i++)
        {
            if (FD_ISSET(i, &fds_read_mask))
            {
                bzero(rcv_buf, MAX_MSG_LEN);
                int rcv_len = read(i, rcv_buf, MAX_MSG_LEN);
                if (rcv_len < 0) {
                    printf("client %d error!\n", i);
                    continue;
                } else if (rcv_len == 0) {
                    //关闭连接并清除相关信息
                    printf("client %d closed!\n", i);
                    FD_CLR(i, &fds_read);
                    close(i);
                    continue;
                }

                //处理消息
                handle_msg(i, rcv_buf, rcv_len);
            }
        }
    }
}


//处理客户端消息
int handle_msg(int conn_fd, char* rcv_buf, int rcv_len) {
    char* send_buf;
    rcv_buf[rcv_len] = 0;
    printf("client: %d msg: %s \n", conn_fd, rcv_buf);

    //pwd 命令
    if (strcmp(rcv_buf, "pwd") == 0) {
        char pwd_buf[256];
        send_buf = getcwd(pwd_buf, 256);
    
    //ls 命令
    } else if (strncmp(rcv_buf, "ls", 2) == 0) {
        send_buf = run_cmd(rcv_buf);
    
    // cd 命令
    } else if (strncmp(rcv_buf, "cd", 2) == 0) {
        char target_dir[128];
        bzero(target_dir, sizeof(target_dir));
        memcpy(target_dir, rcv_buf + 3, rcv_len - 3);
        int rc = chdir(target_dir);
        //chdir 成功不返回信息
        if (rc == 0) {
            return 0;
        }

        //chdir 出错则返回错误信息
        char err_str[32];
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