# coding=utf-8
import sys
reload(sys)
sys.setdefaultencoding('utf-8')

from scrapy.spiders import Spider
from scrapy.selector import Selector
import scrapy
from ..items import ProxyItem


class proxy_spider(Spider):
    name = 'proxy_spider'
    custom_settings = {
        'ITEM_PIPELINES': {'lzy_spider.pipelines.ProxyPipeline': 300},
        'DOWNLOADER_MIDDLEWARES': {}
    }
    start_urls = ['http://www.xicidaili.com/wt/']
    def start_requests(self):
        for url in self.start_urls:
            yield scrapy.Request(url=url, callback=self.parse_proxy)

    def parse_proxy(self, response):
        sel = Selector(response)
        proxies = sel.xpath('//table[@id="ip_list"]/tr')
        for proxy in proxies:
            try:
                proxy_item = ProxyItem()
                proxy_item['type'] = proxy.xpath('./td[6]/text()').extract()[0]
                proxy_item['ip'] = proxy.xpath('./td[2]/text()').extract()[0]
                proxy_item['port'] = proxy.xpath('./td[3]/text()').extract()[0]
                proxy_item['time'] = proxy.xpath('./td[9]/text()').extract()[0]
                proxy_item['valid'] = False
                yield proxy_item
            except:
                pass
