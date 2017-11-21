# -*- coding: utf-8 -*-

# Define your item pipelines here
#
# Don't forget to add your pipeline to the ITEM_PIPELINES setting
# See: http://doc.scrapy.org/en/latest/topics/item-pipeline.html

from .items import ProxyItem
import requests


class ProxyPipeline(object):
    def __init__(self):
        self.proxy_file = open('./lzy_spider/proxy_file.py', 'w')
        self.proxy_file.write('PROXIES=[\n')

    def process_item(self, item, spider):
        if isinstance(item, ProxyItem):
            self.verify_proxy(item)
            self.save_proxy(item)

    def close_spider(self, spider):
        self.proxy_file.write(']\n')
        self.proxy_file.close()

    def verify_proxy(self, item):
        if item['time']>24:
            proxies = {'http': '{0}://{1}:{2}'.format(item['type'], item['ip'], item['port'])}
            try:
                requests.get('http://baidu.com', proxies=proxies, timeout=3)
            except Exception as e:
                pass
            else:
                print 'successful get proxy {0}://{1}:{2}'.format(item['type'], item['ip'], item['port'])
                item['valid'] = True

    def save_proxy(self, item):
        if item['valid']:
            str_dict = "'addr': '{0}://{1}:{2}'".format(item['type'], item['ip'], item['port'])
            self.proxy_file.write('{'+str_dict+'},\n')