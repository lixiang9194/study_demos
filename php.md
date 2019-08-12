## PHP

* echo、print、print_r、var_dump区别
	echo能一次输出多个变量，print只能输出一个
	echo、print是语言结构，即PHP语言关键词，而print_r、var_dump是函数
	语言结构不能在ini文件禁用，其它语言结构：empty、isset、unset、die

* 单引号、双引号区别
	双引号可以被分析器识别，其中的变量会被替换，单引号原样输出

* isset、empty区别
	isset判断变量是否被设置且不为null，empty判断变量是否为空，0、false、空数组均返回true

* static、self、$this区别
	static:定义属性或方法为静态，另外static还有静态绑定功能，即调用的函数由最终继承的类来确定绑定。
	self:访问类的静态属性、方法、常量，指向当前定义所在的类
	$this:指向实际调用时的对象

* include、require、include_once、require_once
	include失败产生warning，脚本继续执行，require失败产生ERROR，中止执行；而_once只包含一次

* 数组常用函数
	count、sort、array_keys、array_values、array_merge

* 预定义变量 
	$GLOBALS、$_SERVERS、$_GET、$POST、$argc、$argv

* php.ini配置
	disable_functions、max_execution_time、memory_limit，error_log、upload_max_filesize、post_max_size
	动态设置函数 ini_set

* php-fpm设置
	pid、error_log、listen

* php数据类型
	4种标量：integer、float、string、boolean
	3种符合类型：array、object、callable
	2种特殊类型：resource、null

* for、foreach
	for使用原数组，每次根据key取数据；而foreach拷贝原数组，通过指针向前移动，通常更快

* PHP数组实现
	5.6 HashTable + 双向链表解决冲突，同时双向链表实现有序访问 =>内存碎片化，指针结构复杂
	7.0 arData数组按顺序插入数据，HashTable存储数组索引，使用单向链表解决冲突	=>内存连续，分配次数少
	7.1 数组和hash表使用同一段内存，hash表存储在数组之前

* PHP垃圾回收
	引用计数算法，当变量引用计数为0时，回收变量内存。
	另外，为解决循环引用问题，5.3引入新算法，1.将refcount减少后大于0的可能根放入根缓冲区，2.根缓冲区满后，进行深度优先遍历，将变量引用计数减1，3.深度优先遍历，将不为0的refcount加1 4.移除refcount大于0的变量，销毁refcount为0的垃圾

* PHP7新特性
	标量类型声明、返回值类型声明、null合并运算符？？、组合比较符<=>、define定义常量数组

* PHP7底层优化
	ZVAL结构体优化，内部不再保存引用计数，由24字节降低为16字节，数组实现由hashtable变为zend array
	Zend引擎细节的大量优化，减少内存分配次数，使用大块连续内存，提高缓存命中率

* PHP设计模式
	单例、工厂、适配器、观察者

* composer
	依赖版本管理工具，根据composer.json文件安装依赖包，使用composer.lock文件锁定版本，根据PSR-4规则实现自动加载。
	自动加载规则：psr-0、psr-4(生成namespace为key，dirpath为value的文件)、classmap、files

* PSR规范
	PSR-1:基本编码风格		遵循PSR-4，类名大写开头，方法名小写开头，驼峰命名法，常量字母全部大写
	PSR-2:更严格的编码风格	类属性和方法必须添加访问修饰符，一行不超过120字符
	PSR-3:日志记录器接口	8个日志级别
	PSR-4:自动加载规范		类名必须与对应.php后缀文件同名