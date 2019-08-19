## MySQL

* 体系结构
	客户端、Server（连接器、查询缓存、分析器、优化器、执行器）、存储引擎

* 数据类型
	整数5种  tinyint 1字节    smallint 2字节     mediumint 3字节    int 4字节    bigint 8字节
	浮点型3种    float 单精度    double 双精度    decimal 高精度
	日期类型5种   year 1字节    date 3字节    time 3字节     datetime 8字节    timestamp 4字节
	字符串类型6种    char(n) 固定长度    vchar(n) 可变长度    tinytext 可变长度    text 可变长度    mediumtext    longtext
	符合类型    ENUM 枚举   SET 集合

* char vchar区别
	char是固定长度，字符串被补空格，vchar是可变长度，需要1-3字节存储长度

* 设计范式
	第一范式：属性具有原子性，不可再分解
	第二范式：唯一性约束，所有非主键依赖于主键
	第三范式：冗余性约束，非主键之间不能互相依赖

* SQL语言分类
	DDL 数据库定义语言    DML 数据库操纵语言    DQL 数据库查询语言    DCL 数据库控制语言

* left join、 right join、 inner join、outer join
	left join以左表为基础，显示右表符合条件的记录，右表不足的显示NULL， right join相反
	inner join仅显示左右表均满足条件的记录，outer join显示左右表所有记录，不满足的部分显示NULL

* union、union all
	union all直接返回，union会排序去重后再返回，相对较慢

* 存储引擎
	InnoDB 支持事务、行级锁，支持崩溃恢复，5.5开始成为默认存储引擎
	MyISAM 不支持事务、行级锁，崩溃后可能丢失数据

* 事务ACID
	原子性、一致性、隔离性、持久性

* 隔离级别
	带来问题：脏读 不可重复读 幻读
	读未提交     读不加锁，写加排他锁
	读提交    纯锁实现性能差，因此采用锁+MVCC实现，依赖undo log与read view。undo log记录数据的多个版本，read view判断数据版本的可见性。
	可重复读    事务开始时生成readview，而读提交每次执行生成一份readview。
	串行化    读加共享锁，写加排他锁

* 更新数据流程
	undo buffer存储数据历史版本，与redo buffer同样为环形缓冲，但缓冲满时被刷新到主ibd文件。
	readview 创建新事务时，innodb将当前活跃事务列表保存一个副本，当读取改行记录时，将当前事务版本号与readview进行比较。
	事物1更改某行，用排它锁锁定改行，记录redo log，把当前值copy到undo log，修改当前行值，填写事物编号，使回滚指针指向undo log
	意向锁：不与行级级锁冲突的表级锁，用来达到锁表前无需遍历行锁的目的。

* 索引
	逻辑角度： 普通索引、唯一索引、主键索引、组合索引、全文索引
	物理存储： 聚簇索引、非聚簇索引
	数据结构： B+数索引、hash索引、FULLTEXT索引、R-Tree索引

	聚簇索引：叶子节点就是数据节点，非聚簇索引的叶子节点存储主键值。

* B+树
	二叉树高度过高，IO次数多；B-Tree每个节点都是数据，需要频繁分裂、合并；B+树非叶子节点只存储索引key,叶子节点存储data,叶子节点间双链表以支持范围查询。
	InnoDB默认数据页16KB,叶子节点key占8字节，pointer占6字节，因此一页共16*1024/14约1170个节点，因此通常2~3层

* 索引失效场景
	1.组合索引没有使用最左列
	2.使用不等号、函数
	3.部分or列无索引
	4.like模板以通配符开头
	5.查询列是字符串，未用引号括起来
	6.join操作主外键类型不一致
	7.mysql估计全表扫描比使用索引快

* redo log
	本质上将随机磁盘写变成了顺序磁盘写，从而提高速度。修改数据前先写redo log到redo log buffer,再在内存中修改数据，事务提交时，先prepare,写redo log到磁盘，然后写binlog到磁盘，最后才完成事务的commit。
	innodb_flush_at_trx_commit参数控制事务提交时redo log的写入策略，0不刷，1持久化到磁盘，2刷到文件缓存。
	InnoDB后台会每秒刷新redo log buffer到磁盘，因此未提交的redo log可能已经持久化。
	sync_binlog参数控制bin log的写入策略，0不刷，1每次持久化，n为n次进行一次持久化。双1才能保证数据安全。

* change buffer
	将对二级索引的数据操作缓存下来，以减少二级索引的随机IO,达到合并操作的效果。
	5.5版本前只支持insert操作，叫做insert buffer，后来支持了更多操作类型，改名为change buffer。

* 常用参数
	innodb_flush_at_trx_commit
	sync_binlog
	innodb_buffer_pool_size 控制 innodb使用的缓存大小，通常为物理内存的 70%至 80%
	innodb_io_capacity 控制 innodb写日志的速度

