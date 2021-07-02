# coding=utf-8
import sys
reload(sys)
sys.setdefaultencoding('utf-8')

from scrapy.spiders import Spider
from scrapy.selector import Selector
import scrapy
from send_email import send_email


class bitcoin_spider(Spider):
    name = 'bitcoin_spider'
    custom_settings = {
        #'ITEM_PIPELINES': {'lzy_spider.pipelines.bitcoinPipeline': 300},
        'DOWNLOADER_MIDDLEWARES': {}
    }
    start_urls = ['https://www.coingecko.com/zh/%E4%BB%B7%E6%A0%BC%E5%9B%BE/%E6%AF%94%E7%89%B9%E5%B8%81/cny']
    
    def start_requests(self):
        for url in self.start_urls:
            yield scrapy.Request(url=url, callback=self.parse_page)

    def parse_page(self, response):
        sel = Selector(response)

        price = sel.xpath('//tbody/tr/td/span/text()').extract()[0]
        curr_price = float(price.split(' ')[1].replace(',',''))
        self.differ_price(curr_price)

    def differ_price(self, curr_price):
        try:
            ff = open('pre_price.txt','r')
            pre_price = float(ff.readline().strip())
        except Exception, e:
            pre_price = 0
        
        if abs(curr_price - pre_price) >= 5000 :
            print 'curr_price %s'%curr_price
            ff = open('pre_price.txt','w')
            ff.write(str(curr_price//5000*5000))
            send_email(str(curr_price))
        
        

   
        
    
