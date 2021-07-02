## Nginx

* 匹配规则
    =精确匹配    ~正则匹配    ~*正则不区分大小写    ^~普通最佳匹配
    先查找精确匹配，再查找最长匹配，然后按顺序匹配正则，匹配到则返回，否则使用最长匹配。

* php 配置
    location / {
        try_files $uri $uri/ /index.php?$query_string;
    }
    location ~ \.php {
        fastcgi_split_path_info ^(.+\.php)(/.+)$
        fastcgi_pass 127.0.0.1:9000;
    }

* vue 配置
    location / {
        try_files $uri $uri/ /index.html last;
        index index.html;
    }

* tcp 配置
    stream {
        server {
            listen 3320;
            proxy_pass 127.0.0.1:3360;
        }
    }

* proxy_cache
    proxy_cache_path files level=1:2 key_zones=my_cache:10m
    proxy_cache my_cache;

* nginx 负载均衡算法
    轮询、加权轮询、最少连接、hash

* nginx 进程
    一个 Master，多个 Worker进程
    IO 多路复用，使用 epoll算法

* nginx lua
    lua 原生支持协程，为每个新连接创建一个协程，占用资源小，切换快，以同步的方式写程序，实现了异步处理的效率。
    通过 ngx_http_lua_module在 postconfiguration 阶段创建 lua 环境，并且创建了 ngx全局变量用于交互。
    kong 定义了 7 个阶段，init、certificate、rewrite、acess、header_filter、body_filter、log
