#coding:utf-8
#本模块用于添加一些机器人
import requests
import urllib

def xiaoHJ(msg):
    url='http://dev.skjqr.com/api/weixin.php?email=2422610747@qq.com&appkey=8aec66092e95c77a7a1f15fc03f59eef&msg='
    print(url+msg)
    msg=msg.encode('GBK')
    #msg=urllib.parse.quote(msg)
    print(url+msg)
    r=requests.get(url+msg).text
    
    #rMsg=r.split('[msg]')[1].split('[/msg]')[0]
    f = open("out.html","w")
    f.write(r)
    return r


def tuLingRobot(msg):
    url='http://www.tuling123.com/openapi/api';
    key='65fd4c94176f384ef72a553f2c975768'
    data={'key':key,'info':msg}
    
    r=requests.post(url,data)
    rMsg=r.json()
    if rMsg['code']==100000:
        rText=rMsg['text']
    elif rMsg['code']==200000:
        rText=rMsg['text']+rMsg['url']
    else :
        rText="您的话我理解不了，算了，你讲个笑话吧"
    return rText
