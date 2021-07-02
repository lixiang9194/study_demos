# start website

pip install virtualenv && \

# if you run this in vagrant, add para --always-copy
virtualenv --no-site-packages env && \

#activate virtual python environment
source env/bin/active && \

#install necessary libs
pip install -r requirements.txt && \

#start server
nohup python manage.py runserver 0.0.0.0:8000
