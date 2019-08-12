### PHP框架

#### Laravel

* IocContainer
	IOC即控制反转，对象间依赖接口而非具体实现，将控制权交给容器。Ioc容器在全局维护类名和对象实例的集合，通过反射机制检查类型，递归解决依赖，在程序运行期间自动注入需要的参数，实现程序解耦。

* serviceProvider
	向容器完成服务的注册，以及做初始化启动操作。实现ServiceProvider接口中的register和boot方法，容器启动时会自动进行调用，另外为了性能还可以进行延迟加载。

* 容器启动过程
	1.容器实例化 2.注册容器本身 3.注册基础服务提供者，如事件、路由 4.核心类别名注册，如app、auth 5.基础路径注册

* 请求准备，Kernel启动过程
	环境检测、配置加载、日志配置、异常处理、外观注册、服务提供者注册、启动服务


* Facade
	一种设计模式，通常称为外观模式，提供了静态接口去访问容器中的类方法，通过__callStatic魔术方法实现，优点是简单易记

* 中间件
	使用装饰器模式对请求进行处理，只需实现相应方法，并在kernel中注册，即可使用，可以在中间件中进行权限的验证。

* 队列
	可以对耗时的任务进行异步处理，缩短响应时间。Laravel支持redis、mysql等驱动，创建队列任务需要实现shouldQueue接口，通过dispatch方法进行任务分发，Laravel会将任务序列化存入redis，实际执行时在取出反序列化。

* 事件
	简单的观察者模式实现，实现代码解耦，一个事件可以有多个互相独立的监听器。监听器也可以实现shouldQueue接口，从而异步进行处理。


#### Yaf

* 优点
	C语言开发，PHP启动时加载，常驻内存，因此性能更高

* 插件机制
	定义了6个Hook,routerStartup,routerShutdown,dispatchLoopStartup,preDispatch,postDispatch,dispatchLoopShutdown

* 路由
	默认路由器为Yaf_Router，默认路由协议为Yaf_Route_Static，另外还有Yaf_Route_Simple、Yaf_Route_Regex等