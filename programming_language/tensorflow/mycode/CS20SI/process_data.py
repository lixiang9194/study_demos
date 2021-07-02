
#from __future__ import print_function
import os
import requests
from contextlib import closing
from utils import *
import zipfile
from collections import Counter
import random
import numpy as np

DOWNLOAD_URL = 'http://mattmahoney.net/dc/'
EXPECTED_BYTES = 31344016
DATA_FOLDER = 'data/'
FILE_NAME = 'text8.zip'
CHUNK_SIZE = 1024

VOCAB_SIZE = 50000
BATCH_SIZE = 500
SKIP_WINDOW = 1

 
# download the dataset text8
def download(file_name, expected_bytes):
    file_path = DATA_FOLDER + file_name
    if os.path.exists(file_path):
        print("Dataset ready")
        return file_path
    with closing(requests.get(DOWNLOAD_URL+file_name, stream=True)) as r:
        total_size = int(r.headers['content-length'])
        download_size = 0
        p = ProgressBar(total_size)
        with open(file_path,'wb') as f:
            for chunk in r.iter_content(CHUNK_SIZE):
                download_size += CHUNK_SIZE
                p.progress_show(download_size)
                f.write(chunk)
    file_stat = os.stat(file_path)
    if file_stat.st_size == expected_bytes:
        print('Successfully download the file', file_name)
    else:
        raise Exception('File '+ file_name +
            ' might be corrupted. You should try downloading manually.')
    return file_path


# get words list from the zipfile
def read_data(file_path):
    with zipfile.ZipFile(file_path) as f:
        data_str = f.read(f.namelist()[0])
        words = data_str.split()
        return words


#build vocabulary of vocab_size 
def build_vocab(words, vocab_size):
    word_dict = {}
    # for the word whoes frequency is too low ,
    # use UNK to replace it
    count = [('UNK',-1)]
    count.extend(Counter(words).most_common(vocab_size-1))
    index = 0
    if not os.path.exists(DATA_FOLDER+'processed'):
        os.mkdir(DATA_FOLDER+'processed')
    with open(DATA_FOLDER+'processed/vocab_1000.tsv','w') as f:
        for word, c in count:
            word_dict[word] = index
            if index < 1000:
                f.write(word + '\n')
            index += 1
    index_dict = dict(zip(word_dict.values(),word_dict.keys()))
    return word_dict, index_dict


# replace each word with its index
def convert_words_to_index(words, word_dict):
    return [word_dict[word] if word in word_dict else 0 for word in words]


# form training pairs according to the skip-gram model
def generat_sample(index_words, skip_window):
    for index, center in enumerate(index_words):
        # the context use a randint,i don't see why 
        contex = random.randint(1,skip_window)
        for target in index_words[max(0,index-contex): index]:
            yield center,target
        # for list index > length ,it just ignore 
        for target in index_words[index+1: index+contex+1]:
            yield center,target 


def get_batch(iterator, batch_size):
    while True:
        center_batch = np.zeros(batch_size, dtype=np.int32)
        target_batch = np.zeros([batch_size,1])
        for index in range(batch_size):
            center_batch[index], target_batch[index] = next(iterator)
        yield center_batch, target_batch

def process_data(vocab_size, batch_size, skip_window):
    file_name = download(FILE_NAME,EXPECTED_BYTES)
    words = read_data(file_name)
    print('--------build words complete')
    word_dict, _ = build_vocab(words,vocab_size)
    print('--------build vocab complete')
    index_words = convert_words_to_index(words, word_dict)
    del words
    single_gen = generat_sample(index_words, skip_window)
    return get_batch(single_gen, batch_size)


def main():
    batch_gen = process_data(VOCAB_SIZE, BATCH_SIZE, SKIP_WINDOW)
    batch = batch_gen.next()
    print(batch[0][0],batch[1][0])

# batch_gen = process_data(VOCAB_SIZE, BATCH_SIZE, SKIP_WINDOW)
if __name__ == '__main__':
    VGG_DOWNLOAD_LINK = 'http://www.vlfeat.org/matconvnet/models/imagenet-vgg-verydeep-19.mat'
    EXPECTED_BYTES = 534904783
    download()