## redis

* 基本数据类型
    String: set get setnx incr decr delete exists expire ttl
    List: lpush、lpop、rpush、rpop
    Hash: hset、hget、hgetall、hkeys、hvals、
    Set: sadd、smembers、sdiff、sinter
    Sorted Set: zadd、zcount、zrank

* redis、memcache
    memcache 仅支持 k/v，而 redis 支持更丰富的数据类型
    memcache 不支持持久化，redis 支持
    memcache 支持多核，redis 仅支持单线程，不需要锁

* 数据结构实现
    SDS: 简单动态字符串
        存储 length，O(1)获取长度，防止缓冲区溢出；
        预分配内存，小于 1M 二倍扩展，大于 1M多分配 1M
        惰性空间释放
    链表：
        用于列表键、发布订阅等
        双向无环、带头尾指针、带长度
    字典：
        单向链表解决冲突
        rehash：大小总是为2 的 n 次方，负载大于 1 或者小于 0.1 自动 rehash
        渐进式 rehash: 新增键插入新 hash表，同时每次 rehash 一个键到新表，最终完成
    跳跃表：
        实现简单，性能媲美二叉树
    整数集合：
        自动升级，节约内存
    压缩列表：
        节约内存，每个节点保存previous_entry_length、encoding、content

* reids对象：
    意义：
        结构 type、encoding、ptr、refcount、lru(该对象最后被访问的时间)
        对数据类型的封装，根据不同场景有不同的实现
        实现命令权限验证，基于引用计数的共享对象和垃圾回收
    字符串：
        数字时使用int；小于 32 字节时使用 embstr(一块内存存储 object 和 SDS)；大于 32 字节时使用 SDS
    列表
        所有字符串长度小于 64 字节，且元素数小于 512时，使用压缩列表编码；否则使用双向链表。
    HASH
        所有键、值长度小于 64字节，且键值对数小于 512时，使用压缩列表编码；否则使用 hashtable
    集合
        所有键都是整数值，数量小于 512 时，使用整数集合；否则使用 hashtable，值全为 null
    有序集合
        所有元素长度小于 64字节，且元素数量小于 128时，使用压缩列表编码；
        否则同时使用跳表和 hashtable，两者共享对象，提升查询和排序性能。
* 对象共享
    redis 初始化时会创建 0-9999的所有字符串对象

* 过期删除
    惰性删除和定时删除

* 持久化
    RDB 持久化
        RDB 文件为二进制文件，不同类型数据格式不同
        保存所有键值对数据，启动时自动加载
        SAVE 命令会阻塞服务器，BGSAVE 命令不会
    AOF 持久化
        AOF为文件记录对数据库的写命令
        appendfsync 参数控制 aof_buf持久化机制，always 每次时间循环，everysec每秒，no由操作系统管理。
        AOF重写可以减小 AOF文件的体积，通过读取数据库中的键值对实现。

