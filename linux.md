## Linux

* 输入输出
	0 标准输入    1标准输出    2标准错误    、dev/null文件
	> 输出重定向    < 输入重定向    >& 输出合并

* 查找命令
	find / -name 'example' 查找硬盘上的文件    grep -r -i 'example' * 查找文本内容

* 权限更改
	chown -R www:www path    chmod -R 777 path (rwx=>421)

* 文件统计
	wc [-wl] file

* xargs
	部分命令不接受标准输入，如kill、rm命令，xargs可将管道或标准输入换成命令行参数
	-d 指定分隔符    -p 询问执行    -n 指定一次传递几个参数

* lsof 查看打开文件
    -i 网络连接

* contrab
    6列 分钟 小时 日 月 星期 命令
    * 任何时刻    - 时间段    , 多个时段    /n 每隔n时间


* Linux 进程状态
    R(TASK_RUNNING)可执行状态
    S(TASK_INTERUPTIBLE) 可中断的睡眠状态 等待 socket 或信号量
    D(TASK_UNINTERUPTIBLE) 不可中断的睡眠状态 等待磁盘
    T(TASK_STOPPED、TASK_TRACED) 暂停或跟踪状态 被调试
    Z(TASK_DEAD-EXIT_ZOMBIE) 退出状态，僵尸进程
    X(TASK_DEAD-EXIT_DEAD) 退出状态，即将被销毁

* 进程间通信
    消息队列、管道、信号、套接字、共享内存、信号量

* 信号
    SIGSTOP SIGCONT SIGKILL SIGINT

* 进程、线程、协程
    进程是系统进行资源分配和调度的独立单位，占用独立内存，上下文切换开销大；
    线程是进程的一个实体，是 cpu调度的基本单位，可与同进程下其它线程共享资源，更轻量；
    协程是用户态的轻量级线程，调度由用户控制，有自己的寄存器上下文和栈。


* 内存管理
    内核使用 3-4G高地址空间，其中128M 动态映射高端内存，
    另896M直接映射物理内存，通过 Buddy和 slab 算法管理。Buddy算法负责大块连续内存分配，Slab分配器负责小内存分配，以Byte 为单位。

    用户使用低地址空间，用户空间划分为代码段、数据段、BSS、堆、栈。
    将物理内存按固定大小进行分页，用户地址空间也同样分页，产生缺页中断时映射虚拟内存到物理内存，逻辑上相邻的页物理上不一定连续。


* 系统负载 top、uptime、w
    显示机器最近 1 分钟、5 分钟、15 分钟的平均负载
    平均负载即处于可执行和不可中断的睡眠状态的进程数平均值，包括等待 CPU和 IO资源的进程，通过指数衰减算法计算。

