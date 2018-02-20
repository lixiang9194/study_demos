# coding=utf-8
import sys
reload(sys)
sys.setdefaultencoding('utf-8')

from scrapy.spiders import Spider
from scrapy.selector import Selector
import scrapy


class test_spider(Spider):
    name = 'test_spider'
    start_urls = ['http://ip.filefab.com/index.php']
    def start_requests(self):
        for url in self.start_urls:
            yield scrapy.Request(url=url, callback=self.parse_proxy)

    def parse_proxy(self, response):
        sel = Selector(response)
        from scrapy.shell import inspect_response
        inspect_response(response, self)
