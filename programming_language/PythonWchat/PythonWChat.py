#coding=utf8
import itchat
from itchat.content import *
from robots import tuLingRobot

@itchat.msg_register([TEXT, MAP, CARD, NOTE, SHARING,PICTURE, RECORDING, ATTACHMENT, VIDEO])
def text_reply(msg):
    if msg.type==TEXT:
        rMsg=tuLingRobot(msg.text)
    else:
        rMsg="你说的啥？我听不懂，也不想听"
    msg.user.send(rMsg)

@itchat.msg_register([TEXT, MAP, CARD, NOTE, SHARING,PICTURE, RECORDING, ATTACHMENT, VIDEO],isGroupChat=True)
def text_reply(msg):
    if msg.type==TEXT:
        rMsg=tuLingRobot(msg.text)
    else:
        rMsg="你说的啥？我听不懂，也不想听"
    msg.user.send(rMsg)

@itchat.msg_register([TEXT, MAP, CARD, NOTE, SHARING,PICTURE, RECORDING, ATTACHMENT, VIDEO],isMpChat=True)
def text_reply(msg):
    if msg.type==TEXT:
        rMsg=tuLingRobot(msg.text)
    else:
        rMsg="你说的啥？我听不懂，也不想听"
    msg.user.send(rMsg)

itchat.auto_login()
itchat.run()

