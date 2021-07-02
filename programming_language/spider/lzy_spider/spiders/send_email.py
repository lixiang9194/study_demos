# -*- coding: utf-8 -*-

from email import encoders
from email.header import Header
from email.mime.text import MIMEText
from email.utils import parseaddr, formataddr
import smtplib

def _format_addr(s):
    name, addr = parseaddr(s)
    return formataddr(( \
        Header(name, 'utf-8').encode(), \
        addr.encode('utf-8') if isinstance(addr, unicode) else addr))

from_addr = 'lzy2954@163.com'
password = '1994lzy'
to_addr = '1504559953@qq.com'
smtp_server = 'smtp.163.com'

def send_email(msgtext):
	msg = MIMEText('实验项目进度更新为 %s 人民币'%msgtext, 'plain', 'utf-8')
	msg['From'] = _format_addr(u'805实验室 <%s>' % from_addr)
	msg['To'] = _format_addr(u'805管理员 <%s>' % to_addr)
	msg['Subject'] = Header(u'实验进度', 'utf-8').encode()

	server = smtplib.SMTP(smtp_server, 25)
	server.set_debuglevel(1)
	server.login(from_addr, password)
	server.sendmail(from_addr, [to_addr], msg.as_string())
	server.quit()